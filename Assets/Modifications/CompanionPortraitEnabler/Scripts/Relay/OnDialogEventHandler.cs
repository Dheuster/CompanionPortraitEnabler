using System;
// WOTR Specific:
using Kingmaker;
using Kingmaker.PubSubSystem;
using Kingmaker.DialogSystem.Blueprints;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Relay
{
	public class OnDialogEventHandler : IDialogFinishHandler, IDialogStartHandler
	{	
		public void HandleDialogStarted(BlueprintDialog dialogMeta)
        {
            CompanionPortraitEnablerMain.OnDialogStarted(dialogMeta);
		}

		public void HandleDialogFinished(BlueprintDialog dialogMeta, bool finishedWithoutCanceling) // IDialogFinishHandler
		{
            CompanionPortraitEnablerMain.OnDialogFinished(dialogMeta, finishedWithoutCanceling);
		}
	}
}