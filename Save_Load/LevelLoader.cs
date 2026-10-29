using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beam;
using Beam.Crafting;
using Beam.Events;
using Beam.Language;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Terrain;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using MEC;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour, ILoader
{
	private static LevelLoader _Instance;

	private bool _creating;

	private Stack<ILoader> _loaders = new Stack<ILoader>();

	private ILoader _currentLoader;

	private HashSet<int> _hashCodes = new HashSet<int>();

	private HashSet<Zone> _loadingZones = new HashSet<Zone>();

	private List<Action> _deferredLoading = new List<Action>();

	private List<Action> _postLoading = new List<Action>();

	private List<Action> _physicalLoading = new List<Action>();

	private Transform _loadingContainer;

	private CoroutineHandle _loadingSequence;

	private bool _isLoadingScene;

	private CoroutineHandle _serverJoinInProgress;

	private JObject _serverClientCopy;

	private CoroutineHandle _syncWorldCoroutine;

	private CoroutineHandle _syncWorldCoroutineSend;

	private CoroutineHandle _syncWorldCoroutineRecv;

	private CoroutineHandle _syncSaveCoroutine;

	private CoroutineHandle _syncSaveCoroutineSend;

	private CoroutineHandle _syncSaveCoroutineRecv;

	public static LevelLoader Instance
	{
		get
		{
			if (_Instance == null && !Game.IsQuitting)
			{
				_Instance = new GameObject("~LevelLoader").AddComponent<LevelLoader>();
				_Instance.transform.SetAsFirstSibling();
				_Instance.transform.position = Vector3.zero;
				_Instance.transform.rotation = Quaternion.identity;
			}
			return _Instance;
		}
	}

	public static bool IsCreatingOrLoading
	{
		get
		{
			if ((!Instance || !Instance._creating) && !IsLoading())
			{
				if ((bool)Singleton<StrandedWorld>.Instance && Singleton<StrandedWorld>.Instance.ZoneLoader != null)
				{
					return Singleton<StrandedWorld>.Instance.ZoneLoader.IsGenerating;
				}
				return false;
			}
			return true;
		}
	}

	public static ILoader CurrentLoader
	{
		get
		{
			return Instance._currentLoader;
		}
		set
		{
			Instance.SetCurrentLoader(value);
		}
	}

	bool ILoader.IsLoading { get; set; }

	public ICollection<int> HashCodes => _hashCodes;

	public ICollection<Zone> LoadingZones => _loadingZones;

	public IList<Action> DeferredLoading => _deferredLoading;

	public IList<Action> PostLoading => _postLoading;

	public IList<Action> PhysicalLoading => _physicalLoading;

	public Transform LoadingContainer => _loadingContainer;

	public JObject ServerClientCopy
	{
		get
		{
			return _serverClientCopy;
		}
		set
		{
			_serverClientCopy = value;
		}
	}

	public static bool IsServerJoinInProgress => Instance?._serverJoinInProgress.IsRunning ?? false;

	public static bool IsLoading()
	{
		return CurrentLoader?.IsLoading ?? false;
	}

	public static bool IsLoading(GameObject gameObject)
	{
		if (IsLoading())
		{
			return Instance.HashCodes.Contains(gameObject.GetHashCode());
		}
		return false;
	}

	public static bool IsLoadingScene()
	{
		return Instance._isLoadingScene;
	}

	private void SetCurrentLoader(ILoader value)
	{
		if (value == null)
		{
			ILoader currentLoader = null;
			if (_loaders.Count > 0)
			{
				currentLoader = _loaders.Pop();
			}
			_currentLoader = currentLoader;
		}
		else
		{
			if (!_currentLoader.IsNullOrDestroyed())
			{
				_loaders.Push(_currentLoader);
			}
			_currentLoader = value;
		}
	}

	public static bool IsPlayable()
	{
		if (!IsLoading())
		{
			return PlayerUtilities.AllPlayersPresent();
		}
		return false;
	}

	public static bool IsClientJoinInProgress()
	{
		if (Game.Mode.IsClient())
		{
			return MultiplayerMng.ServerGameState.IsGame();
		}
		return false;
	}

	private void Awake()
	{
		if ((bool)_Instance)
		{
			BeamTeam.Destroy(base.gameObject);
			return;
		}
		_Instance = this;
		Debug.Log(string.Format("{0}:: Created instance, #{1}", "LevelLoader", GetHashCode()));
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		_loadingContainer = new GameObject("LoadingContainer").transform;
		_loadingContainer.gameObject.SetActive(value: false);
		_loadingContainer.parent = _Instance.transform;
		_loadingContainer.transform.position = Vector3.zero;
		_loadingContainer.transform.rotation = Quaternion.identity;
		EventManager.AddListener<PlayerConnectedEvent>(OnConnect);
		EventManager.AddListener<PlayerDisconnectedEvent>(OnDisconnect);
		SceneManager.sceneLoaded += SceneManager_SceneLoaded;
	}

	private void OnDestroy()
	{
		_loaders.Clear();
		_currentLoader = null;
		BeamTeam.Destroy(_loadingContainer.gameObject);
		EventManager.RemoveListener<PlayerConnectedEvent>(OnConnect);
		EventManager.RemoveListener<PlayerDisconnectedEvent>(OnDisconnect);
		SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
	}

	private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex == 1)
		{
			Debug.Log("LevelLoader:: Destroying instance...");
			BeamTeam.Destroy(base.gameObject);
		}
	}

	private void OnConnect(PlayerConnectedEvent @event)
	{
		if (!Game.State.IsMenu() && Game.Mode.IsServer() && @event.PlayerId == 1 && !_loadingSequence.IsRunning)
		{
			_serverJoinInProgress = Timing.RunCoroutine(ServerJoinInProgress());
		}
	}

	private void OnDisconnect(PlayerDisconnectedEvent @event)
	{
		if (!Game.State.IsMenu())
		{
			Debug.LogError($"OnDisconnect {@event.PlayerId} {Game.State} Loading: {_loadingSequence.IsRunning} Join {_serverJoinInProgress.IsRunning}");
			if (Game.Mode.IsServer() && IsInNormalGameplay())
			{
				UpdateServerClientCopy(PlayerUtilities.GetPlayerFromId(1));
			}
			Timing.RunCoroutine(OnDisconnect_Sequence(@event));
		}
	}

	private bool IsInNormalGameplay()
	{
		if (!_serverJoinInProgress.IsRunning)
		{
			return !_loadingSequence.IsRunning;
		}
		return false;
	}

	private IEnumerator<float> OnDisconnect_Sequence(PlayerDisconnectedEvent @event)
	{
		yield return 0f;
		if (Game.Mode.IsServer() && @event.PlayerId == 1)
		{
			if (_serverJoinInProgress.IsRunning)
			{
				_serverJoinInProgress.IsRunning = false;
				StopSync();
			}
			else if (_loadingSequence.IsRunning)
			{
				StopSync();
			}
		}
		bool hostDisconnected = @event.PlayerId == 0;
		if (hostDisconnected)
		{
			Timing.KillCoroutines();
		}
		Rewired.Player player = ReInput.players.GetPlayer(0);
		if (player.IsValid() && @event.TryGetDisconnectDialog(out var title, out var content))
		{
			player.SetAndCacheMapsEnabled(state: false, 0);
			player.SetAndCacheMapsEnabled(state: true, 16);
			player.SetCursorEnabled(enabled: true);
			DialogueBox.Show(player, title, content, DialogueButton.Ok, DialogueIcon.Warning).Accept += delegate
			{
				if (hostDisconnected)
				{
					StartCoroutine(QuitToMenu_Sequence());
				}
			};
		}
		else if (hostDisconnected)
		{
			StartCoroutine(QuitToMenu_Sequence());
		}
	}

	private void StopSync()
	{
		_syncSaveCoroutine.IsRunning = false;
		_syncSaveCoroutineSend.IsRunning = false;
		_syncSaveCoroutineRecv.IsRunning = false;
		_syncWorldCoroutine.IsRunning = false;
		_syncWorldCoroutineSend.IsRunning = false;
		_syncWorldCoroutineRecv.IsRunning = false;
	}

	public void ShutdownToMainMenu()
	{
		Timing.KillCoroutines();
		StartCoroutine(QuitToMenu_Sequence());
	}

	private IEnumerator QuitToMenu_Sequence()
	{
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_CLEANING_UP"));
		yield return 0f;
		MainMenuPresenter.Instance.QuitToMenu();
	}

	public void UpdateServerClientCopy(IPlayer client)
	{
		if (client.IsNullOrDestroyed())
		{
			return;
		}
		try
		{
			_serverClientCopy = new JObject().Set(string.Format("Persistent/{0}/#{1}", "PlayerRegistry", client.Id), SavingUtilities.Save(client).Clear("Movement"));
		}
		catch (Exception ex)
		{
			Debug.LogError("Error in UpdateServerClientCopy\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void PatchServerClientCopy(JObject save)
	{
		if (!Game.Mode.IsClient() && !_serverClientCopy.IsNullOrDestroyed() && !save.IsNullOrDestroyed())
		{
			string path = "Persistent/PlayerRegistry/#1";
			if (!save.Get(path).IsValid())
			{
				save.Clear(path).ApplyPatch(_serverClientCopy);
			}
		}
	}

	private IEnumerator<float> ServerJoinInProgress()
	{
		yield return BeamTiming.WaitForPeer(1, 10f);
		if (PlayerUtilities.GetPeerFromId(1).IsNullOrDestroyed())
		{
			yield break;
		}
		EventManager.RaiseEvent(new ServerJoinInProgressStarted());
		EventManager.EventDelegate<PlayerDisconnectedEvent> onDisconnect = null;
		onDisconnect = delegate(PlayerDisconnectedEvent @event)
		{
			if (@event.PlayerId == 1)
			{
				EventManager.RemoveListener(onDisconnect);
				Singleton<GameTime>.Instance.Paused = false;
				Physics.autoSimulation = true;
				--PlayerRegistry.LocalPlayer.Movement.IsBusy;
				PlayerRegistry.LocalPlayer.Input.SetAndCacheMapsEnabled(state: true, PlayerRegistry.LocalPlayer.Input.GetCurrentMapId());
				PlayerRegistry.LocalPlayer.PlayerUI.Refresh(RefreshUIMode.Clear);
				MainMenuPresenter.Instance.LoadingScreenPresenter.Hide();
				Singleton<Fader>.Instance.FadeIn(0.1f);
				AudioListener.volume = 1f;
				PlayerRegistry.LocalPeer.SetTimeout(Peer.GetDefaultTimeout());
				MultiplayerWorldSync.Instance?.Reset();
				EventManager.RaiseEvent(new ServerJoinInProgressEnded(PlayerRegistry.IsClientPeerValid));
			}
		};
		while (IsLoading())
		{
			yield return 0f;
		}
		EventManager.AddListener(onDisconnect);
		Singleton<GameTime>.Instance.Paused = true;
		Physics.autoSimulation = false;
		Singleton<Fader>.Instance.FadeOut(0.1f);
		AudioListener.volume = 0f;
		MainMenuPresenter.Instance.LoadingScreenPresenter.Show();
		++PlayerRegistry.LocalPlayer.Movement.IsBusy;
		yield return PlayerRegistry.LocalPeer.SetTimeout_Async(Peer.GetSessionTimeout());
		JObject save = new JObject();
		JObject persistent = new JObject();
		bool succeeded = true;
		yield return Timing.WaitUntilDone(Timing.Instance.RunCoroutine(SaveManager.GenerateSave_Sequence(save, persistent), delegate(Exception ex)
		{
			if (ex != null)
			{
				succeeded = false;
				Debug.LogException(ex);
			}
		}));
		if (succeeded)
		{
			yield return Timing.WaitUntilDone(SyncSave_Async(delegate(JObject saveData)
			{
				persistent = SaveManager.GetPersistentData(saveData);
			}, save));
			yield return Timing.WaitUntilDone(SyncWorld_Async(serverLoadWorld: false));
			yield return BeamTiming.WaitForAllPlayers();
			PlayerRegistry.InitializeReplicants();
			yield return Timing.WaitUntilDone(Timing.RunCoroutine(ServerJoinInProgress_LoadPlayerReplicant(persistent)));
			yield return Timing.WaitUntilDone(MultiplayerFence.Sync(Fence.FinishLoading));
			onDisconnect(new PlayerDisconnectedEvent(1, null));
		}
	}

	private IEnumerator<float> ServerJoinInProgress_LoadPlayerReplicant(JObject persistent)
	{
		IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(1);
		if (playerFromId.IsNullOrDestroyed())
		{
			yield break;
		}
		StorageLoader storageLoader = new StorageLoader
		{
			IsLoading = true
		};
		CurrentLoader = storageLoader;
		if (persistent.IsValid())
		{
			JObject jObject = persistent.Get("PlayerRegistry");
			if (jObject.IsValid() && jObject.Children.IsValid() && jObject.Children.Count > 1)
			{
				JObject jObject2 = jObject.Children[1];
				if (jObject2.IsValid() && !jObject2.IsNull())
				{
					SavingUtilities.Load(playerFromId, jObject2);
				}
			}
		}
		Zone zone = StrandedWorld.GetZone(PlayerRegistry.LocalPlayer.transform.position);
		int numLoadedZones = (zone ? 1 : 0);
		if (!persistent.Get("StrandedWorld/Zones").GetZoneData(Singleton<StrandedWorld>.Instance.NmlZone.Id).GetField("Objects")
			.IsNull())
		{
			numLoadedZones++;
		}
		for (int i = 0; i < numLoadedZones; i++)
		{
			yield return Timing.WaitUntilDone(MultiplayerFence.Sync(Fence.ProcessLoading));
		}
		for (int j = 0; j < storageLoader.DeferredLoading.Count; j++)
		{
			LoadingActionUtilities.ExecuteDeferredAction(storageLoader.DeferredLoading[j]);
		}
		for (int k = 0; k < storageLoader.PostLoading.Count; k++)
		{
			LoadingActionUtilities.ExecuteDeferredAction(storageLoader.PostLoading[k]);
		}
		for (int l = 0; l < storageLoader.PhysicalLoading.Count; l++)
		{
			LoadingActionUtilities.ExecuteDeferredAction(storageLoader.PhysicalLoading[l]);
		}
		storageLoader.IsLoading = false;
		CurrentLoader = null;
	}

	private IEnumerator<float> ClientActivateEntities()
	{
		yield return BeamTiming.WaitForPeer(0);
		MultiplayerMng.ReceivedEntitiesActive = true;
		yield return Timing.WaitForSeconds(1f);
	}

	private CoroutineHandle SyncWorld_Async(bool serverLoadWorld = true)
	{
		return _syncWorldCoroutine = Timing.RunCoroutine(SyncWorld_Sequence(serverLoadWorld));
	}

	private IEnumerator<float> SyncWorld_Sequence(bool serverLoadWorld = true)
	{
		if (Game.Mode.IsServer() && serverLoadWorld)
		{
			LoadingResult loadingResult = World.LoadWorld();
			if (loadingResult.Succeeded)
			{
				Singleton<StrandedWorld>.Instance.ApplyWorldToZones();
			}
			else
			{
				Debug.LogError("StrandedWorld:: Error loading world: " + loadingResult.Message);
			}
			PlayerRegistry.ServerPeer.WorldSeed = StrandedWorld.LOCAL_WORLD_SEED;
		}
		if (Game.Mode.IsServer() && PlayerRegistry.IsClientPeerValid)
		{
			yield return MultiplayerWorldSync.Instance.SendWorld_Async(World.CurrentWorld, delegate(CoroutineHandle coroutine)
			{
				_syncWorldCoroutineSend = coroutine;
			}, delegate(float ratio)
			{
				EventManager.RaiseEvent(new LoadingProgressChangedEvent(string.Format("{0} [{1}%]", "LOADING_PROGRESS_SYNC_WORLD", (int)((double)ratio * 100.0))));
			}, delegate
			{
			}, ShutdownToMainMenu);
		}
		if (Game.Mode.IsClient())
		{
			yield return MultiplayerWorldSync.Instance.ReceiveWorld_Async(delegate(CoroutineHandle coroutine)
			{
				_syncWorldCoroutineRecv = coroutine;
			}, delegate(float ratio)
			{
				EventManager.RaiseEvent(new LoadingProgressChangedEvent(string.Format("{0} [{1}%]", "LOADING_PROGRESS_SYNC_WORLD", (int)((double)ratio * 100.0))));
			}, delegate(World.Bundle world)
			{
				World.PushWorld(world);
				Singleton<StrandedWorld>.Instance.ApplyWorldToZones();
			}, ShutdownToMainMenu);
		}
	}

	private CoroutineHandle SyncSave_Async(Action<JObject> onSaveFile, JObject saveFile = null)
	{
		return _syncSaveCoroutine = Timing.RunCoroutine(SyncSave_Sequence(onSaveFile, saveFile));
	}

	private IEnumerator<float> SyncSave_Sequence(Action<JObject> onSaveFile, JObject saveFile)
	{
		if (Game.Mode.IsServer())
		{
			saveFile = saveFile ?? (SaveManager.ValidateSaveFile(Game.Mode) ? SaveManager.LoadSaveFile(Game.Mode) : null);
			if (PlayerRegistry.IsClientPeerValid)
			{
				yield return MultiplayerWorldSync.Instance.SendSave_Async(saveFile, delegate(CoroutineHandle coroutine)
				{
					_syncSaveCoroutineSend = coroutine;
				}, delegate(float ratio)
				{
					EventManager.RaiseEvent(new LoadingProgressChangedEvent(string.Format("{0} [{1}%]", "LOADING_PROGRESS_SYNC_SAVE", (int)((double)ratio * 100.0))));
				}, onSaveFile, ShutdownToMainMenu);
			}
			else
			{
				onSaveFile?.Invoke(saveFile);
			}
		}
		if (Game.Mode.IsClient())
		{
			yield return MultiplayerWorldSync.Instance.ReceiveSave_Async(delegate(CoroutineHandle coroutine)
			{
				_syncSaveCoroutineRecv = coroutine;
			}, delegate(float ratio)
			{
				EventManager.RaiseEvent(new LoadingProgressChangedEvent(string.Format("{0} [{1}%]", "LOADING_PROGRESS_SYNC_SAVE", (int)((double)ratio * 100.0))));
			}, onSaveFile, ShutdownToMainMenu);
		}
	}

	public static void LoadLevel(int level, LoadSceneMode mode, Action OnStart, Action OnComplete)
	{
		Instance.Load(level, mode, OnStart, OnComplete);
	}

	public void Load(int level, LoadSceneMode mode, Action OnStart, Action OnComplete)
	{
		Timing.KillCoroutines(_loadingSequence);
		_loadingSequence = Timing.RunCoroutine(LoadScene_Sequence(level, mode, OnStart, OnComplete));
	}

	private IEnumerator<float> LoadScene_Sequence(int level, LoadSceneMode mode, Action OnStart, Action OnComplete)
	{
		_serverClientCopy = null;
		_isLoadingScene = true;
		if (Game.State == GameState.LOAD_GAME)
		{
			CurrentLoader = this;
			CurrentLoader.IsLoading = true;
		}
		_creating = Game.State == GameState.NEW_GAME;
		OnStart?.Invoke();
		RewiredUtilities.Players.SetAndCacheMapsEnabled(state: false, 0);
		AudioListener.volume = 0f;
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_INITIALIZING_OBJECTS"));
		yield return 0f;
		float timeout = Peer.GetDefaultTimeout();
		if (Game.Mode.IsMultiplayer())
		{
			if (Game.Mode.IsServer())
			{
				MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: true);
			}
			else
			{
				MainMenuPresenter.Instance.LoadingScreenPresenter.Show("TEXT_SCREEN_LABEL_JOINING");
			}
			timeout = PlayerRegistry.LocalPeer.Timeout;
			yield return PlayerRegistry.LocalPeer.SetTimeout_Async(Peer.GetSessionTimeout());
		}
		Prefabs.Initialize();
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_LOADING_SCENE"));
		yield return 0f;
		AsyncOperation async = SceneManager.LoadSceneAsync(level, mode);
		async.allowSceneActivation = false;
		bool loaded = false;
		JObject rootData = null;
		while (!async.isDone)
		{
			if (async.progress >= 0.9f)
			{
				if (Game.State == GameState.LOAD_GAME)
				{
					if (Game.Mode.IsMultiplayer())
					{
						if (Game.State == GameState.LOAD_GAME && !loaded)
						{
							yield return Timing.WaitUntilDone(SyncSave_Async(delegate(JObject save)
							{
								rootData = save;
								loaded = true;
								async.allowSceneActivation = true;
							}));
						}
					}
					else
					{
						if (Game.State == GameState.LOAD_GAME && !loaded)
						{
							rootData = SaveManager.LoadSaveFile(Game.Mode);
							loaded = true;
						}
						async.allowSceneActivation = true;
					}
				}
				else
				{
					async.allowSceneActivation = true;
				}
			}
			yield return 0f;
		}
		Singleton<GameTime>.Instance.Paused = true;
		if ((Game.State == GameState.NEW_GAME || Game.State == GameState.LOAD_GAME) && Game.Mode.IsClient())
		{
			Singleton<GameTime>.Instance.PostReplicateTick();
		}
		if ((bool)SceneCallbacks.Instance)
		{
			yield return Timing.WaitUntilDone(SceneCallbacks.Instance.Level_SceneLoaded(level));
		}
		foreach (IGameCamera allCamera in Cameras.AllCameras)
		{
			allCamera.gameObject.SetActive(value: false);
		}
		if (Game.State == GameState.INTRO)
		{
			PrivateJet_Assetloader privateJet_Assetloader = UnityEngine.Object.FindObjectOfType<PrivateJet_Assetloader>();
			if (privateJet_Assetloader != null)
			{
				yield return privateJet_Assetloader.Load_Async();
			}
		}
		else if (Game.State == GameState.NEW_GAME)
		{
			if (Game.Mode.IsMultiplayer())
			{
				yield return Timing.WaitUntilDone(SyncWorld_Async());
			}
			yield return CreateGame_Async();
		}
		else if (Game.State == GameState.LOAD_GAME)
		{
			if (Game.Mode.IsMultiplayer())
			{
				yield return Timing.WaitUntilDone(SyncWorld_Async());
			}
			yield return LoadGame_Async(rootData);
		}
		else if (Game.State == GameState.MAP_EDITOR)
		{
			AudioListener.volume = 1f;
			yield break;
		}
		if (Game.State.IsGame() && Game.Mode.IsClient())
		{
			yield return Timing.WaitUntilDone(Timing.RunCoroutine(ClientActivateEntities()));
		}
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_UNLOADING_ASSETS"));
		yield return 0f;
		Resources.UnloadUnusedAssets();
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_CLEANING_UP"));
		yield return 0f;
		GC.Collect();
		OnComplete?.Invoke();
		if (Game.State == GameState.LOAD_GAME)
		{
			CurrentLoader.DeferredLoading.Clear();
			CurrentLoader.PostLoading.Clear();
			CurrentLoader.PhysicalLoading.Clear();
			CurrentLoader.IsLoading = false;
			CurrentLoader = null;
		}
		_creating = false;
		_isLoadingScene = false;
		foreach (IGameCamera allCamera2 in Cameras.AllCameras)
		{
			allCamera2.gameObject.SetActive(value: true);
		}
		FollowSpawn[] spawners = UnityEngine.Object.FindObjectsOfType<FollowSpawn>();
		bool generating;
		do
		{
			generating = false;
			FollowSpawn[] array = spawners;
			foreach (FollowSpawn followSpawn in array)
			{
				generating |= followSpawn.IsGenerating;
			}
			if (generating)
			{
				yield return 0f;
			}
		}
		while (generating);
		yield return Timing.WaitForSeconds(2f);
		yield return Timing.WaitUntilDone(MultiplayerFence.Sync(Fence.FinishLoading));
		if (Game.State == GameState.NEW_GAME || Game.State == GameState.LOAD_GAME)
		{
			if (Game.Mode.IsServer())
			{
				Singleton<GameTime>.Instance.ReplicateTickRepeating();
			}
			Singleton<GameTime>.Instance.Paused = false;
			Singleton<Fader>.Instance.ResetFade(FadeMode.Solid, FadeDirection.In);
			Singleton<Fader>.Instance.SetFadeColor(Color.black);
			Singleton<Fader>.Instance.SetFadeEasing(EasingType.Linear);
			yield return Timing.WaitForSeconds(1f);
			TMPTextScreenViewAdapter textScreen = UnityEngine.Object.FindObjectOfType<TMPTextScreenViewAdapter>();
			string headerText = string.Format("{0} {1}", Localization.GetLanguageHandler().Localize("MAIN_MENU_LABEL_DAY"), Singleton<GameCalendar>.Instance.DaysAlive);
			textScreen.SetHeaderText(headerText);
			textScreen.SetSubheaderText(string.Empty);
			textScreen.SetText(string.Empty);
			textScreen.SetTooltipText(string.Empty);
			textScreen.Show();
			yield return Timing.WaitForSeconds(IsClientJoinInProgress() ? 2f : 4f);
			textScreen.Hide();
		}
		Singleton<Fader>.Instance.FadeIn(2.7f);
		float timer = 0f;
		while (timer < 2.7f)
		{
			timer += Time.unscaledDeltaTime;
			AudioListener.volume += 10f / 27f * Time.unscaledDeltaTime;
			yield return 0f;
		}
		AudioListener.volume = 1f;
		Singleton<Fader>.Instance.ResetColor();
		Singleton<Fader>.Instance.ResetEase();
		if (Game.State == GameState.NEW_GAME || Game.State == GameState.LOAD_GAME)
		{
			for (int j = 0; j < PlayerRegistry.AllPlayers.Count; j++)
			{
				PlayerRegistry.AllPlayers[j].Movement.IsBusy = default(Condition);
			}
			RewiredUtilities.Players.SetAndCacheMapsEnabled(state: false, 9);
			RewiredUtilities.Players.SetAndCacheMapsEnabled(state: true, 0);
			if (Game.Mode.IsServer())
			{
				MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: false);
			}
		}
		else if (Game.State == GameState.INTRO)
		{
			RewiredUtilities.Players.SetAndCacheMapsEnabled(state: true, 9);
		}
		if (Game.Mode.IsMultiplayer())
		{
			PlayerRegistry.LocalPeer.SetTimeout(timeout);
		}
	}

	private float CreateGame_Async()
	{
		return Timing.WaitUntilDone(CreateGame());
	}

	private float LoadGame_Async(JObject rootData)
	{
		return Timing.WaitUntilDone(LoadGame(rootData));
	}

	private CoroutineHandle CreateGame()
	{
		return Timing.RunCoroutine(CreateGame_Sequence());
	}

	private CoroutineHandle LoadGame(JObject rootData)
	{
		return Timing.RunCoroutine(LoadGame_Sequence(rootData));
	}

	private IEnumerator<float> CreateGame_Sequence()
	{
		yield return Singleton<StrandedWorld>.Instance.CreateWorld_Async();
	}

	private IEnumerator<float> LoadGame_Sequence(JObject rootData)
	{
		JObject persistentData = SaveManager.GetPersistentData(rootData);
		Vector3 originalP1position = Vector3.zero;
		if (persistentData != null && !persistentData.IsNull())
		{
			JObject field = persistentData.GetField("GameTime");
			SavingUtilities.Load(Singleton<GameTime>.Instance, field);
			JObject field2 = persistentData.GetField("GameCalendar");
			SavingUtilities.Load(Singleton<GameCalendar>.Instance, field2);
			Physics.autoSimulation = false;
			float fixedDeltaTime = Time.fixedDeltaTime;
			if (!Game.Mode.IsMultiplayer())
			{
				Time.fixedDeltaTime = 10f;
			}
			LegacyUtilities.LoadPlayer(persistentData);
			IPlayer player = PlayerRegistry.AllPlayers[0];
			player.Character.RefreshPosition();
			player.Character.RefreshRotation();
			player.Character.SetCameraMode(player.PlayerCamera.CameraMode);
			player.PlayerCamera.CameraMovement.SetPosition(player.Character.CameraRigTransform);
			player.PlayerCamera.CameraMovement.SetRotation(player.Character.CameraRigTransform);
			player.PlayerCamera.UpdateCameraPosition(force: true);
			originalP1position = player.transform.position;
			JObject field3 = persistentData.GetField("StrandedWorld");
			yield return Singleton<StrandedWorld>.Instance.LoadWorld_Async(rootData, field3);
			(from behaviour in UnityEngine.Object.FindObjectsOfType<MonoBehaviour>()
				where behaviour is IPersistentSaveable && !(behaviour is GameTime) && !(behaviour is GameCalendar) && !(behaviour is StrandedWorld)
				select behaviour).Cast<IPersistentSaveable>().ToList().ForEach(delegate(IPersistentSaveable saveableObject)
			{
				string text = saveableObject.GetType().Name;
				JObject field4 = persistentData.GetField(text);
				if (field4 != null)
				{
					SavingUtilities.Load(saveableObject, field4);
				}
				else
				{
					Debug.LogWarning("LevelLoader:: No data was saved for persistent object [" + text + "].");
				}
			});
			for (int i = 0; i < DeferredLoading.Count; i++)
			{
				LoadingActionUtilities.ExecuteDeferredAction(DeferredLoading[i]);
			}
			for (int j = 0; j < PostLoading.Count; j++)
			{
				LoadingActionUtilities.ExecuteDeferredAction(PostLoading[j]);
			}
			if (!Game.Mode.IsMultiplayer())
			{
				Time.fixedDeltaTime = fixedDeltaTime;
			}
			Physics.autoSimulation = true;
		}
		else
		{
			Debug.LogError("LevelLoader:: [Persistent] node is null or empty.");
		}
		if (!Game.State.IsGame() || !Game.Mode.IsClient() || !(originalP1position != Vector3.zero))
		{
			yield break;
		}
		IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(1);
		LayerMask physicsLayerMask = GetPhysicsLayerMask(Layers.PLAYER);
		physicsLayerMask = (int)physicsLayerMask ^ (1 << Layers.PLAYER);
		if (!Physics.CheckSphere(playerFromId.transform.position, 0.7f, physicsLayerMask))
		{
			yield break;
		}
		Vector3 vector = Vector3.zero;
		bool flag = false;
		float num = 0f;
		int num2 = 180;
		for (int k = 0; k < num2; k++)
		{
			float x = Mathf.Sin(num);
			float z = Mathf.Cos(num);
			num += MathF.PI * 2f / (float)num2;
			vector = new Vector3(x, 0f, z);
			float radius = 0.25f;
			if (!Physics.CheckSphere(originalP1position + vector, radius, physicsLayerMask) && !Physics.Raycast(originalP1position + vector, originalP1position - (originalP1position + vector), vector.magnitude, physicsLayerMask))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			playerFromId.transform.position = originalP1position + vector;
		}
	}

	private LayerMask GetPhysicsLayerMask(int currentLayer)
	{
		int num = 0;
		for (int i = 0; i < 32; i++)
		{
			if (!Physics.GetIgnoreLayerCollision(currentLayer, i))
			{
				num |= 1 << i;
			}
		}
		return num;
	}
}
