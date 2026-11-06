using System;
using System.Linq;
using Beam.AccountServices;
using Beam.Events;
using Beam.Language;
using Beam.Utilities;
using Beam.Utilities.Reflection;
using Funlabs;
using Photon.Bolt;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Beam.UI;

public class NewGameMenuPresenter : MonoBehaviour
{
	private static NewGameMenuPresenter _Instance;

	[SerializeField]
	private MainMenuPresenter _mainMenuPresenter;

	[SerializeField]
	private NewGameMenuViewAdapterBase _view;

	[SerializeField]
	private PlayerPreviewViewBase _p1PreviewView;

	[SerializeField]
	private PlayerPreviewViewBase _p2PreviewView;

	private IViewAdapter _parent;

	private bool _clientConnected;

	private bool _selectionChangedViaReplication;

	private bool _continueMPGame;

	private bool _suspend;

	private bool _lateSuspend;

	public Action<GameSettings> StartCallback;

	public Action StartMultiplayerContinueCallback;

	private DialogueBoxViewAdapter _notAllPlayersDialog;

	private GameMode _gameMode;

	private IButtonViewAdapter[] _navigableItems;

	public static NewGameMenuPresenter Instance
	{
		get
		{
			if ((Object)(object)_Instance == (Object)null)
			{
				_Instance = new NewGameMenuPresenter();
			}
			return _Instance;
		}
		set
		{
			_Instance = value;
		}
	}

