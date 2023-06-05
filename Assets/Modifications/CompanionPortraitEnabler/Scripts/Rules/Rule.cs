using System;
using Newtonsoft.Json;

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

    [Serializable]
    public class Rule
    {
		// resourceId
		//
        //   Example: Ember_Portrait. This is the name used as the key for the
        //            PortraitData cache on the primary resource interceptor. 
		//
		//            If we wish to update the interceptor cache in the
        //            Main.cs file, we need this value. 		
        [JsonIgnore]
        public string resourceId { get; set; }  // Example: Ember_Portrait

		// portraitId
		//
		//   When rule is in regards to Portrait... 
		//
        //   Example: This is the value that the customPortraitManager uses. Basically
		//            it uses the folder name under "Portaits" that contains the 3
        //            custom images as the id for a set of custom portraits. 
		//
		//            If we wish to instantiate new custom portraits, we need
        //            these values to work with the existing customPortraitManager.
        [JsonIgnore]
        public string portraitId { get; set; } // Example: npc_Portraits/Ember_Portrait/portrait_01


		// fileName
		//
		//   When rule is in regards to Body... 
        //
		//   body.json that the rule corresponds to.
		//
        [JsonIgnore]
        public string fileName { get; set; }   // Example: npc_Portraits/Ember_Portrait/body_01/body.json

		[JsonIgnore]
		public int bodyList { get; set; } // Indicates which internal list is currently in use by the body.josn.

        // ruleEvaluator
        //
        //   A convenience pointer to the RuleEvalutor
		//
        [JsonIgnore]
		public RuleEvaluator ruleEvaluator {  get; set; }

        public string comment { get; set; }
        public Condition[] conditions { get; set; }

        [Serializable]
        public class Condition
        {
            public PropValues prop { get; set; }
            public CondValues cond { get; set; }
            public string value { get; set; }
            public LogicValues next { set; get; }
        }

        public enum LogicValues
        {
            and, // First listed is default
            or
        }

		public enum CondValues
        {
			eq,   // equals
			gt,   // greaterThan
			lt,   // lessThan
			gte,  // greaterThanOrEqualTo
			lte,  // lessThanOrEqualTo
			neq,  // notEqualTo
			any,  // MatchesAny
			all,  // MatchesAll
			nany, // DoesNotMatchAny
			nall  // DoesNotMatchAll
		}

		public enum PropValues // and aliases
        {
			None,
			// HEALTH
			HP_Base,
			    HPBASE,
            HP_Max,
			    HPMAX,
            HP_Mod,
				HPMOD,
		    HP,
				HITPOINTS,
				HIT_POINTS,
		    HP_Percent,
                HPPERCENT,
                HITPOINTPERCENT,
                HIT_POINT_PERCENT,
            // Attributes
            Str,
                STRENGTH,
			Dex,
				DEXTERITY,
			Con,
				CONSTITUTION,
			Int,
				INTELLIGENCE,
			Wis,
				WISDOM,
			Chr,
				CHARISMA,
			Str_Base,
				STRBASE,
				STRENGTH_BASE,
				STRENGTHBASE,
			Dex_Base,
				DEXBASE,
				DEXTERITY_BASE,
				DEXTERITYBASE,
			Con_Base,
				CONBASE,
				CONSTITUTION_BASE,
				CONSTITUTIONBASE,
			Int_Base,
				INTBASE,
				INTELLIGENCE_BASE,
				INTELLIGENCEBASE,
			Wis_Base,
				WISBASE,
				WISDOM_BASE,
				WISDOMBASE,
			Chr_Base,
				CHRBASE,
				CHARISMA_BASE,
				CHARISMABASE,
			// Saves
		    Fort,
				FORTITUDE,
			Will,
				WILLPOWER,
			Reflex,
			Fort_Base,
				FORTBASE,
				FORTITUDE_BASE,
				FORTITUDEBASE,
			Will_Base,
				WILLBASE,
				WILLPOWER_BASE,
				WILLPOWERBASE,
			Reflex_Base,
				REFLEXBASE,
			// Skills
			Mobility,
			Athletics,
			Perception,
			Thievery,
			LoreNature,
			LORE_NATURE,
			KnowledgeArcana,
				KNOWLEDGE_ARCANA,
				KNOWLEDGE_ARCANE,
				KNOWLEDGEARCANE,
			Persuasion,
			Stealth,
		    UseMagicDevice,
				USE_MAGIC_DEVICE,
				USE_DEVICE,
				USEDEVICE,
			LoreReligion,
				LORE_RELIGION,
			KnowledgeWorld,
				KNOWLEDGE_WORLD,
			Mobility_Base,
				MOBILITYBASE,
			Athletics_Base,
				ATHLETICSBASE,
			Perception_Base,
				PERCEPTIONBASE,
			Thievery_Base,
				THIEVERYBASE,
			LoreNature_Base,
				LORE_NATURE_BASE,
				LORENATUREBASE,
			KnowledgeArcana_Base,
				KNOWLEDGE_ARCANA_BASE,
				KNOWLEDGE_ARCANE_BASE,
				KNOWLEDGEARCANE_BASE,
				KNOWLEDGEARCANABASE,
				KNOWLEDGEARCANEBASE,
			Persuasion_Base,
				PERSUASIONBASE,
			Stealth_Base,
				STEALTHBASE,
			UseMagicDevice_Base,
				USE_MAGIC_DEVICE_BASE,
				USEMAGICDEVICEBASE,
			LoreReligion_Base,
				LORE_RELIGION_BASE,
				LORERELIGIONBASE,
			KnowledgeWorld_Base,
				KNOWLEDGE_WORLD_BASE,
				KNOWLEDGEWORLDBASE,
			// Misc
			AC,
				ARMOR_CLASS,
				ARMORCLASS,
			AC_Base,
				ACBASE,
				ARMOR_CLASS_BASE,
				ARMORCLASS_BASE,
				ARMORCLASSBASE,
			AC_TOUCH,
				ACTOUCH,
			ARMOR_CLASS_TOUCH,
				ARMORCLASS_TOUCH,
				ARMORCLASSTOUCH,
			AC_FLATFOOTED,
				AC_FLAT_FOOTED,
				ACFLATFOOTED,
				ARMOR_CLASS_FLAT_FOOTED,
				ARMOR_CLASS_FLATFOOTED,
				ARMORCLASS_FLATFOOTED,
				ARMORCLASSFLATFOOTED,
			Initiative,
			Initiative_Base,
				INITIATIVEBASE,
			Speed,
			Speed_Base,
				SPEEDBASE,
			Corruption,		
			Corruption_Max,
				CORRUPTIONMAX,
			Corruption_Percent,
				CORRUPTIONPERCENT,
			// Level/Class
			Level_Base,
				LEVELBASE,
			Level,
			InCombat,
				IN_COMBAT,
				INCOMBAT,
			IsNaked,
			    IS_NAKED,
				ISNAKED,
			UsingDefaultEquipment,
			    USING_DEFAULT_EQUIPMENT,
				USINGDEFAULTEQUIPMENT,
			// Enums
			PrimaryClassCategory,
				PRIMARY_CLASS_CATEGORY,
				PRIMARYCLASS_CATEGORY,
				PRIMARY_CATEGORY,
				PRIMARYCATEGORY,
			SecondaryClassCategory,
				SECONDARY_CLASS_CATEGORY,
				SECONDARYCLASS_CATEGORY,
				SECONDARY_CATEGORY,
				SECONDARYCATEGORY,
			PrimaryClassArchType,
				PRIMARY_CLASS_ARCH_TYPE,
				PRIMARY_CLASS_ARCHTYPE,
				PRIMARYCLASS_ARCH_TYPE,
				PRIMARYCLASS_ARCHTYPE,
				PRIMARY_ARCH_TYPE,
				PRIMARY_ARCHTYPE,
				PRIMARYARCHTYPE,
			SecondaryClassArchType,
				SECONDARY_CLASS_ARCH_TYPE,
				SECONDARY_CLASS_ARCHTYPE,
				SECONDARYCLASS_ARCH_TYPE,
				SECONDARYCLASS_ARCHTYPE,
				SECONDARY_ARCH_TYPE,
				SECONDARY_ARCHTYPE,
				SECONDARYARCHTYPE,
			Race,
			Civility,
			Morality,
			Alignment,
			Acuity,
			Size,
			Size_Base,
				SIZEBASE,
            // Open Sets (4 HashSet<String>s)
			Facts,
			Buffs,
			SharedStash,
				SHARED_STASH,
				STASH,
			Inventory,
            EquippedArmor, // test for "" to test if no armor is on...
			    EQUIPTARMOR,
			    EQUIPPED_ARMOR,
			    EQUIPT_ARMOR,
            EquippedWeapons,
                EQUIPTWEAPONS,
				EQUIPPED_WEAPONS,
				EQUIPT_WEAPONS,
            EquippedRings,
			    EQUIPTRINGS,
				EQUIPPED_RINGS,
				EQUIPT_RINGS,
            EquippedNecklaces,
			    EQUIPTNECKLACES,
				EQUIPPED_NECKLACES,
				EQUIPT_NECKLACE,
			Equipped,
				EQUIPT,
			ActiveQuests,
			    ACTIVEQUEST,
			    ACTIVE_QUEST,
			    ACTIVE_QUESTS,
			CompletedQuests,
			    COMPLETED_QUESTS,
			FailedQuests,
			    FAILED_QUESTS,
			KnownQuests,
			    KNOWN_QUESTS,
			Dialog,
			Area,
			XP,
				EXPERIENCE,
				EXPERIENCE_POINTS,
				EXPERIENCEPOINTS
        }
    }
}