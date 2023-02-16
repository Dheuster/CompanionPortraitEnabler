// System/C# Generic
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

// Unity Engine Specific
using UnityEngine;


// WOTR Specific
using Kingmaker;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Rest;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Modding;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.CharSelect;
using Kingmaker.UI.MVVM._VM.SaveLoad;
using Kingmaker.Utility;
using Kingmaker.EntitySystem;
using Kingmaker.Utility.UnitDescription;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects;

using Owlcat.Runtime.Core.Logging;

// Mod Specific
using OwlcatModification.Modifications.NPCCustomPortraitEnabler.Relay;
using OwlcatModification.Modifications.NPCCustomPortraitEnabler.Rules;
using OwlcatModification.Modifications.NPCCustomPortraitEnabler.Utility;

// 3rd Party
using HarmonyLib;

namespace OwlcatModification.Modifications.NPCCustomPortraitEnabler
{
	public static class NPCCustomPortraitEnablerMain
	{
		private const string DefaultSubDirectory = "πpcPortraits";
		private const string DefaultDocumentation = "Subdir property should not have slashes!";
		private static string DefaultPortraitsFolder = "Portraits";


		public static Kingmaker.Modding.OwlcatModification Modification { get; private set; }

		public static bool IsEnabled { get; private set; } = true;

		public static LogChannel Logger => Modification.Logger;

		private static ConfigData Config = new ConfigData();

		// private static Dictionary<string, BlueprintPortrait> CacheLookup = new Dictionary<string, BlueprintPortrait>();
		private static Dictionary<string, PortraitData> CacheLookup = new Dictionary<string, PortraitData>();

		private static Dictionary<string, NPCMonitor> MonitoredNPCs = new Dictionary<string, NPCMonitor>();

		// Most non-random-gen npc portraits end with "Portrait". The exceptions:
		private static HashSet<string> NameFilter = new HashSet<string>() {
			"AasimarMaleBloodrager",
			"DhampirFemaleBard",
			"DwarfMaleRogue",
			"DwarfMaleRogueUndead",
			"ElfMaleNoble",
			"GnomeMaleNoble",
			"HalfElfMaleMage",
			"HalfOrcFemaleTank",
			"HalfOrcFemaleTankScars",
			"HalflingFemaleRogue",
			"HalflingMaleTank",
			"HilorHumanMale",
			"HumanTianXiaFemaleNoble",
			"MythicAzataFemale",
			"OdanHalfelfMale",
			"PetTriceratops",
			"PetVelociraptor",
			"PharasmaGoddessFemale",
			"TieflingFemalePriest",
			"TreverHumanMwangiMaleWarrior",
			"TricksterMythicMale",
			"HumanMaleTank",
			"TieflingMaleArcher",
			"MythicDevilMale"
		};

		// ReSharper disable once UnusedMember.Global
		[OwlcatModificationEnterPoint]
		public static void Initialize(Kingmaker.Modding.OwlcatModification modification)
		{

//			Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(LogSinkFactory.CreateFull(PersistentDataPath, "NPCCustomPortraits.txt", false), false);
//			Owlcat.Runtime.Core.Logging.Logger.Instance  =
//			Logger log = Owlcat.Runtime.Core.Logging.Logger.Instance.GetLogger<NPCCustomPortraitEnablerMain>();
			
			Modification = modification;

			// Use Harmony to auto-patch our assembly into runtime instance of the
			// main assembly. This makes uninstall easier.

			var harmony = new Harmony(modification.Manifest.UniqueName);
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			InitConfig();
			if (IsEnabled)
			{
				if (!Config.Disabled)
				{
					logDebug("NPCCustomPortraitEnabler Registering for Events.");
					AddLoadResourceCallback();
					modification.OnDrawGUI += OnGUI;
					modification.IsEnabled += () => IsEnabled;
					modification.OnSetEnabled += enabled => IsEnabled = enabled;
					modification.OnShowGUI += () => Logger.Log("OnShowGUI");
					modification.OnHideGUI += () => Logger.Log("OnHideGUI");
					EventBus.Subscribe(new OnAreaLoadRelayHandler());
					EventBus.Subscribe(new OnDialogFinishRelayHandler());
					EventBus.Subscribe(new OnDialogStartRelayHandler());
					EventBus.Subscribe(new OnPartyGainXPRelayHandler());     // Twice?
					EventBus.Subscribe(new OnWeatherChangedRelayHandler());
					EventBus.Subscribe(new OnSaveLoadRelayHandler());
					// EventBus.Subscribe(new OnPartyRelayHandler()); // Needed to detect companion add/removal?

/*
					EventBus.Subscribe(new OnAlignmentChangeRelayHandler()); // x
					EventBus.Subscribe(new OnAttributeDamagedRelayHandler()); //x
					EventBus.Subscribe(new OnCharacterSelectRelayHandler());
					EventBus.Subscribe(new OnCompanionChangeRelayHandler()); //x
					EventBus.Subscribe(new OnCorruptionLevelRelayHandler()); //x
					EventBus.Subscribe(new OnDamageRelayHandler());
					EventBus.Subscribe(new OnHealingRelayHandler());
					EventBus.Subscribe(new OnIBookEventUIRelayHandler());
					EventBus.Subscribe(new OnIBookPageRelayHandler());
					EventBus.Subscribe(new OnMythicSelectionRelayHandler());
					EventBus.Subscribe(new OnPartyLeaveAreaRelayHandler());
					EventBus.Subscribe(new OnPortraitHoverUIRelayHandler());
					EventBus.Subscribe(new OnQuestRelayHandler());
					EventBus.Subscribe(new OnRestCampRelayHandler()); // ? (RestCampUIHandler...)
					EventBus.Subscribe(new OnRestFinishedRelayHandler());
					EventBus.Subscribe(new OnSafeZoneRelayHandler());
					EventBus.Subscribe(new OnUnitPortraitChangedRelayHandler());
*/
				}
				else
				{
					logAlways("NPCCustomPortraitEnabler Disabled from json config.");
					IsEnabled = false;
				}
			}
			else
			{
				logAlways("NPCCustomPortraitEnabler Disabled from Mod Manager");
			}
		}

