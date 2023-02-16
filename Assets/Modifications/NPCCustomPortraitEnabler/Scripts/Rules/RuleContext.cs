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

// using OwlcatModification.Modifications.NPCCustomPortraitEnabler.Utility;

namespace OwlcatModification.Modifications.NPCCustomPortraitEnabler.Rules
{


	/**
	 * RuleContext identifies the properties available for inspection and evaluation from 
	 * within Rule definitions.
	 **/
	public class RuleContext 
	{
		//#################################################################################
		//                     STATICS, CONSTANTS and ENUMS....
		//#################################################################################

		public enum PROPERTY : ushort
        {
			None                        = 0,
			// HEALTH  (5 items)
		    HP_Base                     = 1,
		    HP_Max                      = 2,
		    HP_Mod                      = 3,
		    HP                          = 4,
		    HP_Percent                  = 5,
			// Attributes (12 items)
			Str                         = 6,
			Dex                         = 7,
			Con                         = 8,
			Int                         = 9,
			Wis                         = 10,
			Chr                         = 11,
			Str_Base                    = 12,
			Dex_Base                    = 13,
			Con_Base                    = 14,
			Int_Base                    = 15,
			Wis_Base                    = 16,
			Chr_Base                    = 17,
			// Saves (6 items)
			Fort                        = 18, 
			Will                        = 19,
			Reflex                      = 20,
			Fort_Base                   = 21,
			Will_Base                   = 22,
			Reflex_Base                 = 23,
			// Skills (22 items)
			Mobility                    = 24,
			Athletics                   = 25,
			Perception                  = 26,
			Thievery                    = 27,
			LoreNature                  = 28,
			KnowledgeArcana             = 29,
			Persuasion                  = 30,
			Stealth                     = 31,
			UseMagicDevice              = 32,
			LoreReligion                = 33,
			KnowledgeWorld              = 34,
			Mobility_Base               = 35,
			Athletics_Base              = 36,
			Perception_Base             = 37,
			Thievery_Base               = 38,
			LoreNature_Base             = 39,
			KnowledgeArcana_Base        = 40,
			Persuasion_Base             = 41,
			Stealth_Base                = 42,
			UseMagicDevice_Base         = 43,
			LoreReligion_Base           = 44,
			KnowledgeWorld_Base         = 45,
			// Misc (11 items)
			AC                          = 46,
			AC_Base                     = 47,
			AC_TOUCH                    = 48,
			AC_FLATFOOTED               = 49,
			Initiative                  = 50,
			Initiative_Base             = 51,
			Speed                       = 52,
			Speed_Base                  = 53,
			Corruption                  = 54,
			Corruption_Max              = 55,
			Corruption_Percent          = 56,
			// Level/Class (5 items)
			PrimaryClassArchTypeLevel   = 57,
			SecondaryClassArchTypeLevel = 58,
			MythicClassLevel            = 59,
			Level_Base                  = 60,
			Level                       = 61,

			// SKIP to 64 as 0-63 can be easily masked out with a shift >> 6 (same as dividing by 2, 6 times).

			// Closed Sets (13 Enums)
			PrimaryClassCategory        = 1  << 6, // (1 * 64)  = 64  | 0b000 000 0001 000000
			SecondaryClassCategory      = 2  << 6, // (2 * 64)  = 128 | 0b000 000 0010 000000
			PrimaryClassArchType        = 3  << 6, // (3 * 64)  = 192 | 0b000 000 0011 000000
			SecondaryClassArchType      = 4  << 6, // (4 * 64)  = 256 | 0b000 000 0100 000000
			MythicClass                 = 5  << 6, // (5 * 64)  = 320 | 0b000 000 0101 000000
			AllClassArchTypes           = 6  << 6, // (6 * 64)  = 384 | 0b000 000 0110 000000
			Health                      = 7  << 6, // (7 * 64)  = 448 | 0b000 000 0111 000000
			Race                        = 8  << 6, // (8 * 64)  = 512 | 0b000 000 1000 000000
			Gender                      = 9  << 6, // (9 * 64)  = 576 | 0b000 000 1001 000000
			Civility                    = 10 << 6, // (10 * 64) = 640 | 0b000 000 1010 000000
			Morality                    = 11 << 6, // (11 * 64) = 704 | 0b000 000 1011 000000
			Alignment                   = 12 << 6, // (12 * 64) = 768 | 0b000 000 1100 000000
			Acuity                      = 13 << 6, // (13 * 64) = 832 | 0b000 000 1101 000000
			Size                        = 14 << 6, // (14 * 64) = 896 | 0b000 000 1110 000000
			Size_Base                   = 15 << 6, // (15 * 64) = 960 | 0b000 000 1111 000000
            // Open Sets (4 HashSet<String>s)
			Facts                       = 1 << 10, // (1*1024) = 1024 | 0b000 001 0000 000000
			Buffs                       = 2 << 10, // (2*1024) = 2048 | 0b000 010 0000 000000
			SharedStash                 = 3 << 10, // (3*1024) = 3072 | 0b000 011 0000 000000
			Inventory                   = 4 << 10, // (4*1024) = 4096 | 0b000 100 0000 000000
			Equipped                    = 5 << 10, // (5*1024) = 5120 | 0b000 101 0000 000000
			// Integers (1 item)
			XP                          = 1 << 13, // (1*8192) = 8192 | 0b001 000 0000 000000
        };

		public enum PROPERTYTYPE : ushort
		{
			// Note : All but container can be represented using ints...
			NONE          = 0,
			USHORT        = 1, 
			CLASSARCHTYPE = 2, 
            CLASSCATEGORY = 3,
            CLASSMYTHIC   = 4,
			HEALTHENUM    = 5, // cast to HEALTHENUM : ushort
			RACEMASK      = 6, // cast to RACE : ushort
			GENDER        = 7, // cast to GENDER : ushort
			CIVILITY      = 8, // cast to CIVILITY : ushort
			MORALITY      = 9, // cast to MORALITY : ushort
			ALIGNMENT     = 10, // cast to ALIGNMENT : ushort
			ACUITY        = 11, // cast to ACUITY : ushort
			NPCSIZE       = 12, // cast to NPCSIZE   Size

			// The first 10 above can be retrieved using a large ushort[] that maintains the values. 

			UINT          = 13,  // cast to uint
			STRSET        = 14   // cast value to HashSet<string>
		};

