using System.Collections.Generic;
using Beam.Utilities;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

public class UMainMenuViewAdapter : MainMenuViewAdapterBase
{
	[SerializeField]
	private GameObject _mainMenuGameObject;

	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private UCanvasGroupViewAdapter _canvasGroup;

	[SerializeField]
	private GameObject _mainMenuControlGroup;

	[SerializeField]
	private GameObject _pauseMenuControlGroup;

	[SerializeField]
	private GameObject _playSubMenu;

	[SerializeField]
	private GameObject _playSubMenuControlGroup;

	[SerializeField]
	private GameObject _playOnlineSubMenu;

	[SerializeField]
	private GameObject _playOnlineSubMenuControlGroup;

	[SerializeField]
	private GameObject _playOnlineHostSubMenu;

	[SerializeField]
	private GameObject _playOnlineHostSubMenuControlGroup;

	[SerializeField]
	private TMPLabelButtonViewAdapter _playButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _playOnlineButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _playSubMenuBackButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _newGameButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _continueGameButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _continueCoopButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _playOnlineBackButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _playOnlineHostBackButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _playOnlineContinueButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _playOnlineNewGameButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _playOnlineHostButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _playOnlineJoinButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _changeSaveSlotButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _cartographerButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _leaderboardsButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _optionsButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _quitToDesktopButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _resumePauseButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _optionsPauseButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _quitGamePauseButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _quitToDesktopPauseButton;

	[SerializeField]
	private TMPLabelViewAdapter _versionNumberLabel;

	[SerializeField]
	private TMPLabelViewAdapter _versionTypeLabel;

	[SerializeField]
	private GameObject _userDetailsGroup;

	[SerializeField]
	private TMPLabelViewAdapter _userNameLabel;

	[SerializeField]
	private TMPLabelViewAdapter _userDetailsDayLabel;

	[SerializeField]
	private TMPLabelViewAdapter _userDetailsHoursLabel;

	[SerializeField]
	private GameObject _lobbyCodeGroup;

	[SerializeField]
	private TMPLabelViewAdapter _lobbyCodeLabel;

	[SerializeField]
	private UButtonViewAdapter _copyLobbyCodeButton;

	[SerializeField]
	private UTextureViewAdapter _backgroundTexture;

	[SerializeField]
	private TooltipController _tooltipController;

	private Dictionary<object, string> _tooltips;

	public override GameObject MainMenuGameObject => _mainMenuGameObject;

	public override ICanvasGroupViewAdapter CanvasGroup => _canvasGroup;

	public override GameObject MainMenuControlGroup => _mainMenuControlGroup;

	public override GameObject PauseMenuControlGroup => _pauseMenuControlGroup;

	public override GameObject PlaySubMenu => _playSubMenu;

	public override GameObject PlaySubMenuControlGroup => _playSubMenuControlGroup;

	public override GameObject PlayOnlineSubMenu => _playOnlineSubMenu;

	public override GameObject PlayOnlineSubMenuControlGroup => _playOnlineSubMenuControlGroup;

	public override GameObject PlayOnlineHostSubMenu => _playOnlineHostSubMenu;

	public override GameObject PlayOnlineHostSubMenuControlGroup => _playOnlineHostSubMenuControlGroup;

	public override ILabelButtonViewAdapter PlayButton => _playButton;

	public override ILabelButtonViewAdapter PlayOnlineButton => _playOnlineButton;

	public override ILabelButtonViewAdapter PlaySubMenuBackButton => _playSubMenuBackButton;

	public override ILabelButtonViewAdapter NewGameButton => _newGameButton;

	public override ILabelButtonViewAdapter ContinueButton => _continueGameButton;

	public override ILabelButtonViewAdapter ContinueCoopButton => _continueCoopButton;

	public override ILabelButtonViewAdapter PlayOnlineBackButton => _playOnlineBackButton;

	public override ILabelButtonViewAdapter PlayOnlineHostBackButton => _playOnlineHostBackButton;

