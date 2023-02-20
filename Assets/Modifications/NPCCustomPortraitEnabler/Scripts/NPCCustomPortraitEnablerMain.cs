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
		private const string  DefaultSubDirectory    = "πpcPortraits";
		private const string  DefaultDocumentation   = "https://github.com/Dheuster/NPCCustomPortraitEnabler/wiki/Mod-Config-Options";

		public static Kingmaker.Modding.OwlcatModification Modification { get; private set; }
		public static bool IsEnabled { get; private set; } = true;
		public static LogChannel Logger => Modification.Logger;
		private static ConfigData Config = new ConfigData();

		// private static Dictionary<string, BlueprintPortrait> CacheLookup = new Dictionary<string, BlueprintPortrait>();
		private static Dictionary<string, PortraitData> CacheLookup = new Dictionary<string, PortraitData>();
		private static Dictionary<string, NPCMonitor> MonitoredNPCs = new Dictionary<string, NPCMonitor>();

		// Most non-random-gen npc portraits end with "Portrait". The exceptions:
		private static HashSet<string> NameFilter = new HashSet<string>() 
		{
			"AasimarMaleBloodrager","DhampirFemaleBard","DwarfMaleRogue","DwarfMaleRogueUndead",
			"ElfMaleNoble","GnomeMaleNoble","HalfElfMaleMage","HalfOrcFemaleTank","HalfOrcFemaleTankScars",
			"HalflingFemaleRogue","HalflingMaleTank","HilorHumanMale","HumanTianXiaFemaleNoble",
			"MythicAzataFemale","OdanHalfelfMale","PetTriceratops","PetVelociraptor","PharasmaGoddessFemale",
			"TieflingFemalePriest","TreverHumanMwangiMaleWarrior","TricksterMythicMale","HumanMaleTank",
			"TieflingMaleArcher","MythicDevilMale"
		};

		// ReSharper disable once UnusedMember.Global
		[OwlcatModificationEnterPoint]
		public static void Initialize(Kingmaker.Modding.OwlcatModification modification)
		{

			Modification = modification;

			// Use Harmony to auto-patch our assembly into the running instance at startup.
            // Since nothing is saved to disk, this makes uninstall as easy as removing the mod.

			var harmony = new Harmony(modification.Manifest.UniqueName);
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			InitConfig();
			SaveConfig();

			if (!IsEnabled)
			{
				logAlways("NPCCustomPortraitEnabler Disabled from Mod Manager");
				return;
			}

			if (Config.Disabled)
			{
				logAlways("NPCCustomPortraitEnabler Disabled from json config.");
				IsEnabled = false;
				return;
			}

			// --== Confirm all expected/needed paths exist ==--
			string PersistentDataPath = Path.GetFullPath(Path.Combine(ApplicationPaths.persistentDataPath, "."));
			if (!verifyPathExists(PersistentDataPath))
			{
				return;
			}

			string WrathDataPath = Path.GetFullPath(Path.Combine(ApplicationPaths.dataPath, "."));
			if (!verifyPathExists(WrathDataPath))
			{
				return;
			}

			// ( "Portraits" is internally enforced and non-optional )
			string PortraitsRoot = Path.GetFullPath(Path.Combine(PersistentDataPath, "Portraits"));
			if (!ensurePathExists(PortraitsRoot))
			{
				return;
			}

			string DefaultNPCPortraitsRoot = Path.GetFullPath(Path.Combine(PortraitsRoot, DefaultSubDirectory));
			if (!ensurePathExists(DefaultNPCPortraitsRoot))
			{
				return;
			}

			// Users may change the config subdir to use some npc portrait pak. So just in case
			// DefaultSubDirectory and actual subDirectory dont match, check both:

			string NPCCustomPortraitsRoot = Path.GetFullPath(Path.Combine(PortraitsRoot, Config.SubDirectory));
			if (!ensurePathExists(NPCCustomPortraitsRoot))
			{
				return;
			}

			createShortCuts(PersistentDataPath, WrathDataPath, NPCCustomPortraitsRoot);

			// TODO: Load Rules? (Will we pre-load or wait until resource is requested?)
			// loadRules(NPCCustomPortraitsRoot)
			Config.SubDirectory += Path.DirectorySeparatorChar;

			logDebug("NPCCustomPortraitEnabler Registering for Events.");
			AddLoadResourceCallback();
			modification.OnDrawGUI += OnGUI;
			modification.IsEnabled += () => IsEnabled;
			modification.OnSetEnabled += enabled => IsEnabled = enabled;
			modification.OnShowGUI += () => Logger.Log("OnShowGUI");
			modification.OnHideGUI += () => Logger.Log("OnHideGUI");

			EventBus.Subscribe(new OnAreaLoadRelayHandler());
			EventBus.Subscribe(new OnWeatherChangedRelayHandler());
			EventBus.Subscribe(new OnSaveLoadRelayHandler());
			EventBus.Subscribe(new OnPartyChangeHandler());

			// EventBus.Subscribe(new OnIBookEventUIRelayHandler());
			// EventBus.Subscribe(new OnIBookPageRelayHandler());
			// EventBus.Subscribe(new OnMythicSelectionRelayHandler());
			// EventBus.Subscribe(new OnPartyLeaveAreaRelayHandler());
			// EventBus.Subscribe(new OnQuestRelayHandler());
			// EventBus.Subscribe(new OnRestFinishedRelayHandler());
			// EventBus.Subscribe(new OnSafeZoneRelayHandler());
			// EventBus.Subscribe(new OnUnitPortraitChangedRelayHandler());
		}

		private static void InitConfig()
        {
            // --== Load Config object from Game Engine ==--
            ConfigData loadedData = Modification.LoadData<ConfigData>();
            if (loadedData == null)
            {
                loadedData = new ConfigData();
            }
            if (String.IsNullOrEmpty(loadedData.LastLoadTime))
            {
                logAlways("NPCCustomPortraitsEnabler - Initializing : Using first run defaults");
                Config.Disabled = false;
                Config.LogDebug = false;
                Config.CreateIfMissing = false;
                Config.SubDirectory = DefaultSubDirectory;
                Config.Documentation = DefaultDocumentation;
            }
            else
            {
                Config.Disabled = loadedData.Disabled;
                Config.LogDebug = loadedData.LogDebug;
                Config.CreateIfMissing = loadedData.CreateIfMissing;
                Config.SubDirectory = loadedData.SubDirectory;
                Config.Documentation = loadedData.Documentation;
            }
            // --== Validate ==--
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
            
            // --== Convert any invalid characters to underscores ==--
            Config.SubDirectory = string.Join("_", Config.SubDirectory.Split(Path.GetInvalidFileNameChars()));
            if (Config.SubDirectory.Contains(".."))
            {
                logAlways("SubDirectory contains illegal path [..]: Resetting Value to Default.");
                Config.SubDirectory = DefaultSubDirectory;
            }
            Config.LastLoadTime = DateTime.Now.ToString();
            
            // --== Summarize values if debug logging is enabled ==--
            if (Config.LogDebug)
            {
                logStartupReport();
            }

			logDebug("InitConfig Complete");
            
		}

		private static void SaveConfig()
		{
			logDebug($"Saving Config Model");

			// NOTE: Config Model encompasses global settings that apply to all games/saves:
			//       Modification.SaveData is stored in the Modifications directory as a json file.

			Modification.SaveData(Config);

			logDebug($"Save Complete");

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

												// try
												// {
												//     File.WriteAllBytes(small, thePortrait.SmallPortrait.texture.EncodeToPNG());
												// }
												// catch (Exception ex)
												// {
												//     logAlways($"Exception creating small portrait [{small}]" + ex.ToString(), Array.Empty<object>());
												// }
												// try
												// {
												//     File.WriteAllBytes(medium, thePortrait.HalfLengthPortrait.texture.EncodeToPNG());
												// }
												// catch (Exception ex)
												// {
												//     logAlways($"Exception creating portrait [{medium}]" + ex.ToString(), Array.Empty<object>());
												// }
												// try
												// {
												//     File.WriteAllBytes(large, thePortrait.FullLengthPortrait.texture.EncodeToPNG());
												// }
												// catch (Exception ex)
												// {
												//     logAlways($"Exception creating portrait [{large}]" + ex.ToString(), Array.Empty<object>());
												// }
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
				StringWriter sw = new StringWriter();
				sw.WriteLine("");
				sw.WriteLine("-------------------------------------------------------");
				sw.WriteLine("Area Loaded");
				sw.WriteLine("-------------------------------------------------------");
				sw.WriteLine($"Difficulty: [{Kingmaker.Settings.SettingsRoot.Difficulty.GameDifficulty.GetValue()}]");
				sw.WriteLine($"Player Name: [{Kingmaker.Game.Instance.Player.MainCharacter.Value.CharacterName}]");
				sw.WriteLine("-------------------------------------------------------");
				logAlways(sw.ToString());
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
			if ("Portraits" != BlueprintRoot.Instance.CharGen.PortraitFolderName)
			{
				logAlways($"Error: Unexpected Portraits root directory [{BlueprintRoot.Instance.CharGen.PortraitFolderName}]. Expected [Portraits]. Mod unlikely to work.");
			}
		}

		// Relayed from OnPartyChangeHandler.cs
		public static void OnCompanionAdded(UnitEntityData unitEntityData)
        {
			logDebug("OnCompanionAdded Called");
			if (unitEntityData == null) return;
			if (unitEntityData.Descriptor == null) return;
			string name = unitEntityData.Descriptor.CharacterName;
			if (name == null) return;
			logDebug($"OnCompanionAdded Called for [{name}]");
			if (MonitoredNPCs.ContainsKey(name))
			{
				MonitoredNPCs[name].OnCompanionAdded();
			}
			else
			{
				MonitoredNPCs.Add(name, new NPCMonitor(unitEntityData, NPCCustomPortraitEnablerMain.Modification, NPCCustomPortraitEnablerMain.Config.LogDebug));
				MonitoredNPCs[name].OnCompanionAdded();
			}
			// May want Register/Unregister callback to subscribe/unsubscribe from events when NPCs are 
			// not member of the party. Issue may be camp scenes where non-party NPCs linger. Their 
			// portraits wouldn't update if we ignore people not in the party... 
		}

		// Relayed from OnPartyChangeHandler.cs
		public static void OnCompanionActivated(UnitEntityData unitEntityData)
		{
			logDebug($"OnCompanionActivated Called");
			if (unitEntityData == null) return;
			if (unitEntityData.Descriptor == null) return;
			string name = unitEntityData.Descriptor.CharacterName;
			if (name == null) return;
			logDebug($"OnCompanionActivated Called for [{name}]");
			if (MonitoredNPCs.ContainsKey(name))
			{
				MonitoredNPCs[name].OnCompanionActivated();
			}
		}

		// Relayed from OnPartyChangeHandler.cs
		public static void OnCompanionRemoved(UnitEntityData unitEntityData, bool stayInGame)
		{
			logDebug($"OnCompanionRemoved Called");
			if (unitEntityData == null) return;
			if (unitEntityData.Descriptor == null) return;
			string name = unitEntityData.Descriptor.CharacterName;
			if (name == null) return;
			logDebug($"OnCompanionRemoved Called for [{name}]");
			if (MonitoredNPCs.ContainsKey(name))
			{
				// not member of the party. 
				MonitoredNPCs[name].OnCompanionRemoved(stayInGame);
			}
			else
			{
				MonitoredNPCs.Add(name, new NPCMonitor(unitEntityData, NPCCustomPortraitEnablerMain.Modification, NPCCustomPortraitEnablerMain.Config.LogDebug));
				MonitoredNPCs[name].OnCompanionRemoved(stayInGame);
			}
			// May want Register/Unregister callback to subscribe/unsubscribe from events when NPCs are 
			// not member of the party. Issue may be camp scenes where non-party NPCs linger. Their 
			// portraits wouldn't update if we ignore people not in the party... 
		}


		public static void OnSaveLoad(SaveLoadMode mode, bool singleMode)
		{
			logDebug("OnSaveLoad()");
			logDebug($"mode [{mode}]");
			logDebug($"singleMode [{singleMode}]");

			// There is a way to save data as part of the games save file, which is then specific to that 
			// save. (Makes more sense for content updates like new quests, etc..). If you save
			// global data to the json AND local data to the save, you can detect when the global settings
			// have changed if you have any cached/update type work that needs to happen... (Detect if
			// version of mod used with save is older than current version).

			// NOTE: Using global permanent storage does not create dependencies on the mod and the 
			// users save games. If you save data within the save games themselves and the user uninstalls
			// your mod, it could leave saves made while your mod was installed corrupt. 

			// var data = Game.Instance.Player.Ensure<EntityPartKeyValueStorage>().GetStorage(ModificationRoot.Modification.Manifest.UniqueName);
			// if (data.Get(propertyName) == null)
			// {
			//     data["version"] = "1";
			//     data["numTimesSavedOrLoaded"] = "0";
			// }
			// else
			// {
			//     data["version"] = (int.Parse(data["version"])).ToString();
			//     data["numTimesSavedOrLoaded"] =  (int.Parse(data["numTimesSavedOrLoaded"]) + 1).ToString();
			// }

		}
		// Relayed by : OnWeatherChangedRelayHandler
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
        
        private static void createShortCuts(string PersistentDataPath, string WrathDataPath, string NPCCustomPortraitsRoot)
        {
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
            
            string configLinkPath = Path.GetFullPath(Path.Combine(WrathDataPath, "..", "Config.lnk"));
			ensureShortCut(configLinkPath, PersistentDataPath, "Config");
            
            string owlcatGamesLinkPath = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Owlcat Games.lnk"));
            ensureShortCut(owlcatGamesLinkPath, Path.GetFullPath(Path.Combine(PersistentDataPath, "..")), "Owlcat Games");

            string npcPortraitsLinkPath = Path.GetFullPath(Path.Combine(PersistentDataPath, "NPCPortraits.lnk"));
            ensureShortCut(npcPortraitsLinkPath, NPCCustomPortraitsRoot, "NPCPortraits");

		}

		private static bool verifyPathExists(string dirPath)
        {
			if (Directory.Exists(dirPath))
			{
				logDebug($"Confirmed Path [{dirPath}] exists");
				return true;
			}
            logAlways($"Path [{dirPath}] not found. Aborting Initialization");
            Config.Disabled = true;
            IsEnabled = false;
            return false;
		}

		private static bool ensurePathExists(string dirPath)
        {
			if (Directory.Exists(dirPath))
			{
				logDebug($"Confirmed Path [{dirPath}]");
				return true;
			}
			logDebug($"Path [{dirPath}] not found. Attempting Creation");
			string errorMsg = "Unknown Reason. Aborting initialization.";
			try
			{
				Directory.CreateDirectory(dirPath);
				if (Directory.Exists(dirPath))
				{
					logDebug($"Path [{dirPath}] created");
					return true;
				}
			}
			catch (Exception objException)
			{
				errorMsg = objException.Message;
			}

			logAlways($"Unable to create folder [{dirPath}]: {errorMsg}");
			Config.Disabled = true;
			IsEnabled = false;
			return false;
		}

		private static void logStartupReport()
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

        private static bool ensureShortCut(string linkPath, string targetPath, string description)
        {
            if (File.Exists(linkPath))
            {
                logDebug($"Confirmed Ease of Access Shortcut [{linkPath}]");
                return true;
            }
            logAlways($"Attempting to creating ease of acccess shortcut: [{linkPath}]");
            string errorMsg = "Generic Failure";
            try
            {
                using (ShellLink shortcut = new ShellLink())
                {
                    shortcut.Target = targetPath;
                    // shortcut.WorkingDirectory = ApplicationPaths.persistentDataPath;
                    shortcut.Description = description;
                    shortcut.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
                    shortcut.Save(linkPath);
                }
                if (File.Exists(linkPath))
                {
                    logAlways($"Ease of Access Shortcut [{linkPath}] created.");
                    return true;
                }
            }
            catch (Exception objException)
            {
                errorMsg = objException.Message;
            }
            logAlways($"Unable to create ease of access shortcut [{linkPath}] : [{errorMsg}]");
			return false;
        }

		private static void loadRules(string NPCCustomPortraitsRoot)
        {
			// -----------------------------------------------------------
			// Pre-Scan and Load NPC Portrait Rules (TODO)
			// -----------------------------------------------------------
			
			// if (Directory.Exists(NPCCustomPortraitsRoot))
			// {
			//	  logDebug($"Path [{Application.persistentDataPath}/portraits/{Config.SubDirectory}] not found. Skippnig Rule Scan.");
			//	  return;
			// }
			// using (CodeTimer.New("LoadAllJson"))
			// {
			//	foreach (string text in Directory.EnumerateFiles(path, "*.jbp", SearchOption.AllDirectories))
			//	{
			//		try
			//		{
			//			BlueprintJsonWrapper blueprintJsonWrapper = BlueprintJsonWrapper.Load(text);
			//			blueprintJsonWrapper.Data.OnEnable();
			//			ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(BlueprintGuid.Parse(blueprintJsonWrapper.AssetId), blueprintJsonWrapper.Data);
			//		}
			//		catch (Exception ex)
			//		{
			//			PFLog.Default.Error("Failed loading blueprint: " + text, Array.Empty<object>());
			//			PFLog.Default.Exception(ex, null, Array.Empty<object>());
			//		}
			//	}
			// }
		}

		// (eventually) relayed by : OnIBookEventUIRelayHandler or OnIBookPageRelayHandler
		/*
		public static void OnBookPageShow(string title, string text)
        {
			logDebug("OnBookPageShow()");
			logDebug($"title: [{title}] text: [{text}]");
		}
		*/

		// (eventually) Relayed by : OnSafeZoneRelayHandler
		/*
		public static void OnEnterSafeZone()
		{
			// Good time to kick off side quests or romance conversations that we don't want interrupted.
			logDebug("OnEnterSafeZone()");
		}
		*/

		// (eventually) relayed by: OnPartyLeaveAreaRelayHandler
		/*
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
		*/
		// (eventually) Relayed by : OnQuestRelayHandler
		/*
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
		*/
		// (eventually?) Relayed by: OnRestFinishedRelayHandler
		/*
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
		*/
	}

	// -----------------------------------------------------------------------
	// NOTES:
	// -----------------------------------------------------------------------
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
	//     BlueprintPortrait updatedPortrait = BlueprintRoot.Instance.CharGen.CustomPortrait
	//     updatedPortrait.Data = new PortraitData(portraitId); // Use cached value?
	//     unit.UISettings.SetPortrait(blueprintPortrait)
	//
	// We might also be able to change the Data part in place and then call 
	// SetPortrait on its current value to cause a refresh/reload

	// Search for:
	//     UnitEntityView unitEntityView = unit.View.Or(null);
	//     UnitAnimationManager unitAnimationManager = (unitEntityView != null) ? unitEntityView.AnimationManager : null;
	//     if (!(unitAnimationManager == null))
	//     {
	//         ...
	//     }
	//
	// UnitDescriptor unit.Descriptor
	//
	// unit.Descriptor.
	// <UnitPartDollData>.SetDefault(DollData data)
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
	// _npc/Camellia/Civility/Lawful
	// _npc/Camellia/Civility/Neutral
	// _npc/Camellia/Civility/Chaotic
	// _npc/Camellia/Morality/Good
	// _npc/Camellia/Morality/Neutral
	// _npc/Camellia/Morality/Evil
	// _npc/Camellia/Acuity/WellRested
	// _npc/Camellia/Acuity/Tired
	// _npc/Camellia/Acuity/Exhausted
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