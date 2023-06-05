// WOTR Specific:
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UI.MVVM._VM.SaveLoad;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Relay
{
	public class OnPartyChangeHandler : IPartyHandler
	{
		public void HandleAddCompanion(UnitEntityData companion)
		{
			CompanionPortraitEnablerMain.OnCompanionAdded(companion);
		}
		public void HandleCompanionActivated(UnitEntityData companion) // : IPartyHandler
		{
			CompanionPortraitEnablerMain.OnCompanionActivated(companion);
		}
		public void HandleCompanionRemoved(UnitEntityData companion, bool stayInGame) // : IPartyHandler
		{
			CompanionPortraitEnablerMain.OnCompanionRemoved(companion, stayInGame);
		}
		public void HandleCapitalModeChanged()
		{
			// Ignore...
		}
	}
}
