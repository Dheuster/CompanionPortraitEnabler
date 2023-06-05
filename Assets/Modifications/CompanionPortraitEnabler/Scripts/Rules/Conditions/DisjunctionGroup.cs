using System;
using System.Collections.Generic; // Needed for List, Set, Dictionary, etc...

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{
   public class DisjunctionGroup : IConditionInstance
    {
        List<IConditionInstance> conditions;

        public DisjunctionGroup(List<IConditionInstance> conditions)
        {
			this.conditions = conditions;
        }
		public bool evaluate(RuleContext rc)
        {
			foreach (IConditionInstance condition in conditions)
            {
				if (condition.evaluate(rc))
                {
					return true;
                }
            }
			return false;
        }
    }
}