		public static PROPERTYTYPE[] PROPERTYTYPES  = new RuleContext.PROPERTYTYPE[]
		{
			// HEALTH  (6 items)
			PROPERTYTYPE.USHORT, // [0]: None (Unused)
			PROPERTYTYPE.USHORT, // [1]: HP_Base
			PROPERTYTYPE.USHORT, // [2]: HP_Max
			PROPERTYTYPE.USHORT, // [3]: HP_Mod 
			PROPERTYTYPE.USHORT, // [4]: HP 
			PROPERTYTYPE.USHORT, // [5]: HP_Percent : int
			// Attributes (12 items)
			PROPERTYTYPE.USHORT, // [6]: Str/Strength
			PROPERTYTYPE.USHORT, // [7]: Dex/Dexterity
			PROPERTYTYPE.USHORT, // [8]: Con/Constitution
			PROPERTYTYPE.USHORT, // [9]: Int/Intelligence
			PROPERTYTYPE.USHORT, // [10]: Wis/Wisdom
			PROPERTYTYPE.USHORT, // [11]: Chr/Charisma
			PROPERTYTYPE.USHORT, // [12]: Str_Base
			PROPERTYTYPE.USHORT, // [13]: Dex_Base
			PROPERTYTYPE.USHORT, // [14]: Con_Base
			PROPERTYTYPE.USHORT, // [15]: Int_Base
			PROPERTYTYPE.USHORT, // [16]: Wis_Base
			PROPERTYTYPE.USHORT, // [17]: Chr_Base
			// Saves (6 items)
			PROPERTYTYPE.USHORT, // [18]: Fort
			PROPERTYTYPE.USHORT, // [19]: Will
			PROPERTYTYPE.USHORT, // [20]: Reflex
			PROPERTYTYPE.USHORT, // [21]: Fort_Base
			PROPERTYTYPE.USHORT, // [22]: Will_Base
			PROPERTYTYPE.USHORT, // [23]: Reflex_Base
			// Skills (22 items)
			PROPERTYTYPE.USHORT, // [24]: Mobility
			PROPERTYTYPE.USHORT, // [25]: Athletics
			PROPERTYTYPE.USHORT, // [26]: Perception
			PROPERTYTYPE.USHORT, // [27]: Thievery
			PROPERTYTYPE.USHORT, // [28]: LoreNature
			PROPERTYTYPE.USHORT, // [29]: KnowledgeArcana
			PROPERTYTYPE.USHORT, // [30]: Persuasion
			PROPERTYTYPE.USHORT, // [31]: Stealth
			PROPERTYTYPE.USHORT, // [32]: UseMagicDevice
			PROPERTYTYPE.USHORT, // [33]: LoreReligion
			PROPERTYTYPE.USHORT, // [34]: KnowledgeWorld
			PROPERTYTYPE.USHORT, // [35]: Mobility_Base
			PROPERTYTYPE.USHORT, // [36]: Athletics_Base
			PROPERTYTYPE.USHORT, // [37]: Perception_Base
			PROPERTYTYPE.USHORT, // [38]: Thievery_Base
			PROPERTYTYPE.USHORT, // [39]: LoreNature_Base
			PROPERTYTYPE.USHORT, // [40]: KnowledgeArcana_Base
			PROPERTYTYPE.USHORT, // [41]: Persuasion_Base
			PROPERTYTYPE.USHORT, // [42]: Stealth_Base
			PROPERTYTYPE.USHORT, // [43]: UseMagicDevice_Base
			PROPERTYTYPE.USHORT, // [44]: LoreReligion_Base 
			PROPERTYTYPE.USHORT, // [45]: KnowledgeWorld_Base
			// Misc (11 items)
			PROPERTYTYPE.USHORT, // [46]: AC
			PROPERTYTYPE.USHORT, // [47]: AC_Base
			PROPERTYTYPE.USHORT, // [48]: AC_TOUCH
			PROPERTYTYPE.USHORT, // [49]: AC_FLATFOOTED
			PROPERTYTYPE.USHORT, // [50]: Initiative
			PROPERTYTYPE.USHORT, // [51]: Initiative_Base
			PROPERTYTYPE.USHORT, // [52]: Speed
			PROPERTYTYPE.USHORT, // [53]: Speed_Base
			PROPERTYTYPE.USHORT, // [54]: Corruption
			PROPERTYTYPE.USHORT, // [55]: Corruption_Max
			PROPERTYTYPE.USHORT, // [56]: Corruption_Percent
			// Level/XP/Class (6 items)
			PROPERTYTYPE.USHORT, // [57]: PrimaryClassArchTypeLevel
			PROPERTYTYPE.USHORT, // [58]: SecondaryClassArchTypeLevel
			PROPERTYTYPE.USHORT, // [59]: MythicClassLevel
			PROPERTYTYPE.USHORT, // [60]: Level_Base
			PROPERTYTYPE.USHORT, // [61]: Level
			PROPERTYTYPE.USHORT, // [62]: Unused
			PROPERTYTYPE.USHORT, // [63]: Unused
			// Closed Sets (13 Enums)
			PROPERTYTYPE.CLASSCATEGORY, // [64]: PrimaryClassCategory
			PROPERTYTYPE.CLASSCATEGORY, // [65]: SecondaryClassCategory
			PROPERTYTYPE.CLASSARCHTYPE, // [66]: PrimaryClassArchType
			PROPERTYTYPE.CLASSARCHTYPE, // [67]: SecondaryClassArchType
			PROPERTYTYPE.CLASSARCHTYPE, // [68]: AllClassArchTypes
			PROPERTYTYPE.CLASSMYTHIC,   // [69]: MythicClass
			PROPERTYTYPE.HEALTHENUM,    // [70]: Health
			PROPERTYTYPE.RACEMASK,      // [71]: Race
			PROPERTYTYPE.GENDER,        // [72]: Gender
			PROPERTYTYPE.CIVILITY,      // [73]: Civility
			PROPERTYTYPE.MORALITY,      // [74]: Morality
			PROPERTYTYPE.ALIGNMENT,     // [75]: Alignment
			PROPERTYTYPE.ACUITY,        // [76]: Acuity
			PROPERTYTYPE.NPCSIZE,       // [77]: Size
			PROPERTYTYPE.NPCSIZE,       // [78]: Size_Base
			// Open Sets (4 HashSet<String>s)
			PROPERTYTYPE.STRSET, // [79]: Facts
			PROPERTYTYPE.STRSET, // [80]: Buffs
			PROPERTYTYPE.STRSET, // [81]: SharedStash
			PROPERTYTYPE.STRSET, // [82]: Inventory
			PROPERTYTYPE.STRSET, // [83]: Equipped
			PROPERTYTYPE.USHORT, // [84]: Unused
			PROPERTYTYPE.USHORT, // [85]: Unused
			// Integers (1 item)
			PROPERTYTYPE.UINT,   // [86]: XP
		};

		private static ushort ushortRange = 0x003F; // 63             or 0b000 000 0000 111111
		private static ushort enumRange   = 0x03C0; // 960            or 0b000 000 1111 000000
		private static ushort setRange    = 0x1C00; // 7168           or 0b000 111 0000 000000
		private static ushort uintRange   = 0xE000; // 57344          or 0b111 000 0000 000000

