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
	public class OnUnitLevelupHandler : ILevelUpCompleteUIHandler
	{
		public void HandleLevelUpComplete(UnitEntityData companion, bool isChargen)
		{
			CompanionPortraitEnablerMain.OnCompanionLevelUp(companion, isChargen);
		}
	}
}
