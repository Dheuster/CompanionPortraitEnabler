using System;
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
using Owlcat.Runtime.Core.Logging;     // Required for LogChannel
using Kingmaker.EntitySystem.Stats;    // Required for StatType
using Kingmaker.EntitySystem;          // Required for EntityFact
using Kingmaker.UnitLogic.Parts;       // Required for UnitPartCompanion
using Kingmaker.UnitLogic.Abilities.Blueprints; // Required for BlueprintAbility
using Kingmaker.UnitLogic.Class.LevelUp; // Required for LevelUpState
using Kingmaker.Blueprints.Root;         // Required for BlueprintRoot
using Kingmaker.Items.Slots;             // Required for ArmorSlot/HandSlot/EquipmentSlot
using Kingmaker;                         // Required for Game
using Kingmaker.ResourceLinks;           // Required for WeakResourceLink, SpriteLink
using UnityEngine;                       // Required for Sprite
using Kingmaker.RuleSystem.Rules.Damage; // Required for RuleDealDamage
using Kingmaker.DialogSystem.Blueprints; // Required for BlueprintDialogue
using Kingmaker.AreaLogic.Cutscenes;     // Required for CutsceneControlledUnit (to detect if npc is in dialogue with player...)



// using OwlcatModification.Modifications.NPCCustomPortraitEnabler.Utility;



// Investigate:
// Kingmaker.UI.MVVM._VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.RaceGenderAlignment
//
// Unity has something called a ReactiveProperty, which is a property with pub/sub characteristics
// so that external entities can subscribe to changes. A simple IObserver interface provides a
// callback that informs all oberservers when a change takes place. The Dispose method provides
// a deletion safe call that supresses the Garbage Collector until the object has informed all
// observers of the event. 

namespace OwlcatModification.Modifications.NPCCustomPortraitEnabler.Rules
{
	// These are the attributes we track for each NPC. They provide the keys and values
	// that rules can be based on

	// TODO:
	//   Consider Tracking Quests or offering a Quest Underway or Completed condition
	//   I remember seeing a subscription setup when tracking through the char level up manager when 
	//   looking for Doll related functions. I may be worth trying to find that code. 