	public override ILabelButtonViewAdapter PlayOnlineContinueButton => _playOnlineContinueButton;

	public override ILabelButtonViewAdapter PlayOnlineNewGameButton => _playOnlineNewGameButton;

	public override ILabelButtonViewAdapter PlayOnlineHostButton => _playOnlineHostButton;

	public override ILabelButtonViewAdapter PlayOnlineJoinButton => _playOnlineJoinButton;

	public override ILabelButtonViewAdapter ChangeSaveSlotButton => _changeSaveSlotButton;

	public override ILabelButtonViewAdapter CartographerButton => _cartographerButton;

	public override ILabelButtonViewAdapter LeaderboardsButton => _leaderboardsButton;

	public override ILabelButtonViewAdapter OptionsButton => _optionsButton;

	public override ILabelButtonViewAdapter QuitToDesktopButton => _quitToDesktopButton;

	public override ILabelButtonViewAdapter ResumePauseButton => _resumePauseButton;

	public override ILabelButtonViewAdapter OptionsPauseButton => _optionsPauseButton;

	public override ILabelButtonViewAdapter QuitGamePauseButton => _quitGamePauseButton;

	public override ILabelButtonViewAdapter QuitToDesktopPauseButton => _quitToDesktopPauseButton;

	public override ILabelViewAdapter VersionNumberLabel => _versionNumberLabel;

	public override ILabelViewAdapter VersionTypeLabel => _versionTypeLabel;

	public override GameObject UserDetailsGroup => _userDetailsGroup;

	public override ILabelViewAdapter UserNameLabel => _userNameLabel;

	public override ILabelViewAdapter UserDetailsDaysLabel => _userDetailsDayLabel;

	public override ILabelViewAdapter UserDetailsHoursLabel => _userDetailsHoursLabel;

	public override GameObject LobbyCodeGroup => _lobbyCodeGroup;

	public override ILabelViewAdapter LobbyCodeLabel => _lobbyCodeLabel;

	public override UButtonViewAdapter CopyLobbyCodeButton => _copyLobbyCodeButton;

	public override ITextureViewAdapter BackgroundTexture => _backgroundTexture;

