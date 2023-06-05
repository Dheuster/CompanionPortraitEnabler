using System;
using System.Collections.Generic; // Needed for List, Set, Dictionary, etc...
using System.Linq; // String.Join, Set.ToList()

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{
	public class ConditionSetAny : IConditionInstance { public HashSet<String> value; public int index;
		public ConditionSetAny(int i,  HashSet<String> value) { this.index = i; this.value = value; }
		public bool evaluate(RuleContext rc) { return  value.Overlaps(rc.STRSET_VALUES[index]); }
	}

	public class TracingConditionSetAny : IConditionInstance { ConditionSetAny wrapped;
		public TracingConditionSetAny(int i, HashSet<String> value) { wrapped = new ConditionSetAny(i,value); }
		public bool evaluate(RuleContext rc) { 
			int totalItems = 0;
			if (this.wrapped.evaluate(rc)) { 
				totalItems = rc.STRSET_VALUES[wrapped.index].Count;
				if (totalItems > 8) {
		            Log.trace($"PASS: value({string.Join(", ", wrapped.value.ToList())}).Overlaps(ruleContext.STRSET_VALUES[{wrapped.index}] ({totalItems} items))?");
				} 
				else
                {
		            Log.trace($"PASS: value({string.Join(", ", wrapped.value.ToList())}).Overlaps(ruleContext.STRSET_VALUES[{wrapped.index}] ({string.Join(", ", rc.STRSET_VALUES[wrapped.index].ToList())}))?");
                }
				return true;
            }
			if (totalItems > 8) {
		        Log.trace($"FAIL: value({string.Join(", ", wrapped.value.ToList())}).Overlaps(ruleContext.STRSET_VALUES[{wrapped.index}] ({totalItems} items))?");
			} 
			else
            {
		        Log.trace($"FAIL: value({string.Join(", ", wrapped.value.ToList())}).Overlaps(ruleContext.STRSET_VALUES[{wrapped.index}] ({string.Join(", ", rc.STRSET_VALUES[wrapped.index].ToList())}))?");
            }
			return false;
		}
	}
}