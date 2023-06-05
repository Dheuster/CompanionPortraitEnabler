using System;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{
	public class ConditionUShortEQ : IConditionInstance { public ushort value; public int index;
		public ConditionUShortEQ(int i, ushort value) { this.index = i; this.value = value; }
		public bool evaluate(RuleContext rc) { return  rc.USHORT_VALUES[index] == this.value; }
	}

	public class TracingConditionUShortEQ : IConditionInstance { ConditionUShortEQ wrapped; 
	    public TracingConditionUShortEQ(int i, ushort value) { wrapped = new ConditionUShortEQ(i,value); }
		public bool evaluate(RuleContext rc) { 
			if (this.wrapped.evaluate(rc)) { 
	            Log.trace($"PASS: ruleContext.USHORT_VALUES[{wrapped.index}] ({rc.USHORT_VALUES[wrapped.index]}) >= value ({wrapped.value})?");
				return true;
            }
	        Log.trace($"FAIL: ruleContext.USHORT_VALUES[{wrapped.index}] ({rc.USHORT_VALUES[wrapped.index]}) >= value ({wrapped.value})?");
			return false;
		}
	}
}
