using System;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Rules.Conditions
{
	public interface IConditionInstanceBuilder
    {
		IConditionInstance equals(uint value);
		IConditionInstance greaterThan(uint value);
		IConditionInstance lessThan(uint value);
		IConditionInstance greaterThanOrEqualTo(uint value);
		IConditionInstance lessThanOrEqualTo(uint value);
		IConditionInstance notEqualTo(uint value);
		IConditionInstance MatchesAny(params string[] args);
		IConditionInstance MatchesAll(params string[] args);
		IConditionInstance DoesNotMatchAny(params string[] args);
		IConditionInstance DoesNotMatchAll(params string[] args);
    }
}