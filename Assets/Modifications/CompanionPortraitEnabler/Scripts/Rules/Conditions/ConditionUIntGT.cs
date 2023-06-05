using System;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{
	public class ConditionUIntGT : IConditionInstance { public uint value; public int index; 
	    public ConditionUIntGT(int i, uint value) { this.index = i; this.value = value; }
		public bool evaluate(RuleContext rc) { return  rc.UINT_VALUES[index] > this.value; }
	}

	public class TracingConditionUIntGT : IConditionInstance { ConditionUIntGT wrapped; 
	    public TracingConditionUIntGT(int i, uint value) { wrapped = new ConditionUIntGT(i,value); }
		public bool evaluate(RuleContext rc) { 
			if (this.wrapped.evaluate(rc)) { 
	            Log.trace($"PASS: ruleContext.UINT_VALUES[{wrapped.index}] ({rc.UINT_VALUES[wrapped.index]}) > value ({wrapped.value})?");
				return true;
            }
	        Log.trace($"FAIL: ruleContext.UINT_VALUES[{wrapped.index}] ({rc.UINT_VALUES[wrapped.index]}) > value ({wrapped.value})?");
			return false;
		}
	}
}
