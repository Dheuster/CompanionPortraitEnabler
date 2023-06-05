using System;
using System.Collections.Generic; // Needed for List, Set, Dictionary, etc...

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{

	public class EnumConditionBuilder : IConditionInstanceBuilder
    {
		int index;
		PROPERTYTYPE pType;
		public EnumConditionBuilder(int valueIndex, PROPERTYTYPE pt)
        {
			this.index = valueIndex;
			this.pType = pt;
        }
		public IConditionInstance MatchesAny(params string[] args)
        {
			if (Log.traceEnabled)
            {
				return new TracingConditionEnumAny(index,getMask(args));
            }
			return new ConditionEnumAny(index,getMask(args));
        } 
		public IConditionInstance MatchesAll(params string[] args)
        {
			if (Log.traceEnabled)
            {
				return new TracingConditionEnumAll(index,getMask(args));
			}
			return new ConditionEnumAll(index,getMask(args));
        } 
		public IConditionInstance DoesNotMatchAny(params string[] args)
        {
			if (Log.traceEnabled)
            {
				return new TracingConditionEnumNotAny(index,getMask(args));
			}
			return new ConditionEnumNotAny(index,getMask(args));
        } 
		public IConditionInstance DoesNotMatchAll(params string[] args)
        {
			if (Log.traceEnabled)
            {
				return new TracingConditionEnumNotAll(index,getMask(args));
			}
			return new ConditionEnumNotAll(index,getMask(args));
        } 
		private ushort getMask(string[] args)
        {
			ushort mask = 0;
            switch(pType)
            {
                case PROPERTYTYPE.CLASSARCHTYPE:
				{ 
					foreach (string arg in args) {
						CLASSARCHTYPE myEnum = CLASSARCHTYPE.None;
						Meta.StrToClassArchTypeEnum.TryGetValue(arg.ToUpper(), out myEnum);
						mask |= (ushort)(myEnum);
					}
					return mask;
				}
				case PROPERTYTYPE.CLASSCATEGORY:
				{
					foreach (string arg in args) { 
						CLASSCATEGORY myEnum = CLASSCATEGORY.None;
						Meta.StrToClassCategoryEnum.TryGetValue(arg.ToUpper(), out myEnum);
						mask |= (ushort)(myEnum);
					}
					return mask;
				}
				//case PROPERTYTYPE.CLASSMYTHIC:
				//{
				//	foreach (string arg in args) { 
				//		CLASSMYTHIC myEnum = CLASSMYTHIC.None;
				//		Meta.StrToClassMythicEnum.TryGetValue(arg.ToUpper(), out myEnum);
				//		mask |= (ushort)(myEnum);
				//	}
				//	return mask;
				//}
				//case PROPERTYTYPE.HEALTHENUM:
				//{
				//	foreach (string arg in args) { 
				//		HEALTHENUM myEnum = HEALTHENUM.None;
				//		Meta.StrToHealthEnum.TryGetValue(arg.ToUpper(), out myEnum);
				//		mask |= (ushort)(myEnum);
				//	}
				//	return mask;
				//}
				case PROPERTYTYPE.RACEMASK:
				{
					foreach (string arg in args) { 
						RACEMASK myEnum = RACEMASK.None;
						Meta.StrToRaceMask.TryGetValue(arg.ToUpper(), out myEnum);
						mask |= (ushort)(myEnum);
					}
					return mask;
				}
				//case PROPERTYTYPE.GENDER:
				//{
				//	foreach (string arg in args) { 
				//		GENDER myEnum = GENDER.None;
				//		Meta.StrToGenderEnum.TryGetValue(arg.ToUpper(), out myEnum);
				//		mask |= (ushort)(myEnum);
				//	}
				//	return mask;
				//}
				case PROPERTYTYPE.CIVILITY:
				{
					foreach (string arg in args) { 
						CIVILITY myEnum = CIVILITY.None;
						Meta.StrToCivilityEnum.TryGetValue(arg.ToUpper(), out myEnum);
						mask |= (ushort)(myEnum);
					}
					return mask;
				}
				case PROPERTYTYPE.MORALITY:
				{
					foreach (string arg in args) { 
						MORALITY myEnum = MORALITY.None;
						Meta.StrToMoralityEnum.TryGetValue(arg.ToUpper(), out myEnum);
						mask |= (ushort)(myEnum);
					}
					return mask;
				}
				case PROPERTYTYPE.ALIGNMENT:
				{
					foreach (string arg in args) { 
						ALIGNMENT myEnum = ALIGNMENT.None;
						Meta.StrToAlignmentEnum.TryGetValue(arg.ToUpper(), out myEnum);
						mask |= (ushort)(myEnum);
					}
					return mask;
				}
				case PROPERTYTYPE.ACUITY:
				{
					foreach (string arg in args) { 
						ACUITY myEnum = ACUITY.None;
						Meta.StrToAcuityEnum.TryGetValue(arg.ToUpper(), out myEnum);
						mask |= (ushort)(myEnum);
					}
					return mask;
				}
				case PROPERTYTYPE.NPCSIZE:
				{
					foreach (string arg in args) { 
						NPCSIZE myEnum = NPCSIZE.None;
						Meta.StrToNPCSizeEnum.TryGetValue(arg.ToUpper(), out myEnum);
						mask |= (ushort)(myEnum);
					}
					return mask;
				}
				default: return 0;
			}
        }
		public IConditionInstance equals(uint value) { 
			if (Log.traceEnabled)
            {
				return new TracingConditionEnumEQ(index,(ushort)value); 
			}
			return new ConditionEnumEQ(index,(ushort)value); 
		}
		public IConditionInstance greaterThan(uint value) { 
			if (Log.traceEnabled)
            {
				return new TracingConditionEnumGT(index,(ushort)value); 
			}
			return new ConditionEnumGT(index,(ushort)value); 
		}
		public IConditionInstance lessThan(uint value) { 
			if (Log.traceEnabled)
            {
				return new TracingConditionEnumLT(index,(ushort)value); 
			}
			return new ConditionEnumLT(index,(ushort)value); 
		}
		public IConditionInstance greaterThanOrEqualTo(uint value) { 
			if (Log.traceEnabled)
            {
				return new TracingConditionEnumGTE(index,(ushort)value); 
			}
			return new ConditionEnumGTE(index,(ushort)value); 
		}
		public IConditionInstance lessThanOrEqualTo(uint value) { 
			if (Log.traceEnabled)
            {
				return new TracingConditionEnumLTE(index,(ushort)value); 
			}
			return new ConditionEnumLTE(index,(ushort)value); 
		}
		public IConditionInstance notEqualTo(uint value) { 
			if (Log.traceEnabled)
            {
				return new TracingConditionEnumNEQ(index,(ushort)value);
			}
			return new ConditionEnumNEQ(index,(ushort)value); 
		}
    }
}