using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Beam;
using Beam.AccountServices;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using MEC;
using Photon.Bolt;
using Photon.Bolt.Matchmaking;
using Photon.Bolt.Utils;
using UdpKit;
using UdpKit.Platform;
using UdpKit.Platform.Photon;
using UnityEngine;

namespace Funlabs;

public class MultiplayerMng : GlobalEventListener
{
	public delegate void Listener<T>(T evnt) where T : Photon.Bolt.Event;

	private delegate void Listener(Photon.Bolt.Event evnt);

	public enum AuthenticationService
	{
		AUTH_SERVICE_UNKNOWN = -1,
		AUTH_SERVICE_GAMESPARKS,
		AUTH_SERVICE_XBOX_XSTS
	}

	private struct SearchContainer
	{
		public Transform Container;

		public bool IncludeInactive;

		public SearchContainer(Transform container, bool includeInactive = false)
		{
			Container = container;
			IncludeInactive = includeInactive;
		}
	}

	private struct BoltDestroyCommand
	{
		private GameObject _gameObject;

		private float _destroyTime;

		public BoltDestroyCommand(GameObject gameObject, float destroyTime)
		{
			Log($"Schedule {gameObject} for destruction", "create/destroy", "magenta");
			_gameObject = gameObject;
			_destroyTime = destroyTime;
		}

		public void Destroy(float time, Action onDestroy = null, bool force = false)
		{
			if (!(time >= _destroyTime || force))
			{
				return;
			}
			Log("Destroying " + _gameObject.ToStringSafe(), "create/destroy", "magenta");
			onDestroy?.Invoke();
			if (_gameObject.IsValid())
			{
				try
				{
					BoltNetwork.Destroy(_gameObject);
				}
				catch (Exception ex)
				{
					LogError("Error while delay destroying multiplayer game object: " + ex.Message + "\n" + ex.StackTrace);
				}
			}
		}
	}

	public static bool SHOW_UI = false;

	public static string DEFAULT_SESSION_ID = "BlueGold_" + Environment.UserName;

	private static MultiplayerTokens.Join _joinToken;

	private const byte SUCCESSFUL_CONNECTION = 1;

	private const float SUCCESSFUL_CONNECTION_TIMEOUT = 5f;

	private const float WAIT_TIME_CONNECTION_REQUEST = 30f;

	private const int MAX_INGAME_CONNECTION_REQUESTS = 3;

	[SerializeField]
	private Transform _receivedEntities;

	[SerializeField]
	private PermissionPromptPresenter _permisionPrompt;

	private MultiplayerPrefabPool _prefabPool;

	private Action _onSuccess;

	private Action<string> _onError;

	private string _sessionToJoin;

	private static MultiplayerMng _instance = null;

	public static DisconnectPayLoad DisconnectPayload = new DisconnectPayLoad();

	private Dictionary<Type, Listener> _listeners = new Dictionary<Type, Listener>();

	private Dictionary<Delegate, Listener> _internalListeners = new Dictionary<Delegate, Listener>();

	private Dictionary<string, PrefabId> _prefabs = new Dictionary<string, PrefabId>();

	private static HashSet<string> _enabledLogTags = new HashSet<string> { "default", "messages", "attach/detach", "create/destroy", "zone_sync", "world_sync" };

	private ushort _messageOutId;

	private ushort _messageInId;

	private float _connectTime;

	private List<MultiplayerMessage> _messages = new List<MultiplayerMessage>();

	private UdpChannelName _zoneStreamChannel;

	private UdpChannelName _worldStreamChannel;

	private static List<string> _blockedClientList = new List<string>();

	private static Dictionary<string, ClientOnHold> _clientsOnHold = new Dictionary<string, ClientOnHold>();

	private static string _connectedClientId = null;

	private static PhotonPlatformConfig _defaultPhotonPlatformConfig;

	private static bool _shuttingDown = false;

	private bool _shutdownDisconnectInProgress;

	private static List<BoltDestroyCommand> _destroyCommands = new List<BoltDestroyCommand>();

	private float _sessionOccupiedLastUpdateTime;

	public static MultiplayerTokens.Join JoinToken
	{
		get
		{
			if (_joinToken == null)
			{
				string displayName = GAccountService.Get().LocalUser.UserInfo.DisplayName;
				_joinToken = new MultiplayerTokens.Join
				{
					Id = SystemInfo.deviceUniqueIdentifier,
					Name = displayName
				};
			}
			return _joinToken;
		}
	}

	public static bool IsLocalPrefab
	{
		set
		{
			if (_instance != null && _instance._prefabPool != null)
			{
				_instance._prefabPool.IsLocal = value;
			}
		}
	}

	public static bool ReceivedEntitiesActive
	{
		get
		{
			return _instance._receivedEntities.gameObject.activeInHierarchy;
		}
		set
		{
			_instance._receivedEntities.gameObject.SetActive(value);
		}
	}

	public static Transform ReceivedEntities => _instance?._receivedEntities;

	public static MultiplayerPrefabPool PrefabPool => _instance._prefabPool;

	public static GameState ServerGameState { get; set; }

	public static MultiplayerMng Instance => _instance;

	public static UdpChannelName ZoneStreamChannel => _instance._zoneStreamChannel;

