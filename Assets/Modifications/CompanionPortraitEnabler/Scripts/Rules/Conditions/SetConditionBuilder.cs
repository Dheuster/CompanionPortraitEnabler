using System;
using System.Collections.Generic; // Needed for List, Set, Dictionary, etc...

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{
	public class SetConditionBuilder : IConditionInstanceBuilder
    {
		int index;
		public SetConditionBuilder(int valueIndex)
        {
			this.index = valueIndex;
        }
		public IConditionInstance MatchesAny(params string[] args)
        {				
			if (Log.traceEnabled)
            {
				return new TracingConditionSetAny(index,new HashSet<String>(args));
			}
			return new ConditionSetAny(index,new HashSet<String>(args));
        } 
		public IConditionInstance MatchesAll(params string[] args)
        {
			if (Log.traceEnabled)
            {
				return new TracingConditionSetAll(index,new HashSet<String>(args));
			}
			return new ConditionSetAll(index,new HashSet<String>(args));
        } 
		public IConditionInstance DoesNotMatchAny(params string[] args)
        {
			if (Log.traceEnabled)
            {
				return new TracingConditionSetNotAny(index,new HashSet<String>(args));
			}
			return new ConditionSetNotAny(index,new HashSet<String>(args));
        } 
		public IConditionInstance DoesNotMatchAll(params string[] args)
        {
			if (Log.traceEnabled)
            {
				return new TracingConditionSetNotAll(index,new HashSet<String>(args));
			}
			return new ConditionSetNotAll(index,new HashSet<String>(args));
        } 
		//---------- NOPE...-------------------------------
		public IConditionInstance equals(uint value) { return null; }
		public IConditionInstance greaterThan(uint value) { return null; }
		public IConditionInstance lessThan(uint value) { return null; }
		public IConditionInstance greaterThanOrEqualTo(uint value) { return null; }
		public IConditionInstance lessThanOrEqualTo(uint value) { return null; }
		public IConditionInstance notEqualTo(uint value) { return null; }
    }
}