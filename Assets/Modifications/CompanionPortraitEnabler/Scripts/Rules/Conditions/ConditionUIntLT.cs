using System;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{
	public class ConditionUIntLT : IConditionInstance { public uint value; public int index; 
	    public ConditionUIntLT(int i, uint value) { this.index = i; this.value = value; }
		public bool evaluate(RuleContext rc) { return rc.UINT_VALUES[index] < this.value; }
	}

	public class TracingConditionUIntLT : IConditionInstance { ConditionUIntLT wrapped; 
	    public TracingConditionUIntLT(int i, uint value) { wrapped = new ConditionUIntLT(i,value); }
		public bool evaluate(RuleContext rc) { 
			if (this.wrapped.evaluate(rc)) { 
	            Log.trace($"PASS: ruleContext.UINT_VALUES[{wrapped.index}] ({rc.UINT_VALUES[wrapped.index]}) < value ({wrapped.value})?");
				return true;
            }
	        Log.trace($"FAIL: ruleContext.UINT_VALUES[{wrapped.index}] ({rc.UINT_VALUES[wrapped.index]}) < value ({wrapped.value})?");
			return false;
		}
	}
}