		private static void InitConfig()
		{

			ConfigData loadedData = Modification.LoadData<ConfigData>();
			if (loadedData == null)
			{
				loadedData = new ConfigData();
			}
			if (String.IsNullOrEmpty(loadedData.LastLoadTime))
			{
				logAlways("NPCCustomPortraitsEnabler - Initializing : Using first run defaults");
				Config.Disabled = false;
				Config.LogDebug = true;
				Config.CreateIfMissing = true;
				Config.SubDirectory = DefaultSubDirectory;
				Config.PortraitsFolder = DefaultPortraitsFolder;
				Config.Documentation = DefaultDocumentation;
			}
			else
			{
				Config.Disabled = loadedData.Disabled;
				Config.LogDebug = loadedData.LogDebug;
				Config.CreateIfMissing = loadedData.CreateIfMissing;
				Config.SubDirectory = loadedData.SubDirectory;
				Config.PortraitsFolder = loadedData.PortraitsFolder;
				Config.Documentation = loadedData.Documentation;
			}
			// Validate
			if (String.IsNullOrEmpty(Config.Documentation))
			{
				logAlways("Documentation is null or empty. Resetting Value to Default.");
				Config.Documentation = DefaultDocumentation;
			}
			if (String.IsNullOrWhiteSpace(Config.SubDirectory))
			{
				logAlways("SubDirectory is null, empty or consists of all spaces. Resetting Value to Default.");
				Config.SubDirectory = DefaultSubDirectory;
			}
			if (String.IsNullOrWhiteSpace(Config.PortraitsFolder))
			{
				logAlways("PortraitsFolder is null, empty or consists of all spaces. Resetting Value to Default.");
				Config.SubDirectory = DefaultPortraitsFolder;
			}
			// Convert any invalid characters to underscores:
			Config.SubDirectory = string.Join("_", Config.SubDirectory.Split(Path.GetInvalidFileNameChars()));
			if (Config.SubDirectory.Contains(".."))
			{
				logAlways("SubDirectory contains illegal path [..]: Resetting Value to Default.");
				Config.SubDirectory = DefaultSubDirectory;
			}
			Config.LastLoadTime = DateTime.Now.ToString();

			// Show values if debug logging is enabled
			if (Config.LogDebug)
			{
				StringWriter sw = new StringWriter();
				sw.WriteLine("");
				sw.WriteLine("-------------------------------------------------------");
				sw.WriteLine("Config");
				sw.WriteLine("-------------------------------------------------------");
				sw.WriteLine($"Disabled: [{Config.Disabled}]");
				sw.WriteLine($"LogDebug: [{Config.LogDebug}]");
				sw.WriteLine($"CreateIfMissing: [{Config.CreateIfMissing}]");
				sw.WriteLine($"SubDirectory: [{Config.SubDirectory}]");
				sw.WriteLine($"LastLoadTime: [{Config.LastLoadTime}]");
				sw.WriteLine($"PortraitsFolder: [{Config.PortraitsFolder}]");
				sw.WriteLine($"Documentation: [{Config.Documentation}]");
				sw.WriteLine("-------------------------------------------------------");
				sw.WriteLine("Environment Paths");
				sw.WriteLine("-------------------------------------------------------");
				sw.WriteLine($"Environment.Desktop: [{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}]");
				sw.WriteLine($"Environment.DesktopDirectory: [{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}]");
				sw.WriteLine($"Environment.Personal: [{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}]");
				sw.WriteLine($"Environment.MyDocuments: [{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}]");
				sw.WriteLine($"Environment.UserProfile: [{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}]");
				sw.WriteLine("-------------------------------------------------------");
				sw.WriteLine("Application Paths");
				sw.WriteLine("-------------------------------------------------------");
				sw.WriteLine($"ApplicationPaths.dataPath: [{ApplicationPaths.dataPath}]");
				sw.WriteLine($"ApplicationPaths.streamingAssetsPath: [{ApplicationPaths.streamingAssetsPath}]");
				sw.WriteLine($"ApplicationPaths.persistentDataPath: [{ApplicationPaths.persistentDataPath}]");
				sw.WriteLine($"ApplicationPaths.temporaryCachePath: [{ApplicationPaths.temporaryCachePath}]");
				sw.WriteLine($"Application.consoleLogPath: [{Application.consoleLogPath}]");
				sw.WriteLine($"Application.version: [{Application.version}]");
				sw.WriteLine("-------------------------------------------------------");
				sw.WriteLine("Command Line Arguments");
				sw.WriteLine("-------------------------------------------------------");
			
				System.String[] args = Environment.GetCommandLineArgs();
				if (args.Length > 0)
				{
					for (int i = 0; i < args.Length; i++)
					{
						sw.WriteLine($"arg [{i}] = [{args[i]}]");
					}
				}
				else
				{
					sw.WriteLine("No Args...");
				}
				logAlways(sw.ToString());
			}

			string PersistentDataPath = Path.GetFullPath(Path.Combine(ApplicationPaths.persistentDataPath, "."));
			if (!Directory.Exists(PersistentDataPath))
			{
				logDebug($"Path [{PersistentDataPath}] not found. Aborting Initialization");
				Config.Disabled = true;
				IsEnabled = false;
				return;
			}
			else
			{
				logDebug($"Confirmed Persistent Data Path [{PersistentDataPath}]");
			}

			// We call GetFullPath because the default value uses the wrong directory seperator which 
			// will cause the call to "create shortcut" to fail...
			string WrathDataPath = Path.GetFullPath(Path.Combine(ApplicationPaths.dataPath, "."));
			if (!Directory.Exists(WrathDataPath))
            {
				logDebug($"Path [{WrathDataPath}] not found. Aborting Initialization");
				Config.Disabled = true;
				IsEnabled = false;
				return;
			} 
			else
            {
				logDebug($"Confirmed Wrath Data Path [{WrathDataPath}]");
			}


			// Timing : We need access to the Kingmaker.Blueprints.Root.CharGenRoot.PortraitFolderName property.
			// Issue is that CharGenRoot is not static. It is an instantiated class spawnedby BlueprintRoot.Instance
			// However, BlueprintRoot.Instance only guarantees an instance pointer to the BlueprintRoot. That classes
			// child classes that need instantiation (like BlueprintRoot.Instance.CharGen) may still be initializing
			// at this point. So tryingto accees BlueprintRoot.Instance.CharGen.PortraitFolderName in this init 
			// context typically throws an undefined (null pointer) exception. 
			//
			// Our solution is to default to the normal value "Portraits", but then revisit when
            // the Area OnLoad fires and if the value is wrong, correct. If you assume this is
            // our second execution, then the Config value would have the cached/correct value
            // from the previous run.  
			string PortraitsRoot = Path.GetFullPath(Path.Combine(PersistentDataPath, Config.PortraitsFolder));
			if (!Directory.Exists(PortraitsRoot))
			{
				logDebug($"Path [{PortraitsRoot}] not found. Attempting Creation");
				try
				{
					Directory.CreateDirectory(PortraitsRoot);
					if (!Directory.Exists(PortraitsRoot))
					{
						logDebug($"Creation attempt failed. Aborting initialization.");
						Config.Disabled = true;
						IsEnabled = false;
						return;
					}
					else
					{
						logDebug($"Path [{PortraitsRoot}] created");
					}
				}
				catch (Exception objException)
				{
					// Log the exception
					logDebug("CreateDirectory failed: " + objException.Message);
				}
			}
			else
			{
				logDebug($"Confirmed Portraits Path [{PortraitsRoot}]");
			}

			string NPCCustomPortraitsRoot = Path.GetFullPath(Path.Combine(PortraitsRoot, Config.SubDirectory));
			if (!Directory.Exists(NPCCustomPortraitsRoot))
            {
				logDebug($"Path [{NPCCustomPortraitsRoot}] not found. Attempting Creation");
				try
				{
					Directory.CreateDirectory(NPCCustomPortraitsRoot);
					if (!Directory.Exists(NPCCustomPortraitsRoot))
					{
						logDebug($"Creation attempt failed. Aborting initialization.");
						Config.Disabled = true;
						return;
					}
					else
					{
						logDebug($"Path [{NPCCustomPortraitsRoot}] created");
					}
				}
				catch (Exception objException)
				{
					// Log the exception
					logDebug("CreateDirectory failed: " + objException.Message);
				}
			}
			else
            {
				logDebug($"Confirmed NPCPortaits Path [{NPCCustomPortraitsRoot}]");
			}

			//--------------------------------------------------------------------------
			// Under OSX, WOTR creates nice and easy to access folders under Documents
			// but everywhere else, it uses the hidden persistent store provided by 
			// the OS. IE: /USERS/$USER/APPDATA/LOCALLOW/.....  This is a pain to 
			// access as it is hidden (requires users who know how to unhide 
			// directories). So we improve things by creating some shortcuts:
			//--------------------------------------------------------------------------
			// - Under the games install directory, we create a shortcut called
			//   "Config" that links to %USER_PROFILE%/AppData/LocalLow/Owlcat Games/Pathfinder Wrath of the Righteous"
			// - Under Documents, we create the shortcut "Owlcat Games" that links to
			//   %USER_PROFILE%/AppData/LocalLow/Owlcat Games
			// - We add the shortcut "NPCPortraits" to the Config directory that links
			//   to Config/Portraits/πpcPortraits. Our default directory starts with
			//   the letter "π" to avoid conflicting with other portraits and to ensure
			//   it is listed last alphanumerically. 
			//--------------------------------------------------------------------------


			// string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Owlcat Games");
			// if (!Directory.Exists(path))
			// {
			// Directory.CreateDirectory(path);
			// }

			string configLinkPath = Path.GetFullPath(Path.Combine(WrathDataPath, "..", "Config.lnk"));
			if (!File.Exists(configLinkPath))
			{
				logDebug($"Attempting to creating ease of acccess shortcut: [{configLinkPath}]");

				try
				{
					using (ShellLink shortcut = new ShellLink())
					{
						shortcut.Target = PersistentDataPath;
						// shortcut.WorkingDirectory = ApplicationPaths.persistentDataPath;
						shortcut.Description = "Config";
						shortcut.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
						shortcut.Save(configLinkPath);
					}
				}
				catch (Exception objException)
				{
					// Log the exception
					logDebug("Shortcut Creation Failed: " + objException.Message);
				}

				if (!File.Exists(configLinkPath))
				{
					logDebug("Shortcut Creation Failed");
				}
				else
                {
					logDebug($"Shortcut [{configLinkPath}] created");
				}
			}
			else 
			{
				logDebug($"Confirmed Ease of Access Shortcut [{configLinkPath}]");
			}

			string owlcatGamesLinkPath = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Owlcat Games.lnk"));
			if (!File.Exists(owlcatGamesLinkPath))
			{
				logDebug($"Attempting to creating ease of acccess shortcut: [{owlcatGamesLinkPath}]");

				try
				{
					using (ShellLink shortcut = new ShellLink())
					{
						shortcut.Target = Path.GetFullPath(Path.Combine(PersistentDataPath,".."));
						// shortcut.WorkingDirectory = ApplicationPaths.persistentDataPath;
						shortcut.Description = "Owlcat Games";
						shortcut.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
						shortcut.Save(owlcatGamesLinkPath);
					}
				}
				catch (Exception objException)
				{
					// Log the exception
					logDebug("Shortcut Creation Failed: " + objException.Message);
				}

				if (!File.Exists(owlcatGamesLinkPath))
				{
					logDebug("Shortcut Creation Failed");
				}
				else
				{
					logDebug($"Shortcut [{owlcatGamesLinkPath}] created");
				}
			}
			else
			{
				logDebug($"Confirmed Ease of Access Shortcut [{owlcatGamesLinkPath}]");
			}

			string npcPortraitsLinkPath = Path.GetFullPath(Path.Combine(PersistentDataPath, "NPCPortraits.lnk"));
			if (!File.Exists(npcPortraitsLinkPath))
			{
				logDebug($"Attempting to creating ease of acccess shortcut: [{npcPortraitsLinkPath}]");

				try
				{
					using (ShellLink shortcut = new ShellLink())
					{
						shortcut.Target = NPCCustomPortraitsRoot;
						// shortcut.WorkingDirectory = ApplicationPaths.persistentDataPath;
						shortcut.Description = "NPCPortraits";
						shortcut.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
						shortcut.Save(npcPortraitsLinkPath);
					}
				}
				catch (Exception objException)
				{
					// Log the exception
					logDebug("Shortcut Creation Failed: " + objException.Message);
				}

				if (!File.Exists(npcPortraitsLinkPath))
				{
					logDebug("Shortcut Creation Failed");
				}
				else
				{
					logDebug($"Shortcut [{npcPortraitsLinkPath}] created");
				}
			}
			else
			{
				logDebug($"Confirmed Ease of Access Shortcut [{npcPortraitsLinkPath}]");
			}


			// -----------------------------------------------------------
			// Pre-Scan and Load NPC Portrait Rules (TODO)
			// -----------------------------------------------------------
			/*
			if (Directory.Exists(NPCCustomPortraitsRoot))
			{
				logDebug($"Path [{Application.persistentDataPath}/portraits/{Config.SubDirectory}] not found. Skippnig Rule Scan.");
				return;
			}
			using (CodeTimer.New("LoadAllJson"))
			{
				foreach (string text in Directory.EnumerateFiles(path, "*.jbp", SearchOption.AllDirectories))
				{
					try
					{
						BlueprintJsonWrapper blueprintJsonWrapper = BlueprintJsonWrapper.Load(text);
						blueprintJsonWrapper.Data.OnEnable();
						ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(BlueprintGuid.Parse(blueprintJsonWrapper.AssetId), blueprintJsonWrapper.Data);
					}
					catch (Exception ex)
					{
						PFLog.Default.Error("Failed loading blueprint: " + text, Array.Empty<object>());
						PFLog.Default.Exception(ex, null, Array.Empty<object>());
					}
				}
			}
			*/

			logDebug($"Saving Config Model");

			// Modification.SaveData is stored in the Modifications directory as a json file. These are global 
			// settings that apply to all games/saves:
			Modification.SaveData(Config);

			Config.SubDirectory += Path.DirectorySeparatorChar;

			// There is also a way to save data as part of the games save file, which is then specific to that 
			// save. (Makes more sense for content updates like new quests, etc..). And if you do both, you can 
			// detect when the global settings have changed if you have any cached/update type work that needs
			// to happen...

			// NOTE: Using global permanent storage does not create dependencies on the mod and the 
			// users save games. If you save data within the save games themselves and the user uninstalls
			// your mod, it could leave any saves made with your mod corrupt.

			// var data = Game.Instance.Player.Ensure<EntityPartKeyValueStorage>().GetStorage(ModificationRoot.Modification.Manifest.UniqueName);
			// if (data.Get(propertyName) == null)
			// {
			//	  data[propertyName] = SOMEDEFAULTSTRINGVALUE;
			// }
			// else
			// {
			//    data[propertyName] = (int.Parse(data[propertyName]) + 1).ToString();
			// }

			logDebug("Init Complete");
		}

