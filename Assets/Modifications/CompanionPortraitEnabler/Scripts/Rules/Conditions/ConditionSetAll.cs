using System;
using System.Collections.Generic;
using System.Linq; // String.Join, Set.ToList()

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{
	public class ConditionSetAll : IConditionInstance { public HashSet<String> value; public int index;
		public ConditionSetAll(int i,  HashSet<String> value) { this.index = i; this.value = value; }
		public bool evaluate(RuleContext rc) { return  value.IsSubsetOf(rc.STRSET_VALUES[index]); }
	}

	public class TracingConditionSetAll : IConditionInstance { ConditionSetAll wrapped;
		public TracingConditionSetAll(int i, HashSet<String> value) { wrapped = new ConditionSetAll(i,value); }
		public bool evaluate(RuleContext rc) { 
			int totalItems = 0;
			if (this.wrapped.evaluate(rc)) { 
				totalItems = rc.STRSET_VALUES[wrapped.index].Count;
				if (totalItems > 8) {
		            Log.trace($"PASS: value({string.Join(", ", wrapped.value.ToList())}).IsSubsetOf(ruleContext.STRSET_VALUES[{wrapped.index}] ({totalItems} items))?");
				} 
				else
                {
		            Log.trace($"PASS: value({string.Join(", ", wrapped.value.ToList())}).IsSubsetOf(ruleContext.STRSET_VALUES[{wrapped.index}] ({string.Join(", ", rc.STRSET_VALUES[wrapped.index].ToList())}))?");
                }
				return true;
            }
			if (totalItems > 8) {
		        Log.trace($"FAIL: value({string.Join(", ", wrapped.value.ToList())}).IsSubsetOf(ruleContext.STRSET_VALUES[{wrapped.index}] ({totalItems} items))?");
			} 
			else
            {
		        Log.trace($"FAIL: value({string.Join(", ", wrapped.value.ToList())}).IsSubsetOf(ruleContext.STRSET_VALUES[{wrapped.index}] ({string.Join(", ", rc.STRSET_VALUES[wrapped.index].ToList())}))?");
            }
			return false;
		}
	}
}
