// WOTR Specific:
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;

namespace OwlcatModification.Modifications.NPCCustomPortraitEnabler.Relay
{
	public class OnAreaLoadRelayHandler : IAreaActivationHandler
	{		
		public void OnAreaActivated()
		{
			NPCCustomPortraitEnablerMain.OnAreaActivated();
		}
	}
}