		private static void AddLoadResourceCallback()
		{
			Modification.OnLoadResource += (resource, guid) =>
			{
				string name = (resource as UnityEngine.Object)?.name ?? resource.ToString();
				BlueprintPortrait blueprintPortrait = (resource as Kingmaker.Blueprints.BlueprintPortrait);
				if (null != blueprintPortrait)
				{
					if (null != name)
					{
						if (!CacheLookup.ContainsKey(name))
						{
							// Prevent future inquires whether we succeed or fail...
							CacheLookup[name] = null;
							logDebug($"BlueprintPortrait resource requested for [{name}] [{guid}]");
							if (name.ToLower().EndsWith("_portrait") || NameFilter.Contains(name))
							{
								try
								{
									string portraitId = Config.SubDirectory + name;
									if (CustomPortraitsManager.Instance.EnsureDirectory(portraitId, false))
									{
										logDebug($"Portrait folder found for [{portraitId}]. Confirming files...");
										string small = CustomPortraitsManager.Instance.GetSmallPortraitPath(portraitId);
										string medium = CustomPortraitsManager.Instance.GetMediumPortraitPath(portraitId);
										string large = CustomPortraitsManager.Instance.GetBigPortraitPath(portraitId);
										if (File.Exists(small) && File.Exists(medium) && File.Exists(large))
										{
											PortraitData injectedData = new PortraitData(portraitId);
											if (null != injectedData.m_PortraitImage)
											{
												// Set AssetId to empty string so that PortraitData.m_portraitImage.Exists() fails in PreLoad() method
												// If any code assumes companion portraits are not custom portraits.
												injectedData.m_PortraitImage.AssetId = "";
											}

											// Don't call the load methods here or it will crash the game!

											// injectedData.SmallPortraitHandle.Load();
											// injectedData.HalfPortraitHandle.Load();
											// injectedData.FullPortraitHandle.Load();
											// injectedData.CheckIfDefaultPortraitData();

											logAlways($"Intercepted Request for [{name}]: Returning CustomPortrait [{portraitId}]");
											BlueprintPortrait altered = (resource as Kingmaker.Blueprints.BlueprintPortrait);
											altered.Data = injectedData;
											(resource as Kingmaker.Blueprints.BlueprintPortrait).Data = injectedData;
											CacheLookup[name] = injectedData;

										}
										else
										{
											string missing = "";
											if (!File.Exists(small))
											{
												missing += "Small.png ";
											}
											if (!File.Exists(medium))
											{
												missing += "Medium.png ";
											}
											if (!File.Exists(large))
											{
												missing += "Fulllength.png";
											}
											logDebug($"[{portraitId}] - Files [{missing}] not found in [{CustomPortraitsManager.Instance.GetPortraitFolderPath(portraitId)}]");
										}
									}
									else
									{
										logDebug($"[{portraitId}] - Not found in Portraits folder");
										if (Config.CreateIfMissing)
										{
											// Only do this on the first cach miss....
											if (CustomPortraitsManager.Instance.EnsureDirectory(portraitId, true))
											{
												logDebug($"Created portraits directory for [{name}]:");
												// Unity makes it easy to import data into its engine, but not so easy to 
												// get it out. Unless the Bundled Texture is earmarked read/write (a non-
												// default value), any attempt to extract it or encode it will throw an
												// exception.  Oh well... It would be nice if we could save off the 
												// originals as we made these directories...

												/*
												try
												{
													File.WriteAllBytes(small, thePortrait.SmallPortrait.texture.EncodeToPNG());
												}
												catch (Exception ex)
												{
													logAlways($"Exception creating small portrait [{small}]" + ex.ToString(), Array.Empty<object>());
												}
												try
												{
													File.WriteAllBytes(medium, thePortrait.HalfLengthPortrait.texture.EncodeToPNG());
												}
												catch (Exception ex)
												{
													logAlways($"Exception creating portrait [{medium}]" + ex.ToString(), Array.Empty<object>());
												}
												try
												{
													File.WriteAllBytes(large, thePortrait.FullLengthPortrait.texture.EncodeToPNG());
												}
												catch (Exception ex)
												{
													logAlways($"Exception creating portrait [{large}]" + ex.ToString(), Array.Empty<object>());
												}
												*/
											}
											else
											{
												logAlways($"Failed to create portraits directory for [{name}]");
											}
										}
									}
								}
								catch (Exception ex)
								{
									logAlways($"Exception processing portrait request for [{name}] [{guid}]:", ex.ToString());
								}
							}
							else
							{
								logDebug($"Skipping [{name}] : Does not end with [_Portrait] and is not a known special case");
							}
						}
						else
						{
							if (null != CacheLookup[name])
							{
								logAlways($"Intercepted Request for [{name}]: Returning Cached CustomPortrait [{Config.SubDirectory + name}]");
								PortraitData injectedData = CacheLookup[name];
								BlueprintPortrait altered = (resource as Kingmaker.Blueprints.BlueprintPortrait);
								altered.Data = injectedData;
								(resource as Kingmaker.Blueprints.BlueprintPortrait).Data = injectedData;


							}
						}
					}
					else
					{
						logAlways($"BlueprintPortrait [{guid}] has no name!");
					}
				}
			};
		}

