// System/C# Generic
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;

// Unity Engine Specific
using UnityEngine;                       // Required for GUILayout

// WOTR Specific
using Kingmaker;                         // Required for Game
using Kingmaker.Blueprints;              // Required for PortraitData
using Kingmaker.Blueprints.Classes;      // Required for BlueprintCharacterClass, BlueprintRace
using Kingmaker.Blueprints.CharGen;      // Required for CustomizationOptions, BlueprintRaceVisualPreset
using Kingmaker.Blueprints.Root;         // Required for BlueprintRoot
using Kingmaker.DialogSystem.Blueprints; // Required for BlueprintDialog
using Kingmaker.EntitySystem.Entities;   // Required for UnitEntityData
using Kingmaker.Modding;                 // Required for OwlcatModificationEnterPointAttribute
using Kingmaker.PubSubSystem;            // Required for EventBus
using Kingmaker.ResourceLinks;           // Required for EquipmentEntityLink
using Kingmaker.UI.MVVM._VM.SaveLoad;    // Required for SaveLoadMode
using Kingmaker.UnitLogic.Abilities;     // Required for AbilityExecutionContext
using Kingmaker.UnitLogic.Buffs;         // Required for Polymorph
using Kingmaker.UnitLogic.Parts;         // Required for UnitPartCompanion
using Kingmaker.Utility;                 // Required for TargetWrapper
using Kingmaker.View;                    // Required for UnitEntityView
using Kingmaker.Visual.CharacterSystem;  // Required for EquipmentEntity, Character


using Owlcat.Runtime.Core.Logging;

// Mod Specific
using OwlcatModification.Modifications.CompanionPortraitEnabler.Relay;
using OwlcatModification.Modifications.CompanionPortraitEnabler.Rules;
using OwlcatModification.Modifications.CompanionPortraitEnabler.Utility;

