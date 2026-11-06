using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Beam.AccountServices;
using Beam.Events;
using Beam.Language;
using Beam.Obfuscated.Users;
using Beam.Serialization;
using Beam.Terrain;
using Beam.Utilities;
using Funlabs;
using MEC;
using Rewired;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Beam.UI;

public class MainMenuPresenter : MonoBehaviour
{
	private enum MainMenuScreen
	{
		MAIN,
		PLAY,
		PLAY_ONLINE,
		HOST,
		HOST_NEW_GAME,
		HOST_CONTINUE_GAME,
		JOIN
	}

	public delegate void QuittingToMenuHandler();

	private class ReplicateStartGame : MultiplayerMessage
	{
		[Replicate]
		public int Mode;

		[Replicate]
		public int State;

		[Replicate]
		public int Scene;

		public override void OnPeer()
		{
			Instance.StartGame((GameMode)Mode, (GameState)State, Scene);
		}
	}

	private class ReplicateQuitToMenu : MultiplayerMessage
	{
		public override void OnPeer()
		{
			string title = "TEXT_ERROR";
			string message = Localization.GetLanguageHandler().Localize("DISCONNECTED_INTERNAL", "HOST_QUIT");
			DialogueBox.Show(ReInput.players.GetPlayer(0), title, message, DialogueButton.None, DialogueIcon.Information);
			Instance.QuitToMenu(8f);
		}
	}

	private static MainMenuPresenter _Instance;

	private static bool NEW_RANDOM_WORLD_CREATED;

	[SerializeField]
	private LoadingScreenPresenter _loadingScreenPresenter;

	[SerializeField]
	private NewGameMenuPresenter _newGameMenuPresenter;

	[SerializeField]
	private ChangeSaveSlotMenuPresenter _changeSaveSlotPresenter;

	[SerializeField]
	private OptionsMenuPresenter _optionsMenuPresenter;

	[SerializeField]
	private CartographerMenuPresenter _cartographerMenuPresenter;

	[SerializeField]
	private LeaderboardsMenuPresenter _leaderboardsMenuPresenter;

	[SerializeField]
	private CreateSessionMenuPresenter _createSessionMenuPresenter;

	[SerializeField]
	private LobbyMenuPresenter _lobbyMenuPresenter;

	[SerializeField]
	private AudioSource _mainMenuTrack;

	[SerializeField]
	private AudioSource _pauseMenuTrack;

	private MainMenuScreen _currentSubScreen;

	[SerializeField]
	private MainMenuViewAdapterBase _view;

	private Player _currentPlayerInput;

	private bool _suspend;

	private bool _childPanelOpen;

	private bool _customBgd;

	private int _mapBeforePaused;

	private const float SESSION_TIMEOUT = 30f;

	private bool _isGamePaused;

	[HideInInspector]
	public bool ContinuingMultiplayerGame;

	private DialogueBoxViewAdapter _joinSessionDialog;

	private CoroutineHandle _joinSessionDialogTimeout;

	public static MainMenuPresenter Instance
	{
		get
		{
			if ((Object)(object)_Instance == (Object)null)
			{
				_Instance = new MainMenuPresenter();
			}
			return _Instance;
		}
		set
		{
			_Instance = value;
		}
	}

	public LoadingScreenPresenter LoadingScreenPresenter => _loadingScreenPresenter;

	public bool IsGamePaused => _isGamePaused;

	public event QuittingToMenuHandler QuittingToMenu;