		//--------------------------------------------------------------------
		// Events : UNIT/NPC SPECIFIC
		// -------------------------------------------------------------------
		// Relayed from registered event handers in the Relay sub-dir
		//
		// Notes: This is far from all the events the game supports, but these
		// are the ones that I felt could create the need for a new portrait.

		// These events include handles to UnitEntityData (IE: NPCs)
		public static void OnAlignmentChanged(UnitEntityData unit, Alignment newAlignment, Alignment prevAlignment)
		{
			logDebug($"OnAlignmentChanged");
			if (unit != null) {
				logDebug($"unit.Descriptor.CharacterName [{unit.Descriptor.CharacterName}]");
			}
			logDebug($"newAlignment [{newAlignment}]");
			logDebug($"prevAlignment [{prevAlignment}]");

			// unit.Commands.IsRunning()
			// unit.Commands.HasAiCommand()
			// unit.Commands.Run(UnitCommand)
			// See Kingmaker.AreaLogic.Cutscenes.Commands for some
			//
			// Some Commands:
			//   Kingmaker.UnitLogic.Commands.UnitInteractWithObject
			//   Kingmaker.UnitLogic.Commands.UnitInteractWithUnit
			//   Kingmaker.UnitLogic.Base.UnitCommand.StartAnimation()
			//
			//
			// Setting a portrait on an event seems rather strait forward:
			//
			// BlueprintPortrait updatedPortrait = BlueprintRoot.Instance.CharGen.CustomPortrait
			// updatedPortrait.Data = new PortraitData(portraitId); // Use cached value...
			// unit.UISettings.SetPortrait(blueprintPortrait)
			//
			// We might also be able to change the Data part in place and then call 
			// SetPortrait on its current value to cause a refresh/reload

			//
			// UnitEntityView unitEntityView = unit.View.Or(null);
			// UnitAnimationManager unitAnimationManager = (unitEntityView != null) ? unitEntityView.AnimationManager : null;
			// if (!(unitAnimationManager == null))
			// {
			// }
			//
			// UnitDescriptor unit.Descriptor
			//
			// unit.Descriptor.
			// <UnitPartDollData>.SetDefailt(DollData data)
			//
			// UnitBody unit.Body <- Access to equipped items (item slots)
			// 
			// PortraitData unit.Portrait <- Shortcut to unit.UISettings.Portrait
			//
			// BlueprintUnit unit.Blueprint <- Shortcut to unit.Descriptor.Blueprint
			//
			// string unit.CharacterName <- Shortcut to this.Descriptor.CharacterName
			//
			// UnitEntityView unit.View <- Number of item centric commands for using things.
			//
			// ItemsCollection unit.Inventory <- Shortcut to unit.Descriptor.Inventory
			//
			// UnitUISettinsg unit.UISettings <- shortcut to unit.Descrptor.UISettings
			//
			// UnitEntityData Master <- Returns null unless Unit is a pet.
			// 
			// One thought is that we could look for and load .jbp files located 
			// in the portrait directory and apply them along with the portrait
			//
			// Most UnitEntities have an OriginalBlueprint property on the UnitDescrptor. We could
			// Restore to that... however then we lose any additional changes to state. 
			//
			// dealing with combinations:
			//
			//
			// 	RACES: Human, Elf, Gnome, Halfling, Dwarf, HalfElf, HalfOrc, Goblin, Spriggan,
			// 	Zombie, Skeleton, Aasimar, Tiefling, Catfolk, Dhampir, Mongrelman, SuccubusIncubus,
			// 	Oread, Ghoul, NotDetermined, Kitsune, Cambion, Drow
			//
			// Suppose we have 5 distinct factors:
			//
			// _npc/Camellia/Race
			// _npc/Camellia/Alignment
			// _npc/Camellia/Morality
			// _npc/Camellia/Mental
			// _npc/Camellia/Health
			//
			// Suppose we have these values:
			//
			// _npc/Camellia/Race/Human
			// _npc/Camellia/Race/Dragon
			// _npc/Camellia/Alignment/Lawful
			// _npc/Camellia/Alignment/Neutral
			// _npc/Camellia/Alignment/Chaotic
			// _npc/Camellia/Morality/Good
			// _npc/Camellia/Morality/Neutral
			// _npc/Camellia/Morality/Evil
			// _npc/Camellia/Mental/WellRested
			// _npc/Camellia/Mental/Tired
			// _npc/Camellia/Mental/Exhausted
			// _npc/Camellia/Health/75_to_100
			// _npc/Camellia/Health/50_to_75
			// _npc/Camellia/Health/25_to_50
			// _npc/Camellia/Health/1_to_25
			//
			// We want to allow users to define combinations without having to repeat the picture over and over again. Instead 
			// of having lots of directories, it makes more sense to have a picture once and have metadata tags attached to it.
			// 
			// Camellia/Pic1_Fullsize.png
			// Camellia/Pic1_Medium.png
			// Camellia/Pic1_Small.png
			// Camellia/Pic2_FullSize.png
			// Camellia/Pic2_Medium.png
			// Camellia/Pic2_Small.png
			// Camellia/Pic3_Fullsize.png
			// Camellia/Pic3_Medium.png
			// Camellia/Pic3_Small.png
			// Camellia/Pic4_Large.png
			// Camellia/Pic4Medium.png
			// Camellia/Pic4Small.png
			//
			// Rules: First Condition to eval to true determines portrait. 
			//        Files are processed alphanumerically. So Rule_01.json will trump Rule_02.json. This avoids loading
			//        all the files all the time.
			//
			// Rule Example:
			//
			// Camellia_model/Rule_01.json:
			// {
			//    "FullSize" : "Pic1_Fulllength.png",
			//    "Medium"   : "Pic1_Medium.png",
			//    "Small"    : "Pic1_Small.png",
			//    "condition" : {
			//       // Format is similar to MONGO query syntax:
			//       $or : [
			//           {"health" : { $in : ["75_to_100","50_to_75","25_to_50","1_to_25"] } },
			//           {"mental_state" : { $in : ["well_rested","tired","exhausted"] } },
			//           {"morality" : { $in : ["well_rested","tired","exhausted"] }
			//       ]
			// }
			//
			// Anchor:
			//
			//   A chain is basically an inner query. The keyword is used where a value would normally go
            //   and represents an inner query that feels the values of an outer query. For example, NPCs
			//   can have multiple classes and have a differnt LEVEL in each class.
			//
			//    "condition" : {
			//       // Format is similar to MONGO query syntax:
			//       $and : [
			//           {"
			//       ]
			//
			//   criteria up to the achorthe result set is remembered and futher criteria only acts on the entries
			//   
			//
			//
			// If no rules are found, then any file containing: <common_prefix>fulllength.png", "<common_prefix>medium.png" and "<common_prefix>small.png"
			// is used 100% of the time. If more than one is found, the first alphanumeric processed with all three file types is used. Note that we intercept requests for Portraits. When companions portraits change in game, often times the 
			// COMPANION RECORD updates to request a different portrait.
			//
			// At runtime, the winning portrait is copied off to temporary cache and renamed to Fulllength.png, Medium.png and Small.png respectively. 
			//
			// condition format is based on mongo's query format. Mongo uses $eq, $ne, $gt, $lt, $gte, $lte, $in and $nin
			// for value comparisions and then it uses $and and $or for grouping comparions together. 
			//
			// Finally it allows you to skip the operators if you are using the most common case, which is testing for
			// equality of several single values at once (all must be true). So the following:
			//
			//    { $and : [ field1: {$eq : "value1"}, field2 : {$eq : "value2"}] }
			//
			// can be written more simply as:
			//
			//    { field1:"value",field2:"value" }
			//
			// When a picture is selected for display, if a .patch file exists that is named after the pictures basename it is also loaded and applied.
			// So "Pic1_Fulllength.png" would get associated with "Pic1_.patch. Patches can allow you to do things like add buffs, change race, gender,
			// stats, equipment, etc...  I don't support full blueprints files (.jbp) because full blueprints are clobber-only and will overwrite
			// changes made by other mods or even the game itself. 
		}		

