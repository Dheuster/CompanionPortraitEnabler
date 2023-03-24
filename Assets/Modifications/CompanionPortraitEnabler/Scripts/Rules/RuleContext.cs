using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

// using Kingmaker.Enums;                       // Required for ???
using Kingmaker.Blueprints.Classes;             // BlueprintRace
using Kingmaker.Blueprints;                     // Required for Race
using Kingmaker.PubSubSystem;                   // Various I* Event Callback Interfaces
using Kingmaker.Blueprints.CharGen;             // Male/Female Customization Options (Heads, Hair, eyebrows, etc...)
using Kingmaker.Controllers.Rest;               // Required for RestStatus
using Kingmaker.EntitySystem.Entities;          // Required for UnitEntityData
using Kingmaker.Blueprints.Items;               // Required for BlueprintItem
using Kingmaker.Blueprints.Items.Equipment;     // Required for BlueprintItem***...
using Kingmaker.Blueprints.Items.Weapons;       // Required for BlueprintItem***...
using Owlcat.Runtime.Core.Logging;              // Required for LogChannel
using Kingmaker.EntitySystem.Stats;             // Required for StatType
using Kingmaker.EntitySystem;                   // Required for EntityFact
using Kingmaker.UnitLogic.Parts;                // Required for UnitPartCompanion, UnitPartDollData
using Kingmaker.UnitLogic.Abilities.Blueprints; // Required for BlueprintAbility
using Kingmaker.UnitLogic.Class.LevelUp;        // Required for LevelUpState
using Kingmaker.Blueprints.Root;                // Required for BlueprintRoot
using Kingmaker.UnitLogic;                      // Required for DollData
using Kingmaker.Visual.CharacterSystem;         // Required for Character, BodyPart, EquipmentEntity, CharacterTexture, Material
using Kingmaker.View;                           // Required for UnitEntityView
using Kingmaker.Items;
using Kingmaker.Enums;
using Kingmaker;                                // Required for Game
using Kingmaker.ResourceLinks;                  // Required for WeakResourceLink, SpriteLink
using UnityEngine;                              // Required for Sprite, Material