		// CONSTANTS (Values for Closed Sets)

		[Flags]
		public enum GENDER : ushort
        {
			None     = 0,
			Male     = 1,
			Female   = 2,
			Any      = 3
        }
		[Flags]
		public enum CIVILITY : ushort
		{
            Neutral   = 0,
			Lawful    = 4,
            Chaotic   = 8,
		}

		[Flags]
		public enum MORALITY : ushort
		{
			Neutral = 0,
			Good    = 1,
			Evil    = 2,
		}

		[Flags]
		public enum ALIGNMENT : ushort
		{
			TrueNeutral    = 0, // CIVILITY.Neutral  | MORALITY.Neutral (default)
			NeutralGood    = 1, // CIVILITY.Neutral  | MORALITY.Good 
			NeutralEvil    = 2, // CIVILITY.Neutral  | MORALITY.Evil 
			LawfulNeutral  = 4, // CIVILITY.Lawful   | MORALITY.Neutral 
			LawfulGood     = 5, // CIVILITY.Lawful   | MORALITY.Good 
			LawfulEvil     = 6, // CIVILITY.Lawful   | MORALITY.Evil 
			ChaoticGood    = 8, // CIVILITY.Chaotic  | MORALITY.Neutral 
			ChaoticNeutral = 9, // CIVILITY.Chaotic  | MORALITY.Good 
			ChaoticEvil    = 10 // CIVILITY.Chaotic  | MORALITY.Evil 
		}

		[Flags]
		public enum ACUITY : ushort
		{
			None      = 0,
			Rested    = 1,
			Fatigued  = 2,
			Exhausted = 4,
			Any       = 7
		}

		// from namespace Kingmaker.Blueprints
		// public enum Race
        // {
		//	  Human,    // 0
		//	  Elf,      // 1
		//	  Dwarf,    // 2
		//	  Halfling, // 3
		//	  HalfOrc,  // 4
		//	  Gnome,    // 5
		//	  HalfElf,  // 6
		//	  Goblin,   // 7
		//	  Aasimar,  // 8
		//	  Tiefling, // 9
		//	  Oread,    // 10
		//	  Dhampir,  // 11
		//	  Kitsune,  // 12
		//	  Catfolk   // 13
        // }

		public enum RACEMASK : ushort
		{
			None     = 0,
			Human    = 1,
			Elf      = 2,
			HalfElf  = 4,
			Tiefling = 8,
			Dwarf    = 16,
			Catfolk  = 32,
			Halfling = 64,
			HalfOrc  = 128,
			Gnome    = 256,
			Goblin   = 512,
			Aasimar  = 1024,
			Oread    = 2048,
			Dhampir  = 4096,
			Kitsune  = 8192,
			Unknown  = 16384,
			Any      = 32767
		}

		[Flags]
		public enum NPCSIZE : ushort
		{
			None       = 0,
			Fine       = 1,
			Diminutive = 2,
			Tiny       = 4,
			Small      = 8,
			Medium     = 16,
			Large      = 32,
			Huge       = 64,
			Gargantuan = 128,
			Colossal   = 256,
			Any        = 511

		}

	    [Flags]
		public enum HEALTHENUM : ushort {
			None         = 0,
			HP_0_to_25   = 1,
			HP_25_to_50  = 2,
			HP_50_to_75  = 4,
			HP_75_to_100 = 8  
		}

		// -----------------------------------------------------------------
	    // Class Breakdown:
		// -----------------------------------------------------------------
		// Category | Archtype
        // -----------------------------------------------------------------
		// Warrior  +-- Fighter  (Can effectively use all manner of weapons, armor. With the right tools, can be a 1-man-army. But without tools can be quite vulnerable.)
		//          |-- Monk     (Minimalist fighter. No weapons, No armor. Wont help if you catch him with his pants down. The king of 1-on-1 combat.)
		// Mage     +-- Wizard   (Learned magic from intelligence and study and the application of scientific principles. Wide selection of spells, but must prepare for fights). 
		//          +-- Sorceror (Innate magic from heritage, bloodline or some otherworldly source. Small selection of spells, but can cast without preparation (Hard to catch off-guard))
		// Healer   +-- Cleric   (Blessed by the divine god they worship, powers are gifts from their gods. Only magic that isn't hindered by armor)
		//          |-- Druid    (Nature based magic/damage/protection. Based on something closer to science than religion)
		// Rogue    +-- Thief    (No magic or fighting proficiency, but everything else: Stealth, Sneak/backstab, Picklock, disarm trap
        //          |-- Bard     The "Jack of all trades, master of none". Often used as misc bucket for other character concepts that don't fit in another category.
        // ------------------------------------------------------------------
		// Mod breaks down each of the games classes into 2 or 3 Class Archtype components. These scores are also exposed as aggregated Class Categroy components.
        // Allowing portraits to change and adjust based on the NPC's level up choices. If the player respects someone into a warrior, the mod portrait can 
		// reflect that. 
        // ------------------------------------------------------------------

		[Flags]
		enum CLASSCATEGORY : ushort {
			None     = 0,
			Warrior  = 1,
			Mage     = 2,
			Healer   = 4,
			Rogue    = 8,
			Any      = 16
		}

		[Flags]
		enum CLASSARCHTYPE : ushort {
			None     = 0,
			Fighter  = 1,
			Monk     = 2,
			Wizard   = 4,
			Sorceror = 8,
			Cleric   = 16,
			Druid    = 32,
			Thief    = 64,
			Bard     = 128,
			Any      = 255
		}

		[Flags]
		enum CLASSMYTHIC : ushort {
			None           = 0,
			Aeon           = 1,
			Angel          = 2,
			Azata          = 4,
			Demon          = 8,
			Devil          = 16,
			GoldDragon     = 32,
			Legend         = 64,
			Lich           = 128,
			Trickster      = 256,
			SwarmThatWalks = 512,
			Any            = 1023
		}

		private static Dictionary<string, CLASSMYTHIC> ClassToMythic = new Dictionary<string, CLASSMYTHIC>()
		{
			{ "AEON",             CLASSMYTHIC.Aeon           },
            { "ANGEL",            CLASSMYTHIC.Angel          },
            { "AZATA",            CLASSMYTHIC.Azata          },
            { "DEMON",            CLASSMYTHIC.Demon          },
            { "DEVIL",            CLASSMYTHIC.Devil          },
            { "GOLDDRAGON",       CLASSMYTHIC.GoldDragon     },
            { "LEGEND",           CLASSMYTHIC.Legend         },
            { "LICH",             CLASSMYTHIC.Lich           },
            { "TRICKSTER",        CLASSMYTHIC.Trickster      },
            { "SWARM-THAT-WALKS", CLASSMYTHIC.SwarmThatWalks }
		};