	public static UdpChannelName WorldStreamChannel => _instance._worldStreamChannel;

	public static PhotonPlatformConfig DefaultPhotonPlatformConfig
	{
		get
		{
			if (_defaultPhotonPlatformConfig.IsNullOrDestroyed())
			{
				_defaultPhotonPlatformConfig = new PhotonPlatformConfig
				{
					AppId = "412f65d8-32e8-449b-91a0-64a7cd0669c3",
					Region = PhotonRegion.GetRegion(PhotonRegion.Regions.BEST_REGION),
					RoomCreateTimeout = 20f,
					RoomJoinTimeout = 20f
				};
			}
			return _defaultPhotonPlatformConfig;
		}
	}

	public static void AuthenticateUser(Action onSuccess = null, Action onError = null, float timeout = 10f)
	{
		onSuccess?.Invoke();
	}

	public static float GetRoundTrip()
	{
		using (IEnumerator<BoltConnection> enumerator = BoltNetwork.Connections.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current.PingAliased;
			}
		}
		return 0f;
	}

	public static void ConnectToLocal(Action onSuccess = null)
	{
		ConnectToCloud(0, onSuccess);
	}

	public static void ConnectToCloud(int cloudRegionIndex = 0, Action onSuccess = null, Action<string> onError = null)
	{
		string text = Game.Mode.ToString();
		CloudRegion cloudRegion = (CloudRegion)cloudRegionIndex;
		Log("Connect " + text + " Region " + cloudRegion);
		if (BoltNetwork.IsRunning)
		{
			LogError("Bolt still running - shutting down");
			BoltNetwork.ShutdownImmediate();
			BeamTiming.WaitAnd(3f, delegate
			{
				ConnectTo(cloudRegionIndex, onSuccess, onError);
			});
		}
		else
		{
			ConnectTo(cloudRegionIndex, onSuccess, onError);
		}
	}

	private static void ConnectTo(int cloudRegionIndex = 0, Action onSuccess = null, Action<string> onError = null)
	{
		if (BoltNetwork.IsRunning)
		{
			throw new Exception($"Bolt: Unable to start game mode {Game.Mode.ToString()} IsRunning {BoltNetwork.IsRunning} IsServer {BoltNetwork.IsServer} IsClient {BoltNetwork.IsClient} IsSinglePlayer {BoltNetwork.IsSinglePlayer}");
		}
		string text = Game.Mode.ToString();
		CloudRegion cloudRegion = (CloudRegion)cloudRegionIndex;
		Log("Connect " + text + " Region " + cloudRegion);
		DisconnectPayload = new DisconnectPayLoad();
		_instance._shutdownDisconnectInProgress = false;
		_instance._onSuccess = onSuccess;
		_instance._onError = onError;
		PhotonPlatformConfig photonPlatformConfig = DefaultPhotonPlatformConfig.Copy();
		photonPlatformConfig.Region = PhotonRegion.GetRegion((PhotonRegion.Regions)cloudRegionIndex);
		BoltLauncher.SetUdpPlatform(new PhotonPlatform(photonPlatformConfig));
		BoltConfig configCopy = BoltRuntimeSettings.instance.GetConfigCopy();
		configCopy.disableAutoSceneLoading = true;
		configCopy.serverConnectionAcceptMode = BoltConnectionAcceptMode.Manual;
		switch (Game.Mode)
		{
		case GameMode.SinglePlayer:
		case GameMode.Coop_2:
		case GameMode.Coop_3:
		case GameMode.Coop_4:
			BoltLauncher.StartSinglePlayer(configCopy);
			break;
		case GameMode.Coop_Server:
			BoltLauncher.StartServer(configCopy);
			break;
		case GameMode.Coop_Client:
			BoltLauncher.StartClient(configCopy);
			break;
		}
	}

	public override void BoltStartBegin()
	{
		BoltRuntimeSettings.instance.overrideTimeScale = Game.Mode.IsMultiplayer();
		_zoneStreamChannel = BoltNetwork.CreateStreamChannel("zone", UdpChannelMode.Reliable, 1);
		_worldStreamChannel = BoltNetwork.CreateStreamChannel("world", UdpChannelMode.Reliable, 1);
		MultiplayerTokens.Register();
	}

	public override void BoltStartDone()
	{
		Log("BoltStartDone");
		_prefabPool = new MultiplayerPrefabPool(_receivedEntities);
		BoltNetwork.SetPrefabPool(_prefabPool);
		if (!Game.Mode.IsMultiplayer())
		{
			PlayerRegistry.CreateLocalPeers();
		}
		PostSuccess();
	}

	public override void BoltStartFailed(UdpConnectionDisconnectReason disconnectReason)
	{
		LogError("BoltStartFailed" + disconnectReason);
		PostError(disconnectReason.ToString());
	}

	public static void ConnectToSession(string sessionId = null, MultiplayerTokens.Session token = null, Action onSuccess = null, Action<string> onError = null)
	{
		if (!Game.Mode.IsMultiplayer())
		{
			onSuccess?.Invoke();
			return;
		}
		sessionId = (string.IsNullOrEmpty(sessionId) ? DEFAULT_SESSION_ID : sessionId);
		token = token ?? new MultiplayerTokens.Session
		{
			SessionId = sessionId,
			RegionIndex = GetCloudRegionIndex(),
			Name = sessionId,
			IsPrivate = true,
			HasUgc = true
		};
		_instance._onSuccess = onSuccess;
		_instance._onError = onError;
		if (Game.Mode.IsServer())
		{
			BoltMatchmaking.CreateSession(sessionId, token);
		}
		else
		{
			_instance._sessionToJoin = sessionId;
		}
	}

	public override void SessionCreatedOrUpdated(UdpSession session)
	{
		Log("Server connected to " + session.HostName);
		PlayerRegistry.CreateLocalPeers();
		PostSuccess();
	}

	public override void SessionCreationFailed(UdpSession session, UdpSessionError errorReason)
	{
		LogError("SessionCreationFailed");
		PostError(errorReason.ToString());
	}

	public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
	{
		Log($"SessionListUpdated {sessionList?.Count ?? 0}");
		if (string.IsNullOrEmpty(_sessionToJoin))
		{
			return;
		}
		foreach (KeyValuePair<Guid, UdpSession> session in sessionList)
		{
			MultiplayerTokens.Session token = session.Value.GetToken();
			if (token != null && _sessionToJoin.Equals(token.Name))
			{
				BoltMatchmaking.JoinSession(_sessionToJoin, JoinToken);
				_sessionToJoin = null;
				break;
			}
		}
	}

	public override void SessionConnected(UdpSession session, IProtocolToken token)
	{
		Log("SessionConnected");
		PlayerRegistry.CreateLocalPeers();
		PostSuccess();
	}

	public override void SessionConnectFailed(UdpSession session, IProtocolToken token, UdpSessionError errorReason)
	{
		LogError("SessionConnectFailed");
		PostError(errorReason.ToString());
	}

	public static void BlockCurrentClientId()
	{
		if (!string.IsNullOrEmpty(_connectedClientId) && !_blockedClientList.Contains(_connectedClientId))
		{
			_blockedClientList.Add(_connectedClientId);
		}
	}

	public static void BlockCurrentClientId(string requestClientId)
	{
		if (!string.IsNullOrEmpty(requestClientId) && !_blockedClientList.Contains(requestClientId))
		{
			_blockedClientList.Add(requestClientId);
		}
	}

	public static void ClearBlockedClientList()
	{
		_blockedClientList.Clear();
	}

	public static void ListenForSuccesfulClientConnection()
	{
		if (BoltNetwork.IsServer)
		{
			BeamTiming.RunCoroutine(CheckForAvailableEndpoint());
		}
	}

	private static IEnumerator<float> CheckForAvailableEndpoint()
	{
		bool listening = true;
		bool success = false;
		float time = Time.time;
		while (listening && !(Time.time - time > 5f))
		{
			foreach (BoltConnection connection in BoltNetwork.Connections)
			{
				connection.ReceiveData(out var data);
				if (data != null && data[0] == 1)
				{
					listening = false;
					success = true;
					break;
				}
			}
			yield return float.NegativeInfinity;
		}
		if (success)
		{
			yield break;
		}
		LogError("client invalid for connection, disconnecting clients");
		foreach (BoltConnection client in BoltNetwork.Clients)
		{
			client.Disconnect();
		}
	}

	public override void ConnectRequest(UdpEndPoint endpoint, IProtocolToken joinToken)
	{
		Log("ConnectRequest " + joinToken.ToStringSafe());
		if (!Game.Mode.IsServer())
		{
			return;
		}
		MultiplayerTokens.Session token = BoltMatchmaking.CurrentSession.GetToken();
		string id = ((MultiplayerTokens.Join)joinToken).Id;
		string nameClient = ((MultiplayerTokens.Join)joinToken).Name;
		if (Game.State.IsGame())
		{
			if (token == null || !token.IsJoinable() || _blockedClientList.Contains(id) || IsClientOnWaitTime(id))
			{
				if (_permisionPrompt.gameObject.activeSelf)
				{
					_permisionPrompt.Hide();
				}
				BoltNetwork.Refuse(endpoint);
			}
			else
			{
				_permisionPrompt.endpoint = endpoint;
				_permisionPrompt.joinToken = joinToken;
				_permisionPrompt.Show(nameClient, id);
			}
		}
		else if (token == null || !token.IsJoinable() || _blockedClientList.Contains(id))
		{
			BoltNetwork.Refuse(endpoint);
		}
		else
		{
			_connectedClientId = id;
			BoltNetwork.Accept(endpoint, new MultiplayerTokens.Accept
			{
				ServerGameState = Game.State
			});
		}
	}

	public static void UpdateClientOnHold(string requestClientId)
	{
		if (string.IsNullOrEmpty(requestClientId))
		{
			return;
		}
		if (_clientsOnHold.TryGetValue(requestClientId, out var value))
		{
			value.Requests++;
			value.LastRequestTime = Time.unscaledTime;
			_clientsOnHold[requestClientId] = value;
			if (value.Requests > 3)
			{
				BlockCurrentClientId(requestClientId);
				RemoveClientOnHold(requestClientId);
			}
		}
		else
		{
			ClientOnHold value2 = new ClientOnHold(requestClientId);
			_clientsOnHold.Add(requestClientId, value2);
		}
	}

	public static void RemoveClientOnHold(string requestClientId)
	{
		if (!string.IsNullOrEmpty(requestClientId) && _clientsOnHold.ContainsKey(requestClientId))
		{
			_clientsOnHold.Remove(requestClientId);
		}
	}

	public static bool IsClientOnWaitTime(string requestClientId)
	{
		if (string.IsNullOrEmpty(requestClientId))
		{
			return false;
		}
		if (_clientsOnHold.TryGetValue(requestClientId, out var value) && Time.unscaledTime - value.LastRequestTime < 30f)
		{
			return true;
		}
		return false;
	}

	public override void Connected(BoltConnection connection)
	{
		Log(string.Format("{0}:: Connection type: {1}, Accept Token: {2}, Connect Token: {3}", "Connected", connection.ConnectionType, connection.AcceptToken, connection.ConnectToken));
		_messageInId = 0;
		_messageOutId = 0;
		_messages.Clear();
		_connectTime = Time.unscaledTime;
		MultiplayerTokens.Accept accept = connection.AcceptToken as MultiplayerTokens.Accept;
		if (BoltNetwork.IsClient && accept == null)
		{
			LogError("Disconnected due to bad token");
			connection.Disconnect();
			return;
		}
		ServerGameState = (BoltNetwork.IsServer ? Game.State : accept.ServerGameState);
		Log($"ServerGameState {ServerGameState}");
		if (!BoltNetwork.IsServer)
		{
			BoltNetwork.Server.SendData(new byte[1] { 1 });
		}
		EventManager.RaiseEvent(new PlayerConnectedEvent(BoltNetwork.IsServer ? 1 : 0));
		if (BoltNetwork.IsServer)
		{
			UpdateSessionToken_IsOccupied(isOccupied: true);
		}
	}

	public override void Disconnected(BoltConnection connection)
	{
		if (_shutdownDisconnectInProgress)
		{
			return;
		}
		Log("Disconnected");
		_messageInId = 0;
		_messageOutId = 0;
		_messages.Clear();
		_connectTime = 0f;
		_connectedClientId = null;
		int num = ((BoltNetwork.IsServer && connection != null) ? 1 : 0);
		_shutdownDisconnectInProgress = num == 0 || (BoltNetwork.IsClient && num == 1);
		EventManager.RaiseEvent(new PlayerDisconnectedEvent(num, DisconnectPayload));
		if (BoltNetwork.IsServer && num == 1)
		{
			if (Game.State == GameState.LOAD_GAME || Game.State == GameState.NEW_GAME)
			{
				UpdateToken();
			}
			else
			{
				UpdateSessionToken_IsOccupied(isOccupied: false);
			}
		}
	}

	private void UpdateToken()
	{
		try
		{
			MultiplayerTokens.Session token = BoltMatchmaking.CurrentSession.GetToken();
			token.IsOccupied = false;
			Zone zone = StrandedWorld.GetZone(PlayerRegistry.LocalPlayer.transform.position);
			if (zone.Biome == Zone.BiomeType.MISSION && !zone.Equals(Beam.Singleton<StrandedWorld>.Instance.FindEndGameZone()))
			{
				token.IsBusy = true;
			}
			else if (GetSessionToken().IsBusy)
			{
				token.IsBusy = false;
			}
			BoltMatchmaking.UpdateSession(token);
		}
		catch (Exception ex)
		{
			LogError("Error updating session token " + ex.Message);
		}
	}

	public override void BoltShutdownBegin(AddCallback registerDoneCallback, UdpConnectionDisconnectReason disconnectReason)
	{
		Log($"BoltShutdownBegin {disconnectReason} isRunning {BoltNetwork.IsRunning}");
		if (BoltNetwork.IsRunning)
		{
			Disconnected(null);
		}
	}

	public static void Shutdown()
	{
		if (BoltNetwork.IsRunning && !_shuttingDown)
		{
			_shuttingDown = true;
			PlayerRegistry.Clear();
			try
			{
				BoltNetwork.ShutdownImmediate();
			}
			catch (Exception)
			{
			}
			_shuttingDown = false;
		}
	}

	private void PostSuccess()
	{
		Action onSuccess = _onSuccess;
		_onSuccess = null;
		_onError = null;
		onSuccess?.Invoke();
	}

	private void PostError(string reason)
	{
		Action<string> onError = _onError;
		_onSuccess = null;
		_onError = null;
		onError?.Invoke(reason);
	}

	public static IEnumerator<float> FindObject_Sequence<T>(DisconnectDetail disconnectDetail, MiniGuid referenceId, Action<T> onSuccess, Action onTimeout = null, float retryPeriod = 0.5f) where T : ISaveableReference
	{
		T obj = default(T);
		float startTime = Time.unscaledTime;
		while (obj.IsNullOrDestroyed() && !(Time.unscaledTime > startTime + Peer.DEFAULT_TIMEOUT))
		{
			obj = FindObject<T>(referenceId);
			if (obj.IsNullOrDestroyed())
			{
				yield return Timing.WaitForSeconds(retryPeriod);
			}
		}
		if (obj.IsNullOrDestroyed())
		{
			onTimeout?.Invoke();
			PlayerRegistry.LocalPeer.ReplicatedDisconnect(disconnectDetail);
		}
		else
		{
			onSuccess?.Invoke(obj);
		}
	}

	public static int GetLocalPeerId()
	{
		if (!Game.Mode.IsServer())
		{
			return 1;
		}
		return 0;
	}

	public static int GetEntityId(BoltEntity entity)
	{
		if (BoltNetwork.IsServer != entity.IsOwner)
		{
			return 1;
		}
		return 0;
	}

	public static string GetCloudRegion()
	{
		string value = "NONE";
		if (!BoltNetwork.IsRunning)
		{
			return value;
		}
		try
		{
			BoltMatchmaking.CurrentMetadata?.TryGetValue<string>("Region", out value);
		}
		catch (Exception)
		{
		}
		if (!string.IsNullOrEmpty(value))
		{
			return value.ToUpper();
		}
		return "NONE";
	}

	public static int GetCloudRegionIndex()
	{
		int result = 0;
		string cloudRegion = GetCloudRegion();
		for (int i = 0; i < BoltRuntimeSettings.photonCloudRegions.Length; i++)
		{
			if (BoltRuntimeSettings.photonCloudRegionsId[i].Equals(cloudRegion.ToLower()))
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private static IEnumerator<SearchContainer> AllSearchContainers()
	{
		if (_instance.IsNullOrDestroyed())
		{
			yield break;
		}
		yield return new SearchContainer(_instance._receivedEntities, includeInactive: true);
		if ((bool)LevelLoader.Instance)
		{
			foreach (Zone loadingZone in LevelLoader.Instance.LoadingZones)
			{
				if (loadingZone.Id != Beam.Singleton<StrandedWorld>.Instance.NmlZone.Id)
				{
					yield return new SearchContainer(loadingZone.SaveContainer);
				}
			}
		}
		Zone[] zones = Beam.Singleton<StrandedWorld>.Instance.Zones;
		foreach (Zone zone in zones)
		{
			if (!LevelLoader.Instance || !LevelLoader.Instance.LoadingZones.Contains(zone))
			{
				yield return new SearchContainer(zone.SaveContainer);
			}
		}
		yield return new SearchContainer(Beam.Singleton<StrandedWorld>.Instance.NmlZone.SaveContainer);
	}

	public static T FindObject<T>(MiniGuid referenceId, bool includeInactive = false) where T : ISaveableReference
	{
		try
		{
			if (referenceId.IsDefault() || _instance.IsNullOrDestroyed())
			{
				return default(T);
			}
			IEnumerator<SearchContainer> enumerator = AllSearchContainers();
			while (enumerator.MoveNext())
			{
				T val = enumerator.Current.Container.FindObject<T>(referenceId, includeInactive & enumerator.Current.IncludeInactive);
				if (val != null)
				{
					return val;
				}
			}
			foreach (BoltEntity entity in BoltNetwork.Entities)
			{
				if (entity.gameObject.TryGetComponent<T>(out var component) && component.ReferenceId.Equals(referenceId))
				{
					return component;
				}
			}
		}
		catch (Exception ex)
		{
			LogError($"Error while finding entity {referenceId}: {ex.Message}\n{ex.StackTrace}");
		}
		return default(T);
	}

	public static T FindObject<T>(ulong networkId) where T : ISaveableReference
	{
		try
		{
			BoltEntity boltEntity = BoltNetwork.FindEntity(new NetworkId(networkId));
			if (null == boltEntity)
			{
				return default(T);
			}
			return boltEntity.GetComponent<T>();
		}
		catch (Exception ex)
		{
			LogError($"Error while finding entity {networkId}: {ex.Message}\n{ex.StackTrace}");
			return default(T);
		}
	}

	public void RegisterPrefabs()
	{
		FieldInfo[] fields = typeof(BoltPrefabs).GetFields(BindingFlags.Static | BindingFlags.Public);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			_prefabs[fieldInfo.Name] = (PrefabId)fieldInfo.GetValue(null);
		}
		Log($"Multiplayer prefabs {fields.Length}");
		Prefabs.LinkMPPrefabs();
	}

	public static string GetBoltPrefabName(string prefabPath)
	{
		return Path.GetFileName(prefabPath).Split(' ', '-').RemoveEmpty()
			.Join("_");
	}

	public static bool TryGetPrefabName(string prefabPath, out string multiplayerPrefabName)
	{
		multiplayerPrefabName = GetBoltPrefabName(prefabPath);
		PrefabId value;
		return _instance?._prefabs.TryGetValue(multiplayerPrefabName, out value) ?? false;
	}

	public static T Instantiate<T>(PrefabId multiplayerPrefab, IProtocolToken token = null) where T : MonoBehaviour
	{
		return BoltNetwork.Instantiate(multiplayerPrefab, token).GetComponent<T>();
	}

	public static T Instantiate<T>(string prefabPath, MiniGuid referenceId) where T : SaveablePrefab
	{
		if (string.IsNullOrEmpty(prefabPath))
		{
			return null;
		}
		PrefabId value;
		T obj = (_instance._prefabs.TryGetValue(GetBoltPrefabName(prefabPath), out value) ? Instantiate<T>(value, new MultiplayerTokens.SaveablePrefab(referenceId)) : UnityEngine.Object.Instantiate(Resources.Load<T>(prefabPath)));
		obj.ReferenceId = referenceId;
		return obj;
	}

	public static T Instantiate<T>(uint prefabId, MiniGuid referenceId, Transform parent = null) where T : SaveablePrefab
	{
		T val = null;
		if (!_instance.IsNullOrDestroyed() && Prefabs.TryGetMultiplayerPrefabName(prefabId, out var multiplayerPrefabName))
		{
			Log($"Instantiate Bolt Pref for {prefabId} -> {_instance._prefabs[multiplayerPrefabName].Value}", "create/destroy");
			val = Instantiate<T>(_instance._prefabs[multiplayerPrefabName], new MultiplayerTokens.SaveablePrefab(referenceId));
			val.gameObject.transform.parent = parent;
		}
		else
		{
			val = (T)Prefabs.CreatePrefabFromId(prefabId, parent);
		}
		val.ReferenceId = referenceId;
		return val;
	}

	public static T Instantiate<T>(uint prefabId, Vector3 position, Quaternion rotation, MiniGuid referenceId, Transform parent = null) where T : SaveablePrefab
	{
		T val = Instantiate<T>(prefabId, referenceId);
		val.transform.position = position;
		val.transform.rotation = rotation;
		val.gameObject.transform.parent = parent;
		return val;
	}

	public static void DestroyMultiplayerEntity(GameObject gameObject, DestroyDelay destroyDelay = DestroyDelay.SAFE)
	{
		if (gameObject.IsNullOrDestroyed())
		{
			Log($"Destroying {gameObject} - redundant destroy", "create/destroy", "magenta");
			return;
		}
		if (Game.Mode.IsMultiplayer() && destroyDelay != 0)
		{
			_destroyCommands.Add(new BoltDestroyCommand(gameObject, Time.unscaledTime + (float)destroyDelay / 30f));
			gameObject.SetActive(value: false);
			return;
		}
		Log($"Destroying {gameObject} -  no delay", "create/destroy", "magenta");
		try
		{
			BoltNetwork.Destroy(gameObject);
		}
		catch (Exception ex)
		{
			LogError("Error while destroying multiplayer game object: " + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public static void Destroy(SaveablePrefab saveablePrefab, DestroyDelay destroyDelay = DestroyDelay.SAFE)
	{
		if (saveablePrefab.IsNullOrDestroyed())
		{
			return;
		}
		if (saveablePrefab.IsMultiplayerEntity)
		{
			if (saveablePrefab.IsOwner)
			{
				DestroyMultiplayerEntity(saveablePrefab.gameObject, destroyDelay);
			}
			else
			{
				saveablePrefab.gameObject.SetActive(value: false);
			}
		}
		else
		{
			BeamTeam.Destroy(saveablePrefab.gameObject);
		}
	}

	private void UpdateDestroyCommands()
	{
		int count = _destroyCommands.Count;
		float unscaledTime = Time.unscaledTime;
		int i = count - 1;
		while (i >= 0)
		{
			_destroyCommands[i].Destroy(unscaledTime, delegate
			{
				_destroyCommands.RemoveAt(i);
			});
			int num = i - 1;
			i = num;
		}
	}

	private void FixedUpdate()
	{
		UpdateDestroyCommands();
		UpdateMessages();
		if (BoltNetwork.IsServer && !BoltNetwork.IsSinglePlayer)
		{
			UpdateSessionOccupied();
		}
	}

	private void UpdateSessionOccupied()
	{
		if (PlayerRegistry.AllPlayers.Count != 0 && Math.Abs(Time.unscaledTime - _sessionOccupiedLastUpdateTime) > 3f)
		{
			bool flag = PlayerRegistry.AllPlayers.Count == 2;
			if (flag != GetSessionToken().IsOccupied)
			{
				UpdateSessionToken_IsOccupied(flag);
			}
			_sessionOccupiedLastUpdateTime = Time.unscaledTime;
		}
	}

	public static void AddListener<T>(Listener<T> listener) where T : Photon.Bolt.Event
	{
		if (_instance._internalListeners.TryGetValue(listener, out var _))
		{
			LogError("Listener for " + typeof(T).Name + " already added");
			return;
		}
		Listener listener2 = delegate(Photon.Bolt.Event e)
		{
			listener((T)e);
		};
		_instance._internalListeners[listener] = listener2;
		Type typeFromHandle = typeof(T);
		_instance._listeners.TryGetValue(typeFromHandle, out var value2);
		value2 = (Listener)Delegate.Combine(value2, listener2);
		_instance._listeners[typeFromHandle] = value2;
	}

	public static void RemoveListener<T>(Listener<T> listener) where T : Photon.Bolt.Event
	{
		if (!_instance.IsNullOrDestroyed())
		{
			Type typeFromHandle = typeof(T);
			if (_instance._internalListeners.TryGetValue(listener, out var value) && _instance._listeners.TryGetValue(typeFromHandle, out var value2))
			{
				value2 = (Listener)Delegate.Remove(value2, value);
				_instance._listeners[typeFromHandle] = value2;
				_instance._internalListeners.Remove(listener);
			}
		}
	}

	public override bool PersistBetweenStartupAndShutdown()
	{
		return true;
	}

	public static void SendGameStateChangeGlobalEvent(GameState fromState, GameState toState)
	{
		GameStateChangeGlobalEvent gameStateChangeGlobalEvent = GameStateChangeGlobalEvent.Create();
		gameStateChangeGlobalEvent.FromState = (int)fromState;
		gameStateChangeGlobalEvent.ToState = (int)toState;
		SendGlobalEvent_Core(gameStateChangeGlobalEvent);
	}

	public override void OnEvent(GameStateChangeGlobalEvent evnt)
	{
		OnGlobalEventReceived_Core(evnt);
	}

	public static ushort GetMessageOutId()
	{
		return _instance._messageOutId++;
	}

	public static void MessageSyncFenceAllocate()
	{
		Log($"Allocate {_instance._messageOutId} - Sync Fence", "messages");
		_instance._messageOutId++;
	}

	public static void MessageSyncFenceConsume()
	{
		Log($"Consume {_instance._messageInId} - Sync Fence", "messages");
		_instance._messageInId++;
	}

	public override void OnEvent(MessageGlobalEvent evnt)
	{
		if (!evnt.FromSelf)
		{
			MultiplayerMessage multiplayerMessage = ((MultiplayerTokens.MessageEnvelope)evnt.Envelope).Get<MultiplayerMessage>();
			multiplayerMessage.ReceivedTime = Time.unscaledTime;
			multiplayerMessage.Ping = ((evnt.RaisedBy == null) ? 0f : evnt.RaisedBy.PingAliased);
			if (BoltNetwork.IsClient && Math.Abs(Time.unscaledTime - _connectTime) < 0.5f && multiplayerMessage.Id > 50)
			{
				Log($"Reject message {multiplayerMessage.GetType().Name} with ID {multiplayerMessage.Id} as it looks as a message posted before the connection was made");
				return;
			}
			Log($"Received message {multiplayerMessage.GetType().Name} with index {multiplayerMessage.Id}");
			_messages.Add(multiplayerMessage);
		}
	}

	public override void OnEvent(I_MessageGlobalEvent evnt)
	{
		if (!evnt.FromSelf)
		{
			I_MultiplayerMessage i_MultiplayerMessage = ((MultiplayerTokens.MessageEnvelope)evnt.Envelope).Get<I_MultiplayerMessage>();
			i_MultiplayerMessage.SetTransportEntity(evnt.Instantiated);
			i_MultiplayerMessage.ReceivedTime = Time.unscaledTime;
			i_MultiplayerMessage.Ping = ((evnt.RaisedBy == null) ? 0f : evnt.RaisedBy.PingAliased);
			if (BoltNetwork.IsClient && Math.Abs(Time.unscaledTime - _connectTime) < 0.5f && i_MultiplayerMessage.Id > 50)
			{
				Log($"Reject message {i_MultiplayerMessage.GetType().Name} with ID {i_MultiplayerMessage.Id} as it looks as a message posted before the connection was made");
				return;
			}
			Log($"Received message  {i_MultiplayerMessage.GetType().Name} with index {i_MultiplayerMessage.Id}");
			_messages.Add(i_MultiplayerMessage);
		}
	}

	private void UpdateMessages()
	{
		bool flag = _messages.Count != 0;
		while (flag)
		{
			float oldestMessageTime;
			int messageIndex = GetMessageIndex(_messageInId, out oldestMessageTime);
			if (messageIndex != -1)
			{
				MultiplayerMessage multiplayerMessage = _messages[messageIndex];
				_messages.RemoveAt(messageIndex);
				_messageInId++;
				multiplayerMessage.Log("Executing " + multiplayerMessage.ToString());
				try
				{
					multiplayerMessage.OnPeer();
				}
				catch (Exception ex)
				{
					LogError("Error executing " + multiplayerMessage.ToString() + " " + ex.Message + "\n" + ex.StackTrace);
				}
				flag = _messages.Count != 0;
			}
			else
			{
				flag = false;
				if (Time.unscaledTime - oldestMessageTime > Peer.DEFAULT_TIMEOUT)
				{
					PlayerRegistry.LocalPeer.ReplicatedDisconnect(DisconnectDetail.Timeout_Messages, null, forceLocalShutdown: true);
				}
			}
		}
		if (_messages.Count != 0)
		{
			Log($"Queued {_messages.Count} with cursor {_messageInId}", "messages");
		}
	}

	private int GetMessageIndex(ushort id, out float oldestMessageTime)
	{
		oldestMessageTime = float.MaxValue;
		int count = _messages.Count;
		for (int i = 0; i < count; i++)
		{
			MultiplayerMessage multiplayerMessage = _messages[i];
			if (multiplayerMessage.Id == id)
			{
				oldestMessageTime = multiplayerMessage.ReceivedTime;
				return i;
			}
			if (multiplayerMessage.ReceivedTime < oldestMessageTime)
			{
				oldestMessageTime = multiplayerMessage.ReceivedTime;
			}
		}
		return -1;
	}

	private static void SendGlobalEvent_Core(Photon.Bolt.Event evnt)
	{
		Log($"Send global event {evnt}", "messages");
		evnt.Send();
	}

	private void OnGlobalEventReceived_Core<T>(T evnt) where T : Photon.Bolt.Event
	{
		Log($"Received global event {evnt}", "messages");
		Type typeFromHandle = typeof(T);
		if (_listeners.TryGetValue(typeFromHandle, out var value))
		{
			value?.Invoke(evnt);
		}
	}

	private void Awake()
	{
		if (_instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		_instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		RegisterPrefabs();
		MultiplayerFence.Register();
		MultiplayerMessageManager.Register();
		SaveManager.BeforeWrite += UpdateSessionToken_BeforeWriteSave;
	}

	private void OnDestroy()
	{
		Log("Shuting down connection");
		_joinToken = null;
		foreach (BoltDestroyCommand destroyCommand in _destroyCommands)
		{
			destroyCommand.Destroy(Time.frameCount, null, force: true);
		}
		_destroyCommands.Clear();
		if (_instance == this)
		{
			_instance = null;
		}
		SaveManager.BeforeWrite -= UpdateSessionToken_BeforeWriteSave;
	}

	private static void UpdateSessionToken_BeforeWriteSave(JObject rootData)
	{
		UpdateSessionToken_Player(rootData);
	}

	public static void UpdateSessionToken_Player(JObject data)
	{
		if (!BoltNetwork.IsServer || data.IsNullOrDestroyed() || data.IsNull() || BoltMatchmaking.CurrentSession.IsNullOrDestroyed() || BoltMatchmaking.CurrentSession.GetToken().IsNullOrDestroyed())
		{
			return;
		}
		try
		{
			MultiplayerTokens.Session token = BoltMatchmaking.CurrentSession.GetToken();
			JObject field = data.GetField("Persistent").GetField(typeof(GameTime).Name);
			if ((bool)field)
			{
				JObject field2 = field.GetField("SessionDuration");
				if (field2 != null)
				{
					token.TimePlayed = Mathf.FloorToInt(field2.GetValue<float>());
				}
			}
			Stats totalStats = GAccountService.Get().LocalUser.UserStats.TotalStats;
			token.DaysSurvived = totalStats.DaysSurvived;
			token.IslandsHopped = totalStats.IslandsHopped;
			token.IslandsExplored = totalStats.IslandsExplored;
			token.FishFished = totalStats.FishFished;
			token.EnemiesKilled = totalStats.AnimalsKilled;
			token.ItemsCrafted = totalStats.ItemsCrafted;
			token.ShipsScavenged = totalStats.ShipsScavenged;
			token.AnimalsTrapped = totalStats.AnimalsTrapped;
			BoltMatchmaking.UpdateSession(token);
		}
		catch (Exception ex)
		{
			LogError("Error updating session token " + ex.Message);
		}
	}

	public static void UpdateSessionToken_IsBusy(bool isBusy)
	{
		if (!BoltNetwork.IsServer || BoltMatchmaking.CurrentSession.IsNullOrDestroyed() || BoltMatchmaking.CurrentSession.GetToken().IsNullOrDestroyed())
		{
			return;
		}
		try
		{
			MultiplayerTokens.Session token = BoltMatchmaking.CurrentSession.GetToken();
			token.IsBusy = isBusy;
			BoltMatchmaking.UpdateSession(token);
		}
		catch (Exception ex)
		{
			LogError("Error updating session token " + ex.Message);
		}
	}

	public static void UpdateSessionToken_IsOccupied(bool isOccupied)
	{
		if (!BoltNetwork.IsServer || BoltMatchmaking.CurrentSession.IsNullOrDestroyed() || BoltMatchmaking.CurrentSession.GetToken().IsNullOrDestroyed())
		{
			return;
		}
		try
		{
			MultiplayerTokens.Session token = BoltMatchmaking.CurrentSession.GetToken();
			token.IsOccupied = isOccupied;
			BoltMatchmaking.UpdateSession(token);
		}
		catch (Exception ex)
		{
			LogError("Error updating session token " + ex.Message);
		}
	}

	public static MultiplayerTokens.Session GetSessionToken()
	{
		return BoltMatchmaking.CurrentSession.GetToken();
	}

	private static string GetTimeStampCosmeticText(string value, string color = "yellow")
	{
		float unscaledTime = Time.unscaledTime;
		float num = Mathf.Floor(unscaledTime);
		double num2 = Math.Floor((unscaledTime - num) * 1000f);
		return $"[{num}:{num2:0.###}] {value}";
	}

	private static string GetCosmeticText(string value, string color = "yellow")
	{
		string[] array = GetTimeStampCosmeticText(value).Split(new char[1] { '\n' });
		string text = "";
		for (int i = 0; i < array.Length; i++)
		{
			string text2 = array[i];
			text = ((i != 0) ? (text + "\n<color=" + color + ">" + text2 + "</color>") : (text + "<color=" + color + ">" + text2 + "</color>"));
		}
		return text;
	}

	public static void Log(string value, string tag = "default", string color = "yellow")
	{
	}

	public static void LogError(string value, UnityEngine.Object context = null)
	{
		string cosmeticText = GetCosmeticText(value, "red");
		BoltConsole.Write(cosmeticText);
		Debug.LogError(cosmeticText, context);
	}

	public static void Log2Reports(string value)
	{
	}

	public static void Log2Screen(string value, float duration = 5f)
	{
		Log(value);
	}

	private void DrawLog2Screen(Rect window)
	{
	}
}