	private void Awake()
	{
		Visible = _mainMenuGameObject.activeInHierarchy;
		_tooltips = new Dictionary<object, string>
		{
			{ _playButton, "MAIN_MENU_BUTTON_PLAY_DESC" },
			{ _playOnlineButton, "MAIN_MENU_BUTTON_PLAY_ONLINE_DESC" },
			{ _cartographerButton, "MAIN_MENU_BUTTON_CARTOGRAPHER_DESC" },
			{ _leaderboardsButton, "MAIN_MENU_BUTTON_LEADERBOARDS_DESC" },
			{ _continueGameButton, "MAIN_MENU_BUTTON_CONTINUE_DESC" },
			{ _continueCoopButton, "MAIN_MENU_BUTTON_CONTINUE_COOP_DESC" },
			{ _newGameButton, "MAIN_MENU_BUTTON_NEW_GAME_DESC" },
			{ _playOnlineContinueButton, "PLAY_ONLINE_MENU_BUTTON_CONTINUE_DESC" },
			{ _playOnlineHostButton, "PLAY_ONLINE_MENU_BUTTON_HOST_DESC" },
			{ _playOnlineJoinButton, "PLAY_ONLINE_MENU_BUTTON_JOIN_DESC" },
			{ _playOnlineNewGameButton, "PLAY_ONLINE_MENU_BUTTON_NEW_GAME_DESC" }
		};
		_tooltipController.ClearTooltip();
		_playButton.Selected += Widget_Selected;
		_playButton.MouseEnter += Widget_MouseEnter;
		_playButton.MouseLeave += Widget_MouseLeave;
		_playOnlineButton.Selected += Widget_Selected;
		_playOnlineButton.MouseEnter += Widget_MouseEnter;
		_playOnlineButton.MouseLeave += Widget_MouseLeave;
		_changeSaveSlotButton.Selected += Widget_Selected;
		_cartographerButton.Selected += Widget_Selected;
		_cartographerButton.MouseEnter += Widget_MouseEnter;
		_cartographerButton.MouseLeave += Widget_MouseLeave;
		_leaderboardsButton.Selected += Widget_Selected;
		_leaderboardsButton.MouseEnter += Widget_MouseEnter;
		_leaderboardsButton.MouseLeave += Widget_MouseLeave;
		_optionsButton.Selected += Widget_Selected;
		_quitToDesktopButton.Selected += Widget_Selected;
		_continueGameButton.Selected += Widget_Selected;
		_continueGameButton.MouseEnter += Widget_MouseEnter;
		_continueGameButton.MouseLeave += Widget_MouseLeave;
		_continueCoopButton.Selected += Widget_Selected;
		_continueCoopButton.MouseEnter += Widget_MouseEnter;
		_continueCoopButton.MouseLeave += Widget_MouseLeave;
		_newGameButton.Selected += Widget_Selected;
		_newGameButton.MouseEnter += Widget_MouseEnter;
		_newGameButton.MouseLeave += Widget_MouseLeave;
		_playSubMenuBackButton.Selected += Widget_Clear_Tooltip;
		_playOnlineContinueButton.Selected += Widget_Selected;
		_playOnlineContinueButton.MouseEnter += Widget_MouseEnter;
		_playOnlineContinueButton.MouseLeave += Widget_MouseLeave;
		_playOnlineHostButton.Selected += Widget_Selected;
		_playOnlineHostButton.MouseEnter += Widget_MouseEnter;
		_playOnlineHostButton.MouseLeave += Widget_MouseLeave;
		_playOnlineJoinButton.Selected += Widget_Selected;
		_playOnlineJoinButton.MouseEnter += Widget_MouseEnter;
		_playOnlineJoinButton.MouseLeave += Widget_MouseLeave;
		_playOnlineBackButton.Selected += Widget_Clear_Tooltip;
		_playOnlineNewGameButton.Selected += Widget_Selected;
		_playOnlineNewGameButton.MouseEnter += Widget_MouseEnter;
		_playOnlineNewGameButton.MouseLeave += Widget_MouseLeave;
		_playOnlineHostBackButton.Selected += Widget_Clear_Tooltip;
	}

	private void OnDestroy()
	{
		_playButton.Selected -= Widget_Selected;
		_playButton.MouseEnter -= Widget_MouseEnter;
		_playButton.MouseLeave -= Widget_MouseLeave;
		_playOnlineButton.Selected -= Widget_Selected;
		_playOnlineButton.MouseEnter -= Widget_MouseEnter;
		_playOnlineButton.MouseLeave -= Widget_MouseLeave;
		_cartographerButton.Selected -= Widget_Selected;
		_cartographerButton.MouseEnter -= Widget_MouseEnter;
		_cartographerButton.MouseLeave -= Widget_MouseLeave;
		_leaderboardsButton.Selected -= Widget_Selected;
		_leaderboardsButton.MouseEnter -= Widget_MouseEnter;
		_leaderboardsButton.MouseLeave -= Widget_MouseLeave;
		_optionsButton.Selected -= Widget_Selected;
		_quitToDesktopButton.Selected -= Widget_Selected;
		_continueGameButton.Selected -= Widget_Selected;
		_continueGameButton.MouseEnter -= Widget_MouseEnter;
		_continueGameButton.MouseLeave -= Widget_MouseLeave;
		_continueCoopButton.Selected -= Widget_Selected;
		_continueCoopButton.MouseEnter -= Widget_MouseEnter;
		_continueCoopButton.MouseLeave -= Widget_MouseLeave;
		_newGameButton.Selected -= Widget_Selected;
		_newGameButton.MouseEnter -= Widget_MouseEnter;
		_newGameButton.MouseLeave -= Widget_MouseLeave;
		_playSubMenuBackButton.Selected -= Widget_Clear_Tooltip;
		_playOnlineContinueButton.Selected -= Widget_Selected;
		_playOnlineContinueButton.MouseEnter -= Widget_MouseEnter;
		_playOnlineContinueButton.MouseLeave -= Widget_MouseLeave;
		_playOnlineHostButton.Selected -= Widget_Selected;
		_playOnlineHostButton.MouseEnter -= Widget_MouseEnter;
		_playOnlineHostButton.MouseLeave -= Widget_MouseLeave;
		_playOnlineJoinButton.Selected -= Widget_Selected;
		_playOnlineJoinButton.MouseEnter -= Widget_MouseEnter;
		_playOnlineJoinButton.MouseLeave -= Widget_MouseLeave;
		_playOnlineBackButton.Selected -= Widget_Clear_Tooltip;
		_playOnlineNewGameButton.Selected -= Widget_Selected;
		_playOnlineNewGameButton.MouseEnter -= Widget_MouseEnter;
		_playOnlineNewGameButton.MouseLeave -= Widget_MouseLeave;
		_playOnlineHostBackButton.Selected -= Widget_Clear_Tooltip;
	}