		private static Dictionary<string, CLASSARCHTYPE[]> ClassToArchTypeList = new Dictionary<string, CLASSARCHTYPE[]>()
		{
			{ "ALCHEMISTCLASS",    new CLASSARCHTYPE[] { CLASSARCHTYPE.Bard,     CLASSARCHTYPE.Wizard,   CLASSARCHTYPE.Druid }     },
			{ "ARCHANISTCLASS",    new CLASSARCHTYPE[] { CLASSARCHTYPE.Wizard,   CLASSARCHTYPE.Sorceror, CLASSARCHTYPE.Sorceror }  },
			{ "BARBARIANCLASS",    new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Monk,     CLASSARCHTYPE.Monk }      }, // Martial Monk without evasion
			{ "BARDCLASS",         new CLASSARCHTYPE[] { CLASSARCHTYPE.Bard,     CLASSARCHTYPE.Bard,     CLASSARCHTYPE.Bard }      },
			{ "BLOODRAGERCLASS",   new CLASSARCHTYPE[] { CLASSARCHTYPE.Monk,     CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Monk }      }, // Martial Monk without evasion
			{ "CAVALIERCLASS",     new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Fighter }   },
			{ "CLERICCLASS",       new CLASSARCHTYPE[] { CLASSARCHTYPE.Cleric,   CLASSARCHTYPE.Cleric,   CLASSARCHTYPE.Cleric }    },
			{ "DRUIDCLASS",        new CLASSARCHTYPE[] { CLASSARCHTYPE.Druid,    CLASSARCHTYPE.Druid,    CLASSARCHTYPE.Druid }     },
			{ "FIGHTERCLASS",      new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Fighter }   },
			{ "HUNTERCLASS",       new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Thief,    CLASSARCHTYPE.Fighter }   },
			{ "INQUISITORCLASS",   new CLASSARCHTYPE[] { CLASSARCHTYPE.Cleric,   CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Thief }     },
			{ "KINETICISTCLASS",   new CLASSARCHTYPE[] { CLASSARCHTYPE.Sorceror, CLASSARCHTYPE.Monk,     CLASSARCHTYPE.Monk }      },

			// MAGUS can be a fighter wizard or fighter sorceror. The Fighter/Sorceror is the Eldrich Knight and even gets to 
			// pick a sorcerors bloodline benefits. I suspect most playing Magus are going the Eldrich Knight route, which is
            // why I earmarked the class more Sorcerer than Wizard. For the well-rounded, a few levels in Slayer:Archane
            // enforcer will get you sneak attack and Alchemist:Chirurgeon or Druid:Feyspeaker when get some healing spells
            // as well.
            
			{ "MAGUSCLASS",        new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Sorceror, CLASSARCHTYPE.Wizard }  },
			{ "MONKCLASS",         new CLASSARCHTYPE[] { CLASSARCHTYPE.Monk,     CLASSARCHTYPE.Monk,     CLASSARCHTYPE.Monk }      },
			{ "ORACLECLASS",       new CLASSARCHTYPE[] { CLASSARCHTYPE.Sorceror, CLASSARCHTYPE.Cleric,   CLASSARCHTYPE.Cleric }    },
			{ "PALADINCLASS",      new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Cleric }    },
			{ "RANGERCLASS",       new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Monk,     CLASSARCHTYPE.Monk }      }, // Martial Monk without damage reduction
			{ "ROGUECLASS",        new CLASSARCHTYPE[] { CLASSARCHTYPE.Thief,    CLASSARCHTYPE.Thief,    CLASSARCHTYPE.Thief }     },
			// Shaman's use hexes, but overall, they are an underpowered druid. The Shadow Shaman at least gets sneak attack. The intended prestige class for 
			// Shaman is Winter Witch.
			{ "SHAMANCLASS",       new CLASSARCHTYPE[] { CLASSARCHTYPE.Druid,    CLASSARCHTYPE.Druid,    CLASSARCHTYPE.Thief }     },

			// I would probably always pick warpriest cult-leader over shaman. 

			{ "SKALDCLASS",        new CLASSARCHTYPE[] { CLASSARCHTYPE.Bard,     CLASSARCHTYPE.Monk,      CLASSARCHTYPE.Fighter }   },
			{ "SLAYERCLASS",       new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Fighter,   CLASSARCHTYPE.Thief }  }, 

			// Slayer:Archane Enforcer is an interesting class, particularly if dueled with Archanist or Wizard:Exploiter. It has
            // fighter thaco, but gets sneak attacks and some special magic buffs (if you already have spells from another class). 