	public bool Suspend
	{
		get
		{
			return _suspend;
		}
		set
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			_suspend = value;
			if (!MPExtensions.IsServer(_gameMode) || !_continueMPGame)
			{
				(_view as TMPNewGameMenuViewAdapter).ToggleHostButtons(!value);
			}
		}
	}

	public event Action ShowView;

	public event Action HideView;

	public void Initialize(IViewAdapter parent)
	{
		_parent = parent;
		if (!BeamTeam.IsNullOrDestroyed(_parent))
		{
			_parent.HideView += Hide;
		}
		_navigableItems = new IButtonViewAdapter[12]
		{
			_view.WorldOptionButton, _view.ModeOptionButton, _view.DifficultyOptionButton, _view.WildlifeOptionButton, _view.GenderOptionButton, _view.Gender2OptionButton, _view.StartingCrateOptionButton, _view.PermadeathOptionButton, _view.CopyCodeButton, _view.KickButton,
			_view.StartButton, _view.BackButton
		};
	}

	private void Awake()
	{
		PopulateControls();
		SetControlsSelections();
		SceneManager.sceneLoaded += SceneManager_SceneLoaded;
		ReInput.ControllerConnectedEvent += Rewired_OnControllerConnected;
		ReInput.ControllerDisconnectedEvent += Rewired_OnControllerDisconnected;
		_view.ShowView += View_ShowView;
		_view.HideView += View_HideView;
		_view.CopyCodeButton.Click += CopyCodeButton_Click;
		_view.KickButton.Click += KickButton_Click;
		_view.StartButton.Click += StartButton_Click;
		_view.BackButton.Click += BackButton_Click;
		_view.ModeOptionButton.SelectionChanged += ModeOptionButton_SelectionChanged;
		_view.GenderOptionButton.SelectionChanged += GenderOptionButton_SelectionChanged;
		_view.Gender2OptionButton.SelectionChanged += Gender2OptionButton_SelectionChanged;
		_view.DifficultyOptionButton.Click += GameDifficultyButton_Click;
		_view.WorldOptionButton.Click += WorldTypeSelection_Click;
		_view.GenderOptionButton.Click += CharacterSelectionButton_Click;
		_view.Gender2OptionButton.Click += Character2SelectionButton_Click;
		_view.PermadeathOptionButton.Click += PermadeathSelection_Click;
		_view.WildlifeOptionButton.Click += WildlifeStateButton_Click;
		_view.StartingCrateOptionButton.Click += StartingCrateButton_Click;
		WireButtonEvents(_view.ModeOptionButton);
		WireButtonEvents(_view.DifficultyOptionButton);
		WireButtonEvents(_view.WorldOptionButton);
		WireButtonEvents(_view.GenderOptionButton);
		WireButtonEvents(_view.Gender2OptionButton);
		WireButtonEvents(_view.PermadeathOptionButton);
		WireButtonEvents(_view.WildlifeOptionButton);
		WireButtonEvents(_view.StartingCrateOptionButton);
		if ((Object)(object)_p1PreviewView == (Object)null)
		{
			_p1PreviewView = NullPlayerPreviewView.Instance;
			Debug.Log((object)"");
		}
		if ((Object)(object)_p2PreviewView == (Object)null)
		{
			_p2PreviewView = NullPlayerPreviewView.Instance;
			Debug.Log((object)"");
		}
		_view.Hide();
	}

	private void OnDestroy()
	{
		if (!BeamTeam.IsNullOrDestroyed(_parent))
		{
			_parent.HideView -= Hide;
		}
		SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
		ReInput.ControllerConnectedEvent -= Rewired_OnControllerConnected;
		ReInput.ControllerDisconnectedEvent -= Rewired_OnControllerDisconnected;
		_view.ShowView -= View_ShowView;
		_view.HideView -= View_HideView;
		_view.CopyCodeButton.Click -= CopyCodeButton_Click;
		_view.KickButton.Click -= KickButton_Click;
		_view.StartButton.Click -= StartButton_Click;
		_view.BackButton.Click -= BackButton_Click;
		_view.ModeOptionButton.SelectionChanged -= ModeOptionButton_SelectionChanged;
		_view.GenderOptionButton.SelectionChanged -= GenderOptionButton_SelectionChanged;
		_view.Gender2OptionButton.SelectionChanged -= Gender2OptionButton_SelectionChanged;
		UnwireButtonEvents(_view.ModeOptionButton);
		UnwireButtonEvents(_view.DifficultyOptionButton);
		UnwireButtonEvents(_view.WorldOptionButton);
		UnwireButtonEvents(_view.GenderOptionButton);
		UnwireButtonEvents(_view.Gender2OptionButton);
		UnwireButtonEvents(_view.PermadeathOptionButton);
		UnwireButtonEvents(_view.WildlifeOptionButton);
		UnwireButtonEvents(_view.StartingCrateOptionButton);
		ShowView = null;
		HideView = null;
		StartCallback = null;
	}

	private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex != 1)
		{
			BeamTeam.Destroy<GameObject>(((Component)_view).gameObject);
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
		}
	}

	private void PopulateControls()
	{
		_view.WorldOptionButton.Initialize(Reflection.GetAttributes<DescriptionAttribute>(typeof(EGameWorldMode)));
		_view.ModeOptionButton.Initialize(Reflection.GetAttributes<DescriptionAttribute>(typeof(EGameMode)));
		_view.DifficultyOptionButton.Initialize(Reflection.GetAttributes<DescriptionAttribute>(typeof(EGameDifficultyMode)));
		_view.WildlifeOptionButton.Initialize(Reflection.GetAttributes<DescriptionAttribute>(typeof(EGameWildlifeMode)));
		_view.GenderOptionButton.Initialize(Reflection.GetAttributes<DescriptionAttribute>(typeof(EGameGenderMode)));
		_view.Gender2OptionButton.Initialize(Reflection.GetAttributes<DescriptionAttribute>(typeof(EGameGenderMode)));
		_view.PermadeathOptionButton.Initialize(Reflection.GetAttributes<DescriptionAttribute>(typeof(EGamePermadeathMode)));
		_view.StartingCrateOptionButton.Initialize(Reflection.GetAttributes<DescriptionAttribute>(typeof(EGameStartingCrate)));
	}

	private void SetControlsSelections()
	{
		_view.WorldOptionButton.SelectedIndex = Reflection.GetIndex(EGameWorldMode.Existing);
		_view.ModeOptionButton.SelectedIndex = Reflection.GetIndex(EGameMode.Single);
		_view.DifficultyOptionButton.SelectedIndex = Reflection.GetIndex(EGameDifficultyMode.Normal);
		_view.WildlifeOptionButton.SelectedIndex = Reflection.GetIndex(EGameWildlifeMode.Normal);
		_view.GenderOptionButton.SelectedIndex = Reflection.GetIndex(EGameGenderMode.Male);
		_view.Gender2OptionButton.SelectedIndex = Reflection.GetIndex(EGameGenderMode.Male);
		_view.PermadeathOptionButton.SelectedIndex = Reflection.GetIndex(EGamePermadeathMode.Off);
		_view.StartingCrateOptionButton.SelectedIndex = Reflection.GetIndex(EGameStartingCrate.Off);
	}

	private void LoadSavedControlsSelections()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		SaveManager.LoadSaveFile(_gameMode);
		_view.DifficultyOptionButton.SelectedIndex = Reflection.GetIndex(Options.CustomSettings.Difficulty);
		_view.GenderOptionButton.SelectedIndex = Reflection.GetIndex(Options.CustomSettings.Gender);
		_view.Gender2OptionButton.SelectedIndex = Reflection.GetIndex(Options.CustomSettings.Gender2);
		_view.WorldOptionButton.SelectedIndex = Reflection.GetIndex(Options.CustomSettings.World);
		_view.PermadeathOptionButton.SelectedIndex = Reflection.GetIndex(Options.CustomSettings.Permadeath);
		_view.WildlifeOptionButton.SelectedIndex = Reflection.GetIndex(Options.CustomSettings.Wildlife);
		_view.StartingCrateOptionButton.SelectedIndex = Reflection.GetIndex(Options.CustomSettings.StartingCrate);
	}

	private void WireWidgetEvents(IOptionWidgetViewAdapter widget)
	{
		widget.Selected += OptionWidget_Selected;
		widget.MouseEnter += OptionWidget_MouseEnter;
		widget.MouseLeave += OptionWidget_MouseLeave;
	}

	private void WireButtonEvents(IOptionButtonViewAdapter button)
	{
		button.SelectionChanged += OptionButton_SelectionChanged;
		WireWidgetEvents(button);
	}

	private void UnwireWidgetEvents(IOptionWidgetViewAdapter widget)
	{
		widget.Selected -= OptionWidget_Selected;
		widget.MouseEnter -= OptionWidget_MouseEnter;
		widget.MouseLeave -= OptionWidget_MouseLeave;
	}

	private void UnwireButtonEvents(IOptionButtonViewAdapter button)
	{
		button.SelectionChanged -= OptionButton_SelectionChanged;
		UnwireWidgetEvents(button);
	}

	private void UnlockCrateAchievement()
	{
		GAchievementsHandler.Get().UnlockAchievement(EAchievement.OUT_OF_THE_FRYING_PAN);
	}

	public void Show()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		Instance = this;
		_gameMode = Game.Mode;
		_clientConnected = false;
		_continueMPGame = _mainMenuPresenter.ContinuingMultiplayerGame;
		PlatformLocalizedText platformLocalizedText = new PlatformLocalizedText();
		if (MPExtensions.IsServer(_gameMode))
		{
			platformLocalizedText.SetTerm("HOST_GAME_MENU_LABEL_TITLE");
			PlayerRegistry.ServerPeer.Continuing = (_continueMPGame ? 1 : 0);
			if (_continueMPGame)
			{
				LoadSavedControlsSelections();
			}
			else
			{
				SetControlsSelections();
			}
			UpdateServerPeerOptions();
		}
		else if (MPExtensions.IsClient(_gameMode))
		{
			platformLocalizedText.SetTerm("PLAY_ONLINE_MENU_BUTTON_JOIN");
			_clientConnected = true;
		}
		else
		{
			platformLocalizedText.SetTerm("NEW_GAME_MENU_LABEL_TITLE");
			SetControlsSelections();
		}
		if (MPExtensions.IsMultiplayer(_gameMode))
		{
			EventManager.AddListener<PlayerConnectedEvent>(OnConnect);
			EventManager.AddListener<PlayerDisconnectedEvent>(OnDisconnect);
			PlayerRegistry.ServerPeer.AddListner(ServerPeerObserver);
			_view.LobbyNameLabel.Text = MultiplayerMng.GetSessionToken().Name;
			_view.RegionLabel.Text = MultiplayerMng.GetCloudRegion() ?? "";
			_view.CodeLabel.Text = MPExtensions.ToCode(Game.MultiplayerSessionName, MultiplayerMng.GetCloudRegionIndex());
		}
		ControllerFocus.Instance?.Subscribe(_view);
		_view.TitleLabel.Text = platformLocalizedText.GetLocalizedText();
		_view.Show();
		UpdateUILayout(selectFirstButton: true);
	}

	private void UpdateUILayout(bool selectFirstButton)
	{
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		if (_view.Visible)
		{
			IButtonViewAdapter[] navigableItems = _navigableItems;
			foreach (IButtonViewAdapter obj in navigableItems)
			{
				((Component)obj.transform).gameObject.SetActive(true);
				obj.Interactable = true;
			}
			_view.LobbyDetails.SetActive(IsLobbyEnabled());
			((Component)_view.StartingCrateOptionButton.transform).gameObject.SetActive(IsStartingCrateEnabled());
			((Component)_view.ModeOptionButton.transform).gameObject.SetActive(IsModeEnabled());
			((Component)_view.CopyCodeButton.transform).gameObject.SetActive(IsCopyCodeEnabled());
			((Component)_view.KickButton.transform).gameObject.SetActive(IsKickButonEnabled());
			((Component)_view.StartButton.transform).gameObject.SetActive(IsStartButtonEnabled());
			((Component)_view.CodeLabel.transform.parent).gameObject.SetActive(IsCodeLabelEnabled());
			bool interactable = MPExtensions.IsMaster(_gameMode) && !_continueMPGame;
			_view.WorldOptionButton.Interactable = interactable;
			if (MPExtensions.IsMultiplayer(_gameMode))
			{
				_view.WorldOptionButton.Interactable = false;
			}
			_view.ModeOptionButton.Interactable = interactable;
			_view.DifficultyOptionButton.Interactable = interactable;
			_view.WildlifeOptionButton.Interactable = interactable;
			_view.StartingCrateOptionButton.Interactable = interactable;
			_view.PermadeathOptionButton.Interactable = interactable;
			UpdateGenderUI(updateNavigation: false);
			_navigableItems.SetVerticalNaviationOrder();
			if (selectFirstButton)
			{
				_navigableItems.GetActive(0, 1)?.Select();
			}
		}
	}

	private void UpdateGenderUI(bool updateNavigation = true)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		if (_view.Visible)
		{
			EGameGenderMode eGameGenderMode = (EGameGenderMode)(MPExtensions.IsClient(_gameMode) ? _view.Gender2OptionButton.SelectedIndex : _view.GenderOptionButton.SelectedIndex);
			EGameGenderMode eGameGenderMode2 = (EGameGenderMode)(MPExtensions.IsClient(_gameMode) ? _view.GenderOptionButton.SelectedIndex : _view.Gender2OptionButton.SelectedIndex);
			PlayerModelPreviewFlags playerModelPreviewFlags = PlayerModelPreviewFlags.None;
			playerModelPreviewFlags |= PlayerModelPreviewFlags.Connected;
			playerModelPreviewFlags = (PlayerModelPreviewFlags)((int)playerModelPreviewFlags | ((eGameGenderMode == EGameGenderMode.Male) ? 1 : 2));
			PlayerModelPreviewFlags playerModelPreviewFlags2 = PlayerModelPreviewFlags.None;
			playerModelPreviewFlags2 = (PlayerModelPreviewFlags)((int)playerModelPreviewFlags2 | ((RewiredUtilities.ConnectedPlayers.Count() >= 2 || _clientConnected) ? 4 : 8));
			playerModelPreviewFlags2 = (PlayerModelPreviewFlags)((int)playerModelPreviewFlags2 | ((eGameGenderMode2 == EGameGenderMode.Male) ? 1 : 2));
			_p1PreviewView.SetState(playerModelPreviewFlags);
			_p1PreviewView.Show();
			_p2PreviewView.SetState(playerModelPreviewFlags2);
			EGameMode modeFromView = GetModeFromView();
			if ((!MPExtensions.IsMultiplayer(_gameMode) && modeFromView == EGameMode.Cooperative) || (MPExtensions.IsMultiplayer(_gameMode) && _clientConnected))
			{
				_view.Gender2OptionButton.Show();
				_p2PreviewView.Show();
			}
			else
			{
				_p2PreviewView.Hide();
				_view.Gender2OptionButton.Hide();
			}
			_view.GenderOptionButton.Interactable = IsGenderInteractable();
			_view.Gender2OptionButton.Interactable = IsGender2Interactable();
			if (updateNavigation)
			{
				_navigableItems.SetVerticalNaviationOrder();
			}
		}
	}

	private bool IsLobbyEnabled()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return MPExtensions.IsMultiplayer(_gameMode);
	}

	private bool IsStartingCrateEnabled()
	{
		return GAchievementsHandler.Get().IsAchievementUnlocked(EAchievement.OUT_OF_THE_FRYING_PAN);
	}

	private bool IsModeEnabled()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return !MPExtensions.IsMultiplayer(_gameMode);
	}

	private bool IsCopyCodeEnabled()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return MPExtensions.IsServer(_gameMode);
	}

	private bool IsKickButonEnabled()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(_gameMode))
		{
			return _clientConnected;
		}
		return false;
	}

	private bool IsStartButtonEnabled()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return MPExtensions.IsMaster(_gameMode);
	}

	private bool IsCodeLabelEnabled()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return MPExtensions.IsServer(_gameMode);
	}

	private bool IsGenderInteractable()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(_gameMode))
		{
			if (!_continueMPGame)
			{
				return MPExtensions.IsServer(_gameMode);
			}
			return false;
		}
		return true;
	}

	private bool IsGender2Interactable()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(_gameMode))
		{
			if (!_continueMPGame)
			{
				return MPExtensions.IsClient(_gameMode);
			}
			return false;
		}
		return true;
	}

	private EGameMode GetModeFromView()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsMultiplayer(_gameMode))
		{
			return (EGameMode)_view.ModeOptionButton.SelectedIndex;
		}
		return EGameMode.Single;
	}

	public void Hide()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		ControllerFocus.Instance?.Unsubscribe(_view);
		((Component)_view.ModeOptionButton.transform).gameObject.SetActive(true);
		((Component)_view.StartButton.transform).gameObject.SetActive(true);
		_p1PreviewView.Hide();
		_p2PreviewView.Hide();
		_view.Hide();
		if (MPExtensions.IsMultiplayer(_gameMode))
		{
			EventManager.RemoveListener<PlayerConnectedEvent>(OnConnect);
			EventManager.RemoveListener<PlayerDisconnectedEvent>(OnDisconnect);
			PlayerRegistry.ServerPeer?.RemoveListener(ServerPeerObserver);
		}
	}

	private void View_ShowView()
	{
		ShowView?.Invoke();
	}

	private void View_HideView()
	{
		HideView?.Invoke();
	}

	private void CheckInput()
	{
		if (!DialogueBox.Notifying)
		{
			Player player = ReInput.players.GetPlayer(0);
			if ((player.GetButtonDown(39) || player.GetButtonDown(31)) && !_lateSuspend && _view.Visible)
			{
				BackButton_Click(null);
			}
		}
	}

	private void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		CheckInput();
		if (MPExtensions.IsMultiplayer(_gameMode))
		{
			bool flag = false;
			foreach (BoltConnection connection in BoltNetwork.Connections)
			{
				int num = (int)(connection.PingAliased * 1000f);
				_view.PingLabel.Text = $"{num} ms";
				flag = true;
			}
			if (!flag)
			{
				_view.PingLabel.Text = "---";
			}
		}
		if (MPExtensions.IsServer(_gameMode) && Input.GetKey((KeyCode)99))
		{
			CopyCodeButton_Click(null);
		}
		_lateSuspend = Suspend;
	}

	private void Rewired_OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		UpdateGenderUI();
	}

	private void Rewired_OnControllerDisconnected(ControllerStatusChangedEventArgs args)
	{
		UpdateGenderUI();
	}

	public void OnConnect(PlayerConnectedEvent @event)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (_view.Visible && !_clientConnected && MPExtensions.IsServer(_gameMode) && @event.PlayerId == 1)
		{
			_clientConnected = true;
			UpdateUILayout(selectFirstButton: false);
		}
	}

	private void OnDisconnect(PlayerDisconnectedEvent @event)
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		if (!_view.Visible)
		{
			return;
		}
		string title = string.Empty;
		string content = string.Empty;
		if ((Object)(object)_notAllPlayersDialog != (Object)null)
		{
			_notAllPlayersDialog.Destroy();
		}
		if (@event.TryGetDisconnectDialog(out title, out content))
		{
			EventManager.RemoveListener<PlayerConnectedEvent>(OnConnect);
			EventManager.RemoveListener<PlayerDisconnectedEvent>(OnDisconnect);
			Player player = ReInput.players.GetPlayer(0);
			RewiredUtilities.SetAndCacheMapsEnabled(player, state: false, 0);
			RewiredUtilities.SetAndCacheMapsEnabled(player, state: true, 16);
			RewiredUtilities.SetCursorEnabled(player, enabled: true);
			DialogueBox.Show(player, title, content, DialogueButton.Ok, DialogueIcon.Warning).Accept += () =>
			{
				Hide();
			};
		}
		else
		{
			if (MPExtensions.IsServer(_gameMode) && @event.PlayerId == 1)
			{
				_clientConnected = false;
				UpdateUILayout(selectFirstButton: false);
			}
			if (MPExtensions.IsClient(_gameMode))
			{
				Hide();
			}
		}
	}

	private void CopyCodeButton_Click(object sender)
	{
		MPExtensions.CopyToClipboard(_view.CodeLabel.Text);
	}

	private void KickButton_Click(object sender)
	{
		_navigableItems.GetActive(0, 1)?.Select();
		MultiplayerMng.BlockCurrentClientId();
		PlayerUtilities.GetPeerFromId(1)?.ReplicatedDisconnect(DisconnectDetail.Kicked_By_Server);
	}

	private void StartButton_Click(object sender)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		EGameMode modeFromView = GetModeFromView();
		if (MPExtensions.IsMultiplayer(_gameMode))
		{
			if (!_continueMPGame && SaveManager.CheckSaveFileVersion((GameMode)4))
			{
				DialogueBox.Show(ReInput.players.GetPlayer(0), "NEW_GAME_MENU_DB_WARNING_TITLE", "NEW_GAME_MENU_DB_WARNING_OVERWRITE_DESC", DialogueButton.YesNo).Accept += StartGame;
			}
			else
			{
				StartGame();
			}
		}
		else if ((modeFromView == EGameMode.Single && SaveManager.CheckSaveFileVersion((GameMode)0)) || (modeFromView == EGameMode.Cooperative && SaveManager.CheckSaveFileVersion((GameMode)1)))
		{
			DialogueBox.Show(ReInput.players.GetPlayer(0), "NEW_GAME_MENU_DB_WARNING_TITLE", "NEW_GAME_MENU_DB_WARNING_OVERWRITE_DESC", DialogueButton.YesNo).Accept += StartGame;
		}
		else
		{
			StartGame();
		}
	}

	private void BackButton_Click(object sender)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(_gameMode))
		{
			PlayerRegistry.LocalPeer.ReplicatedDisconnect(DisconnectDetail.Intended);
		}
		Hide();
	}

	private void ModeOptionButton_SelectionChanged(IOptionButtonViewAdapter sender)
	{
		UpdateGenderUI();
	}

	private void GenderOptionButton_SelectionChanged(IOptionButtonViewAdapter sender)
	{
		UpdateGenderUI();
	}

	private void Gender2OptionButton_SelectionChanged(IOptionButtonViewAdapter sender)
	{
		UpdateGenderUI();
	}

	private void GameDifficultyButton_Click(object obj)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(_gameMode) && BeamTeam.IsValid(PlayerRegistry.ServerPeer))
		{
			PlayerRegistry.ServerPeer.GameDifficulty = (EGameDifficultyMode)_view.DifficultyOptionButton.SelectedIndex;
		}
	}

	private void CharacterSelectionButton_Click(object obj)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(_gameMode) && BeamTeam.IsValid(PlayerRegistry.ServerPeer))
		{
			PlayerRegistry.ServerPeer.Gender = (EGameGenderMode)_view.GenderOptionButton.SelectedIndex;
		}
		UpdateGenderUI();
	}

	private void Character2SelectionButton_Click(object obj)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(_gameMode) && BeamTeam.IsValid(PlayerRegistry.ServerPeer))
		{
			PlayerRegistry.ServerPeer.Gender2 = (EGameGenderMode)_view.Gender2OptionButton.SelectedIndex;
		}
		UpdateGenderUI();
	}

	private void WorldTypeSelection_Click(object obj)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(_gameMode) && BeamTeam.IsValid(PlayerRegistry.ServerPeer))
		{
			PlayerRegistry.ServerPeer.World = (EGameWorldMode)_view.WorldOptionButton.SelectedIndex;
		}
	}

	private void PermadeathSelection_Click(object obj)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(_gameMode) && BeamTeam.IsValid(PlayerRegistry.ServerPeer))
		{
			PlayerRegistry.ServerPeer.Permadeath = (EGamePermadeathMode)_view.PermadeathOptionButton.SelectedIndex;
		}
	}

	private void WildlifeStateButton_Click(object obj)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(_gameMode) && BeamTeam.IsValid(PlayerRegistry.ServerPeer))
		{
			PlayerRegistry.ServerPeer.Wildlife = (EGameWildlifeMode)_view.WildlifeOptionButton.SelectedIndex;
		}
	}

	private void StartingCrateButton_Click(object obj)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(_gameMode) && BeamTeam.IsValid(PlayerRegistry.ServerPeer))
		{
			PlayerRegistry.ServerPeer.StartingCrate = (EGameStartingCrate)_view.StartingCrateOptionButton.SelectedIndex;
		}
	}

	private void OptionWidget_Selected(ISelectableViewAdapter selectable)
	{
		IOptionWidgetViewAdapter optionWidgetViewAdapter = selectable as IOptionWidgetViewAdapter;
		if (!BeamTeam.IsNullOrDestroyed(optionWidgetViewAdapter))
		{
			string tooltip = optionWidgetViewAdapter.GetTooltip();
			_view.SetTooltip(tooltip);
		}
	}

	private void OptionWidget_MouseEnter(ISelectableViewAdapter selectable)
	{
		IOptionWidgetViewAdapter optionWidgetViewAdapter = selectable as IOptionWidgetViewAdapter;
		if (!BeamTeam.IsNullOrDestroyed(optionWidgetViewAdapter))
		{
			string tooltip = optionWidgetViewAdapter.GetTooltip();
			_view.SetTooltip(tooltip);
		}
	}

	private void OptionWidget_MouseLeave(ISelectableViewAdapter selectable)
	{
		_view.SetTooltip(string.Empty);
	}

	private void OptionButton_SelectionChanged(IOptionButtonViewAdapter button)
	{
		if (_view.Visible && !_selectionChangedViaReplication)
		{
			_view.SetTooltip(button.GetTooltip());
		}
	}

	private void StartGame()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsClient(_gameMode))
		{
			return;
		}
		if (GetModeFromView() == EGameMode.Cooperative && RewiredUtilities.ConnectedPlayers.Count() < 2)
		{
			DialogueBox.Show(ReInput.players.GetPlayer(0), "NEW_GAME_MENU_DB_WARNING_TITLE", "NEW_GAME_MENU_DB_WARNING_DESC", DialogueButton.Ok, DialogueIcon.Warning);
			return;
		}
		Suspend = true;
		_lateSuspend = true;
		if (MPExtensions.IsMultiplayer(_gameMode) && (!Object.op_Implicit((Object)(object)_notAllPlayersDialog) || !((Behaviour)_notAllPlayersDialog).enabled))
		{
			if (!PlayerUtilities.AllPeersPresent())
			{
				_notAllPlayersDialog = DialogueBox.Show(ReInput.players.GetPlayer(0), "MAIN_MENU_DB_WARNING_TITLE", "HOST_GAME_MENU_DB_WARNING_NOT_ALL_PLAYERS_DESC", DialogueButton.OkCancel);
				_notAllPlayersDialog.Accept += () =>
				{
					ActualStart();
				};
				_notAllPlayersDialog.Cancel += () =>
				{
					Suspend = false;
				};
				_notAllPlayersDialog.Decline += () =>
				{
					Suspend = false;
				};
			}
			else
			{
				ActualStart();
			}
		}
		else
		{
			ActualStart();
		}
	}

	private void ActualStart()
	{
		Options.CustomSettings = new GameSettings(GetModeFromView(), (EGameDifficultyMode)_view.DifficultyOptionButton.SelectedIndex, (EGameWorldMode)_view.WorldOptionButton.SelectedIndex, (EGameGenderMode)_view.GenderOptionButton.SelectedIndex, (EGameGenderMode)_view.Gender2OptionButton.SelectedIndex, (EGamePermadeathMode)_view.PermadeathOptionButton.SelectedIndex, (EGameWildlifeMode)_view.WildlifeOptionButton.SelectedIndex, (EGameStartingCrate)_view.StartingCrateOptionButton.SelectedIndex);
		if (_continueMPGame)
		{
			StartMultiplayerContinueCallback?.Invoke();
		}
		else
		{
			StartCallback?.Invoke(Options.CustomSettings);
		}
	}

	private void UpdateServerPeerOptions()
	{
		WorldTypeSelection_Click(null);
		GameDifficultyButton_Click(null);
		WildlifeStateButton_Click(null);
		CharacterSelectionButton_Click(null);
		Character2SelectionButton_Click(null);
		StartingCrateButton_Click(null);
		PermadeathSelection_Click(null);
	}

	private void ServerPeerObserver(Peer.Params param, int value)
	{
		_selectionChangedViaReplication = true;
		switch (param)
		{
		case Peer.Params.GameDifficulty:
			_view.DifficultyOptionButton.SelectedIndex = value;
			break;
		case Peer.Params.World:
			_view.WorldOptionButton.SelectedIndex = value;
			break;
		case Peer.Params.Gender:
			_view.GenderOptionButton.SelectedIndex = value;
			UpdateGenderUI();
			break;
		case Peer.Params.Gender2:
			_view.Gender2OptionButton.SelectedIndex = value;
			UpdateGenderUI();
			break;
		case Peer.Params.Permadeath:
			_view.PermadeathOptionButton.SelectedIndex = value;
			break;
		case Peer.Params.Wildlife:
			_view.WildlifeOptionButton.SelectedIndex = value;
			break;
		case Peer.Params.GameStartingCrate:
			_view.StartingCrateOptionButton.SelectedIndex = value;
			break;
		case Peer.Params.Continuing:
			if (value == 1)
			{
				_continueMPGame = true;
			}
			UpdateUILayout(selectFirstButton: true);
			break;
		}
		_selectionChangedViaReplication = false;
	}
}