		//--------------------------------------------------------------------
		// Events : General
		//--------------------------------------------------------------------
		// These events are general and do not apply to anyone specifically or
		// they apply to the Player

		// Area finished loading and now can be safely accessed (If applying
		// visual changes for example). 
		public static void OnAreaActivated()
		{
			if (Config.LogDebug)
			{
				logAlways("",
					"-------------------------------------------------------",
					"Area Loaded",
					"-------------------------------------------------------",
					$"Difficulty: [{Kingmaker.Settings.SettingsRoot.Difficulty.GameDifficulty.GetValue()}]",
					$"Player Name: [{Kingmaker.Game.Instance.Player.MainCharacter.Value.CharacterName}]",
					"-------------------------------------------------------");
			}

			// Create/Populate Party Monitoring Instances if they don't already exist:
			foreach (UnitEntityData unitEntityData in Game.Instance.Player.AllCharacters)
			{
				BlueprintUnit blueprintUnit = unitEntityData.Blueprint;
				UnitPartCompanion unitPartCompanion = unitEntityData.Get<UnitPartCompanion>();
				if (null != unitPartCompanion && !unitEntityData.IsMainCharacter)
				{
					string name = blueprintUnit.CharacterName ?? "";
					if (MonitoredNPCs.ContainsKey(name))
					{
						MonitoredNPCs[name].OnAreaActivated();
					}
					else
					{
						MonitoredNPCs.Add(name, new NPCMonitor(unitEntityData, NPCCustomPortraitEnablerMain.Modification, NPCCustomPortraitEnablerMain.Config.LogDebug));
					}
				}
			}
			if (Config.PortraitsFolder != BlueprintRoot.Instance.CharGen.PortraitFolderName)
			{
				logDebug($"Updating Portraits Folder from [{Config.PortraitsFolder}] to [{BlueprintRoot.Instance.CharGen.PortraitFolderName}]");
				Config.PortraitsFolder = BlueprintRoot.Instance.CharGen.PortraitFolderName;
				Modification.SaveData(Config);
			}

		}