// using OwlcatModification.Modifications.CompanionPortraitEnabler.Utility;
using OwlcatModification.Modifications.CompanionPortraitEnabler.RuleEnums;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules
{

    /**	
	 // RuleContext identifies the properties available for inspection and evaluation from 
	 // within Rule definitions.
	public class RuleContext 
	{
		//#################################################################################
		//                     INSTANCE DATA
		//#################################################################################

		public ushort[] USHORT_VALUES = new ushort[]
		{

			//======================================================================
			// HEALTH (6 items)
			//======================================================================

			0,   // [0] USHORT_VALUES[PROPERTY.None]
			1,   // [1] USHORT_VALUES[PROPERTY.HP_Base]
			1,   // [2] USHORT_VALUES[PROPERTY.HP_Max]
			1,   // [3] USHORT_VALUES[PROPERTY.HP_Mod]
			1,   // [4] USHORT_VALUES[PROPERTY.HP]
			100, // [5] USHORT_VALUES[PROPERTY.HP_Percent]

			//======================================================================
			// STATS
			//======================================================================

			// --------------------------------------
			// Stats : Attributes (12 items)
			// --------------------------------------

			5, // [6]  USHORT_VALUES[PROPERTY.Str]
			5, // [7]  USHORT_VALUES[PROPERTY.Dex]
			5, // [8]  USHORT_VALUES[PROPERTY.Con]
			5, // [9]  USHORT_VALUES[PROPERTY.Int]
			5, // [10] USHORT_VALUES[PROPERTY.Wis]
			5, // [11] USHORT_VALUES[PROPERTY.Chr]
			5, // [12] USHORT_VALUES[PROPERTY.Str_Base]
			5, // [13] USHORT_VALUES[PROPERTY.Dex_Base]
			5, // [14] USHORT_VALUES[PROPERTY.Con_Base]
			5, // [15] USHORT_VALUES[PROPERTY.Int_Base]
			5, // [16] USHORT_VALUES[PROPERTY.Wis_Base]
			5, // [17] USHORT_VALUES[PROPERTY.Chr_Base]

			// --------------------------------------
			// Stats : Saves (6 items)
			// --------------------------------------
			0, // [18] USHORT_VALUES[PROPERTY.Fort]
			0, // [19] USHORT_VALUES[PROPERTY.Will]
			0, // [20] USHORT_VALUES[PROPERTY.Reflex]
			0, // [21] USHORT_VALUES[PROPERTY.Fort_Base]
			0, // [22] USHORT_VALUES[PROPERTY.Will_Base]
			0, // [23] USHORT_VALUES[PROPERTY.Reflex_Base]

			// --------------------------------------
			// Stats : Skills (22 items)
			// --------------------------------------
			0, // [24] USHORT_VALUES[PROPERTY.Mobility]
			0, // [25] USHORT_VALUES[PROPERTY.Athletics]
			0, // [26] USHORT_VALUES[PROPERTY.Perception]
			0, // [27] USHORT_VALUES[PROPERTY.Thievery]
			0, // [28] USHORT_VALUES[PROPERTY.LoreNature]
			0, // [29] USHORT_VALUES[PROPERTY.KnowledgeArcana]
			0, // [30] USHORT_VALUES[PROPERTY.Persuasion]
			0, // [31] USHORT_VALUES[PROPERTY.Stealth]
			0, // [32] USHORT_VALUES[PROPERTY.UseMagicDevice]
			0, // [33] USHORT_VALUES[PROPERTY.LoreReligion]
			0, // [34] USHORT_VALUES[PROPERTY.KnowledgeWorld]
			0, // [35] USHORT_VALUES[PROPERTY.Mobility_Base]
			0, // [36] USHORT_VALUES[PROPERTY.Athletics_Base]
			0, // [37] USHORT_VALUES[PROPERTY.Perception_Base]
			0, // [38] USHORT_VALUES[PROPERTY.Thievery_Base]
			0, // [39] USHORT_VALUES[PROPERTY.LoreNature_Base]
			0, // [40] USHORT_VALUES[PROPERTY.KnowledgeArcana_Base]
			0, // [41] USHORT_VALUES[PROPERTY.Persuasion_Base]
			0, // [42] USHORT_VALUES[PROPERTY.Stealth_Base]
			0, // [43] USHORT_VALUES[PROPERTY.UseMagicDevice_Base]
			0, // [44] USHORT_VALUES[PROPERTY.LoreReligion_Base]
			0, // [45] USHORT_VALUES[PROPERTY.KnowledgeWorld_Base]

			// --------------------------------------
			// Stats : Misc (11 items)
			// --------------------------------------
			0,  // [46] USHORT_VALUES[PROPERTY.AC]
			0,  // [47] USHORT_VALUES[PROPERTY.AC_Base]
			0,  // [48] USHORT_VALUES[PROPERTY.AC_TOUCH]
			0,  // [49] USHORT_VALUES[PROPERTY.AC_FLATFOOTED]

			0,  // [50] USHORT_VALUES[PROPERTY.Initiative]
			0,  // [51] USHORT_VALUES[PROPERTY.Initiative_Base]

			20, // [52] USHORT_VALUES[PROPERTY.Speed]
			20, // [53] USHORT_VALUES[PROPERTY.Speed_Base]

			0,  // [54] USHORT_VALUES[PROPERTY.Corruption]
			1,  // [55] USHORT_VALUES[PROPERTY.Corruption_Max]
			0,  // [56] USHORT_VALUES[PROPERTY.Corruption_Percent]

			//======================================================================
			// Classes / Levels / Experience
			//======================================================================

			// Players and NPCs can have any number of classes in WOTR. You can literally
			// pick another class specialization with each level, however for rule purposes
			// we only track archtypes and we only track 3.
			//
			// The PrimaryClassArchType is the one which has the most levels
			// The SecondaryClassArchType is the one that has the second most levels.
			// The Mythic Class reveals the mythic path you choose and is independent of
			//     Primary or Secondary.
			//
			// When you have multiple classes and there is a tie in terms of level 
			// investment, then the amount of time spent as the class is used as 
			// the tie breaker. If you have 3 classes and they are all level 1, 
			// The Primary is the one you chose first and Secondary is the one 
			// you chose second. 
			//
			// When you have only 1 class, Secondary_Class = None
			//
			// Primary and Secondary Classes describe the Main Class and are not
			// broken up into sub-class specializations. IE: If you choose
			// Rogue -> Rowdy, it counts as a Rogue class. This is in line with the games
			// leveling system which will not allow you to pick more than 1 specialization
			// per Class, but allows you to pick multiple classes/specializations overall.

       		0, // [57] USHORT_VALUES[PROPERTY.PrimaryClassArchTypeLevel]
   			0, // [58] USHORT_VALUES[PROPERTY.SecondaryClassArchTypeLevel]
   			0, // [59] USHORT_VALUES[PROPERTY.MythicClassLevel]
			1, // [60] USHORT_VALUES[PROPERTY.Level_Base]
			1, // [61] USHORT_VALUES[PROPERTY.Level]  // Current Invested Level (If player doesn't level up, this does not change).

			0, // [62] UNUSED 
			0  // [63] UNUSED
		};

		public ushort[] ENUM_VALUES = new ushort[] 
		{

			//======================================================================
			// Closed Sets (Enum : short)
			//======================================================================

			0,                             // [0] ENUM_VALUES[PROPERTY.None                   >> 6] (Unused)
			(ushort)CLASSCATEGORY.None,    // [1] ENUM_VALUES[PROPERTY.PrimaryClassCategory   >> 6]
			(ushort)CLASSCATEGORY.None,    // [2] ENUM_VALUES[PROPERTY.SecondaryClassCategory >> 6]
			(ushort)CLASSARCHTYPE.None,    // [3] ENUM_VALUES[PROPERTY.PrimaryClassArchType   >> 6]
			(ushort)CLASSARCHTYPE.None,    // [4] ENUM_VALUES[PROPERTY.SecondaryClassArchType >> 6]
			(ushort)CLASSARCHTYPE.None,    // [5] ENUM_VALUES[PROPERTY.AllClassArchTypes      >> 6]
			(ushort)CLASSMYTHIC.None,      // [6] ENUM_VALUES[PROPERTY.MythicClass            >> 6]

			(ushort)HEALTHENUM.None,       // [7] ENUM_VALUES[PROPERTY.Health    >> 6]
			(ushort)RACEMASK.Human,        // [8] ENUM_VALUES[PROPERTY.Race      >> 6]
			(ushort)GENDER.Male,           // [9] ENUM_VALUES[PROPERTY.Gender    >> 6]
			(ushort)CIVILITY.Neutral,      // [10] ENUM_VALUES[PROPERTY.Civility  >> 6]
			(ushort)MORALITY.Neutral,      // [11] ENUM_VALUES[PROPERTY.Morality  >> 6]
			(ushort)ALIGNMENT.TrueNeutral, // [12] ENUM_VALUES[PROPERTY.Alignment >> 6]
			(ushort)ACUITY.Rested,         // [13] ENUM_VALUES[PROPERTY.Acuity    >> 6]
			(ushort)NPCSIZE.Medium,        // [14] ENUM_VALUES[PROPERTY.Size      >> 6]
			(ushort)NPCSIZE.Medium         // [15] ENUM_VALUES[PROPERTY.Size_Base >> 6]
		};

	    //======================================================================
		// Open Sets (Maps)
	    //======================================================================

		public HashSet<string>[] STRSET_VALUES = new HashSet<string>[]
        {
			new HashSet<string>(),     // [0] STRSET_VALUES[PROPERTY.None        >> 10] (Unused)
			new HashSet<string>(),     // [1] STRSET_VALUES[PROPERTY.Facts       >> 10]
			new HashSet<string>(),     // [2] STRSET_VALUES[PROPERTY.Buffs       >> 10]
			new HashSet<string>(),     // [3] STRSET_VALUES[PROPERTY.SharedStash >> 10]
			new HashSet<string>(),     // [4] STRSET_VALUES[PROPERTY.Inventory   >> 10]
			new HashSet<string>()      // [5] STRSET_VALUES[PROPERTY.Equipped    >> 10]
        };

	    //======================================================================
		// Integers (Value may exceed limit of short = 16384)
	    //======================================================================

		public uint[] UINT_VALUES = new uint[] {
			0,                         // [0] UINT_VALUES[PROPERTY.None >> 13] // (Unused)
			0                          // [1] UINT_VALUES[PROPERTY.XP   >> 13] // Experience Points.
		};
			


		// ######################################################################################
		// ######################################################################################
		//           LOCAL SUPPORT BELOW THIS LINE. DO NOT REFERENCE VARIABLES
		// ######################################################################################
		// ######################################################################################

		// local
	    // long hashCode = 0;

		// Logging support
		public static Kingmaker.Modding.OwlcatModification Modification { get; private set; }
		public static LogChannel Logger => Modification.Logger;
		public static Boolean m_logDebug = false;

		// ------------------------------------------------------------------
		// Constructor
		// ------------------------------------------------------------------
		public RuleContext(Kingmaker.Modding.OwlcatModification modification, Boolean debug = false, UnitEntityData npc = null)
		{

			if (null == RuleContext.Modification)
			{
				RuleContext.Modification = modification;
				RuleContext.m_logDebug = debug;
			}

			if (null != npc) {
				updateBase(npc);
				updateBuffs(npc);
				updateStats(npc);
				updateFacts(npc);
				updateClasses(npc);
				updateEquipped(npc);
				updateInventory(npc);
				updateHealth(npc);
			}

			// Register For events on Construction:
			EventBus.Subscribe(this);
		}

		// Class Change Tracking
		private string maxClassName        = "";
		private int    maxClassLevel       = 0;
		private string nextToMaxClassName  = "";
		private int    nextToMaxClassLevel = 0;
		private int    mythicClassLevel    = 0;

		// Inventory Change Tracking
		private int SharedStashCount = 0;
		private int InventoryCount = 0;

		// Equipped Change Tracking
		private String equipped_primary = "";
        private String equipped_secondary = "";
        private String equipped_armor = "";
        private String equipped_shirt = "";
        private String equipped_belt = "";
        private String equipped_head = "";
        private String equipped_eyes = "";
        private String equipped_feet = "";
        private String equipped_gloves = "";
        private String equipped_neck = "";
        private String equipped_ring1 = "";
        private String equipped_ring2 = "";
        private String equipped_wrist = "";
        private String equipped_shoulders = "";

		// ----------------------------------------------------------------------   
		// Updaters
		// ----------------------------------------------------------------------   
		public void updateHealth(UnitEntityData npc) {
			USHORT_VALUES[(int)PROPERTY.HP_Base] = (npc.Stats.HitPoints.BaseValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.HitPoints.BaseValue);
			// UINT_VALUES[PROPERTY.HP_Base] = (npc.Stats.HitPoints.BaseValue <= 0) ? 0 : npc.Stats.HitPoints.BaseValue;
		    ushort hp_max = (npc.MaxHP <= 0) ? (ushort)(0) : (ushort)(npc.MaxHP);
			USHORT_VALUES[(int)PROPERTY.HP_Max]  = hp_max;
			USHORT_VALUES[(int)PROPERTY.HP_Mod]  = (npc.Stats.HitPoints.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.HitPoints.ModifiedValue);
		    ushort hp = (npc.HPLeft <= 0) ? (ushort)(0) : (ushort)(npc.HPLeft); // npc.Stats.HitPoints.ModifiedValue;

            USHORT_VALUES[(int)PROPERTY.HP]      = hp;
		    ushort hp_percent = (ushort)((hp*100)/hp_max);
			USHORT_VALUES[(int)PROPERTY.HP_Percent] = hp_percent;

			ENUM_VALUES[ ((int)PROPERTY.Health) >> 6] = (hp_percent > 75) ? (ushort)(HEALTHENUM.HP_75_to_100) :
				                                        (hp_percent > 50) ? (ushort)(HEALTHENUM.HP_50_to_75) :
				                                        (hp_percent > 25) ? (ushort)(HEALTHENUM.HP_25_to_50) :
				                                        (ushort)(HEALTHENUM.HP_0_to_25);
		}

		public void updateStats(UnitEntityData npc)
		{
			// ASSUMES updateBase and updateBuffs were called first!
			// ENUMS
            ENUM_VALUES[(((int)PROPERTY.Race) >> 6)] = (ushort)(strToRaceMask(npc.Descriptor.Progression.VisibleRace.ToString()));
			ushort alignment = (ushort)(strToAlignment(npc.Descriptor.Alignment.ValueVisible.ToString()));
            ENUM_VALUES[(((int)PROPERTY.Alignment) >> 6)] = alignment;
			Tuple<CIVILITY, MORALITY> alignmentParts = Meta.AlignmentToParts[(ALIGNMENT)(alignment)];
            ENUM_VALUES[(((int)PROPERTY.Civility) >> 6)] = (ushort)(alignmentParts.Item1);
            ENUM_VALUES[(((int)PROPERTY.Morality) >> 6)] =  (ushort)(alignmentParts.Item2);
            ENUM_VALUES[(((int)PROPERTY.Size) >> 6)] = (ushort)(Meta.KMSizeToNPCSize[(npc.State?.Size ?? npc.Descriptor.OriginalSize)]);
            ENUM_VALUES[(((int)PROPERTY.Acuity) >> 6)] = STRSET_VALUES[((int)PROPERTY.Buffs)>>10].Contains("EXHAUSTED") ? (ushort)(ACUITY.Exhausted) : 
				                                         STRSET_VALUES[((int)PROPERTY.Buffs)>>10].Contains("FATIGUED")  ? (ushort)(ACUITY.Fatigued) : 
											             (ushort)(ACUITY.Rested);
			// STATS : Attributes
			USHORT_VALUES[(int)PROPERTY.Str] = (ushort)(npc.Stats.Strength.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.Dex] = (ushort)(npc.Stats.Dexterity.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.Con] = (ushort)(npc.Stats.Constitution.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.Int] = (ushort)(npc.Stats.Intelligence.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.Wis] = (ushort)(npc.Stats.Wisdom.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.Chr] = (ushort)(npc.Stats.Charisma.ModifiedValue);
			// STATS : Saves
			USHORT_VALUES[(int)PROPERTY.Fort] = (npc.Stats.SaveFortitude.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SaveFortitude.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.Reflex] = (npc.Stats.SaveReflex.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SaveReflex.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.Will] = (npc.Stats.SaveWill.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SaveWill.ModifiedValue);
			// STATS : Skills
			USHORT_VALUES[(int)PROPERTY.Mobility] = (npc.Stats.SkillMobility.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillMobility.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.Athletics] = (npc.Stats.SkillAthletics.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillAthletics.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.Perception] = (npc.Stats.SkillPerception.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillPerception.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.Thievery] = (npc.Stats.SkillThievery.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillThievery.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.LoreNature] = (npc.Stats.SkillLoreNature.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillLoreNature.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.KnowledgeArcana] = (npc.Stats.SkillKnowledgeArcana.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillKnowledgeArcana.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.Persuasion] = (npc.Stats.SkillPersuasion.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillPersuasion.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.Stealth] = (npc.Stats.SkillStealth.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillStealth.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.UseMagicDevice] = (npc.Stats.SkillUseMagicDevice.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillUseMagicDevice.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.LoreReligion] = (npc.Stats.SkillLoreReligion.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillLoreReligion.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.KnowledgeWorld] = (npc.Stats.SkillKnowledgeWorld.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillKnowledgeWorld.ModifiedValue);
			// STATS : Misc
			USHORT_VALUES[(int)PROPERTY.Initiative] = (npc.Stats.Initiative.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.Initiative.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.Speed] = (npc.Stats.Speed.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.Speed.ModifiedValue);
			
			USHORT_VALUES[(int)PROPERTY.AC] = (npc.Stats.AC.ModifiedValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.AC.ModifiedValue);
			USHORT_VALUES[(int)PROPERTY.AC_TOUCH] = (ushort)(npc.Stats.AC.Touch);
			USHORT_VALUES[(int)PROPERTY.AC_FLATFOOTED] = (ushort)(npc.Stats.AC.FlatFooted);
			USHORT_VALUES[(int)PROPERTY.Level] = (ushort)(npc.Descriptor.Progression.CharacterLevel);
			UINT_VALUES[((int)PROPERTY.XP) >> 13] = (uint)(npc.Descriptor.Progression.Experience);
			ushort corruption =  (ushort)(Game.Instance.Player.Corruption.CurrentValue);
			USHORT_VALUES[(int)PROPERTY.Corruption] = corruption;
			int discard = 0;
			USHORT_VALUES[(int)PROPERTY.Corruption_Percent] = (ushort)(Math.DivRem(USHORT_VALUES[(int)PROPERTY.Corruption_Max] * corruption, 100, out discard));
			ENUM_VALUES[((int)PROPERTY.Gender) >> 6] = (npc.Gender == Kingmaker.Blueprints.Gender.Male) ? (ushort)(GENDER.Male) : (ushort)(GENDER.Female);

		}

		public void updateBase(UnitEntityData npc)
		{
			// BASE ENUMS:
			ENUM_VALUES[(((int)PROPERTY.Size_Base) >> 6)] = (ushort)(Meta.KMSizeToNPCSize[npc.Descriptor.OriginalSize]);
			// BASE STATS : Attributes
			USHORT_VALUES[(int)PROPERTY.Str_Base]             = (ushort)(npc.Stats.Strength.BaseValue);
			USHORT_VALUES[(int)PROPERTY.Dex_Base]             = (ushort)(npc.Stats.Dexterity.BaseValue);
			USHORT_VALUES[(int)PROPERTY.Con_Base]             = (ushort)(npc.Stats.Constitution.BaseValue);
			USHORT_VALUES[(int)PROPERTY.Int_Base]             = (ushort)(npc.Stats.Intelligence.BaseValue);
			USHORT_VALUES[(int)PROPERTY.Wis_Base]             = (ushort)(npc.Stats.Wisdom.BaseValue);
			USHORT_VALUES[(int)PROPERTY.Chr_Base]             = (ushort)(npc.Stats.Charisma.BaseValue);
			// BASE STATS : Saves
			USHORT_VALUES[(int)PROPERTY.Fort_Base]            = (npc.Stats.SaveFortitude.BaseValue        <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SaveFortitude.BaseValue);
			USHORT_VALUES[(int)PROPERTY.Reflex_Base]          = (npc.Stats.SaveReflex.BaseValue           <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SaveReflex.BaseValue);
			USHORT_VALUES[(int)PROPERTY.Will_Base]            = (npc.Stats.SaveWill.BaseValue             <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SaveWill.BaseValue);
			// BASE STATS : Skills
			USHORT_VALUES[(int)PROPERTY.Mobility_Base]        = (npc.Stats.SkillMobility.BaseValue        <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillMobility.BaseValue);
			USHORT_VALUES[(int)PROPERTY.Athletics_Base]       = (npc.Stats.SkillAthletics.BaseValue       <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillAthletics.BaseValue);
			USHORT_VALUES[(int)PROPERTY.Perception_Base]      = (npc.Stats.SkillPerception.BaseValue      <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillPerception.BaseValue);
			USHORT_VALUES[(int)PROPERTY.Thievery_Base]        = (npc.Stats.SkillThievery.BaseValue        <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillThievery.BaseValue);
			USHORT_VALUES[(int)PROPERTY.LoreNature_Base]      = (npc.Stats.SkillLoreNature.BaseValue      <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillLoreNature.BaseValue);
			USHORT_VALUES[(int)PROPERTY.KnowledgeArcana_Base] = (npc.Stats.SkillKnowledgeArcana.BaseValue <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillKnowledgeArcana.BaseValue);
			USHORT_VALUES[(int)PROPERTY.Persuasion_Base]      = (npc.Stats.SkillPersuasion.BaseValue      <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillPersuasion.BaseValue);
			USHORT_VALUES[(int)PROPERTY.Stealth_Base]         = (npc.Stats.SkillStealth.BaseValue         <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillStealth.BaseValue);
			USHORT_VALUES[(int)PROPERTY.UseMagicDevice_Base]  = (npc.Stats.SkillUseMagicDevice.BaseValue  <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillUseMagicDevice.BaseValue);
			USHORT_VALUES[(int)PROPERTY.LoreReligion_Base]    = (npc.Stats.SkillLoreReligion.BaseValue    <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillLoreReligion.BaseValue);
			USHORT_VALUES[(int)PROPERTY.KnowledgeWorld_Base]  = (npc.Stats.SkillKnowledgeWorld.BaseValue  <= 0) ? (ushort)(0) : (ushort)(npc.Stats.SkillKnowledgeWorld.BaseValue);
			// BASE STATS : Misc
			USHORT_VALUES[(int)PROPERTY.Initiative_Base]      = (npc.Stats.Initiative.BaseValue           <= 0) ? (ushort)(0) : (ushort)(npc.Stats.Initiative.BaseValue);
			USHORT_VALUES[(int)PROPERTY.Speed_Base]           = (npc.Stats.Speed.BaseValue                <= 0) ? (ushort)(0) : (ushort)(npc.Stats.Speed.BaseValue);
			USHORT_VALUES[(int)PROPERTY.AC_Base]              = (npc.Stats.AC.BaseValue                   <= 0) ? (ushort)(0) : (ushort)(npc.Stats.AC.BaseValue);
			USHORT_VALUES[(int)PROPERTY.Level_Base]           = (ushort)(npc.Descriptor.Progression.GetClassLevel(npc.Descriptor.Progression.ClassesOrder[0]));
			USHORT_VALUES[(int)PROPERTY.Corruption_Max]       = (ushort)(BlueprintRoot.Instance.Corruption?.MaxValue ?? 100);
		}

		public bool updateFacts(UnitEntityData npc)
		{
			int factOffset = ((int)PROPERTY.Facts) >> 10;
			HashSet<String> factSetRef = STRSET_VALUES[factOffset]; // HashSet is a class object, thus local var will be reference, not copy.
			int count = factSetRef.Count();
			foreach (Kingmaker.EntitySystem.EntityFact f in npc.Facts.List)
			{
				Kingmaker.Blueprints.Facts.BlueprintUnitFact fact = f.Blueprint as Kingmaker.Blueprints.Facts.BlueprintUnitFact;
				if (null != fact && !string.IsNullOrEmpty(fact.Name))
				{
					if (factSetRef.Contains(fact.Name.ToUpper()))
					{
						count--;
					}
					else
					{
						count = -1;
						break;
					}
				}
			}
			if (count != -1)
			{
				foreach (Kingmaker.UnitLogic.Abilities.Ability a in npc.Abilities.Visible)
				{
					BlueprintAbility ability = a.Blueprint;
					if (null != ability && !string.IsNullOrEmpty(ability.Name))
					{
						if (factSetRef.Contains(ability.Name.ToUpper()))
						{
							count--;
						}
						else
						{
							count = -1;
							break;
						}
					}
				}
			}
			if (0 == count)
			{
				return false; // no change (avoid rebuild)
			}
			if (RuleContext.m_logDebug)
			{
				if (count > 0)
				{
					logAlways($"[{count}] Facts Removed");
				}
				else
				{
					logAlways($"1 or more Facts Added");
				}
			}

			// Change took place (To visible UnitFact or Ability). Rebuild:
			factSetRef.Clear();
			foreach (Kingmaker.EntitySystem.EntityFact f in npc.Facts.List)
			{
				Kingmaker.Blueprints.Facts.BlueprintUnitFact fact = f.Blueprint as Kingmaker.Blueprints.Facts.BlueprintUnitFact;
				if (null != fact && !string.IsNullOrEmpty(fact.Name))
				{
					factSetRef.Add(fact.Name.ToUpper()); // fact.Name.ToUpper()
				}
			}
			foreach (Kingmaker.UnitLogic.Abilities.Ability a in npc.Abilities.Visible)
			{
				BlueprintAbility ability = a.Blueprint;
				if (null != ability && !string.IsNullOrEmpty(ability.Name))
				{
					factSetRef.Add(ability.Name.ToUpper()); // ability.Name.ToUpper()
				}
			}
			return true;
		}

		public bool updateBuffs(UnitEntityData npc)
		{
			int buffOffset = ((int)PROPERTY.Buffs) >> 10;
			HashSet<String> buffSetRef = STRSET_VALUES[buffOffset]; // HashSet is a class object, thus local var will be references, not copies.
			int count = buffSetRef.Count();

			foreach (Kingmaker.UnitLogic.Buffs.Buff b in npc.Buffs)
			{
				Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff buff = b.Blueprint;
				if (null != buff && !string.IsNullOrEmpty(buff.Name))
				{
					if (buffSetRef.Contains(buff.Name.ToUpper()))
					{
						count--;
					}
					else
					{
						count = -1;
						break;
					}
				}
			}
			if (0 == count)
			{
				return false; // no change (avoid rebuild)
			}
			if (RuleContext.m_logDebug)
			{
				if (count > 0)
				{
					logAlways($"[{count}] Buffs Removed");
				}
				else
				{
					logAlways($"1 or more Buffs Added");
				}
			}

			// Change took place. Rebuild:
			buffSetRef.Clear();
			foreach (Kingmaker.UnitLogic.Buffs.Buff b in npc.Buffs)
			{
				Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff buff = b.Blueprint;
				if (null != buff && !string.IsNullOrEmpty(buff.Name))
				{
					buffSetRef.Add(buff.Name.ToUpper());
				}
			}
			return true;
		}


		public bool updateInventory(UnitEntityData npc)
		{
			UnitDescriptor descriptor = npc.Descriptor;
			if (descriptor.Inventory.Items.Count() == SharedStashCount)
            {
				return false;
            }
			HashSet<String> SharedStashRef  = STRSET_VALUES[(((int)PROPERTY.SharedStash) >> 10)]; // HashSet is a class object, thus local var will be references, not copies.
			HashSet<String> InventorySetRef = STRSET_VALUES[(((int)PROPERTY.Inventory) >> 10)]; // HashSet is a class object, thus local var will be references, not copies.

			int  invCount = 0;
			int  stashCount     = 0;
			bool needsRefresh=false;
			foreach (ItemEntity item in descriptor.Inventory)
			{
				stashCount++;
				string Name = item.Name.ToUpper();

				if (descriptor == item.Owner) // filter out the ones in this npc's holding slots
                {
					if (!(InventorySetRef.Contains($"{Name}")))
					{
                        logAlways($"1 or more items unassiged from NPC");
						needsRefresh = true;
						break;
					}
					invCount++;
                }
				if (!(SharedStashRef.Contains($"{Name}")))
				{
                    logAlways($"1 or more items removed from Shared Stash");
					needsRefresh = true;
					break;
				}
			}
			if (!needsRefresh)
            {
				if (stashCount > this.SharedStashCount)
                {
					logDebug($"{(this.SharedStashCount - stashCount)} Items Added to Shared Stash");
					needsRefresh = true;
                }
				if (invCount > this.InventoryCount)
                {
					logDebug($"{(this.InventoryCount - invCount)} New Items assigned to NPC");
					needsRefresh = true;
                }
            }
			if (!needsRefresh)
            {
				return false;
            }

			// Note : These values are not simply the size of the sets. NPCs can have 
			// multiple items of the same name. THe counts are based on lists and
			// thus account for duplicates. The sets only store 1 copy of each unique
			// name. (Tags are unique, but not items). If the party mule is carrying 
			// 100 halberts, the set will only have 1 "halbert", but the list count
            // will indicate 100 items.

			InventorySetRef.Clear();
			SharedStashRef.Clear();
			this.InventoryCount   = 0;
			this.SharedStashCount = descriptor.Inventory.Items.Count();
			foreach (ItemEntity item in descriptor.Inventory)
			{
				string Name = item.Name.ToUpper();

                // Decided to drop support for WeaponCategory. Just too
                // many to do an effective masking operation. (more than 
				// 63 and we can't use bitmasks for fast set container
                // checks. We could technically use 2 ulongs to extend
                // fast container checking up to 127 items, maybe another
                // version... C# doesn't have a specialized EnumMap and
                // EnumSet implementation like Java that auto-maps
                // enum offsets to a series of long masking operations
				// for superfast set intersection checks. Shame... Maybe
                // I should port it over... 

//				// HINT: BlueprintItem blueprintItem = item.Blueprint;
//				// HINT: string identifiedName = item.Blueprint.Name;
//
//              // HINT: from Kingmaker.UI.Common.ItemsFilter
//              // enum ItemType : [ Weapon, Shield, Armor, Ring, Belt, Feet, Gloves, Head, Neck, Shoulders, Wrist, Usable, NonUsable, Other, Glasses, Shirt ]
//
//				if (Kingmaker.UI.Common.ItemsFilter.ItemType.Weapon == item.Blueprint.ItemType)
//              {
//					ItemEntityWeapon weaponMeta = item as ItemEntityWeapon;
//					if (null != weaponMeta)
//					{
//                      // WeaponCategory is an enum, but there are over 80 categories...
//						WeaponCategory wc = weaponMeta.Blueprint.Category;
//					}
//              }

				if (descriptor == item.Owner) // filter out the ones in this npc's holding slots
                {
					this.InventoryCount++;
					InventorySetRef.Add($"{Name}");
                }
				else
                {
					SharedStashRef.Add($"{Name}");
                }
			}
			return true;
		}

		public bool updateEquipped(UnitEntityData npc)
		{
			int EquippedOffset = ((int)PROPERTY.Equipped) >> 10;
			HashSet<String> EquippedSetRef = STRSET_VALUES[EquippedOffset]; // HashSet is a class object, thus local var will be references, not copies.

			// Need to add event monitor for IUnitEquipmentHandler/IUnitActiveEquipmentSetHandler. There is a slot updated method we can 
			// to to monitor when these should be ran.

            bool needsRefresh = (0 == EquippedSetRef.Count());
            if (npc.Body.CurrentHandsEquipmentSet.PrimaryHand.MaybeWeapon != null)
            {
				String itemCheck = npc.Body.CurrentHandsEquipmentSet.PrimaryHand.MaybeWeapon.Name.ToUpper();
                needsRefresh = needsRefresh || (this.equipped_primary != itemCheck);
                this.equipped_primary = itemCheck;
            }
            else
            {
                needsRefresh = needsRefresh || (this.equipped_primary != "");
                this.equipped_primary = "";
            }
            if (npc.Body.CurrentHandsEquipmentSet.SecondaryHand.MaybeWeapon != null)
            {
				String itemCheck = npc.Body.CurrentHandsEquipmentSet.SecondaryHand.MaybeWeapon.Name.ToUpper();
                needsRefresh = needsRefresh || (this.equipped_secondary != itemCheck);
                this.equipped_secondary = itemCheck;
            }
            else if (npc.Body.CurrentHandsEquipmentSet.SecondaryHand.MaybeShield != null)
            {
				String itemCheck = npc.Body.CurrentHandsEquipmentSet.SecondaryHand.MaybeShield.Name.ToUpper();
                needsRefresh = needsRefresh || (this.equipped_secondary != itemCheck);
                this.equipped_secondary = itemCheck;
            } 
            else
            {
                needsRefresh = needsRefresh || (this.equipped_secondary != "");
                this.equipped_secondary = "";
            }
			if (npc.Body.Armor.HasArmor)
            {
				String itemCheck = npc.Body.Armor.Item.Name.ToUpper();
                needsRefresh = needsRefresh || (this.equipped_armor != itemCheck);
				this.equipped_armor = itemCheck;
            }
			else
            {
                needsRefresh = needsRefresh || (this.equipped_armor != "");
				this.equipped_armor = "";
            }
			if (null != npc.Body.Shirt.MaybeItem)
            {
				String itemCheck = npc.Body.Shirt.MaybeItem.Name.ToUpper();
                needsRefresh = needsRefresh || (this.equipped_shirt != itemCheck);
				this.equipped_shirt = itemCheck;
            }
			else
            {
                needsRefresh = needsRefresh || (this.equipped_shirt != "");
				this.equipped_shirt = "";
            }
			if (null != npc.Body.Belt.MaybeItem)
            {
				String itemCheck = npc.Body.Belt.MaybeItem.Name.ToUpper();
                needsRefresh = needsRefresh || (this.equipped_belt != itemCheck);
				this.equipped_belt = itemCheck;
            }
			else 
			{ 
                needsRefresh = needsRefresh || (this.equipped_belt != "");
				this.equipped_belt = "";
			}
			if (null != npc.Body.Head.MaybeItem)
            {
				String itemCheck = npc.Body.Head.MaybeItem.Name.ToUpper();
                needsRefresh = needsRefresh || (this.equipped_head != itemCheck);
				this.equipped_head = itemCheck;
            }
            else 
			{ 
                needsRefresh = needsRefresh || (this.equipped_head != "");
				this.equipped_head = "";
			}
			if (null != npc.Body.Glasses.MaybeItem)
            {
				String itemCheck = npc.Body.Glasses.MaybeItem.Name.ToUpper();
                needsRefresh = needsRefresh || (this.equipped_eyes != itemCheck);
				this.equipped_eyes = itemCheck;
            }
            else 
			{ 
                needsRefresh = needsRefresh || (this.equipped_eyes != "");
				this.equipped_eyes = "";
			}
			if (null != npc.Body.Feet.MaybeItem)
            {
				String itemCheck = npc.Body.Feet.MaybeItem.Name.ToUpper();
                needsRefresh = needsRefresh || (this.equipped_feet != itemCheck);
				this.equipped_feet = itemCheck;
            }
            else 
			{ 
                needsRefresh = needsRefresh || (this.equipped_feet != "");
				this.equipped_feet = "";
			}
			if (null != npc.Body.Gloves.MaybeItem)
            {
				String itemCheck =  npc.Body.Gloves.MaybeItem.Name.ToUpper();
                needsRefresh = needsRefresh || (this.equipped_gloves != npc.Body.Gloves.MaybeItem.Name);
				this.equipped_gloves = npc.Body.Gloves.MaybeItem.Name;
            }
            else 
			{ 
                needsRefresh = needsRefresh || (this.equipped_gloves != "");
				this.equipped_gloves = "";
			}
			if (null != npc.Body.Neck.MaybeItem)
            {
                needsRefresh = needsRefresh || (this.equipped_neck != npc.Body.Neck.MaybeItem.Name);
				this.equipped_neck = npc.Body.Neck.MaybeItem.Name;
            }
            else 
			{ 
                needsRefresh = needsRefresh || (this.equipped_neck != "");
				this.equipped_neck = "";
			}
			if (null != npc.Body.Ring1.MaybeItem)
            {
                needsRefresh = needsRefresh || (this.equipped_ring1 != npc.Body.Ring1.MaybeItem.Name);
				this.equipped_ring1 = npc.Body.Ring1.MaybeItem.Name;
            }
            else 
			{ 
                needsRefresh = needsRefresh || (this.equipped_ring1 != "");
				this.equipped_ring1 = "";
			}
			if (null != npc.Body.Ring2.MaybeItem)
            {
                needsRefresh = needsRefresh || (this.equipped_ring2 != npc.Body.Ring2.MaybeItem.Name);
				this.equipped_ring2 = npc.Body.Ring2.MaybeItem.Name;
            }
            else 
			{ 
                needsRefresh = needsRefresh || (this.equipped_ring2 != "");
				this.equipped_ring2 = "";
			}
			if (null != npc.Body.Wrist.MaybeItem)
            {
                needsRefresh = needsRefresh || (this.equipped_wrist != npc.Body.Wrist.MaybeItem.Name);
				this.equipped_wrist = npc.Body.Wrist.MaybeItem.Name;
            }
            else 
			{ 
                needsRefresh = needsRefresh || (this.equipped_wrist != "");
				this.equipped_wrist = "";
			}
			if (null != npc.Body.Shoulders.MaybeItem)
            {
                needsRefresh = needsRefresh || (this.equipped_shoulders != npc.Body.Shoulders.MaybeItem.Name);
				this.equipped_shoulders = npc.Body.Shoulders.MaybeItem.Name;
            }
            else 
			{ 
                needsRefresh = needsRefresh || (this.equipped_shoulders != "");
				this.equipped_shoulders = "";
			}
			if (needsRefresh)
            {
				EquippedSetRef.Clear();
				EquippedSetRef.Add(equipped_primary);
				EquippedSetRef.Add(equipped_secondary);
				EquippedSetRef.Add(equipped_armor);
				EquippedSetRef.Add(equipped_shirt);
				EquippedSetRef.Add(equipped_belt);
				EquippedSetRef.Add(equipped_head);
				EquippedSetRef.Add(equipped_eyes);
				EquippedSetRef.Add(equipped_feet);
				EquippedSetRef.Add(equipped_gloves);
				EquippedSetRef.Add(equipped_neck);
				EquippedSetRef.Add(equipped_ring1);
				EquippedSetRef.Add(equipped_ring2);
				EquippedSetRef.Add(equipped_wrist);
				EquippedSetRef.Add(equipped_shoulders);
				EquippedSetRef.Remove("");
				return true;
            }
			return false;
		}

		public bool updateClasses(UnitEntityData npc)
		{			
			BlueprintCharacterClass mClass = null;	    
			BlueprintCharacterClass nClass = null;
			BlueprintCharacterClass mythClass = null;
			int mLevel = -1;
			int nLevel = -1;
			int mythLevel = 0;

			// allClasses
			// Mythic

			foreach (Kingmaker.UnitLogic.ClassData cInstance in npc.Progression.Classes)
			{
				BlueprintCharacterClass cMeta = cInstance.CharacterClass;
				int clevel = cInstance.Level;
				logDebug($"Class Name [{cMeta.NameSafe().ToUpper()}] Level [{clevel}] detected");
				if  (cMeta.PrestigeClass || cMeta.IsMythic)
                {
					clevel = clevel << 1;
					if (cMeta.IsMythic)
                    {
						logDebug($"  -> is Mythic Class");
						mythClass = cMeta;
						mythLevel = clevel;
                    } 
					else
                    {
						logDebug($"  -> is Prestige Class");
                    }
                }
				if (-1 == mLevel) {
			        mLevel = clevel;
			        mClass = cMeta;
			    } else if (clevel > mLevel) {
					if (nLevel != -1)
                    {
						nLevel = mLevel;
						nClass = mClass;
                    }
			        mLevel = clevel;
			        mClass = cMeta;
			    } else if (clevel > nLevel) {
                    nLevel = clevel;
                    nClass = cMeta;
				}
			}

			if (mLevel == -1) return false;
			if (nLevel == -1)
            {
 			    nLevel = mLevel;
			    nClass = mClass;
            }

			string mName    = mClass.NameSafe().ToUpper();
			string nName    = nClass.NameSafe().ToUpper();
			string mythName = mythClass.NameSafe().ToUpper();
			Boolean changed = false;

			if ((this.maxClassLevel != mLevel) || (this.nextToMaxClassLevel != nLevel) || (this.mythicClassLevel != mythLevel) || (!(this.maxClassName.Equals(mName))) || (!(this.nextToMaxClassName.Equals(nName))))
            {				
				// ushort maxArchType       = (ushort)(CLASSARCHTYPE.None);
				// ushort nextToMaxArchType = (ushort)(CLASSARCHTYPE.None);
				if (0 != mythLevel)
                {
					this.mythicClassLevel = mythLevel;

					this.USHORT_VALUES[(int)PROPERTY.MythicClassLevel] = (ushort)(mythLevel);

					int mythicOffset = ((int)PROPERTY.MythicClass) >> 6;
					CLASSMYTHIC mythEnum = CLASSMYTHIC.None;
					Meta.ClassToMythic.TryGetValue(mythName, out mythEnum);
					if (mythEnum != CLASSMYTHIC.None) { 
						this.ENUM_VALUES[mythicOffset] = (ushort)(mythEnum);
						changed = true;
					}
                }
				if (-1 != mLevel) {


					this.maxClassLevel       = mLevel;
					this.nextToMaxClassLevel = nLevel;
					this.maxClassName        = mName;
					this.nextToMaxClassName  = nName;

					this.USHORT_VALUES[(int)PROPERTY.PrimaryClassArchTypeLevel]   = (ushort)(mLevel);
					this.USHORT_VALUES[(int)PROPERTY.SecondaryClassArchTypeLevel] = (ushort)(nLevel);

					ushort[] archTypeWeights = new ushort[] { 0,0,0,0,0,0,0,0 };
					if (Meta.ClassToArchTypeList.ContainsKey(mName))
                    {
						int weight = 5;
						foreach (ushort archType in Meta.ClassToArchTypeList[mName])
                        {
							int offset = (archType == (ushort)(CLASSARCHTYPE.Bard))     ? 7 : (archType == (ushort)(CLASSARCHTYPE.Thief))    ? 6 :
									 	 (archType == (ushort)(CLASSARCHTYPE.Druid))    ? 5 : (archType == (ushort)(CLASSARCHTYPE.Cleric))   ? 4 :
							 			 (archType == (ushort)(CLASSARCHTYPE.Sorceror)) ? 3 : (archType == (ushort)(CLASSARCHTYPE.Wizard))   ? 2 :
										 (archType == (ushort)(CLASSARCHTYPE.Monk))     ? 1 : (archType == (ushort)(CLASSARCHTYPE.Fighter))  ? 0 : -1;
							if (-1 != offset) archTypeWeights[offset]+=(ushort)((mLevel*weight));
							weight = weight - (weight >> 1); //  weight values:  5, 3, 2 [, 1, 1, 1, 1.....]
						}
					} 
					else
                    {
						logDebug($"Unknown Max Class [{mName}]");
                    }
					if (Meta.ClassToArchTypeList.ContainsKey(nName))
					{
						int weight = 5;
						foreach (ushort archType in Meta.ClassToArchTypeList[nName])
						{
							int offset = (archType == (ushort)(CLASSARCHTYPE.Bard))     ? 7 : (archType == (ushort)(CLASSARCHTYPE.Thief))    ? 6 :
									 	 (archType == (ushort)(CLASSARCHTYPE.Druid))    ? 5 : (archType == (ushort)(CLASSARCHTYPE.Cleric))   ? 4 :
							 			 (archType == (ushort)(CLASSARCHTYPE.Sorceror)) ? 3 : (archType == (ushort)(CLASSARCHTYPE.Wizard))   ? 2 :
										 (archType == (ushort)(CLASSARCHTYPE.Monk))     ? 1 : (archType == (ushort)(CLASSARCHTYPE.Fighter))  ? 0 : -1; 
							if (-1 != offset) archTypeWeights[offset]+=(ushort)((nLevel* weight));
							weight = weight - (weight >> 1); //  weight values:  5, 3, 2 [, 1, 1, 1, 1.....]
						}
					}
					else
					{
						logDebug($"Unknown Next-To-Max Class [{nName}]");
					}

					int  maxWeight = 0;
					int  maxWeightIndex = -1;
					int  nextToMaxWeight = 0;
					int  nextToMaxWeightIndex = -1;

					for (int i = 0;i < 8;i++)
                    {
						if (archTypeWeights[i] > maxWeight)
                        {
							maxWeight = archTypeWeights[i];
							maxWeightIndex = i;
                        } 
						else if (archTypeWeights[i] > nextToMaxWeight)
                        {
							nextToMaxWeight = archTypeWeights[i];
							nextToMaxWeightIndex = i;
                        }
                    }
					if (maxWeightIndex > -1)
                    {
						int primaryCategoryOffset   = ((int)PROPERTY.PrimaryClassCategory)   >> 6;
						int secondaryCategoryOffset = ((int)PROPERTY.SecondaryClassCategory) >> 6;
						int primaryArchTypeOffset   = ((int)PROPERTY.PrimaryClassArchType)   >> 6;
						int secondaryArchTypeOffset = ((int)PROPERTY.SecondaryClassArchType) >> 6;

						if (nextToMaxWeightIndex == -1)
                        {
							nextToMaxWeightIndex = maxWeightIndex;
							nextToMaxWeight      = maxWeight;
                        }

						if (maxWeightIndex == 7) // CLASSARCHTYPE.Bard
                        {
							this.ENUM_VALUES[primaryCategoryOffset] = (ushort)(CLASSCATEGORY.Rogue);
							this.ENUM_VALUES[primaryArchTypeOffset] = (ushort)(CLASSARCHTYPE.Bard);
                        }
						else if (maxWeightIndex == 6) // CLASSARCHTYPE.Thief
                        {
							this.ENUM_VALUES[primaryCategoryOffset] = (ushort)(CLASSCATEGORY.Rogue);
							this.ENUM_VALUES[primaryArchTypeOffset] = (ushort)(CLASSARCHTYPE.Thief);
                        }
						else if (maxWeightIndex == 5) // CLASSARCHTYPE.Druid
                        {
							this.ENUM_VALUES[primaryCategoryOffset] = (ushort)(CLASSCATEGORY.Healer);
							this.ENUM_VALUES[primaryArchTypeOffset] = (ushort)(CLASSARCHTYPE.Druid);
                        }
						else if (maxWeightIndex == 4) // CLASSARCHTYPE.Cleric
                        {
							this.ENUM_VALUES[primaryCategoryOffset] = (ushort)(CLASSCATEGORY.Healer);
							this.ENUM_VALUES[primaryArchTypeOffset] = (ushort)(CLASSARCHTYPE.Cleric);
                        }
						else if (maxWeightIndex == 3) // CLASSARCHTYPE.Sorceror
                        {
							this.ENUM_VALUES[primaryCategoryOffset] = (ushort)(CLASSCATEGORY.Mage);
							this.ENUM_VALUES[primaryArchTypeOffset] = (ushort)(CLASSARCHTYPE.Sorceror);
                        }
						else if (maxWeightIndex == 2) // CLASSARCHTYPE.Wizard
                        {
							this.ENUM_VALUES[primaryCategoryOffset] = (ushort)(CLASSCATEGORY.Mage);
							this.ENUM_VALUES[primaryArchTypeOffset] = (ushort)(CLASSARCHTYPE.Wizard);
                        }
						else if (maxWeightIndex == 1) // CLASSARCHTYPE.Monk
                        {
							this.ENUM_VALUES[primaryCategoryOffset] = (ushort)(CLASSCATEGORY.Warrior);
							this.ENUM_VALUES[primaryArchTypeOffset] = (ushort)(CLASSARCHTYPE.Monk);
                        }
						else // assume (maxWeightIndex == 0) // CLASSARCHTYPE.Fighter
                        {
							this.ENUM_VALUES[primaryCategoryOffset] = (ushort)(CLASSCATEGORY.Warrior);
							this.ENUM_VALUES[primaryArchTypeOffset] = (ushort)(CLASSARCHTYPE.Fighter);
                        }


						if (nextToMaxWeightIndex == maxWeightIndex)
                        {
							this.ENUM_VALUES[secondaryCategoryOffset] = this.ENUM_VALUES[primaryCategoryOffset];
							this.ENUM_VALUES[secondaryArchTypeOffset] = this.ENUM_VALUES[primaryArchTypeOffset];
                        } 
						else
                        {
							if (nextToMaxWeightIndex == 7) // CLASSARCHTYPE.Bard
							{
								this.ENUM_VALUES[secondaryCategoryOffset] = (ushort)(CLASSCATEGORY.Rogue);
								this.ENUM_VALUES[secondaryArchTypeOffset] = (ushort)(CLASSARCHTYPE.Bard);
							}
							else if (nextToMaxWeightIndex == 6) // CLASSARCHTYPE.Theif
							{
								this.ENUM_VALUES[secondaryCategoryOffset] = (ushort)(CLASSCATEGORY.Rogue);
								this.ENUM_VALUES[secondaryArchTypeOffset] = (ushort)(CLASSARCHTYPE.Thief);
							}
							else if (nextToMaxWeightIndex == 5) // CLASSARCHTYPE.Druid
							{
								this.ENUM_VALUES[secondaryCategoryOffset] = (ushort)(CLASSCATEGORY.Healer);
								this.ENUM_VALUES[secondaryArchTypeOffset] = (ushort)(CLASSARCHTYPE.Druid);
							}
							else if (nextToMaxWeightIndex == 4) // CLASSARCHTYPE.Cleric
							{
								this.ENUM_VALUES[secondaryCategoryOffset] = (ushort)(CLASSCATEGORY.Healer);
								this.ENUM_VALUES[secondaryArchTypeOffset] = (ushort)(CLASSARCHTYPE.Cleric);
							}
							else if (nextToMaxWeightIndex == 3) // CLASSARCHTYPE.Sorceror
							{
								this.ENUM_VALUES[secondaryCategoryOffset] = (ushort)(CLASSCATEGORY.Mage);
								this.ENUM_VALUES[secondaryArchTypeOffset] = (ushort)(CLASSARCHTYPE.Sorceror);
							}
							else if (nextToMaxWeightIndex == 2) // CLASSARCHTYPE.Wizard
							{
								this.ENUM_VALUES[secondaryCategoryOffset] = (ushort)(CLASSCATEGORY.Mage);
								this.ENUM_VALUES[secondaryArchTypeOffset] = (ushort)(CLASSARCHTYPE.Wizard);
							}
							else if (nextToMaxWeightIndex == 1) // CLASSARCHTYPE.Monk
							{
								this.ENUM_VALUES[secondaryCategoryOffset] = (ushort)(CLASSCATEGORY.Warrior);
								this.ENUM_VALUES[secondaryArchTypeOffset] = (ushort)(CLASSARCHTYPE.Monk);
							}
							else /// assume nextToMaxWeightIndex == 0 / CLASSARCHTYPE.Fighter
							{
								this.ENUM_VALUES[secondaryCategoryOffset] = (ushort)(CLASSCATEGORY.Warrior);
								this.ENUM_VALUES[secondaryArchTypeOffset] = (ushort)(CLASSARCHTYPE.Fighter);
							}

							logDebug($"computed primaryArchType [{((CLASSARCHTYPE)this.ENUM_VALUES[primaryArchTypeOffset])}] final weight [{archTypeWeights[maxWeightIndex]}]");
							logDebug($"computed secondaryArchType [{((CLASSARCHTYPE)this.ENUM_VALUES[secondaryArchTypeOffset])}] final weight [{archTypeWeights[nextToMaxWeightIndex]}]");

                        }
						changed = true;
                    }
				}
			}
			return changed;
		}

		// ---------------------------------------------------------
		// PARSER LOOKUPS:
		// ---------------------------------------------------------
		public static RACEMASK strToRaceMask(string r)
		{
			string ru = r.ToUpper();
			if (!Meta.StrToRaceMask.ContainsKey(ru))
			{
				logAlways($"Error processing Race [{r}]: does not match any known race. Using Human");
				ru = "HUMAN";
			}
			return Meta.StrToRaceMask[ru];
		}

		public static ALIGNMENT strToAlignment(string a)
		{
			string au = a.ToUpper();
			if (!Meta.StrToAlignmentEnum.ContainsKey(au))
			{
				logAlways($"Error processing Alignment [{a}]: Does not match any known alignment. Using TrueNeutral");
				au = "TRUENEUTRAL";
			}
			return Meta.StrToAlignmentEnum[au];
		}

		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Stats: Attributes");
			sw.WriteLine("------------------------------------------------------------------------------");
			logDebug("Computing Stats: Attributes...");
			sw.WriteLine($"Str_Base [{USHORT_VALUES[(int)PROPERTY.Str_Base]}] Str [{USHORT_VALUES[(int)PROPERTY.Str]}]");
			sw.WriteLine($"Dex_Base [{USHORT_VALUES[(int)PROPERTY.Dex_Base]}] Dex [{USHORT_VALUES[(int)PROPERTY.Dex]}]");
			sw.WriteLine($"Con_Base [{USHORT_VALUES[(int)PROPERTY.Con_Base]}] Con [{USHORT_VALUES[(int)PROPERTY.Con]}]");
			sw.WriteLine($"Int_Base [{USHORT_VALUES[(int)PROPERTY.Int_Base]}] Int [{USHORT_VALUES[(int)PROPERTY.Int]}]");
			sw.WriteLine($"Wis_Base [{USHORT_VALUES[(int)PROPERTY.Wis_Base]}] Wis [{USHORT_VALUES[(int)PROPERTY.Wis]}]");
			sw.WriteLine($"Chr_Base [{USHORT_VALUES[(int)PROPERTY.Chr_Base]}] Chr [{USHORT_VALUES[(int)PROPERTY.Chr]}]");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Stats: Saves");
			sw.WriteLine("------------------------------------------------------------------------------");
			logDebug("Computing Stats: Saves...");
			sw.WriteLine($"Fort_Base   [{USHORT_VALUES[(int)PROPERTY.Fort_Base]  }] Fort   [{USHORT_VALUES[(int)PROPERTY.Fort  ]}]");
			sw.WriteLine($"Will_Base   [{USHORT_VALUES[(int)PROPERTY.Will_Base]  }] Will   [{USHORT_VALUES[(int)PROPERTY.Will  ]}]");
			sw.WriteLine($"Reflex_Base [{USHORT_VALUES[(int)PROPERTY.Reflex_Base]}] Reflex [{USHORT_VALUES[(int)PROPERTY.Reflex]}]");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Stats: Skills");
			sw.WriteLine("------------------------------------------------------------------------------");
			logDebug("Computing Stats: Skills...");
			sw.WriteLine($"Mobility_Base        [{USHORT_VALUES[(int)PROPERTY.Mobility_Base       ]}] Mobility        [{USHORT_VALUES[(int)PROPERTY.Mobility       ]}]");
			sw.WriteLine($"Athletics_Base       [{USHORT_VALUES[(int)PROPERTY.Athletics_Base      ]}] Athletics       [{USHORT_VALUES[(int)PROPERTY.Athletics      ]}]");
			sw.WriteLine($"Perception_Base      [{USHORT_VALUES[(int)PROPERTY.Perception_Base     ]}] Perception      [{USHORT_VALUES[(int)PROPERTY.Perception     ]}]");
			sw.WriteLine($"Thievery_Base        [{USHORT_VALUES[(int)PROPERTY.Thievery_Base       ]}] Thievery        [{USHORT_VALUES[(int)PROPERTY.Thievery       ]}]");
			sw.WriteLine($"LoreNature_Base      [{USHORT_VALUES[(int)PROPERTY.LoreNature_Base     ]}] LoreNature      [{USHORT_VALUES[(int)PROPERTY.LoreNature     ]}]");
			sw.WriteLine($"KnowledgeArcana_Base [{USHORT_VALUES[(int)PROPERTY.KnowledgeArcana_Base]}] KnowledgeArcana [{USHORT_VALUES[(int)PROPERTY.KnowledgeArcana]}]");
			sw.WriteLine($"Persuasion_Base      [{USHORT_VALUES[(int)PROPERTY.Persuasion_Base     ]}] Persuasion      [{USHORT_VALUES[(int)PROPERTY.Persuasion     ]}]");
			sw.WriteLine($"Stealth_Base         [{USHORT_VALUES[(int)PROPERTY.Stealth_Base        ]}] Stealth         [{USHORT_VALUES[(int)PROPERTY.Stealth        ]}]");
			sw.WriteLine($"UseMagicDevice_Base  [{USHORT_VALUES[(int)PROPERTY.UseMagicDevice_Base ]}] UseMagicDevice  [{USHORT_VALUES[(int)PROPERTY.UseMagicDevice ]}]");
			sw.WriteLine($"LoreReligion_Base    [{USHORT_VALUES[(int)PROPERTY.LoreReligion_Base   ]}] LoreReligion    [{USHORT_VALUES[(int)PROPERTY.LoreReligion   ]}]");
			sw.WriteLine($"KnowledgeWorld_Base  [{USHORT_VALUES[(int)PROPERTY.KnowledgeWorld_Base ]}] KnowledgeWorld  [{USHORT_VALUES[(int)PROPERTY.KnowledgeWorld ]}]");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Stats: Enums");
			sw.WriteLine("------------------------------------------------------------------------------");
			logDebug("Computing Stats: Enums...");

			// The compiler should optimize these out and replace them with constants...

			int civilityOffset      = ((int)PROPERTY.Civility)  >> 6;
			int moralityOffset      = ((int)PROPERTY.Morality)  >> 6;
			int alignmentOffset     = ((int)PROPERTY.Alignment) >> 6;
			int acuityOffset        = ((int)PROPERTY.Acuity)    >> 6;
			int raceOffset          = ((int)PROPERTY.Race)      >> 6;
			int sizeOffset          = ((int)PROPERTY.Size)      >> 6;
			int sizeOffsetBase      = ((int)PROPERTY.Size_Base) >> 6;
			int genderOffset        = ((int)PROPERTY.Gender)    >> 6;
			int healthOffset        = ((int)PROPERTY.Health)    >> 6;


			int classPrimCatOffset  = ((int)PROPERTY.PrimaryClassCategory)   >> 6; // [64]: PrimaryClassCategory
			int classSecCatOffset   = ((int)PROPERTY.SecondaryClassCategory) >> 6; // [65]: SecondaryClassCategory
			int classPrimArchOffset = ((int)PROPERTY.PrimaryClassArchType)   >> 6; // [66]: PrimaryClassArchType
			int classSecArchOffset  = ((int)PROPERTY.SecondaryClassArchType) >> 6; // [67]: SecondaryClassArchType
			int classMythOffset     = ((int)PROPERTY.MythicClass)            >> 6; // [69]: MythicClass

			// TODO: Add computation of "AllClassArchTypes"
			sw.WriteLine($"Class: Primary   Category  [{(CLASSCATEGORY)ENUM_VALUES[classPrimCatOffset] }]");
			sw.WriteLine($"Class: Secondary Category  [{(CLASSCATEGORY)ENUM_VALUES[classSecCatOffset]  }]");
			sw.WriteLine($"Class: Primary   ArchType  [{(CLASSARCHTYPE)ENUM_VALUES[classPrimArchOffset]}] Level [{USHORT_VALUES[(int)PROPERTY.PrimaryClassArchTypeLevel]   }]");
			sw.WriteLine($"Class: Secondary ArchType  [{(CLASSARCHTYPE)ENUM_VALUES[classSecArchOffset] }] Level [{USHORT_VALUES[(int)PROPERTY.SecondaryClassArchTypeLevel] }]");
			sw.WriteLine($"Class: Mythic              [{(CLASSMYTHIC)ENUM_VALUES[classMythOffset]      }] Level [{USHORT_VALUES[(int)PROPERTY.MythicClassLevel]            }]");
			sw.WriteLine($"Civility  [{(CIVILITY)ENUM_VALUES[civilityOffset]}]");
			sw.WriteLine($"Morality  [{(MORALITY)ENUM_VALUES[moralityOffset]}]");
			sw.WriteLine($"Alignment [{(ALIGNMENT)ENUM_VALUES[alignmentOffset]}]");
			sw.WriteLine($"Acuity    [{(ACUITY)ENUM_VALUES[acuityOffset]}]");
			sw.WriteLine($"Race      [{(RACEMASK)ENUM_VALUES[raceOffset]}]");
			sw.WriteLine($"Gender    [{(GENDER)ENUM_VALUES[genderOffset]}]");
			sw.WriteLine($"Health    [{(HEALTHENUM)ENUM_VALUES[healthOffset]}]");
			sw.WriteLine($"Size      [{(NPCSIZE)ENUM_VALUES[sizeOffset]}]");
			sw.WriteLine($"Size_Base [{(NPCSIZE)ENUM_VALUES[sizeOffsetBase]}]");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Misc");
			sw.WriteLine("------------------------------------------------------------------------------");
			logDebug("Computing Misc...");
			sw.WriteLine($"Level         [{USHORT_VALUES[(int)PROPERTY.Level]        }] Level_Base      [{USHORT_VALUES[(int)PROPERTY.Level_Base]}]");
			sw.WriteLine($"Initiative    [{USHORT_VALUES[(int)PROPERTY.Initiative]   }] Initiative_Base [{USHORT_VALUES[(int)PROPERTY.Initiative_Base]}]");
			sw.WriteLine($"Speed         [{USHORT_VALUES[(int)PROPERTY.Speed]        }] Speed_Base      [{USHORT_VALUES[(int)PROPERTY.Speed_Base]}]");
			sw.WriteLine($"AC            [{USHORT_VALUES[(int)PROPERTY.AC]           }] AC_Base         [{USHORT_VALUES[(int)PROPERTY.AC_Base]}]");
			sw.WriteLine($"Corruption    [{USHORT_VALUES[(int)PROPERTY.Corruption]   }] Corruption_Max  [{USHORT_VALUES[(int)PROPERTY.Corruption_Max]}]");
			sw.WriteLine($"AC_TOUCH      [{USHORT_VALUES[(int)PROPERTY.AC_TOUCH]    }]");
			sw.WriteLine($"AC_FLATFOOTED [{USHORT_VALUES[(int)PROPERTY.AC_FLATFOOTED]}]");
			sw.WriteLine($"XP            [{UINT_VALUES[(((int)PROPERTY.XP)>> 13)]}]");
			sw.WriteLine($"Corruption_Precent [{USHORT_VALUES[(int)PROPERTY.Corruption_Percent]}]");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Health");
			sw.WriteLine("------------------------------------------------------------------------------");
			logDebug("Computing Health...");
			sw.WriteLine($"HP_Base       [{USHORT_VALUES[(int)PROPERTY.HP_Base]      }] HP              [{USHORT_VALUES[(int)PROPERTY.HP]}] ");
			sw.WriteLine($"HP_Max        [{USHORT_VALUES[(int)PROPERTY.HP_Max]       }] HP_Mod          [{USHORT_VALUES[(int)PROPERTY.HP_Mod]}]");
			sw.WriteLine($"HP_Percent    [{USHORT_VALUES[(int)PROPERTY.HP_Percent]   }]");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Current Tags: Facts");
			sw.WriteLine("------------------------------------------------------------------------------");
			logDebug("Computing Facts...");
			HashSet<String> factsPtr       = STRSET_VALUES[((int)PROPERTY.Facts) >> 10];
			HashSet<String> buffsPtr       = STRSET_VALUES[((int)PROPERTY.Buffs) >> 10];
			HashSet<String> sharedStashPtr = STRSET_VALUES[((int)PROPERTY.SharedStash) >> 10];
			HashSet<String> inventoryPtr   = STRSET_VALUES[((int)PROPERTY.Inventory) >> 10];
			HashSet<String> equippedPtr    = STRSET_VALUES[((int)PROPERTY.Equipped) >> 10];

			foreach (string tag in factsPtr)
			{
				sw.WriteLine($"[{tag}]");
			}
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Current Buffs");
			sw.WriteLine("------------------------------------------------------------------------------");
			logDebug("Computing Buffs...");
			foreach (string tag in buffsPtr)
			{
				sw.WriteLine($"[{tag}]");
			}
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Current Shared Stash (All items available to party, assigned, equipped or not)");
			sw.WriteLine("------------------------------------------------------------------------------");
			logDebug("Computing Shared...");
			foreach (string item in sharedStashPtr)
			{
				sw.WriteLine($"[{item}]");
			}
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Current Inventory (Items assigned to NPC)");
			sw.WriteLine("------------------------------------------------------------------------------");
			logDebug("Computing Inventory...");
			foreach (string item in inventoryPtr)
			{
				sw.WriteLine($"[{item}]");
			}
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Current Equipped (Items readied/worn by NPC)");
			sw.WriteLine("------------------------------------------------------------------------------");
			logDebug("Computing Equipped...");
			foreach (string item in equippedPtr)
			{
				sw.WriteLine($"[{item}]");
			}
            return sw.ToString();
		}

        // Enum Typing Support
		public static PROPERTYTYPE GetPropType(PROPERTY prop)
        {
			if ( (((ushort)prop) & Meta.ushortRange) != 0) return Meta.PROPERTYTYPES[((int)prop)];
			if ( (((ushort)prop) & Meta.enumRange)   != 0) return Meta.PROPERTYTYPES[63 + (((int)prop) >> 6)];
			if ( (((ushort)prop) & Meta.setRange)    != 0) return Meta.PROPERTYTYPES[78 + (((int)prop )>> 10)];
			if ( (((ushort)prop) & Meta.uintRange)   != 0) return Meta.PROPERTYTYPES[85 + (((int)prop) >> 13)];
			return Meta.PROPERTYTYPES[0];
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
	*/

}