	private void Widget_Selected(object sender)
	{
		_tooltips.TryGetValue(sender, out var value);
		((Component)_tooltipController).transform.SetParent(((IButtonViewAdapter)sender).transform.parent, false);
		((Component)_tooltipController).transform.SetAsLastSibling();
		_tooltipController.SetTooltip(value);
	}

	private void Widget_MouseEnter(ISelectableViewAdapter sender)
	{
		Widget_Selected(sender);
	}

	private void Widget_MouseLeave(ISelectableViewAdapter sender)
	{
		_tooltipController.SetTooltip(string.Empty);
	}

	private void Widget_Clear_Tooltip(ISelectableViewAdapter sender)
	{
		_tooltipController.SetTooltip(string.Empty);
	}

	public override void Show()
	{
		_tooltipController.ClearTooltip();
		_mainMenuGameObject.SetActive(true);
		((Behaviour)_canvas).enabled = true;
		_canvasGroup.Interactable = true;
		base.Show();
	}

	public override void Hide()
	{
		_mainMenuGameObject.SetActive(false);
		((Behaviour)_canvas).enabled = false;
		_canvasGroup.Interactable = false;
		base.Hide();
	}

	public override void SetInput(Player input)
	{
		base.SetInput(input);
		if (Input != null)
		{
			RewiredUtilities.SetUserInterfaceFromInput(((Component)((Component)this).transform).GetComponentsInChildren<Selectable>(true), Input);
		}
	}

