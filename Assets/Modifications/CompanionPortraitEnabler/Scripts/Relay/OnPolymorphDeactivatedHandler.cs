using System;
// WOTR Specific:
using Kingmaker;
using Kingmaker.PubSubSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Relay
{
	public class OnPolymorphDeactivatedHandler : IPolymorphDeactivatedHandler
	{		
		public void OnPolymorphDeactivated(UnitEntityData unitEntityData, Polymorph polymorph)
        {
			CompanionPortraitEnablerMain.OnPolymorphEnd(unitEntityData, polymorph);
        }
	}
}