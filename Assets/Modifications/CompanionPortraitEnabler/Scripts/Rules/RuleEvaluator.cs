using System;
using System.Collections.Generic; // Needed for List, Set, Dictionary, etc...

using OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules
{
    //#################################################################################
	// Basic Components of Rule Engine:
	// --------------------------------------------------------------------------------
	// 1) RuleContext :  A snapshot of the objects/environment that a rule can be
    //                   evaluated against. Exposes the nouns that a rule can be about.
	//
	//                  Example: Snapshot of current NPC.XP and inventory
	//
	// 2) Rule :         List of conditions against values that act as trigger points 
    //                   for the nouns exposed by the Rule Context. 
	//
	//                   Example: if (NPC.XP > 1234) && (inventory.contains("kitkat"))
	//
	// 3) RuleEvaluator: A streamlined/compiled component that takes a rule and a
	//                   rule context and quickly evaluates if the rule is true against
	//                   the given context.
	//
	//                   Example: RuleEvaluator re = new RuleEvaluator(rule)
	//                            if (re.evaluate(ruleContext)) then ...
	// ---------------------------------------------------------------------------------
	// Other Components:
	// ---------------------------------------------------------------------------------
	// 1) RuleFactory: Used typically at startup to deserialize rules and load them 
	//                 from disk. May also act as a cache for rules regarding some 
	//                 domain such as "any rule found in this directory". 
	//
	// 2) EventMonitor: Registers for events in order to keep a RuleContext up-to-date
	//                  and relavent for various tracked objects (NPCs). Typically
    //                  tracks when a rule-conext becomes stale and should be
    //                  re-evaluated again. We call this component NPCMonitor.
	//
	//#################################################################################

	public class RuleEvaluator
    {
		List<IConditionInstance> ruleData = new List<IConditionInstance>();

		public RuleEvaluator(List<IConditionInstance> ruleData)
        {
			this.ruleData = ruleData;
        }
		public bool evaluate(RuleContext rc)
        {
			foreach (IConditionInstance condition in ruleData)
            {
				if (!condition.evaluate(rc))
                {
					return false;
                }
            }
			return true;
        }

    	public class Builder
        {
		    List<IConditionInstance> ruleData = new List<IConditionInstance>();
		    List<IConditionInstance> disjunctionGroup = new List<IConditionInstance>();
		    protected Builder() { }
            public static Builder New()
            {
                return new Builder();
            }
		    public Builder And(IConditionInstance condition)
            {
			    if (disjunctionGroup.Count > 0)
                {
				    ruleData.Add(new DisjunctionGroup(disjunctionGroup));
				    disjunctionGroup = new List<IConditionInstance>();
                }
			    ruleData.Add(condition);
			    return this;
            } 
		    public Builder Or(IConditionInstance condition)
            {
			    disjunctionGroup.Add(condition);
			    return this;
            }
		    public RuleEvaluator build()
            {
			    if (disjunctionGroup.Count > 0)
                {
				    ruleData.Add(new DisjunctionGroup(disjunctionGroup));
                }
			    return new RuleEvaluator(ruleData);
            }
        }
    }
}