	public override void SetNavigation()
	{
		Transform element = (((Component)_playOnlineButton).gameObject.activeSelf ? ((Component)_playOnlineButton).transform : ((Component)_changeSaveSlotButton).transform);
		((Component)_playButton).transform.SetNavigation<Transform>(NavigationDirection.Down, element);
		((Component)_playOnlineButton).transform.SetNavigation<Transform>(NavigationDirection.Up, ((Component)_playButton).transform);
		((Component)_playOnlineButton).transform.SetNavigation<Transform>(NavigationDirection.Down, ((Component)_changeSaveSlotButton).transform);
		Transform element2 = (((Component)_playOnlineButton).gameObject.activeSelf ? ((Component)_playOnlineButton).transform : ((Component)_playButton).transform);
		((Component)_changeSaveSlotButton).transform.SetNavigation<Transform>(NavigationDirection.Up, element2);
		((Component)_changeSaveSlotButton).transform.SetNavigation<Transform>(NavigationDirection.Down, ((Component)_cartographerButton).transform);
		Transform element3 = (((Component)_leaderboardsButton).gameObject.activeSelf ? ((Component)_leaderboardsButton).transform : ((Component)_optionsButton).transform);
		((Component)_cartographerButton).transform.SetNavigation<Transform>(NavigationDirection.Up, ((Component)_changeSaveSlotButton).transform);
		((Component)_cartographerButton).transform.SetNavigation<Transform>(NavigationDirection.Down, element3);
		((Component)_leaderboardsButton).transform.SetNavigation<Transform>(NavigationDirection.Up, ((Component)_cartographerButton).transform);
		((Component)_leaderboardsButton).transform.SetNavigation<Transform>(NavigationDirection.Down, ((Component)_optionsButton).transform);
		Transform element4 = (((Component)_leaderboardsButton).gameObject.activeSelf ? ((Component)_leaderboardsButton).transform : ((Component)_cartographerButton).transform);
		((Component)_optionsButton).transform.SetNavigation<Transform>(NavigationDirection.Up, element4);
		((Component)_optionsButton).transform.SetNavigation<Transform>(NavigationDirection.Down, ((Component)_quitToDesktopButton).transform);
		((Component)_quitToDesktopButton).transform.SetNavigation<Transform>(NavigationDirection.Up, ((Component)_optionsButton).transform);
		Transform element5 = (((Component)_continueCoopButton).gameObject.activeSelf ? ((Component)_continueCoopButton).transform : ((Component)_newGameButton).transform);
		((Component)_continueGameButton).transform.SetNavigation<Transform>(NavigationDirection.Down, element5);
		((Component)_continueCoopButton).transform.SetNavigation<Transform>(NavigationDirection.Up, ((Component)_continueGameButton).transform);
		((Component)_continueCoopButton).transform.SetNavigation<Transform>(NavigationDirection.Down, ((Component)_newGameButton).transform);
		Transform element6 = (((Component)_continueCoopButton).gameObject.activeSelf ? ((Component)_continueCoopButton).transform : ((Component)_continueGameButton).transform);
		((Component)_newGameButton).transform.SetNavigation<Transform>(NavigationDirection.Up, element6);
		((Component)_newGameButton).transform.SetNavigation<Transform>(NavigationDirection.Down, ((Component)_playSubMenuBackButton).transform);
		((Component)_playSubMenuBackButton).transform.SetNavigation<Transform>(NavigationDirection.Up, ((Component)_newGameButton).transform);
		((Component)_playOnlineContinueButton).transform.SetNavigation<Transform>(NavigationDirection.Down, ((Component)_playOnlineNewGameButton).transform);
		((Component)_playOnlineNewGameButton).transform.SetNavigation<Transform>(NavigationDirection.Up, ((Component)_playOnlineContinueButton).transform);
		((Component)_playOnlineNewGameButton).transform.SetNavigation<Transform>(NavigationDirection.Down, ((Component)_playOnlineHostBackButton).transform);
		((Component)_playOnlineHostBackButton).transform.SetNavigation<Transform>(NavigationDirection.Up, ((Component)_playOnlineNewGameButton).transform);
		((Component)_playOnlineHostButton).transform.SetNavigation<Transform>(NavigationDirection.Down, ((Component)_playOnlineJoinButton).transform);
		((Component)_playOnlineJoinButton).transform.SetNavigation<Transform>(NavigationDirection.Up, ((Component)_playOnlineHostButton).transform);
		((Component)_playOnlineJoinButton).transform.SetNavigation<Transform>(NavigationDirection.Down, ((Component)_playOnlineBackButton).transform);
	}

	public override IButtonViewAdapter GetFirstButton()
	{
		return BeamTeam.GetInterface<IButtonViewAdapter>((Component)(object)Beam.Utilities.ExtensionMethods.GetFirstChild(GetActiveMenuControlsGroup(), active: true));
	}

	private Transform GetActiveMenuControlsGroup()
	{
		if (_playSubMenu.activeSelf)
		{
			return _playSubMenuControlGroup.transform;
		}
		if (_playOnlineSubMenu.activeSelf)
		{
			return _playOnlineSubMenuControlGroup.transform;
		}
		if (_playOnlineHostSubMenu.activeSelf)
		{
			return _playOnlineHostSubMenuControlGroup.transform;
		}
		if (_pauseMenuControlGroup.activeSelf)
		{
			return _pauseMenuControlGroup.transform;
		}
		return _mainMenuControlGroup.transform;
	}
}
