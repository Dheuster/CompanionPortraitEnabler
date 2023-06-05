using System;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{
	public class ConditionEnumAny : IConditionInstance { public ushort mask; public int index;
		public ConditionEnumAny(int i, ushort mask) { this.index = i; this.mask = mask; }
		public bool evaluate(RuleContext rc) { return (0 != (rc.ENUM_VALUES[index] & mask)); }
	}

	public class TracingConditionEnumAny : IConditionInstance { ConditionEnumAny wrapped;
		public TracingConditionEnumAny(int i, ushort mask) { wrapped = new ConditionEnumAny(i,mask); }
		public bool evaluate(RuleContext rc) { 
			if (this.wrapped.evaluate(rc)) { 
	            Log.trace($"PASS: 0 != (ruleContext.ENUM_VALUES[{wrapped.index}] ({Convert.ToString(rc.ENUM_VALUES[wrapped.index], 2)}) & mask ({Convert.ToString(wrapped.mask, 2)}))?");
				return true;
            }
	        Log.trace($"FAIL: 0 != (ruleContext.ENUM_VALUES[{wrapped.index}] ({Convert.ToString(rc.ENUM_VALUES[wrapped.index], 2)}) & mask ({Convert.ToString(wrapped.mask, 2)}))?");
			return false;
		}
	}
}