	private void Awake()
	{
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)("Stranded Deep Version: " + BuildInformation.GetPrettyString()));
		Game.State = (GameState)0;
		Application.targetFrameRate = 60;
		QualitySettings.SetQualityLevel(QualitySettings.names.Length);
		_view.Hide();
		_newGameMenuPresenter.Initialize(_view);
		_changeSaveSlotPresenter.Initialize(_view);
		_optionsMenuPresenter.Initialize(_view);
		_cartographerMenuPresenter.Initialize(_view);
		_createSessionMenuPresenter.Initialize(_view);
		_newGameMenuPresenter.ShowView += MenuPresenter_ShowView;
		_newGameMenuPresenter.HideView += MenuPresenter_HideView;
		NewGameMenuPresenter newGameMenuPresenter = _newGameMenuPresenter;
		newGameMenuPresenter.StartCallback = (Action<GameSettings>)Delegate.Combine(newGameMenuPresenter.StartCallback, new Action<GameSettings>(StartNewGame));
		NewGameMenuPresenter newGameMenuPresenter2 = _newGameMenuPresenter;
		newGameMenuPresenter2.StartMultiplayerContinueCallback = (Action)Delegate.Combine(newGameMenuPresenter2.StartMultiplayerContinueCallback, new Action(StartContinueGame));
		_changeSaveSlotPresenter.ShowView += MenuPresenter_ShowView;
		_changeSaveSlotPresenter.HideView += MenuPresenter_HideView;
		_changeSaveSlotPresenter.SlotChanged += ChangeSaveSlotMenuPresenter_SlotChanged;
		_optionsMenuPresenter.ShowView += MenuPresenter_ShowView;
		_optionsMenuPresenter.HideView += MenuPresenter_HideView;
		_cartographerMenuPresenter.ShowView += MenuPresenter_ShowView;
		_cartographerMenuPresenter.HideView += MenuPresenter_HideView;
		_cartographerMenuPresenter.CreatedNewWorld += CartographerMenuPresenter_CreatedNewWorld;
		_leaderboardsMenuPresenter.ShowView += MenuPresenter_ShowView;
		_leaderboardsMenuPresenter.HideView += MenuPresenter_HideView;
		_createSessionMenuPresenter.ShowView += MenuPresenter_ShowView;
		_createSessionMenuPresenter.HideView += MenuPresenter_HideView;
		_lobbyMenuPresenter.ShowView += MenuPresenter_ShowView;
		_lobbyMenuPresenter.HideView += MenuPresenter_HideView;
		ReInput.ControllerDisconnectedEvent += Rewired_OnControllerDisconnected;
		SaveManager.DeletedSaveFile += SaveManager_DeletedSaveFile;
		CheckBuildVersion();
		SetControlEvents();
		SubscribeToInputEvents();
		Game.Mode = (GameMode)0;
		Timing.RunCoroutine(Initialize_Sequence());
		_currentSubScreen = MainMenuScreen.MAIN;
	}

	private void Start()
	{
		ControllerFocus.Instance?.Subscribe(_view);
	}

	private IEnumerator<float> Initialize_Sequence()
	{
		_view.CanvasGroup.Interactable = false;
		if ((int)Game.PreviousState == 0)
		{
			Singleton<Fader>.Instance.SetFadeColor(Color.black);
		}
		Singleton<Fader>.Instance.FadeOut(0f);
		_view.Hide();
		_loadingScreenPresenter.Show();
		Player input = (_currentPlayerInput = ReInput.players.GetPlayer(0));
		bool checkFilePermissions = FilePath.CheckFilePermissions() && FilePath.CreateDirectories();
		if (!checkFilePermissions)
		{
			_loadingScreenPresenter.Hide();
			string message = Localization.GetLanguageHandler().Localize("MAIN_MENU_DB_FILE_PERMISSIONS_DESC", FilePath.SAVE_FOLDER);
			DialogueBoxViewAdapter dialogueBoxViewAdapter = DialogueBox.Show(_currentPlayerInput, "MAIN_MENU_DB_WARNING_TITLE", message, "MAIN_MENU_DB_BUTTON_VISIT", "MAIN_MENU_BUTTON_QUIT", "", DialogueIcon.Warning);
			dialogueBoxViewAdapter.Accept += () =>
			{
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				Application.OpenURL("https://forums.strandeddeepgame.com");
				Singleton<Fader>.Instance.FadeOut(3f, FadeMode.Solid, QuitToDesktop);
			};
			dialogueBoxViewAdapter.Decline += () =>
			{
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				Singleton<Fader>.Instance.FadeOut(3f, FadeMode.Solid, QuitToDesktop);
			};
		}
		while (!checkFilePermissions)
		{
			yield return 0f;
		}
		IAccountService accountService = GAccountService.Get();
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_INITIALIZING " + accountService.Name));
		yield return 0f;
		CheckAccountServicesInitialized();
		while (!accountService.Initialized)
		{
			yield return 0f;
		}
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_AUTHENTICATING " + accountService.Name));
		yield return 0f;
		IAuthenticationHandler authentication = accountService.GetAuthenticationHandler();
		authentication.Login(Authentication_OnLoginCallback);
		while (!authentication.LoggedIn)
		{
			yield return 0f;
		}
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_RETRIEVING_USER_INFO"));
		yield return 0f;
		AccountUser localUser = accountService.LocalUser;
		while (localUser.UserInfo == null)
		{
			yield return 0f;
		}
		UpdateUserName(localUser.UserInfo.DisplayName);
		Options.Load();
		GraphicsUtilities.SetFixedFrameRate(Options.GraphicsSettings.FixedFrameRate);
		GraphicsUtilities.SetVSync(Options.GraphicsSettings.VerticalSync);
		yield return Timing.WaitUntilDone(Timing.RunCoroutine(Load_Sequence()));
		RewiredUtilities.SetCursorEnabled(input, enabled: true);
	}

	private IEnumerator<float> Reload_Sequence()
	{
		_view.CanvasGroup.Interactable = false;
		yield return Singleton<Fader>.Instance.FadeOut_Async(0f);
		_view.Hide();
		_loadingScreenPresenter.Show();
		yield return Timing.WaitUntilDone(Timing.RunCoroutine(Load_Sequence()));
	}

	private IEnumerator<float> Load_Sequence()
	{
		SaveManager.ChangeCurrentSlot(Options.GeneralSettings.LastSaveSlotUsed);
		yield return Timing.WaitUntilDone(Timing.RunCoroutine(Migrate_Sequence()));
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_LOADING_WORLD"));
		yield return 0f;
		Prefabs.LoadPrefabMap();
		yield return Timing.WaitUntilDone(Timing.RunCoroutine(LoadWorld()));
		_loadingScreenPresenter.Hide();
		Show(_currentPlayerInput);
		if ((int)Game.PreviousState == 4)
		{
			Singleton<Fader>.Instance.ResetFade(FadeMode.Solid, FadeDirection.Out);
			CartographerButton_Click(null);
		}
		else
		{
			LeanTween.value(((Component)this).gameObject, 0f, 1f, 1f).setOnUpdate((Action<float>)((float value) =>
			{
				AudioListener.volume = Mathf.Clamp01(value);
			})).setIgnoreTimeScale(true);
			yield return Singleton<Fader>.Instance.FadeIn_Async(1f);
			Singleton<Fader>.Instance.ResetColor();
		}
		_view.CanvasGroup.Interactable = true;
		if ((int)Game.PreviousState != 4)
		{
			_view.SelectFirstButton();
		}
	}

	private IEnumerator<float> Migrate_Sequence()
	{
		if (Options.GeneralSettings.LastSaveSlotUsed != 0 || Directory.Exists(PathTools.Combine(new string[3]
		{
			SaveManager.GetSaveFolderPath(),
			"World",
			"0"
		})))
		{
			yield break;
		}
		string filePath = string.Empty;
		if (!LegacyUtilities.HasSaveFilePre_74_00((GameMode)0, out filePath) && !LegacyUtilities.HasSaveFilePre_74_00((GameMode)1, out filePath))
		{
			yield break;
		}
		bool import = false;
		DialogueBox.Show(_currentPlayerInput, "IMPORT", "Save data location has changed for game versions 0.75.00 and above.\n\nNo save data was found in the new location, but save data was found in the old location at:\n\n" + filePath + "\n\nDo you want to import?", DialogueButton.YesNo, DialogueIcon.Information).Accept += () =>
		{
			import = true;
		};
		while (DialogueBox.Notifying)
		{
			yield return 0f;
		}
		if (!import)
		{
			yield break;
		}
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("IMPORTING LEGACY SAVE DATA"));
		yield return 0f;
		string text = string.Empty;
		bool flag;
		try
		{
			LegacyUtilities.MigrateSaveDataPre_74_00();
			flag = true;
		}
		catch (Exception ex)
		{
			flag = false;
			text = ex.Message;
			Debug.LogException(ex);
		}
		if (!flag)
		{
			DialogueBox.Show(_currentPlayerInput, "TEXT_ERROR", "There was an error importing:\n" + text + "\n\nPlease visit the Stranded Deep Steam forum below for more information on how to migrate your old save data manually.", "MAIN_MENU_DB_BUTTON_VISIT", "MAIN_MENU_DB_BUTTON_CLOSE", "", DialogueIcon.Warning).Accept += () =>
			{
				Application.OpenURL("https://steamcommunity.com/sharedfiles/filedetails/?id=2265407307");
			};
			while (DialogueBox.Notifying)
			{
				yield return 0f;
			}
		}
		else
		{
			Options.Load();
			Object.FindObjectOfType<AudioMenuPresenter>()?.LoadMixerValues();
		}
	}

	private IEnumerator<float> LoadWorld()
	{
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("UPGRADING LEGACY WORLD"));
		yield return 0f;
		LegacyUtilities.UpgradeLegacyGame();
		bool flag = World.CheckWorldSeedFileExists();
		if (!SaveManager.ValidateSaveFile((GameMode)0) && !SaveManager.ValidateSaveFile((GameMode)1) && !SaveManager.ValidateSaveFile((GameMode)4) && !flag)
		{
			EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_CREATING_WORLD"));
			yield return 0f;
			_cartographerMenuPresenter.CreateRandomWorld();
			yield break;
		}
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_LOADING_WORLD"));
		yield return 0f;
		if (!World.LoadWorld().Succeeded)
		{
			DialogueBox.Show(_currentPlayerInput, "MAIN_MENU_DB_ERROR_TITLE", "MAIN_MENU_DB_ERROR_LOAD_WORLD_DESC", DialogueButton.Ok, DialogueIcon.Warning).Accept += () =>
			{
				_view.NewGameButton.Enabled = false;
				_cartographerMenuPresenter.Show();
			};
		}
	}

	private bool CheckAccountServicesInitialized()
	{
		IAccountService accountService = GAccountService.Get();
		bool initialized = accountService.Initialized;
		if (!initialized)
		{
			_loadingScreenPresenter.Hide();
			string message = Localization.GetLanguageHandler().Localize("MAIN_MENU_DB_ERROR_INITIALIZE_DRM_DESC", accountService.Name);
			DialogueBox.Show(_currentPlayerInput, "TEXT_ERROR", message, DialogueButton.Ok, DialogueIcon.Warning).Accept += () =>
			{
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				Singleton<Fader>.Instance.FadeOut(3f, FadeMode.Solid, QuitToDesktop);
			};
		}
		return initialized;
	}

	private void Authentication_OnLoginCallback(AuthCallbackInfo callbackInfo)
	{
		if (callbackInfo.Result != 0)
		{
			_loadingScreenPresenter.Hide();
			string message = Localization.GetLanguageHandler().Localize("MAIN_MENU_DB_ERROR_AUTHENTICATE_DESC", GAccountService.Get().Name, callbackInfo.Result.ToString("X8"));
			DialogueBox.Show(_currentPlayerInput, "MAIN_MENU_DB_ERROR_TITLE", message, DialogueButton.Ok, DialogueIcon.Warning).Accept += () =>
			{
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				Singleton<Fader>.Instance.FadeOut(3f, FadeMode.Solid, QuitToDesktop);
			};
		}
	}

	private void CheckSystemSpecs()
	{
	}

	private void CheckBuildVersion()
	{
		_view.VersionNumberLabel.Text = BuildInformation.GetPrettyString();
	}

	private void OnDestroy()
	{
		QuittingToMenu = null;
		if (Object.op_Implicit((Object)(object)ControllerFocus.Instance))
		{
			ControllerFocus.Instance.Unsubscribe(_view);
		}
		_view.HideView -= _newGameMenuPresenter.Hide;
		_view.HideView -= _changeSaveSlotPresenter.Hide;
		_view.HideView -= _optionsMenuPresenter.Hide;
		_view.HideView -= _cartographerMenuPresenter.Hide;
		_view.HideView -= _createSessionMenuPresenter.Hide;
		_newGameMenuPresenter.ShowView -= MenuPresenter_ShowView;
		_newGameMenuPresenter.HideView -= MenuPresenter_HideView;
		_changeSaveSlotPresenter.ShowView -= MenuPresenter_ShowView;
		_changeSaveSlotPresenter.HideView -= MenuPresenter_HideView;
		_changeSaveSlotPresenter.SlotChanged -= ChangeSaveSlotMenuPresenter_SlotChanged;
		_optionsMenuPresenter.ShowView -= MenuPresenter_ShowView;
		_optionsMenuPresenter.HideView -= MenuPresenter_HideView;
		_cartographerMenuPresenter.ShowView -= MenuPresenter_ShowView;
		_cartographerMenuPresenter.HideView -= MenuPresenter_HideView;
		_leaderboardsMenuPresenter.ShowView -= MenuPresenter_ShowView;
		_leaderboardsMenuPresenter.HideView -= MenuPresenter_HideView;
		_createSessionMenuPresenter.ShowView -= MenuPresenter_ShowView;
		_createSessionMenuPresenter.HideView -= MenuPresenter_HideView;
		_lobbyMenuPresenter.ShowView -= MenuPresenter_ShowView;
		_lobbyMenuPresenter.HideView -= MenuPresenter_HideView;
		_cartographerMenuPresenter.CreatedNewWorld -= CartographerMenuPresenter_CreatedNewWorld;
		ReInput.ControllerDisconnectedEvent -= Rewired_OnControllerDisconnected;
		SaveManager.DeletedSaveFile -= SaveManager_DeletedSaveFile;
		UnsubscribeFromInputEvents();
		EventManager.RemoveListener<PlayerConnectedEvent>(OnPlayerConnected);
	}

	private void SetControlEvents()
	{
		_view.PlayButton.Click += PlayButton_Click;
		_view.PlaySubMenuBackButton.Click += PlayBackButton_Click;
		_view.PlayOnlineButton.Click += PlayOnlineButton_Click;
		_view.PlayOnlineContinueButton.Click += PlayOnlineContinueButton_Click;
		_view.PlayOnlineNewGameButton.Click += PlayOnlineNewGameButton_Click;
		_view.PlayOnlineHostButton.Click += PlayOnlineHostButton_Click;
		_view.PlayOnlineJoinButton.Click += PlayOnlineJoinButton_Click;
		_view.PlayOnlineBackButton.Click += PlayOnlineBackButton_Click;
		_view.PlayOnlineHostBackButton.Click += PlayOnlineHostBackButton_Click;
		_view.NewGameButton.Click += NewGameButton_Click;
		_view.ContinueButton.Click += ContinueButton_Click;
		_view.ContinueCoopButton.Click += ContinueCoopButton_Click;
		_view.ChangeSaveSlotButton.Click += ChangeSaveSlotButton_Click;
		_view.CartographerButton.Click += CartographerButton_Click;
		_view.LeaderboardsButton.Click += LeaderboardsButton_Click;
		_view.OptionsButton.Click += OptionsButton_Click;
		_view.QuitToDesktopButton.Click += QuitToDesktopButton_Click;
		_view.ResumePauseButton.Click += ResumePauseButton_Click;
		_view.OptionsPauseButton.Click += OptionsButton_Click;
		_view.QuitGamePauseButton.Click += QuitGamePauseButton_Click;
		_view.QuitToDesktopPauseButton.Click += QuitToDesktopPauseButton_Click;
		_view.CopyLobbyCodeButton.Click += CopyLobbyCodeButton_Click;
	}

	private void CopyLobbyCodeButton_Click(object sender)
	{
		MPExtensions.CopyToClipboard(_view.LobbyCodeLabel.Text);
	}

	private void SubscribeToInputEvents()
	{
		for (int i = 0; i < RewiredUtilities.Players.Count; i++)
		{
			RewiredUtilities.Players[i].AddInputEventDelegate((Action<InputActionEventData>)RewiredPlayer_EscapeButtonDown, (UpdateLoopType)0, (InputActionEventType)3, 39);
		}
	}

	private void UnsubscribeFromInputEvents()
	{
		if (ReInput.isReady)
		{
			for (int i = 0; i < RewiredUtilities.Players.Count; i++)
			{
				RewiredUtilities.Players[i].RemoveInputEventDelegate((Action<InputActionEventData>)RewiredPlayer_EscapeButtonDown, (UpdateLoopType)0, (InputActionEventType)3, 39);
			}
		}
	}

	private void Update()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Shader.SetGlobalVector("_UnscaledTime", new Vector4(Time.realtimeSinceStartup / 20f, Time.realtimeSinceStartup, Time.realtimeSinceStartup * 2f, Time.realtimeSinceStartup * 3f));
		Player player = ReInput.players.GetPlayer(0);
		if ((player.GetButtonDown(39) || player.GetButtonDown(31)) && _view.Visible)
		{
			AttemptToGoToPreviousScreen();
		}
	}

	public void Show(Player input)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		Instance = this;
		if (!_view.Visible)
		{
			if (Object.op_Implicit((Object)(object)Singleton<SoundtrackManager>.Instance))
			{
				Singleton<SoundtrackManager>.Instance.PauseTracks();
			}
			AudioSource val = (((int)Game.State == 0) ? _mainMenuTrack : _pauseMenuTrack);
			if (!val.isPlaying)
			{
				val.Play();
			}
			IAudioPlayer audioPlayer = AudioManager.GetAudioPlayer();
			audioPlayer.GetAudioMixer().GetChannel(AudioMixerChannels.Environment).Mute = true;
			audioPlayer.GetAudioMixer().GetChannel(AudioMixerChannels.FX).Mute = true;
			audioPlayer.GetAudioMixer().GetChannel(AudioMixerChannels.Voice).Mute = true;
			Time.timeScale = 0f;
			_currentPlayerInput = input;
			if ((int)Game.State != 0)
			{
				_isGamePaused = true;
				_mapBeforePaused = RewiredUtilities.GetCurrentMapId(_currentPlayerInput);
				RewiredUtilities.SetAndCacheMapsEnabled(_currentPlayerInput, state: false, _mapBeforePaused);
				_currentPlayerInput.AddInputEventDelegate((Action<InputActionEventData>)RewiredPlayer_EscapeButtonDown, (UpdateLoopType)0, (InputActionEventType)3, 31);
			}
			else
			{
				RewiredUtilities.SetAndCacheMapsEnabled(_currentPlayerInput, state: false, 0);
			}
			RewiredUtilities.DisableMaps(RewiredUtilities.GetOtherPlayers(_currentPlayerInput));
			RewiredUtilities.SetAndCacheMapsEnabled(_currentPlayerInput, state: true, 8);
			RewiredUtilities.SetCursorEnabled(_currentPlayerInput, enabled: true);
			EventManager.RaiseEvent(new MainMenuOpenedEvent(RewiredUtilities.GetPlayerFromInput(input)?.Id ?? input.id, opened: true));
			IPlayer playerFromInput = RewiredUtilities.GetPlayerFromInput(_currentPlayerInput);
			if (!BeamTeam.IsNullOrDestroyed(playerFromInput))
			{
				playerFromInput.IsBusy = true;
				++playerFromInput.Movement.IsBusy;
				playerFromInput.Interacter.IsInPauseMenu = true;
				((Behaviour)playerFromInput.PlayerCamera.MouseLook).enabled = false;
			}
			Refresh();
			((Component)_view.LobbyCodeGroup.transform).gameObject.SetActive(_isGamePaused && MPExtensions.IsServer(Game.Mode));
			if (MPExtensions.IsMultiplayer(Game.Mode))
			{
				_view.LobbyCodeLabel.Text = MPExtensions.ToCode(Game.MultiplayerSessionName, MultiplayerMng.GetCloudRegionIndex());
			}
			_view.SetInput(_currentPlayerInput);
			_view.Show();
			_view.SelectFirstButton();
			if (MPExtensions.IsServer(Game.Mode))
			{
				EventManager.AddListener<PlayerConnectedEvent>(OnPlayerConnected);
			}
		}
	}

	private void OnPlayerConnected(PlayerConnectedEvent @event)
	{
		Hide();
	}

	private void Refresh()
	{
		InitializeControls();
		InitializeStats();
	}

	private void Hide()
	{
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		EventManager.RemoveListener<PlayerConnectedEvent>(OnPlayerConnected);
		if (_view.Visible)
		{
			_mainMenuTrack.Stop();
			_pauseMenuTrack.Stop();
			if (Object.op_Implicit((Object)(object)Singleton<SoundtrackManager>.Instance))
			{
				Singleton<SoundtrackManager>.Instance.ResumeTracks();
			}
			IAudioPlayer audioPlayer = AudioManager.GetAudioPlayer();
			audioPlayer.GetAudioMixer().GetChannel(AudioMixerChannels.Environment).Mute = false;
			audioPlayer.GetAudioMixer().GetChannel(AudioMixerChannels.FX).Mute = false;
			audioPlayer.GetAudioMixer().GetChannel(AudioMixerChannels.Voice).Mute = false;
			Time.timeScale = 1f;
			EventManager.RaiseEvent(new MainMenuOpenedEvent(RewiredUtilities.GetPlayerFromInput(_currentPlayerInput)?.Id ?? _currentPlayerInput.id, opened: false));
			IPlayer playerFromInput = RewiredUtilities.GetPlayerFromInput(_currentPlayerInput);
			if (!BeamTeam.IsNullOrDestroyed(playerFromInput))
			{
				playerFromInput.IsBusy = false;
				--playerFromInput.Movement.IsBusy;
				playerFromInput.Interacter.IsInPauseMenu = false;
				((Behaviour)playerFromInput.PlayerCamera.MouseLook).enabled = true;
			}
			if ((int)Game.State != 0)
			{
				_isGamePaused = false;
				RewiredUtilities.SetAndCacheMapsEnabled(_currentPlayerInput, state: true, _mapBeforePaused);
				_currentPlayerInput.RemoveInputEventDelegate((Action<InputActionEventData>)RewiredPlayer_EscapeButtonDown, (UpdateLoopType)0, (InputActionEventType)3, 31);
			}
			else
			{
				RewiredUtilities.SetAndCacheMapsEnabled(_currentPlayerInput, state: true, 0);
			}
			RewiredUtilities.RestoreMapsEnabled(RewiredUtilities.GetOtherPlayers(_currentPlayerInput));
			RewiredUtilities.SetAndCacheMapsEnabled(_currentPlayerInput, state: false, 8);
			RewiredUtilities.SetCursorEnabled(_currentPlayerInput, enabled: false);
			_currentPlayerInput = null;
			_view.Hide();
		}
	}

	private void InitializeControls()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		_view.PlaySubMenu.SetActive(false);
		_view.PlayOnlineSubMenu.SetActive(false);
		_view.UserDetailsGroup.SetActive(true);
		if ((int)Game.State == 0)
		{
			_view.MainMenuControlGroup.SetActive(true);
			_view.PauseMenuControlGroup.SetActive(false);
			ValidateContinueButtons();
		}
		else
		{
			_view.MainMenuControlGroup.SetActive(false);
			_view.PauseMenuControlGroup.SetActive(true);
		}
		_view.SetNavigation();
	}

	private void InitializeStats()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		if ((int)Game.State == 0)
		{
			if (IsContinueButtonAvailable())
			{
				LoadStats((GameMode)0);
				UpdateUserStats(GAccountService.Get().LocalUser.UserStats.SessionStats);
			}
			else
			{
				UpdateUserStats("---", "---");
			}
		}
		else if ((int)Game.State == 2 || (int)Game.State == 3)
		{
			UpdateUserStats(Singleton<GameCalendar>.Instance.DaysAlive, Mathf.FloorToInt(Singleton<GameTime>.Instance.SessionDuration));
		}
	}

	private void LoadStats(GameMode mode)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		GAccountService.Get().LocalUser.UserStats.Load(mode);
	}

	private void ResetStats()
	{
		GAccountService.Get().LocalUser.UserStats.Reset();
	}

	private void ValidateContinueButtons()
	{
		if (SaveManager.ValidateSaveFile((GameMode)0))
		{
			_view.ContinueButton.Show();
		}
		else
		{
			_view.ContinueButton.Hide();
		}
		if (SaveManager.ValidateSaveFile((GameMode)1))
		{
			_view.ContinueCoopButton.Show();
			_view.ContinueCoopButton.Enabled = ReInput.players.playerCount == 2;
		}
		else
		{
			_view.ContinueCoopButton.Hide();
		}
	}

	private void UpdateUserName(string name)
	{
		_view.UserNameLabel.Text = name;
	}

	private void UpdateUserStats(Stats stats)
	{
		UpdateUserStats(stats.DaysSurvived + 1, stats.TimePlayed);
	}

	private void UpdateUserStats(int days, int seconds)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds((double)seconds);
		UpdateUserStats(days.ToString(), $"{timeSpan.Days:0}d {timeSpan.Hours:00}h {timeSpan.Minutes:00}m");
	}

	private void UpdateUserStats(string days, string seconds)
	{
		_view.UserDetailsDaysLabel.Text = days;
		_view.UserDetailsHoursLabel.Text = seconds;
	}

	private void RewiredPlayer_EscapeButtonDown(InputActionEventData eventData)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		if (!_suspend && !_childPanelOpen && (int)Game.State != 0 && (int)Game.State != 1 && !Singleton<Fader>.Instance.IsFading && !LevelLoader.IsLoading() && !LevelLoader.IsServerJoinInProgress && !SaveManager.IsSaving && !DialogueBox.Notifying && !BeamTeam.IsNullOrDestroyed(RewiredUtilities.GetPlayerFromInput(eventData.player)) && !RewiredUtilities.GetPlayerFromInput(eventData.player).Lighter.IsLightingFire && !RewiredUtilities.GetPlayerFromInput(eventData.player).Movement.IsInEndGameScene)
		{
			if (_view.Visible && _currentPlayerInput == eventData.player)
			{
				Hide();
			}
			else if (!_view.Visible && _currentPlayerInput == null)
			{
				Show(eventData.player);
			}
		}
	}

	private void MenuPresenter_ShowView()
	{
		_childPanelOpen = true;
		_view.MainMenuGameObject.SetActive(false);
	}

	private void MenuPresenter_HideView()
	{
		_childPanelOpen = false;
		_view.MainMenuGameObject.SetActive(true);
		_view.SelectFirstButton();
	}

	private void ChangeSaveSlotMenuPresenter_SlotChanged()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Timing.RunCoroutine(Reload_Sequence());
	}

	private void CartographerMenuPresenter_CreatedNewWorld()
	{
		NEW_RANDOM_WORLD_CREATED = true;
		_view.NewGameButton.Enabled = true;
	}

	private void SaveManager_DeletedSaveFile(GameMode mode)
	{
		ValidateContinueButtons();
		InitializeStats();
	}

	private void PlayButton_Click(object sender)
	{
		Game.Mode = (GameMode)0;
		ContinuingMultiplayerGame = false;
		if (!IsContinueButtonAvailable())
		{
			NewGameButton_Click(null);
			return;
		}
		_view.MainMenuControlGroup.SetActive(false);
		_view.PlaySubMenu.SetActive(true);
		_view.UserDetailsGroup.SetActive(false);
		_currentSubScreen = MainMenuScreen.PLAY;
		_view.SelectFirstButton();
	}

	private void PlayBackButton_Click(object sender)
	{
		_view.PlaySubMenu.SetActive(false);
		_view.MainMenuControlGroup.SetActive(true);
		_view.UserDetailsGroup.SetActive(true);
		_currentSubScreen = MainMenuScreen.MAIN;
		_view.SelectFirstButton();
	}

	private void PlayOnlineButton_Click(object sender)
	{
		CheckOnlineAvailable(() =>
		{
			_view.MainMenuControlGroup.SetActive(false);
			_view.PlayOnlineSubMenu.SetActive(true);
			_view.UserDetailsGroup.SetActive(false);
			_currentSubScreen = MainMenuScreen.PLAY_ONLINE;
			_view.SelectFirstButton();
		}, () =>
		{
			_view.SelectFirstButton();
		});
	}

	private void PlayOnlineContinueButton_Click(object sender)
	{
		CheckOnlineAvailable(() =>
		{
			ContinuingMultiplayerGame = true;
			_currentSubScreen = MainMenuScreen.HOST_CONTINUE_GAME;
			_createSessionMenuPresenter.Show(_currentPlayerInput);
		}, () =>
		{
			_view.SelectFirstButton();
		});
	}

	private void PlayOnlineNewGameButton_Click(object sender)
	{
		CheckOnlineAvailable(() =>
		{
			_currentSubScreen = MainMenuScreen.HOST_NEW_GAME;
			ContinuingMultiplayerGame = false;
			_createSessionMenuPresenter.Show(_currentPlayerInput);
		}, () =>
		{
			_view.SelectFirstButton();
		});
	}

	private void PlayOnlineHostButton_Click(object sender)
	{
		CheckOnlineAvailable(() =>
		{
			_currentSubScreen = MainMenuScreen.HOST;
			if (SaveManager.ValidateSaveFile((GameMode)4))
			{
				_view.PlayOnlineSubMenu.SetActive(false);
				_view.PlayOnlineHostSubMenu.SetActive(true);
				_view.SelectFirstButton();
			}
			else
			{
				PlayOnlineNewGameButton_Click(null);
			}
		}, () =>
		{
			_view.SelectFirstButton();
		});
	}

	private void PlayOnlineJoinButton_Click(object sender)
	{
		CheckOnlineAvailable(() =>
		{
			_currentSubScreen = MainMenuScreen.JOIN;
			_lobbyMenuPresenter.Show(_currentPlayerInput);
		}, () =>
		{
			_view.SelectFirstButton();
		});
	}

	private void PlayOnlineBackButton_Click(object sender)
	{
		_view.PlayOnlineSubMenu.SetActive(false);
		_view.MainMenuControlGroup.SetActive(true);
		_view.UserDetailsGroup.SetActive(true);
		_currentSubScreen = MainMenuScreen.MAIN;
		_view.SelectFirstButton();
	}

	private void PlayOnlineHostBackButton_Click(object sender)
	{
		_view.PlayOnlineSubMenu.SetActive(true);
		_view.PlayOnlineHostSubMenu.SetActive(false);
		_currentSubScreen = MainMenuScreen.PLAY_ONLINE;
		_view.SelectFirstButton();
	}

	private void NewGameButton_Click(object sender)
	{
		_newGameMenuPresenter.Show();
	}

	private void ContinueButton_Click(object sender)
	{
		StartGame((GameMode)0, (GameState)3, 3);
	}

	private void ContinueCoopButton_Click(object sender)
	{
		if (RewiredUtilities.ConnectedPlayers.Count() < 2)
		{
			DialogueBox.Show(_currentPlayerInput, "MAIN_MENU_DB_WARNING_TITLE", "MAIN_MENU_DB_WARNING_DESC_NO_CONTROLLERS_ASSIGNED", DialogueButton.Ok, DialogueIcon.Warning);
		}
		else
		{
			StartGame((GameMode)1, (GameState)3, 3);
		}
	}

	private void Rewired_OnControllerDisconnected(ControllerStatusChangedEventArgs args)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		if ((int)Game.Mode != 1 || _currentPlayerInput == null || _currentPlayerInput.id != 1 || !_view.Visible)
		{
			return;
		}
		RewiredUtilities.SetAndCacheMapsEnabled(_currentPlayerInput, state: true, 0);
		RewiredUtilities.SetAndCacheMapsEnabled(_currentPlayerInput, state: false, 8);
		RewiredUtilities.DisableMaps(_currentPlayerInput);
		_currentPlayerInput = ReInput.players.GetPlayer(0);
		RewiredUtilities.RestoreMapsEnabled(_currentPlayerInput);
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if (BeamTeam.IsNullOrDestroyed(allPlayer.Input))
			{
				return;
			}
			allPlayer.PlayerUI.UserInterface.EventSystem.Suspend(allPlayer.Input.id != _currentPlayerInput.id);
			allPlayer.PlayerUI.UserInterface.EventSystem.SetSelectedGameObject((GameObject)null);
		}
		RewiredUtilities.SetAndCacheMapsEnabled(_currentPlayerInput, state: false, 0);
		RewiredUtilities.SetAndCacheMapsEnabled(_currentPlayerInput, state: true, 8);
		RewiredUtilities.SetCursorEnabled(_currentPlayerInput, enabled: true);
		_view.SetInput(_currentPlayerInput);
		if (_optionsMenuPresenter.CurrentPanel == null)
		{
			_view.SelectFirstButton();
		}
		else
		{
			_optionsMenuPresenter.ResetControls(_currentPlayerInput);
		}
	}

	private void ChangeSaveSlotButton_Click(object sender)
	{
		Game.Mode = (GameMode)0;
		_changeSaveSlotPresenter.Show();
	}

	private void CartographerButton_Click(object sender)
	{
		Game.Mode = (GameMode)0;
		_cartographerMenuPresenter.Show();
	}

	private void LeaderboardsButton_Click(object sender)
	{
		if (!DialogueBox.Notifying)
		{
			Game.Mode = (GameMode)0;
			if (SteamUser.BLoggedOn())
			{
				_leaderboardsMenuPresenter.Show(_currentPlayerInput);
				return;
			}
			string title = "TEXT_ERROR";
			string message = Localization.GetLanguageHandler().Localize("LEADERBOARDS_MENU_DB_NO_INTERNET_DESC");
			DialogueBox.Show(ReInput.players.GetPlayer(0), title, message, DialogueButton.Ok, DialogueIcon.Information);
		}
	}

	private void OptionsButton_Click(object sender)
	{
		_optionsMenuPresenter.Show(_currentPlayerInput);
	}

	private void QuitToDesktopButton_Click(object sender)
	{
		DialogueBoxViewAdapter dialogueBoxViewAdapter = DialogueBox.Show(_currentPlayerInput, "MAIN_MENU_DB_QUIT_TO_DESKTOP_TITLE", "MAIN_MENU_DB_QUIT_TO_DESKTOP_DESC", DialogueButton.YesNo);
		dialogueBoxViewAdapter.Accept += QuitToDesktop;
		dialogueBoxViewAdapter.Decline += _view.SelectFirstButton;
	}

	private void QuitGamePauseButton_Click(object sender)
	{
		MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: true);
		DialogueBoxViewAdapter dialogueBoxViewAdapter = DialogueBox.Show(_currentPlayerInput, "MAIN_MENU_DB_QUIT_TITLE", "MAIN_MENU_DB_QUIT_DESC", DialogueButton.YesNo);
		dialogueBoxViewAdapter.Accept += MPFriendlyQuit;
		dialogueBoxViewAdapter.Decline += () =>
		{
			MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: false);
		};
		dialogueBoxViewAdapter.Cancel += () =>
		{
			MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: false);
		};
	}

	private void QuitToDesktopPauseButton_Click(object sender)
	{
		DialogueBox.Show(_currentPlayerInput, "MAIN_MENU_DB_QUIT_TO_DESKTOP_TITLE", "MAIN_MENU_DB_QUIT_TO_DESKTOP_DESC", DialogueButton.YesNo).Accept += QuitToDesktop;
	}

	private void AttemptToGoToPreviousScreen()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (_view.Visible && !DialogueBox.Notifying && (int)Game.State == 0)
		{
			switch (_currentSubScreen)
			{
			case MainMenuScreen.PLAY:
				PlayBackButton_Click(null);
				break;
			case MainMenuScreen.PLAY_ONLINE:
				PlayOnlineBackButton_Click(null);
				break;
			case MainMenuScreen.HOST:
				PlayOnlineHostBackButton_Click(null);
				break;
			case MainMenuScreen.HOST_CONTINUE_GAME:
				_currentSubScreen = MainMenuScreen.HOST;
				break;
			case MainMenuScreen.HOST_NEW_GAME:
				_currentSubScreen = MainMenuScreen.HOST;
				break;
			case MainMenuScreen.JOIN:
				_currentSubScreen = MainMenuScreen.PLAY_ONLINE;
				break;
			}
		}
	}

	private void ResumePauseButton_Click(object sender)
	{
		Hide();
	}

	private void StartNewGame(GameSettings settings)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		AccountUserStats userStats = GAccountService.Get().LocalUser.UserStats;
		userStats.SessionStats.Reset();
		userStats.Rebase();
		GameMode mode = Game.Mode;
		if (settings.Mode == EGameMode.Cooperative)
		{
			mode = (GameMode)1;
		}
		GameState state = (GameState)1;
		int scene = 2;
		Options.CustomSettings = settings;
		if (settings.World == EGameWorldMode.Existing || (settings.World == EGameWorldMode.Random && NEW_RANDOM_WORLD_CREATED))
		{
			ReplicatedStartGame(mode, state, scene);
		}
		else if (settings.World == EGameWorldMode.Random)
		{
			Timing.RunCoroutine(CreateRandomWorld_Sequence(mode, state, scene));
		}
	}

	private IEnumerator<float> CreateRandomWorld_Sequence(GameMode mode, GameState state, int scene)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(mode))
		{
			yield return PlayerRegistry.LocalPeer.SetTimeout_Async(Peer.GetSessionTimeout());
		}
		_cartographerMenuPresenter.ShowRandomizeWorldDialogue_NoInput(() =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			if (MPExtensions.IsServer(mode))
			{
				PlayerRegistry.LocalPeer.WorldSeed = StrandedWorld.LOCAL_WORLD_SEED;
			}
			ReplicatedStartGame(mode, state, scene);
			_newGameMenuPresenter.Suspend = false;
		}, () =>
		{
			_newGameMenuPresenter.Suspend = false;
		});
	}

	private void StartContinueGame()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		ReplicatedStartGame(Game.Mode, (GameState)3, 3);
	}

	public void StartGame(GameMode mode, GameState state, int scene)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Invalid comparison between Unknown and I4
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		Time.timeScale = 1f;
		if (Object.op_Implicit((Object)(object)_view))
		{
			_view.CanvasGroup.Interactable = false;
		}
		else
		{
			Debug.LogError((object)"No _view");
		}
		Game.Mode = mode;
		Game.State = state;
		ResetStats();
		if (MPExtensions.IsMaster(Game.Mode) && (int)Game.State == 3)
		{
			LoadStats(Game.Mode);
		}
		if (!MPExtensions.IsMultiplayer(Game.Mode))
		{
			MultiplayerMng.ConnectToLocal(() =>
			{
				_newGameMenuPresenter.Hide();
				StartScene(scene);
			});
		}
		else
		{
			_newGameMenuPresenter.Hide();
			StartScene(scene);
		}
		_view.PlayOnlineHostSubMenu.SetActive(false);
	}

	private void StartScene(int scene)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		FadeMode mode = FadeMode.Custom;
		if ((int)Game.State != 3 || !_customBgd)
		{
			mode = FadeMode.Solid;
		}
		Singleton<Fader>.Instance.FadeOut(3f, mode, () =>
		{
			LoadLevel(scene, (LoadSceneMode)1);
		});
	}

	private void CloseSession(DisconnectDetail detail)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsClient(Game.Mode))
		{
			World.PopWorld();
		}
		SetSceneRootsActive(3, value: false);
		PlayerUtilities.SetPlayerBundlesActive(active: false);
		PlayerRegistry.LocalPeer.ReplicatedDisconnect(detail, null, forceLocalShutdown: true);
		if (!BeamTeam.IsNullOrDestroyed(MultiplayerMng.Instance))
		{
			Object.DestroyImmediate((Object)(object)((Component)MultiplayerMng.Instance).gameObject);
		}
	}

	public static void SetSceneRootsActive(int sceneBuildIndex, bool value)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (BeamTeam.IsNullOrDestroyed(sceneAt) || sceneAt.buildIndex != sceneBuildIndex)
			{
				continue;
			}
			GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
			foreach (GameObject val in rootGameObjects)
			{
				if ((Object)(object)val != (Object)null)
				{
					val.SetActive(value);
				}
			}
		}
	}

	public void QuitToMenu(float fadeOutTime = 3f)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		NEW_RANDOM_WORLD_CREATED = false;
		_view.CanvasGroup.Interactable = false;
		_suspend = true;
		QuittingToMenu?.Invoke();
		CloseSession(DisconnectDetail.Intended);
		RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, false, 15);
		Singleton<Fader>.Instance.FadeOut(fadeOutTime, FadeMode.Solid, (Action)Game.QuitToMenu);
	}

	private void QuitToDesktop()
	{
		Game.QuitToDesktop();
	}

	public void LoadLevel(int level, LoadSceneMode mode = (LoadSceneMode)1)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		CursorUtils.LockState = (CursorLockMode)1;
		CursorUtils.Visible = false;
		LevelLoader.LoadLevel(level, mode, SceneLoad_Started, SceneLoad_Finished);
	}

	private void SceneLoad_Started()
	{
		Hide();
		_loadingScreenPresenter.Show();
	}

	private void SceneLoad_Finished()
	{
		_loadingScreenPresenter.Hide();
		_view.BackgroundTexture.Enabled = false;
		((Component)_view.BackgroundTexture.transform).gameObject.SetActive(false);
	}

	public void ShowJoinSessionDialog(Action onTimeout = null, EmptyEventHandler onSkip = null)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		RewiredUtilities.SetAndCacheMapsEnabled(_currentPlayerInput, state: false, 8);
		_joinSessionDialog = DialogueBox.Show(_currentPlayerInput, "NEW_GAME_MENU_LABEL_TITLE", "PLAY_ONLINE_MENU_CREATE_SESSION_CREATING", DialogueButton.None);
		if (onSkip != null)
		{
			_joinSessionDialog.Decline += onSkip;
		}
		_joinSessionDialogTimeout = Timing.RunCoroutine(TimeOut_Sequence(onTimeout));
	}

	private IEnumerator<float> TimeOut_Sequence(Action onTimeout)
	{
		yield return Timing.WaitForSeconds(30f);
		onTimeout?.Invoke();
	}

	public void HideJoinSessionDialog()
	{
		RewiredUtilities.SetAndCacheMapsEnabled(_currentPlayerInput, state: true, 8);
		if (Object.op_Implicit((Object)(object)_joinSessionDialog))
		{
			_joinSessionDialog.Destroy();
		}
		if (_joinSessionDialogTimeout.IsValid && _joinSessionDialogTimeout.IsRunning)
		{
			_joinSessionDialogTimeout.IsRunning = false;
		}
	}

	public void ShowNewGameMenuPresenter()
	{
		_newGameMenuPresenter.Show();
	}

	private void MPFriendlyQuit()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: false);
		if (MPExtensions.IsServer(Game.Mode))
		{
			new ReplicateQuitToMenu().Post();
			BeamTiming.WaitAnd(0.33f, () =>
			{
				QuitToMenu();
			});
		}
		else
		{
			QuitToMenu();
		}
	}

	public void ReplicatedStartGame(GameMode mode, GameState state, int scene)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected I4, but got Unknown
		if (MPExtensions.IsServer(mode))
		{
			ReplicateStartGame replicateStartGame = new ReplicateStartGame();
			replicateStartGame.Mode = (((int)mode == 4) ? 5 : 4);
			replicateStartGame.State = (int)state;
			replicateStartGame.Scene = scene;
			replicateStartGame.Post();
		}
		StartGame(mode, state, scene);
	}

	private void SetBackgroundTexture(Texture2D texture)
	{
		if (!((Component)_view.BackgroundTexture.transform).gameObject.activeSelf)
		{
			_view.BackgroundTexture.Enabled = true;
			((Component)_view.BackgroundTexture.transform).gameObject.SetActive(true);
		}
		_view.BackgroundTexture.Texture = (Texture)(object)texture;
		Singleton<Fader>.Instance.SetCustomTexture(texture);
	}

	private Texture2D LoadTextureBytes(string path)
	{
		if (!File.Exists(path))
		{
			return null;
		}
		byte[] array;
		using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
		{
			array = new byte[fileStream.Length];
			fileStream.Read(array, 0, (int)fileStream.Length);
		}
		return LoadTextureBytes(array);
	}

	private Texture2D LoadTextureBytes(byte[] bytes)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected Obj, but got Unknown
		if (bytes == null)
		{
			return null;
		}
		Texture2D val = new Texture2D(1, 1);
		if (ImageConversion.LoadImage(val, bytes))
		{
			return val;
		}
		return null;
	}

	protected bool IsContinueButtonAvailable()
	{
		if (!_view.ContinueButton.Visible)
		{
			return _view.ContinueCoopButton.Visible;
		}
		return true;
	}

	public async void CheckOnlineAvailable(Action onAvailable, Action onNotAvailableDialogueDismissed, Action onNotAvailable = null)
	{
		_view.CanvasGroup.Interactable = false;
		if (await NetworkConnectivity.CheckHttpConnectivityAsync() != 0)
		{
			_view.CanvasGroup.Interactable = true;
			onNotAvailable?.Invoke();
			DialogueBox.Show(_currentPlayerInput, "TEXT_ERROR", "PLAY_ONLINE_MENU_CREATE_SESSION_CREATING_ERROR_CONNECTING", DialogueButton.Ok, DialogueIcon.Warning).Accept += () =>
			{
				onNotAvailableDialogueDismissed?.Invoke();
			};
		}
		else
		{
			_view.CanvasGroup.Interactable = true;
			onAvailable?.Invoke();
		}
	}
}
