using System;
// WOTR Specific:
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Relay
{
	public class OnAbilityEffectAppliedHandler : IApplyAbilityEffectHandler
	{	
		public void OnAbilityEffectApplied(AbilityExecutionContext context)
        {
            // ignored...
        }
		public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, TargetWrapper target)
        {
            CompanionPortraitEnablerMain.OnApplySpellAttempt(context, target);
        }
		public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, TargetWrapper target)
        {
            // ignored...
        }
	}
}