//			{ "ARCANEENFORCER",    new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Sorceror, CLASSARCHTYPE.Thief }     },

			{ "SORCERORCLASS",     new CLASSARCHTYPE[] { CLASSARCHTYPE.Sorceror, CLASSARCHTYPE.Sorceror,  CLASSARCHTYPE.Sorceror }  },
			{ "WARPRIESTCLASS",    new CLASSARCHTYPE[] { CLASSARCHTYPE.Cleric,   CLASSARCHTYPE.Fighter,   CLASSARCHTYPE.Thief }     },

			// WARPRIEST:CULT LEADER is probably the most interesting in that it provides sneak attack. So it is like a Cleric/Fighter/Theif

			{ "WITCHCLASS",        new CLASSARCHTYPE[] { CLASSARCHTYPE.Druid,    CLASSARCHTYPE.Druid,     CLASSARCHTYPE.Sorceror }  },

			// The most interesting Witch is the Stigmatized Witch, which has the Oracle's curse and 
			// casts like a sorceror (from resevior instead of memorized spells). 

			{ "WIZARDCLASS",       new CLASSARCHTYPE[] { CLASSARCHTYPE.Wizard,   CLASSARCHTYPE.Wizard,    CLASSARCHTYPE.Wizard }    },

			{ "ALDORISWORDLORD",   new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Fighter,   CLASSARCHTYPE.Fighter }   }, 
			{ "ARCANETRICKSTER",   new CLASSARCHTYPE[] { CLASSARCHTYPE.Thief,    CLASSARCHTYPE.Thief,     CLASSARCHTYPE.Wizard }    },
			{ "ASSASSIN",          new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Thief,     CLASSARCHTYPE.Thief }     },
			{ "DRAGONDISCIPLINE",  new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Fighter,   CLASSARCHTYPE.Sorceror }  },
			{ "DUELIST",           new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Fighter,   CLASSARCHTYPE.Fighter }   },
			{ "ELDRICHKNIGHT",     new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Fighter,   CLASSARCHTYPE.Fighter }   },
			{ "HELLKNIGHT",        new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Fighter,   CLASSARCHTYPE.Cleric }    },
			{ "HELLKNIGHTSIGNIFER",new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Fighter,   CLASSARCHTYPE.Wizard }    },
			{ "LOREMASTER",        new CLASSARCHTYPE[] { CLASSARCHTYPE.Wizard,   CLASSARCHTYPE.Sorceror,  CLASSARCHTYPE.Sorceror }  },
			{ "MYSTICTHEURGE",     new CLASSARCHTYPE[] { CLASSARCHTYPE.Wizard,   CLASSARCHTYPE.Cleric,    CLASSARCHTYPE.Cleric }    },
			{ "STALWARDDEFENDER",  new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Monk,      CLASSARCHTYPE.Monk }      },
			{ "STUDENTOFWAR",      new CLASSARCHTYPE[] { CLASSARCHTYPE.Monk,     CLASSARCHTYPE.Monk,      CLASSARCHTYPE.Monk }      },
			{ "WINTERWITCH",       new CLASSARCHTYPE[] { CLASSARCHTYPE.Druid,    CLASSARCHTYPE.Druid,     CLASSARCHTYPE.Sorceror }  },

			// MYTHIC
			{ "AEONCLASS",         new CLASSARCHTYPE[] { CLASSARCHTYPE.Sorceror, CLASSARCHTYPE.Cleric,    CLASSARCHTYPE.Cleric }    },
            { "ANGELCLASS",        new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Fighter,   CLASSARCHTYPE.Cleric }    },
            { "AZATACLASS",        new CLASSARCHTYPE[] { CLASSARCHTYPE.Bard,     CLASSARCHTYPE.Bard,      CLASSARCHTYPE.Bard }      },
            { "DEMONCLASS",        new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Sorceror,  CLASSARCHTYPE.Sorceror }  },
            { "DEVILCLASS",        new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Thief,     CLASSARCHTYPE.Thief }     },
            { "GOLDDRAGONCLASS",   new CLASSARCHTYPE[] { CLASSARCHTYPE.Monk,     CLASSARCHTYPE.Monk,      CLASSARCHTYPE.Monk }      },
            { "LEGENDCLASS",       new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Fighter,   CLASSARCHTYPE.Fighter  }  },
            { "LICHCLASS",         new CLASSARCHTYPE[] { CLASSARCHTYPE.Wizard,   CLASSARCHTYPE.Wizard,    CLASSARCHTYPE.Wizard }    },
            { "TRICKSTERCLASS",    new CLASSARCHTYPE[] { CLASSARCHTYPE.Thief,    CLASSARCHTYPE.Sorceror,  CLASSARCHTYPE.Wizard }    },
            { "SWARM-THAT-WALKSCLASS",  new CLASSARCHTYPE[] { CLASSARCHTYPE.Druid,    CLASSARCHTYPE.Druid,     CLASSARCHTYPE.Druid }     }

		};


		private static Dictionary<string, RACEMASK> StrToRaceMask = new Dictionary<string,  RACEMASK>()
		{
			// from Kingmaker.Visual.CharacterSystem.CharacterStudio.Race
			// from Kingmaker.Visual.CharacterSystem.EquipmentEntitiesDictionary.Races

			{ "HUMAN",        RACEMASK.Human    },
			{ "ELF",          RACEMASK.Elf      },
			{ "DWARF",        RACEMASK.Dwarf    },
			{ "HALFLING",     RACEMASK.Halfling },
			{ "HALFORC",      RACEMASK.HalfOrc  },
			{ "GNOME",        RACEMASK.Gnome    },
			{ "HALFELF",      RACEMASK.HalfElf  },
			{ "GOBLIN",       RACEMASK.Goblin   },
			{ "AASIMAR",      RACEMASK.Aasimar  },
			{ "TIEFLING",     RACEMASK.Tiefling },
			{ "OREAD",        RACEMASK.Oread    },
			{ "DHAMPIR",      RACEMASK.Dhampir  },
			{ "KITSUNE",      RACEMASK.Kitsune  },
			{ "CATFOLK",      RACEMASK.Catfolk  },

			{ "HUMANRACE",    RACEMASK.Human    },
			{ "ELFRACE",      RACEMASK.Elf      },
			{ "DWARFRACE",    RACEMASK.Dwarf    },
			{ "HALFLINGRACE", RACEMASK.Halfling },
			{ "HALFORCRACE",  RACEMASK.HalfOrc  },
			{ "GNOMERACE",    RACEMASK.Gnome    },
			{ "HALFELFRACE",  RACEMASK.HalfElf  },
			{ "GOBLINRACE",   RACEMASK.Goblin   },
			{ "AASIMARRACE",  RACEMASK.Aasimar  },
			{ "TIEFLINGRACE", RACEMASK.Tiefling },
			{ "OREADRACE",    RACEMASK.Oread    },
			{ "DHAMPIRRACE",  RACEMASK.Dhampir  },
			{ "KITSUNERACE",  RACEMASK.Kitsune  },
			{ "CATFOLKRACE",  RACEMASK.Catfolk  }
		};

		private static Dictionary<string, ALIGNMENT> StrToAlignmentEnum = new Dictionary<string, ALIGNMENT>() {
			// From Kingmaker.Enums.Alignment
			{ "LAWFULGOOD",     ALIGNMENT.LawfulGood     },
			{ "NEUTRALGOOD",    ALIGNMENT.NeutralGood    },
			{ "CHAOTICGOOD",    ALIGNMENT.ChaoticGood    },
			{ "LAWFULNEUTRAL",  ALIGNMENT.LawfulNeutral  },
			{ "TRUENEUTRAL",    ALIGNMENT.TrueNeutral    },
			{ "CHAOTICNEUTRAL", ALIGNMENT.ChaoticNeutral },
			{ "LAWFULEVIL",     ALIGNMENT.LawfulEvil     },
			{ "NEUTRALEVIL",    ALIGNMENT.NeutralEvil    },
			{ "CHAOTICEVIL",    ALIGNMENT.ChaoticEvil    }
		};


		private static Dictionary<ALIGNMENT, Tuple<CIVILITY, MORALITY>> AlignmentToParts = 
			new Dictionary<ALIGNMENT, Tuple<CIVILITY, MORALITY>>() {
			{ ALIGNMENT.TrueNeutral,    new Tuple<CIVILITY, MORALITY>(CIVILITY.Neutral, MORALITY.Neutral) },
			{ ALIGNMENT.NeutralGood,    new Tuple<CIVILITY, MORALITY>(CIVILITY.Neutral, MORALITY.Good) },
			{ ALIGNMENT.NeutralEvil,    new Tuple<CIVILITY, MORALITY>(CIVILITY.Neutral, MORALITY.Evil) },
			{ ALIGNMENT.LawfulNeutral,  new Tuple<CIVILITY, MORALITY>(CIVILITY.Lawful,  MORALITY.Neutral) },
			{ ALIGNMENT.LawfulGood,     new Tuple<CIVILITY, MORALITY>(CIVILITY.Lawful,  MORALITY.Good) },
			{ ALIGNMENT.LawfulEvil,     new Tuple<CIVILITY, MORALITY>(CIVILITY.Lawful,  MORALITY.Evil) },
			{ ALIGNMENT.ChaoticGood,    new Tuple<CIVILITY, MORALITY>(CIVILITY.Chaotic, MORALITY.Good) },
			{ ALIGNMENT.ChaoticNeutral, new Tuple<CIVILITY, MORALITY>(CIVILITY.Chaotic, MORALITY.Neutral) },
			{ ALIGNMENT.ChaoticEvil,    new Tuple<CIVILITY, MORALITY>(CIVILITY.Chaotic, MORALITY.Evil) }
		};

		private static Dictionary<Kingmaker.Enums.Size, NPCSIZE> KMSizeToNPCSize = new Dictionary<Kingmaker.Enums.Size, NPCSIZE>() {
			{ Kingmaker.Enums.Size.Fine,       NPCSIZE.Fine       },
			{ Kingmaker.Enums.Size.Diminutive, NPCSIZE.Diminutive },
			{ Kingmaker.Enums.Size.Tiny,       NPCSIZE.Tiny       },
			{ Kingmaker.Enums.Size.Small,      NPCSIZE.Small      },
			{ Kingmaker.Enums.Size.Medium,     NPCSIZE.Medium     },
			{ Kingmaker.Enums.Size.Large,      NPCSIZE.Large      },
			{ Kingmaker.Enums.Size.Huge,       NPCSIZE.Huge       },
			{ Kingmaker.Enums.Size.Gargantuan, NPCSIZE.Gargantuan },
			{ Kingmaker.Enums.Size.Colossal,   NPCSIZE.Colossal   }
		};

