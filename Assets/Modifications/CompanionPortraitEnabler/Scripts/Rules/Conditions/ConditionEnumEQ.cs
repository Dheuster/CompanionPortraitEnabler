using System;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{
	public class ConditionEnumEQ : IConditionInstance { public ushort mask; public int index;
		public ConditionEnumEQ(int i, ushort mask) { this.index = i; this.mask = mask; }
		public bool evaluate(RuleContext rc) { return (rc.ENUM_VALUES[index] == mask); }
	}

	public class TracingConditionEnumEQ : IConditionInstance { ConditionEnumEQ wrapped;
		public TracingConditionEnumEQ(int i, ushort mask) { wrapped = new ConditionEnumEQ(i,mask); }
		public bool evaluate(RuleContext rc) { 
			if (this.wrapped.evaluate(rc)) { 
	            Log.trace($"PASS: ruleContext.ENUM_VALUES[{wrapped.index}] ({Convert.ToString(rc.ENUM_VALUES[wrapped.index], 2)}) == mask ({Convert.ToString(wrapped.mask, 2)}) != 0?");
				return true;
            }
	        Log.trace($"FAIL: ruleContext.ENUM_VALUES[{wrapped.index}] ({Convert.ToString(rc.ENUM_VALUES[wrapped.index], 2)}) == mask ({Convert.ToString(wrapped.mask, 2)}) != 0?");
			return false;
		}
	}
}