// 3rd Party (But included with WOTR)
using HarmonyLib;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler
{
	public static class CompanionPortraitEnablerMain
	{
		private const string  DefaultSubDirectory       = "πpcPortraits";
		private const string  DefaultSnapshotsDirectory = "πpcSnapshots";
		private const string  DefaultDocumentation      = "https://github.com/Dheuster/CompanionPortraitEnabler/wiki/Mod-Config-Options";

		public static Kingmaker.Modding.OwlcatModification Modification { get; private set; }
		public static bool IsEnabled { get; private set; } = true;
		public static LogChannel Logger => Modification.Logger;
		private static ConfigData Config = new ConfigData();
		public static bool EnableAllUnitScan { get; private set; } = true;

		public static string ExpectedPortraitDir     = "Portraits";
		public static string PersistentDataPath      = null;
		public static string WrathDataPath           = null;
		public static string PortraitsRoot           = null;
		public static string DefaultNPCPortraitsRoot = null;
		public static string DefaultSnapshotsRoot    = null;		
		public static string CompanionPortraitsRoot  = null;

		public static bool   ResetFlag               = false;

		// private static Dictionary<string, BlueprintPortrait> CacheLookup = new Dictionary<string, BlueprintPortrait>();
		private static Dictionary<string, PortraitData> CacheLookup = new Dictionary<string, PortraitData>();
		private static Dictionary<string, NPCMonitor> MonitoredNPCs = new Dictionary<string, NPCMonitor>();
		private static Dictionary<string, string> EquipmentEntityCache = new Dictionary<string, string>();

		public static void resetState()
        {
			Log.trace("Resetting State");
			CompanionPortraitEnablerMain.ResetFlag = false;
			CacheLookup.Clear();
			CacheLookup = new Dictionary<string, PortraitData>();
			EquipmentEntityCache.Clear();
			EquipmentEntityCache = new Dictionary<string, string>();
			foreach(string charName in MonitoredNPCs.Keys) {
				MonitoredNPCs[charName].destroy();
			}
			MonitoredNPCs.Clear();
			MonitoredNPCs = new Dictionary<string, NPCMonitor>();
            if (Config.LogDebug)
            {
                logStartupReport();
            }
			JsonUtil.resetState();
        }

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
			Log.init(modification);

			// Use Harmony to auto-patch our assembly into the running instance at startup.
            // Since nothing is saved to disk, this makes uninstall as easy as removing the mod.

			var harmony = new Harmony(modification.Manifest.UniqueName);
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			// --== Confirm all expected/needed paths exist ==--
			CompanionPortraitEnablerMain.PersistentDataPath = Path.GetFullPath(Path.Combine(ApplicationPaths.persistentDataPath, "."));
			if (!verifyPathExists(CompanionPortraitEnablerMain.PersistentDataPath))
			{
				return;
			}

			InitConfig();

			if (!IsEnabled)
			{
				Log.always("CompanionPortraitEnabler Disabled from Mod Manager");
				return;
			}

			if (Config.Disabled)
			{
				Log.always("CompanionPortraitEnabler Disabled from json config.");
				IsEnabled = false;
				return;
			}

			CompanionPortraitEnablerMain.WrathDataPath = Path.GetFullPath(Path.Combine(ApplicationPaths.dataPath, "."));
			if (!verifyPathExists(CompanionPortraitEnablerMain.WrathDataPath))
			{
				return;
			}

			// ( "Portraits" is internally enforced and non-optional )
			CompanionPortraitEnablerMain.PortraitsRoot = Path.GetFullPath(Path.Combine(PersistentDataPath, ExpectedPortraitDir));
			if (!ensurePathExists(CompanionPortraitEnablerMain.PortraitsRoot))
			{
				return;
			}

			CompanionPortraitEnablerMain.DefaultNPCPortraitsRoot = Path.GetFullPath(Path.Combine(CompanionPortraitEnablerMain.PortraitsRoot, DefaultSubDirectory));
			if (!ensurePathExists(CompanionPortraitEnablerMain.DefaultNPCPortraitsRoot))
			{
				return;
			}

			CompanionPortraitEnablerMain.DefaultSnapshotsRoot = Path.GetFullPath(Path.Combine(CompanionPortraitEnablerMain.PortraitsRoot, DefaultSnapshotsDirectory));
			if (!ensurePathExists(CompanionPortraitEnablerMain.DefaultSnapshotsRoot))
			{
				return;
			}

			// Users may change the config subdir to use some npc portrait pak. So just in case
			// DefaultSubDirectory and actual subDirectory dont match, check both:

			CompanionPortraitEnablerMain.CompanionPortraitsRoot = Path.GetFullPath(Path.Combine(CompanionPortraitEnablerMain.PortraitsRoot, Config.SubDirectory));
			if (!ensurePathExists(CompanionPortraitsRoot))
			{
				return;
			}

			if (Config.AllowShortcutCreation)
			{ 
				createShortCuts(
					CompanionPortraitEnablerMain.PersistentDataPath, 
					CompanionPortraitEnablerMain.WrathDataPath, 
					CompanionPortraitEnablerMain.CompanionPortraitsRoot
				);
			}
			
			if (!Config.SubDirectory.EndsWith("" +  Path.DirectorySeparatorChar))
			{ 
				Config.SubDirectory += Path.DirectorySeparatorChar;
			}

			Log.debug("CompanionPortraitEnabler Registering for Events.");
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
			EventBus.Subscribe(new OnUnitLevelupHandler());
			EventBus.Subscribe(new OnPolymorphDeactivatedHandler());
			EventBus.Subscribe(new OnDialogEventHandler());			
			EventBus.Subscribe(new OnAbilityEffectAppliedHandler());
			// EventBus.Subscribe(new OnSafeZoneRelayHandler());
		}

		public class ConfigData
		{
			// A flattened version of the settings to be shared... and allows valid validation...
			public string SubDirectory         = "πpcPortraits";
			public bool LogDebug             = false;
			public bool LogTrace             = false;
			public bool Disabled             = false;
			public bool CreateIfMissing      = true;

			public string SnapshotHome         = "πpcSnapshots";
            public bool AllowPortraits         = true;
            public bool AllowPortraitRules     = true;
            public bool AllowBodies            = true;
            public bool AllowBodyRules         = true;
            public bool AllowPartyInfo         = true;
            public bool AllowDialogueInfo      = true;
            public bool AllowEquipmentInfo     = true;
            public bool AllowBodySnapshots     = true;
            public bool AllowShortcutCreation  = true;
            public bool AvoidNudity            = true;
			public bool AutoScale              = true;
			
			public bool UninstallMode          = false;

		}

		private static void InitConfig()
        {
            // --== Load Config  ==--
            // Settings settings = Modification.LoadData<Settings>(); // Game Engine supported method. Not a fan. Can't control name of file or have overrides... 

			string settingsPath = Path.Combine(PersistentDataPath,"Modifications","CompanionPortraitEnabler_Settings.json");
			Settings settings   = JsonUtil.LoadSettings(settingsPath);
			Settings overrides  = JsonUtil.LoadSettings(Path.Combine(PersistentDataPath,"Modifications","CompanionPortraitEnabler_Overrides.json"));
			bool saveSettings = false;

			if (null == settings)
            {
			    settings = new Settings();
				saveSettings = true;
			}

			// Set Defaults for any missing properties... 

		    settings.general.Name                          = settings.general.Name                          ?? "Companion Portrait Enabler";
		    settings.general.Website                       = settings.general.Website                       ?? "http://www.nexusmods.com/...";
		    settings.general.Comments                      = settings.general.Comments                      ?? "source: https://github.com/Dheuster/CompanionPortraitEnabler/";

			settings.portraitSettings.PortraitHome         = settings.portraitSettings.PortraitHome         ?? "πpcPortraits";
			settings.portraitSettings.AllowPortraits       = settings.portraitSettings.AllowPortraits       ?? Boolean.TrueString;
			settings.portraitSettings.AllowPortraitRules   = settings.portraitSettings.AllowPortraitRules   ?? Boolean.TrueString;
			settings.portraitSettings.CreateMissingFolders = settings.portraitSettings.CreateMissingFolders ?? Boolean.FalseString;

			settings.bodySettings.AllowBodies              = settings.bodySettings.AllowBodies              ?? Boolean.TrueString;
			settings.bodySettings.AvoidNudity              = settings.bodySettings.AvoidNudity              ?? Boolean.TrueString;
			settings.bodySettings.AutoScale                = settings.bodySettings.AutoScale                ?? Boolean.TrueString;
			settings.bodySettings.AllowBodyRules           = settings.bodySettings.AllowBodyRules           ?? Boolean.TrueString;

			settings.snapshotSettings.SnapshotHome         = settings.snapshotSettings.SnapshotHome         ?? "πpcSnapshots";
			settings.snapshotSettings.AllowPartyInfo       = settings.snapshotSettings.AllowPartyInfo       ?? Boolean.TrueString;
			settings.snapshotSettings.AllowDialogueInfo    = settings.snapshotSettings.AllowDialogueInfo    ?? Boolean.TrueString;
			settings.snapshotSettings.AllowEquipmentInfo   = settings.snapshotSettings.AllowEquipmentInfo   ?? Boolean.TrueString;
			settings.snapshotSettings.AllowBodySnapshots   = settings.snapshotSettings.AllowBodySnapshots   ?? Boolean.TrueString;

			settings.logging.LogTrace                      = settings.logging.LogTrace                      ?? Boolean.FalseString;
			settings.logging.LogDebug                      = settings.logging.LogDebug                      ?? Boolean.FalseString;

			settings.permissions.AllowShortcutCreation     = settings.permissions.AllowShortcutCreation     ?? Boolean.TrueString;

			settings.Disabled                              = settings.Disabled                              ?? Boolean.FalseString;
			settings.UninstallMode                         = settings.UninstallMode                         ?? Boolean.FalseString;

			if (saveSettings) 
			{ 
				JsonUtil.SaveSettings(settings,settingsPath);
            }

			if (null == overrides)
            {
				Log.always("Overrides is null");
            }
            else
            {
				Log.always("Loading Overrides");
				if (null == (overrides?.portraitSettings?.PortraitHome))
				{
					Log.always("Overrides: PortraitHome is null");
				}
				if (null == (overrides?.portraitSettings?.CreateMissingFolders))
				{
					Log.always("Overrides: CreateMissingFolders is null");
				}
				if (null == (overrides?.portraitSettings?.AllowPortraits))
				{
					Log.always("Overrides: AllowPortraits is null");
				}
				if (null == (overrides?.portraitSettings?.AllowPortraitRules))
				{
					Log.always("Overrides: AllowPortraitRules is null");
				}
				if (null == (overrides?.bodySettings?.AllowBodies))
				{
					Log.always("Overrides: AllowBodies is null");
				}
				if (null == (overrides?.bodySettings?.AllowBodyRules))
				{
					Log.always("Overrides: AllowBodyRules is null");
				}
				if (null == (overrides?.bodySettings?.AvoidNudity))
				{
					Log.always("Overrides: AvoidNudity is null");
				}
				if (null == (overrides?.bodySettings?.AutoScale))
				{
					Log.always("Overrides: AutoScale is null");
				}
            }

			Config.SubDirectory           = (overrides?.portraitSettings?.PortraitHome)         ?? (settings.portraitSettings.PortraitHome);
            Config.CreateIfMissing        = Boolean.Parse((overrides?.portraitSettings?.CreateMissingFolders) ?? (settings.portraitSettings.CreateMissingFolders));
            Config.AllowPortraits         = Boolean.Parse((overrides?.portraitSettings?.AllowPortraits)       ?? (settings.portraitSettings.AllowPortraits));
            Config.AllowPortraitRules     = Boolean.Parse((overrides?.portraitSettings?.AllowPortraitRules)   ?? (settings.portraitSettings.AllowPortraitRules));
            Config.AllowBodies            = Boolean.Parse((overrides?.bodySettings?.AllowBodies)              ?? (settings.bodySettings.AllowBodies));
            Config.AvoidNudity		      = Boolean.Parse((overrides?.bodySettings?.AvoidNudity)              ?? (settings.bodySettings.AvoidNudity));
            Config.AutoScale		      = Boolean.Parse((overrides?.bodySettings?.AutoScale)                ?? (settings.bodySettings.AutoScale));
            Config.AllowBodyRules         = Boolean.Parse((overrides?.bodySettings?.AllowBodyRules)           ?? (settings.bodySettings.AllowBodyRules));
			Config.SnapshotHome           = (overrides?.snapshotSettings?.SnapshotHome)         ?? (settings.snapshotSettings.SnapshotHome);
            Config.AllowPartyInfo         = Boolean.Parse((overrides?.snapshotSettings?.AllowPartyInfo)       ?? (settings.snapshotSettings.AllowPartyInfo));
            Config.AllowDialogueInfo      = Boolean.Parse((overrides?.snapshotSettings?.AllowDialogueInfo)    ?? (settings.snapshotSettings.AllowDialogueInfo));
            Config.AllowEquipmentInfo     = Boolean.Parse((overrides?.snapshotSettings?.AllowEquipmentInfo)   ?? (settings.snapshotSettings.AllowEquipmentInfo));
            Config.AllowBodySnapshots     = Boolean.Parse((overrides?.snapshotSettings?.AllowBodySnapshots)   ?? (settings.snapshotSettings.AllowBodySnapshots));
            Config.LogDebug               = Boolean.Parse(settings.logging.LogDebug);
            Config.LogTrace               = Boolean.Parse(settings.logging.LogTrace);
            Config.AllowShortcutCreation  = Boolean.Parse(settings.permissions.AllowShortcutCreation);
            Config.Disabled               = Boolean.Parse(settings.Disabled);			
			Config.UninstallMode          = Boolean.Parse(settings.UninstallMode);

			if (Config.UninstallMode)
            {
				Config.CreateIfMissing = false;
				Config.AllowPortraits = false;
				Config.AllowPortraitRules = false;
				Config.AllowBodies = false;
				Config.AllowBodyRules = false;
            }

			Log.setup(Config.LogDebug, Config.LogTrace);

            // --== Validate ==--
            if (String.IsNullOrWhiteSpace(Config.SubDirectory))
            {
                Log.always("TRACE: PortraitHome is null, empty or consists of all spaces. Resetting Value to Default.");
                Config.SubDirectory = DefaultSubDirectory;
            }
            if (String.IsNullOrWhiteSpace(Config.SnapshotHome))
            {
                Log.always("TRACE: SnapshotHome is null, empty or consists of all spaces. Resetting Value to Default.");
                Config.SnapshotHome = DefaultSnapshotsDirectory;
            }
            
            Config.SubDirectory = ensureValidFileName(Config.SubDirectory, "PortraitHome", DefaultSubDirectory);

            Config.SnapshotHome = ensureValidFileName(Config.SnapshotHome, "SnapshotHome", DefaultSnapshotsDirectory);
				
            
            // --== Summarize values if debug logging is enabled ==--
            if (Config.LogDebug)
            {
                logStartupReport();
            }

			if (Config.UninstallMode)
            {
				// This will cause all portraits to reset to defaults when the use loads a game... 
				Config.SubDirectory = "___CPEUNINSTALL___";
            }
			Log.trace("TRACE: InitConfig Complete");            
		}

		public static string ensureValidFileName(string filename,  string defaultErrorPrefix = null, string defaultValue = null)
        {
			if (String.IsNullOrWhiteSpace(filename))
            {
				if (null != defaultValue && defaultValue.Length > 0)
				{
					if (null != defaultErrorPrefix && defaultErrorPrefix.Length > 0) 
					{ 
		                Log.always($"{defaultErrorPrefix} is null, empty or consists of all spaces.  Returning Default Value [{defaultValue}]");
						return defaultValue;
					}
					Log.always($"Illegal path encountered that is null, empty or consists of all spaces. Returning Default Value [{defaultValue}]");
					return defaultValue;
				}
				if (null != defaultErrorPrefix && defaultErrorPrefix.Length > 0) { 
					Log.always($"{defaultErrorPrefix} contains contains path that is null, empty or consists of all spaces. Returning [NULL]");
					return "NULL";
				}
				Log.always($"Illegal path that is null, empty or consists of all spaces encountered: Returning [NULL]");
				return "NULL";
            }
			string ret = string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
			if (ret.Contains(".."))
            {
				if (null != defaultValue && defaultValue.Length > 0)
				{
					if (null != defaultErrorPrefix && defaultErrorPrefix.Length > 0) 
					{ 
		                Log.always($"{defaultErrorPrefix} contains illegal path [{filename}].  Returning Default Value [{defaultValue}]");
						return defaultValue;
					}
					Log.always($"Illegal path [{filename}] encountered. Returning Default Value [{defaultValue}]");
					return defaultValue;
				}
				if (null != defaultErrorPrefix && defaultErrorPrefix.Length > 0) { 
					Log.always($"{defaultErrorPrefix} contains contains illegal path [{filename}]. Returning [NULL]");
					return "NULL";
				}
				Log.always($"Illegal path [{filename}] encountered: Returning [NULL]");
				return "NULL";
			}
			return ret;
        }

	    // Called from rule engine with rule indicates it is time to change a portrait...
		public static PortraitData UpdatedPortraitDataCache(string resourceId, string portraitId)
        {
			Log.trace("TRACE UpdatedPortraitDataCache Called");
			if (null != resourceId)
			{
				try
				{
					PortraitData injectedData = null;
					if (CacheLookup.ContainsKey(resourceId))
					{
						injectedData = CacheLookup[resourceId];
					} 
					else
					{
						CacheLookup[resourceId] = null;
					}
					if (injectedData != null)
					{
						Log.trace($"TRACE: Updating PortraitData for [{resourceId}]: Returning Cached CustomPortrait [{portraitId}]");
					}
					else
					{
						Log.trace($"TRACE: Creating PortraitData for [{resourceId}]: Returning Cached CustomPortrait [{portraitId}]");
					}
					if (CustomPortraitsManager.Instance.EnsureDirectory(portraitId, false))
					{
						Log.debug($"Portrait folder found for [{portraitId}]. Confirming files...");
						string small = CustomPortraitsManager.Instance.GetSmallPortraitPath(portraitId);
						string medium = CustomPortraitsManager.Instance.GetMediumPortraitPath(portraitId);
						string large = CustomPortraitsManager.Instance.GetBigPortraitPath(portraitId);
						if (Config.AllowPortraits && File.Exists(small) && File.Exists(medium) && File.Exists(large))
						{
							PortraitData portraitData = new PortraitData(portraitId);
							if (null != portraitData.m_PortraitImage)
							{
								// Set AssetId to empty string so that PortraitData.m_portraitImage.Exists() fails in PreLoad() method
								// Helps address any code that assumes companion portraits are not custom portraits.
								portraitData.m_PortraitImage.AssetId = "";
							}
							CacheLookup[resourceId] = portraitData;
							if (null != injectedData)
                            {
								injectedData = null;
                            }
							portraitData.SmallPortraitHandle.Load();
							portraitData.HalfPortraitHandle.Load();
							portraitData.FullPortraitHandle.Load();
							return portraitData;
						}
						else if (Config.AllowPortraits)
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
							Log.debug($"[{portraitId}] - Files [{missing}] not found in [{CustomPortraitsManager.Instance.GetPortraitFolderPath(portraitId)}]");
						}
						else
                        {
							Log.debug($"Bailing. AllowPortraits is False...");
                        }
					}
					else
					{
						Log.debug($"[{portraitId}] - Not found in Portraits folder");
					}
				}
				catch (Exception ex)
                {
					Log.always($"Error: Exception processing portrait update for [{resourceId}]: {ex.ToString()}");
                }
			}
			else
            {
				Log.always($"Error: resourceId is null!");
            }
			return null;
		}

		private static void AddLoadResourceCallback()
		{
			Modification.OnLoadResource += (resource, guid) =>
			{
				
				if (null != (resource as Kingmaker.Blueprints.BlueprintPortrait))
				{
					BlueprintPortrait blueprintPortrait = (resource as Kingmaker.Blueprints.BlueprintPortrait);
					string name = (resource as UnityEngine.Object)?.name ?? resource.ToString();
					if (null != name)
					{
						if (!CacheLookup.ContainsKey(name))
						{
							// Prevent future inquires whether we succeed or fail...
							CacheLookup[name] = null;
							Log.trace($"TRACE: BlueprintPortrait resource requested for [{name}] [{guid}]");
							if (name.ToLower().EndsWith("_portrait") || NameFilter.Contains(name))
							{
								try
								{
									string portraitId = Config.SubDirectory + name;
									if (CustomPortraitsManager.Instance.EnsureDirectory(portraitId, false))
									{
										Log.debug($"Portrait folder found for [{portraitId}]. Confirming files...");
										string small = CustomPortraitsManager.Instance.GetSmallPortraitPath(portraitId);
										string medium = CustomPortraitsManager.Instance.GetMediumPortraitPath(portraitId);
										string large = CustomPortraitsManager.Instance.GetBigPortraitPath(portraitId);
										if (Config.AllowPortraits && File.Exists(small) && File.Exists(medium) && File.Exists(large))
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

											Log.debug($"Intercepted Request for [{name}]: Returning CustomPortrait [{portraitId}]");
											BlueprintPortrait altered = (resource as Kingmaker.Blueprints.BlueprintPortrait);
											altered.Data = injectedData;
											(resource as Kingmaker.Blueprints.BlueprintPortrait).Data = injectedData;
											CacheLookup[name] = injectedData;

										}
										else if (Config.AllowPortraits)
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
											Log.debug($"[{portraitId}] - Files [{missing}] not found in [{CustomPortraitsManager.Instance.GetPortraitFolderPath(portraitId)}]");
										} 
										else
                                        {
											Log.debug($"Bailing. AllowPortraits is False...");
                                        }
									}
									else
									{
										Log.debug($"[{portraitId}] - Not found in Portraits folder");
										if (Config.CreateIfMissing)
										{
											// Only do this on the first cach miss....
											if (CustomPortraitsManager.Instance.EnsureDirectory(portraitId, true))
											{
												Log.debug($"Created portraits directory for [{name}]:");
											}
											else
											{
												Log.always($"Failed to create portraits directory for [{name}]");
											}
										}
									}
								}
								catch (Exception ex)
								{
									Log.always($"Exception processing portrait request for [{name}] [{guid}]:{ex.ToString()}");
								}
							}
							else
							{
								Log.trace($"TRACE: Skipping [{name}] : Does not end with [_Portrait] and is not a known special case");
							}
						}
						else
						{
							if (null != CacheLookup[name])
							{
								Log.always($"Intercepted Request for [{name}]: Returning Cached CustomPortrait [{Config.SubDirectory + name}]");
								PortraitData injectedData = CacheLookup[name];
								BlueprintPortrait altered = (resource as Kingmaker.Blueprints.BlueprintPortrait);
								altered.Data = injectedData;
								(resource as Kingmaker.Blueprints.BlueprintPortrait).Data = injectedData;
							}
						}
					}
					else
					{
						Log.trace($"TRACE: BlueprintPortrait [{guid}] has no name!");
					}
				} 
				else if (resource as EquipmentEntity)
                {
					EquipmentEntityCache[(resource as EquipmentEntity).name] = guid;					
                }
			};
		}

		//--------------------------------------------------------------------
		// Events : General
		//--------------------------------------------------------------------
		// These events are general and do not apply to anyone specifically or
		// they apply to the Player
		//--------------------------------------------------------------------


		// Area finished loading and now can be safely accessed (If applying visual changes for example). 
		public static void OnAreaActivated()
		{
            try 
			{
				Log.trace("TRACE: OnAreaActivated Called");
				// PROTOTYPE: Try always reseting state when an area loads... period.

				if (CompanionPortraitEnablerMain.ResetFlag) { 
					// Delayed Reset because someone opened up the Load Game Menu... 
					CompanionPortraitEnablerMain.resetState();
				}

				if (null != BlueprintRoot.Instance.CharGen.PortraitFolderName)
				{
					if (CompanionPortraitEnablerMain.ExpectedPortraitDir != BlueprintRoot.Instance.CharGen.PortraitFolderName)
					{
						CompanionPortraitEnablerMain.ExpectedPortraitDir = BlueprintRoot.Instance.CharGen.PortraitFolderName;

						CompanionPortraitEnablerMain.PortraitsRoot = Path.GetFullPath(Path.Combine(
							CompanionPortraitEnablerMain.PersistentDataPath, 
							CompanionPortraitEnablerMain.ExpectedPortraitDir)
						);
						ensurePathExists(CompanionPortraitEnablerMain.PortraitsRoot);
  						CompanionPortraitEnablerMain.DefaultNPCPortraitsRoot = Path.GetFullPath(Path.Combine(
							CompanionPortraitEnablerMain.PortraitsRoot, 
							DefaultSubDirectory)
						);
						ensurePathExists(CompanionPortraitEnablerMain.DefaultNPCPortraitsRoot);
						CompanionPortraitEnablerMain.CompanionPortraitsRoot = Path.GetFullPath(Path.Combine(
							CompanionPortraitEnablerMain.PortraitsRoot, 
							Config.SubDirectory)
						);
						ensurePathExists(CompanionPortraitsRoot);
					}
				}

				// When an area loads, we need to do things in this order:
				// 1) Introspect all units (as it detected/creates NPCMonitor instances for companions)
				// 2) Call OnAreaActivated() for all NPCs (So their rule contexts will update)
				// 3) Create the developer snapshots. Since we already did the introspect, we have flag to skip it.

				try 
				{ 
					foreach (UnitEntityData unit in Game.Instance.State.Units) 
					{
						HandleUnitIntrospect(unit);
					}
				} 
				catch (Exception ex)
				{
					Log.debug($"Error inspecting units: {ex.ToString()}");
				}

				foreach(string charName in MonitoredNPCs.Keys) {
					MonitoredNPCs[charName].OnAreaActivated();
				}

				CreateDeveloperSnapshots(false);
			
				if ("Portraits" != BlueprintRoot.Instance.CharGen.PortraitFolderName) {
					Log.always($"Error: Unexpected Portraits root directory [{BlueprintRoot.Instance.CharGen.PortraitFolderName}]. Expected [Portraits]. Mod unlikely to work.");
				}
			}
			catch (Exception ex) 
			{
				Log.always($"OnAreaActivated: Exception Caught : {ex.ToString()}");
			}
		}

		public static void CreateDeveloperSnapshots(bool introspect = true)
        {
			if (introspect)
			{ 
				try 
				{ 
					foreach (UnitEntityData unit in Game.Instance.State.Units) 
					{

						//--------------------------------------------------------------------
						// NOTES:
						//--------------------------------------------------------------------
						// During the prologue, the game uses a number of NPCs that are companion
						// clones. The real companions are not spawned until after the prologue and
						// any calls to Game.Instance.Player.PartyCharacters returns an empty list.
						//
						// "8e0bb56ebdd92274bab3c840a8261d9e" ==  Fake Sosiel
						//	"54be53f0b35bf3c4592a97ae335fe765" == Fake Seelah
						//	"397b090721c41044ea3220445300e1b8" == Fake Camellia
						//	"54baba4efada97d44837ca9fddf4e7ee" == Fake Daeran
						//	"38a1251314b369f4e99572e0c1fd10de" == Fake Ember
						//
						// Fortunately, the clones have the same portraits as the originals, so 
						// if we resolve the portrait, we can still look for a body.json to load
						// and apply. It does however mean implementing a solution that doesn't
						// rely on Game.Instance.Player.PartyCharacters.
						//--------------------------------------------------------------------

						HandleUnitIntrospect(unit);
					}
				} 
				catch (Exception ex)
				{
					Log.debug($"Error inspecting units: {ex.ToString()}");
				}
			}

			UnitEntityData player = Game.Instance.Player.MainCharacter.Value;

			StringWriter sw = new StringWriter();

			sw.WriteLine("-------------------------------------------------------");
			sw.WriteLine($"Area Loaded [{Game.Instance.CurrentlyLoadedArea.name}]");
			sw.WriteLine("-------------------------------------------------------");
			sw.WriteLine($"Difficulty: [{Kingmaker.Settings.SettingsRoot.Difficulty.GameDifficulty.GetValue()}]");
			sw.WriteLine($"Player Name: [{player.CharacterName}]");


			if (Config.AllowPartyInfo)
			{ 
				bool first = true;
				foreach(string charName in MonitoredNPCs.Keys) 
				{
					if (MonitoredNPCs[charName].npc.IsInGame) 
					{ 
						sw.Write(MonitoredNPCs[charName].ToString(first));
						first = false;
					}
				}
				try 
				{
					File.WriteAllText(Path.Combine(DefaultSnapshotsRoot,"Party_Info.log"), sw.ToString());
				} 
				catch (Exception ex) 
				{
					Log.debug($"Error saving Party_Info.log: {ex.ToString()}");
				}
			} 
			else 
			{
				Log.trace("TRACE: Party_Info.json logging disabled.");
            }

			if (Config.AllowEquipmentInfo) 
			{ 
				try 
				{				
					File.WriteAllText(Path.Combine(DefaultSnapshotsRoot,"Equipment_Info.log"), JsonUtil.GetKeyValueMap(EquipmentEntityCache));
				} 
				catch (Exception ex) 
				{
					Log.debug($"Error saving Equipment_Info.log: {ex.ToString()}");
				}
			} 
			else 
			{
				Log.trace("TRACE: Equipment_Info.json logging disabled.");
            }
        }

		public static void HandleUnitIntrospect(UnitEntityData unit)
        {
			string name = getName(unit);
			if (null == name) {
				Log.trace($"TRACE: HandleUnitIntrospect. Unable to determine name of entity. Bailing...");
				return;
			}
			name = ensureValidFileName(unNasty(name),"GetName");
			string portraitName =  unit?.UISettings?.PortraitBlueprint?.name ??  unit?.UISettings?.Owner?.Blueprint?.PortraitSafe?.name;
			portraitName = (null == portraitName) ? $"{name}_Portrait" : portraitName;
			Log.trace($"TRACE: HandleUnitIntrospect. Inspecting Unit [{name}] portrait [{portraitName}]");

			// BAKED:
			//
			// Normal NPCs have a list of body parts/models that need to be rendered
			// in a specific order to create their appearance. However this is expensive.
			// So the game also has BAKED NPCs. These NPCs are 1 single model with no
			// layers, etc... It is more efficient, but you can't change how they look.
			//
			// To save resources, most NPCs in the game are in fact Baked. Only NPCs whos
			// inventory you might change are typically not baked. This includes all
			// travelling companions.
			//
			// So a quick check for the equipmententities tells us if the NPC's appearance
			// can be changed and (typically) is also an indicator that they are a companion
			// option. No point in generating bodies or creating (body support) portrait
			// folders if the NPC has no equipment enties. We may still make a portrait folder
			// if they have a portrait, but that is handled elsewhere.

			if ((null != unit?.View?.CharacterAvatar?.EquipmentEntities) && (unit.View.CharacterAvatar.EquipmentEntities.Count > 0))
			{ 
				if (Config.AllowBodySnapshots)
				{ 
					BodyPartsInfo bodyPartsInfo = getBodyPartsInfoUnSafe(unit);
					if (bodyPartsInfo != null) 
					{
						JsonUtil.SaveBodyPartsInfo(bodyPartsInfo,ensureValidFileName(name,"BodyPartsInfo"),DefaultSnapshotsRoot);
					}
				}
				if (Config.CreateIfMissing) { 
					ensurePathExists(Path.Combine(CompanionPortraitsRoot,portraitName),false);
				}
				if (null == unit.Get<UnitPartCompanion>())
				{
					if (Config.AllowBodies) {
						// Not a companion/party member... Or at least not being reported as one.
						// Typically this branch gets hit with companion clones 
						string bodyPath = Path.Combine(CompanionPortraitsRoot,portraitName,"body.json");
						if (File.Exists(bodyPath)) {
							BodyPartsInfo bodyPartsInfo = JsonUtil.LoadBodyPartsInfo(bodyPath);
							if (null != bodyPartsInfo) {
								// Non-Companions: never load armor/weapons (third param). If someone wants 
								// those things, they should be included in the body.json...
								Log.trace($"TRACE: Attempting to apply body to [{name}]");
								NPCMonitor.applyBody(unit, NPCMonitor.computeInternal(bodyPartsInfo), false, false);
							}
						}
					}
				} 
				else if (!unit.IsMainCharacter && (null == unit.Master))
                {
                    // I can see dead people... but I shouldn't.
					if ((null != unit.Descriptor?.State) && !unit.Descriptor.State.IsFinallyDead)
					{ 
						if (!MonitoredNPCs.ContainsKey(name))
						{
							Log.debug($"Creating Monitor for [{name}]");

							// We do this here because purchased mercenary party members are not returned
							// by Game.Instance.Player.PartyCharacters, even when travelling with the player.
							// So to pick them up as party members, we need to rely on the cast above.
							ensureMonitor(unit);
						}
						else
                        {
							Log.debug($"MonitoredNPCs already contains NPC [{name}]");
                        }
					}
					else
                    {
						Log.debug($"NPC [{name}] has no State.");
                    }
				}
            }
        }

		public static void OnCompanionLevelUp(UnitEntityData unitEntityData, bool isChargen)
        {
			try
			{ 
				if (!isChargen) return;
				if (unitEntityData == null) return;
				if (unitEntityData.Descriptor == null) return;
				string name = unitEntityData.Descriptor.CharacterName;
				if (name == null) return;
				Log.trace($"TRACE: OnCompanionLevelUp Called for [{name}]");
				if (MonitoredNPCs.ContainsKey(name)) { 
					MonitoredNPCs[name].OnCompanionLevelUp();
				} else {
					string allMonitored = string.Join(",", MonitoredNPCs.Select(npc => $"{npc.Key}"));
					Log.trace($"TRACE: [{name}] Not currently in monitored list [{allMonitored}]");
				}
			}
			catch (Exception ex) 
			{
				Log.always($"OnCompanionLevelUp: Exception Caught : {ex.ToString()}");
			}
        }
		

		// Relayed from OnPartyChangeHandler.cs
		public static void OnCompanionAdded(UnitEntityData unitEntityData)
        {
			try
			{ 
				if (unitEntityData == null) return;
				if (unitEntityData.Descriptor == null) return;
				string name = unitEntityData.Descriptor.CharacterName;
				if (name == null) return;
				Log.trace($"TRACE: OnCompanionAdded Called for [{name}]");
				ensureMonitor(unitEntityData);
				if (MonitoredNPCs.ContainsKey(name)) {
					MonitoredNPCs[name].OnCompanionAdded();
				} else {
					string allMonitored = string.Join(",", MonitoredNPCs.Select(npc => $"{npc.Key}"));
					Log.trace($"TRACE: [{name}] Not currently in monitored list [{allMonitored}]");
				}
			} 
			catch (Exception ex) 
			{
				Log.always($"OnCompanionAdded: Exception Caught : {ex.ToString()}");
			}
		}

		// Relayed from OnPartyChangeHandler.cs
		public static void OnCompanionActivated(UnitEntityData unitEntityData)
		{
			try
			{ 
				if (unitEntityData == null) return;
				if (unitEntityData.Descriptor == null) return;
				string name = unitEntityData.Descriptor.CharacterName;
				if (name == null) return;
				Log.trace($"TRACE: OnCompanionActivated Called for [{name}]");
				ensureMonitor(unitEntityData);
				if (MonitoredNPCs.ContainsKey(name)) {
					MonitoredNPCs[name].OnCompanionActivated();
				} else {
					string allMonitored = string.Join(",", MonitoredNPCs.Select(npc => $"{npc.Key}"));
					Log.trace($"TRACE: [{name}] Not currently in monitored list [{allMonitored}]");
				}
			}
			catch (Exception ex) 
			{
				Log.always($"OnCompanionActivated: Exception Caught : {ex.ToString()}");
			}
		}

		// Relayed from OnPartyChangeHandler.cs
		public static void OnCompanionRemoved(UnitEntityData unitEntityData, bool stayInGame)
		{
			try
			{ 
				if (unitEntityData == null) return;
				if (unitEntityData.Descriptor == null) return;
				string name = unitEntityData.Descriptor.CharacterName;
				if (name == null) return;
				Log.trace($"TRACE: OnCompanionRemoved Called for [{name}]");
				ensureMonitor(unitEntityData);
				if (MonitoredNPCs.ContainsKey(name)) {
					MonitoredNPCs[name].OnCompanionRemoved(stayInGame);
				} else {
					string allMonitored = string.Join(",", MonitoredNPCs.Select(npc => $"{npc.Key}"));
					Log.trace($"TRACE: [{name}] Not currently in monitored list [{allMonitored}]");
				}
			}
			catch (Exception ex) 
			{
				Log.always($"OnCompanionRemoved: Exception Caught : {ex.ToString()}");
			}
		}

		// Relayed from OnAbilityEffectAppliedHandler (Only when debug is active)
		public static void OnApplySpellAttempt(AbilityExecutionContext context, TargetWrapper targetWrapper)
        {
			try
			{ 
				if (!Config.AllowBodySnapshots) return;
				if (null==context||null==targetWrapper||null==context.MaybeCaster||null==context.Ability||!targetWrapper.IsUnit) return;
				UnitEntityData player = Game.Instance.Player.MainCharacter.Value;
				UnitEntityData caster = context.MaybeCaster;
				UnitEntityData target = targetWrapper.Unit;
				if (player != caster && player != target) return;
				if ("Inflict Light Wounds" != context.Ability.Name) return;
				string casterName = (null != caster.Descriptor) ? caster.Descriptor.CharacterName : "Unknown";
				string targetName = (null != target.Descriptor) ? target.Descriptor.CharacterName : "Unknown";
				Log.debug($"DEBUG: Inflict Light Wounds Detected. Caster [{casterName}] Target [{targetName}]");
				BodyPartsInfo bodyPartsInfo = getBodyPartsInfo(target);
				if (bodyPartsInfo == null) return;
				JsonUtil.SaveBodyPartsInfo(bodyPartsInfo,targetName,DefaultSnapshotsRoot);
			}
			catch (Exception ex) 
			{
				Log.always($"OnApplySpellAttempt: Exception Caught : {ex.ToString()}");
			}
        }

		public static void OnPolymorphEnd(UnitEntityData unitEntityData, Polymorph polymorph)
        {
			try
			{ 
				if (null==unitEntityData) return;
				string name = unitEntityData.Descriptor.CharacterName;
				if (null == name) return;
				if (!MonitoredNPCs.ContainsKey(name)) {
					string allMonitored = string.Join(",", MonitoredNPCs.Select(npc => $"{npc.Key}"));
					Log.trace($"TRACE: OnPolymoreEnd Called for [{name}], but NPC not currently in monitored list [{allMonitored}]");
					return;
				}
				string spellName = polymorph.name;
				Log.trace($"TRACE: OnPolymorphEnd Called for [{name}] ({spellName})");
				MonitoredNPCs[name].OnPolymorphEnd(polymorph);
			}
			catch (Exception ex) 
			{
				Log.always($"OnPolymorphEnd: Exception Caught : {ex.ToString()}");
			}
        }

		public static void OnDialogStarted(BlueprintDialog dialogMeta)
        {
			try
			{ 
				Log.trace($"TRACE: OnDialogStarted");
				if (Config.AllowDialogueInfo)
				{ 
					string dname = (null == dialogMeta) ? "???" : $"{dialogMeta}";
					string dtype = (null == dialogMeta) ? "???" : $"{dialogMeta.Type}";
					bool first = true;
					StringWriter sw = new StringWriter();
					sw.WriteLine("\n------------------------------------------------------------------------------");
					sw.Write($"Dialog Detected [{dname}] Type [{dtype}]. Involved: [");
					foreach(UnitEntityData unitEntityData in Game.Instance.DialogController.InvolvedUnits)
					{
						if (first) {
							first = false;
							sw.Write(unitEntityData.Descriptor.CharacterName);
						} else {
							sw.Write($",{unitEntityData.Descriptor.CharacterName}");
						}
					}
					sw.WriteLine("]");
					sw.WriteLine($"(\"prop\":\"Dialog\", \"cond\":\"any\", \"value\":\"{dname}\")");
					try {
						File.AppendAllText(Path.Combine(DefaultSnapshotsRoot,"Dialog_Info.log"), sw.ToString());
					} catch (Exception ex) {
						Log.always($"Error saving Party_Info.log: {ex.ToString()}");
					}
				}
				foreach(string charName in MonitoredNPCs.Keys) {
					MonitoredNPCs[charName].OnDialogStart(dialogMeta);
				}
			}
			catch (Exception ex) 
			{
				Log.always($"OnDialogStarted: Exception Caught : {ex.ToString()}");
			}
		}

		public static void OnDialogFinished(BlueprintDialog dialogMeta, bool finishedWithoutCanceling) // IDialogFinishHandler
		{
			try
			{ 
				Log.trace($"TRACE: OnDialogFinished");
				foreach(string charName in MonitoredNPCs.Keys) {
					MonitoredNPCs[charName].OnDialogEnd(dialogMeta, finishedWithoutCanceling);
				}
			}
			catch (Exception ex) 
			{
				Log.always($"OnDialogFinished: Exception Caught : {ex.ToString()}");
			}
		}

		public static BodyPartsInfo getBodyPartsInfo(UnitEntityData unitEntityData)
        {
			UnitEntityView unitEntityView = unitEntityData.View;
			if (null == unitEntityView) {
				Log.trace($"TRACE: [{unitEntityData.Descriptor.CharacterName}] : No UnitEntityView. Bailing...");
				return null;
			}
			Character character = unitEntityView.CharacterAvatar;
			if (null == character) {
				Log.trace($"TRACE: [{unitEntityData.Descriptor.CharacterName}] : No CharacterAvatar. Bailing...");
				return null;
			}
			if (null == character.EquipmentEntities) {
				Log.trace($"TRACE: [{unitEntityData.Descriptor.CharacterName}] : EquipmentEntities is Null. Bailing...");
				return null;
			}
			if (0 == character.EquipmentEntities.Count) {
				Log.trace($"TRACE: [{unitEntityData.Descriptor.CharacterName}] EquipmentEntities is Empty. Bailing...");
				return null;
			}
			return getBodyPartsInfoUnSafe(unitEntityData);
		}

		public static BodyPartsInfo getBodyPartsInfoUnSafe(UnitEntityData unitEntityData)
		{ 		

			// Unsafe assumes CharacterAvatar non-null check and character.EquipmentEntities.Count check already happened
			Character character = unitEntityData.View.CharacterAvatar;
			List<BodyPartsInfo.BodyPart> bodyPartList = new List<BodyPartsInfo.BodyPart>();
			foreach (EquipmentEntity ee in character.EquipmentEntities)
            {
				int primaryColorIndex = character.GetPrimaryRampIndex(ee);
				int secondaryColorIndex = character.GetSecondaryRampIndex(ee);
				string ename = ee.name;
				string assetId   = "UNKNOWN";
				if (!EquipmentEntityCache.ContainsKey(ename)) {
					Log.trace($"TRACE:Cache miss looking up EquipmentEntity [{ename}]. Refreshing...");
					updateEquipmentEntityCache(ename);
				}
				if (EquipmentEntityCache.ContainsKey(ename)) {
					assetId = EquipmentEntityCache[ee.name];					
				} 
				else
                {
					Log.trace($"TRACE: Unable to resolve AssetID for EquipmentEntity [{ename}].");
                }
				bodyPartList.Add(new BodyPartsInfo.BodyPart(ee.name, assetId, primaryColorIndex, secondaryColorIndex));
			}

			BodyPartsInfo bodyPartsInfo = new BodyPartsInfo();
			bodyPartsInfo.gender = $"{unitEntityData.Gender}";
			bodyPartsInfo.raceName = "UNKNOWN"; 

			if (null!=unitEntityData?.Descriptor?.Progression?.VisibleRace) {
				bodyPartsInfo.raceName = (unitEntityData.Descriptor.Progression.VisibleRace).name;
            }
			bodyPartsInfo.standardAppearance = new BodyPartsInfo.BodyPart[bodyPartList.Count];
			int i = 0;
			foreach(BodyPartsInfo.BodyPart bodyPart in bodyPartList) {
				bodyPartsInfo.standardAppearance[i++] = bodyPart;
			}
			return bodyPartsInfo;
        }

		public static void updateEquipmentEntityCache(string earlyBail) {

			// ResourcesLibrary manages in-memory instances of resources. It is basically a cache and you
            // can query it based on type of object. However not all objects are in memory/cache, so
            // you are only quering what is visible in game. Also, this approach doesn't tell you
            // much about the queried objects. Like what skeletons it is compatible with. (Race + gender
            // typically dictate the skeleton and what visual model components are compatible with it).
            //
            // In many (most?) cases, naming conventions allow you to decipher the compatible race and gender
            // from the ee name. IE, most EEs (EquipmentEntity)s end with _F_HM or _M_HE  ie: _<GENDER>_<RACECODE>
			//
            // It is a safe bet that anyone you might cast a spell on is loaded in memory, so this is
            // sufficient for our needs. But if the goal is/was generating a reference that includes
            // Objects not visible on the screen, you are likely to miss a lot relying on the
            // ResourcesLibrary. 

			IEnumerator<string> assetIdEnumerator = ResourcesLibrary.GetLoadedAssetIdsOfType<EquipmentEntity>().GetEnumerator();
			IEnumerator<EquipmentEntity> eeEnumerator = ResourcesLibrary.GetLoadedResourcesOfType<EquipmentEntity>().GetEnumerator();
			while (assetIdEnumerator.MoveNext() && eeEnumerator.MoveNext()) {
				string assetId = assetIdEnumerator.Current;
				EquipmentEntity ee = eeEnumerator.Current;
				EquipmentEntityCache[ee.name] = assetId;
            }

			if ((null != earlyBail) && (EquipmentEntityCache.ContainsKey(earlyBail))) return;

			// Here we add the standard stuff... 
			BlueprintCharacterClassReference[] allClasses = BlueprintRoot.Instance.Progression.CharacterClasses;
			BlueprintRaceReference[] allRaces = BlueprintRoot.Instance.Progression.CharacterRaces;
			Gender[] allGenders = new Gender[2] { Gender.Male, Gender.Female };
			foreach(Gender gender in allGenders ) { 
				foreach(BlueprintRaceReference blueprintRaceReference in allRaces ) {
					BlueprintRace bRace = blueprintRaceReference.Get();
					if (null == bRace) continue;
					CustomizationOptions genderOptions = (gender == Gender.Female) ? bRace.FemaleOptions : bRace.MaleOptions;
					foreach(EquipmentEntityLink eel in genderOptions.Heads) {
						EquipmentEntity ee = eel.Load(false,false);
						if (null != ee) EquipmentEntityCache[ee.name] = eel.AssetId;
					}
					foreach(EquipmentEntityLink eel in genderOptions.Eyebrows) {
						EquipmentEntity ee = eel.Load(false,false);
						if (null != ee) EquipmentEntityCache[ee.name] = eel.AssetId;
					}
					foreach(EquipmentEntityLink eel in genderOptions.Hair) {
						EquipmentEntity ee = eel.Load(false,false);
						if (null != ee) EquipmentEntityCache[ee.name] = eel.AssetId;
					}
					foreach(EquipmentEntityLink eel in genderOptions.Beards) {
						EquipmentEntity ee = eel.Load(false,false);
						if (null != ee) EquipmentEntityCache[ee.name] = eel.AssetId;
					}
					foreach(EquipmentEntityLink eel in genderOptions.Horns) {
						EquipmentEntity ee = eel.Load(false,false);
						if (null != ee) EquipmentEntityCache[ee.name] = eel.AssetId;
					}
					foreach(EquipmentEntityLink eel in genderOptions.TailSkinColors) {
						EquipmentEntity ee = eel.Load(false,false);
						if (null != ee) EquipmentEntityCache[ee.name] = eel.AssetId;
					}
					foreach(BlueprintRaceVisualPresetReference presetReference in (BlueprintRaceVisualPresetReference[]) bRace.Presets) {
						BlueprintRaceVisualPreset preset = presetReference.Get();
						if (null == preset) continue;
						KingmakerEquipmentEntity skin = preset.Skin;
						EquipmentEntityLink[] skinEELs = skin.GetLinks(gender,bRace.RaceId);
						foreach (EquipmentEntityLink eel in skinEELs) {
							EquipmentEntity ee = eel.Load(true, false);
							if (null != ee) EquipmentEntityCache[ee.name] = eel.AssetId;
						}
					}
					foreach(BlueprintCharacterClassReference blueprintCharacterClassReference in allClasses) {
						BlueprintCharacterClass bClass = blueprintCharacterClassReference.Get();
						if (null == bClass) continue;
						List<EquipmentEntityLink> eels = bClass.GetClothesLinks(gender,bRace.RaceId);
						foreach(EquipmentEntityLink eel in eels) {
							EquipmentEntity ee = eel.Load(false,false);
							if (null != ee) EquipmentEntityCache[ee.name] = eel.AssetId;
						}
					}
					foreach(EquipmentEntityLink eel in BlueprintRoot.Instance.CharGen.WarpaintsForCustomization) {
						EquipmentEntity ee = eel.Load(false,false);
						if (null != ee) EquipmentEntityCache[ee.name] = eel.AssetId;
					}
					foreach(EquipmentEntityLink eel in BlueprintRoot.Instance.CharGen.ScarsForCustomization) {
						EquipmentEntity ee = eel.Load(false,false);
						if (null != ee) EquipmentEntityCache[ee.name] = eel.AssetId;
					}
					foreach(EquipmentEntityLink eel in BlueprintRoot.Instance.CharGen.MaleClothes) {
						EquipmentEntity ee = eel.Load(false,false);
						if (null != ee) EquipmentEntityCache[ee.name] = eel.AssetId;
					}
					foreach(EquipmentEntityLink eel in BlueprintRoot.Instance.CharGen.FemaleClothes) {
						EquipmentEntity ee = eel.Load(false,false);
						if (null != ee) EquipmentEntityCache[ee.name] = eel.AssetId;
					}					
				}
			}
        }

		public static string getName(UnitEntityData unit, string defaultValue = null)
        {

			// NOTES:
            //
            // unit.Descriptor.CharacterName and unit.Blueprint.CharacterName
            // and unit.name can all differ.
            //
            // unit.Descriptor.CharacterName : This is the in-game display of 
			//   the NPC. In the prologue there are dozens of Citizen's and Nobles
			//   standing around. Only a few NPCs have unique names. This is 
			//   preferred for travelling companions, but not preferred or even
			//   useful for non-companions. 
			//
			// unit.Blueprint.CharacterName : With PREFAB NPCs, this value 
			//   typically matches the unit.Descriptor.CharacterName. The 
			//   runtime Descriptor above pulls its value from this field. 
			//   However, if the player purchases and customized a 
			//   mercenary companion, the CharacterName field is 
			//   "Player Character", reflecting the original NPC instance
			//   that was cloned to make the runtime NPC. Meanwhile, the
			//   Descriptor.CharacterName will reflect the name that the
			//   player gave to the NPC. This mismatch MAY be useful for
			//   detecting mercenary/purchased companions, but in general
            //   this value is useless as it is either replicated by
            //   Descriptor.CharacterName or inconsistent and misleading.
			//
			// unit.name : This value more uniquely reflects the NPC from
            //   a resource bundle perspective. While "Noble" may be shared
			//   by 5 different NPCs in the prologue, this value will reflect
			//   the actual flavor of noble. "female_noble_1", "female_noble_2".
			//
			//   The downside of this value is that sometimes it gets big and
			//   nasty when a flavor has been customized for a particular
            //   area or chapter. Like
            //
            //      "123456789abcdef123456789abcedf12_BTC_SOME_DUDE"
			//
			// Logic: Most generic "furniture" type NPCs don't have portraits, 
			//        while the games main cast tends to have portraits. So 
			//        we use the presence of the portrait to decide which 
			//        name to use. 

			// Can technically return null if unit.Blueprint.name and unit.Descriptor.CharacterName are null, but that
            // would be unexpected. The blueprint.name almost always has a value, even if it is cryptic.

			string name = (null != unit?.UISettings?.Owner?.Blueprint?.PortraitSafe) ? (unit?.Descriptor?.CharacterName ?? null) : null;
			return ((null == name) ? (((BlueprintScriptableObject)unit.Blueprint)?.name ?? defaultValue) : name);

//			BlueprintPortrait bPortrait = unit?.UISettings?.PortraitBlueprint ?? unit?.UISettings?.Owner?.Blueprint?.PortraitSafe;
//			string name = (null != (bPortrait?.name ?? bPortrait?.ToString())) ? (unit?.Descriptor?.CharacterName ?? null) : null;
//			return ((null == name) ? (((BlueprintScriptableObject)unit.Blueprint)?.name ?? defaultValue) : name);
        }

		public static string unNasty(string name) {

			//   "Nasty" names have a 32 hex ID followed by an underscore. 
			//    Example:
			//
			//      "123456789abcdef123456789abcedf12_BTC_SOME_DUDE"

			//   Not a perfect check, but check if size > 32 characters and
			//   the 32nd character is an "_" and if so, look for up to 2
			//   words at the end separated by underscores. Given the 
			//   string above, this would return "SOME_DUDE"

			if (name.Length > 32 && name.ElementAt(32) == '_')
            {
				int i = name.Length;
				i = name.LastIndexOf("_", (i-1), (i-32));
				if (i > 32) i = name.LastIndexOf("_", (i-1), (i-32));
				i = (i < 33) ? (32 + 1) : i+1;
				return name.Substring(i);
			}
			return name;
        }

		public static void ensureMonitor(UnitEntityData unitEntityData)
        {
			BlueprintUnit blueprintUnit = unitEntityData.Blueprint;
			UnitPartCompanion unitPartCompanion = unitEntityData.Get<UnitPartCompanion>();
			if (null == unitPartCompanion || unitEntityData.IsMainCharacter) {
				return;
            }


			string preFilterName = getName(unitEntityData);
			if (null == preFilterName) {
				Log.debug("Unable to determine name of UnitEntityData. Bailing....");
				return;
			}
			string name = ensureValidFileName(unNasty(preFilterName),"GetPreFilterName");

			if (0 == name.Length) {
				Log.debug($"Name [{preFilterName}] reduced to empty string during filter. Bailing...");
				return;
            }
				
			if (MonitoredNPCs.ContainsKey(name)) return;

			Log.trace($"TRACE: Allocating NPCMonitor for [{name}] PortraitsRoot [{PortraitsRoot}] SubDirectory [{Config.SubDirectory}] Debug [{Config.LogDebug}] Trace [{Config.LogTrace}]");
			MonitoredNPCs.Add(name, 
				new NPCMonitor(unitEntityData, 
					CompanionPortraitEnablerMain.Modification, 
					CompanionPortraitEnablerMain.PortraitsRoot,
					CompanionPortraitEnablerMain.Config)
			);
		}

		public static void OnSave(OnSaveLoadRelayHandler.SaveState state)
		{
			Log.trace($"TRACE: OnSave - SaveState [{state}]");
			try
			{ 
				if (state == OnSaveLoadRelayHandler.SaveState.SaveMenuDisplayed)
                {
					if (MonitoredNPCs.Count > 0)
					{ 
						Log.always("OnSave - Creating Developer Snapshots");

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

						CreateDeveloperSnapshots();
						return;
					}
                }
				Log.trace($"TRACE: OnSave - Ignoring Event...");
			}
			catch (Exception ex) 
			{
				Log.always($"OnSave: Exception Caught state [{state}] : {ex.ToString()}");
			}
		}

		public static void OnLoad(OnSaveLoadRelayHandler.LoadState state)
		{
			Log.trace($"TRACE: OnLoad - LoadState [{state}]");
			try
			{ 
				if (state == OnSaveLoadRelayHandler.LoadState.LoadSingle)
                {
					if (MonitoredNPCs.Count > 0)
					{ 
						// LoadSingle means there is no lingering state to worry about
						// So we are safe to resetState immediatly.
						Log.always("OnLoad - Calling resetState");
        				CompanionPortraitEnablerMain.resetState();
						return;
					}
                }
				if (state == OnSaveLoadRelayHandler.LoadState.LoadMenuLoading)
                {
					if (MonitoredNPCs.Count > 0)
					{ 
						// They may back out, but to be safe, we set a flag
                        // to reset state on the next OnAreaLoaded Event. If
						// they follow through, the loaded game will reset 
						// when that event fires. If they back out, the current
						// game will reset when they change areas, but it won't
						// be noticable to the player.... 

						Log.always("OnLoad - Setting Reset Delay Flag");
						CompanionPortraitEnablerMain.ResetFlag = true;
						return;
					}
                }
				Log.trace($"TRACE: OnLoad - Ignoring Event...");
			}
			catch (Exception ex) 
			{
				Log.always($"OnLoad: Exception Caught : {ex.ToString()}");
			}
		}

		// Relayed by : OnWeatherChangedRelayHandler
		public static void OnWeatherChanged()
		{
			try
			{ 
				Log.debug("OnWeatherChanged()");
			}
			catch (Exception ex) 
			{
				Log.always($"OnWeatherChanged: Exception Caught : {ex.ToString()}");
			}
		}

        private static void OnGUI()
        {
			try
			{ 
				// Todo: Provice in-game ability to customize?
				GUILayout.Label("NPC Custom Portrait Enabler is On!");
				GUILayout.Button("OK");
			}
			catch (Exception ex) 
			{
				Log.always($"OnGUI: Exception Caught : {ex.ToString()}");
			}
        }
        
        private static void createShortCuts(string PersistentDataPath, string WrathDataPath, string CompanionPortraitsRoot)
        {
			try
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
				ensureShortCut(npcPortraitsLinkPath, CompanionPortraitsRoot, "NPCPortraits");
			}
			catch (Exception ex) 
			{
				Log.always($"createShortCuts: Exception Caught : {ex.ToString()}");
			}
		}

		private static bool verifyPathExists(string dirPath)
        {
			if (Directory.Exists(dirPath))
			{
				Log.debug($"Confirmed Path [{dirPath}] exists");
				return true;
			}
            Log.always($"Path [{dirPath}] not found. Aborting Initialization");
            Config.Disabled = true;
            IsEnabled = false;
            return false;
		}

		private static bool ensurePathExists(string dirPath, bool disableOnFailure=true)
        {
			if (Directory.Exists(dirPath))
			{
				Log.trace($"TRACE: Confirmed Path [{dirPath}]");
				return true;
			}
			Log.trace($"Path [{dirPath}] not found. Attempting Creation");
			string errorMsg = "Unknown Reason. Aborting initialization.";
			try
			{
				Directory.CreateDirectory(dirPath);
				if (Directory.Exists(dirPath))
				{
					Log.debug($"Path [{dirPath}] created");
					return true;
				}
			}
			catch (Exception objException)
			{
				errorMsg = objException.Message;
			}

			if (!disableOnFailure) { 
				Log.debug($"Unable to create folder [{dirPath}]: {errorMsg}");
				return false;
			}
			
			Log.always($"Unable to create folder [{dirPath}]: {errorMsg}");
			Config.Disabled = true;
			IsEnabled = false;
			return false;
		}

		private static void logStartupReport()
        {
			try
			{ 
				StringWriter sw = new StringWriter();
				sw.WriteLine("");
				sw.WriteLine("-------------------------------------------------------");
				sw.WriteLine("Config");
				sw.WriteLine("-------------------------------------------------------");
				sw.WriteLine($"PortraitHome: [{Config.SubDirectory}]");
				sw.WriteLine($"CreateMissingFolders: [{Config.CreateIfMissing}]");
				sw.WriteLine($"AllowPortraits: [{Config.AllowPortraits}]");
				sw.WriteLine($"AllowPortraitRules: [{Config.AllowPortraitRules}]");
				sw.WriteLine($"AllowBodies: [{Config.AllowBodies}]");
				sw.WriteLine($"AvoidNudity: [{Config.AvoidNudity}]");
				sw.WriteLine($"AutoScale: [{Config.AutoScale}]");
				sw.WriteLine($"AllowBodyRules: [{Config.AllowBodyRules}]");
				sw.WriteLine($"SnapshotHome: [{Config.SnapshotHome}]");
				sw.WriteLine($"AllowPartyInfo: [{Config.AllowPartyInfo}]");
				sw.WriteLine($"AllowDialogueInfo: [{Config.AllowDialogueInfo}]");
				sw.WriteLine($"AllowEquipmentInfo: [{Config.AllowEquipmentInfo}]");
				sw.WriteLine($"AllowBodySnapshots: [{Config.AllowBodySnapshots}]");
				sw.WriteLine($"LogDebug: [{Config.LogDebug}]");
				sw.WriteLine($"LogTrace: [{Config.LogTrace}]");
				sw.WriteLine($"AllowShortcutCreation: [{Config.AllowShortcutCreation}]");
				sw.WriteLine($"UninstallMode: [{Config.UninstallMode}]");
				sw.WriteLine($"Disabled: [{Config.Disabled}]");	
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
				Log.always(sw.ToString());
			}
			catch (Exception ex) 
			{
				Log.always($"logStartupReport: Exception Caught : {ex.ToString()}");
			}
		}

        private static bool ensureShortCut(string linkPath, string targetPath, string description)
        {
            if (File.Exists(linkPath))
            {
                Log.debug($"Confirmed Ease of Access Shortcut [{linkPath}]");
                return true;
            }
            Log.always($"Attempting to creating ease of acccess shortcut: [{linkPath}]");
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
                    Log.always($"Ease of Access Shortcut [{linkPath}] created.");
                    return true;
                }
            }
            catch (Exception objException)
            {
                errorMsg = objException.Message;
            }
            Log.always($"Unable to create ease of access shortcut [{linkPath}] : [{errorMsg}]");
			return false;
        }


		// It makes sense to have a safezone state flag so portraits/outfits can change 
		// in safer areas. Just not there yet...

		/*
		public static void OnEnterSafeZone()
		{
			// Good time to kick off side quests or romance conversations that we don't want interrupted.
			Log.debug("OnEnterSafeZone()");
		}
		*/

	}
}