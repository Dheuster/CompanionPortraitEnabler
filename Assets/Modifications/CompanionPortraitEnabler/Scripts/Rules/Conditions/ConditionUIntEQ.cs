using System;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{
	public class ConditionUIntEQ : IConditionInstance { public uint value; public int index;
		public ConditionUIntEQ(int i, uint value) { this.index = i; this.value = value; }
		public bool evaluate(RuleContext rc) { return  rc.UINT_VALUES[index] == this.value; }
	}

	public class TracingConditionUIntEQ : IConditionInstance { ConditionUIntEQ wrapped; 
	    public TracingConditionUIntEQ(int i, uint value) { wrapped = new ConditionUIntEQ(i,value); }
		public bool evaluate(RuleContext rc) { 
			if (this.wrapped.evaluate(rc)) { 
	            Log.trace($"PASS: ruleContext.UINT_VALUES[{wrapped.index}] ({rc.UINT_VALUES[wrapped.index]}) == value ({wrapped.value})?");
				return true;
            }
	        Log.trace($"FAIL: ruleContext.UINT_VALUES[{wrapped.index}] ({rc.UINT_VALUES[wrapped.index]}) == value ({wrapped.value})?");
			return false;
		}
	}
}