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
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_Instance == (Object)null && !Game.IsQuitting)
			{
				_Instance = new GameObject("~LevelLoader").AddComponent<LevelLoader>();
				((Component)_Instance).transform.SetAsFirstSibling();
				((Component)_Instance).transform.position = Vector3.zero;
				((Component)_Instance).transform.rotation = Quaternion.identity;
			}
			return _Instance;
		}
	}

	public static bool IsCreatingOrLoading
	{
		get
		{
			if ((!Object.op_Implicit((Object)(object)Instance) || !Instance._creating) && !IsLoading())
			{
				if (Object.op_Implicit((Object)(object)Singleton<StrandedWorld>.Instance) && Singleton<StrandedWorld>.Instance.ZoneLoader != null)
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

	public static bool IsServerJoinInProgress
	{
		get
		{
			LevelLoader instance = Instance;
			if (instance == null)
			{
				return false;
			}
			return instance._serverJoinInProgress.IsRunning;
		}
	}

	public static bool IsLoading()
	{
		return CurrentLoader?.IsLoading ?? false;
	}

	public static bool IsLoading(GameObject gameObject)
	{
		if (IsLoading())
		{
			return Instance.HashCodes.Contains(((object)gameObject).GetHashCode());
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
			if (!BeamTeam.IsNullOrDestroyed(_currentLoader))
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
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsClient(Game.Mode))
		{
			return MPExtensions.IsGame(MultiplayerMng.ServerGameState);
		}
		return false;
	}

	private void Awake()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)_Instance))
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
			return;
		}
		_Instance = this;
		Debug.Log((object)string.Format("{0}:: Created instance, #{1}", "LevelLoader", ((object)this).GetHashCode()));
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		_loadingContainer = new GameObject("LoadingContainer").transform;
		((Component)_loadingContainer).gameObject.SetActive(false);
		_loadingContainer.parent = ((Component)_Instance).transform;
		((Component)_loadingContainer).transform.position = Vector3.zero;
		((Component)_loadingContainer).transform.rotation = Quaternion.identity;
		EventManager.AddListener<PlayerConnectedEvent>(OnConnect);
		EventManager.AddListener<PlayerDisconnectedEvent>(OnDisconnect);
		SceneManager.sceneLoaded += SceneManager_SceneLoaded;
	}

	private void OnDestroy()
	{
		_loaders.Clear();
		_currentLoader = null;
		BeamTeam.Destroy<GameObject>(((Component)_loadingContainer).gameObject);
		EventManager.RemoveListener<PlayerConnectedEvent>(OnConnect);
		EventManager.RemoveListener<PlayerDisconnectedEvent>(OnDisconnect);
		SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
	}

	private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex == 1)
		{
			Debug.Log((object)"LevelLoader:: Destroying instance...");
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
		}
	}

	private void OnConnect(PlayerConnectedEvent @event)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsMenu(Game.State) && MPExtensions.IsServer(Game.Mode) && @event.PlayerId == 1 && !_loadingSequence.IsRunning)
		{
			_serverJoinInProgress = Timing.RunCoroutine(ServerJoinInProgress());
		}
	}

	private void OnDisconnect(PlayerDisconnectedEvent @event)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsMenu(Game.State))
		{
			Debug.LogError((object)string.Format("OnDisconnect {0} {1} Loading: {2} Join {3}", new object[4]
			{
				@event.PlayerId,
				Game.State,
				_loadingSequence.IsRunning,
				_serverJoinInProgress.IsRunning
			}));
			if (MPExtensions.IsServer(Game.Mode) && IsInNormalGameplay())
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
		if (MPExtensions.IsServer(Game.Mode) && @event.PlayerId == 1)
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
		Player player = ReInput.players.GetPlayer(0);
		if (BeamTeam.IsValid(player) && @event.TryGetDisconnectDialog(out var title, out var content))
		{
			RewiredUtilities.SetAndCacheMapsEnabled(player, state: false, 0);
			RewiredUtilities.SetAndCacheMapsEnabled(player, state: true, 16);
			RewiredUtilities.SetCursorEnabled(player, enabled: true);
			DialogueBox.Show(player, title, content, DialogueButton.Ok, DialogueIcon.Warning).Accept += () =>
			{
				if (hostDisconnected)
				{
					((MonoBehaviour)this).StartCoroutine(QuitToMenu_Sequence());
				}
			};
		}
		else if (hostDisconnected)
		{
			((MonoBehaviour)this).StartCoroutine(QuitToMenu_Sequence());
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
		((MonoBehaviour)this).StartCoroutine(QuitToMenu_Sequence());
	}

	private IEnumerator QuitToMenu_Sequence()
	{
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_CLEANING_UP"));
		yield return 0f;
		MainMenuPresenter.Instance.QuitToMenu();
	}

	public void UpdateServerClientCopy(IPlayer client)
	{
		if (BeamTeam.IsNullOrDestroyed(client))
		{
			return;
		}
		try
		{
			_serverClientCopy = new JObject().Set(string.Format("Persistent/{0}/#{1}", "PlayerRegistry", client.Id), SavingUtilities.Save(client).Clear("Movement"));
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Error in UpdateServerClientCopy\n" + ex.Message + "\n" + ex.StackTrace));
		}
	}

	public void PatchServerClientCopy(JObject save)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsClient(Game.Mode) && !BeamTeam.IsNullOrDestroyed(_serverClientCopy) && !BeamTeam.IsNullOrDestroyed(save))
		{
			string path = "Persistent/PlayerRegistry/#1";
			if (!BeamTeam.IsValid(save.Get(path)))
			{
				save.Clear(path).ApplyPatch(_serverClientCopy);
			}
		}
	}

	private IEnumerator<float> ServerJoinInProgress()
	{
		yield return BeamTiming.WaitForPeer(1, 10f);
		if (BeamTeam.IsNullOrDestroyed(PlayerUtilities.GetPeerFromId(1)))
		{
			yield break;
		}
		EventManager.RaiseEvent(new ServerJoinInProgressStarted());
		EventManager.EventDelegate<PlayerDisconnectedEvent> onDisconnect = null;
		onDisconnect = (PlayerDisconnectedEvent @event) =>
		{
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			if (@event.PlayerId == 1)
			{
				EventManager.RemoveListener(onDisconnect);
				Singleton<GameTime>.Instance.Paused = false;
				Physics.autoSimulation = true;
				--PlayerRegistry.LocalPlayer.Movement.IsBusy;
				RewiredUtilities.SetAndCacheMapsEnabled(PlayerRegistry.LocalPlayer.Input, state: true, RewiredUtilities.GetCurrentMapId(PlayerRegistry.LocalPlayer.Input));
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
		yield return Timing.WaitUntilDone(BeamTeam.RunCoroutine(Timing.Instance, SaveManager.GenerateSave_Sequence(save, persistent), (Exception ex) =>
		{
			if (ex != null)
			{
				succeeded = false;
				Debug.LogException(ex);
			}
		}));
		if (succeeded)
		{
			yield return Timing.WaitUntilDone(SyncSave_Async((JObject saveData) =>
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
		if (BeamTeam.IsNullOrDestroyed(playerFromId))
		{
			yield break;
		}
		StorageLoader storageLoader = new StorageLoader
		{
			IsLoading = true
		};
		CurrentLoader = storageLoader;
		if (BeamTeam.IsValid(persistent))
		{
			JObject jObject = persistent.Get("PlayerRegistry");
			if (BeamTeam.IsValid(jObject) && BeamTeam.IsValid(jObject.Children) && jObject.Children.Count > 1)
			{
				JObject jObject2 = jObject.Children[1];
				if (BeamTeam.IsValid(jObject2) && !jObject2.IsNull())
				{
					SavingUtilities.Load(playerFromId, jObject2);
				}
			}
		}
		Zone zone = StrandedWorld.GetZone(PlayerRegistry.LocalPlayer.transform.position);
		int numLoadedZones = (Object.op_Implicit((Object)(object)zone) ? 1 : 0);
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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return _syncWorldCoroutine = Timing.RunCoroutine(SyncWorld_Sequence(serverLoadWorld));
	}

	private IEnumerator<float> SyncWorld_Sequence(bool serverLoadWorld = true)
	{
		if (MPExtensions.IsServer(Game.Mode) & serverLoadWorld)
		{
			LoadingResult loadingResult = World.LoadWorld();
			if (loadingResult.Succeeded)
			{
				Singleton<StrandedWorld>.Instance.ApplyWorldToZones();
			}
			else
			{
				Debug.LogError((object)("StrandedWorld:: Error loading world: " + loadingResult.Message));
			}
			PlayerRegistry.ServerPeer.WorldSeed = StrandedWorld.LOCAL_WORLD_SEED;
		}
		if (MPExtensions.IsServer(Game.Mode) && PlayerRegistry.IsClientPeerValid)
		{
			yield return MultiplayerWorldSync.Instance.SendWorld_Async(World.CurrentWorld, (CoroutineHandle coroutine) =>
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				_syncWorldCoroutineSend = coroutine;
			}, (float ratio) =>
			{
				EventManager.RaiseEvent(new LoadingProgressChangedEvent(string.Format("{0} [{1}%]", "LOADING_PROGRESS_SYNC_WORLD", (int)((double)ratio * 100.0))));
			}, () =>
			{
			}, ShutdownToMainMenu);
		}
		if (MPExtensions.IsClient(Game.Mode))
		{
			yield return MultiplayerWorldSync.Instance.ReceiveWorld_Async((CoroutineHandle coroutine) =>
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				_syncWorldCoroutineRecv = coroutine;
			}, (float ratio) =>
			{
				EventManager.RaiseEvent(new LoadingProgressChangedEvent(string.Format("{0} [{1}%]", "LOADING_PROGRESS_SYNC_WORLD", (int)((double)ratio * 100.0))));
			}, (World.Bundle world) =>
			{
				World.PushWorld(world);
				Singleton<StrandedWorld>.Instance.ApplyWorldToZones();
			}, ShutdownToMainMenu);
		}
	}

	private CoroutineHandle SyncSave_Async(Action<JObject> onSaveFile, JObject saveFile = null)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return _syncSaveCoroutine = Timing.RunCoroutine(SyncSave_Sequence(onSaveFile, saveFile));
	}

	private IEnumerator<float> SyncSave_Sequence(Action<JObject> onSaveFile, JObject saveFile)
	{
		if (MPExtensions.IsServer(Game.Mode))
		{
			saveFile = saveFile ?? (SaveManager.ValidateSaveFile(Game.Mode) ? SaveManager.LoadSaveFile(Game.Mode) : null);
			if (PlayerRegistry.IsClientPeerValid)
			{
				yield return MultiplayerWorldSync.Instance.SendSave_Async(saveFile, (CoroutineHandle coroutine) =>
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0002: Unknown result type (might be due to invalid IL or missing references)
					_syncSaveCoroutineSend = coroutine;
				}, (float ratio) =>
				{
					EventManager.RaiseEvent(new LoadingProgressChangedEvent(string.Format("{0} [{1}%]", "LOADING_PROGRESS_SYNC_SAVE", (int)((double)ratio * 100.0))));
				}, onSaveFile, ShutdownToMainMenu);
			}
			else
			{
				onSaveFile?.Invoke(saveFile);
			}
		}
		if (MPExtensions.IsClient(Game.Mode))
		{
			yield return MultiplayerWorldSync.Instance.ReceiveSave_Async((CoroutineHandle coroutine) =>
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				_syncSaveCoroutineRecv = coroutine;
			}, (float ratio) =>
			{
				EventManager.RaiseEvent(new LoadingProgressChangedEvent(string.Format("{0} [{1}%]", "LOADING_PROGRESS_SYNC_SAVE", (int)((double)ratio * 100.0))));
			}, onSaveFile, ShutdownToMainMenu);
		}
	}

	public static void LoadLevel(int level, LoadSceneMode mode, Action OnStart, Action OnComplete)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Instance.Load(level, mode, OnStart, OnComplete);
	}

	public void Load(int level, LoadSceneMode mode, Action OnStart, Action OnComplete)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_loadingSequence);
		_loadingSequence = Timing.RunCoroutine(LoadScene_Sequence(level, mode, OnStart, OnComplete));
	}

	private IEnumerator<float> LoadScene_Sequence(int level, LoadSceneMode mode, Action OnStart, Action OnComplete)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		_serverClientCopy = null;
		_isLoadingScene = true;
		if ((int)Game.State == 3)
		{
			CurrentLoader = this;
			CurrentLoader.IsLoading = true;
		}
		_creating = (int)Game.State == 2;
		OnStart?.Invoke();
		RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, false, 0);
		AudioListener.volume = 0f;
		EventManager.RaiseEvent(new LoadingProgressChangedEvent("LOADING_PROGRESS_TEXT_INITIALIZING_OBJECTS"));
		yield return 0f;
		float timeout = Peer.GetDefaultTimeout();
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			if (MPExtensions.IsServer(Game.Mode))
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
				if ((int)Game.State == 3)
				{
					if (MPExtensions.IsMultiplayer(Game.Mode))
					{
						if ((int)Game.State == 3 && !loaded)
						{
							yield return Timing.WaitUntilDone(SyncSave_Async((JObject save) =>
							{
								rootData = save;
								loaded = true;
								async.allowSceneActivation = true;
							}));
						}
					}
					else
					{
						if ((int)Game.State == 3 && !loaded)
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
		if (((int)Game.State == 2 || (int)Game.State == 3) && MPExtensions.IsClient(Game.Mode))
		{
			Singleton<GameTime>.Instance.PostReplicateTick();
		}
		if (Object.op_Implicit((Object)(object)SceneCallbacks.Instance))
		{
			yield return Timing.WaitUntilDone(SceneCallbacks.Instance.Level_SceneLoaded(level));
		}
		foreach (IGameCamera allCamera in Cameras.AllCameras)
		{
			allCamera.gameObject.SetActive(false);
		}
		if ((int)Game.State == 1)
		{
			PrivateJet_Assetloader privateJet_Assetloader = Object.FindObjectOfType<PrivateJet_Assetloader>();
			if ((Object)(object)privateJet_Assetloader != (Object)null)
			{
				yield return privateJet_Assetloader.Load_Async();
			}
		}
		else if ((int)Game.State == 2)
		{
			if (MPExtensions.IsMultiplayer(Game.Mode))
			{
				yield return Timing.WaitUntilDone(SyncWorld_Async());
			}
			yield return CreateGame_Async();
		}
		else if ((int)Game.State == 3)
		{
			if (MPExtensions.IsMultiplayer(Game.Mode))
			{
				yield return Timing.WaitUntilDone(SyncWorld_Async());
			}
			yield return LoadGame_Async(rootData);
		}
		else if ((int)Game.State == 4)
		{
			AudioListener.volume = 1f;
			yield break;
		}
		if (MPExtensions.IsGame(Game.State) && MPExtensions.IsClient(Game.Mode))
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
		if ((int)Game.State == 3)
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
			allCamera2.gameObject.SetActive(true);
		}
		FollowSpawn[] spawners = Object.FindObjectsOfType<FollowSpawn>();
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
		if ((int)Game.State == 2 || (int)Game.State == 3)
		{
			if (MPExtensions.IsServer(Game.Mode))
			{
				Singleton<GameTime>.Instance.ReplicateTickRepeating();
			}
			Singleton<GameTime>.Instance.Paused = false;
			Singleton<Fader>.Instance.ResetFade(FadeMode.Solid, FadeDirection.In);
			Singleton<Fader>.Instance.SetFadeColor(Color.black);
			Singleton<Fader>.Instance.SetFadeEasing(EasingType.Linear);
			yield return Timing.WaitForSeconds(1f);
			TMPTextScreenViewAdapter textScreen = Object.FindObjectOfType<TMPTextScreenViewAdapter>();
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
		if ((int)Game.State == 2 || (int)Game.State == 3)
		{
			for (int num2 = 0; num2 < PlayerRegistry.AllPlayers.Count; num2++)
			{
				PlayerRegistry.AllPlayers[num2].Movement.IsBusy = default;
			}
			RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, false, 9);
			RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, true, 0);
			if (MPExtensions.IsServer(Game.Mode))
			{
				MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: false);
			}
		}
		else if ((int)Game.State == 1)
		{
			RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, true, 9);
		}
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			PlayerRegistry.LocalPeer.SetTimeout(timeout);
		}
	}

	private float CreateGame_Async()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Timing.WaitUntilDone(CreateGame());
	}

	private float LoadGame_Async(JObject rootData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return Timing.WaitUntilDone(LoadGame(rootData));
	}

	private CoroutineHandle CreateGame()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Timing.RunCoroutine(CreateGame_Sequence());
	}

	private CoroutineHandle LoadGame(JObject rootData)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
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
			if (!MPExtensions.IsMultiplayer(Game.Mode))
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
			(from behaviour in Object.FindObjectsOfType<MonoBehaviour>()
				where behaviour is IPersistentSaveable && !(behaviour is GameTime) && !(behaviour is GameCalendar) && !(behaviour is StrandedWorld)
				select behaviour).Cast<IPersistentSaveable>().ToList().ForEach((IPersistentSaveable saveableObject) =>
			{
				string name = saveableObject.GetType().Name;
				JObject field4 = persistentData.GetField(name);
				if (field4 != null)
				{
					SavingUtilities.Load(saveableObject, field4);
				}
				else
				{
					Debug.LogWarning((object)("LevelLoader:: No data was saved for persistent object [" + name + "]."));
				}
			});
			for (int num = 0; num < DeferredLoading.Count; num++)
			{
				LoadingActionUtilities.ExecuteDeferredAction(DeferredLoading[num]);
			}
			for (int num2 = 0; num2 < PostLoading.Count; num2++)
			{
				LoadingActionUtilities.ExecuteDeferredAction(PostLoading[num2]);
			}
			if (!MPExtensions.IsMultiplayer(Game.Mode))
			{
				Time.fixedDeltaTime = fixedDeltaTime;
			}
			Physics.autoSimulation = true;
		}
		else
		{
			Debug.LogError((object)"LevelLoader:: [Persistent] node is null or empty.");
		}
		if (!MPExtensions.IsGame(Game.State) || !MPExtensions.IsClient(Game.Mode) || !(originalP1position != Vector3.zero))
		{
			yield break;
		}
		IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(1);
		LayerMask physicsLayerMask = GetPhysicsLayerMask(Layers.PLAYER);
		physicsLayerMask = LayerMask.op_Implicit(LayerMask.op_Implicit(physicsLayerMask) ^ (1 << Layers.PLAYER));
		if (!Physics.CheckSphere(playerFromId.transform.position, 0.7f, LayerMask.op_Implicit(physicsLayerMask)))
		{
			yield break;
		}
		Vector3 val = Vector3.zero;
		bool flag = false;
		float num3 = 0f;
		int num4 = 180;
		for (int num5 = 0; num5 < num4; num5++)
		{
			float num6 = Mathf.Sin(num3);
			float num7 = Mathf.Cos(num3);
			num3 += (float)Math.PI * 2f / (float)num4;
			val = new Vector3(num6, 0f, num7);
			float num8 = 0.25f;
			if (!Physics.CheckSphere(originalP1position + val, num8, LayerMask.op_Implicit(physicsLayerMask)) && !Physics.Raycast(originalP1position + val, originalP1position - (originalP1position + val), val.magnitude, LayerMask.op_Implicit(physicsLayerMask)))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			playerFromId.transform.position = originalP1position + val;
		}
	}

	private LayerMask GetPhysicsLayerMask(int currentLayer)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < 32; i++)
		{
			if (!Physics.GetIgnoreLayerCollision(currentLayer, i))
			{
				num |= 1 << i;
			}
		}
		return LayerMask.op_Implicit(num);
	}
}