//		private static Dictionary<RuleContext.ALIGNMENT, string> AlignmentToStr = new Dictionary<RuleContext.ALIGNMENT, string>() {
//			{ RuleContext.ALIGNMENT.LawfulGood,     "LawfulGood"     },
//			{ RuleContext.ALIGNMENT.NeutralGood,    "NeutralGood"    },
//			{ RuleContext.ALIGNMENT.ChaoticGood,    "ChaoticGood"    },
//			{ RuleContext.ALIGNMENT.LawfulNeutral,  "LawfulNeutral"  },
//			{ RuleContext.ALIGNMENT.TrueNeutral,    "TrueNeutral"    },
//			{ RuleContext.ALIGNMENT.ChaoticNeutral, "ChaoticNeutral" },
//			{ RuleContext.ALIGNMENT.LawfulEvil,     "LawfulEvil"     },
//			{ RuleContext.ALIGNMENT.NeutralEvil,    "NeutralEvil"    },
//			{ RuleContext.ALIGNMENT.ChaoticEvil,    "ChaoticEvil"    }
//		};

//		private static Dictionary<RuleContext.NPCSIZE, string> NPCSizeToStr = new Dictionary<RuleContext.NPCSIZE, string>()
//		{
//			{ RuleContext.NPCSIZE.Fine,       "Fine"       },
//			{ RuleContext.NPCSIZE.Diminutive, "Diminutive" },
//			{ RuleContext.NPCSIZE.Tiny,       "Tiny"       },
//			{ RuleContext.NPCSIZE.Small,      "Small"      },
//			{ RuleContext.NPCSIZE.Medium,     "Medium"     },
//			{ RuleContext.NPCSIZE.Large,      "Large"      },
//			{ RuleContext.NPCSIZE.Huge,       "Huge"       },
//			{ RuleContext.NPCSIZE.Gargantuan, "Gargantuan" },
//			{ RuleContext.NPCSIZE.Colossal,   "Colossal"   }
//		};


//		private static Dictionary<RuleContext.RACEMASK, string> RaceMaskToStr = new Dictionary<RuleContext.RACEMASK, string>()
//		{
//			{ RuleContext.RACEMASK.Human,    "Human"    },
//			{ RuleContext.RACEMASK.Elf,      "Elf"      },
//			{ RuleContext.RACEMASK.Dwarf,    "Dwarf"    },
//			{ RuleContext.RACEMASK.Halfling, "Halfling" },
//			{ RuleContext.RACEMASK.HalfOrc,  "HalfOrc"  },
//			{ RuleContext.RACEMASK.Gnome,    "Gnome"    },
//			{ RuleContext.RACEMASK.HalfElf,  "HalfElf"  },
//			{ RuleContext.RACEMASK.Goblin,   "Goblin"   },
//			{ RuleContext.RACEMASK.Aasimar,  "Aasimar"  },
//			{ RuleContext.RACEMASK.Tiefling, "Tiefling" },
//			{ RuleContext.RACEMASK.Oread,    "Oread"    },
//			{ RuleContext.RACEMASK.Dhampir,  "Dhampir"  },
//			{ RuleContext.RACEMASK.Kitsune,  "Kitsune"  },
//			{ RuleContext.RACEMASK.Catfolk,  "Catfolk"  }
//		};

//		private static Dictionary<RuleContext.CIVILITY, string> CivilityToStr = new Dictionary<RuleContext.CIVILITY, string>()
//		{
//			{ RuleContext.CIVILITY.Neutral, "Neutral" },
//			{ RuleContext.CIVILITY.Lawful,  "Lawful"  },
//			{ RuleContext.CIVILITY.Chaotic, "Chaotic" }
//		};

//		private static Dictionary<RuleContext.MORALITY, string> MoralityToStr = new Dictionary<RuleContext.MORALITY, string>()
//		{
//			{ RuleContext.MORALITY.Neutral, "Neutral" },
//			{ RuleContext.MORALITY.Good,    "Good"    },
//			{ RuleContext.MORALITY.Evil,    "Evil"    }
//		};