	public class NPCMonitor : IUnitSubscriber,      // GetSubscribingUnit (To identify subscriber)
		 IPartyGainExperienceHandler,               // HandlePartyGainExperience
		 IOwnerGainLevelHandler,                    // HandleUnitGainLevel
		 IRestCampUIHandler,                        // HandleShowResults (Rest/Camp results)
		 IUnitCalculateSkillPointsOnLevelupHandler, // HandleUnitCalculateSkillPointsOnLevelup
		 ICompanionChangeHandler,                   // HandleRecruit, HandleUnrecruit
		 ICorruptionLevelHandler,                   // HandleIncreaseCorruption, HandleClearCorruption
		 IPartyHandler,                             // HandleAddCompanion, HandleCompanionActivated, HandleCompanionRemoved
		 IPartyCombatHandler,                       // HandlePartyCombatStateChanged
		 IAttributeDamageHandler,                   // HandleAttributeDamage
		 IUnitGainFactHandler,                      // HandleUnitGainFact
		 IAlignmentChangeHandler,                   // HandleAlignmentChange
		 IDamageHandler,                            // HandleDamageDealt
		 IDialogStartHandler,                       // HandleDialogStarted
		 IDialogFinishHandler,                      // HandleDialogFinished
		 IHealingHandler                            // HandleHealing
	{
		// static/const
		public static Kingmaker.Modding.OwlcatModification Modification { get; private set; }
		public static LogChannel Logger => Modification.Logger;
		public static Boolean m_logDebug = false;
		public static long EventFloodGate = 1001;

		// ------------------------------------------------------------------
		// Non-Rule Variables
		// ------------------------------------------------------------------
		private UnitEntityData npc;
		private long nextAcceptedEvent = 0;
		private bool inParty = false;

		protected RuleContext ruleContext;

		// ------------------------------------------------------------------
		// Basics
		// ------------------------------------------------------------------
		public string Name               = "";
		public string currentRule        = "Default";
		public string portraitName       = "Default";
		public string portraitFileLarge  = "Default";
		public string portraitFileMedium = "Default";
		public string portraitFileSmall  = "Default";

		// ------------------------------------------------------------------
		// Constructor
		// ------------------------------------------------------------------
		public NPCMonitor(UnitEntityData unit, Kingmaker.Modding.OwlcatModification modification, Boolean debug = false)
		{
			if (null == NPCMonitor.Modification)
			{
				NPCMonitor.Modification = modification;
				NPCMonitor.m_logDebug = debug;
			}
			this.npc = unit;
			this.Name = this.npc.CharacterName;

			if (null != this.npc.UISettings.PortraitBlueprintRaw)
			{
				// this.portraitName = this.npc.UISettings.PortraitBlueprintRaw.ToString();
				this.portraitName = (this.npc.UISettings.PortraitBlueprintRaw.Data.m_PortraitImage as WeakResourceLink<Sprite>).AssetId;
			}
			else if (null != this.npc.UISettings.CustomPortraitRaw)
			{
				this.portraitName = this.npc.UISettings.CustomPortraitRaw.CustomId;
			}
			else
			{
				logDebug($"[{this.Name}] PortraitBlueprint and CustomPortrait are null");
			}
			this.ruleContext = new RuleContext(modification, debug, this.npc);
			if (this.updateInParty() || debug)
			{
				logAlways(this.ToString());
			}

			// Register For events on Construction:
			EventBus.Subscribe(this);
		}

		// Called from NPCCustomPortraitEnablerMain.cs
		public void OnAreaActivated()
		{
			this.ruleContext.updateBase(this.npc);
			this.ruleContext.updateBuffs(this.npc);
			this.ruleContext.updateStats(this.npc);
			this.ruleContext.updateFacts(this.npc);
			this.ruleContext.updateClasses(this.npc);
			this.ruleContext.updateEquipped(this.npc);
			this.ruleContext.updateInventory(this.npc);
			this.ruleContext.updateHealth(this.npc);
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
		// Event Handlers
		// ------------------------------------------------------------------

		public UnitEntityData GetSubscribingUnit() // : IUnitSubscriber
		{
			// This callback used by several interfaces to get the NPC to associate events with.
			return this.npc;
		}

        /*
		public static Dictionary<MethodBase, Tuple<Timer,object[]> TimerLookup = new Dictionary<MethodBase, Tuple<Timer, object[]>();

		public static RegisterForSingleUpdate(Object clazz, float timeout, String handle, params object[] args)
        {
			clazz.GetType().GetProperties().Select(p => p.Name == handle)
			MethodBase callback = System.Reflection.GetMethodFromHandle(handle);
			if TimerLookup.contains(callback)
            {
				// Renew Timer
			}
			else
            {

            }

		}
		*/
		public void HandleDamageDealt(RuleDealDamage damageMeta) // IDamageHandler
		{
			if (damageMeta.IsFake)
            {
				return;
            }
			if (damageMeta.GetRuleTarget() == this.npc)
			{ 
				if (NPCMonitor.m_logDebug)
                {
					logDebug($"[{this.Name}] Received [{damageMeta.DamageWithoutReduction}]/[{damageMeta.DamageBeforeDifficulty}]/[{damageMeta.Result}] damage. [{this.npc.HPLeft}] HP remaining");
                }
            }
		}

		// TODO : If we register for Dialog start AND end, we could have a rule that shows a portrait for a 
		//        specific dialog while that dialog in running. 
		public void HandleDialogStarted(BlueprintDialog dialogMeta)
        {
			if (null != dialogMeta)
            {

				// ProtoType: When dialogue kicks off, attempt to reload/reset portrait from disk (in case it has changed)
				logDebug($"HandleDialogStarted: Attempting Portrait Reload");
				this.npc.UISettings.SetPortrait(this.npc.Portrait);
				this.npc.Portrait.EnsureImages();
				logDebug($"HandleDialogStarted: Flagging UI as needing Refresh");

				if (NPCMonitor.m_logDebug)
                {
					// Kingmaker.DialogSystem.Blueprints.DialogueType enum = [Common,Book,Interchapter,Epilogue] 
					if (CutsceneControlledUnit.GetControllingPlayer(this.npc))
					{
						logDebug($"HandleDialogStarted: dialog is [{dialogMeta}] Type is [{dialogMeta.Type}] In dialogue with [{this.Name}] [True]");
					}
					else
					{
						logDebug($"HandleDialogStarted: dialog is [{dialogMeta}] Type is [{dialogMeta.Type}] In dialogue with [{this.Name}] [False]");
					}
                }
			}
			else
            {
				logDebug("HandleDialogStarted: Dialog started but metadata was null");
            }
		}

		public void HandleDialogFinished(BlueprintDialog dialogMeta, bool finishedWithoutCanceling) // IDialogFinishHandler
		{
			if (null != dialogMeta)
            {
				if (NPCMonitor.m_logDebug)
                {
					// Kingmaker.DialogSystem.Blueprints.DialogueType enum = [Common,Book,Interchapter,Epilogue] 
					if (CutsceneControlledUnit.GetControllingPlayer(this.npc))
					{
						logDebug($"HandleDialogFinished: dialog is [{dialogMeta}] Type is [{dialogMeta.Type}] result = [{finishedWithoutCanceling}] In dialogue with player [True]");
					}
					else
					{
						logDebug($"HandleDialogFinished: dialog is [{dialogMeta}] Type is [{dialogMeta.Type}] result = [{finishedWithoutCanceling}] In dialogue with player [False]");
					}
                }
			}
			else
            {
				logDebug("HandleDialogFinished: Dialog ended but metadata was null");
            }
		}

		public void HandleHealing(RuleHealDamage healMeta)
		{
			logDebug($"[{this.Name}] HandleHealing() Called");
			if (healMeta.IsFake)
            {
				return;
            }
			if (healMeta.GetRuleTarget() == this.npc)
			{ 
				if (NPCMonitor.m_logDebug)
                {
					logDebug($"[{this.Name}] Healed [{healMeta.ValueWithoutReduction}]/[{healMeta.Value}] HPs. HP now at [{this.npc.HPLeft}]");
                }
            }
		}

		// TODO : Try disconnecting this from the MAIN/Relay and see if we can get individual events?
		public void HandlePartyGainExperience(int gained) // :  IPartyGainExperienceHandler
		{
			logDebug($"[{this.Name}] HandlePartyGainExperience({gained})");
			if (!this.inParty && !this.updateInParty()) return;

			// RegisterForSingleUpdate(24.0f, HandlePartyGainExperienceHelper, )
			// Note: Game settings may allow XP to spread to everyone ... or only the NPC who
			// performed the action. For example, picking locks. Does everyone get XP or just
			// the thief?

            // TimerTools.ScheduleInvocation(500,this.HandlePartyGainExperienceHelper,gained);

			if (DateTimeOffset.Now.ToUnixTimeMilliseconds() < nextAcceptedEvent)
			{
				  logDebug($"HandlePartyGainExperience Ignored: (Event Flooding)");
				  return;
			}
			nextAcceptedEvent = DateTimeOffset.Now.ToUnixTimeMilliseconds() + EventFloodGate;

		}

		public void HandlePartyGainExperienceHelper(int gained)
        {
			logDebug($"[{this.Name}] HandlePartyGainExperienceHelper({gained})");

        }

		public void HandleShowResults() // : IRestCampUIHandler
		{
			// Player just finished resting and the results are being displayed...
			// Good time to check things that change/reset when we rest like Buffs,
            // acuity, corruption and HP 
			if (!this.inParty && !this.updateInParty()) return;
			logDebug($"[{this.Name}] HandleShowResults()");
		}

		public void HandleUnitCalculateSkillPointsOnLevelup(LevelUpState state, ref int extraSkillPoints) // : IUnitCalculateSkillPointsOnLevelupHandler
		{
			// Uses CallBack "GetSubscribingUnit()" to determine who the event is about.
			// I think this only gets called when level up results in new Skill points for distribution, though
			// it might get called before the player has accepted the changes....
			logDebug($"[{this.Name}] HandleUnitCalculateSkillPointsOnLevelup. NextClassLevel [{state.NextClassLevel}]");
		}

		public void HandleUnitGainLevel() // : IOwnerGainLevelHandler
		{
			// Uses CallBack "GetSubscribingUnit()" to determine who the event is about.
			// Likely called AFTER the player has confirmed the changes.
			logDebug($"[{this.Name}] HandleUnitGainLevel(). Level is [{this.npc.Descriptor.Progression.CharacterLevel}]");
			if (DateTimeOffset.Now.ToUnixTimeMilliseconds() < nextAcceptedEvent)
			{
				logDebug($"HandleUnitGainLevel Ignored: (Event Flooding)");
				return;
			}
			nextAcceptedEvent = DateTimeOffset.Now.ToUnixTimeMilliseconds() + EventFloodGate;
			ruleContext.updateBase(this.npc);
			ruleContext.updateBuffs(this.npc);
			ruleContext.updateStats(this.npc);
			ruleContext.updateFacts(this.npc);
			ruleContext.updateClasses(this.npc);
			ruleContext.updateInventory(this.npc);
		}

		public void HandleRecruit(UnitEntityData companion) // :  ICompanionChangeHandler
		{
			if (null == companion) return;
			logDebug($"[{this.Name}] HandleRecruit for [{companion.Descriptor.CharacterName}]");
			if (companion != this.npc) return;
			logDebug($"[{this.Name}] - Recruit detected");
			this.updateInParty();

			// Do Stuff?

		}

		public void HandleUnrecruit(UnitEntityData companion) // :  ICompanionChangeHandler
		{
			if (null == companion) return;
			logDebug($"[{this.Name}] HandleUnrecruit for [{companion.Descriptor.CharacterName}]");
			if (companion != this.npc) return;
			logDebug($"[{this.Name}] - Unrecruit detected");
			this.updateInParty();

			// Do Stuff?

		}

		// I assume this is specific to the player
		public void HandleIncreaseCorruption() // : ICorruptionLevelHandler
		{
			if (!this.inParty && !this.updateInParty()) { return; }
			logDebug($"[{this.Name}] HandleIncreaseCorruption()");

			// TODO: Need to figure out how to get corruption. I think it may 
			// be player only, but I will share the value with NPCs...
		}

		// I assume this is specific to the player
		public void HandleClearCorruption() // : ICorruptionLevelHandler
		{
			if (!this.inParty && !this.updateInParty()) { return; }
			logDebug($"[{this.Name}] HandleClearCorruption()");

			// TODO: Need to figure out how to get corruption. I think it may 
			// be player only, but I will share the value with NPCs...
		}

		public void HandleAddCompanion(UnitEntityData companion) // : IPartyHandler
		{
			if (null == companion) return;
			logDebug($"[{this.Name}] HandleAddCompanion for [{companion.Descriptor.CharacterName}]");
			if (companion != this.npc) return;
			logDebug($"[{this.Name}] - Add detected");
			this.updateInParty();

			// Do Stuff?
		}

		public void HandleCompanionActivated(UnitEntityData companion) // : IPartyHandler
		{
			if (null == companion) return;
			logDebug($"[{this.Name}] HandleCompanionActivated for [{companion.Descriptor.CharacterName}]");
			if (companion != this.npc) return;
			logDebug($"[{this.Name}] - Activation Detected");

			// Do Stuff?
		}

		public void HandleCompanionRemoved(UnitEntityData companion, bool stayInGame) // : IPartyHandler
		{
			if (null == companion) return;
			logDebug($"[{this.Name}] HandleCompanionRemoved for [{companion.Descriptor.CharacterName}] stayInGame [{stayInGame}]");
			if (companion != this.npc) return;
			logDebug($"[{this.Name}] - Removal Detected");
			this.updateInParty();

			// Do Stuff?
		}


		public void HandlePartyCombatStateChanged(bool inCombat) // : IPartyCombatHandler
		{
			if (!this.inParty && !this.updateInParty()) return;
			logDebug($"[{this.Name}] HandlePartyCombatStateChanged");

			// Do Stuff?
		}

		public void HandleAttributeDamage(UnitEntityData unit, StatType attribute, int oldDamage, int newDamage, bool drain) // : IAttributeDamageHandler
		{
			if (null == unit) return;
			logDebug($"[{this.Name}] OnAttributeDamaged for [{unit.Descriptor.CharacterName}] attribute [{attribute}] oldDamage [{oldDamage}] newDamage [{newDamage}] drain[{drain}]");
			if (unit != this.npc) return;
			logDebug($"[{this.Name}] - Attribute Damage Detected");
			if (!this.inParty && !this.updateInParty()) return;
			if (DateTimeOffset.Now.ToUnixTimeMilliseconds() < nextAcceptedEvent)
			{
				logDebug($"HandleAttributeDamage Ignored: (Event Flooding)");
				return;
			}
			nextAcceptedEvent = DateTimeOffset.Now.ToUnixTimeMilliseconds() + EventFloodGate;
			ruleContext.updateBase(this.npc);
			ruleContext.updateStats(this.npc);
		}

		public void HandleUnitGainFact(EntityFact fact) // : IUnitGainFactHandler
		{
			// Uses CallBack "GetSubscribingUnit()" to determine who the event is about.
			if (null != fact)
			{
				logDebug($"[{this.Name}] HandleUnitGainFact [{fact}]");
			}
			else
            {
				logDebug($"[{this.Name}] HandleUnitGainFact [null]");
			}
			if (DateTimeOffset.Now.ToUnixTimeMilliseconds() < nextAcceptedEvent)
			{
				logDebug($"HandleUnitGainFact Ignored: (Event Flooding)");
				return;
			}
			nextAcceptedEvent = DateTimeOffset.Now.ToUnixTimeMilliseconds() + EventFloodGate;
			ruleContext.updateFacts(this.npc);
		}

		public void HandleAlignmentChange(UnitEntityData unit, Kingmaker.Enums.Alignment newAlignment, Kingmaker.Enums.Alignment prevAlignment) // : IAlignmentChangeHandler
		{
			if (null == unit) return;
			logDebug($"[{this.Name}] HandleAlignmentChange for [{unit.Descriptor.CharacterName}] newAlignment [{newAlignment}] prevAlignment [{prevAlignment}]");
			if (unit != this.npc) return;
			logDebug($"[{this.Name}] - Alignment Change Detected");
			ruleContext.updateStats(this.npc);
		}

		// ------------------------------------------------------------------
		// IGNORED EVENTS            (Required by Interface, but not used...)
		// ------------------------------------------------------------------		
		public void HandleSkipPhase() { logDebug("HandleSkipPhase()"); } // IRestCampUIHandler
		public void HandleOpenRestCamp() { logDebug("HandleOpenRestCamp()"); } // IRestCampUIHandler
		public void HandleVisualCampPhaseFinished() { logDebug("HandleVisualCampPhaseFinished()"); } // IRestCampUIHandler
		public void HandleCloseRestCamp() { logDebug("HandleCloseRestCamp()"); } // IRestCampUIHandler
		public void HandleDecreaseCorruption() { logDebug($"HandleDecreaseCorruption()"); } // : ICorruptionLevelHandler		
		public void HandleCapitalModeChanged() { } // :  IPartyHandler ( Fires so often we dont even log it )




		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			sw.WriteLine("");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine($"Companion [{this.Name}]");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine($"currentRule        [{this.currentRule}]");
			sw.WriteLine($"portraitName       [{this.portraitName}]");
			sw.WriteLine($"portraitFileLarge  [{this.portraitFileLarge}]");
			sw.WriteLine($"portraitFileMedium [{this.portraitFileMedium}]");
			sw.WriteLine($"portraitFileSmall  [{this.portraitFileSmall}]");
			sw.WriteLine(ruleContext.ToString());
			return sw.ToString();
		}

		public static void logDebug(string value)
		{
			if (m_logDebug)
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
	}
}

