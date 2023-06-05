using System;                     // Needed for basic types like String
using System.Collections.Generic; // Needed for List, Set, Dictionary, etc...

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules
{

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
		// Level/Class (4 items)
//		Unused                      = 57, // Previously PrimaryClassArchTypeLevel
//		Unused                      = 58, // Previously SecondaryClassArchTypeLevel
		Level_Base                  = 59,
		Level                       = 60,
		// State (3 items)
		InCombat                    = 61,
		IsNaked                     = 62,
		UsingDefaultEquipment       = 63,

		// SKIP to 64 as 0-63 can be easily masked out with a shift >> 6 (same as dividing by 2, 6 times).

		// Closed Sets (13 Enums)
		PrimaryClassCategory        = 1  << 6, // (1 * 64)  = 64  | 0b00 0000 0001 000000
		SecondaryClassCategory      = 2  << 6, // (2 * 64)  = 128 | 0b00 0000 0010 000000
		PrimaryClassArchType        = 3  << 6, // (3 * 64)  = 192 | 0b00 0000 0011 000000
		SecondaryClassArchType      = 4  << 6, // (4 * 64)  = 256 | 0b00 0000 0100 000000
//		Unused                      = 5  << 6, // (5 * 64)  = 320 | 0b00 0000 0101 000000  // Previously MythicClass
//		Unused                      = 6  << 6, // (6 * 64)  = 384 | 0b00 0000 0110 000000  // Previously AllClassArchTypes
//		Unused                      = 7  << 6, // (7 * 64)  = 448 | 0b00 0000 0111 000000  // Previously Health
		Race                        = 8  << 6, // (8 * 64)  = 512 | 0b00 0000 1000 000000
//		Unused                      = 9  << 6, // (9 * 64)  = 576 | 0b00 0000 1001 000000  // Previously Gender
		Civility                    = 10 << 6, // (10 * 64) = 640 | 0b00 0000 1010 000000
		Morality                    = 11 << 6, // (11 * 64) = 704 | 0b00 0000 1011 000000
		Alignment                   = 12 << 6, // (12 * 64) = 768 | 0b00 0000 1100 000000
		Acuity                      = 13 << 6, // (13 * 64) = 832 | 0b00 0000 1101 000000
		Size                        = 14 << 6, // (14 * 64) = 896 | 0b00 0000 1110 000000
		Size_Base                   = 15 << 6, // (15 * 64) = 960 | 0b00 0000 1111 000000
        // String Sets (15 HashSet<String>'s)
		Facts                       = 1 << 10,  // (1*1024)  = 1024  | 0b00 0001 0000 000000
		Buffs                       = 2 << 10,  // (2*1024)  = 2048  | 0b00 0010 0000 000000
		SharedStash                 = 3 << 10,  // (3*1024)  = 3072  | 0b00 0011 0000 000000
		Inventory                   = 4 << 10,  // (4*1024)  = 4096  | 0b00 0100 0000 000000

		ActiveQuests                = 5 << 10,  // (5*1024)  = 5120  | 0b00 0101 0000 000000
		CompletedQuests             = 6 << 10,  // (6*1024)  = 6144  | 0b00 0110 0000 000000
		FailedQuests                = 7 << 10,  // (7*1024)  = 7168  | 0b00 0111 0000 000000
		KnownQuests                 = 8 << 10,  // (8*1024)  = 8192  | 0b00 1000 0000 000000

		EquippedArmor               = 9 << 10,   // (9*1024)  = 9216  | 0b00 1001 0000 000000
		EquippedWeapons             = 10 << 10,  // (10*1024) = 10240 | 0b00 1010 0000 000000
		EquippedRings               = 11 << 10,  // (11*1024) =       | 0b00 1011 0000 000000
		EquippedNecklaces           = 12 << 10,  // (12*1024) =       | 0b00 1100 0000 000000
		Equipped                    = 13 << 10,  // (13*1024) =       | 0b00 1101 0000 000000

		Dialog                      = 14 << 10,  // (14*1024) =       | 0b00 1110 0000 000000
		Area                        = 15 << 10,  // (15*1024) =       | 0b00 1111 0000 000000
		// Integers (1 item)
		XP                          = 1 << 14,   // (1*8192) = 16384  | 0b01 0000 0000 000000
//		Unused                      = 2 << 14,   // (2*8192) = 32768  | 0b10 0000 0000 000000
//		Unused                      = 3 << 14,   // (1*8192) = 49152  | 0b11 0000 0000 000000
    };

	public enum PROPERTYTYPE : ushort
	{
		// Note : All but container can be represented using ints...
		NONE          = 0,
		USHORT        = 1, 
		CLASSARCHTYPE = 2, 
        CLASSCATEGORY = 3,
//      CLASSMYTHIC   = 4,  // Previously CLASSMYTHIC   : ushort
//		HEALTHENUM    = 5,  // Previously HEALTHENUM    : ushort
		RACEMASK      = 6,  // cast to RACE       : ushort
//		GENDER        = 7,  // Previously GENDER        : ushort
		CIVILITY      = 8,  // cast to CIVILITY   : ushort
		MORALITY      = 9,  // cast to MORALITY   : ushort
		ALIGNMENT     = 10, // cast to ALIGNMENT  : ushort
		ACUITY        = 11, // cast to ACUITY     : ushort
		NPCSIZE       = 12, // cast to NPCSIZE Size

		// The first 10 above can be retrieved using a large ushort[] that maintains the values. 

		UINT          = 13,  // cast to uint
		STRSET        = 14   // cast value to HashSet<string>
	};