//		private static Dictionary<RuleContext.ACUITY, string> AcuityToStr = new Dictionary<RuleContext.ACUITY, string>()
//		{
//			{ RuleContext.ACUITY.Rested,    "Rested"    },
//			{ RuleContext.ACUITY.Fatigued,  "Fatigued"  },
//			{ RuleContext.ACUITY.Exhausted, "Exhausted" }
//		};




		// NOTES: Criteria can be composed of multuple conditions and each condition could apply to 
		//        a different property with differnt typing.
		//        
		//        Ideally Conditions are dedicated type aware objects
		//
		//          ConditionUShortGreaterThan()
		//
		//        As such, they would know with array to use and what index offset to check:
		//        class Interface Condition {
		//            public abstract void evaluate(RuleContext r);
		//        }

		// 		  static class ConditionUShortEqual() : Condition {
		//                ushort value;
		//                int index;
		//                ConditionUShortEqual(int i, ushort value) {
		//                    this.index = i;
		//                    this.value = value;
		//                }
		//                boolean evaluate(RuleContext rc) {
		//                    // See how this condition object has intimate knowledge of 
		//                    // the rulecontext and knows exactly what fields and offsets to check. 
		//
		//                    // Because of this tight coupling, it may make sense for the Condition
		//                    // objects to live in the RuleContext as part of that class. The broader
		//                    // concept of Criteria (list of Conditions) and a Rule (How to evaluate 
		//                    // a list of conditions) and RuleEngine (How to apply multiple rules 
		//                    // to a give ruleContext and when to bail, try again, etc...).. those 
		//                    // aspects can remain separated. 
		//
		//                    return rc.USHORT_VALUES[propIndex] == this.value;
		//                }
	    //            }

        //        }
		//
		//        
		//              if (r.
	    //            }
        //
        //        expect the property to be in the proper zone, 

		// condition_equal()
		// public Boolean routeRuleCondition(Rule rule) // rule.prop
        // {
		//	if  (0 != rule.prop & ushortRange) return handle_ushort_condition((rule.prop & ushortRange),       rule.ushortValue, rule.condition;
		//	if  (0 != rule.prop & enumRange)   return handle_enum_condition(  ((rule.prop & enumRange) >> 6),  rule.enumValue,   rule.condition);
		//	if  (0 != rule.prop & setRange)    return handle_set_condition(   ((rule.prop & setRange)  >> 10), rule.setValue,    rule.condition);
		//	if  (0 != rule.prop & uintRange)   return handle_uint_condition(  ((rule.prop & uintRange) >> 13), rule.uintValue,   rule.condition);
		//	return false;
        // }

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
			ushort alignment = (ushort)(RuleContext.strToAlignment(npc.Descriptor.Alignment.ValueVisible.ToString()));
            ENUM_VALUES[(((int)PROPERTY.Alignment) >> 6)] = alignment;
			Tuple<CIVILITY, MORALITY> alignmentParts = AlignmentToParts[(ALIGNMENT)(alignment)];
            ENUM_VALUES[(((int)PROPERTY.Civility) >> 6)] = (ushort)(alignmentParts.Item1);
            ENUM_VALUES[(((int)PROPERTY.Morality) >> 6)] =  (ushort)(alignmentParts.Item2);
            ENUM_VALUES[(((int)PROPERTY.Size) >> 6)] = (ushort)(RuleContext.KMSizeToNPCSize[(npc.State?.Size ?? npc.Descriptor.OriginalSize)]);
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
			ENUM_VALUES[(((int)PROPERTY.Size_Base) >> 6)] = (ushort)(RuleContext.KMSizeToNPCSize[npc.Descriptor.OriginalSize]);
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
					ClassToMythic.TryGetValue(mythName, out mythEnum);
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
					if (RuleContext.ClassToArchTypeList.ContainsKey(mName))
                    {
						int weight = 5;
						foreach (ushort archType in RuleContext.ClassToArchTypeList[mName])
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
					if (RuleContext.ClassToArchTypeList.ContainsKey(nName))
					{
						int weight = 5;
						foreach (ushort archType in RuleContext.ClassToArchTypeList[nName])
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

		public static RuleContext.RACEMASK strToRaceMask(string r)
		{
			string ru = r.ToUpper();
			if (!StrToRaceMask.ContainsKey(ru))
			{
				logAlways($"Error processing Race [{r}]: does not match any known race. Using Human");
				ru = "HUMAN";
			}
			return StrToRaceMask[ru];
		}

		public static RuleContext.ALIGNMENT strToAlignment(string a)
		{
			string au = a.ToUpper();
			if (!StrToAlignmentEnum.ContainsKey(au))
			{
				logAlways($"Error processing Alignment [{a}]: Does not match any known alignment. Using TrueNeutral");
				au = "TRUENEUTRAL";
			}
			return StrToAlignmentEnum[au];
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

			// TODO: Try outputting these enums without the ...ToStr Dictionaries.
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
//			sw.WriteLine($"Civility  [{(CIVILITY)ENUM_VALUES[civilityOffset]}]");
//			sw.WriteLine($"Civility  [{RuleContext.CivilityToStr[(RuleContext.CIVILITY)ENUM_VALUES[civilityOffset]]}]");
//			sw.WriteLine($"Morality  [{RuleContext.MoralityToStr[(RuleContext.MORALITY)ENUM_VALUES[moralityOffset]]}]");
//			sw.WriteLine($"Alignment [{RuleContext.AlignmentToStr[(RuleContext.ALIGNMENT)ENUM_VALUES[alignmentOffset]]}]");
//			sw.WriteLine($"Acuity    [{RuleContext.AcuityToStr[(RuleContext.ACUITY)ENUM_VALUES[acuityOffset]]}]");
//			sw.WriteLine($"Race      [{RuleContext.RaceMaskToStr[(RuleContext.RACEMASK)ENUM_VALUES[raceOffset]]}]");
//			sw.WriteLine($"Gender    [{(GENDER)ENUM_VALUES[genderOffset]}]");
//			sw.WriteLine($"Health    [{(HEALTHENUM)ENUM_VALUES[healthOffset]}]");
//			sw.WriteLine($"Size      [{RuleContext.NPCSizeToStr[(RuleContext.NPCSIZE)ENUM_VALUES[sizeOffset]]}]");
//			sw.WriteLine($"Size_Base [{RuleContext.NPCSizeToStr[(RuleContext.NPCSIZE)ENUM_VALUES[sizeOffsetBase]]}]");
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
		public static RuleContext.PROPERTYTYPE GetPropType(RuleContext.PROPERTY prop)
        {
			if ( (((ushort)prop) & ushortRange) != 0) return PROPERTYTYPES[((int)prop)];
			if ( (((ushort)prop) & enumRange)   != 0) return PROPERTYTYPES[63 + (((int)prop) >> 6)];
			if ( (((ushort)prop) & setRange)    != 0) return PROPERTYTYPES[78 + (((int)prop )>> 10)];
			if ( (((ushort)prop) & uintRange)   != 0) return PROPERTYTYPES[85 + (((int)prop) >> 13)];
			return PROPERTYTYPES[0];
        }

		// Rule Support
		public interface ICondition {
		    bool evaluate(RuleContext rc);
	    }

		public class ConditionUShortEQ : ICondition { ushort value; int index;
			ConditionUShortEQ(int i, ushort value) { this.index = i; this.value = value; }
			public bool evaluate(RuleContext rc) { return this.value == rc.USHORT_VALUES[index]; }
		}
		public class ConditionUShortGT : ICondition { ushort value; int index; 
	        ConditionUShortGT(int i, ushort value) { this.index = i; this.value = value; }
			public bool evaluate(RuleContext rc) { return this.value > rc.USHORT_VALUES[index]; }
		}
		public class ConditionUShortGTE : ICondition { ushort value; int index; 
	        ConditionUShortGTE(int i, ushort value) { this.index = i; this.value = value; }
			public bool evaluate(RuleContext rc) { return this.value >= rc.USHORT_VALUES[index]; }
		}
		public class ConditionUShortLT : ICondition { ushort value; int index; 
	        ConditionUShortLT(int i, ushort value) { this.index = i; this.value = value; }
			public bool evaluate(RuleContext rc) { return this.value < rc.USHORT_VALUES[index]; }
		}
		public class ConditionUShortLTE : ICondition { ushort value; int index; 
	        ConditionUShortLTE(int i, ushort value) { this.index = i; this.value = value; }
			public bool evaluate(RuleContext rc) { return this.value <= rc.USHORT_VALUES[index]; }
		}
		public class ConditionUShortNEQ : ICondition { ushort value; int index;
			ConditionUShortNEQ(int i, ushort value) { this.index = i; this.value = value; }
			public bool evaluate(RuleContext rc) { return this.value != rc.USHORT_VALUES[index]; }
		}


		public class ConditionEnumAny : ICondition { ushort mask; int index;
			ConditionEnumAny(int i, ushort mask) { this.index = i; this.mask = mask; }
			public bool evaluate(RuleContext rc) { return (0 != (rc.ENUM_VALUES[index] & mask)); }
		}
		public class ConditionEnumNotAny : ICondition { ushort mask; int index;
			ConditionEnumNotAny(int i, ushort mask) { this.index = i; this.mask = mask; }
			public bool evaluate(RuleContext rc) { return (0 == (rc.ENUM_VALUES[index] & mask)); }
		}
		public class ConditionEnumAll : ICondition { ushort mask; int index;
			ConditionEnumAll(int i, ushort mask) { this.index = i; this.mask = mask; }
			public bool evaluate(RuleContext rc) { return (mask == (rc.ENUM_VALUES[index] & mask)); }
		}
		public class ConditionEnumNotAll : ICondition { ushort mask; int index;
			ConditionEnumNotAll(int i, ushort mask) { this.index = i; this.mask = mask; }
			public bool evaluate(RuleContext rc) { return (mask != (rc.ENUM_VALUES[index] & mask)); }
		}
		public class ConditionEnumEQ : ICondition { ushort mask; int index;
			ConditionEnumEQ(int i, ushort mask) { this.index = i; this.mask = mask; }
			public bool evaluate(RuleContext rc) { return (mask == rc.ENUM_VALUES[index]); }
		}
		public class ConditionEnumGT : ICondition { ushort mask; int index;
			ConditionEnumGT(int i, ushort mask) { this.index = i; this.mask = mask; }
			public bool evaluate(RuleContext rc) { return (mask > rc.ENUM_VALUES[index]); }
		}
		public class ConditionEnumLT : ICondition { ushort mask; int index;
			ConditionEnumLT(int i, ushort mask) { this.index = i; this.mask = mask; }
			public bool evaluate(RuleContext rc) { return (mask < rc.ENUM_VALUES[index]); }
		}
		public class ConditionEnumLTE : ICondition { ushort mask; int index;
			ConditionEnumLTE(int i, ushort mask) { this.index = i; this.mask = mask; }
			public bool evaluate(RuleContext rc) { return (mask <= rc.ENUM_VALUES[index]); }
		}
		public class ConditionEnumGTE : ICondition { ushort mask; int index;
			ConditionEnumGTE(int i, ushort mask) { this.index = i; this.mask = mask; }
			public bool evaluate(RuleContext rc) { return (mask >= rc.ENUM_VALUES[index]); }
		}
		public class ConditionEnumNEQ : ICondition { ushort mask; int index;
			ConditionEnumNEQ(int i, ushort mask) { this.index = i; this.mask = mask; }
			public bool evaluate(RuleContext rc) { return (mask != rc.ENUM_VALUES[index]); }
		}

		public class ConditionSetAny : ICondition { HashSet<String> value; int index;
			ConditionSetAny(int i,  HashSet<String> value) { this.index = i; this.value = value; }
			public bool evaluate(RuleContext rc) { return  value.Overlaps(rc.STRSET_VALUES[index]); }
		}
		public class ConditionSetAll : ICondition { HashSet<String> value; int index;
			ConditionSetAll(int i,  HashSet<String> value) { this.index = i; this.value = value; }
			public bool evaluate(RuleContext rc) { return  value.IsSubsetOf(rc.STRSET_VALUES[index]); }
		}
		public class ConditionSetNotAny : ICondition { HashSet<String> value; int index;
			ConditionSetNotAny(int i,  HashSet<String> value) { this.index = i; this.value = value; }
			public bool evaluate(RuleContext rc) { return  !value.Overlaps(rc.STRSET_VALUES[index]); }
		}
		public class ConditionSetNotAll : ICondition { HashSet<String> value; int index;
			ConditionSetNotAll(int i,  HashSet<String> value) { this.index = i; this.value = value; }
			public bool evaluate(RuleContext rc) { return  !value.IsSubsetOf(rc.STRSET_VALUES[index]); }
		}


		public class ConditionUIntEQ : ICondition { uint value; int index;
			ConditionUIntEQ(int i, uint value) { this.index = i; this.value = value; }
			public bool evaluate(RuleContext rc) { return this.value == rc.UINT_VALUES[index]; }
		}
		public class ConditionUIntGT : ICondition { uint value; int index; 
	        ConditionUIntGT(int i, uint value) { this.index = i; this.value = value; }
			public bool evaluate(RuleContext rc) { return this.value > rc.UINT_VALUES[index]; }
		}
		public class ConditionUIntGTE : ICondition { uint value; int index; 
	        ConditionUIntGTE(int i, uint value) { this.index = i; this.value = value; }
			public bool evaluate(RuleContext rc) { return this.value >= rc.UINT_VALUES[index]; }
		}
		public class ConditionUIntLT : ICondition { uint value; int index; 
	        ConditionUIntLT(int i, uint value) { this.index = i; this.value = value; }
			public bool evaluate(RuleContext rc) { return this.value < rc.UINT_VALUES[index]; }
		}
		public class ConditionUIntLTE : ICondition { uint value; int index; 
	        ConditionUIntLTE(int i, uint value) { this.index = i; this.value = value; }
			public bool evaluate(RuleContext rc) { return this.value <= rc.UINT_VALUES[index]; }
		}
		public class ConditionUIntNEQ : ICondition { uint value; int index;
			ConditionUIntNEQ(int i, uint value) { this.index = i; this.value = value; }
			public bool evaluate(RuleContext rc) { return this.value != rc.UINT_VALUES[index]; }
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

