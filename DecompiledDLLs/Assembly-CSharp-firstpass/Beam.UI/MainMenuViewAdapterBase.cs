using UnityEngine;

namespace Beam.UI;

public abstract class MainMenuViewAdapterBase : MenuViewAdapterBase
{
	public abstract GameObject MainMenuGameObject { get; }

	public abstract ICanvasGroupViewAdapter CanvasGroup { get; }

	public abstract GameObject MainMenuControlGroup { get; }

	public abstract GameObject PauseMenuControlGroup { get; }

	public abstract GameObject PlaySubMenu { get; }

	public abstract GameObject PlaySubMenuControlGroup { get; }

	public abstract GameObject PlayOnlineSubMenu { get; }

	public abstract GameObject PlayOnlineSubMenuControlGroup { get; }

	public abstract GameObject PlayOnlineHostSubMenu { get; }

	public abstract GameObject PlayOnlineHostSubMenuControlGroup { get; }

	public abstract ILabelButtonViewAdapter PlayButton { get; }

	public abstract ILabelButtonViewAdapter PlaySubMenuBackButton { get; }

	public abstract ILabelButtonViewAdapter PlayOnlineButton { get; }

	public abstract ILabelButtonViewAdapter PlayOnlineContinueButton { get; }

	public abstract ILabelButtonViewAdapter PlayOnlineNewGameButton { get; }

	public abstract ILabelButtonViewAdapter PlayOnlineHostButton { get; }

	public abstract ILabelButtonViewAdapter PlayOnlineJoinButton { get; }

	public abstract ILabelButtonViewAdapter PlayOnlineBackButton { get; }

	public abstract ILabelButtonViewAdapter PlayOnlineHostBackButton { get; }

	public abstract ILabelButtonViewAdapter NewGameButton { get; }

	public abstract ILabelButtonViewAdapter ContinueButton { get; }

	public abstract ILabelButtonViewAdapter ContinueCoopButton { get; }

	public abstract ILabelButtonViewAdapter ChangeSaveSlotButton { get; }

	public abstract ILabelButtonViewAdapter CartographerButton { get; }

	public abstract ILabelButtonViewAdapter LeaderboardsButton { get; }

	public abstract ILabelButtonViewAdapter OptionsButton { get; }

	public abstract ILabelButtonViewAdapter QuitToDesktopButton { get; }

	public abstract ILabelButtonViewAdapter ResumePauseButton { get; }

	public abstract ILabelButtonViewAdapter OptionsPauseButton { get; }

	public abstract ILabelButtonViewAdapter QuitGamePauseButton { get; }

	public abstract ILabelButtonViewAdapter QuitToDesktopPauseButton { get; }

	public abstract ILabelViewAdapter VersionNumberLabel { get; }

	public abstract ILabelViewAdapter VersionTypeLabel { get; }

	public abstract GameObject UserDetailsGroup { get; }

	public abstract ILabelViewAdapter UserNameLabel { get; }

	public abstract ILabelViewAdapter UserDetailsDaysLabel { get; }

	public abstract ILabelViewAdapter UserDetailsHoursLabel { get; }

	public abstract GameObject LobbyCodeGroup { get; }

	public abstract ILabelViewAdapter LobbyCodeLabel { get; }

	public abstract UButtonViewAdapter CopyLobbyCodeButton { get; }

	public abstract ITextureViewAdapter BackgroundTexture { get; }

	public abstract void SetNavigation();
}
