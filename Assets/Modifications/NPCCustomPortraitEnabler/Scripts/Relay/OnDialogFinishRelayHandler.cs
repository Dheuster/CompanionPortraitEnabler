// WOTR Specific:
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.DialogSystem.Blueprints;

namespace OwlcatModification.Modifications.NPCCustomPortraitEnabler.Relay
{
	public class OnDialogFinishRelayHandler : IDialogFinishHandler
	{
		public void HandleDialogFinished(BlueprintDialog dialog, bool success)
		{
			// NPCCustomPortraitEnablerMain.OnDialogFinished(dialog, success);
		}
	}
}