using System;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{
	public class ConditionEnumAll : IConditionInstance {public  ushort mask; public int index;
		public ConditionEnumAll(int i, ushort mask) { this.index = i; this.mask = mask; }
		public bool evaluate(RuleContext rc) { return (mask == (rc.ENUM_VALUES[index] & mask)); }
	}

	public class TracingConditionEnumAll : IConditionInstance { ConditionEnumAll wrapped;
		public TracingConditionEnumAll(int i, ushort mask) { wrapped = new ConditionEnumAll(i,mask); }
		public bool evaluate(RuleContext rc) { 
			string maskStr = Convert.ToString(wrapped.mask, 2);
			if (this.wrapped.evaluate(rc)) { 
	            Log.trace($"PASS: mask ({maskStr}) == (ruleContext.ENUM_VALUES[{wrapped.index}] ({Convert.ToString(rc.ENUM_VALUES[wrapped.index], 2)}) & mask ({maskStr}))?");
				return true;
            }
	        Log.trace($"FAIL: mask ({maskStr}) == (ruleContext.ENUM_VALUES[{wrapped.index}] ({Convert.ToString(rc.ENUM_VALUES[wrapped.index], 2)}) & mask ({maskStr}))?");
			return false;
		}
	}
}
