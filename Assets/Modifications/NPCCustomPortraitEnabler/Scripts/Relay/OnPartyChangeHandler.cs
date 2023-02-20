// WOTR Specific:
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UI.MVVM._VM.SaveLoad;

namespace OwlcatModification.Modifications.NPCCustomPortraitEnabler.Relay
{
	public class OnPartyChangeHandler : IPartyHandler
	{
		public void HandleAddCompanion(UnitEntityData companion)
		{
			NPCCustomPortraitEnablerMain.OnCompanionAdded(companion);
		}
		public void HandleCompanionActivated(UnitEntityData companion) // : IPartyHandler
		{
			NPCCustomPortraitEnablerMain.OnCompanionActivated(companion);
		}
		public void HandleCompanionRemoved(UnitEntityData companion, bool stayInGame) // : IPartyHandler
		{
			NPCCustomPortraitEnablerMain.OnCompanionRemoved(companion, stayInGame);
		}
		public void HandleCapitalModeChanged()
		{
			// Ignore...
		}
	}
}
