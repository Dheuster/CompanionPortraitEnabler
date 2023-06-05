using System;
using System.Collections.Generic; // List
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Utility
{
    [Serializable]
    public class BodyPartsInfo
    {
        // computed* values are for internal use and tracking
        [JsonIgnore]
        public Kingmaker.Enums.Size computedSize { get; set; } = Kingmaker.Enums.Size.Medium;

        [JsonIgnore]
        public float computedAdditionalScale { get; set; } = 0.0f;

        [JsonIgnore]
        public UnityEngine.Vector3 computedScaleByVector { get; set; } = UnityEngine.Vector3.zero;

        [JsonIgnore]
        public bool  computedHasScale { get; set; } = false;

        [JsonIgnore]
        public Kingmaker.Blueprints.Classes.BlueprintRace computedRace { get; set; } = null;

        /* ---------------------------------------------------------------------
         * Appearance Options:
         * ---------------------------------------------------------------------
         * 
         * 1) standardAppearance : 
         * 
         * Item set for standard appearance. Equipped items will cover up or 
         * take the place of items in this appearance set.
         * 
         * 2) minimalAppearance : 
         * 
         * Only used/shown when the NPC has no items in the armor/shirt/robe 
         * slot.  Equipped items will cover up or take the place of items in 
         * this appearance set.
         * 
         * 3) defaultAppearance : 
         * 
         * When a user activates the "Show Default Equipment" flag on an NPC, 
         * item slot equipment disappears and the NPC displays the class default
         * appearance from character creation. This set overrides what the NPC 
         * looks like when that flag is on.
         * 
         * 4) size :
         * 
         *     This sets the size category of the NPC. IE: Small is like casting
         *     reduce person on someone and will impact reach, dexterity and 
         *     max strength. Do not recommend using the boundaries (Fine and 
         *     Colossal), as they leave no room for spells to change the size
         *     category. 
         * 
         *     Fine :       (Scale = 0.10) 
         *     Diminutive : (Scale = 0.25) 
         *     Tiny       : (Scale = 0.50)
         *     Small      : (Scale = 0.75)
         *     Medium     : (Scale = 1.00)
         *     Large      : (Scale = 1.50)
         *     Huge       : (Scale = 2.00)
         *     Gargantuan : (Scale = 2.50)
         *     Colossal   : (Scale = 3.00)
         *     
         * 5) additionalSizeScale :
         * 
         *    This non-negative float changes the BASE for calculating 
         *    size from 1.0 to another number idealling resulting in a 
         *    larger NPC. When an NPC is their original size, the math 
         *    is pretty simple... it is an additive. 
         *    
         *    IE: If this value is 0.25 and the NPC is their default size, 
         *    instead of using 1.0 for size calculations, it would use 1.25.
         *    
         *    Things however get complicated when an NPC is smaller or 
         *    larger than their default size. I will try my best to explain
         *    the algorith, but it doesn't make complete sense and I think 
         *    there is actually a bug.  
         *    
         *    Example: Ember is normally Medium. If we set additionalSize
         *    Scale to 0.25, that results in an ADDITIVE CHANGE:
         *    
         *    (1.0 + 0.25) = 1.25 <- This is her new size scale. 
         *    
         *    That makes perfect sense. But now what if ember is currently 
         *    size small. In that case, the algorithm once again adds 
         *    the delta:
         *    
         *    (1.0 + 0.25) = 1.25
         *    
         *    But then it multiplies it by an adjuster that starts at 1 
         *    and shrinks by 0.66 for each size less than the orginal 
         *    size. In this case, small is 1 less than Medium, so it 
         *    multiplies by 0.66:
         *    
         *    (1.0 + 0.25) * 0.66 = 0.825
         *    
         *    I believe it is TRYING to multiply by the base of the size, 
         *    but the algorithm writer didn't want to bother with looking 
         *    up what the size actually was so they used this shortcut. 
         *    However, this means there is an error factor which becomes 
         *    more/less significant based on many factors. For example, 
         *    what if we use scale .1 when Ember is small:
         *    
         *     1.1 * 0.66 = .726
         *     
         *     Small defaults to 0.75, so in this case, the additive
         *     value actually causes ember to shrink. If I go crazy
         *     and use 0.02:
         *     
         *     1.02 * 0.66 = 0.6732
         *     
         *     That is significantly less that 0.75. 
         *     
         * 6) desiredScale
         * 
         *    When present, this trumps size and additionalSizeScale.
         *    It basically tells the mod to figure out the minimum/best
         *    settings.  
         * 
         * ---------------------------------------------------------------------
         * Extra:
         * ---------------------------------------------------------------------
         * 
         * 1) equipItemsOnRecruit : 
         * 
         * When you first recruit an NPC, if they have no armor/robes, then the 
         * flag "Show Default Equipment" is automatically turned on for that 
         * NPC. You can avoid this by defining equipItemsOnRecruit.
         * 
         * When a newly recruited NPC has no armor/shirt (And only when that is 
         * true), this list of items is added to the NPC and equipped when you
         * recruit them. 
         * 
         * Example:
         * 
         *   "equipItemsOnRecruit" : [
         *     "598540b85673d984a8d45effcadda93f",
         *     "dfcc9dc3411234d4a902960a7e00d669"
         *   ]
         *   
         * The first item above is the ID for the PADDED ARMOR and the second
         * is the ID for the CLOAK OF DISGUISE. The items must be valid for 
         * the NPC's class or the equip will fail. You can discover item
         * IDs by looking at the Party_Info.json file that is generated in 
         * the Portrtaits/npcSnapshots folder. 
         * 
         * Common items to use:
         * 
         * LIGHT ARMOR:
         * ============
         * [1AC][PADDED ARMOR    ] (598540b85673d984a8d45effcadda93f)
         * [2AC][PADDED ARMOR +1 ] (c18cd5b70b611104393f06301e237060)
         * [2AC][LEATHER ARMOR   ] (9f76e9a3353e914479c5ddb4b4a82fb4)
         * [3AC][LEATHER ARMOR +1] (18c627302593ab142bb8219525e1aed1)
         * [3AC][STUD-LEATHER    ] (afbe88d27a0eb544583e00fa78ffb2c7)
         * [4AC][STUD-LEATHER +1 ] (af51a42724e27474c89d9d61392e09f4)
         * [4AC][CHAINSHIRT      ] (c65f6fc979d5556489b20e478189cbdd)
         * [5AC][CHAINSHIRT +1   ] (91bf657f26eb80f4ba05b0b8440b1e8c)
         * 
         * MEDIUM ARMOR:
         * =============
         * [4AC][HIDE ARMOR      ] (385be51e5706a55418384f70d8341371)  
         * [5AC][HIDE ARMOR +1   ] (45e86ae29df5f4b48a66da15fde62217)  
         * [5AC][SCALEMAIL       ] (d7963e1fcf260c148877afd3252dbc91)  
         * [6AC][SCALEMAIL +1    ] (c147f25768aa5094e8494013aea3786b)  
         * [6AC][BREASTPLATE     ] (9809987cc12d94545a64ff20e6fdb216)  
         * [7AC][BREASTPLATE +1  ] (5041415db3e6c394a8b2173c39fd4ec4)  
         * [6AC][CHAINMAIL       ] (02e9f83be5d1c5d4e927b5c44ed34840)  
         * [7AC][CHAINMAIL +1    ] (dd3834fe3f48182438b59fd99675fd6c) 
         * 
         * HEAVY ARMOR:
         * ============
         * [7AC][BANDED MAIL     ] (1638fa11f5af1814191cf6c05cdcf2b6)
         * [8AC][BANDED MAIL +1  ] (b277c8713472c3a4fb0f1e0ea6d6ed47)
         * [8AC][HALF-PLATE      ] (ed6bbd7ecd050c04690fe11d4c3b3f7d)   
         * [9AC][HALF-PLATE +1   ] (65de3fcad4c01ac40bc8567f67901b5b)   
         * [9AC][FULL PLATE      ] (559b0b6f194656c428c403a000ceee78)   
         * [10 ][FULL PLATE +1   ] (ba780a9c1e5b0304892bd2bc0c22fe4d)
         * 
         * NON-ARMOR:
         * ==========
         * [0AC][ALCHEMISTS SHIRT] (004e05369fd13e2428bf7eb84bcd38ec)
         * [0AC][SILVER ROBES    ] (...)
         * 
         * 2) Notes : 
         * 
         * If you only define one appearance set above, the other appearance 
         * sets will use the same items. If you define 2 apppearance sets and 
         * only 1 is missing, standardAppearance will be used unless it is the 
         * one missing inwhich case defaultAppearance will be used. 
         * 
         * ---------------------------------------------------------------------
         *     
         *     
        */

		public string     gender              { get; set; } 
		public string     raceName            { get; set; } 
        public string     size                { get; set; } = null;
        public string     additionalScale     { get; set; } = null;
        public string     desiredScale        { get; set; } = null;
        public string[]   equipItemsOnRecruit { get; set; }
		public BodyPart[] standardAppearance  { get; set; }
		public BodyPart[] minimalAppearance   { get; set; } 
        public BodyPart[] defaultAppearance   { get; set; }

        [Serializable]
        public class BodyPart
        {
            public BodyPart(string name, string assetId, int primaryColor, int secondaryColor)
            {
                this.name = name;
                this.assetId = assetId;
                this.primaryColor = primaryColor;
                this.secondaryColor = secondaryColor;
            }
		    public string   name { get; set; } 
		    public string   assetId { get; set; } 
            public int      primaryColor { get; set; } 
            public int      secondaryColor { get; set; } 
        }
    }
}