		public static void OnBookPageShow(string title, string text)
        {
			logDebug("OnBookPageShow()");
			logDebug($"title: [{title}] text: [{text}]");
		}
		public static void OnCapitalModeChanged()
		{
			logDebug("OnCapitalModeChanged()");
		}

		// Good time to kick off side quests or romance conversations that we don't want interrupted.
		public static void OnEnterSafeZone()
		{
			logDebug("OnEnterSafeZone()");
		}
		public static void OnPartyGainExperience(int gained)
		{
			logDebug("OnPartyGainExperience()");
			logDebug($"gained: [{gained}]");
		}
		public static void OnPartyLeaveArea(BlueprintArea currentArea, BlueprintAreaEnterPoint targetArea, AreaTransitionPart areaTransition)
		{
			logDebug("OnPartyLeaveArea()");
			if (currentArea != null)
			{
				logDebug($"currentArea: [{currentArea}]");
			}
			else
            {
				logDebug("currentArea: [NULL]");
			}
			if (targetArea != null)
			{
				logDebug($"targetArea: [{targetArea}]");
			}
			else
			{
				logDebug("targetArea: [NULL]");
			}
			if (areaTransition != null)
			{
				logDebug($"areaTransition: [{areaTransition}]");
			}
			else
			{
				logDebug("areaTransition: [NULL]");
			}
		}
		public static void OnQuestCompleted(Quest objective)
		{
			logDebug("OnQuestCompleted()");
			if (objective == null) { return; };
			logDebug($"objective: [{objective}]");
		}
		public static void OnQuestFailed(Quest objective)
		{
			logDebug("OnQuestFailed()");
			if (objective == null) { return; };
			logDebug($"objective: [{objective}]");
		}
		public static void OnQuestStarted(Quest quest)
		{
			logDebug("OnQuestStarted()");
			if (quest == null) { return; };
			logDebug($"objective: [{quest}]");
		}
		public static void OnRestCloseCamp()
		{
			logDebug("OnRestCloseCamp()");
		}
		public static void OnRestOpenCamp()
		{
			logDebug("OnRestOpenCamp()");
		}
		public static void OnRestShowResults()
		{
			logDebug("OnRestShowResults()");
		}
		public static void OnRestSkipPhase()
		{
			logDebug("OnRestSkipPhase()");
		}
		public static void OnRestVisualCampPhaseFinished()
		{
			logDebug("OnRestVisualCampPhaseFinished()");
		}
		public static void OnRestFinished(RestStatus status)
		{
			logDebug("OnRestFinished()");
			if (status != null)
            {
				logDebug($"RestStatus [{status}]");
			}
		}
		public static void OnSaveLoad(SaveLoadMode mode, bool singleMode)
		{
			logDebug("OnSaveLoad()");
			logDebug($"mode [{mode}]");
			logDebug($"singleMode [{singleMode}]");
		}
		public static void OnWeatherChanged()
		{
			logDebug("OnWeatherChanged()");
		}

		public static void logDebug(string value)
		{
			if (Config.LogDebug)
			{
				logAlways(value);
			}
		}

		public static void logAlways(params string[] list)
		{
			for (int i = 0; i < list.Length; i++)
			{
				Logger.Log(list[i]);
			}
		}

		private static void OnGUI()
		{
			// Todo: Provice in-game ability to customize?
			GUILayout.Label("NPC Custom Portrait Enabler is On!");
			GUILayout.Button("OK");
		}
	}
	
}