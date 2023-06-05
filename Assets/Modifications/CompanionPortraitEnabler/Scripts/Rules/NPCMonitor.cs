using System;
// using System.Threading;                // Interlocked.Exchange, Interlocked.Increment
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

using Kingmaker.Blueprints.Classes;    // BlueprintRace
using Kingmaker.Blueprints;            // Required for Race
using Kingmaker.PubSubSystem;          // Various I* Event Callback Interfaces

using Kingmaker.Controllers.Rest;      // Required for RestStatus
using Kingmaker.EntitySystem.Entities; // Required for UnitEntityData
using Kingmaker.Blueprints.Items.Equipment; // Required for BlueprintItem***...
using Kingmaker.EntitySystem.Stats;    // Required for StatType
using Kingmaker.EntitySystem;          // Required for EntityFact
using Kingmaker.UnitLogic.Parts;       // Required for UnitPartCompanion
using Kingmaker.UnitLogic.Abilities.Blueprints; // Required for BlueprintAbility
using Kingmaker.UnitLogic.Class.LevelUp; // Required for LevelUpState
using Kingmaker.Blueprints.Root;         // Required for BlueprintRoot
using Kingmaker.Items.Slots;             // Required for ArmorSlot/HandSlot/EquipmentSlot
using Kingmaker.Items;                   // Required for IInventoryChangedUIHandler (ItemEntity)
                                         // and IItemsCollectionHandler (ItemsCollection)
using Kingmaker.Visual.CharacterSystem;  // Required for Character (Body avatar)
using Kingmaker.Blueprints.Facts;        // Required for BlueprintUnitFact

using Kingmaker;                         // Required for Game
using Kingmaker.ResourceLinks;           // Required for WeakResourceLink, SpriteLink
using UnityEngine;                       // Required for Sprite
using Kingmaker.RuleSystem.Rules.Damage; // Required for RuleDealDamage
using Kingmaker.DialogSystem.Blueprints; // Required for BlueprintDialogue
using Kingmaker.AreaLogic.Cutscenes;     // Required for CutsceneControlledUnit (to detect if npc is in dialogue with player...)
using Kingmaker.AreaLogic.QuestSystem;   // Required for IQuestHandler (Quest)
using Kingmaker.UnitLogic.ActivatableAbilities; // Required for BlueprintActivatableAbility, ActivatableAbility, ActivatableAbilityCollection

using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;

using Kingmaker.ResourceManagement;      // Needed to get/create EquipmentEntityLinks using BundledResourceHandle
using Kingmaker.UnitLogic.Buffs;         // Polymorph
using Kingmaker.Blueprints.Items;        // Require for BlueprintItem

