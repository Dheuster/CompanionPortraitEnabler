using System; // String
using System.IO; // StringWriter
using System.Collections.Generic; // List, Dictionary, Set, etc...


using Kingmaker;                                // Required for Game
using Kingmaker.EntitySystem.Entities;          // Required for UnitEntityData
using Kingmaker.Blueprints.Root;                // Required for BlueprintRoot
using Kingmaker.UnitLogic.Abilities.Blueprints; // Required for BlueprintAbility
using Kingmaker.DialogSystem.Blueprints;        // Required for BlueprintDialog
using Kingmaker.Blueprints.Classes;             // Required for BlueprintCharacterClass, BlueprintRace
using Kingmaker.Blueprints;                     // Required for Race and extension methods (NameSafe)
using Kingmaker.UnitLogic;                      // Required for UnitDescriptor
using Kingmaker.Items;                          // Required for ItemEntity
using Kingmaker.AreaLogic.QuestSystem;          // Required for Quest



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
	// 1) RuleFactory: Provides the Rule Creation API. Used typically by devs to
    //                 create rules programatically
	//
	// 2) EventMonitor: Registers for events in order to keep a RuleContext up-to-date
	//                  and relavent for various tracked objects (NPCs). Typically
    //                  tracks when a rule-conext becomes stale and should be
    //                  re-evaluated again. We call this component NPCMonitor.
	//
	//#################################################################################

	public class RuleContext 
	{
		// (See Constants.cs for more info):
        // 
        // Rule Context uses a LONG bitmask to store what a rule encompasses:
        //
        //
		// USHORT VALUES : 0000 0000 0011 1111 :
        //
        //   6 bits provide a value between 0 and 63 to lookup a ushort in
        //   an array of ushorts
        //   
		// ENUMS         : 0000 0011 1100 0000 :
		//
		//   4 bits provide a value between 0 and 15 to lookup a uint
		//   mask that can be used to determine the presence of pre-known values
		//
		// Sets          : 0011 1100 0000 0000 :
		//
		//   4 bits provide a value between 0 and 15 too lookup a string set
		//   to confirm or deny a value is present. 
		//
		//   4 bits provide a value between 0 and 15 to lookup a uint
		//   mask that can be used to determine the presence of pre-known values
		// 
		// Ints          : 110 0000 0000 0000 :
		//
		//   2 bits provide a value between 0 and 3 to lookup an int from an array 
		//   of ints to compare a value against. 
		//
		// See:
        //   Constants.Meta.ushortRange = 0x003F; // 63             or 0b00 0000 0000 111111
		//   Constants.Meta.enumRange   = 0x03C0; // 960            or 0b00 0000 1111 000000
		//   Constants.Meta.setRange    = 0x3C00; // 15360          or 0b00 1111 0000 000000
		//   Constants.Meta.uintRange   = 0xC000; // 49152          or 0b11 0000 0000 000000


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

			5, // [6]  USHORT_VALUES[PROPERTY.Str] ?
			5, // [7]  USHORT_VALUES[PROPERTY.Dex] ?
			5, // [8]  USHORT_VALUES[PROPERTY.Con] ?
			5, // [9]  USHORT_VALUES[PROPERTY.Int] ?
			5, // [10] USHORT_VALUES[PROPERTY.Wis] ?
			5, // [11] USHORT_VALUES[PROPERTY.Chr] ?
			5, // [12] USHORT_VALUES[PROPERTY.Str_Base] ?
			5, // [13] USHORT_VALUES[PROPERTY.Dex_Base] ?
			5, // [14] USHORT_VALUES[PROPERTY.Con_Base] ?
			5, // [15] USHORT_VALUES[PROPERTY.Int_Base] ?
			5, // [16] USHORT_VALUES[PROPERTY.Wis_Base] ?
			5, // [17] USHORT_VALUES[PROPERTY.Chr_Base] ?

			// --------------------------------------
			// Stats : Saves (6 items)
			// --------------------------------------
			0, // [18] USHORT_VALUES[PROPERTY.Fort] ?
			0, // [19] USHORT_VALUES[PROPERTY.Will] ?
			0, // [20] USHORT_VALUES[PROPERTY.Reflex] ?
			0, // [21] USHORT_VALUES[PROPERTY.Fort_Base] ?
			0, // [22] USHORT_VALUES[PROPERTY.Will_Base] ?
			0, // [23] USHORT_VALUES[PROPERTY.Reflex_Base] ?

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

			0,  // [50] USHORT_VALUES[PROPERTY.Initiative] ?
			0,  // [51] USHORT_VALUES[PROPERTY.Initiative_Base] ?

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

       		0, // [57] UNUSED // PREVIOUSLY : USHORT_VALUES[PROPERTY.PrimaryClassArchTypeLevel]
   			0, // [58] UNUSED // PREVIOUSLY USHORT_VALUES[PROPERTY.SecondaryClassArchTypeLevel]
   			1, // [59] USHORT_VALUES[PROPERTY.Level_Base]
			1, // [60] USHORT_VALUES[PROPERTY.Level]  // Current Invested Level (If player doesn't level up, this does not change).
			0, // [61] USHORT_VALUES[PROPERTY.InCombat] // 0 = false, 1 = true

			0, // [62] USHORT_VALUES[PROPERTY.IsNaked] // 0 = false, 1 = true
			0  // [63] USHORT_VALUES[PROPERTY.UsingDefaultEquipment] // 0 = false, 1 = true
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
			0,                             // [5] UNUSED // Previously CLASSARCHTYPE: ENUM_VALUES[PROPERTY.AllClassArchTypes      >> 6]
			0,                             // [6] UNUSED // Previously CLASSMYTHIC: ENUM_VALUES[PROPERTY.MythicClass            >> 6]

			0,                             // [7] UNUSED // Previously HEALTHENUM:ENUM_VALUES[PROPERTY.Health    >> 6]
			(ushort)RACEMASK.Human,        // [8] ENUM_VALUES[PROPERTY.Race      >> 6]
			0,                             // [9] UNUSED // Previously GENDER:ENUM_VALUES[PROPERTY.Gender    >> 6]
			(ushort)CIVILITY.Neutral,      // [10] ENUM_VALUES[PROPERTY.Civility  >> 6] ?
			(ushort)MORALITY.Neutral,      // [11] ENUM_VALUES[PROPERTY.Morality  >> 6] ?
			(ushort)ALIGNMENT.TrueNeutral, // [12] ENUM_VALUES[PROPERTY.Alignment >> 6]
			(ushort)ACUITY.Rested,         // [13] ENUM_VALUES[PROPERTY.Acuity    >> 6]
			(ushort)NPCSIZE.Medium,        // [14] ENUM_VALUES[PROPERTY.Size      >> 6]
			(ushort)NPCSIZE.Medium         // [15] ENUM_VALUES[PROPERTY.Size_Base >> 6] ?
		};

	    //======================================================================
		// Open Sets (Maps)
	    //======================================================================

		public HashSet<string>[] STRSET_VALUES = new HashSet<string>[]
        {
			new HashSet<string>(),     // [0] STRSET_VALUES[PROPERTY.None              >> 10] (Unused)
			new HashSet<string>(),     // [1] STRSET_VALUES[PROPERTY.Facts             >> 10]
			new HashSet<string>(),     // [2] STRSET_VALUES[PROPERTY.Buffs             >> 10]
			new HashSet<string>(),     // [3] STRSET_VALUES[PROPERTY.SharedStash       >> 10]
			new HashSet<string>(),     // [4] STRSET_VALUES[PROPERTY.Inventory         >> 10]
			new HashSet<string>(),     // [5] STRSET_VALUES[PROPERTY.EquippedArmor     >> 10]
			new HashSet<string>(),     // [6] STRSET_VALUES[PROPERTY.EquippedWeapons   >> 10]
			new HashSet<string>(),     // [7] STRSET_VALUES[PROPERTY.EquippedRings     >> 10]
			new HashSet<string>(),     // [8] STRSET_VALUES[PROPERTY.EquippedNecklaces >> 10]
			new HashSet<string>(),     // [9] STRSET_VALUES[PROPERTY.Equipped          >> 10]
			new HashSet<string>(),     // [10] STRSET_VALUES[PROPERTY.ActiveQuests     >> 10]
			new HashSet<string>(),     // [11] STRSET_VALUES[PROPERTY.CompletedQuests  >> 10]
			new HashSet<string>(),     // [12] STRSET_VALUES[PROPERTY.FailedQuests     >> 10]
			new HashSet<string>(),     // [13] STRSET_VALUES[PROPERTY.KnownQuests      >> 10]
			new HashSet<string>(),     // [14] STRSET_VALUES[PROPERTY.Dialog           >> 10]
			new HashSet<string>()      // [15] STRSET_VALUES[PROPERTY.Area             >> 10]
        };

		public Dictionary<string,string> AllItems = new Dictionary<string, string>();

	    //======================================================================
		// Integers (Value may exceed limit of short = 16384)
	    //======================================================================

		public uint[] UINT_VALUES = new uint[] {
			0,                         // [0] UINT_VALUES[PROPERTY.None >> 14] // (Unused)
			0                          // [1] UINT_VALUES[PROPERTY.XP   >> 14] // Experience Points.
		};
			


		// --------------------------------------------------------------------------------------
		// --------------------------------------------------------------------------------------
		//           LOCAL SUPPORT BELOW THIS LINE. DO NOT REFERENCE VARIABLES
		// --------------------------------------------------------------------------------------
		// --------------------------------------------------------------------------------------

		// local
	    // long hashCode = 0;

		// ------------------------------------------------------------------
		// Constructor
		// ------------------------------------------------------------------
		public RuleContext(UnitEntityData npc = null)
		{
			if (null != npc) {
				updateBase(npc);
				updateBuffs(npc);
				updateStats(npc);
				updateFacts(npc);
				updateClasses(npc);
				updateEquipped(npc);
				updateDialog(npc);
				updateQuests(npc);
				updateInventory(npc);
				updateHealth(npc);
			}
		}

		// Class Change Tracking
		private string maxClassName        = "";
		private int    maxClassLevel       = 0;
		private string nextToMaxClassName  = "";
		private int    nextToMaxClassLevel = 0;

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

		// Unavailable Abilities/Buffs count
		private int unavailableFacts = 0;

		// State Tracking
		private int inCombat = 0;
		private int isNaked  = 0;
		private int UsingDefaultEquipment = 0;


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

			// ENUM_VALUES[ ((int)PROPERTY.Health) >> 6] = (hp_percent > 75) ? (ushort)(HEALTHENUM.HP_75_to_100) :
			//	                                        (hp_percent > 50) ? (ushort)(HEALTHENUM.HP_50_to_75) :
			//	                                        (hp_percent > 25) ? (ushort)(HEALTHENUM.HP_25_to_50) :
			//	                                        (ushort)(HEALTHENUM.HP_0_to_25);
		}

		public void updateCombatState(UnitEntityData npc)
		{
			this.USHORT_VALUES[(int)PROPERTY.InCombat] = (ushort)(npc.IsInCombat ? 1 : 0);
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
			UINT_VALUES[((int)PROPERTY.XP) >> 14] = (uint)(npc.Descriptor.Progression.Experience);
			ushort corruption =  (ushort)(Game.Instance.Player.Corruption.CurrentValue);
			USHORT_VALUES[(int)PROPERTY.Corruption] = corruption;
			int discard = 0;
			USHORT_VALUES[(int)PROPERTY.Corruption_Percent] = (ushort)(Math.DivRem(USHORT_VALUES[(int)PROPERTY.Corruption_Max] * corruption, 100, out discard));
			// ENUM_VALUES[((int)PROPERTY.Gender) >> 6] = (npc.Gender == Kingmaker.Blueprints.Gender.Male) ? (ushort)(GENDER.Male) : (ushort)(GENDER.Female);

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
			int count = 0;
			bool earlyBail = true;

			int currentUnavailableFacts = 0;
			foreach (Kingmaker.UnitLogic.Abilities.Ability a in npc.Abilities.RawFacts) {
				if (!(a.Data?.IsAvailable ?? true)) {
					currentUnavailableFacts++;
                }
			}
			foreach (Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbility aa in npc.ActivatableAbilities.RawFacts) {
				if (!(aa.IsAvailable && aa.IsOn)) {
					currentUnavailableFacts++;
                }
			}
			if (currentUnavailableFacts == this.unavailableFacts) {
				count = factSetRef.Count;
				if (0 != count) {
					foreach (Kingmaker.EntitySystem.EntityFact f in npc.Facts.List) {
						string name = f.Blueprint?.name;
						if (null != name) {
							if (factSetRef.Contains(name.ToUpper())) {
								count--;
							} else if (currentUnavailableFacts > 0) {
								// This is far from perfect. But it is the simplest/most 
								// performant thing I can think of... We have some 
								// leniency for misses, but the leniency isn't a 
								// dependable or precise value. 
								currentUnavailableFacts--;
							} else { 
								Log.debug("1 or more Facts Added");
								count = -1;
								break;
							}
						}
					}
					if (0 != count) {
						earlyBail = false;
                    }
				} else {
					Log.debug("Initializing Facts");
					earlyBail = false;
                }
            } else { 
				if (Log.debugEnabled) {
					Log.debug($"[{((this.unavailableFacts < currentUnavailableFacts) ? (currentUnavailableFacts - this.unavailableFacts) : (this.unavailableFacts - currentUnavailableFacts))}] Ability Availabilities Changed");
				}				
				this.unavailableFacts = currentUnavailableFacts;
				earlyBail = false;
				count = -1;
            }
			if (earlyBail) {
				return false; // no change (avoid rebuild)
			}

			if (count > 0) {
				Log.debug($"[{count}] Facts Changed (Positive = Added. Negative = Removed)");
			}

			// Change took place (To visible UnitFact or Ability). Rebuild:
			factSetRef.Clear();
			foreach (Kingmaker.EntitySystem.EntityFact f in npc.Facts.List)
			{
				string name = f.Blueprint?.name;
				if (null != name) {
					factSetRef.Add(name.ToUpper()); // fact.Name.ToUpper()
				}

				// Kingmaker.Blueprints.Facts.BlueprintUnitFact fact = f.Blueprint as Kingmaker.Blueprints.Facts.BlueprintUnitFact;
				// if (null != fact && !string.IsNullOrEmpty(fact.Name))
				// {
				// 	factSetRef.Add(fact.Name.ToUpper()); // fact.Name.ToUpper()
				// }
			}
			if (this.unavailableFacts > 0) {
				foreach (Kingmaker.UnitLogic.Abilities.Ability a in npc.Abilities.RawFacts) {
					if (!(a.Data?.IsAvailable ?? true)) {
						string name = a.Blueprint?.name;
						if (null != name) {
							if (!factSetRef.Remove(name.ToUpper())) {
								Log.trace($"Failed to find/remove unavailable/disabled Fact [{name}] from facts");
                            };
						}
					}
				}
				foreach (Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbility aa in npc.ActivatableAbilities.RawFacts) {
					if (!(aa.IsAvailable && aa.IsOn)) {
						string name = aa.Blueprint?.name;
						if (null != name) {
							if (!factSetRef.Remove(name.ToUpper())) {
								Log.trace($"Failed to find/remove unavailable/disabled Fact [{name}] from facts");
                            };
						}
					}
				}
			}
			return true;
		}

		public bool updateBuffs(UnitEntityData npc)
		{
			int buffOffset = ((int)PROPERTY.Buffs) >> 10;
			HashSet<String> buffSetRef = STRSET_VALUES[buffOffset]; // HashSet is a class object, thus local var will be references, not copies.
			int count = buffSetRef.Count;

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
			if (Log.debugEnabled)
			{
				if (count > 0)
				{
					Log.debug($"TRACE: [{count}] Buffs Removed");
				}
				else
				{
					Log.debug($"TRACE1 or more Buffs Added");
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
			if (descriptor.Inventory.Items.Count == SharedStashCount)
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
                        Log.trace($"TRACE: 1 or more items unassiged from NPC");
						needsRefresh = true;
						break;
					}
					invCount++;
                }
				if (!(SharedStashRef.Contains($"{Name}")))
				{
                    Log.trace($"TRACE: 1 or more items removed from Shared Stash");
					needsRefresh = true;
					break;
				}
			}
			if (!needsRefresh)
            {
				if (stashCount > this.SharedStashCount)
                {
					Log.debug($"{(this.SharedStashCount - stashCount)} Items Added to Shared Stash");
					needsRefresh = true;
                }
				if (invCount > this.InventoryCount)
                {
					Log.debug($"{(this.InventoryCount - invCount)} New Items assigned to NPC");
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
			AllItems.Clear();

			this.InventoryCount   = 0;
			this.SharedStashCount = descriptor.Inventory.Items.Count;
			foreach (ItemEntity item in descriptor.Inventory)
			{
				string Name = item.Name.ToUpper();
				string assetId = ((BlueprintScriptableObject)item.Blueprint).AssetGuidThreadSafe;


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
				AllItems[Name] = assetId;
			}
			return true;
		}

		static readonly int EquippedArmorOffset     = ((int)PROPERTY.EquippedArmor) >> 10;
		static readonly int EquippedWeaponsOffset   = ((int)PROPERTY.EquippedWeapons) >> 10;
		static readonly int EquippedRingsOffset     = ((int)PROPERTY.EquippedRings) >> 10;
		static readonly int EquippedNecklacesOffset = ((int)PROPERTY.EquippedNecklaces) >> 10;
		static readonly int EquippedOffset          = ((int)PROPERTY.Equipped) >> 10;

		public bool updateEquipped(UnitEntityData npc)
		{
			HashSet<String> EquippedSetRef          = STRSET_VALUES[EquippedOffset]; // HashSet is a class object, thus local var will be references, not copies.

			// Need to add event monitor for IUnitEquipmentHandler/IUnitActiveEquipmentSetHandler. There is a slot updated method we can 
			// to to monitor when these should be ran.

            bool needsRefresh = (0 == EquippedSetRef.Count);
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
                needsRefresh = needsRefresh || (this.equipped_gloves != itemCheck);
				this.equipped_gloves = itemCheck;
            }
            else 
			{ 
                needsRefresh = needsRefresh || (this.equipped_gloves != "");
				this.equipped_gloves = "";
			}
			if (null != npc.Body.Neck.MaybeItem)
            {
				String itemCheck =  npc.Body.Neck.MaybeItem.Name.ToUpper();
                needsRefresh = needsRefresh || (this.equipped_neck != itemCheck);
				this.equipped_neck = itemCheck;
            }
            else 
			{ 
                needsRefresh = needsRefresh || (this.equipped_neck != "");
				this.equipped_neck = "";
			}
			if (null != npc.Body.Ring1.MaybeItem)
            {
				String itemCheck =  npc.Body.Ring1.MaybeItem.Name.ToUpper();
                needsRefresh = needsRefresh || (this.equipped_ring1 != itemCheck);
				this.equipped_ring1 = itemCheck;
            }
            else 
			{ 
                needsRefresh = needsRefresh || (this.equipped_ring1 != "");
				this.equipped_ring1 = "";
			}
			if (null != npc.Body.Ring2.MaybeItem)
            {
				String itemCheck =  npc.Body.Ring2.MaybeItem.Name.ToUpper();
                needsRefresh = needsRefresh || (this.equipped_ring2 != itemCheck);
				this.equipped_ring2 = itemCheck;
            }
            else 
			{ 
                needsRefresh = needsRefresh || (this.equipped_ring2 != "");
				this.equipped_ring2 = "";
			}
			if (null != npc.Body.Wrist.MaybeItem)
            {
				String itemCheck =  npc.Body.Wrist.MaybeItem.Name.ToUpper();
                needsRefresh = needsRefresh || (this.equipped_wrist != itemCheck);
				this.equipped_wrist = itemCheck;
            }
            else 
			{ 
                needsRefresh = needsRefresh || (this.equipped_wrist != "");
				this.equipped_wrist = "";
			}
			if (null != npc.Body.Shoulders.MaybeItem)
            {
				String itemCheck =  npc.Body.Shoulders.MaybeItem.Name.ToUpper();
                needsRefresh = needsRefresh || (this.equipped_shoulders != itemCheck);
				this.equipped_shoulders = itemCheck;
            }
            else 
			{ 
                needsRefresh = needsRefresh || (this.equipped_shoulders != "");
				this.equipped_shoulders = "";
			}
			if ((1 == this.UsingDefaultEquipment) != npc.UISettings.ShowClassEquipment)
            {
				needsRefresh = true;
				this.UsingDefaultEquipment = (1 == this.UsingDefaultEquipment) ? 0 : 1;
            }
			if ((1 == this.isNaked) != ((null == npc?.Body?.Shirt?.MaybeItem) && (!(npc.Body.Armor.HasArmor))))
			{ 
				needsRefresh = true;
				this.isNaked = (1 == this.isNaked) ? 0 : 1;

			}
			if (needsRefresh)
            {
				HashSet<String> EquippedArmorSetRef     = STRSET_VALUES[EquippedArmorOffset];     // HashSet is a class object, thus local var will be references, not copies.
				EquippedArmorSetRef.Clear();
				EquippedArmorSetRef.Add(equipped_armor);
				// Allow empty string 1 item set

				HashSet<String> EquippedWeaponsSetRef   = STRSET_VALUES[EquippedWeaponsOffset];   // HashSet is a class object, thus local var will be references, not copies.
				EquippedWeaponsSetRef.Clear();
				EquippedWeaponsSetRef.Add(equipped_primary);
				EquippedWeaponsSetRef.Add(equipped_secondary);
				// Allow empty string 1 item set
					
				HashSet<String> EquippedRingsSetRef     = STRSET_VALUES[EquippedRingsOffset];     // HashSet is a class object, thus local var will be references, not copies.
				EquippedRingsSetRef.Clear();
				EquippedRingsSetRef.Add(equipped_ring1);
				EquippedRingsSetRef.Add(equipped_ring2);
				// Allow empty string 1 item set

				HashSet<String> EquippedNecklacesSetRef = STRSET_VALUES[EquippedNecklacesOffset]; // HashSet is a class object, thus local var will be references, not copies.
				EquippedNecklacesSetRef.Clear();
				EquippedNecklacesSetRef.Add(equipped_neck);
				// Allow empty string 1 item set

				EquippedSetRef.Clear();
				EquippedSetRef.Add(equipped_primary);
				EquippedSetRef.Add(equipped_secondary);
				EquippedSetRef.Add(equipped_armor);
				EquippedSetRef.Add(equipped_ring1);
				EquippedSetRef.Add(equipped_ring2);
				EquippedSetRef.Add(equipped_neck);

				EquippedSetRef.Add(equipped_shirt);
				EquippedSetRef.Add(equipped_belt);
				EquippedSetRef.Add(equipped_head);
				EquippedSetRef.Add(equipped_eyes);
				EquippedSetRef.Add(equipped_feet);
				EquippedSetRef.Add(equipped_gloves);
				EquippedSetRef.Add(equipped_wrist);
				EquippedSetRef.Add(equipped_shoulders); // Cloak
				EquippedSetRef.Remove("");
				
				this.USHORT_VALUES[(int)PROPERTY.IsNaked] = (ushort)(((null == npc?.Body?.Shirt?.MaybeItem) && (!(npc.Body.Armor.HasArmor))) ? 1 : 0);
				this.USHORT_VALUES[(int)PROPERTY.UsingDefaultEquipment] = (ushort)((npc.UISettings.ShowClassEquipment) ? 1 : 0);
				return true;
            }
			return false;
		}

		public bool updateDialog(UnitEntityData npc)
		{
			int dialogueOffset = ((int)PROPERTY.Dialog) >> 10;
			HashSet<String> dialogSetRef = STRSET_VALUES[dialogueOffset]; // HashSet is a class object, thus local var will be reference, not copy.

			string currentDialog = null;
			BlueprintDialog blueprintDialog = Game.Instance.DialogController.Dialog;
			if (null != blueprintDialog)
            {
				currentDialog = blueprintDialog.name.ToUpper();
            }
			if (null == currentDialog)
            {
				if (dialogSetRef.Count != 0)
                {
					dialogSetRef.Clear();
					return true;
                }
				return false;
            }
			if (dialogSetRef.Contains(currentDialog))
            {
				return false;
            }
			dialogSetRef.Clear();
			dialogSetRef.Add(currentDialog);
			return true;
		}

		public bool updateArea(UnitEntityData npc)
		{
			Log.trace($"Update Area Called");
			int areaOffset       = ((int)PROPERTY.Area)     >> 10;
			// HashSet is a class object, thus local var will be reference, not copy
			HashSet<String> areaSetRef = STRSET_VALUES[areaOffset]; 
			int areaCount = areaSetRef.Count;

			string currentArea = Game.Instance?.CurrentlyLoadedArea?.name;
			if (null == currentArea)
            {
				Log.trace($"CurrentlyLoadedArea.name is null");
				return false;
            }
			currentArea = currentArea.ToUpper();
			if (areaSetRef.Contains(currentArea))
            {
				Log.trace($"Area [{currentArea}] has not changed");
				return false;
            }
			Log.trace($"New Area [{currentArea}] detected");
			areaSetRef.Clear();
			areaSetRef.Add(currentArea);
			return true;
		}

		public bool updateQuests(UnitEntityData npc)
		{
			int knownQuestsOffset       = ((int)PROPERTY.KnownQuests)     >> 10;
			// HashSet is a class object, thus local var will be reference, not copy.
			HashSet<String> knownSetRef = STRSET_VALUES[knownQuestsOffset]; 
			int knownCount = knownSetRef.Count;

			IEnumerable<Quest> allQuests = Game.Instance.Player.QuestBook.Quests;
			int allQuestsCount = 0;

			foreach(Quest quest in allQuests)
            {
				allQuestsCount++;
			}
			if (allQuestsCount <= knownCount)
            {
				return false; // no change (avoid rebuild)
            }

			Log.debug($"[{(allQuestsCount - knownCount)}] Quest Change events detected");

			int activeQuestsOffset         = ((int)PROPERTY.ActiveQuests)    >> 10;
			int completedQuestsOffset      = ((int)PROPERTY.CompletedQuests) >> 10;
			int failedQuestsOffset         = ((int)PROPERTY.FailedQuests)    >> 10;

			HashSet<String> activeSetRef   = STRSET_VALUES[activeQuestsOffset];
			HashSet<String> completedetRef = STRSET_VALUES[completedQuestsOffset];
			HashSet<String> failedSetRef   = STRSET_VALUES[failedQuestsOffset]; 

			foreach(Quest quest in allQuests)
            {
				string questToUpper = (String)quest.Blueprint.name.ToUpper();
				knownSetRef.Add(questToUpper);
				if (quest.State == QuestState.Started)
                {
					activeSetRef.Add(questToUpper);
                }
				else if (quest.State == QuestState.Completed)
                {
					completedetRef.Add(questToUpper);
                }
				else if (quest.State == QuestState.Failed)
                {
					failedSetRef.Add(questToUpper);
                }				
			}

			return true;
		}

		public bool updateClasses(UnitEntityData npc)
		{			
			BlueprintCharacterClass mClass = null;	    
			BlueprintCharacterClass nClass = null;
			// BlueprintCharacterClass mythClass = null;
			int mLevel = -1;
			int nLevel = -1;
			// int mythLevel = 0;

			// allClasses
			// Mythic

			foreach (Kingmaker.UnitLogic.ClassData cInstance in npc.Progression.Classes)
			{
				BlueprintCharacterClass cMeta = cInstance.CharacterClass;
				int clevel = cInstance.Level;
				Log.debug($"Class Name [{cMeta.NameSafe().ToUpper()}] Level [{clevel}] detected");
				if (!cMeta.IsMythic)
				{ 
					if  (cMeta.PrestigeClass) //  || cMeta.IsMythic
					{
						clevel = clevel << 1;
						//if (cMeta.IsMythic)
						//{
						//	Log.debug($"  -> is Mythic Class");
						//	mythClass = cMeta;
						//	mythLevel = clevel;
						//} 
						//else
						//{
							Log.debug($"  -> is Prestige Class");
						//}
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
			}

			if (mLevel == -1) return false;
			if (nLevel == -1)
            {
 			    nLevel = mLevel;
			    nClass = mClass;
            }

			string mName    = mClass.NameSafe().ToUpper();
			string nName    = nClass.NameSafe().ToUpper();
			// string mythName = mythClass.NameSafe().ToUpper();
			Boolean changed = false;

			if ((this.maxClassLevel != mLevel) || (this.nextToMaxClassLevel != nLevel) || (!(this.maxClassName.Equals(mName))) || (!(this.nextToMaxClassName.Equals(nName))))
            {				
				// ushort maxArchType       = (ushort)(CLASSARCHTYPE.None);
				// ushort nextToMaxArchType = (ushort)(CLASSARCHTYPE.None);
			//	if (0 != mythLevel)
            //    {
			//		this.mythicClassLevel = mythLevel;
			//
			//		this.USHORT_VALUES[(int)PROPERTY.MythicClassLevel] = (ushort)(mythLevel);
			//
			//		int mythicOffset = ((int)PROPERTY.MythicClass) >> 6;
			//		CLASSMYTHIC mythEnum = CLASSMYTHIC.None;
			//		Meta.StrToClassMythicEnum.TryGetValue(mythName, out mythEnum);
			//		if (mythEnum != CLASSMYTHIC.None) { 
			//			this.ENUM_VALUES[mythicOffset] = (ushort)(mythEnum);
			//			changed = true;
			//		}
             //   }
				if (-1 != mLevel) {


					this.maxClassLevel       = mLevel;
					this.nextToMaxClassLevel = nLevel;
					this.maxClassName        = mName;
					this.nextToMaxClassName  = nName;

					// this.USHORT_VALUES[(int)PROPERTY.PrimaryClassArchTypeLevel]   = (ushort)(mLevel);
					// this.USHORT_VALUES[(int)PROPERTY.SecondaryClassArchTypeLevel] = (ushort)(nLevel);

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
						Log.debug($"Unknown Max Class [{mName}]");
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
						Log.debug($"Unknown Next-To-Max Class [{nName}]");
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

							Log.debug($"computed primaryArchType [{((CLASSARCHTYPE)this.ENUM_VALUES[primaryArchTypeOffset])}] final weight [{archTypeWeights[maxWeightIndex]}]");
							Log.debug($"computed secondaryArchType [{((CLASSARCHTYPE)this.ENUM_VALUES[secondaryArchTypeOffset])}] final weight [{archTypeWeights[nextToMaxWeightIndex]}]");

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
				Log.debug($"Race [{r}]: does not match a unique race. Using Human");
				ru = "HUMAN";
			}
			return Meta.StrToRaceMask[ru];
		}

		public static ALIGNMENT strToAlignment(string a)
		{
			string au = a.ToUpper();
			if (!Meta.StrToAlignmentEnum.ContainsKey(au))
			{
				Log.always($"Error processing Alignment [{a}]: Does not match any known alignment. Using TrueNeutral");
				au = "TRUENEUTRAL";
			}
			return Meta.StrToAlignmentEnum[au];
		}

        public string SharedToString()
        {
			HashSet<String> sharedStashPtr       = STRSET_VALUES[((int)PROPERTY.SharedStash) >> 10];
			HashSet<String> activeQuestsPtr      = STRSET_VALUES[((int)PROPERTY.ActiveQuests) >> 10];
			HashSet<String> completedQuestsPtr   = STRSET_VALUES[((int)PROPERTY.CompletedQuests) >> 10];
			HashSet<String> failedQuestsPtr      = STRSET_VALUES[((int)PROPERTY.FailedQuests) >> 10];
			HashSet<String> knownQuestsPtr       = STRSET_VALUES[((int)PROPERTY.KnownQuests) >> 10];

			StringWriter sw = new StringWriter();
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("SharedStash (Current Snapshot)");
			sw.WriteLine("------------------------------------------------------------------------------");
			string value;
			foreach (string item in sharedStashPtr)
			{
				sw.WriteLine($"[{item}]    {{\"prop\":\"SharedStash\", \"cond\":\"any\", \"value\":\"{item}\"}}");
			}
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("ActiveQuests (Current Snapshot)");
			sw.WriteLine("------------------------------------------------------------------------------");
			foreach (string quest in activeQuestsPtr)
			{
				sw.WriteLine($"[{quest}]    {{\"prop\":\"ActiveQuests\", \"cond\":\"any\", \"value\":\"{quest}\"}}");
				sw.WriteLine($"          or {{\"prop\":\"KnownQuests\",  \"cond\":\"any\", \"value\":\"{quest}\"}}");
			}
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("CompletedQuests (Current Snapshot)");
			sw.WriteLine("------------------------------------------------------------------------------");
			foreach (string quest in completedQuestsPtr)
			{
				sw.WriteLine($"[{quest}]    {{\"prop\":\"CompletedQuests\", \"cond\":\"any\", \"value\":\"{quest}\"}}");
				sw.WriteLine($"          or {{\"prop\":\"KnownQuests\",     \"cond\":\"any\", \"value\":\"{quest}\"}}");
			}
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("FailedQuests (Current Snapshot)");
			sw.WriteLine("------------------------------------------------------------------------------");
			foreach (string quest in failedQuestsPtr)
			{
				sw.WriteLine($"[{quest}]    {{\"prop\":\"FailedQuests\", \"cond\":\"any\", \"value\":\"{quest}\"}}");
				sw.WriteLine($"          or {{\"prop\":\"KnownQuests\",  \"cond\":\"any\", \"value\":\"{quest}\"}}");
			}
			return sw.ToString();
        }

		public void destroy()
        {
			// Clear all the sets and dictionaries
			STRSET_VALUES[((int)PROPERTY.Facts) >> 10].Clear();
			STRSET_VALUES[((int)PROPERTY.Buffs) >> 10].Clear();
			STRSET_VALUES[((int)PROPERTY.SharedStash) >> 10].Clear();
			STRSET_VALUES[((int)PROPERTY.Inventory) >> 10].Clear();
			STRSET_VALUES[((int)PROPERTY.EquippedArmor) >> 10].Clear();
			STRSET_VALUES[((int)PROPERTY.EquippedWeapons) >> 10].Clear();
			STRSET_VALUES[((int)PROPERTY.EquippedRings) >> 10].Clear();
			STRSET_VALUES[((int)PROPERTY.EquippedNecklaces) >> 10].Clear();
			STRSET_VALUES[((int)PROPERTY.Equipped) >> 10].Clear();
			STRSET_VALUES[((int)PROPERTY.ActiveQuests) >> 10].Clear();
			STRSET_VALUES[((int)PROPERTY.CompletedQuests) >> 10].Clear();
			STRSET_VALUES[((int)PROPERTY.FailedQuests) >> 10].Clear();
			STRSET_VALUES[((int)PROPERTY.KnownQuests) >> 10].Clear();
			STRSET_VALUES[((int)PROPERTY.Dialog) >> 10].Clear();
			STRSET_VALUES[((int)PROPERTY.Area) >> 10].Clear();
			AllItems.Clear();
        }

		public override string ToString()
		{

			// The compiler should optimize these out and replace them with constants...

			int civilityOffset      = ((int)PROPERTY.Civility)  >> 6;
			int moralityOffset      = ((int)PROPERTY.Morality)  >> 6;
			int alignmentOffset     = ((int)PROPERTY.Alignment) >> 6;
			int acuityOffset        = ((int)PROPERTY.Acuity)    >> 6;
			int raceOffset          = ((int)PROPERTY.Race)      >> 6;
			int sizeOffset          = ((int)PROPERTY.Size)      >> 6;
			int sizeOffsetBase      = ((int)PROPERTY.Size_Base) >> 6;
			// int genderOffset        = ((int)PROPERTY.Gender)    >> 6;
			// int healthOffset        = ((int)PROPERTY.Health)    >> 6;
			int classPrimCatOffset  = ((int)PROPERTY.PrimaryClassCategory)   >> 6; // [64]: PrimaryClassCategory
			int classSecCatOffset   = ((int)PROPERTY.SecondaryClassCategory) >> 6; // [65]: SecondaryClassCategory
			int classPrimArchOffset = ((int)PROPERTY.PrimaryClassArchType)   >> 6; // [66]: PrimaryClassArchType
			int classSecArchOffset  = ((int)PROPERTY.SecondaryClassArchType) >> 6; // [67]: SecondaryClassArchType
			// int classMythOffset     = ((int)PROPERTY.MythicClass)            >> 6; // [69]: MythicClass

			HashSet<String> areaPtr              = STRSET_VALUES[((int)PROPERTY.Area) >> 10];
			HashSet<String> factsPtr             = STRSET_VALUES[((int)PROPERTY.Facts) >> 10];
			HashSet<String> buffsPtr             = STRSET_VALUES[((int)PROPERTY.Buffs) >> 10];
			HashSet<String> inventoryPtr         = STRSET_VALUES[((int)PROPERTY.Inventory) >> 10];
			HashSet<String> equippedArmorPtr     = STRSET_VALUES[((int)PROPERTY.EquippedArmor) >> 10];
			HashSet<String> equippedWeaponsPtr   = STRSET_VALUES[((int)PROPERTY.EquippedWeapons) >> 10];
			HashSet<String> equippedRingsPtr     = STRSET_VALUES[((int)PROPERTY.EquippedRings) >> 10];
			HashSet<String> equippedNecklacesPtr = STRSET_VALUES[((int)PROPERTY.EquippedNecklaces) >> 10];
			HashSet<String> equippedPtr          = STRSET_VALUES[((int)PROPERTY.Equipped) >> 10];


			StringWriter sw = new StringWriter();
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Attributes");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine($"Str [{USHORT_VALUES[(int)PROPERTY.Str]}]    {{\"prop\":\"Str\", \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Str]}\"}}");
			sw.WriteLine($"Dex [{USHORT_VALUES[(int)PROPERTY.Dex]}]    {{\"prop\":\"Dex\", \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Dex]}\"}}");
			sw.WriteLine($"Con [{USHORT_VALUES[(int)PROPERTY.Con]}]    {{\"prop\":\"Con\", \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Con]}\"}}");
			sw.WriteLine($"Int [{USHORT_VALUES[(int)PROPERTY.Int]}]    {{\"prop\":\"Int\", \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Int]}\"}}");
			sw.WriteLine($"Wis [{USHORT_VALUES[(int)PROPERTY.Wis]}]    {{\"prop\":\"Wis\", \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Wis]}\"}}");
			sw.WriteLine($"Chr [{USHORT_VALUES[(int)PROPERTY.Chr]}]    {{\"prop\":\"Chr\", \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Chr]}\"}}");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine($"Str_Base [{USHORT_VALUES[(int)PROPERTY.Str_Base]}]    {{\"prop\":\"Str_Base\", \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Str_Base]}\"}}");
			sw.WriteLine($"Dex_Base [{USHORT_VALUES[(int)PROPERTY.Dex_Base]}]    {{\"prop\":\"Dex_Base\", \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Dex_Base]}\"}}");
			sw.WriteLine($"Con_Base [{USHORT_VALUES[(int)PROPERTY.Con_Base]}]    {{\"prop\":\"Con_Base\", \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Con_Base]}\"}}");
			sw.WriteLine($"Int_Base [{USHORT_VALUES[(int)PROPERTY.Int_Base]}]    {{\"prop\":\"Int_Base\", \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Int_Base]}\"}}");
			sw.WriteLine($"Wis_Base [{USHORT_VALUES[(int)PROPERTY.Wis_Base]}]    {{\"prop\":\"Wis_Base\", \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Wis_Base]}\"}}");
			sw.WriteLine($"Chr_Base [{USHORT_VALUES[(int)PROPERTY.Chr_Base]}]    {{\"prop\":\"Chr_Base\", \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Chr_Base]}\"}}");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("SavingThrows");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine($"Fort   [{USHORT_VALUES[(int)PROPERTY.Fort       ]}]    {{\"prop\":\"Fort\",        \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Fort       ]}\"}}");
			sw.WriteLine($"Will   [{USHORT_VALUES[(int)PROPERTY.Will       ]}]    {{\"prop\":\"Will\",        \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Will       ]}\"}}");
			sw.WriteLine($"Reflex [{USHORT_VALUES[(int)PROPERTY.Reflex     ]}]    {{\"prop\":\"Reflex\",      \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Reflex     ]}\"}}");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine($"Fort_Base   [{USHORT_VALUES[(int)PROPERTY.Fort_Base  ]}]    {{\"prop\":\"Fort_Base\",   \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Fort_Base  ]}\"}}");
			sw.WriteLine($"Will_Base   [{USHORT_VALUES[(int)PROPERTY.Will_Base  ]}]    {{\"prop\":\"Will_Base\",   \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Will_Base  ]}\"}}");
			sw.WriteLine($"Reflex_Base [{USHORT_VALUES[(int)PROPERTY.Reflex_Base]}]    {{\"prop\":\"Reflex_Base\", \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Reflex_Base]}\"}}");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Skills");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine($"Mobility        [{USHORT_VALUES[(int)PROPERTY.Mobility       ]}]    {{\"prop\":\"Mobility\",        \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Mobility       ]}\"}}");
			sw.WriteLine($"Athletics       [{USHORT_VALUES[(int)PROPERTY.Athletics      ]}]    {{\"prop\":\"Athletics\",       \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Athletics      ]}\"}}");
			sw.WriteLine($"Perception      [{USHORT_VALUES[(int)PROPERTY.Perception     ]}]    {{\"prop\":\"Perception\",      \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Perception     ]}\"}}");
			sw.WriteLine($"Thievery        [{USHORT_VALUES[(int)PROPERTY.Thievery       ]}]    {{\"prop\":\"Thievery\",        \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Thievery       ]}\"}}");
			sw.WriteLine($"LoreNature      [{USHORT_VALUES[(int)PROPERTY.LoreNature     ]}]    {{\"prop\":\"LoreNature\",      \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.LoreNature     ]}\"}}");
			sw.WriteLine($"KnowledgeArcana [{USHORT_VALUES[(int)PROPERTY.KnowledgeArcana]}]    {{\"prop\":\"KnowledgeArcana\", \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.KnowledgeArcana]}\"}}");
			sw.WriteLine($"Persuasion      [{USHORT_VALUES[(int)PROPERTY.Persuasion     ]}]    {{\"prop\":\"Persuasion\",      \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Persuasion     ]}\"}}");
			sw.WriteLine($"Stealth         [{USHORT_VALUES[(int)PROPERTY.Stealth        ]}]    {{\"prop\":\"Stealth\",         \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Stealth        ]}\"}}");
			sw.WriteLine($"UseMagicDevice  [{USHORT_VALUES[(int)PROPERTY.UseMagicDevice ]}]    {{\"prop\":\"UseMagicDevice\",  \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.UseMagicDevice ]}\"}}");
			sw.WriteLine($"LoreReligion    [{USHORT_VALUES[(int)PROPERTY.LoreReligion   ]}]    {{\"prop\":\"LoreReligion\",    \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.LoreReligion   ]}\"}}");
			sw.WriteLine($"KnowledgeWorld  [{USHORT_VALUES[(int)PROPERTY.KnowledgeWorld ]}]    {{\"prop\":\"KnowledgeWorld\",  \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.KnowledgeWorld ]}\"}}");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine($"Mobility_Base        [{USHORT_VALUES[(int)PROPERTY.Mobility_Base       ]}]    {{\"prop\":\"Mobility_Base\"         \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Mobility_Base       ]}\"}}");
			sw.WriteLine($"Athletics_Base       [{USHORT_VALUES[(int)PROPERTY.Athletics_Base      ]}]    {{\"prop\":\"Athletics_Base\",       \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Athletics_Base      ]}\"}}");
			sw.WriteLine($"Perception_Base      [{USHORT_VALUES[(int)PROPERTY.Perception_Base     ]}]    {{\"prop\":\"Perception_Base\",      \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Perception_Base     ]}\"}}");
			sw.WriteLine($"Thievery_Base        [{USHORT_VALUES[(int)PROPERTY.Thievery_Base       ]}]    {{\"prop\":\"Thievery_Base\",        \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Thievery_Base       ]}\"}}");
			sw.WriteLine($"KnowledgeArcana_Base [{USHORT_VALUES[(int)PROPERTY.LoreNature_Base     ]}]    {{\"prop\":\"KnowledgeArcana_Base\", \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.LoreNature_Base     ]}\"}}");
			sw.WriteLine($"KnowledgeArcana_Base [{USHORT_VALUES[(int)PROPERTY.KnowledgeArcana_Base]}]    {{\"prop\":\"KnowledgeArcana_Base\", \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.KnowledgeArcana_Base]}\"}}");
			sw.WriteLine($"Persuasion_Base      [{USHORT_VALUES[(int)PROPERTY.Persuasion_Base     ]}]    {{\"prop\":\"Persuasion_Base\",      \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Persuasion_Base     ]}\"}}");
			sw.WriteLine($"Stealth_Base         [{USHORT_VALUES[(int)PROPERTY.Stealth_Base        ]}]    {{\"prop\":\"Stealth_Base\",         \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Stealth_Base        ]}\"}}");
			sw.WriteLine($"UseMagicDevice_Base  [{USHORT_VALUES[(int)PROPERTY.UseMagicDevice_Base ]}]    {{\"prop\":\"UseMagicDevice_Base\",  \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.UseMagicDevice_Base ]}\"}}");
			sw.WriteLine($"LoreReligion_Base    [{USHORT_VALUES[(int)PROPERTY.LoreReligion_Base   ]}]    {{\"prop\":\"LoreReligion_Base\",    \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.LoreReligion_Base   ]}\"}}");
			sw.WriteLine($"KnowledgeWorld_Base  [{USHORT_VALUES[(int)PROPERTY.KnowledgeWorld_Base ]}]    {{\"prop\":\"KnowledgeWorld_Base\",  \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.KnowledgeWorld_Base ]}\"}}");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Misc Props (int based values used with cond \"eq\",\"gt\",\"lt\",\"neq\",etc...");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine($"XP         [{UINT_VALUES[(((int)PROPERTY.XP)>> 14) ]}]    {{\"prop\":\"XP\",         \"cond\":\"gte\", \"value\":\"{UINT_VALUES[(((int)PROPERTY.XP)>> 14)  ]}\"}}");
			sw.WriteLine($"HP         [{USHORT_VALUES[(int)PROPERTY.HP        ]}]    {{\"prop\":\"HP\",         \"cond\":\"eq\",  \"value\":\"{USHORT_VALUES[(int)PROPERTY.HP         ]}\"}}");
			sw.WriteLine($"HP_Percent [{USHORT_VALUES[(int)PROPERTY.HP_Percent]}]    {{\"prop\":\"HP_Percent\", \"cond\":\"lte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.HP_Percent ]}\"}}");
			sw.WriteLine($"AC         [{USHORT_VALUES[(int)PROPERTY.AC        ]}]    {{\"prop\":\"AC\",         \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.AC         ]}\"}}");
			sw.WriteLine($"Speed      [{USHORT_VALUES[(int)PROPERTY.Speed     ]}]    {{\"prop\":\"Speed\",      \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Speed      ]}\"}}");
			sw.WriteLine($"Initiative [{USHORT_VALUES[(int)PROPERTY.Initiative]}]    {{\"prop\":\"Initiative\", \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Initiative ]}\"}}");
			sw.WriteLine($"Corruption [{USHORT_VALUES[(int)PROPERTY.Corruption]}]    {{\"prop\":\"Corruption\", \"cond\":\"lte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Corruption ]}\"}}");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine($"HP_Max        [{USHORT_VALUES[(int)PROPERTY.HP_Max       ]}]    {{\"prop\":\"HP_Max\",        \"cond\":\"eq\",  \"value\":\"{USHORT_VALUES[(int)PROPERTY.HP_Max        ]}\"}}");
			sw.WriteLine($"HP_Mod        [{USHORT_VALUES[(int)PROPERTY.HP_Mod       ]}]    {{\"prop\":\"HP_Mod\",        \"cond\":\"eq\",  \"value\":\"{USHORT_VALUES[(int)PROPERTY.HP_Mod        ]}\"}}");
			sw.WriteLine($"AC_TOUCH      [{USHORT_VALUES[(int)PROPERTY.AC_TOUCH     ]}]    {{\"prop\":\"AC_TOUCH\",      \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.AC_TOUCH      ]}\"}}");
			sw.WriteLine($"AC_FLATFOOTED [{USHORT_VALUES[(int)PROPERTY.AC_FLATFOOTED]}]    {{\"prop\":\"AC_FLATFOOTED\", \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.AC_FLATFOOTED ]}\"}}");
			sw.WriteLine($"InCombat      [{USHORT_VALUES[(int)PROPERTY.InCombat     ]}]    {{\"prop\":\"InCombat\",      \"cond\":\"eq\",  \"value\":\"{USHORT_VALUES[(int)PROPERTY.InCombat      ]}\"}} (0 = false, 1 = true)");
			sw.WriteLine($"IsNaked       [{USHORT_VALUES[(int)PROPERTY.IsNaked      ]}]    {{\"prop\":\"IsNaked\",       \"cond\":\"eq\",  \"value\":\"{USHORT_VALUES[(int)PROPERTY.IsNaked      ]}\"}} (0 = false, 1 = true)");
			sw.WriteLine($"UsingDefaultEquipment [{USHORT_VALUES[(int)PROPERTY.UsingDefaultEquipment]}] {{\"prop\":\"UsingDefaultEquipment\", \"cond\":\"eq\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.UsingDefaultEquipment]}\"}} (0 = false, 1 = true)");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine($"HP_Base            [{USHORT_VALUES[(int)PROPERTY.HP_Base           ]}]    {{\"prop\":\"HP_Base\",            \"cond\":\"eq\",  \"value\":\"{USHORT_VALUES[(int)PROPERTY.HP_Base           ]}\"}}");
			sw.WriteLine($"AC_Base            [{USHORT_VALUES[(int)PROPERTY.AC_Base           ]}]    {{\"prop\":\"AC_Base\",            \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.AC_Base           ]}\"}}");
			sw.WriteLine($"Speed_Base         [{USHORT_VALUES[(int)PROPERTY.Speed_Base        ]}]    {{\"prop\":\"Speed_Base\",         \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Speed_Base        ]}\"}}");
			sw.WriteLine($"Initiative_Base    [{USHORT_VALUES[(int)PROPERTY.Initiative_Base   ]}]    {{\"prop\":\"Initiative_Base\",    \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Initiative_Base   ]}\"}}");
			sw.WriteLine($"Corruption_Max     [{USHORT_VALUES[(int)PROPERTY.Corruption_Max    ]}]    {{\"prop\":\"Corruption_Max\",     \"cond\":\"lte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Corruption_Max    ]}\"}}");
			sw.WriteLine($"Corruption_Precent [{USHORT_VALUES[(int)PROPERTY.Corruption_Percent]}]    {{\"prop\":\"Corruption_Precent\", \"cond\":\"lt\",  \"value\":\"{USHORT_VALUES[(int)PROPERTY.Corruption_Percent]}\"}}");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Misc Props (Typically comma separated values used with cond \"any\")");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine($"Civility  [{(CIVILITY)ENUM_VALUES[civilityOffset  ]}]    {{\"prop\":\"Civility\",  \"cond\":\"any\", \"value\":\"{(CIVILITY)ENUM_VALUES[civilityOffset  ]}\"}}");
			sw.WriteLine($"Morality  [{(MORALITY)ENUM_VALUES[moralityOffset  ]}]    {{\"prop\":\"Morality\",  \"cond\":\"any\", \"value\":\"{(MORALITY)ENUM_VALUES[moralityOffset  ]}\"}}");
			sw.WriteLine($"Alignment [{(ALIGNMENT)ENUM_VALUES[alignmentOffset]}]    {{\"prop\":\"Alignment\", \"cond\":\"any\", \"value\":\"{(ALIGNMENT)ENUM_VALUES[alignmentOffset]}\"}}");
			sw.WriteLine($"Acuity    [{(ACUITY)ENUM_VALUES[acuityOffset      ]}]    {{\"prop\":\"Acuity\",    \"cond\":\"any\", \"value\":\"{(ACUITY)ENUM_VALUES[acuityOffset      ]}\"}}");
			sw.WriteLine($"Race      [{(RACEMASK)ENUM_VALUES[raceOffset      ]}]    {{\"prop\":\"Race\",      \"cond\":\"any\", \"value\":\"{(RACEMASK)ENUM_VALUES[raceOffset      ]}\"}}");
//			sw.WriteLine($"Gender    [{(GENDER)ENUM_VALUES[genderOffset      ]}]    {{\"prop\":\"Gender\",    \"cond\":\"any\", \"value\":\"{(GENDER)ENUM_VALUES[genderOffset      ]}\"}}");
//			sw.WriteLine($"Health    [{(HEALTHENUM)ENUM_VALUES[healthOffset  ]}]    {{\"prop\":\"Health\",    \"cond\":\"any\", \"value\":\"{(HEALTHENUM)ENUM_VALUES[healthOffset  ]}\"}}");
			sw.WriteLine($"Size      [{(NPCSIZE)ENUM_VALUES[sizeOffset       ]}]    {{\"prop\":\"Size\",      \"cond\":\"any\", \"value\":\"{(NPCSIZE)ENUM_VALUES[sizeOffset       ]}\"}}");
			sw.WriteLine($"Size_Base [{(NPCSIZE)ENUM_VALUES[sizeOffsetBase   ]}]    {{\"prop\":\"Size_Base\", \"cond\":\"any\", \"value\":\"{(NPCSIZE)ENUM_VALUES[sizeOffsetBase   ]}\"}}");
			foreach (string area in areaPtr)
			{
				sw.WriteLine($"Area      [{area}]    {{\"prop\":\"Area\", \"cond\":\"any\", \"value\":\"{area}\"}}");
			}
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Class and Level Props");
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine($"PrimaryClassCategory        [{(CLASSCATEGORY)ENUM_VALUES[classPrimCatOffset          ]}]    {{\"prop\":\"PrimaryClassCategory\",    \"cond\":\"any\", \"value\":\"{(CLASSCATEGORY)ENUM_VALUES[classPrimCatOffset ]}\"}}");
			sw.WriteLine($"SecondaryClassCategory      [{(CLASSCATEGORY)ENUM_VALUES[classSecCatOffset           ]}]    {{\"prop\":\"SecondaryClassCategory\",  \"cond\":\"any\", \"value\":\"{(CLASSCATEGORY)ENUM_VALUES[classSecCatOffset  ]}\"}}");
			sw.WriteLine($"PrimaryClassArchType        [{(CLASSARCHTYPE)ENUM_VALUES[classPrimArchOffset         ]}]    {{\"prop\":\"PrimaryClassArchType\",    \"cond\":\"any\", \"value\":\"{(CLASSARCHTYPE)ENUM_VALUES[classPrimArchOffset]}\"}}");
			sw.WriteLine($"SecondaryClassArchType      [{(CLASSARCHTYPE)ENUM_VALUES[classSecArchOffset          ]}]    {{\"prop\":\"SecondaryClassArchType\",  \"cond\":\"any\", \"value\":\"{(CLASSARCHTYPE)ENUM_VALUES[classSecArchOffset ]}\"}}");
			sw.WriteLine($"Level      [{USHORT_VALUES[(int)PROPERTY.Level     ]}]    {{\"prop\":\"Level\",      \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Level      ]}\"}}");
			sw.WriteLine($"Level_Base         [{USHORT_VALUES[(int)PROPERTY.Level_Base        ]}]    {{\"prop\":\"Level_Base\",         \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.Level_Base        ]}\"}}");
//			sw.WriteLine($"MythicClass                 [{(CLASSMYTHIC)ENUM_VALUES[classMythOffset               ]}]    {{\"prop\":\"MythicClass\",             \"cond\":\"any\", \"value\":\"{(CLASSMYTHIC)ENUM_VALUES[classMythOffset      ]}\"}}");
//			sw.WriteLine($"PrimaryClassArchTypeLevel   [{USHORT_VALUES[(int)PROPERTY.PrimaryClassArchTypeLevel  ]}]    {{\"prop\":\"PrimaryClassArchTypeLevel\",   \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.PrimaryClassArchTypeLevel  ]}\"}}");
//			sw.WriteLine($"SecondaryClassArchTypeLevel [{USHORT_VALUES[(int)PROPERTY.SecondaryClassArchTypeLevel]}]    {{\"prop\":\"SecondaryClassArchTypeLevel\", \"cond\":\"gte\", \"value\":\"{USHORT_VALUES[(int)PROPERTY.SecondaryClassArchTypeLevel]}\"}}");
			// AllClassArchTypes? TODO: Add computation of "AllClassArchTypes"
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Facts Snapshot");
			sw.WriteLine("------------------------------------------------------------------------------");
			foreach (string fact in factsPtr)
			{
				sw.WriteLine($"[{fact}]    {{\"prop\":\"Facts\", \"cond\":\"any\", \"value\":\"{fact}\"}}");
			}
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Buffs Snapshot");
			sw.WriteLine("------------------------------------------------------------------------------");
			foreach (string buff in buffsPtr)
			{
				sw.WriteLine($"[{buff}]    {{\"prop\":\"Buffs\", \"cond\":\"any\", \"value\":\"{buff}\"}}");
			}

			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Inventory Snapshot");
			sw.WriteLine("------------------------------------------------------------------------------");
			string value;
			foreach (string item in inventoryPtr)
			{
				sw.WriteLine($"[{item}] ({(AllItems.TryGetValue(item, out value) ? value : "???")})   {{\"prop\":\"Inventory\", \"cond\":\"all\", \"value\":\"{item}\"}}");
			}
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("EquippedArmor (Use {{\"prop\":\"EquippedArmor\",\"cond\":\"any\",\"value\":\"\"}} to test for no armor)");
			sw.WriteLine("------------------------------------------------------------------------------");
			foreach (string item in equippedArmorPtr)
			{
				sw.WriteLine($"[{item}] ({(AllItems.TryGetValue(item, out value) ? value : "???")})   {{\"prop\":\"EquippedArmor\", \"cond\":\"any\", \"value\":\"{item}\"}}");
			}
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("EquippedWeapons Snapshot");
			sw.WriteLine("------------------------------------------------------------------------------");
			foreach (string item in equippedWeaponsPtr)
			{
				sw.WriteLine($"[{item}] ({(AllItems.TryGetValue(item, out value) ? value : "???")})   {{\"prop\":\"EquippedWeapons\", \"cond\":\"any\", \"value\":\"{item}\"}}");
			}
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("EquippedRings Snapshot");
			sw.WriteLine("------------------------------------------------------------------------------");
			foreach (string item in equippedRingsPtr)
			{
				sw.WriteLine($"[{item}] ({(AllItems.TryGetValue(item, out value) ? value : "???")})   {{\"prop\":\"EquippedRings\", \"cond\":\"any\", \"value\":\"{item}\"}}");
			}
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("EquippedNecklaces Snapshot");
			sw.WriteLine("------------------------------------------------------------------------------");
			foreach (string item in equippedNecklacesPtr)
			{
				sw.WriteLine($"[{item}] ({(AllItems.TryGetValue(item, out value) ? value : "???")})   {{\"prop\":\"EquippedNecklaces\", \"cond\":\"any\", \"value\":\"{item}\"}}");
			}
			sw.WriteLine("------------------------------------------------------------------------------");
			sw.WriteLine("Equipped Snapshot");
			sw.WriteLine("------------------------------------------------------------------------------");
			foreach (string item in equippedPtr)
			{
				sw.WriteLine($"[{item}] ({(AllItems.TryGetValue(item, out value) ? value : "???")})   {{\"prop\":\"Equipped\", \"cond\":\"all\", \"value\":\"{item}\"}}");
			}
            return sw.ToString();
		}
	}
}