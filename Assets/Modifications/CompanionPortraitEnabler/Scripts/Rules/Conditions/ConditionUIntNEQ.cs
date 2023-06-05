using System;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{
	public class ConditionUIntNEQ : IConditionInstance { public uint value; public int index;
		public ConditionUIntNEQ(int i, uint value) { this.index = i; this.value = value; }
		public bool evaluate(RuleContext rc) { return rc.UINT_VALUES[index] != this.value; }
	}

	public class TracingConditionUIntNEQ : IConditionInstance { ConditionUIntNEQ wrapped; 
	    public TracingConditionUIntNEQ(int i, uint value) { wrapped = new ConditionUIntNEQ(i,value); }
		public bool evaluate(RuleContext rc) { 
			if (this.wrapped.evaluate(rc)) { 
	            Log.trace($"PASS: ruleContext.UINT_VALUES[{wrapped.index}] ({rc.UINT_VALUES[wrapped.index]}) != value ({wrapped.value})?");
				return true;
            }
	        Log.trace($"FAIL: ruleContext.UINT_VALUES[{wrapped.index}] ({rc.UINT_VALUES[wrapped.index]}) != value ({wrapped.value})?");
			return false;
		}
	}
}