//	[Flags]
//	public enum GENDER : ushort
//   {
//		None     = 0,
//		Male     = 1,
//		Female   = 2,
//		Any      = 3
 //   }

	[Flags]
	public enum CIVILITY : ushort
	{
        None      = 0,
        Neutral   = 1,
		Lawful    = 2,
        Chaotic   = 4,
		Any       = 7
	}

	[Flags]
	public enum MORALITY : ushort
	{
        None    = 0,
        Neutral = 1,
		Good    = 2,
		Evil    = 4,
		Any     = 7
	}

	[Flags]
	public enum ALIGNMENT : ushort
	{
        None           = 0,
		ChaoticGood    = 1,    
		LawfulGood     = 2,    
		NeutralGood    = 4,   
		LawfulNeutral  = 8,    
		TrueNeutral    = 16,  
		ChaoticNeutral = 32,  
		NeutralEvil    = 64,   
		LawfulEvil     = 128, 
		ChaoticEvil    = 256,
		Any            = 511
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

//	[Flags]
//	public enum HEALTHENUM : ushort {
//		None         = 0,
//		HP_0_to_25   = 1,
//		HP_25_to_50  = 2,
//		HP_50_to_75  = 4,
//		HP_75_to_100 = 8  
//	}

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
    //          |-- Bard     The diplomatic and charasmatic thief. Typically a "Jack of all Trades". Often used as misc bucket for other character concepts that don't fit in another category.
    // ------------------------------------------------------------------
	// Mod breaks down each of the games classes into 2 or 3 Class Archtype components. These scores are also exposed as aggregated Class Categroy components.
    // Allowing portraits to change and adjust based on the NPC's level up choices. If the player respects someone into a warrior, the mod portrait can 
	// reflect that. 
    // ------------------------------------------------------------------

	[Flags]
	public enum CLASSCATEGORY : ushort {
		None     = 0,
		Warrior  = 1,
		Mage     = 2,
		Healer   = 4,
		Rogue    = 8,
		Any      = 15
	}

	[Flags]
	public enum CLASSARCHTYPE : ushort {
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

//	[Flags]
//	public enum CLASSMYTHIC : ushort {
//		None           = 0,
//		Aeon           = 1,
//		Angel          = 2,
//		Azata          = 4,
//		Demon          = 8,
//		Devil          = 16,
//		GoldDragon     = 32,
//		Legend         = 64,
//		Lich           = 128,
//		Trickster      = 256,
//		SwarmThatWalks = 512,
//		Any            = 1023
//	}

	public static class Meta 
	{ 
		public static ushort ushortRange = 0x003F; // 63             or 0b00 0000 0000 111111
		public static ushort enumRange   = 0x03C0; // 960            or 0b00 0000 1111 000000
		public static ushort setRange    = 0x3C00; // 15360          or 0b00 1111 0000 000000
		public static ushort uintRange   = 0xC000; // 49152          or 0b11 0000 0000 000000

		public static PROPERTYTYPE[] PROPERTYTYPES  = new PROPERTYTYPE[]
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
			// Level/XP/Class (7 items)
			PROPERTYTYPE.NONE,   // [57]: UNUSED // Previously PROPERTYTYPE.USHORT - PrimaryClassArchTypeLevel
			PROPERTYTYPE.NONE,   // [58]: UNUSED // Previously PROPERTYTYPE.USHORT - SecondaryClassArchTypeLevel
			PROPERTYTYPE.USHORT, // [59]: Level_Base
			PROPERTYTYPE.USHORT, // [60]: Level
			PROPERTYTYPE.USHORT, // [61]: InCombat
			PROPERTYTYPE.USHORT, // [62]: IsNaked
			PROPERTYTYPE.USHORT, // [63]: UsingDefaultEquipment
			// Closed Sets (15 Enums)
			PROPERTYTYPE.CLASSCATEGORY, // [64]: PrimaryClassCategory
			PROPERTYTYPE.CLASSCATEGORY, // [65]: SecondaryClassCategory
			PROPERTYTYPE.CLASSARCHTYPE, // [66]: PrimaryClassArchType
			PROPERTYTYPE.CLASSARCHTYPE, // [67]: SecondaryClassArchType
			PROPERTYTYPE.NONE,          // [68]: UNUSED // Previously PROPERTYTYPE.CLASSARCHTYPE - AllClassArchTypes
			PROPERTYTYPE.NONE,          // [69]: UNUSED // Previously PROPERTYTYPE.CLASSMYTHIC   - MythicClass
			PROPERTYTYPE.NONE,          // [70]: UNUSED // Previously PROPERTYTYPE.HEALTHENUM    - Health
			PROPERTYTYPE.RACEMASK,      // [71]: Race
			PROPERTYTYPE.NONE,          // [72]: UNUSED // Previously PROPERTYTYPE.GENDER        - Gender
			PROPERTYTYPE.CIVILITY,      // [73]: Civility
			PROPERTYTYPE.MORALITY,      // [74]: Morality
			PROPERTYTYPE.ALIGNMENT,     // [75]: Alignment
			PROPERTYTYPE.ACUITY,        // [76]: Acuity
			PROPERTYTYPE.NPCSIZE,       // [77]: Size
			PROPERTYTYPE.NPCSIZE,       // [78]: Size_Base
			// Open Sets (15 HashSet<String>s)
			PROPERTYTYPE.STRSET, // [79]: Facts
			PROPERTYTYPE.STRSET, // [80]: Buffs
			PROPERTYTYPE.STRSET, // [81]: SharedStash
			PROPERTYTYPE.STRSET, // [82]: Inventory
			PROPERTYTYPE.STRSET, // [83]: EquippedArmor
			PROPERTYTYPE.STRSET, // [84]: EquippedWeapons
			PROPERTYTYPE.STRSET, // [85]: EquippedRings
			PROPERTYTYPE.STRSET, // [86]: EquippedNecklaces
			PROPERTYTYPE.STRSET, // [87]: Equipped
			PROPERTYTYPE.STRSET, // [88]: ActiveQuests
			PROPERTYTYPE.STRSET, // [89]: CompletedQuests
			PROPERTYTYPE.STRSET, // [90]: FailedQuests
			PROPERTYTYPE.STRSET, // [91]: KnownQuests
			PROPERTYTYPE.STRSET, // [92]: Dialog
			PROPERTYTYPE.STRSET, // [93]: Area

			// Integers (1 item)
			PROPERTYTYPE.UINT,   // [94]: XP
		};

		public static readonly Dictionary<string, CLASSARCHTYPE[]> ClassToArchTypeList = new Dictionary<string, CLASSARCHTYPE[]>()
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

			// MAGUS can be a fighter wizard or fighter sorceror. The Fighter/Sorceror is the Eldrich Scion and even gets to 
			// pick a sorcerors bloodline benefits. I suspect most playing Magus are going the Eldrich Scion route, which is
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

			{ "ARCANEENFORCER",    new CLASSARCHTYPE[] { CLASSARCHTYPE.Fighter,  CLASSARCHTYPE.Sorceror, CLASSARCHTYPE.Thief }     },

			{ "SORCERORCLASS",     new CLASSARCHTYPE[] { CLASSARCHTYPE.Sorceror, CLASSARCHTYPE.Sorceror,  CLASSARCHTYPE.Sorceror }  },
			{ "WARPRIESTCLASS",    new CLASSARCHTYPE[] { CLASSARCHTYPE.Cleric,   CLASSARCHTYPE.Fighter,   CLASSARCHTYPE.Thief }     },

			// WARPRIEST:CULT LEADER is probably the most interesting in that it provides sneak attack. So it is like a Cleric/Fighter/Theif

			{ "WITCHCLASS",        new CLASSARCHTYPE[] { CLASSARCHTYPE.Druid,    CLASSARCHTYPE.Druid,     CLASSARCHTYPE.Sorceror }  },

			// The most interesting Witch is the Stigmatized Witch, which has the Oracle's curse and 
			// casts like a sorceror (from resevior instead of memorized spells). 

			{ "WIZARDCLASS",       new CLASSARCHTYPE[] { CLASSARCHTYPE.Wizard,   CLASSARCHTYPE.Wizard,    CLASSARCHTYPE.Wizard }    },

			// Prestige
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

			// MYTHIC (Companions can't actually choose these). 
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


		public static readonly Dictionary<string, RACEMASK> StrToRaceMask = new Dictionary<string,  RACEMASK>()
		{

			// from Kingmaker.Visual.CharacterSystem.CharacterStudio.Race

			{ "",             RACEMASK.None    },
			{ "NONE",         RACEMASK.None    },
			{ "HUMAN",        RACEMASK.Human   },
			{ "ELF",          RACEMASK.Elf      },
			{ "HALFELF",      RACEMASK.HalfElf  },
			{ "HALF-ELF",     RACEMASK.HalfElf  },
			{ "HALF_ELF",     RACEMASK.HalfElf  },
			{ "HALF ELF",     RACEMASK.HalfElf  },
			{ "TIEFLING",     RACEMASK.Tiefling },
			{ "DWARF",        RACEMASK.Dwarf    },
			{ "CATFOLK",      RACEMASK.Catfolk  },
			{ "HALFLING",     RACEMASK.Halfling },
			{ "HALFORC",      RACEMASK.HalfOrc  },
			{ "HALF-ORC",     RACEMASK.HalfOrc  },
			{ "HALF_ORC",     RACEMASK.HalfOrc  },
			{ "HALF ORC",     RACEMASK.HalfOrc  },
			{ "GNOME",        RACEMASK.Gnome    },
			{ "GOBLIN",       RACEMASK.Goblin   },
			{ "AASIMAR",      RACEMASK.Aasimar  },
			{ "OREAD",        RACEMASK.Oread    },
			{ "DHAMPIR",      RACEMASK.Dhampir  },
			{ "KITSUNE",      RACEMASK.Kitsune  },
			{ "UNKNOWN",      RACEMASK.Unknown  },
			{ "ANY",          RACEMASK.Any      },

			// from Kingmaker.Visual.CharacterSystem.EquipmentEntitiesDictionary.Races

			{ "HUMANRACE",    RACEMASK.Human    },
			{ "ELFRACE",      RACEMASK.Elf      },
			{ "HALFELFRACE",  RACEMASK.HalfElf  },
			{ "TIEFLINGRACE", RACEMASK.Tiefling },
			{ "DWARFRACE",    RACEMASK.Dwarf    },
			{ "CATFOLKRACE",  RACEMASK.Catfolk  },
			{ "HALFLINGRACE", RACEMASK.Halfling },
			{ "HALFORCRACE",  RACEMASK.HalfOrc  },
			{ "GNOMERACE",    RACEMASK.Gnome    },
			{ "GOBLINRACE",   RACEMASK.Goblin   },
			{ "AASIMARRACE",  RACEMASK.Aasimar  },
			{ "OREADRACE",    RACEMASK.Oread    },
			{ "DHAMPIRRACE",  RACEMASK.Dhampir  },
			{ "KITSUNERACE",  RACEMASK.Kitsune  }
		};

		public static readonly Dictionary<string, ALIGNMENT> StrToAlignmentEnum = new Dictionary<string, ALIGNMENT>() {
			// From Kingmaker.Enums.Alignment
			{ "",                ALIGNMENT.None           },
			{ "NONE",            ALIGNMENT.None           },
			{ "CHAOTICGOOD",     ALIGNMENT.ChaoticGood    },
			{ "CHAOTIC-GOOD",    ALIGNMENT.ChaoticGood    },
			{ "CHAOTIC_GOOD",    ALIGNMENT.ChaoticGood    },
			{ "CHAOTIC GOOD",    ALIGNMENT.ChaoticGood    },
			{ "LAWFULGOOD",      ALIGNMENT.LawfulGood     },
			{ "LAWFUL-GOOD",     ALIGNMENT.LawfulGood     },
			{ "LAWFUL_GOOD",     ALIGNMENT.LawfulGood     },
			{ "LAWFUL GOOD",     ALIGNMENT.LawfulGood     },
			{ "LAWFULLGOOD",     ALIGNMENT.LawfulGood     },
			{ "LAWFULL-GOOD",    ALIGNMENT.LawfulGood     },
			{ "LAWFULL_GOOD",    ALIGNMENT.LawfulGood     },
			{ "LAWFULL GOOD",    ALIGNMENT.LawfulGood     },
			{ "NEUTRALGOOD",     ALIGNMENT.NeutralGood    },
			{ "NEUTRAL-GOOD",    ALIGNMENT.NeutralGood    },
			{ "NEUTRAL_GOOD",    ALIGNMENT.NeutralGood    },
			{ "NEUTRAL GOOD",    ALIGNMENT.NeutralGood    },
			{ "CHAOTICNEUTRAL",  ALIGNMENT.ChaoticNeutral },
			{ "CHAOTIC-NEUTRAL", ALIGNMENT.ChaoticNeutral },
			{ "CHAOTIC_NEUTRAL", ALIGNMENT.ChaoticNeutral },
			{ "CHAOTIC NEUTRAL", ALIGNMENT.ChaoticNeutral },
			{ "TRUENEUTRAL",     ALIGNMENT.TrueNeutral    },
			{ "TRUE-NEUTRAL",    ALIGNMENT.TrueNeutral    },
			{ "TRUE_NEUTRAL",    ALIGNMENT.TrueNeutral    },
			{ "TRUE NEUTRAL",    ALIGNMENT.TrueNeutral    },
			{ "LAWFULNEUTRAL",   ALIGNMENT.LawfulNeutral  },
			{ "LAWFUL-NEUTRAL",  ALIGNMENT.LawfulNeutral  },
			{ "LAWFUL_NEUTRAL",  ALIGNMENT.LawfulNeutral  },
			{ "LAWFUL NEUTRAL",  ALIGNMENT.LawfulNeutral  },
			{ "LAWFULLNEUTRAL",  ALIGNMENT.LawfulNeutral  },
			{ "LAWFULL-NEUTRAL", ALIGNMENT.LawfulNeutral  },
			{ "LAWFULL_NEUTRAL", ALIGNMENT.LawfulNeutral  },
			{ "LAWFULL NEUTRAL", ALIGNMENT.LawfulNeutral  },
			{ "NEUTRALEVIL",     ALIGNMENT.NeutralEvil    },
			{ "NEUTRAL-EVIL",    ALIGNMENT.NeutralEvil    },
			{ "NEUTRAL_EVIL",    ALIGNMENT.NeutralEvil    },
			{ "NEUTRAL EVIL",    ALIGNMENT.NeutralEvil    },
			{ "LAWFULEVIL",      ALIGNMENT.LawfulEvil     },
			{ "LAWFUL-EVIL",     ALIGNMENT.LawfulEvil     },
			{ "LAWFUL_EVIL",     ALIGNMENT.LawfulEvil     },
			{ "LAWFUL EVIL",     ALIGNMENT.LawfulEvil     },
			{ "LAWFULLEVIL",     ALIGNMENT.LawfulEvil     },
			{ "LAWFULL-EVIL",    ALIGNMENT.LawfulEvil     },
			{ "LAWFULL_EVIL",    ALIGNMENT.LawfulEvil     },
			{ "LAWFULL EVIL",    ALIGNMENT.LawfulEvil     },
			{ "CHAOTICEVIL",     ALIGNMENT.ChaoticEvil    },
			{ "CHAOTIC-EVIL",    ALIGNMENT.ChaoticEvil    },
			{ "CHAOTIC_EVIL",    ALIGNMENT.ChaoticEvil    },
			{ "CHAOTIC EVIL",    ALIGNMENT.ChaoticEvil    },
			{ "ANY",             ALIGNMENT.Any            }
		};

		public static readonly Dictionary<string, CLASSARCHTYPE> StrToClassArchTypeEnum = new Dictionary<string, CLASSARCHTYPE>()
		{
			{ "",                  CLASSARCHTYPE.None           },
			{ "NONE",              CLASSARCHTYPE.None           },
			{ "FIGHTER",           CLASSARCHTYPE.Fighter        },
			{ "MONK",              CLASSARCHTYPE.Monk           },
			{ "WIZARD",            CLASSARCHTYPE.Wizard         },
			{ "SORCEROR",          CLASSARCHTYPE.Sorceror       },
			{ "CLERIC",            CLASSARCHTYPE.Cleric         },
			{ "DRUID",             CLASSARCHTYPE.Druid          },
			{ "THIEF",             CLASSARCHTYPE.Thief          },
			{ "BARD",              CLASSARCHTYPE.Bard           },
			{ "ANY",               CLASSARCHTYPE.Any            }
		};

		public static readonly Dictionary<string, CLASSCATEGORY> StrToClassCategoryEnum = new Dictionary<string, CLASSCATEGORY>()
		{
			{ "",                  CLASSCATEGORY.None           },
			{ "NONE",              CLASSCATEGORY.None           },
			{ "WARRIOR",           CLASSCATEGORY.Warrior        },
			{ "MAGE",              CLASSCATEGORY.Mage           },
			{ "HEALER",            CLASSCATEGORY.Healer         },
			{ "ROGUE",             CLASSCATEGORY.Rogue          },
			{ "ANY",               CLASSCATEGORY.Any            }
		};

//		public static readonly Dictionary<string, CLASSMYTHIC> StrToClassMythicEnum = new Dictionary<string, CLASSMYTHIC>()
//		{
//			{ "",                 CLASSMYTHIC.None           },
//			{ "NONE",             CLASSMYTHIC.None           },
//			{ "AEON",             CLASSMYTHIC.Aeon           },
//          { "ANGEL",            CLASSMYTHIC.Angel          },
//          { "AZATA",            CLASSMYTHIC.Azata          },
//          { "DEMON",            CLASSMYTHIC.Demon          },
//          { "DEVIL",            CLASSMYTHIC.Devil          },
//          { "GOLDDRAGON",       CLASSMYTHIC.GoldDragon     },
//          { "GOLD_DRAGON",      CLASSMYTHIC.GoldDragon     },
//          { "GOLD DRAGON",      CLASSMYTHIC.GoldDragon     },
//          { "LEGEND",           CLASSMYTHIC.Legend         },
//          { "LICH",             CLASSMYTHIC.Lich           },
//          { "TRICKSTER",        CLASSMYTHIC.Trickster      },
//          { "SWARM-THAT-WALKS", CLASSMYTHIC.SwarmThatWalks },
//          { "SWARM_THAT_WALKS", CLASSMYTHIC.SwarmThatWalks },
//          { "SWARM THAT WALKS", CLASSMYTHIC.SwarmThatWalks },
//          { "SWARMTHATWALKS",   CLASSMYTHIC.SwarmThatWalks },
//			{ "ANY",              CLASSMYTHIC.Any            }
//		};


//		public static readonly Dictionary<string, HEALTHENUM> StrToHealthEnum = new Dictionary<string, HEALTHENUM>()
//		{
//			{ "",                 HEALTHENUM.None           },
//			{ "NONE",             HEALTHENUM.None           },
//			{ "HP_0_TO_25",       HEALTHENUM.HP_0_to_25     },
//			{ "HP 0 TO 25",       HEALTHENUM.HP_0_to_25     },
//			{ "HP 0-25",          HEALTHENUM.HP_0_to_25     },
//			{ "HP 0>25",          HEALTHENUM.HP_0_to_25     },
//			{ "HP_0-25",          HEALTHENUM.HP_0_to_25     },
//			{ "HP_0>25",          HEALTHENUM.HP_0_to_25     },
//			{ "0_TO_25",          HEALTHENUM.HP_0_to_25     },
//			{ "0 TO 25",          HEALTHENUM.HP_0_to_25     },
//			{ "0-25",             HEALTHENUM.HP_0_to_25     },
//			{ "0>25",             HEALTHENUM.HP_0_to_25     },

//			{ "HP_1_TO_25",       HEALTHENUM.HP_0_to_25     },
//			{ "HP 1 TO 25",       HEALTHENUM.HP_0_to_25     },
//			{ "HP 1-25",          HEALTHENUM.HP_0_to_25     },
//			{ "HP 1>25",          HEALTHENUM.HP_0_to_25     },
//			{ "HP_1-25",          HEALTHENUM.HP_0_to_25     },
//			{ "HP_1>25",          HEALTHENUM.HP_0_to_25     },
//			{ "1_TO_25",          HEALTHENUM.HP_0_to_25     },
//			{ "1 TO 25",          HEALTHENUM.HP_0_to_25     },
//			{ "1-25",             HEALTHENUM.HP_0_to_25     },
//			{ "1>25",             HEALTHENUM.HP_0_to_25     },

//          { "HP_25_TO_50",      HEALTHENUM.HP_25_to_50    },
//          { "HP 25 TO 50",      HEALTHENUM.HP_25_to_50    },
//          { "HP 25-50",         HEALTHENUM.HP_25_to_50    },
//          { "HP 25>50",         HEALTHENUM.HP_25_to_50    },
//          { "HP_25-50",         HEALTHENUM.HP_25_to_50    },
//          { "HP_25>50",         HEALTHENUM.HP_25_to_50    },
//          { "25_TO_50",         HEALTHENUM.HP_25_to_50    },
//          { "25 TO 50",         HEALTHENUM.HP_25_to_50    },
//          { "25-50",            HEALTHENUM.HP_25_to_50    },
//          { "25>50",            HEALTHENUM.HP_25_to_50    },


//          { "HP_50_TO_75",      HEALTHENUM.HP_50_to_75    },
//          { "HP 50 TO 75",      HEALTHENUM.HP_50_to_75    },
//          { "HP 50-75",         HEALTHENUM.HP_50_to_75    },
//          { "HP 50>75",         HEALTHENUM.HP_50_to_75    },
//          { "HP_50-75",         HEALTHENUM.HP_50_to_75    },
//          { "HP_50>75",         HEALTHENUM.HP_50_to_75    },
//          { "50_TO_75",         HEALTHENUM.HP_50_to_75    },
//          { "50 TO 75",         HEALTHENUM.HP_50_to_75    },
//          { "50-75",            HEALTHENUM.HP_50_to_75    },
//          { "50>75",            HEALTHENUM.HP_50_to_75    },

//          { "HP_75_TO_100",      HEALTHENUM.HP_75_to_100    },
//          { "HP 75 TO 100",      HEALTHENUM.HP_75_to_100    },
//          { "HP 75-100",         HEALTHENUM.HP_75_to_100    },
//          { "HP 75>100",         HEALTHENUM.HP_75_to_100    },
//          { "HP_75-100",         HEALTHENUM.HP_75_to_100    },
//          { "HP_75>100",         HEALTHENUM.HP_75_to_100    },
//          { "75_TO_100",         HEALTHENUM.HP_75_to_100    },
//          { "75 TO 100",         HEALTHENUM.HP_75_to_100    },
//          { "75-100",            HEALTHENUM.HP_75_to_100    },
//          { "75>100",            HEALTHENUM.HP_75_to_100    }
//		};

//		public static readonly Dictionary<string, GENDER> StrToGenderEnum = new Dictionary<string,  GENDER>()
//      {
//			{ "",                 GENDER.None           },
//			{ "NONE",             GENDER.None           },
//			{ "MALE",             GENDER.Male           },
//			{ "MAN",              GENDER.Male           },
//			{ "FEMALE",           GENDER.Female         },
//			{ "WOMAN",            GENDER.Female         },
//			{ "ANY",              GENDER.Any            }
 //     };

		public static readonly Dictionary<string, CIVILITY> StrToCivilityEnum = new Dictionary<string,  CIVILITY>()
        {
			{ "",                 CIVILITY.None         },
			{ "None",             CIVILITY.None         },
			{ "NEUTRAL",          CIVILITY.Neutral      },
			{ "LAWFUL",           CIVILITY.Lawful       },
			{ "LAW",              CIVILITY.Lawful       }, // For lazy people...
			{ "LAWFULL",          CIVILITY.Lawful       }, // For people who can't spell...
			{ "CHAOTIC",          CIVILITY.Chaotic      },
			{ "CHAOS",            CIVILITY.Chaotic      }, // For lazy people...
			{ "ANY",              CIVILITY.Any          }
		};

		public static readonly Dictionary<string, MORALITY> StrToMoralityEnum = new Dictionary<string,  MORALITY>()
        {
			{ "",                 MORALITY.None         },
			{ "None",             MORALITY.None         },
			{ "NEUTRAL",          MORALITY.Neutral      },
			{ "GOOD",             MORALITY.Good         },
			{ "EVIL",             MORALITY.Evil         },
			{ "ANY",              MORALITY.Any          }
		};

		public static readonly Dictionary<string, ACUITY> StrToAcuityEnum = new Dictionary<string,  ACUITY>()
        {
			{ "",                 ACUITY.None         },
			{ "None",             ACUITY.None         },
			{ "RESTED",           ACUITY.Rested       },
			{ "FATIGUED",         ACUITY.Fatigued     },
			{ "EXHAUSTED",        ACUITY.Exhausted    },
			{ "ANY",              ACUITY.Any          }
		};

		public static readonly Dictionary<string, NPCSIZE> StrToNPCSizeEnum = new Dictionary<string,  NPCSIZE>()
        {
			{ "",                 NPCSIZE.None       },
			{ "None",             NPCSIZE.None       },
			{ "FINE",             NPCSIZE.Fine       },
			{ "DIMINUTIVE",       NPCSIZE.Diminutive },
			{ "TINY",             NPCSIZE.Tiny       },
			{ "SMALL",            NPCSIZE.Small      },
			{ "MEDIUM",           NPCSIZE.Medium     },
			{ "LARGE",            NPCSIZE.Large      },
			{ "HUGE",             NPCSIZE.Huge       },
			{ "GARGANTUAN",       NPCSIZE.Gargantuan },
			{ "COLOSSAL",         NPCSIZE.Colossal   },
			{ "ANY",              NPCSIZE.Any        }
		};

		public static readonly Dictionary<string, PROPERTY> StrToPropertyEnum = new Dictionary<string, PROPERTY>()
		{
			{ "",                  PROPERTY.None           },
			{ "NONE",              PROPERTY.None           },
			{ "HP_BASE",           PROPERTY.HP_Base        },
			{ "HPBASE",            PROPERTY.HP_Base        },
			{ "HP_MAX",            PROPERTY.HP_Max         },
			{ "HPMAX",             PROPERTY.HP_Max         },
			{ "HP_MOD",            PROPERTY.HP_Mod         },
			{ "HPMOD",             PROPERTY.HP_Mod         },
			{ "HP",                PROPERTY.HP             },
			{ "HITPOINTS",         PROPERTY.HP             },
			{ "HIT_POINTS",        PROPERTY.HP             },
			{ "HP_PERCENT",        PROPERTY.HP_Percent     },
			{ "HPPERCENT",         PROPERTY.HP_Percent     },
			{ "HITPOINTPERCENT",   PROPERTY.HP_Percent     },
			{ "HIT_POINT_PERCENT", PROPERTY.HP_Percent     },
			{ "STR",               PROPERTY.Str            },
			{ "STRENGTH",          PROPERTY.Str            },
			{ "DEX",               PROPERTY.Dex            },
			{ "DEXTERITY",         PROPERTY.Dex            },
			{ "CON",               PROPERTY.Con            },
			{ "CONSTITUTION",      PROPERTY.Con            },
			{ "INT",               PROPERTY.Int            },
			{ "INTELLIGENCE",      PROPERTY.Int            },
			{ "WIS",               PROPERTY.Wis            },
			{ "WISDOM",            PROPERTY.Wis            },
			{ "CHR",               PROPERTY.Chr            },
			{ "CHARISMA",          PROPERTY.Chr            },
			{ "STR_BASE",          PROPERTY.Str_Base       },
			{ "STRBASE",           PROPERTY.Str_Base       },
			{ "STRENGTH_BASE",     PROPERTY.Str_Base       },
			{ "STRENGTHBASE",      PROPERTY.Str_Base       },
			{ "DEX_BASE",          PROPERTY.Dex_Base       },
			{ "DEXBASE",           PROPERTY.Dex_Base       },
			{ "DEXTERITY_BASE",    PROPERTY.Dex_Base       },
			{ "DEXTERITYBASE",     PROPERTY.Dex_Base       },
			{ "CON_BASE",          PROPERTY.Con_Base       },
			{ "CONBASE",           PROPERTY.Con_Base       },
			{ "CONSTITUTION_BASE", PROPERTY.Con_Base       },
			{ "CONSTITUTIONBASE",  PROPERTY.Con_Base       },
			{ "INT_BASE",          PROPERTY.Int_Base       },
			{ "INTBASE",           PROPERTY.Int_Base       },
			{ "INTELLIGENCE_BASE", PROPERTY.Int_Base       },
			{ "INTELLIGENCEBASE",  PROPERTY.Int_Base       },
			{ "WIS_BASE",          PROPERTY.Wis_Base       },
			{ "WISBASE",           PROPERTY.Wis_Base       },
			{ "WISDOM_BASE",       PROPERTY.Wis_Base       },
			{ "WISDOMBASE",        PROPERTY.Wis_Base       },
			{ "CHR_BASE",          PROPERTY.Chr_Base       },
			{ "CHRBASE",           PROPERTY.Chr_Base       },
			{ "CHARISMA_BASE",     PROPERTY.Chr_Base       },
			{ "CHARISMABASE",      PROPERTY.Chr_Base       },
			{ "FORT",              PROPERTY.Fort           },
			{ "FORTITUDE",         PROPERTY.Fort           },
			{ "WILL",              PROPERTY.Will           },
			{ "WILLPOWER",         PROPERTY.Will           },
			{ "REFLEX",            PROPERTY.Reflex         },
			{ "FORT_BASE",         PROPERTY.Fort_Base      },
			{ "FORTBASE",          PROPERTY.Fort_Base      },
			{ "FORTITUDE_BASE",    PROPERTY.Fort_Base      },
			{ "FORTITUDEBASE",     PROPERTY.Fort_Base      },
			{ "WILL_BASE",         PROPERTY.Will_Base      },
			{ "WILLBASE",          PROPERTY.Will_Base      },
			{ "WILLPOWER_BASE",    PROPERTY.Will_Base      },
			{ "WILLPOWERBASE",     PROPERTY.Will_Base      },
			{ "REFLEX_BASE",       PROPERTY.Reflex_Base    },
			{ "REFLEXBASE",        PROPERTY.Reflex_Base    },
			{ "MOBILITY",          PROPERTY.Mobility       },
			{ "ATHLETICS",         PROPERTY.Athletics      },
			{ "PERCEPTION",        PROPERTY.Perception     },
			{ "THIEVERY",          PROPERTY.Thievery       },
			{ "LORE_NATURE",       PROPERTY.LoreNature     },
			{ "LORENATURE",        PROPERTY.LoreNature     },
			{ "KNOWLEDGE_ARCANA",  PROPERTY.KnowledgeArcana },
			{ "KNOWLEDGE_ARCANE",  PROPERTY.KnowledgeArcana },
			{ "KNOWLEDGEARCANA",   PROPERTY.KnowledgeArcana },
			{ "KNOWLEDGEARCANE",   PROPERTY.KnowledgeArcana },
			{ "PERSUASION",        PROPERTY.Persuasion      },
			{ "STEALTH",           PROPERTY.Stealth         },
			{ "USE_MAGIC_DEVICE",  PROPERTY.UseMagicDevice  },
			{ "USEMAGICDEVICE",    PROPERTY.UseMagicDevice  },
			{ "USE_DEVICE",        PROPERTY.UseMagicDevice  },
			{ "USEDEVICE",         PROPERTY.UseMagicDevice  },
			{ "LORE_RELIGION",     PROPERTY.LoreReligion    },
			{ "LORERELIGION",      PROPERTY.LoreReligion    },
			{ "KNOWLEDGE_WORLD",   PROPERTY.KnowledgeWorld  },
			{ "KNOWLEDGEWORLD",    PROPERTY.KnowledgeWorld  },
			{ "MOBILITY_BASE",     PROPERTY.Mobility_Base   },
			{ "MOBILITYBASE",      PROPERTY.Mobility_Base   },
			{ "ATHLETICS_BASE",    PROPERTY.Athletics_Base  },
			{ "ATHLETICSBASE",     PROPERTY.Athletics_Base  },
			{ "PERCEPTION_BASE",   PROPERTY.Perception_Base },
			{ "PERCEPTIONBASE",    PROPERTY.Perception_Base },
			{ "THIEVERY_BASE",     PROPERTY.Thievery_Base   },
			{ "THIEVERYBASE",      PROPERTY.Thievery_Base   },
			{ "LORE_NATURE_BASE",  PROPERTY.LoreNature_Base },
			{ "LORENATURE_BASE",   PROPERTY.LoreNature_Base },
			{ "LORENATUREBASE",    PROPERTY.LoreNature_Base },
			{ "KNOWLEDGE_ARCANA_BASE", PROPERTY.KnowledgeArcana_Base },
			{ "KNOWLEDGE_ARCANE_BASE", PROPERTY.KnowledgeArcana_Base },
			{ "KNOWLEDGEARCANA_BASE",  PROPERTY.KnowledgeArcana_Base },
			{ "KNOWLEDGEARCANE_BASE",  PROPERTY.KnowledgeArcana_Base },
			{ "KNOWLEDGEARCANABASE",   PROPERTY.KnowledgeArcana_Base },
			{ "KNOWLEDGEARCANEBASE",   PROPERTY.KnowledgeArcana_Base },
			{ "PERSUASION_BASE",   PROPERTY.Persuasion_Base },
			{ "PERSUASIONBASE",    PROPERTY.Persuasion_Base },
			{ "STEALTH_BASE",      PROPERTY.Stealth_Base },
			{ "STEALTHBASE",       PROPERTY.Stealth_Base },
			{ "USE_MAGIC_DEVICE_BASE", PROPERTY.UseMagicDevice_Base },
			{ "USEMAGICDEVICE_BASE",   PROPERTY.UseMagicDevice_Base },
			{ "USEMAGICDEVICEBASE",    PROPERTY.UseMagicDevice_Base },
			{ "LORE_RELIGION_BASE",    PROPERTY.LoreReligion_Base },
			{ "LORERELIGION_BASE",     PROPERTY.LoreReligion_Base },
			{ "LORERELIGIONBASE",      PROPERTY.LoreReligion_Base },
			{ "KNOWLEDGE_WORLD_BASE",  PROPERTY.KnowledgeWorld_Base },
			{ "KNOWLEDGEWORLD_BASE",   PROPERTY.KnowledgeWorld_Base },
			{ "KNOWLEDGEWORLDBASE",    PROPERTY.KnowledgeWorld_Base },
			{ "AC",                    PROPERTY.AC },
			{ "ARMOR_CLASS",           PROPERTY.AC },
			{ "ARMORCLASS",            PROPERTY.AC },
			{ "AC_BASE",               PROPERTY.AC_Base },
			{ "ACBASE",                PROPERTY.AC_Base },
			{ "ARMOR_CLASS_BASE",      PROPERTY.AC_Base },
			{ "ARMORCLASS_BASE",       PROPERTY.AC_Base },
			{ "ARMORCLASSBASE",        PROPERTY.AC_Base },
			{ "AC_TOUCH",              PROPERTY.AC_TOUCH },
			{ "ACTOUCH",               PROPERTY.AC_TOUCH },
			{ "ARMOR_CLASS_TOUCH",     PROPERTY.AC_TOUCH },
			{ "ARMORCLASS_TOUCH",      PROPERTY.AC_TOUCH },
			{ "ARMORCLASSTOUCH",       PROPERTY.AC_TOUCH },
			{ "AC_FLAT_FOOTED",          PROPERTY.AC_FLATFOOTED },
			{ "AC_FLATFOOTED",           PROPERTY.AC_FLATFOOTED },
			{ "ACFLATFOOTED",            PROPERTY.AC_FLATFOOTED },
			{ "ARMOR_CLASS_FLAT_FOOTED", PROPERTY.AC_FLATFOOTED },
			{ "ARMOR_CLASS_FLATFOOTED",  PROPERTY.AC_FLATFOOTED },
			{ "ARMORCLASS_FLATFOOTED",   PROPERTY.AC_FLATFOOTED },
			{ "ARMORCLASSFLATFOOTED",    PROPERTY.AC_FLATFOOTED },
			{ "INITIATIVE",            PROPERTY.Initiative },
			{ "INITIATIVE_BASE",       PROPERTY.Initiative_Base },
			{ "INITIATIVEBASE",        PROPERTY.Initiative_Base },
			{ "SPEED",                 PROPERTY.Speed },
			{ "SPEED_BASE",            PROPERTY.Speed_Base },
			{ "SPEEDBASE",             PROPERTY.Speed_Base },
			{ "CORRUPTION",            PROPERTY.Corruption },
			{ "CORRUPTION_MAX",        PROPERTY.Corruption_Max },
			{ "CORRUPTIONMAX",         PROPERTY.Corruption_Max },
			{ "CORRUPTION_PERCENT",    PROPERTY.Corruption_Percent },
			{ "CORRUPTIONPERCENT",     PROPERTY.Corruption_Percent },
//			{ "PRIMARY_CLASS_ARCHTYPE_LEVEL",   PROPERTY.PrimaryClassArchTypeLevel },
//			{ "PRIMARYCLASSARCHTYPELEVEL",      PROPERTY.PrimaryClassArchTypeLevel },
//			{ "PRIMARY_ARCHTYPE_LEVEL",         PROPERTY.PrimaryClassArchTypeLevel },
//			{ "PRIMARYARCHTYPELEVEL",           PROPERTY.PrimaryClassArchTypeLevel },
//			{ "PRIMARY_LEVEL",                  PROPERTY.PrimaryClassArchTypeLevel },
//			{ "PRIMARYLEVEL",                   PROPERTY.PrimaryClassArchTypeLevel },
//			{ "SECONDARY_CLASS_ARCHTYPE_LEVEL", PROPERTY.SecondaryClassArchTypeLevel },
//			{ "SECONDARYCLASSARCHTYPELEVEL",    PROPERTY.SecondaryClassArchTypeLevel },
//			{ "SECONDARY_ARCHTYPE_LEVEL",       PROPERTY.SecondaryClassArchTypeLevel },
//			{ "SECONDARYARCHTYPELEVEL",         PROPERTY.SecondaryClassArchTypeLevel },
//			{ "SECONDARY_LEVEL",                PROPERTY.SecondaryClassArchTypeLevel },
//			{ "SECONDARYLEVEL",                 PROPERTY.SecondaryClassArchTypeLevel },
			{ "LEVEL_BASE",                     PROPERTY.Level_Base },
			{ "LEVELBASE",                      PROPERTY.Level_Base },
			{ "LEVEL",                          PROPERTY.Level },
			{ "INCOMBAT",                       PROPERTY.InCombat },
			{ "IN_COMBAT",                      PROPERTY.InCombat },
			{ "ISNAKED",                        PROPERTY.IsNaked },
			{ "IS_NAKED",                       PROPERTY.IsNaked },
			{ "USINGDEFAULTEQUIPMENT",          PROPERTY.UsingDefaultEquipment },
			{ "USING_DEFAULT_EQUIPMENT",        PROPERTY.UsingDefaultEquipment },			
			{ "PRIMARY_CLASS_CATEGORY",         PROPERTY.PrimaryClassCategory },
			{ "PRIMARYCLASS_CATEGORY",          PROPERTY.PrimaryClassCategory },
			{ "PRIMARYCLASSCATEGORY",           PROPERTY.PrimaryClassCategory },
			{ "PRIMARY_CATEGORY",               PROPERTY.PrimaryClassCategory },
			{ "PRIMARYCATEGORY",                PROPERTY.PrimaryClassCategory },
			{ "SECONDARY_CLASS_CATEGORY",       PROPERTY.SecondaryClassCategory },
			{ "SECONDARYCLASS_CATEGORY",        PROPERTY.SecondaryClassCategory },
			{ "SECONDARYCLASSCATEGORY",         PROPERTY.SecondaryClassCategory },
			{ "SECONDARY_CATEGORY",             PROPERTY.SecondaryClassCategory },
			{ "SECONDARYCATEGORY",              PROPERTY.SecondaryClassCategory },
			{ "PRIMARY_CLASS_ARCH_TYPE",        PROPERTY.PrimaryClassArchType },
			{ "PRIMARY_CLASS_ARCHTYPE",         PROPERTY.PrimaryClassArchType },
			{ "PRIMARYCLASS_ARCH_TYPE",         PROPERTY.PrimaryClassArchType },
			{ "PRIMARYCLASS_ARCHTYPE",          PROPERTY.PrimaryClassArchType },
			{ "PRIMARYCLASSARCHTYPE",           PROPERTY.PrimaryClassArchType },
			{ "PRIMARY_ARCH_TYPE",              PROPERTY.PrimaryClassArchType },
			{ "PRIMARY_ARCHTYPE",               PROPERTY.PrimaryClassArchType },
			{ "PRIMARYARCHTYPE",                PROPERTY.PrimaryClassArchType },
			{ "SECONDARY_CLASS_ARCH_TYPE",      PROPERTY.SecondaryClassArchType },
			{ "SECONDARY_CLASS_ARCHTYPE",       PROPERTY.SecondaryClassArchType },
			{ "SECONDARYCLASS_ARCHTYPE",        PROPERTY.SecondaryClassArchType },
			{ "SECONDARYCLASSARCHTYPE",         PROPERTY.SecondaryClassArchType },
			{ "SECONDARY_ARCH_TYPE",            PROPERTY.SecondaryClassArchType },
			{ "SECONDARY_ARCHTYPE",             PROPERTY.SecondaryClassArchType },
			{ "SECONDARYARCHTYPE",              PROPERTY.SecondaryClassArchType },
//			{ "MYTHIC_CLASS",                   PROPERTY.MythicClass },
//			{ "MYTHICCLASS",                    PROPERTY.MythicClass },
//			{ "ALL_CLASS_ARCH_TYPES",           PROPERTY.AllClassArchTypes },
//			{ "ALL_CLASS_ARCHTYPES",            PROPERTY.AllClassArchTypes },
//			{ "ALLCLASSARCHTYPES",              PROPERTY.AllClassArchTypes },
//			{ "ALLARCHTYPES",                   PROPERTY.AllClassArchTypes },
//			{ "HEALTH",                         PROPERTY.Health },
			{ "RACE",                           PROPERTY.Race },
//			{ "GENDER",                         PROPERTY.Gender },
			{ "CIVILITY",                       PROPERTY.Civility },
			{ "MORALITY",                       PROPERTY.Morality },
			{ "ALIGNMENT",                      PROPERTY.Alignment },
			{ "ACUITY",                         PROPERTY.Acuity },
			{ "SIZE",                           PROPERTY.Size },
			{ "SIZE_BASE",                      PROPERTY.Size_Base },
			{ "SIZEBASE",                       PROPERTY.Size_Base },
			{ "FACTS",                          PROPERTY.Facts },
			{ "BUFFS",                          PROPERTY.Buffs },
			{ "SHARED_STASH",                   PROPERTY.SharedStash },
			{ "SHAREDSTASH",                    PROPERTY.SharedStash },
			{ "STASH",                          PROPERTY.SharedStash },
			{ "INVENTORY",                      PROPERTY.Inventory },
			{ "EQUIPPEDARMOR",                  PROPERTY.EquippedArmor },
			{ "EQUIPTARMOR",                    PROPERTY.EquippedArmor },
			{ "EQUIPPED_ARMOR",                 PROPERTY.EquippedArmor },
			{ "EQUIPT_ARMOR",                   PROPERTY.EquippedArmor },
			{ "EQUIPPEDWEAPONS",                PROPERTY.EquippedWeapons },
			{ "EQUIPTWEAPONS",                  PROPERTY.EquippedWeapons },
			{ "EQUIPPED_WEAPONS",               PROPERTY.EquippedWeapons },
			{ "EQUIPT_WEAPONS",                 PROPERTY.EquippedWeapons },
			{ "EQUIPPEDRINGS",                  PROPERTY.EquippedRings },
			{ "EQUIPTRINGS",                    PROPERTY.EquippedRings },
			{ "EQUIPPED_RINGS",                 PROPERTY.EquippedRings },
			{ "EQUIPT_RINGS",                   PROPERTY.EquippedRings },
			{ "EQUIPPEDNECKLACES",              PROPERTY.EquippedNecklaces },
			{ "EQUIPTNECKLACES",                PROPERTY.EquippedNecklaces },
			{ "EQUIPPED_NECKLACES",             PROPERTY.EquippedNecklaces },
			{ "EQUIPT_NECKLACES",               PROPERTY.EquippedNecklaces },
			{ "EQUIPPED",                       PROPERTY.Equipped },
			{ "EQUIPT",                         PROPERTY.Equipped },
			{ "ACTIVEQUEST",                    PROPERTY.ActiveQuests },
			{ "ACTIVEQUESTS",                   PROPERTY.ActiveQuests },
			{ "ACTIVE_QUEST",                   PROPERTY.ActiveQuests },
			{ "ACTIVE_QUESTS",                  PROPERTY.ActiveQuests },
			{ "COMPLETEDQUESTS",                PROPERTY.CompletedQuests },
			{ "COMPLETED_QUESTS",               PROPERTY.CompletedQuests },
			{ "FAILEDQUESTS",                   PROPERTY.FailedQuests },
			{ "FAILED_QUESTS",                  PROPERTY.FailedQuests },
			{ "KNOWN_QUESTS",                   PROPERTY.KnownQuests },
			{ "KNOWNQUESTS",                    PROPERTY.KnownQuests },
			{ "DIALOG",                         PROPERTY.Dialog },
			{ "AREA",                           PROPERTY.Area },
			{ "XP",                             PROPERTY.XP },
			{ "EXPERIENCE",                     PROPERTY.XP },
			{ "EXPERIENCE_POINTS",              PROPERTY.XP },
			{ "EXPERIENCEPOINTS",               PROPERTY.XP }
		};



		public static readonly Dictionary<ALIGNMENT, Tuple<CIVILITY, MORALITY>> AlignmentToParts = 
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

		public static readonly Dictionary<Kingmaker.Enums.Size, NPCSIZE> KMSizeToNPCSize = new Dictionary<Kingmaker.Enums.Size, NPCSIZE>() {
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

        // Enum Typing Support
		public static PROPERTYTYPE GetPropType(PROPERTY prop)
        {
			if ( (((ushort)prop) & Meta.ushortRange) != 0) return Meta.PROPERTYTYPES[((int)prop)];
			if ( (((ushort)prop) & Meta.enumRange)   != 0) return Meta.PROPERTYTYPES[63 + (((int)prop) >> 6)];
			if ( (((ushort)prop) & Meta.setRange)    != 0) return Meta.PROPERTYTYPES[78 + (((int)prop )>> 10)];
			if ( (((ushort)prop) & Meta.uintRange)   != 0) return Meta.PROPERTYTYPES[93 + (((int)prop) >> 14)];
			return Meta.PROPERTYTYPES[0];
        }

	}
}