// WOTR Specific:
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Relay
{
	public class OnAreaLoadRelayHandler : IAreaActivationHandler
	{		
		public void OnAreaActivated()
		{
			CompanionPortraitEnablerMain.OnAreaActivated();
		}
	}
}