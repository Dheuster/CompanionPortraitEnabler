// WOTR Specific:
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UI.MVVM._VM.SaveLoad;

namespace OwlcatModification.Modifications.NPCCustomPortraitEnabler.Relay
{
	public class OnSaveLoadRelayHandler : ISaveLoadUIHandler
	{
		public void HandleOpenSaveLoad(SaveLoadMode mode, bool singleMode)
        {
			NPCCustomPortraitEnablerMain.OnSaveLoad(mode, singleMode);
		}
	}
}