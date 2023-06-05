// WOTR Specific:
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UI.MVVM._VM.SaveLoad;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Relay
{

	public class OnSaveLoadRelayHandler : ISaveLoadUIHandler, IFullScreenUIHandler, IGameModeHandler
	{
		LoadState currLoadState = LoadState.None;
		SaveState currSaveState = SaveState.None;

		public enum SaveState
		{
			None,
			MainMenu,
			SaveMenuLoading,
			SaveMenuDisplayed,
			SaveMenuClosing,	
			SaveSingle,
			SaveMenuClosed
		}

		public enum LoadState
		{
			None,
			MainMenu,
			LoadMenuLoading,
			LoadMenuDisplayed,
			LoadMenuClosing,			
			LoadSingle,
			LoadMenuClosed,
		}

		public void OnGameModeStart(Kingmaker.GameModes.GameModeType gameMode) // IGameModeHandler
        {
			Log.trace($"TRACE: OnSaveLoadRelayHandler - OnGameModeStart [{gameMode}]");
			if (gameMode == Kingmaker.GameModes.GameModeType.EscMode){
				currSaveState = SaveState.MainMenu;
				currLoadState = LoadState.MainMenu;
				CompanionPortraitEnablerMain.OnSave(currSaveState);
				CompanionPortraitEnablerMain.OnLoad(currLoadState);
			}
        }

		public void OnGameModeStop(Kingmaker.GameModes.GameModeType gameMode) // IGameModeHandler
		{
			Log.trace($"TRACE: OnSaveLoadRelayHandler OnGameModeStop [{gameMode}]");
			if (gameMode == Kingmaker.GameModes.GameModeType.FullScreenUi)
            {
				if (currSaveState == SaveState.SaveMenuClosing)
                {
					currSaveState = SaveState.SaveMenuClosed;
					currLoadState = LoadState.None;
					CompanionPortraitEnablerMain.OnSave(currSaveState);
					return;
                }
				if (currLoadState == LoadState.LoadMenuClosing)
                {
					currLoadState = LoadState.LoadMenuClosed;
					currSaveState = SaveState.None;
					CompanionPortraitEnablerMain.OnLoad(currLoadState);
					return;
                }
			}
		}

		public void HandleOpenSaveLoad(SaveLoadMode mode, bool singleMode) { // ISaveLoadUIHandler
			Log.trace($"TRACE: OnSaveLoadRelayHandler - HandleOpenSaveLoad mode [{mode}] singleMode [{singleMode}]");
			if (singleMode) {
				if (mode == SaveLoadMode.Save)
				{ 
					currSaveState = SaveState.SaveSingle;
					currLoadState = LoadState.None;
					CompanionPortraitEnablerMain.OnSave(currSaveState);
					return;
				}
				currLoadState = LoadState.LoadSingle;
				currSaveState = SaveState.None;
				CompanionPortraitEnablerMain.OnLoad(currLoadState);
				return;
			}
			if (mode == SaveLoadMode.Save)
			{ 
				currSaveState = SaveState.SaveMenuLoading;
				currLoadState = LoadState.None;
				CompanionPortraitEnablerMain.OnSave(currSaveState);
				return;
			}
			currLoadState = LoadState.LoadMenuLoading;
			currSaveState = SaveState.None;
			CompanionPortraitEnablerMain.OnLoad(currLoadState);
		}

		public void HandleFullScreenUiChanged(bool displayed, Kingmaker.UI.FullScreenUITypes.FullScreenUIType fullScreenUIType) { // IFullScreenUIHandler
			Log.trace($"TRACE: OnSaveLoadRelayHandler - HandleFullScreenUiChanged state [{displayed}] FullScreenUIType [{fullScreenUIType}]");
			if (displayed)
			{ 
				if (fullScreenUIType == Kingmaker.UI.FullScreenUITypes.FullScreenUIType.SaveLoad)
				{
					if ((currSaveState == SaveState.SaveMenuLoading) || (currSaveState == SaveState.SaveSingle)) {
						currSaveState = SaveState.SaveMenuDisplayed;
						currLoadState = LoadState.None;
						CompanionPortraitEnablerMain.OnSave(currSaveState);
						return;
					}
					if ((currLoadState == LoadState.LoadMenuLoading) || (currLoadState == LoadState.LoadSingle)) {
						currLoadState = LoadState.LoadMenuDisplayed;
						currSaveState = SaveState.None;
						CompanionPortraitEnablerMain.OnLoad(currLoadState);
						return;
					}
					Log.trace($"TRACE: Unexpected State. Bailing... ");
				}
				return;
			}
			// No longer displayed... 
			if (fullScreenUIType == Kingmaker.UI.FullScreenUITypes.FullScreenUIType.SaveLoad)
			{
				if (currSaveState == SaveState.SaveMenuDisplayed) {
					currSaveState = SaveState.SaveMenuClosing;
					currLoadState = LoadState.None;
					CompanionPortraitEnablerMain.OnSave(currSaveState);
					return;
				}
				if (currLoadState == LoadState.LoadMenuDisplayed) {
					currLoadState = LoadState.LoadMenuClosing;
					currSaveState = SaveState.None;
					CompanionPortraitEnablerMain.OnLoad(currLoadState);
					return;
				}
				if ((currSaveState == SaveState.SaveMenuLoading) || (currSaveState == SaveState.SaveSingle)) {
					Log.trace($"TRACE: Unexpected State. Proceeding... ");
					currSaveState = SaveState.SaveMenuClosing;
					currLoadState = LoadState.None;
					CompanionPortraitEnablerMain.OnSave(currSaveState);
					return;
				}
				if ((currLoadState == LoadState.LoadMenuLoading) || (currLoadState == LoadState.LoadSingle)) {
					Log.trace($"TRACE: Unexpected State. Proceeding... ");
					currLoadState = LoadState.LoadMenuClosing;
					currSaveState = SaveState.None;
					CompanionPortraitEnablerMain.OnLoad(currLoadState);
					return;
				}
				Log.trace($"TRACE: Unexpected State. Bailing... ");
			}
        }
	}
}