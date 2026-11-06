using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
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
using Photon.Bolt.Internal;
using Photon.Bolt.Matchmaking;
using Photon.Bolt.Utils;
using UdpKit;
using UdpKit.Platform;
using UdpKit.Platform.Photon;
using UnityEngine;

namespace Funlabs;

public class MultiplayerMng : GlobalEventListener
{
	public delegate void Listener<T>(T evnt) where T : Event;

	private delegate void Listener(Event evnt);

	public enum AuthenticationService
	{
		AUTH_SERVICE_UNKNOWN = -1,
		AUTH_SERVICE_GAMESPARKS,
		AUTH_SERVICE_XBOX_XSTS
	}

	private struct SearchContainer(Transform container, bool includeInactive = false)
	{
		public Transform Container = container;

		public bool IncludeInactive = includeInactive;
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
			if (!((time >= _destroyTime) | force))
			{
				return;
			}
			Log("Destroying " + MPExtensions.ToStringSafe(_gameObject), "create/destroy", "magenta");
			onDestroy?.Invoke();
			if (BeamTeam.IsValid(_gameObject))
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
			if ((Object)(object)_instance != (Object)null && _instance._prefabPool != null)
			{
				_instance._prefabPool.IsLocal = value;
			}
		}
	}

	public static bool ReceivedEntitiesActive
	{
		get
		{
			return ((Component)_instance._receivedEntities).gameObject.activeInHierarchy;
		}
		set
		{
			((Component)_instance._receivedEntities).gameObject.SetActive(value);
		}
	}

	public static Transform ReceivedEntities => _instance?._receivedEntities;

	public static MultiplayerPrefabPool PrefabPool => _instance._prefabPool;

	public static GameState ServerGameState
	{
		[CompilerGenerated]
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public static MultiplayerMng Instance => _instance;

	public static UdpChannelName ZoneStreamChannel
	{
		get
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return _instance._zoneStreamChannel;
		}
	}

	public static UdpChannelName WorldStreamChannel
	{
		get
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return _instance._worldStreamChannel;
		}
	}

	public static PhotonPlatformConfig DefaultPhotonPlatformConfig
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Expected Obj, but got Unknown
			if (BeamTeam.IsNullOrDestroyed(_defaultPhotonPlatformConfig))
			{
				_defaultPhotonPlatformConfig = new PhotonPlatformConfig
				{
					AppId = "412f65d8-32e8-449b-91a0-64a7cd0669c3",
					Region = PhotonRegion.GetRegion((Regions)0),
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
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		string? text = ((object)Game.Mode/*cast due to constrained. prefix*/).ToString();
		CloudRegion cloudRegion = (CloudRegion)cloudRegionIndex;
		Log("Connect " + text + " Region " + cloudRegion);
		if (BoltNetwork.IsRunning)
		{
			LogError("Bolt still running - shutting down");
			BoltNetwork.ShutdownImmediate();
			BeamTiming.WaitAnd(3f, () =>
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
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected Obj, but got Unknown
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected I4, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (BoltNetwork.IsRunning)
		{
			throw new Exception(string.Format("Bolt: Unable to start game mode {0} IsRunning {1} IsServer {2} IsClient {3} IsSinglePlayer {4}", new object[5]
			{
				((object)Game.Mode/*cast due to constrained. prefix*/).ToString(),
				BoltNetwork.IsRunning,
				BoltNetwork.IsServer,
				BoltNetwork.IsClient,
				BoltNetwork.IsSinglePlayer
			}));
		}
		string? text = ((object)Game.Mode/*cast due to constrained. prefix*/).ToString();
		CloudRegion cloudRegion = (CloudRegion)cloudRegionIndex;
		Log("Connect " + text + " Region " + cloudRegion);
		DisconnectPayload = new DisconnectPayLoad();
		_instance._shutdownDisconnectInProgress = false;
		_instance._onSuccess = onSuccess;
		_instance._onError = onError;
		PhotonPlatformConfig val = MPExtensions.Copy(DefaultPhotonPlatformConfig);
		val.Region = PhotonRegion.GetRegion((Regions)cloudRegionIndex);
		BoltLauncher.SetUdpPlatform((UdpPlatform)new PhotonPlatform(val));
		BoltConfig configCopy = BoltRuntimeSettings.instance.GetConfigCopy();
		configCopy.disableAutoSceneLoading = true;
		configCopy.serverConnectionAcceptMode = (BoltConnectionAcceptMode)1;
		GameMode mode = Game.Mode;
		switch ((int)mode)
		{
		case 0:
		case 1:
		case 2:
		case 3:
			BoltLauncher.StartSinglePlayer(configCopy);
			break;
		case 4:
			BoltLauncher.StartServer(configCopy, (string)null);
			break;
		case 5:
			BoltLauncher.StartClient(configCopy);
			break;
		}
	}

	public override void BoltStartBegin()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		BoltRuntimeSettings.instance.overrideTimeScale = MPExtensions.IsMultiplayer(Game.Mode);
		_zoneStreamChannel = BoltNetwork.CreateStreamChannel("zone", (UdpChannelMode)1, 1);
		_worldStreamChannel = BoltNetwork.CreateStreamChannel("world", (UdpChannelMode)1, 1);
		MultiplayerTokens.Register();
	}

	public override void BoltStartDone()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Log("BoltStartDone");
		_prefabPool = new MultiplayerPrefabPool(_receivedEntities);
		BoltNetwork.SetPrefabPool((IPrefabPool)(object)_prefabPool);
		if (!MPExtensions.IsMultiplayer(Game.Mode))
		{
			PlayerRegistry.CreateLocalPeers();
		}
		PostSuccess();
	}

	public override void BoltStartFailed(UdpConnectionDisconnectReason disconnectReason)
	{
		LogError("BoltStartFailed" + ((object)disconnectReason/*cast due to constrained. prefix*/).ToString());
		PostError(((object)disconnectReason/*cast due to constrained. prefix*/).ToString());
	}

	public static void ConnectToSession(string sessionId = null, MultiplayerTokens.Session token = null, Action onSuccess = null, Action<string> onError = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsMultiplayer(Game.Mode))
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
		if (MPExtensions.IsServer(Game.Mode))
		{
			BoltMatchmaking.CreateSession(sessionId, (IProtocolToken)(object)token, (string)null, (IProtocolToken)null);
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
		PostError(((object)errorReason/*cast due to constrained. prefix*/).ToString());
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
			MultiplayerTokens.Session token = MPExtensions.GetToken(session.Value);
			if (token != null && _sessionToJoin.Equals(token.Name))
			{
				BoltMatchmaking.JoinSession(_sessionToJoin, (IProtocolToken)(object)JoinToken);
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
		PostError(((object)errorReason/*cast due to constrained. prefix*/).ToString());
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
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
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
		byte[] array = default;
		while (listening && !(Time.time - time > 5f))
		{
			foreach (BoltConnection connection in BoltNetwork.Connections)
			{
				connection.ReceiveData(ref array);
				if (array != null && array[0] == 1)
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
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		Log("ConnectRequest " + MPExtensions.ToStringSafe(joinToken));
		if (!MPExtensions.IsServer(Game.Mode))
		{
			return;
		}
		MultiplayerTokens.Session token = MPExtensions.GetToken(BoltMatchmaking.CurrentSession);
		string id = ((MultiplayerTokens.Join)(object)joinToken).Id;
		string name = ((MultiplayerTokens.Join)(object)joinToken).Name;
		if (MPExtensions.IsGame(Game.State))
		{
			if (token == null || !token.IsJoinable() || _blockedClientList.Contains(id) || IsClientOnWaitTime(id))
			{
				if (((Component)_permisionPrompt).gameObject.activeSelf)
				{
					_permisionPrompt.Hide();
				}
				BoltNetwork.Refuse(endpoint);
			}
			else
			{
				_permisionPrompt.endpoint = endpoint;
				_permisionPrompt.joinToken = joinToken;
				_permisionPrompt.Show(name, id);
			}
		}
		else if (token == null || !token.IsJoinable() || _blockedClientList.Contains(id))
		{
			BoltNetwork.Refuse(endpoint);
		}
		else
		{
			_connectedClientId = id;
			BoltNetwork.Accept(endpoint, (IProtocolToken)(object)new MultiplayerTokens.Accept
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
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		Log(string.Format("{0}:: Connection type: {1}, Accept Token: {2}, Connect Token: {3}", new object[4] { "Connected", connection.ConnectionType, connection.AcceptToken, connection.ConnectToken }));
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
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Invalid comparison between Unknown and I4
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Invalid comparison between Unknown and I4
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
			if ((int)Game.State == 3 || (int)Game.State == 2)
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
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			MultiplayerTokens.Session token = MPExtensions.GetToken(BoltMatchmaking.CurrentSession);
			token.IsOccupied = false;
			Zone zone = StrandedWorld.GetZone(PlayerRegistry.LocalPlayer.transform.position);
			if (zone.Biome == Zone.BiomeType.MISSION && !((object)zone).Equals((object?)Singleton<StrandedWorld>.Instance.FindEndGameZone()))
			{
				token.IsBusy = true;
			}
			else if (GetSessionToken().IsBusy)
			{
				token.IsBusy = false;
			}
			BoltMatchmaking.UpdateSession((IProtocolToken)(object)token);
		}
		catch (Exception ex)
		{
			LogError("Error updating session token " + ex.Message);
		}
	}

	public override void BoltShutdownBegin(AddCallback registerDoneCallback, UdpConnectionDisconnectReason disconnectReason)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Log($"BoltShutdownBegin {disconnectReason} isRunning {BoltNetwork.IsRunning}");
		if (BoltNetwork.IsRunning)
		{
			((GlobalEventListenerBase)this).Disconnected((BoltConnection)null);
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
		T obj = default;
		float startTime = Time.unscaledTime;
		while (BeamTeam.IsNullOrDestroyed(obj) && !(Time.unscaledTime > startTime + Peer.DEFAULT_TIMEOUT))
		{
			obj = FindObject<T>(referenceId);
			if (BeamTeam.IsNullOrDestroyed(obj))
			{
				yield return Timing.WaitForSeconds(retryPeriod);
			}
		}
		if (BeamTeam.IsNullOrDestroyed(obj))
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
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsServer(Game.Mode))
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
		string text = "NONE";
		if (!BoltNetwork.IsRunning)
		{
			return text;
		}
		try
		{
			Dictionary<string, object> currentMetadata = BoltMatchmaking.CurrentMetadata;
			if (currentMetadata != null)
			{
				BoltUtils.TryGetValue<string>(currentMetadata, "Region", ref text);
			}
		}
		catch (Exception)
		{
		}
		if (!string.IsNullOrEmpty(text))
		{
			return text.ToUpper();
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
		if (BeamTeam.IsNullOrDestroyed(_instance))
		{
			yield break;
		}
		yield return new SearchContainer(_instance._receivedEntities, includeInactive: true);
		if (Object.op_Implicit((Object)(object)LevelLoader.Instance))
		{
			foreach (Zone loadingZone in LevelLoader.Instance.LoadingZones)
			{
				if (loadingZone.Id != Singleton<StrandedWorld>.Instance.NmlZone.Id)
				{
					yield return new SearchContainer(loadingZone.SaveContainer);
				}
			}
		}
		Zone[] zones = Singleton<StrandedWorld>.Instance.Zones;
		foreach (Zone zone in zones)
		{
			if (!Object.op_Implicit((Object)(object)LevelLoader.Instance) || !LevelLoader.Instance.LoadingZones.Contains(zone))
			{
				yield return new SearchContainer(zone.SaveContainer);
			}
		}
		yield return new SearchContainer(Singleton<StrandedWorld>.Instance.NmlZone.SaveContainer);
	}

	public static T FindObject<T>(MiniGuid referenceId, bool includeInactive = false) where T : ISaveableReference
	{
		try
		{
			if (referenceId.IsDefault() || BeamTeam.IsNullOrDestroyed(_instance))
			{
				return default;
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
			T result = default;
			foreach (BoltEntity entity in BoltNetwork.Entities)
			{
				if (((Component)entity).gameObject.TryGetComponent<T>(ref result) && result.ReferenceId.Equals(referenceId))
				{
					return result;
				}
			}
		}
		catch (Exception ex)
		{
			LogError($"Error while finding entity {referenceId}: {ex.Message}\n{ex.StackTrace}");
		}
		return default;
	}

	public static T FindObject<T>(ulong networkId) where T : ISaveableReference
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			BoltEntity val = BoltNetwork.FindEntity(new NetworkId(networkId));
			if ((Object)null == (Object)(object)val)
			{
				return default;
			}
			return ((Component)val).GetComponent<T>();
		}
		catch (Exception ex)
		{
			LogError($"Error while finding entity {networkId}: {ex.Message}\n{ex.StackTrace}");
			return default;
		}
	}

	public void RegisterPrefabs()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
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
		return BoltUtils.Join<string>((IEnumerable<string>)MPExtensions.RemoveEmpty(Path.GetFileName(prefabPath).Split(new char[2] { ' ', '-' })), "_");
	}

	public static bool TryGetPrefabName(string prefabPath, out string multiplayerPrefabName)
	{
		multiplayerPrefabName = GetBoltPrefabName(prefabPath);
		PrefabId value;
		return _instance?._prefabs.TryGetValue(multiplayerPrefabName, out value) ?? false;
	}

	public static T Instantiate<T>(PrefabId multiplayerPrefab, IProtocolToken token = null) where T : MonoBehaviour
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)BoltNetwork.Instantiate(multiplayerPrefab, token)).GetComponent<T>();
	}

	public static T Instantiate<T>(string prefabPath, MiniGuid referenceId) where T : SaveablePrefab
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(prefabPath))
		{
			return null;
		}
		T val = (_instance._prefabs.TryGetValue(GetBoltPrefabName(prefabPath), out var value) ? MultiplayerMng.Instantiate<T>(value, (IProtocolToken)(object)new MultiplayerTokens.SaveablePrefab(referenceId)) : Object.Instantiate<T>(Resources.Load<T>(prefabPath)));
		val.ReferenceId = referenceId;
		return val;
	}

	public static T Instantiate<T>(uint prefabId, MiniGuid referenceId, Transform parent = null) where T : SaveablePrefab
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		T val = null;
		if (!BeamTeam.IsNullOrDestroyed(_instance) && Prefabs.TryGetMultiplayerPrefabName(prefabId, out var multiplayerPrefabName))
		{
			Log($"Instantiate Bolt Pref for {prefabId} -> {_instance._prefabs[multiplayerPrefabName].Value}", "create/destroy");
			val = MultiplayerMng.Instantiate<T>(_instance._prefabs[multiplayerPrefabName], (IProtocolToken)(object)new MultiplayerTokens.SaveablePrefab(referenceId));
			((Component)val).gameObject.transform.parent = parent;
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
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		T val = Instantiate<T>(prefabId, referenceId);
		((Component)val).transform.position = position;
		((Component)val).transform.rotation = rotation;
		((Component)val).gameObject.transform.parent = parent;
		return val;
	}

	public static void DestroyMultiplayerEntity(GameObject gameObject, DestroyDelay destroyDelay = DestroyDelay.SAFE)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (BeamTeam.IsNullOrDestroyed(gameObject))
		{
			Log($"Destroying {gameObject} - redundant destroy", "create/destroy", "magenta");
			return;
		}
		if (MPExtensions.IsMultiplayer(Game.Mode) && destroyDelay != DestroyDelay.NOW)
		{
			_destroyCommands.Add(new BoltDestroyCommand(gameObject, Time.unscaledTime + (float)destroyDelay / 30f));
			gameObject.SetActive(false);
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
		if (BeamTeam.IsNullOrDestroyed(saveablePrefab))
		{
			return;
		}
		if (saveablePrefab.IsMultiplayerEntity)
		{
			if (saveablePrefab.IsOwner)
			{
				DestroyMultiplayerEntity(((Component)saveablePrefab).gameObject, destroyDelay);
			}
			else
			{
				((Component)saveablePrefab).gameObject.SetActive(false);
			}
		}
		else
		{
			BeamTeam.Destroy<GameObject>(((Component)saveablePrefab).gameObject);
		}
	}

	private void UpdateDestroyCommands()
	{
		int count = _destroyCommands.Count;
		float unscaledTime = Time.unscaledTime;
		int i = count - 1;
		while (i >= 0)
		{
			_destroyCommands[i].Destroy(unscaledTime, () =>
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

	public static void AddListener<T>(Listener<T> listener) where T : Event
	{
		if (_instance._internalListeners.TryGetValue(listener, out var _))
		{
			LogError("Listener for " + typeof(T).Name + " already added");
			return;
		}
		Listener listener2 = (Event e) =>
		{
			listener((T)(object)e);
		};
		_instance._internalListeners[listener] = listener2;
		Type typeFromHandle = typeof(T);
		_instance._listeners.TryGetValue(typeFromHandle, out var value2);
		value2 = (Listener)Delegate.Combine(value2, listener2);
		_instance._listeners[typeFromHandle] = value2;
	}

	public static void RemoveListener<T>(Listener<T> listener) where T : Event
	{
		if (!BeamTeam.IsNullOrDestroyed(_instance))
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
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected I4, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected I4, but got Unknown
		GameStateChangeGlobalEvent val = GameStateChangeGlobalEvent.Create();
		val.FromState = (int)fromState;
		val.ToState = (int)toState;
		SendGlobalEvent_Core((Event)(object)val);
	}

	public override void OnEvent(GameStateChangeGlobalEvent evnt)
	{
		OnGlobalEventReceived_Core<GameStateChangeGlobalEvent>(evnt);
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
		if (!((Event)evnt).FromSelf)
		{
			MultiplayerMessage multiplayerMessage = ((MultiplayerTokens.MessageEnvelope)(object)evnt.Envelope).Get<MultiplayerMessage>();
			multiplayerMessage.ReceivedTime = Time.unscaledTime;
			multiplayerMessage.Ping = ((((Event)evnt).RaisedBy == null) ? 0f : ((Event)evnt).RaisedBy.PingAliased);
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
		if (!((Event)evnt).FromSelf)
		{
			I_MultiplayerMessage i_MultiplayerMessage = ((MultiplayerTokens.MessageEnvelope)(object)evnt.Envelope).Get<I_MultiplayerMessage>();
			i_MultiplayerMessage.SetTransportEntity(evnt.Instantiated);
			i_MultiplayerMessage.ReceivedTime = Time.unscaledTime;
			i_MultiplayerMessage.Ping = ((((Event)evnt).RaisedBy == null) ? 0f : ((Event)evnt).RaisedBy.PingAliased);
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
			int messageIndex = GetMessageIndex(_messageInId, out var oldestMessageTime);
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

	private static void SendGlobalEvent_Core(Event evnt)
	{
		Log($"Send global event {evnt}", "messages");
		evnt.Send();
	}

	private void OnGlobalEventReceived_Core<T>(T evnt) where T : Event
	{
		Log($"Received global event {evnt}", "messages");
		Type typeFromHandle = typeof(T);
		if (_listeners.TryGetValue(typeFromHandle, out var value))
		{
			value?.Invoke((Event)(object)evnt);
		}
	}

	private void Awake()
	{
		if ((Object)(object)_instance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		_instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
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
		if ((Object)(object)_instance == (Object)(object)this)
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
		if (!BoltNetwork.IsServer || BeamTeam.IsNullOrDestroyed(data) || data.IsNull() || BeamTeam.IsNullOrDestroyed(BoltMatchmaking.CurrentSession) || BeamTeam.IsNullOrDestroyed(MPExtensions.GetToken(BoltMatchmaking.CurrentSession)))
		{
			return;
		}
		try
		{
			MultiplayerTokens.Session token = MPExtensions.GetToken(BoltMatchmaking.CurrentSession);
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
			BoltMatchmaking.UpdateSession((IProtocolToken)(object)token);
		}
		catch (Exception ex)
		{
			LogError("Error updating session token " + ex.Message);
		}
	}

	public static void UpdateSessionToken_IsBusy(bool isBusy)
	{
		if (!BoltNetwork.IsServer || BeamTeam.IsNullOrDestroyed(BoltMatchmaking.CurrentSession) || BeamTeam.IsNullOrDestroyed(MPExtensions.GetToken(BoltMatchmaking.CurrentSession)))
		{
			return;
		}
		try
		{
			MultiplayerTokens.Session token = MPExtensions.GetToken(BoltMatchmaking.CurrentSession);
			token.IsBusy = isBusy;
			BoltMatchmaking.UpdateSession((IProtocolToken)(object)token);
		}
		catch (Exception ex)
		{
			LogError("Error updating session token " + ex.Message);
		}
	}

	public static void UpdateSessionToken_IsOccupied(bool isOccupied)
	{
		if (!BoltNetwork.IsServer || BeamTeam.IsNullOrDestroyed(BoltMatchmaking.CurrentSession) || BeamTeam.IsNullOrDestroyed(MPExtensions.GetToken(BoltMatchmaking.CurrentSession)))
		{
			return;
		}
		try
		{
			MultiplayerTokens.Session token = MPExtensions.GetToken(BoltMatchmaking.CurrentSession);
			token.IsOccupied = isOccupied;
			BoltMatchmaking.UpdateSession((IProtocolToken)(object)token);
		}
		catch (Exception ex)
		{
			LogError("Error updating session token " + ex.Message);
		}
	}

	public static MultiplayerTokens.Session GetSessionToken()
	{
		return MPExtensions.GetToken(BoltMatchmaking.CurrentSession);
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

	public static void LogError(string value, Object context = null)
	{
		string cosmeticText = GetCosmeticText(value, "red");
		BoltConsole.Write(cosmeticText);
		Debug.LogError((object)cosmeticText, context);
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