using UniRx; // MainThread.Schedule
using OwlcatModification.Modifications.CompanionPortraitEnabler.Utility;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules
{
    //#################################################################################
	// Basic Components of Rule Engine:
	// --------------------------------------------------------------------------------
	// 1) RuleContext :  A snapshot of the objects/environment that a rule can be
    //                   evaluated against. Exposes the nouns that a rule can be about.
	//
	//                  Example: Snapshot of current NPC.XP and inventory
	//
	// 2) Rule :         List of conditions against values that act as trigger points 
    //                   for the nouns exposed by the Rule Context. 
	//
	//                   Example: if (NPC.XP > 1234) && (inventory.contains("kitkat"))
	//
	// 3) RuleEvaluator: A streamlined/compiled component that takes a rule and a
	//                   rule context and quickly evaluates if the rule is true against
	//                   the given context.
	//
	//                   Example: RuleEvaluator re = new RuleEvaluator(rule)
	//                            if (re.evaluate(ruleContext)) then ...
	// ---------------------------------------------------------------------------------
	// Other Components:
	// ---------------------------------------------------------------------------------
	// 1) RuleFactory: Used typically at startup to deserialize rules and load them 
	//                 from disk. May also act as a cache for rules regarding some 
	//                 domain such as "any rule found in this directory". 
	//
	// 2) EventMonitor: Registers for events in order to keep a RuleContext up-to-date
	//                  and relavent for various tracked objects (NPCs). Typically
    //                  tracks when a rule-conext becomes stale and should be
    //                  re-evaluated again. We call this component NPCMonitor.
	//
	//#################################################################################

	// These are the attributes we track for each NPC. They provide the keys and values
	// that rules can be based on

	public class NPCMonitor : IUnitSubscriber,      // GetSubscribingUnit (To identify subscriber)
		 IPartyGainExperienceHandler,               // HandlePartyGainExperience
		 IRestCampUIHandler,                        // HandleShowResults (Rest/Camp results)
		 IUnitCalculateSkillPointsOnLevelupHandler, // HandleUnitCalculateSkillPointsOnLevelup
		 ICompanionChangeHandler,                   // HandleRecruit, HandleUnrecruit
		 ICorruptionLevelHandler,                   // HandleIncreaseCorruption, HandleClearCorruption
		 IPartyCombatHandler,                       // HandlePartyCombatStateChanged
		 IAttributeDamageHandler,                   // HandleAttributeDamage
		 IUnitGainFactHandler,                      // HandleUnitGainFact
		 IUnitLostFactHandler,                      // HandleUnitLostFact

		 IAlignmentChangeHandler,                   // HandleAlignmentChange
		 IDamageHandler,                            // HandleDamageDealt
		 IHealingHandler,                           // HandleHealing

		 IMythicSelectionCompleteUIHandler,         // HandleMythicSelectionComplete
		 IQuestHandler,                             // HandleQuestStarted, HandleQuestCompleted, HandleQuestFailed
		 IUnitEquipmentHandler,                     // HandleEquipmentSlotUpdated
		 IItemsCollectionHandler,                   // HandleItemsAdded, HandleItemsRemoved
		 IUnitSizeHandler                           // HandleUnitSizeChanged		 
	{
		// static/const
		public static Boolean avoidNudity             = true;
		public static Boolean autoScale               = true;
		public static string portraitsRoot            = null;
		public static string npcSubDir                = null;
		public static CompanionPortraitEnablerMain.ConfigData settings = null;
		public static BlueprintActivatableAbility ChangeShapeKitsuneAToggleBlueprint = null;
		public static BlueprintActivatableAbility ChangeShapeKitsuneBToggleBlueprint = null;

		// I commented the locks out for now. I think the game engine ensures
		// only 1 thread within class code at a time.. . (I'm not seeing
		// contention reports in the logs)

		// private static int[] locks = new int[32];
		// private static int nextLock = 0;
		// public static long EventFloodGate = 1001;

		public static readonly System.TimeSpan D16   = new TimeSpan(320000L);   // 0.016 sec
		public static readonly System.TimeSpan D32   = new TimeSpan(320000L);   // 0.032 sec
		public static readonly System.TimeSpan D64   = new TimeSpan(640000L);   // 0.064 sec
		public static readonly System.TimeSpan D128  = new TimeSpan(1280000L);  // 0.128 sec
		public static readonly System.TimeSpan D256  = new TimeSpan(2560000L);  // 0.256 sec
		public static readonly System.TimeSpan D512  = new TimeSpan(5120000L);  // 0.512 sec
		public static readonly System.TimeSpan D1024 = new TimeSpan(10240000L); // 1.024 sec

		public static readonly System.TimeSpan[] ThrottleGears = new System.TimeSpan[] {D32,D64,D128,D256,D512,D1024 };
		public static readonly int HIGHESTGEAR = 6; // (0 based offset)

		public static readonly System.TimeSpan OneSecond = new TimeSpan(10000000L);
		public static readonly System.TimeSpan AFewMS    = new TimeSpan(150000L);

		// ------------------------------------------------------------------
		// Non-Rule Variables
		// ------------------------------------------------------------------
		public UnitEntityData npc;
		private bool inParty = false;
		public float bodyScale = -1.0f;

		protected List<Rule> allPortraitRules = null;
		protected List<Rule> allBodyRules     = null;
		protected RuleContext ruleContext     = null;

		// ------------------------------------------------------------------
		// Basics
		// ------------------------------------------------------------------
		public string Name                = "";
		public Rule   currentPortraitRule = null;
		public Rule   currentBodyRule     = null; 
		public BodyPartsInfo currentBody  = null;
		public bool  previouslyNaked    = false;
		public bool  previouslyDefault  = false;
		public string resourceHome        = null; // ex: "Ember_Portrait"
		public string portraitPath        = "Default";
		public string portraitFileLarge   = "Default";
		public string portraitFileMedium  = "Default";
		public string portraitFileSmall   = "Default";
		public string defaultBodyPath     = null;
		public BodyPartsInfo defaultBody  = null;
		public bool noBodyFound           = false;
		public int lockId                 = -1;
		bool forceBodyRefresh             = false;
		bool forcePortraitRefresh         = false;
		
		int evfloodProtection = -1;
		int evThrottleGear = 0;

		public static readonly float[] sizeOrdinalToScale = new float[] { 0.10f, 0.25f, 0.50f, 0.75f, 1.0f, 1.5f, 2.0f, 2.5f, 3.0f };

		// ------------------------------------------------------------------
		// Constructor
		// ------------------------------------------------------------------
		public NPCMonitor(UnitEntityData unit, Kingmaker.Modding.OwlcatModification modification, string portraitsRoot, CompanionPortraitEnablerMain.ConfigData config)
		{
			// this.lockId = Interlocked.Increment(ref nextLock);
			// locks[lockId] = 0;

			if (null == NPCMonitor.settings) {
				// NPCMonitor.Modification = modification;
				NPCMonitor.settings = config;
				NPCMonitor.portraitsRoot = portraitsRoot;
				NPCMonitor.npcSubDir = config.SubDirectory;
				NPCMonitor.avoidNudity = config.AvoidNudity;
				NPCMonitor.autoScale = config.AutoScale;
			}
			this.npc = unit;
			this.Name = this.npc.CharacterName;
			this.noBodyFound = !settings.AllowBodies;

			if (null == ChangeShapeKitsuneAToggleBlueprint) {
				BlueprintGuid deserializedGuid = BlueprintGuid.Parse("52bed4c5617625e4faf029b5c750667f");
				if (null != deserializedGuid) { 
					ChangeShapeKitsuneAToggleBlueprint = (BlueprintActivatableAbility)ResourcesLibrary.TryGetBlueprint(deserializedGuid);
					if (null!=ChangeShapeKitsuneAToggleBlueprint) {
						Log.trace($"TRACE: Extracted Flag [{ChangeShapeKitsuneAToggleBlueprint}]");
                    } else {
						Log.trace("TRACE: ResourcesLibrary.TryGetBlueprint (ChangeShapeKitsuneAToggleBlueprint) Failed.");
                    }
				} else {
					Log.trace("TRACE: BlueprintGuid.parse Failed to deserialized ChangeShapeKitsuneAToggleBlueprint.");
                }
			}
			else
            {
				Log.trace("TRACE: ChangeShapeKitsuneAToggleBlueprint is not null. Skipping load.");				
            }
			if (null == ChangeShapeKitsuneBToggleBlueprint) { 
				BlueprintGuid deserializedGuid = BlueprintGuid.Parse("4252c9d9a25549146b8683c5ea45e14e");
				if (null != deserializedGuid) { 
					ChangeShapeKitsuneBToggleBlueprint = (BlueprintActivatableAbility)ResourcesLibrary.TryGetBlueprint(deserializedGuid);
					if (null!=ChangeShapeKitsuneBToggleBlueprint) {
						Log.trace($"TRACE: Extracted Flag [{ChangeShapeKitsuneBToggleBlueprint}]");
                    } else {
						Log.trace("TRACE: ResourcesLibrary.TryGetBlueprint (ChangeShapeKitsuneBToggleBlueprint) Failed.");
                    }
				} else {
					Log.trace("TRACE: BlueprintGuid.parse Failed to deserialized ChangeShapeKitsuneBToggleBlueprint.");
                }
			} 
			else
            {
				Log.trace("TRACE: ChangeShapeKitsuneBToggleBlueprint is not null. Skipping load.");				
            }

			// Log.trace($"TRACE: NPCMonitor [{lockId}] created for [{this.Name}]: npcSubDir [{npcSubDir}]");
			Log.trace($"TRACE: NPCMonitor created for [{this.Name}]: npcSubDir [{npcSubDir}]");

			CheckForMissingPortrait();
			TryLoadPortraitRules();
			TryLoadBodyAndRules();

			this.ruleContext = new RuleContext(this.npc);

			// Don't evaluateRules or subscribe to events immediatly. Wait for an OnAreaLoad or some
			// other external event fire first to ensure game resources are ready... 
		}

		public void CheckForMissingPortrait()
        {
			// Scenario A: Users has been playing with PAK A and installs PAK B. When he loads game,
            // we should detect that the custom paths do not match the subdir specified by the
            // current override and update on load. Do this by resetting portrait back to original
            // value so that when rules load, new paths are applied...

			// Scenario B: User has been playing with PAK A and removes it. Notices all the 
			// portraits in his party are now empty faces in the Load menu. When he loads game,
            // we should detect that the paths now point to non-existant folders and reset
            // back to original.

			if (null == npc?.UISettings?.CustomPortraitRaw?.CustomId) return;
			string oldSubdir = isolateSubDir(npc.UISettings.CustomPortraitRaw.CustomId);
			string newSubdir = isolateSubDir(NPCMonitor.npcSubDir);
			if ((oldSubdir == newSubdir) && (File.Exists(Path.Combine(NPCMonitor.portraitsRoot,npc.UISettings.CustomPortraitRaw.CustomId,"small.png")))) return;
			Log.trace($"TRACE: Old subdir [{oldSubdir}] new subdir [{NPCMonitor.npcSubDir}]. Attempting to correct missing/changed portrait for [{this.Name}]. ");
			this.forcePortraitRefresh = true;
			BlueprintPortrait bPortrait = npc?.UISettings?.Owner?.Blueprint?.PortraitSafe;
			if (null != bPortrait) { 
				this.resourceHome = bPortrait.name;
				enforceDefaultPortraitRule();
			}
        }

		public static string isolateSubDir(string input) {
			int start = input.StartsWith(""+System.IO.Path.DirectorySeparatorChar) ? 1 : 0;
			int end = input.IndexOf(System.IO.Path.DirectorySeparatorChar, start, (input.Length - start));
			return (end < 1) ? input.Substring(start) : input.Substring(start,(end-start));
		}

		public void TryLoadPortraitRules()
        {
			// Do NOT call evaluateRules from this execution branch or it may 
			// cause infinite loop
			Log.trace($"TRACE: TryLoadPortraitRules called for [{this.Name}]");
			this.resourceHome = extractResourceHome(this.npc, this.resourceHome);
			Log.trace($"TRACE: ResourceHome [{this.resourceHome}]");
			if (this.resourceHome == null)  return;

			string resourceRoot = Path.Combine(NPCMonitor.portraitsRoot,NPCMonitor.npcSubDir, this.resourceHome);
			Log.trace($"TRACE: resourceRoot [{resourceRoot}]");

			if (!settings.AllowPortraits) 
			{ 
				Log.debug($"[{resourceRoot}] AllowPortraits Disabled...");
				return;
			}
			if (!settings.AllowPortraitRules) 
			{ 
				Log.debug($"[{resourceRoot}] AllowPortraitRules Disabled...");
				return;
			}

			if (!File.Exists(Path.Combine(resourceRoot, "fulllength.png")) ||
				!File.Exists(Path.Combine(resourceRoot, "medium.png"))     ||
				!File.Exists(Path.Combine(resourceRoot, "small.png")))
			{
				Log.debug($"[{resourceRoot}] No Images");							
				return;
			}
			this.allPortraitRules = RuleFactory.findRules(NPCMonitor.portraitsRoot,NPCMonitor.npcSubDir,this.resourceHome,"portrait",true);
			if (this.allPortraitRules != null)
            {
				Log.trace($"TRACE: [{this.allPortraitRules.Count}] portrait rules found");
            }
			else
            {
				Log.trace($"TRACE: No portrait rules found (allPortraitRules is null)");
            }
		}

		// Unlike the version in CompanionPortraitEnablerMain, this verison caches the body
		// so that we can do more stuff with the computeInternal method. 
		public BodyPartsInfo GetBodyPartsInfo()
		{
			if (noBodyFound) { 
				return null;
			}
			if (this.currentBodyRule != null)
            {
				if (this.currentBody != null)
                {
					return this.currentBody;
                }
				this.currentBody = computeInternal(JsonUtil.LoadBodyPartsInfo(this.currentBodyRule.fileName));
				return this.currentBody;
            }
			if (this.defaultBody != null)
            {
				return defaultBody;
            }
			if (null == this.defaultBodyPath)
            {
				if (this.resourceHome == null) {
					Log.trace($"TRACE: GetBodyPartsInfo - ResourceHome is null. Bailing...");
					return null;
				}
				Log.trace($"TRACE: ResourceHome [{this.resourceHome}]");
				string resourceRoot = Path.Combine(NPCMonitor.portraitsRoot,NPCMonitor.npcSubDir, this.resourceHome);
				Log.trace($"TRACE: resourceRoot [{resourceRoot}]");
				if (!File.Exists(Path.Combine(resourceRoot, "body.json"))) {
					Log.trace($"TRACE: [{Path.Combine(resourceRoot, "body.json")}] Not Found");
					noBodyFound = true;
					return null;
				}
				if (null != JsonUtil.LoadBodyPartsInfo(Path.Combine(resourceRoot, "body.json")))
                {
					this.defaultBodyPath = Path.Combine(resourceRoot, "body.json");
                }
				else
                {
					Log.always($"TRACE: JsonUtil Failed to load [{Path.Combine(resourceRoot, "body.json")}]. Bailing... (Body rules will be skipped)");
					noBodyFound = true;
                }
            }
			this.defaultBody = computeInternal(JsonUtil.LoadBodyPartsInfo(this.defaultBodyPath));
			return this.defaultBody;
		}

		public static readonly float[] ERRORVECTOR = new float[3]{-1.0f,-1.0f,-1.0f};

		public static float[] stringToVector(string input) {
			if (input.StartsWith(",")) return ERRORVECTOR;
			int first  = input.IndexOf(",");
			if (first < 0) return ERRORVECTOR;
			string firstStr = input.Substring(0,first).Trim();
			if (0 == firstStr.Length || "." == firstStr) return ERRORVECTOR;
			int second = input.IndexOf(",", first+1, (input.Length - first - 1));
			if (second < 0) return ERRORVECTOR;
			string secondStr  = input.Substring(first+1,(second - first - 1)).Trim();
			if (0 == secondStr.Length || "." == secondStr) return ERRORVECTOR;
			string thirdStr  = input.Substring(second+1).Trim();
			if (0 == thirdStr.Length || "." == thirdStr) return ERRORVECTOR;
			float parsed;
			float.TryParse(firstStr,out parsed);
			if (parsed < 0.0f) return ERRORVECTOR;
			float[] ret = new float[3]{-1.0f,-1.0f,-1.0f};
			ret[0] = parsed;
			float.TryParse(secondStr,out parsed);
			if (parsed < 0.0f) return ERRORVECTOR;
			ret[1] = parsed;
			float.TryParse(thirdStr,out parsed);
			if (parsed < 0.0f) return ERRORVECTOR;
			ret[2] = parsed;
			return ret;
		}

		// This interceptor is used to decortate the body model object with any information 
		// we don't want to constantly be re-computing or looking up... 
		public static BodyPartsInfo computeInternal(BodyPartsInfo body)
		{         
			BodyPartsInfo ret = body;
			ret.computedHasScale = false;

			// Need original size ...
			Kingmaker.Enums.Size originalSizeEnum = Kingmaker.Enums.Size.Medium;
			int originalSize = (int)originalSizeEnum;
			if (null != body.raceName)
            {
				ProgressionRoot pr = Game.Instance.BlueprintRoot.Progression;
				for (int i = 0; i < pr.CharacterRaces.Length; i++)
				{
					BlueprintRace br = pr.CharacterRaces[i];
					if (br.name == body.raceName) 
					{
						ret.computedRace = br;
						originalSizeEnum = br.Size;
						originalSize = (int)originalSizeEnum;
						Log.trace($"TRACE: Found Race [{br.name}] Race.Size [{br.Size}] int value [{originalSize}]");
						break;
					}
				}
            }

			if ((null == body.desiredScale) && (null == body.size) && (null == body.additionalScale))
            {
				return ret;
            }

			if (null != body.desiredScale)
            {
				// When desiredScale is supplied, it trumps whatever values are under size and additionalScale.
				body.size = null;
				body.additionalScale = null;

				string desiredScaleStr = body.desiredScale;
				float desiredScale = -1.0f;
				if (desiredScaleStr.IndexOf(',') != -1)
                {
					float[] v = stringToVector(desiredScaleStr);
					if (v == ERRORVECTOR)
                    {
						Log.always($"WARN: Invalid Desired Scale [{body.desiredScale}] : Requires single or triple positive float between 0.19 and 10.0. Ignoring.");
						body.desiredScale = null;
						return ret;
                    }
					ret.computedScaleByVector = new Vector3(v[0],v[1],v[2]);
					ret.computedHasScale = true;
					return ret;
                }
				else
				{ 
					float.TryParse(body.desiredScale,out desiredScale);
					if (desiredScale < 0.0f) 
					{ 
						Log.always($"WARN: Invalid Desired Scale [{body.desiredScale}] : Requires positive value between 0.19 and 10.0. Ignoring.");
						body.desiredScale = null;
						return ret;
					} 

					if (desiredScale < 1.0f && originalSize > 0) 
					{
						int computedSize;
						// We don't want to actually change anyone's size to "fine" or it will break reduction spells
						if ((desiredScale > (66f/100f)) || originalSize < 2)
						{
							computedSize = originalSize - 1;
							ret.computedAdditionalScale =  (desiredScale < 0.6732) ? 0.0f : ((100f*desiredScale/66f) - 1f);
						}
						else if ((desiredScale > ((66f*66f)/(100f*100f))) || originalSize < 3) // ~0.45
						{
							computedSize = originalSize - 2; // Max for Halflings
							ret.computedAdditionalScale = (desiredScale < 0.444312f) ? 0.0f : ((10000f*desiredScale/4356f) - 1f);
						}
						else if ((desiredScale > ((66f*66f*66f)/(100f*100f*100f))) || originalSize < 4) // ~ 0.30
						{
							computedSize = originalSize - 3; // Max for Gnomes
							ret.computedAdditionalScale = (desiredScale < 0.29324592f) ? 0.0f : ((1000000f*desiredScale/287496f) - 1f);
						}
						else if ((desiredScale > ((66f*66f*66f*66f)/(100f*100f*100f*100f))) || originalSize < 5) // ~ 0.20
						{
							computedSize = originalSize - 4; // Max for most races including Human
							ret.computedAdditionalScale = (desiredScale < 0.1935423072f) ? 0.0f : ((100000000f*desiredScale/18974736f) - 1f);
						}
						else if ((desiredScale > (0.1252332576f)) || originalSize < 6) // ~ 0.15
						{
							computedSize = originalSize - 5; // Max for Half-Orcs
							ret.computedAdditionalScale = (desiredScale < 0.13029268120704f) ? 0.0f : ((desiredScale*7.828528744290567f) - 1f);
						}
						else 
						{
							Log.always($"WARN: Unexpected Size Category [{originalSizeEnum}] with desired scale [{desiredScale}]. Ignoring... ");
							body.desiredScale = null;
							return ret;
						}
						ret.computedHasScale = true;
						ret.computedSize = ((Kingmaker.Enums.Size)computedSize);
						ret.size = $"{ret.computedSize}";
						ret.additionalScale = (0.0f == ret.computedAdditionalScale) ? null : $"{ret.computedAdditionalScale}";
						Log.trace($"TRACE: Desired Scale [{desiredScale}] resulted in Size [{ret.computedSize}] Additional Scale [{ret.computedAdditionalScale}]");
						return ret;
					} 
					if (desiredScale > 1.0f && originalSize < 8)
					{
						int computedSize;
						// We don't want to actually change anyone's size to "colossal" or it will break enlargement spells. 
						if ((desiredScale < 1.5f) )
						{
							computedSize = originalSize;
							ret.computedAdditionalScale = desiredScale - 1f; // Easy case...
						} 
						else if (desiredScale < 2.29f || originalSize > 5) 
						{
							computedSize = originalSize + 1;
							ret.computedAdditionalScale = (desiredScale < 1.55) ? 0.0f : ((desiredScale*0.66f) - 1f);
						}
						else if (desiredScale < 3.47f || originalSize > 4) // Max for Half-Orcs
						{
							computedSize = originalSize + 2;
							ret.computedAdditionalScale = (desiredScale < 2.35f) ? 0.0f : ((desiredScale*0.4356f) - 1f);
						}
						else if (desiredScale < 5.27f || originalSize > 3) // Max for most races including Human
						{
							computedSize = originalSize + 3;
							ret.computedAdditionalScale = (desiredScale < 3.55f) ? 0.0f : ((desiredScale*0.287496f) - 1f);
						}
						else if (desiredScale < 7.98f || originalSize > 2) // Max for Gnomes
						{
							computedSize = originalSize + 4;
							ret.computedAdditionalScale = (desiredScale < 5.36f) ? 0.0f : ((desiredScale*0.18974736f) - 1f);
						} 
						else 
						{
							if (desiredScale > 10.0f) desiredScale = 10.0f; // Max for Game Engine..
							computedSize = originalSize + 5;
							ret.computedAdditionalScale = (desiredScale < 8.15f) ? 0.0f : ((desiredScale*0.1252332576f) - 1f);
						}
						ret.computedHasScale = true;
						ret.computedSize =  ((Kingmaker.Enums.Size)computedSize);
						ret.size = (computedSize == originalSize) ? null : $"{ret.computedSize}";
						ret.additionalScale = (0.0f == ret.computedAdditionalScale) ? null : $"{ret.computedAdditionalScale}";
						Log.trace($"TRACE: Desired Scale [{desiredScale}] resulted in Size [{ret.computedSize}] Additional Scale [{ret.computedAdditionalScale}]");
						return ret;
					} 
				}

				// Either desiredScale was 1.0, which means... do nothing pretty much...
				// Or, the original size was 0/8, so no adjustment could be made. 
				body.desiredScale= null;
				return ret;
            }

			if (null != body.additionalScale) {
				float additionalScale = -1.0f;
				float.TryParse(body.additionalScale,out additionalScale);
				if (additionalScale <= 0.0f) { 
					Log.always("WARN: Invalid additional Scale. Must be greater than 0.0. Ignoring Value");
					body.additionalScale = null;
					ret.computedAdditionalScale = 0.0f;
                } 
				else
                {
					ret.computedAdditionalScale = additionalScale;
                }
            }

			if (null != body.size) {
				string sizeStr = body.size.ToLower();
				if (sizeStr == "fine")            { ret.computedSize = Kingmaker.Enums.Size.Fine; }
				else if (sizeStr == "diminutive") { ret.computedSize = Kingmaker.Enums.Size.Diminutive; }
				else if (sizeStr == "tiny")       { ret.computedSize = Kingmaker.Enums.Size.Tiny; }
				else if (sizeStr == "small")      { ret.computedSize = Kingmaker.Enums.Size.Small; }
				else if (sizeStr == "medium")     { ret.computedSize = Kingmaker.Enums.Size.Medium; }
				else if (sizeStr == "large")      { ret.computedSize = Kingmaker.Enums.Size.Large; }
				else if (sizeStr == "huge")       { ret.computedSize = Kingmaker.Enums.Size.Huge; }
				else if (sizeStr == "gargantuan") { ret.computedSize = Kingmaker.Enums.Size.Gargantuan; }
				else if (sizeStr == "colossal")   { ret.computedSize = Kingmaker.Enums.Size.Colossal; }
				else {
					Log.always($"WARN: Invalid Size [{body.size}] : Must be one of [fine,diminutive,tiny,small,medium,large,huge,gargantuan,colossal]. Ignoring. ");
					body.size = null;
				}
				if (((int)ret.computedSize) == originalSize) {
					Log.always($"WARN: Requested Body Size [{body.size}] matches Original Size. Ignoring");
					body.size = null;
				}
            }

			if ((null == body.size) && (null == body.additionalScale)) {
				return ret;
            }

			ret.computedHasScale = true;
			return ret;
        }

		// Assumes TryLoadPortraitRules is called first!
		public void TryLoadBodyAndRules()
        {
			// Do NOT call evaluateRules from this execution branch or it may 
			// cause infinite loop

			Log.trace($"TRACE: TryLoadBodyAndRules called for [{this.Name}]");
			if (this.noBodyFound) return;

			BodyPartsInfo bodyPartsInfo = this.GetBodyPartsInfo();

			if (null == bodyPartsInfo) {
				Log.trace($"TRACE: No Body Parts Found Bailing...");
				return;
			}

			applyBody(this.npc, bodyPartsInfo);

			if (this.resourceHome == null) {
				Log.trace($"TRACE: ResourceHome is null. Bailing...");
				return;
			}

			if (!settings.AllowBodyRules) {
				Log.debug($"AllowBodyRules Disabled...");
				return;
			}
			this.allBodyRules = RuleFactory.findRules(NPCMonitor.portraitsRoot,NPCMonitor.npcSubDir,this.resourceHome,"body",false);
			if (this.allBodyRules != null)
            {
				Log.trace($"TRACE: [{this.allBodyRules.Count}] body rules found");
            }
			else
            {
				Log.trace($"TRACE: No body rules found (allBodyRules is null)");
            }
		}

		static public string extractResourceHome(UnitEntityData npc, string defaultValue = null)
		{
			Log.trace("extractResourceHome Called");

			// Issue: Kitsune have 2 forms and an ability toggle to flip between them. These forms 
			// don't count as polymorph and don't fire polymorph events... Visible Race also
            // doesn't update (keeps reporting kitsune). This causes issues as the home of the
            // rules/portraits/bodies may suddently change. So we have special code to deal 
			// with kitsune....

			if (npc.Progression.Race.RaceId == Race.Kitsune)
			{ 
				Log.trace("shapeshifter detected");
				string name = npc?.Descriptor?.CharacterName;
				if (null != name)
				{ 
					ActivatableAbility ChangeShapeKitsuneToggle = null;
					if (null != ChangeShapeKitsuneAToggleBlueprint)
					{ 
						ChangeShapeKitsuneToggle = GetActivatableAbility(npc, ChangeShapeKitsuneAToggleBlueprint);
					}
					if (null == ChangeShapeKitsuneToggle && null != ChangeShapeKitsuneBToggleBlueprint)
                    {
						ChangeShapeKitsuneToggle = GetActivatableAbility(npc, ChangeShapeKitsuneBToggleBlueprint);
                    }
					if (null != ChangeShapeKitsuneToggle)
					{ 
						if (!ChangeShapeKitsuneToggle.IsAvailable || ChangeShapeKitsuneToggle.IsOn)
						{
							Log.trace("Returning Human Form Path");
							return $"{name}Human_Portrait";
						}
						else
						{
							Log.trace("Returning Fox Form Path");
							return $"{name}Fox_Portrait";
						}
					}
					else
                    {
						Log.trace($"NPC [{name}] : Unable to determine ChangeShapeKitsuneToggle");
                    }
				}
				else
                {
					Log.trace("name was null. Unable to determine resource.");
                }

				// fall through intentional...
				if (null != defaultValue)
				{ 					 
					return defaultValue;
				}
			}

			string preference = npc?.UISettings?.PortraitBlueprint?.name;
			if (null != preference)
			{ 
				Log.trace($"returning preference [{preference}]");
				return preference;
			}
			BlueprintPortrait bPortrait = npc?.UISettings?.Owner?.Blueprint?.PortraitSafe;
			return (bPortrait?.name ?? bPortrait?.ToString());
		}

		public static ActivatableAbility GetActivatableAbility(UnitEntityData npc, BlueprintActivatableAbility blueprint)
		{
			for (int i = 0; i < npc.ActivatableAbilities.RawFacts.Count; i++)
			{
				ActivatableAbility activatableAbility = npc.ActivatableAbilities.RawFacts[i];
				if (activatableAbility.Blueprint == blueprint)
				{
					return activatableAbility;
				}
			}
			return null;
		}

		static HashSet<string> modestyIds = new HashSet<string>() {
			"14f8645017b27e441adc9a15b2b6e874", // EE_UnderwearNormal_F_Any
			"12bcc4600d574fa48803c82efd186d06", // EE_UnderwearDancer_F_Any",
			"a1a0d015c441a454eb3fd21d0da361e7"  // EE_UnderwearNormal_M_Any
		};

		static public void applyBody(UnitEntityData npc, BodyPartsInfo bodyPartsInfo, bool refreshItems = true, bool allowMinimalOutfit = true)
        {
			float scale = -1.0f;
			try
			{ 
				if (null == bodyPartsInfo)
				{ 
					Log.trace("TRACE: BodyPartsInfo is null. Bailing...");
					return;
				}
				if (npc.Body.IsPolymorphed)
				{ 
					if (npc.Progression.Race.RaceId != Race.Kitsune)
					{
						Log.trace("TRACE: NPC appears to be polymorphed. Bailing...");
						return;
					}
					// Add more here? Not really sure what to do. CHeck that polymorph is kitsune? Think I'll pass...
				}

				if (null == bodyPartsInfo.standardAppearance && null == bodyPartsInfo.defaultAppearance && null == bodyPartsInfo.minimalAppearance) {
					Log.trace("TRACE: BodyPartsInfo standardAppearance, defaultAppearance and minimalAppearance are null");
					return;
				}

				if (null == bodyPartsInfo.standardAppearance || null == bodyPartsInfo.minimalAppearance || null == bodyPartsInfo.defaultAppearance) {
					if (null == bodyPartsInfo.standardAppearance )
					{
						bodyPartsInfo.standardAppearance = (null != bodyPartsInfo.defaultAppearance) ? bodyPartsInfo.defaultAppearance : bodyPartsInfo.minimalAppearance;
					}
					if (null == bodyPartsInfo.defaultAppearance )
					{
						bodyPartsInfo.defaultAppearance = (null != bodyPartsInfo.standardAppearance) ? bodyPartsInfo.standardAppearance : bodyPartsInfo.minimalAppearance;
					}
					if (null == bodyPartsInfo.minimalAppearance)
					{
						bodyPartsInfo.minimalAppearance = (null != bodyPartsInfo.standardAppearance) ? bodyPartsInfo.standardAppearance : bodyPartsInfo.defaultAppearance;
					}
				}
				if (0 == bodyPartsInfo.standardAppearance.Length) {
					Log.debug("Assertion Failure: BodyPartsInfo standardAppearance is empty.");
					return;
				}
				if (0 == bodyPartsInfo.minimalAppearance.Length) {
					Log.debug("Assertion Failure: BodyPartsInfo minimalAppearance is empty.");
					return;
				}
				if (0 == bodyPartsInfo.defaultAppearance.Length) {
					Log.debug("Assertion Failure: BodyPartsInfo defaultAppearance is empty.");
					return;
				}

				// See if we can extract Race... (If not, assume race compatible items)
				BlueprintRace blueprintRace = npc.Descriptor.Progression.VisibleRace;
				if (null != bodyPartsInfo.computedRace) {
					blueprintRace = bodyPartsInfo.computedRace;
					Log.trace($"TRACE: Extracted Race [{blueprintRace.name}]");
                }

				if (bodyPartsInfo.computedHasScale || npc.View.OverrideDollRoomScale != Vector3.zero)
                {
					if (Log.traceEnabled) { 
						if (null != bodyPartsInfo.desiredScale)
		                {
							Log.trace($"Handling Desired Scale [{bodyPartsInfo.desiredScale}] Size [{bodyPartsInfo.size}] AdditionalScale [{bodyPartsInfo.additionalScale}] current Size [{npc.Descriptor.State.Size}] current Scale [{npc.Progression.CurrentScalePercent}]");
				        }
					}
					npc.View.DoNotAdjustScale = autoScale ? false : bodyPartsInfo.computedHasScale;
					bool needsRefresh = false;
					if (null != bodyPartsInfo.size)
                    {
						if (npc.Descriptor.State.Size !=  bodyPartsInfo.computedSize)
                        {
							Log.trace($"TRACE: Changing body size to [{bodyPartsInfo.computedSize}]");
							npc.Descriptor.State.Size = bodyPartsInfo.computedSize;
							needsRefresh = true;
                        }
                    }
					else if (npc.Descriptor.State.Size != npc.Descriptor.OriginalSize)
                    {
						Log.trace($"TRACE: Resetting Size to Original value [{npc.Descriptor.OriginalSize}]");
						npc.Descriptor.State.Size = npc.Descriptor.OriginalSize;
						needsRefresh = true;
                    }
					if (null != bodyPartsInfo.additionalScale)
                    {
						if (npc.Progression.CurrentScalePercent != bodyPartsInfo.computedAdditionalScale)
						{ 
							Log.trace($"TRACE: Updating additional scale to [{bodyPartsInfo.additionalScale}]");
							npc.Progression.ResetScalePercent();
							npc.Progression.UpdateScalePercent(bodyPartsInfo.computedAdditionalScale);
							needsRefresh = true;
						}
                    } 
					else if (npc.Progression.CurrentScalePercent != 0f)
                    {
						Log.trace("TRACE: Resetting additional scale to 0");
						npc.Progression.ResetScalePercent();
						needsRefresh = true;
					}
					if (bodyPartsInfo.computedScaleByVector != Vector3.zero)
                    {
						if ((npc.View as UnityEngine.Component).transform.localScale != bodyPartsInfo.computedScaleByVector)
                        {
							npc.View.DoNotAdjustScale = true;
							npc.View.OverrideDollRoomScale = bodyPartsInfo.computedScaleByVector;
                        }
						(npc.View as UnityEngine.Component).transform.localScale = bodyPartsInfo.computedScaleByVector;
                    } else if (needsRefresh) {
						// If we make ANY change (size or scale), we need to call ResetSizeScale(). Otherwise
						// UpdateScaleImmediatly will just use the cached value instead of recomputing a new one.
						npc.View.ResetSizeScale();
						// Bypasses DoNotAdjustScale checks. Invokes recomputation if cache was invalidated.
						npc.View.UpdateScaleImmediately();
						float actualScale = npc.View.GetSizeScale();
						Log.trace($"TRACE: new (actual) scale [{actualScale}]");
						npc.View.OverrideDollRoomScale = (actualScale > 1.5f) ? (UnityEngine.Vector3.one * 1.5f) : (UnityEngine.Vector3.one * actualScale);
						
                    }
					if (!bodyPartsInfo.computedHasScale && npc.View.OverrideDollRoomScale != Vector3.zero)
                    {
						npc.View.OverrideDollRoomScale = Vector3.zero;
                    }
                } 
			    else 
				{
					Log.trace($"TRACE: No body scale changes detected");
				}

				Dictionary<string, Tuple<int,int>> nameToColor = new Dictionary<string, Tuple<int, int>>();
				Character characterAvatar = npc.View.CharacterAvatar;
				Kingmaker.Blueprints.Gender gender = ("Female" == bodyPartsInfo.gender) ? Kingmaker.Blueprints.Gender.Female : Kingmaker.Blueprints.Gender.Male;

				characterAvatar.RemoveAllEquipmentEntities(false);

				// This doesn't do anything... -> characterAvatar.SetRaceAndGender(gender,blueprintRace.RaceId);

				// If Race isn't current race, add new Hidden fact so their race doesn't appear to change. 
				//
				// if (blueprintRace !=  npc.Progression.Race) {
				//
				//    // For these to not break if/when the mod is uninstalled, we would need to 
				//    // have ReplaceRace return a static reference or use code to return a specific value. IE: The code
				//    // would need to stand alone and not rely on this mod or any variables, methods, etc...
                //    // Ideally they uninstall and we cleaning remove the hidden facts while restorig the race. 
				//    // I hesitate on this feature because changing RACE would be persisted by the save game, 
				//    // but scaling and model mesh items are always pulled from source, so any runtime changes\
				//    // we make are restored if/when the mod is uninstalled without any cleanup needed.
				//
				//    npc.Ensure<UnitPartHiddenFacts>().Add(new IHiddenFacts() {
				//        public BlueprintRace ReplaceRace {get;} = this.m_ReplaceRace;
				//	      public HashSet<BlueprintUnitFact> Facts { get; } = ImmutableHashSet<BlueprintUnitFact>.Empty()
				//    });
				// }
				// npc.Progression.SetRace(blueprintRace);
				// // Don't support gender changes because the skeletons/animations are not compatible with the opposite gender models.
				// // npc.Progression.SetGender(gender);
				//
				// // npc.View.CharacterAvatar.Skeleton = prev.View.CharacterAvatar.Skeleton;
				// // npc.View.CharacterAvatar.CopyEquipmentFrom( prev.View.CharacterAvatar);
				// // npc.View.CharacterAvatar.UpdateSkeleton();

				int modestyIdCount = 0;

				// BundledResourceHandle<EquipmentEntity> bundledResourceHandle = BundledResourceHandle<EquipmentEntity>.Request(bodyPart.assetId, false);
				// EquipmentEntity ee = bundledResourceHandle.Object;
				// // or possibly : // EquipmentEntity ee = (EquipmentEntity) ResourcesLibrary.TryGetScriptable(bodyPart.assetId);	
				// // or possibly : // EquipmentEntity ee = ResourcesLibrary.TryGetResource<EquipmentEntity>(bodyPart.assetId); 
				// characterAvatar.AddEquipmentEntity(ee, false, bodyPart.primaryColor, bodyPart.secondaryColor);

				// This issue with pre-loading the EquipmentEntities above is that we become responsible for thier life cycle. IE:
				// We have to decide when it is safe to call the "dispose" method on the bundle. But when we pass the assetId 
				// in as a string, the class loads the bundle and manages the life-cycle for us...  Which I prefer. It means
				// saving off the color info and applying it in a second pass...
				if (npc.UISettings.ShowClassEquipment) {
					refreshItems = false;
					Log.trace("TRACE: ShowClassEquipment is true");
					foreach (BodyPartsInfo.BodyPart bodyPart in bodyPartsInfo.defaultAppearance)
					{
						nameToColor[bodyPart.name] = Tuple.Create(bodyPart.primaryColor,bodyPart.secondaryColor);
						if (modestyIds.Contains(bodyPart.assetId)) modestyIdCount += 1;
						characterAvatar.AddEquipmentEntity(bodyPart.assetId);
					}
				} else {
					Log.trace("TRACE: ShowClassEquipment is false");
					if (allowMinimalOutfit && ((null == npc?.Body?.Shirt?.MaybeItem) && !(npc.Body.Armor.HasArmor))) { 
						Log.trace("TRACE: NPC isNaked (No Armor and No Shirt/Robe)");
						foreach (BodyPartsInfo.BodyPart bodyPart in bodyPartsInfo.minimalAppearance)
						{
							nameToColor[bodyPart.name] = Tuple.Create(bodyPart.primaryColor,bodyPart.secondaryColor);
							if (modestyIds.Contains(bodyPart.assetId)) modestyIdCount += 1;
							characterAvatar.AddEquipmentEntity(bodyPart.assetId);
						}
					} 
					else
					{
						if (allowMinimalOutfit) { 
							Log.trace("TRACE: NPC seems to be clothed (Armor or Shirt/Robe detected)");
						} else {
							Log.trace("TRACE: allowMinimalOutfit is false. Using standard outfit");
						}
						foreach (BodyPartsInfo.BodyPart bodyPart in bodyPartsInfo.standardAppearance)
						{
							nameToColor[bodyPart.name] = Tuple.Create(bodyPart.primaryColor,bodyPart.secondaryColor);
							if (modestyIds.Contains(bodyPart.assetId)) modestyIdCount += 1;
							characterAvatar.AddEquipmentEntity(bodyPart.assetId);
						}
					}
				}
				if (avoidNudity && 0 == modestyIdCount)
				{
					characterAvatar.AddEquipmentEntity((gender == Kingmaker.Blueprints.Gender.Female) ? "12bcc4600d574fa48803c82efd186d06" : "a1a0d015c441a454eb3fd21d0da361e7");
				}

				if (0 == characterAvatar.EquipmentEntityCount)
				{
					Log.debug("Equipment Management was complete and total failure!... Run for the hills! (And then tell the author)");
				}

				foreach (EquipmentEntity ee in characterAvatar.EquipmentEntities)
				{
					if (nameToColor.ContainsKey(ee.name)) {
						Log.trace($"TRACE: Loaded BodyPart [{ee.name}] Setting Colors...");
						Tuple<int,int> colors = nameToColor[ee.name];
						characterAvatar.SetPrimaryRampIndex(ee, colors.Item1);
						characterAvatar.SetSecondaryRampIndex(ee, colors.Item2);
					}
				}
				characterAvatar.RebuildOutfit();
				if (refreshItems) { 
					npc.View.UpdateBodyEquipmentVisibility();
				}
			}
			catch (Exception ex) 
			{
				Log.always($"mon: applyBody: Exception Caught : {ex.ToString()}");
			}
		}

		public bool GUIIsUp()
        {
			// TODO
			return true;
        }


		public void evaluateRules()
        {
			// We use reactive throttling to merge notifications and process in batches
			Log.trace("TRACE: evaluateRules Called...");
			Kingmaker.GameModes.GameModeType gm = Game.Instance.CurrentMode;
			if (gm == Kingmaker.GameModes.GameModeType.FullScreenUi || gm == Kingmaker.GameModes.GameModeType.Dialog)
            {
                // gm == GameModeType.Pause     || gm == GameModeType.FullScreenUi      || gm == GameModeType.Cutscene
                // gm == GameModeType.Rest      || gm == GameModeType.Kingdom           || gm == GameModeType.GameOver
                // gm == GameModeType.BugReport || gm == GameModeType.KingdomSettlement || gm == GameModeType.CutsceneGlobalMap
				// gm == GameModeType.PhotoMode || gm == GameModeType.EscMode

				Log.trace("Fullscreen/Dialog Mode Detected. Evaluating Rules immediately");
				this.evfloodProtection = -1;
				evaluateRulesNow();
				return;
            }
			this.evfloodProtection++;
			if (0 == this.evfloodProtection) {
				UniRx.Scheduler.MainThread.Schedule(ThrottleGears[this.evThrottleGear], this.evaluateRulesLater);
     		}
		}

		public void evaluateRulesLater()
        {
			Log.trace("TRACE: evaluateRules Later Called...");
			if (-1 == this.evfloodProtection) return;
			int throttle = this.evThrottleGear;
			if (0 == this.evfloodProtection) {
       			// Nothing new happened while we delayed... 
       			this.evfloodProtection = -1;
       			throttle = (throttle > 0) ? (throttle - 1) : 0;
       			this.evThrottleGear = throttle;        
   			} else {
				// More events arrived while we were waiting. Increase throttle and renew
				this.evfloodProtection = 0;
       			throttle = (throttle < HIGHESTGEAR) ? (throttle + 1) : HIGHESTGEAR;
       			this.evThrottleGear = throttle;
				UniRx.Scheduler.MainThread.Schedule(ThrottleGears[throttle], this.evaluateRulesLater);
				// If we reach max throttle, then allow event to fire, otherwise delay longer...  
				if (throttle < HIGHESTGEAR) return;
			}
			evaluateRulesNow();
		}

		public void evaluateRulesNow()
        {
			Log.trace("TRACE: evaluateRules NOW Called...");

			if (this.npc.Progression.Race.RaceId == Race.Kitsune)
			{ 				 
				string currentResourceHome = extractResourceHome(this.npc,this.resourceHome);
				if (null != currentResourceHome && currentResourceHome != this.resourceHome)
				{ 
					Log.trace($"TRACE: this.resourceHome [{this.resourceHome}] Changed to [{currentResourceHome}]. Reloading Rules.");
					if (null != allPortraitRules) this.allPortraitRules.Clear();
					if (null != allBodyRules) this.allBodyRules.Clear();
					this.allPortraitRules = null;
					this.allBodyRules = null;
					// Reset Cache variables
					this.currentBodyRule = null;
					this.currentBody = null;
					this.defaultBody = null;
					this.defaultBodyPath = null;
					TryLoadPortraitRules();
					TryLoadBodyAndRules(); // will reset this.resourceHome
					this.forceBodyRefresh = true;
					this.forcePortraitRefresh = true;
				}
			}

//			if (0 == Interlocked.Exchange(ref locks[lockId], 1)) try  {
			Rule ruleWinner;
			if (null != this.allPortraitRules) {
				ruleWinner = null;
				foreach (Rule rule in this.allPortraitRules)
				{
					if (rule.ruleEvaluator.evaluate(this.ruleContext))
					{
						Log.trace($"TRACE: Rule [{rule.portraitId}] evaluated to TRUE!");
						ruleWinner = rule;
						break;
					}
					Log.trace($"TRACE: Rule [{rule.portraitId}] evaluated to false");
				}
				if (ruleWinner != null)
				{
					enforcePortraitRule(ruleWinner);
				}
				else
				{
					enforceDefaultPortraitRule();
				}
			} else { 
				Log.trace("TRACE: No Portrait Rules...");
			}

			if (noBodyFound) return;
				
			ruleWinner = null;
			if (null != this.allBodyRules) {
				foreach (Rule rule in this.allBodyRules)
				{
					if (rule.ruleEvaluator.evaluate(this.ruleContext))
					{
						Log.trace($"TRACE: Rule [{rule.portraitId}] evaluated to TRUE!");
						ruleWinner = rule;
						break;
					}
					Log.trace($"TRACE: Rule [{rule.portraitId}] evaluated to false");
				}
			} else { 
				Log.trace("TRACE: No Body Rules...");
			}
			if (ruleWinner != null)
			{
				enforceBodyRule(ruleWinner);
			}
			else
			{
				enforceDefaultBodyRule();
			}
		}

		public void enforcePortraitRule(Rule rule)
        {
			// Is it changing?
			if (forcePortraitRefresh)
            {
				Log.trace("TRACE: forcePortraitRefresh is true. Forcing refresh...");
				forcePortraitRefresh = false;
            } else if (this.currentPortraitRule != null && (this.currentPortraitRule.portraitId == rule.portraitId)) {
				Log.trace("TRACE: Portrait Rule already active. Skipping...");
				return;
            } 
			else
            {
				Log.debug($"Changing to Portrait Rule [{rule.portraitId}]");
            }
			this.currentPortraitRule = rule;
			PortraitData portraitData = CompanionPortraitEnablerMain.UpdatedPortraitDataCache(
				rule.resourceId, // example value: "ember_portrait"
				rule.portraitId  // example value: "πpcPortraits/ember_portrait"  ??? (Confirm)
			);
			if (portraitData != null)
			{		
				Log.trace("TRACE: Calling setPortrait...");
				npc.UISettings.SetPortrait(portraitData);
			} 
			else
            {
				Log.trace("TRACE: portraitData is null. Nothing to do... ");
            }
        }

		public void enforceBodyScale() {

			if (bodyScale > 0.0f) {
				npc.View.OverrideDollRoomScale=UnityEngine.Vector3.one * bodyScale;
				(npc.View as UnityEngine.Component).transform.localScale = UnityEngine.Vector3.one * bodyScale;
			} 
			// else
            // {
			//		npc.View.OverrideDollRoomScale=UnityEngine.Vector3.one * npc.View.GetOriginalScale();
			// }
        }

		public void enforceBodyRule(Rule rule)
        {
			if (forceBodyRefresh)
            {
				Log.trace("TRACE: forceBodyRefresh is true. Forcing refresh...");
				this.forceBodyRefresh = false;
			}
			else if ((this.currentBodyRule != null) && 
				     (this.currentBodyRule.portraitId == rule.portraitId) &&
				     ((1 == this.ruleContext.USHORT_VALUES[(int)PROPERTY.IsNaked]) == this.previouslyNaked) && 
					 ((1 == this.ruleContext.USHORT_VALUES[(int)PROPERTY.UsingDefaultEquipment]) == this.previouslyDefault)
					)
            {
				Log.trace("TRACE: Body Rule already active. Skipping...");
				return;
            } 
			else
            {
				Log.debug($"Changing to Body Rule [{rule.portraitId}]");
            }
			this.currentBodyRule = rule;
			this.currentBody = null;
			this.previouslyNaked = ((null == this.npc?.Body?.Shirt?.MaybeItem) && (!(this.npc.Body.Armor.HasArmor)));
			this.previouslyDefault = npc.UISettings.ShowClassEquipment;
			BodyPartsInfo bodyPartsInfo = this.GetBodyPartsInfo();
			if (null != bodyPartsInfo) {
				applyBody(this.npc, bodyPartsInfo);
			}
        }

		public void enforceDefaultPortraitRule()
        {
			if (forcePortraitRefresh) // Used by UninstallMode
            {
				Log.trace("TRACE: forcePortraitRefresh is true. Forcing refresh...");
				forcePortraitRefresh = false;
            } 
			else if (this.currentPortraitRule == null)
			{
				Log.trace("TRACE: Default Portrait Rule already active. Skipping Update...");
				return;
			} 
			else
            {					
				Log.debug($"Loading Default Portrait [{npc.Descriptor.Blueprint.PortraitSafe}]");
            }

			this.currentPortraitRule = null;

            CompanionPortraitEnablerMain.UpdatedPortraitDataCache(
			    this.resourceHome, 
			    Path.Combine(npcSubDir,this.resourceHome)
			);

			// npc.UISettings.OverridePortraitBlueprintRaw
			// npc.UISettings.PortraitBlueprint;

			// The game mostly uses npc.UISettings.PortraitBlueprint. If the npc has a custom portrait, 
			// that method returns the custom portrait. But ... if they do not, PortraitBlueprint checks
			// a flag to see if the Portrait should be ignore. When true, "PortraitBlueprint" returns an
            // OverridePortrait. This is why setting Portrait to PortraitSafe can/will cause Nenio to
            // appear human. SetPortrait, given a blueprint, sets the custom portrait to null which
            // in turn causes the PortraitBlueprint to return Nenio's alternative portrait even if you
            // set the portrait to something else...  
			npc.UISettings.SetPortrait(npc.Descriptor.Blueprint.PortraitSafe);
		}

		public void enforceDefaultBodyRule()
        {
			if (forceBodyRefresh)
            {
				Log.debug("forceBodyRefresh is true. Forcing refresh...");
				forceBodyRefresh = false;
            } 
			else if ((this.currentBodyRule == null) && 
				     ((1 == this.ruleContext.USHORT_VALUES[(int)PROPERTY.IsNaked]) == this.previouslyNaked) && 
					 ((1 == this.ruleContext.USHORT_VALUES[(int)PROPERTY.UsingDefaultEquipment]) == this.previouslyDefault)
					)
            {
				Log.trace("TRACE: Default Body Rule already active. Skipping Update...");
				return;
            }

			this.currentBodyRule = null;
			this.currentBody     = null;
			this.previouslyNaked = ((null == this.npc?.Body?.Shirt?.MaybeItem) && (!(this.npc.Body.Armor.HasArmor)));
			this.previouslyDefault = npc.UISettings.ShowClassEquipment;

			BodyPartsInfo bodyPartsInfo = this.GetBodyPartsInfo();
			if (null != bodyPartsInfo)
			{ 
				applyBody(this.npc, bodyPartsInfo);
			} 
			else
            {
				Log.debug("Default Body is null (No body.json in parent). Skipping...");
            }
		}

		// Relayed from CompanionPortraitEnablerMain
		public void OnCompanionActivated() // : IPartyHandler
		{
			try 
			{ 
				Log.trace($"TRACE: OnCompanionActivated called on [{this.Name}]");
				EventBus.Unsubscribe(this);
				this.npc.UISettings.UnsubscribeFromEquipmentVisibilityChange(this.OnEquipmentVisibilityChange);

				if (null != this.defaultBodyPath)
				{
					forceBodyRefresh=true;
					if (settings.AvoidNudity)
					{
						BodyPartsInfo bodyPartsInfo = this.GetBodyPartsInfo();
						if (null != bodyPartsInfo && null != bodyPartsInfo.equipItemsOnRecruit) 
						{
							// Only if showclassequipment is false AND they are currently naked 
							// People could still cheat, but it takes a bit more effort...  
							if ((null == this.npc?.Body?.Shirt?.MaybeItem) && !(this.npc.Body.Armor.HasArmor) && !(this.npc.UISettings.ShowClassEquipment))
                            {
								for (int i = 0; i < bodyPartsInfo.equipItemsOnRecruit.Length; i++) {
									BlueprintGuid deserializedGuid = BlueprintGuid.Parse(bodyPartsInfo.equipItemsOnRecruit[i]);
									if (null != deserializedGuid) { 
										BlueprintItem blueprintItem = (BlueprintItem)ResourcesLibrary.TryGetBlueprint(deserializedGuid);
										if (null!=blueprintItem) {
											try { 
												EquipUtil.equipOnNPC(this.npc, blueprintItem);
											} 
											catch (Exception e)
											{
												Log.trace($"Exception thrown attempting to equip items: {e.Message}");
											}
										}
									}
								}
                            }
							bodyPartsInfo.equipItemsOnRecruit = null; // Just in case...
						}
					}
					this.ruleContext.updateEquipped(this.npc);
					if (1 == this.ruleContext.USHORT_VALUES[(int)PROPERTY.IsNaked]) {
						npc.UISettings.ShowClassEquipment = true;
					}
				}
				evaluateRules();
				EventBus.Subscribe(this);
				this.npc.UISettings.SubscribeOnEquipmentVisibilityChange(this.OnEquipmentVisibilityChange);
			}
			catch (Exception ex)
			{
				Log.always($"mon:OnCompanionActivated: Exception Caught : {ex.ToString()}");
			}
		}

		// Called from CompanionPortraitEnablerMain.cs
		public void OnAreaActivated()
		{
            EventBus.Unsubscribe(this);
            EventBus.Subscribe(this);
			this.npc.UISettings.UnsubscribeFromEquipmentVisibilityChange(this.OnEquipmentVisibilityChange);
			this.npc.UISettings.SubscribeOnEquipmentVisibilityChange(this.OnEquipmentVisibilityChange);
			this.handleChangeAllFloodProtection = 1; // Jump in line...
			HandleChangeAllHelper();
		}

		// Called from UnitUISettings OnEquipmentVisibilityChange subscription (See constructor)
		public void OnEquipmentVisibilityChange()
        {
			Log.trace($"TRACE: [{this.Name}] OnEquipmentVisibilityChange Fired!");
			forceBodyRefresh=true;
			HandleEquippedChanges();
        }

		private bool updateInParty()
		{
			UnitPartCompanion unitPartCompanion = this.npc.Get<UnitPartCompanion>();
			if (unitPartCompanion != null && unitPartCompanion.State == CompanionState.InParty)
			{
				this.inParty = true;
			}
			else 
			{
				this.inParty = false;
			}
			return this.inParty;
		}

		// ------------------------------------------------------------------
		// Relay Handlers
		// ------------------------------------------------------------------
		// These are events that CompanionPortraitEnablerMain subsrcibes
		// to and relays to npc instances. We get these whether the NPCs
        // have rules or not. 
		// ------------------------------------------------------------------

		// Relayed from CompanionPortraitEnablerMain
		public void OnCompanionAdded()
		{
			if (!npc.IsInGame) {
				Log.trace($"TRACE: OnCompanionAdded called on [{this.Name}]. NPC is new/never before seen. Calling OnCompanionActivated.");
				OnCompanionActivated();
			} else { 
				Log.trace($"TRACE: OnCompanionAdded called on [{this.Name}]");
				this.updateInParty();
			}
		}

		// Relayed from CompanionPortraitEnablerMain
		public void OnCompanionRemoved(bool stayInGame) // : IPartyHandler
		{
			Log.trace($"TRACE: OnCompanionRemoved called on [{this.Name}] stayInGame [{stayInGame}]");
			this.updateInParty();
		}

		public void OnCompanionLevelUp()  // Relayed from CompanionPortraitEnablerMain
        {
			// Typically during level up, (particularly at camp), the NPC view is reset, but
			// our rules will think we are already displaying the right stuff and thus do 
			// nothing. So we have to set force-refresh (usually). 
			Log.trace($"TRACE: [{this.Name}] OnCompanionLevelUp(). Level is [{this.npc.Descriptor.Progression.CharacterLevel}]");
			this.forceBodyRefresh=true;
			this.forcePortraitRefresh=true;
			HandleChangeAll();
        }

		// Relayed from CompanionPortraitEnablerMain
		public void OnPolymorphEnd(Polymorph polymorph)
		{
			Log.trace($"TRACE: OnPolymorphEnd called on [{this.Name}] polymorph [{polymorph}]");
			this.forceBodyRefresh = true;
			this.HandleChangeAll();
		}

		public void OnDialogStart(BlueprintDialog dialogMeta)
        {
			HandleDialogChanges();
		}

		public void OnDialogEnd(BlueprintDialog dialogMeta, bool finishedWithoutCanceling) // IDialogFinishHandler
		{
			HandleDialogChanges();
		}

		// ------------------------------------------------------------------
		// Event Handlers
		// ------------------------------------------------------------------

		public void HandleUnitSizeChanged(UnitEntityData unit) { // IUnitSizeHandler
			if (null == unit) return;
			if (null == unit.CharacterName) return;
			if (unit.CharacterName != this.Name) {
				Log.trace($"TRACE: [{this.Name}]. HandleUnitSizeChanged bailing : [{this.Name}] != unit [{unit.CharacterName}]");
				return;
            }
			if (unit != this.npc)
            {
				Log.trace($"TRACE: Assertion warning : [{this.Name}]. HandleUnitSizeChanged : Entity comparison failure. (Not sure why?)");
            }
			if (unit.Descriptor != null && unit.Descriptor.State != null)
            {
				if (unit.Descriptor.OriginalSize == unit.Descriptor.State.Size)
				{
					Log.trace($"TRACE: [{this.Name}] HandleUnitSizeChanged. Returning to normal size. Requesting forced Body Refresh.");
					forceBodyRefresh=true;
					evaluateRules();
				} 
				else
				{
					Log.trace($"TRACE: [{this.Name}] HandleUnitSizeChanged. Changing size. Ignoring Event");
				}
            } 
			else
            {
				Log.trace($"TRACE: [{this.Name}] bailing: Unit does not have State");
            }
        }

        public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count) // : IItemsCollectionHandler
        {
			if (null == this.npc) return;
			ItemsCollection iCollection = null;

			Kingmaker.UnitLogic.UnitDescriptor ownerUnit = null;
			Kingmaker.UnitLogic.UnitDescriptor iownerUnit = null;
			if (null != collection) ownerUnit = collection.OwnerUnit;
			if (null != item) {
				iCollection = item.Collection;
				if (null != iCollection) iownerUnit = iCollection.OwnerUnit;
            }

			EntityDataBase npe = (EntityDataBase)this.npc;
			if (null != npe)
            {
				if (null != collection)
                {
					// Log.trace($"TRACE: Comparing Collection.OwnerRef.Entity [{collection.OwnerRef.Entity}] with npc as EntityDataBase [{npe}]");
					if (npe == collection.OwnerRef.Entity)
                    {
						Log.debug($"[{this.Name}] HandleItemsAdded called with item [{item}] count [{count}]. isOwner [true] (c-entities match) isSharedStash [{collection?.IsSharedStash}]"); 
						HandleInventoryChanges();
						return;
					}
				} 
				else if (null != iCollection)
				{ 
					// Log.trace($"TRACE: Comparing item.Collection.OwnerRef.Entity [{iCollection.OwnerRef.Entity}] with npc as EntityDataBase [{npe}]");
					if (npe == iCollection.OwnerRef.Entity)
                    {
						Log.debug($"[{this.Name}] HandleItemsAdded called with item [{item}] count [{count}]. isOwner [true] (i-entities match) isSharedStash [{collection?.IsSharedStash}]"); 
						HandleInventoryChanges();
						return;
					}
				} 
            } 
			else
            {
				Log.trace($"TRACE: HandleItemsAdded - npc cast to EntityDataBase failed");
            }

			if (null != ownerUnit)
            {
				// Log.trace($"TRACE: Comparing Collection OwnerUnit [{ownerUnit}] with npc.Descriptor [{this.npc.Descriptor}]");
				if (ownerUnit.ToString() == this.npc.Descriptor.ToString())
                {
					Log.debug($"[{this.Name}] HandleItemsAdded called with item [{item}] count [{count}]. isOwner [true] (c-descriptors match) isSharedStash [{collection?.IsSharedStash}]"); 
					HandleInventoryChanges();
					return;
                } 
            } 
			else if (null != iownerUnit)
            {
				// Log.trace($"TRACE: Comparing item.Collection.OwnerUnit [{iownerUnit}] with npc.Descriptor [{this.npc.Descriptor}]");
				if (iownerUnit.ToString() == this.npc.Descriptor.ToString())
                {
					Log.debug($"[{this.Name}] HandleItemsAdded called with item [{item}] count [{count}]. isOwner [true] (i-descriptors match) isSharedStash [{collection?.IsSharedStash}]"); 
					HandleInventoryChanges();
					return;
                } 
            }
			Log.trace($"TRACE: [{this.Name}] Ignoring added item [{item}] count [{count}]. isPlayerInventory [{collection?.IsPlayerInventory}]");
        }


		// Token: 0x060036E5 RID: 14053
		public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count) // : IItemsCollectionHandler
        {			
			if (null == this.npc) return;
			ItemsCollection iCollection = null;

			Kingmaker.UnitLogic.UnitDescriptor ownerUnit = null;
			Kingmaker.UnitLogic.UnitDescriptor iownerUnit = null;
			if (null != collection) ownerUnit = collection.OwnerUnit;
			if (null != item) {
				iCollection = item.Collection;
				if (null != iCollection) iownerUnit = iCollection.OwnerUnit;
            }

			EntityDataBase npe = (EntityDataBase)this.npc;
			if (null != npe)
            {
				if (null != collection)
                {
					// Log.trace($"TRACE: Comparing Collection.OwnerRef.Entity [{collection.OwnerRef.Entity}] with npc as EntityDataBase [{npe}]");
					if (npe == collection.OwnerRef.Entity)
                    {
						Log.debug($"[{this.Name}] HandleItemsRemoved called with item [{item}] count [{count}]. isOwner [true] (c-entities match) isSharedStash [{collection?.IsSharedStash}]"); 
						HandleInventoryChanges();
						return;
					}
				} 
				else if (null != iCollection)
				{ 
					// Log.trace($"TRACE: Comparing item.Collection.OwnerRef.Entity [{iCollection.OwnerRef.Entity}] with npc as EntityDataBase [{npe}]");
					if (npe == iCollection.OwnerRef.Entity)
                    {
						Log.debug($"[{this.Name}] HandleItemsRemoved called with item [{item}] count [{count}]. isOwner [true] (i-entities match) isSharedStash [{collection?.IsSharedStash}]"); 
						HandleInventoryChanges();
						return;
					}
				} 
            } 
			else
            {
				Log.trace($"TRACE: HandleItemsRemoved - npc cast to EntityDataBase failed");
            }

			if (null != ownerUnit)
			{
				// Log.trace($"TRACE: Comparing Collection OwnerUnit [{ownerUnit}] with npc.Descriptor [{this.npc.Descriptor}]");
				if (ownerUnit == this.npc.Descriptor)
				{
					Log.debug($"[{this.Name}] HandleItemsRemoved called with item [{item}] count [{count}]. isOwner [true] (c-descriptors match) isSharedStash [{collection?.IsSharedStash}]"); 
					HandleInventoryChanges();
					return;
				} 
			} 
			else if (null != iownerUnit)
			{
				// Log.trace($"TRACE: Comparing item.Collection.OwnerUnit [{iownerUnit}] with npc.Descriptor [{this.npc.Descriptor}]");
				if (iownerUnit == this.npc.Descriptor)
				{
					Log.debug($"[{this.Name}] HandleItemsRemoved called with item [{item}] count [{count}]. isOwner [true] (i-descriptors match) isSharedStash [{collection?.IsSharedStash}]"); 
					HandleInventoryChanges();
					return;
				} 
			} 

			Log.trace($"TRACE: [{this.Name}] Ignoring removed item [{item}] count [{count}] isPlayerInventory [{collection?.IsPlayerInventory}]");
        }

		public void HandleMythicSelectionComplete(UnitEntityData companion) // : IMythicSelectionCompleteUIHandler
		{
			if (null == companion) return;
			if (companion != this.npc) return;
			Log.trace($"TRACE: [{this.Name}] HandleMythicSelectionComplete for [{companion.Descriptor.CharacterName}]");
		}

		public void HandleQuestStarted(Quest quest) // :  IQuestHandler
        {
			if (Log.debugEnabled)
			{
				StringWriter sw = new StringWriter();
				sw.WriteLine("\n------------------------------------------------------------------------------");
				sw.WriteLine($"QuestStarted: \"prop\":\"ActiveQuests\", \"cond\":\"any\", \"value\":\"{quest}\"");
				sw.WriteLine("------------------------------------------------------------------------------");
				Log.debug(sw.ToString());
			}
			HandleQuestChanges();
        }

		public void HandleQuestCompleted(Quest quest) // : IQuestHandler
        {
			if (Log.debugEnabled)
			{
				StringWriter sw = new StringWriter();
				sw.WriteLine("\n------------------------------------------------------------------------------");
				sw.WriteLine($"QuestCompleted: \"prop\":\"CompletedQuests\", \"cond\":\"any\", \"value\":\"{quest}\"");
				sw.WriteLine("------------------------------------------------------------------------------");
				Log.debug(sw.ToString());
			}
			HandleQuestChanges();
        }

		public void HandleQuestFailed(Quest quest) // : IQuestHandler
        {
			if (Log.debugEnabled)
			{
				StringWriter sw = new StringWriter();
				sw.WriteLine("\n------------------------------------------------------------------------------");
				sw.WriteLine($"QuestFailed: \"prop\":\"FailedQuests\", \"cond\":\"any\", \"value\":\"{quest}\"");
				sw.WriteLine("------------------------------------------------------------------------------");
				Log.debug(sw.ToString());
			}
			HandleQuestChanges();
        }

		public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem) // : IUnitEquipmentHandler
		{ 
			if (slot.Owner.Unit != this.npc) return;
			Log.trace($"TRACE: [{this.Name}] - HandleEquipmentSlotUpdated detected");
			HandleEquippedChanges();
			// Slot changes also affect who's inventory is considered whos... 
			// However inventory updates are delayed. So rules wont evaluate
			// until after the window closes. 
			HandleInventoryChanges();
		}
		//-------------------------------------------------------------------
		public UnitEntityData GetSubscribingUnit() // : IUnitSubscriber
		{
			// This callback used by several interfaces to get the NPC to associate events with.
			return this.npc;
		}

		public void HandleDamageDealt(RuleDealDamage damageMeta) // IDamageHandler
		{
			Log.trace($"TRACE: [{this.Name}] HandleDamageDealt() Called");
			if (damageMeta.IsFake)
            {
				Log.trace($"TRACE: [{this.Name}] Damage is Fake... ");
				return;
            }
			if (Log.traceEnabled)
			{ 
				if (damageMeta.GetRuleTarget() == this.npc)
				{ 
					Log.trace($"TRACE: [{this.Name}] Received [{damageMeta.DamageWithoutReduction}]/[{damageMeta.DamageBeforeDifficulty}]/[{damageMeta.Result}] damage. [{this.npc.HPLeft}] HP remaining");
				}
  			    else
				{
				    Log.trace($"TRACE: [{this.Name}] Received [{damageMeta.DamageWithoutReduction}]/[{damageMeta.DamageBeforeDifficulty}]/[{damageMeta.Result}] damage. [{this.npc.HPLeft}] HP remaining. Target was [{damageMeta.GetRuleTarget().CharacterName}] (Area effect damage?)");
				}
            }
			HandleHealthChanges();
		}

		public void HandleHealing(RuleHealDamage healMeta)
		{
			Log.trace($"TRACE: [{this.Name}] HandleHealing() Called");
			if (healMeta.IsFake)
            {
				return;
            }
			if (healMeta.GetRuleTarget() == this.npc)
			{ 
				if (Log.traceEnabled)
                {
					Log.trace($"TRACE: [{this.Name}] Healed [{healMeta.ValueWithoutReduction}]/[{healMeta.Value}] HPs. HP now at [{this.npc.HPLeft}]");
                }
            }
			HandleHealthChanges();
		}

		public void HandlePartyGainExperience(int gained) // :  IPartyGainExperienceHandler
		{
			Log.trace($"TRACE: [{this.Name}] HandlePartyGainExperience({gained})");
			HandleStatChanges();
		}

		public void HandleShowResults() // : IRestCampUIHandler
		{
			// Player just finished resting and the results are being displayed...
			// Good time to check things that change/reset when we rest like Buffs,
            // acuity, corruption and HP 
			Log.trace($"TRACE: [{this.Name}] HandleShowResults()");
			HandleChangeAll();
		}

		public void HandleUnitCalculateSkillPointsOnLevelup(LevelUpState state, ref int extraSkillPoints) // : IUnitCalculateSkillPointsOnLevelupHandler
		{
			// Uses CallBack "GetSubscribingUnit()" to determine who the event is about.
			// I think this only gets called when level up results in new Skill points for distribution, though
			// it might get called before the player has accepted the changes....
			Log.trace($"TRACE: [{this.Name}] HandleUnitCalculateSkillPointsOnLevelup. NextClassLevel [{state.NextClassLevel}]");
		}

		public void HandleRecruit(UnitEntityData companion) // :  ICompanionChangeHandler
		{
			if (null == companion) return;
			if (companion != this.npc) return;
			Log.trace($"TRACE: [{this.Name}] HandleRecruit for [{companion.Descriptor.CharacterName}]");
			HandleChangeAll();
		}

		public void HandleUnrecruit(UnitEntityData companion) // :  ICompanionChangeHandler
		{
			if (null == companion) return;
			if (companion != this.npc) return;
			Log.trace($"TRACE: [{this.Name}] HandleUnrecruit for [{companion.Descriptor.CharacterName}]");
			this.updateInParty();
		}

		// I assume this is specific to the player
		public void HandleIncreaseCorruption() // : ICorruptionLevelHandler
		{
			Log.trace($"TRACE: [{this.Name}] HandleIncreaseCorruption()");
			HandleStatChanges();
		}

		// I assume this is specific to the player
		public void HandleClearCorruption() // : ICorruptionLevelHandler
		{
			Log.trace($"TRACE: [{this.Name}] HandleClearCorruption()");
			HandleStatChanges();
		}

		public void HandleDecreaseCorruption() // : ICorruptionLevelHandler
		{ 
			Log.debug($"HandleDecreaseCorruption()"); 
			HandleStatChanges();
		} 		

		public void HandlePartyCombatStateChanged(bool inCombat) // : IPartyCombatHandler
		{
			Log.trace($"TRACE: [{this.Name}] HandlePartyCombatStateChanged");
			if (inCombat)
            {
				HandleCombatStart();
            } 
			else
            {
				HandleCombatEnd();
            }
			// Do Stuff?
		}

		public void HandleAttributeDamage(UnitEntityData unit, StatType attribute, int oldDamage, int newDamage, bool drain) // : IAttributeDamageHandler
		{
			if (null == unit) return;
			if (unit != this.npc) return;
			Log.trace($"TRACE: [{this.Name}] OnAttributeDamaged for [{unit.Descriptor.CharacterName}] attribute [{attribute}] oldDamage [{oldDamage}] newDamage [{newDamage}] drain[{drain}]");
			HandleStatChanges();
		}

		public void HandleUnitGainFact(EntityFact fact) // : IUnitGainFactHandler
		{
			// Uses CallBack "GetSubscribingUnit()" to determine who the event is about.
			if (null != fact)
			{
				Log.debug($"[{this.Name}] HandleUnitGainFact [{fact}]");
			}
			else
            {
				Log.debug($"[{this.Name}] HandleUnitGainFact [null]");
			}
			HandleFactChanges();
		}

		public void HandleUnitLostFact(EntityFact fact) // : IUnitLostFactHandler
        {
			// Uses CallBack "GetSubscribingUnit()" to determine who the event is about.
			if (null != fact)
			{
				Log.trace($"TRACE: [{this.Name}] HandleUnitLostFact [{fact}]");
			}
			else
            {
				Log.trace($"TRACE: [{this.Name}] HandleUnitLostFact [null]");
			}
			HandleFactChanges();
        }


		public void HandleAlignmentChange(UnitEntityData unit, Kingmaker.Enums.Alignment newAlignment, Kingmaker.Enums.Alignment prevAlignment) // : IAlignmentChangeHandler
		{
			if (null == unit) return;
			if (unit != this.npc) return;
			Log.trace($"TRACE: [{this.Name}] HandleAlignmentChange for [{unit.Descriptor.CharacterName}] newAlignment [{newAlignment}] prevAlignment [{prevAlignment}]");
			HandleStatChanges();
		}

		// ------------------------------------------------------------------
		// IGNORED EVENTS            (Required by Interface, but not used...)
		// Log.debug		
		public void HandleSkipPhase() { Log.debug("HandleSkipPhase()"); } // IRestCampUIHandler
		public void HandleOpenRestCamp() { Log.debug("HandleOpenRestCamp()"); } // IRestCampUIHandler
		public void HandleVisualCampPhaseFinished() { Log.debug("HandleVisualCampPhaseFinished()"); } // IRestCampUIHandler
		public void HandleCloseRestCamp() { Log.debug("HandleCloseRestCamp()"); } // IRestCampUIHandler


		public string ToString(Boolean includeShared)
		{
			StringWriter sw = new StringWriter();
			if (includeShared)
            {
				sw.WriteLine(this.ruleContext.SharedToString());
			}

			string currentPortraitRuleStr = this.currentPortraitRule?.portraitId;
			if (null == currentPortraitRuleStr)
            {
				BlueprintPortrait bPortrait = this.npc?.UISettings?.PortraitBlueprint ?? this.npc?.UISettings?.Owner?.Blueprint?.PortraitSafe;
				currentPortraitRuleStr = (bPortrait?.name ?? bPortrait?.ToString());
				if (null == currentPortraitRuleStr) currentPortraitRuleStr = "Default";

            }
			string currentBodyRuleStr = this.currentBodyRule?.fileName ?? this.defaultBodyPath;
			if (null == currentBodyRuleStr) currentBodyRuleStr = "Default";

			sw.WriteLine("");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine($"Companion [{this.Name}]");
			sw.WriteLine($"Portrait  [{currentPortraitRuleStr}]");
			sw.WriteLine($"Body      [{currentBodyRuleStr}]");
			sw.WriteLine(this.ruleContext.ToString());
			return sw.ToString();
		}

		public static int checkBail(UnitEntityData npc,  bool bailNonCompanion = true, bool bailDetachedCompanion = false, bool bailExCompanion=true, bool bailRemoteCompanion = true)
        {
			if (npc == null) return 1;
			if (!npc.IsInGame) return 2;
			if (npc.Suppressed) return 3;
			// Are they visible/present on current map?
			//   Kingmaker.Blueprints.Area.AreaService.Instance.IsInMechanicBounds(npc.Position))
            UnitPartCompanion unitPartCompanion = npc.Get<UnitPartCompanion>();
			if (null == unitPartCompanion) {
				if (bailNonCompanion) return 4;
				return 0;
			}
			CompanionState cState = unitPartCompanion.State;
			if (cState == CompanionState.None) return 5;
			if ((cState == CompanionState.ExCompanion) && bailExCompanion) return 6;
			if ((cState == CompanionState.Remote) && bailRemoteCompanion) {
				// REMOTE: Means they are not part of the current party (standing around at camp). However, the player
				// may be at camp, so it doesn't rule out the possibility of their presence without another check:
				if (!Kingmaker.Blueprints.Area.AreaService.Instance.IsInMechanicBounds(npc.Position)) { 
					return 7;
				}
			}
			if ((cState == CompanionState.InPartyDetached) && bailDetachedCompanion) {
				// Deteched means ... not currently selected. IE: If you click on an enemy, detached followers will ignore you. 
				// When you are at camp, you can't select anyone other than the player, so all followers are detached in that case.
				return 8;
			}
			return 0;
        }

		public static string getBailReason(int failValue)
        {
			if (1 == failValue) return "Npc/arguement was null";
			if (2 == failValue) return "Npc is not in the game";
			if (3 == failValue) return "Npc is suppressed/unavailable";
			if (4 == failValue) return "Npc is not a companion option";
			if (5 == failValue) return "Npc companion state is NONE (You haven't met them yet)";
			if (6 == failValue) return "Npc left the party";
			if (7 == failValue) return "Npc is not in the current map";
			if (8 == failValue) return "Npc is detached (whatever that means)";
			return "UNKNOWN";
        }

		// ------------------------------------------------------------------
		// Event Flooding Prevention Helpers
		// ------------------------------------------------------------------

		public int HandleHealthChangeFloodProtection    = 0;
		public int HandleStatChangeFloodProtection      = 0;
		public int handleChangeAllFloodProtection       = 0;
		public int HandleFactChangeFloodProtection      = 0;
		public int HandleQuestChangeFloodProtection     = 0;
		public int HandleInventoryChangeFloodProtection = 0;
		public int HandleCombatEndFloodProtection       = 0;

		public void HandleHealthChanges() {
			if (!this.inParty && !this.updateInParty()) {
				if (Log.traceEnabled) {
					Log.trace($"TRACE: [{this.Name}] HandleHealthChanges Called. Npc not in party.");
				}
				return;
			}
			if (Log.traceEnabled) {
				Log.trace($"TRACE: [{this.Name}] HandleHealthChanges Called.");
			}

			int snapshot = this.HandleHealthChangeFloodProtection;
			this.HandleHealthChangeFloodProtection += 1;
			if (0 == snapshot)
            {
		        UniRx.Scheduler.MainThread.Schedule(OneSecond, this.HandleHealthChangeHelper);
            } 
			else if (snapshot > 25)
			{
				HandleHealthChangeHelper();
            }
		}

		public void HandleCombatStart() 
		{
			if (!this.inParty && !this.updateInParty()) {
				if (Log.traceEnabled) {
					Log.trace($"TRACE: [{this.Name}] HandleCombatStart Called. Npc not in party.");
				}
				return;
			}
			if (Log.traceEnabled) {
				Log.trace($"TRACE: [{this.Name}] HandleCombatStart Called.");
			}
			this.ruleContext.updateCombatState(this.npc);
			evaluateRules();
		}

		public void HandleCombatEnd() 
		{
			if (!this.inParty && !this.updateInParty()) {
				if (Log.traceEnabled) {
					Log.trace($"TRACE: [{this.Name}] HandleCombatEnd Called. Npc not in party.");
				}
				return;
			}
			if (Log.traceEnabled) {
				Log.trace($"TRACE: [{this.Name}] HandleCombatEnd Called.");
			}
			int snapshot = this.HandleCombatEndFloodProtection;
			this.HandleCombatEndFloodProtection += 1;
			if (0 == snapshot)
            {
		        UniRx.Scheduler.MainThread.Schedule(OneSecond, this.HandleCombatEndHelper);
            } 
			else if (snapshot > 5)
			{
				HandleCombatEndHelper();
            }
		}

		public void HandleDialogChanges() 
		{
			if (0 != checkBail(this.npc)) { 
				if (Log.traceEnabled) {
					Log.trace($"TRACE: [{this.Name}] HandleDialogChanges Called - {getBailReason(checkBail(this.npc))} : Bailing.");
                }
				return;
			}
			if (Log.traceEnabled) {
				Log.trace($"TRACE: [{this.Name}] HandleDialogChanges Called");
			}

			// Can't delay on Dialog events even for a few ms as the game pauses once
            // the dialog starts, preventing scheduled actions from taking place.
            // Can't allow the thread to return until the portrait has been updated
            // (If an update is needed). Fortunately, I am unaware of any issues 
			// with dialog event flooding.

			this.ruleContext.updateDialog(this.npc);
			evaluateRules();
		}

		public void HandleQuestChanges() {
			if (0 != checkBail(this.npc)) { 
				if (Log.traceEnabled) {
					Log.trace($"TRACE: [{this.Name}] HandleQuestChanges Called - {getBailReason(checkBail(this.npc))} : Bailing.");
                }
				return;
			}
			if (Log.traceEnabled) {
				Log.trace($"TRACE: [{this.Name}] HandleQuestChanges Called");
			}

			int snapshot = this.HandleQuestChangeFloodProtection;
			this.HandleQuestChangeFloodProtection += 1;
			if (0 == snapshot)
            {
		        UniRx.Scheduler.MainThread.Schedule(OneSecond, this.HandleQuestChangeHelper);
            } 
			else if (snapshot > 15)
			{
				HandleQuestChangeHelper();
            }
		}

		public void HandleInventoryChanges() {

			if (0 != checkBail(this.npc)) { 
				if (Log.traceEnabled) {
					Log.trace($"TRACE: [{this.Name}] HandleInventoryChanges Called - {getBailReason(checkBail(this.npc))} : Bailing.");
                }
				return;
			}
			if (Log.traceEnabled) {
				Log.trace($"TRACE: [{this.Name}] HandleInventoryChanges Called");
			}

			int snapshot = this.HandleInventoryChangeFloodProtection;
			this.HandleInventoryChangeFloodProtection += 1;
			if (0 == snapshot)
            {
		        UniRx.Scheduler.MainThread.Schedule(OneSecond, this.HandleInventoryChangeHelper);
            } 
			else if (snapshot > 15)
			{
				HandleInventoryChangeHelper();
            }
		}

		public void HandleEquippedChanges()
        {
			// Can't delay on Equipment Changes. These changes take place from the 
			// Fullscreen Doll UI, which puts the game in a paused state. So once
			// this thread returns, queued actions wont proceed until they close
			// the inventory UI. 

			if (0 != checkBail(this.npc)) { 
				if (Log.traceEnabled) {
					Log.trace($"TRACE: [{this.Name}] HandleEquippedChanges Called - {getBailReason(checkBail(this.npc))} : Bailing.");
                }
				return;
			}
			if (Log.traceEnabled) {
				Log.trace($"TRACE: [{this.Name}] HandleEquippedChanges Called");
			}

			// if (!this.inParty && !this.updateInParty()) {
			//	  return;
			// }

			this.ruleContext.updateBase(this.npc);  //Must be called before Stats
			this.ruleContext.updateBuffs(this.npc); //Must be called before Stats
			this.ruleContext.updateStats(this.npc);
			this.ruleContext.updateEquipped(this.npc);
			evaluateRules();
        }

		public void HandleStatChanges()
        {
			if (0 != checkBail(this.npc)) { 
				if (Log.traceEnabled) {
					Log.trace($"TRACE: [{this.Name}] HandleStatChanges Called - {getBailReason(checkBail(this.npc))} : Bailing.");
                }
				return;
			}
			if (Log.traceEnabled) {
				Log.trace($"TRACE: [{this.Name}] HandleStatChanges Called");
			}

			int snapshot = this.HandleStatChangeFloodProtection;
			this.HandleStatChangeFloodProtection += 1;
			if (0 == snapshot)
            {
		        UniRx.Scheduler.MainThread.Schedule(OneSecond, this.HandleStatChangeHelper);
            } 
			else if (snapshot > 15)
			{
				HandleStatChangeHelper();
            }
        }

		public void HandleChangeAll()
        {
			if (0 != checkBail(this.npc)) { 
				if (Log.traceEnabled) {
					Log.trace($"TRACE: [{this.Name}] HandleChangeAll Called - {getBailReason(checkBail(this.npc))} : Bailing.");
                }
				return;
			}
			if (Log.traceEnabled) {
				Log.trace($"TRACE: [{this.Name}] HandleChangeAll Called");
			}

			int snapshot = this.handleChangeAllFloodProtection;
			this.handleChangeAllFloodProtection += 1;
			if (0 == snapshot)
            {
		        UniRx.Scheduler.MainThread.Schedule(OneSecond, this.HandleChangeAllHelper);
            } 
			else if (snapshot > 15)
			{
				HandleChangeAllHelper();
            }
        }

		public void HandleFactChanges()
        {
			if (!this.inParty && !this.updateInParty()) {
				if (Log.traceEnabled) {
					Log.trace($"TRACE: [{this.Name}] HandleFactChanges Called. Npc not in party.");
				}
				return;
			}
			if (Log.traceEnabled) {
				Log.trace($"TRACE: [{this.Name}] HandleFactChanges Called.");
			}

			int snapshot = this.HandleFactChangeFloodProtection;
			this.HandleFactChangeFloodProtection += 1;
			if (0 == snapshot)
            {
		        UniRx.Scheduler.MainThread.Schedule(OneSecond, this.HandleFactChangeHelper);
            } 
			else if (snapshot > 50)
			{
				HandleFactChangeHelper();
            }
        }

		public void HandleHealthChangeHelper() 
        {
			Log.trace($"TRACE: [{this.Name}] HandleHealthChangeHelper Called. [{this.HandleHealthChangeFloodProtection}] events ignored during delay.");
			if (0 == HandleHealthChangeFloodProtection) return;
			this.HandleHealthChangeFloodProtection = 0;
			this.ruleContext.updateHealth(this.npc);
			evaluateRules();
        }

		public void HandleCombatEndHelper() 
        {
			Log.trace($"TRACE: [{this.Name}] HandleCombatEndHelper Called. [{this.HandleCombatEndFloodProtection}] events ignored during delay.");
			if (0 == HandleCombatEndFloodProtection) return;
			this.HandleCombatEndFloodProtection = 0;
			this.ruleContext.updateCombatState(this.npc);
			evaluateRules();
        }

		public void HandleQuestChangeHelper() 
        {
			Log.trace($"TRACE: [{this.Name}] HandleQuestChangeHelper Called. [{this.HandleQuestChangeFloodProtection}] events ignored during delay.");
			if (0 == HandleQuestChangeFloodProtection) return;
			this.HandleQuestChangeFloodProtection = 0;
			this.ruleContext.updateQuests(this.npc);
			evaluateRules();
        }
		
		public void HandleInventoryChangeHelper() 
        {
			Log.trace($"TRACE: [{this.Name}] HandleInventoryChangeHelper Called. [{this.HandleInventoryChangeFloodProtection}] events ignored during delay.");
			if (0 == HandleInventoryChangeFloodProtection) return;
			this.HandleInventoryChangeFloodProtection = 0;
			this.ruleContext.updateInventory(this.npc);
			evaluateRules();
        }

		public void HandleStatChangeHelper()
        {
			Log.trace($"TRACE: [{this.Name}] HandleStatChangeHelper Called.[{this.HandleStatChangeFloodProtection}] events ignored during delay.");
			if (0 == HandleStatChangeFloodProtection) return;
			this.HandleStatChangeFloodProtection = 0;
			this.ruleContext.updateBase(this.npc);  //Must be called before Stats
			this.ruleContext.updateBuffs(this.npc); //Must be called before Stats
			this.ruleContext.updateStats(this.npc);
			evaluateRules();
        }

		public void HandleChangeAllHelper()
        {
			Log.trace($"TRACE: [{this.Name}] HandleChangeAllHelper Called. [{this.handleChangeAllFloodProtection}] events ignored during delay.");
			if (0 == handleChangeAllFloodProtection) return;
			this.handleChangeAllFloodProtection = 0;
			this.ruleContext.updateArea(this.npc);
			this.ruleContext.updateBase(this.npc);  //Must be called before Stats
			this.ruleContext.updateBuffs(this.npc); //Must be called before Stats
			this.ruleContext.updateStats(this.npc);
			this.ruleContext.updateFacts(this.npc);
			this.ruleContext.updateClasses(this.npc);
			this.ruleContext.updateEquipped(this.npc);				
			this.ruleContext.updateInventory(this.npc);
			this.ruleContext.updateQuests(this.npc);
			this.ruleContext.updateHealth(this.npc);
			evaluateRules();
        }

		public void HandleFactChangeHelper()
        {
			Log.trace($"TRACE: [{this.Name}] HandleFactChangeHelper Called. [{this.HandleFactChangeFloodProtection}] events ignored during delay.");
			if (0 == HandleFactChangeFloodProtection) return;
			this.HandleFactChangeFloodProtection = 0;
			this.ruleContext.updateFacts(this.npc);
			evaluateRules();
        }

		public void destroy()
        {
			Log.trace($"TRACE: [{this.Name}] destroy called. Unsubscribing from events:");
            EventBus.Unsubscribe(this);
			this.npc.UISettings.UnsubscribeFromEquipmentVisibilityChange(this.OnEquipmentVisibilityChange);
			Log.trace($"TRACE: [{this.Name}] destroy called. Clearing Rule Cache");
			if (null != allPortraitRules) allPortraitRules.Clear();
		    if (null != allBodyRules) allBodyRules.Clear();
			ruleContext.destroy();

			// This shouldn't be needed, but we do it anyway
			Log.trace($"TRACE: [{this.Name}] destroy called. Resetting Instance Variables");
			this.bodyScale = -1.0f;
			this.allPortraitRules = null;
			this.allBodyRules = null;
			this.ruleContext = null;
			this.Name = "";
			this.currentPortraitRule = null;
			// Reset Cache variables
			this.currentBody     = null;
			this.currentBodyRule = null;
			this.defaultBody     = null;
			this.defaultBodyPath = null;


			this.previouslyNaked = false;
			this.previouslyDefault = false;
			this.resourceHome = null;
			this.portraitPath        = "Default";
			this.portraitFileLarge   = "Default";
			this.portraitFileMedium  = "Default";
			this.portraitFileSmall   = "Default";
			this.lockId = -1;
			this.forceBodyRefresh = false;
			this.forcePortraitRefresh = false;
			this.evfloodProtection = -1;
			this.evThrottleGear = 0;		


			Log.trace($"TRACE: [{this.Name}] destroy called. Resetting Static Variables");

			NPCMonitor.portraitsRoot  = null;
			NPCMonitor.npcSubDir      = null;
			NPCMonitor.settings       = null;
			NPCMonitor.ChangeShapeKitsuneAToggleBlueprint = null;
			NPCMonitor.ChangeShapeKitsuneBToggleBlueprint = null;
        }
	}
}

