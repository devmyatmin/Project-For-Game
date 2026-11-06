using System;
using System.Collections.Generic;
using System.Linq;
using Beam.Events;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using MEC;
using Photon.Bolt;
using Rewired;
using UnityEngine;

namespace Beam;

public static class PlayerRegistry
{
	private sealed class PlayerRegistryFinalizer
	{
		~PlayerRegistryFinalizer()
		{
			Unload();
		}
	}

	private static readonly PlayerRegistryFinalizer _finalizer;

	private static List<Peer> _AllPeers;

	private static Peer _LocalPeer;

	private static Peer _ServerPeer;

	private static Player _LocalPlayer;

	private static List<IPlayer> _AllPlayers;

	private static float _postGameOverTime;

	public static List<Peer> AllPeers => _AllPeers;

	public static Peer LocalPeer => _LocalPeer;

	public static Peer ServerPeer => _ServerPeer;

	public static bool IsClientPeerValid
	{
		get
		{
			Peer peerFromId = PlayerUtilities.GetPeerFromId(1);
			if (peerFromId == null)
			{
				return false;
			}
			return !peerFromId.IsTimeout;
		}
	}

	public static IPlayer LocalPlayer => _LocalPlayer;

	public static IList<IPlayer> AllPlayers => _AllPlayers;

	static PlayerRegistry()
	{
		_finalizer = new PlayerRegistryFinalizer();
		_AllPeers = new List<Peer>();
		_AllPlayers = new List<IPlayer>();
	}

	private static void Unload()
	{
		Debug.Log((object)"PlayerRegistry::Unload:: Success.");
	}

	public static void Clear()
	{
		_AllPeers.Clear();
		_AllPlayers.Clear();
		_LocalPeer = null;
		_LocalPlayer = null;
		_ServerPeer = null;
	}

	public static int GetValidPeersCount()
	{
		int num = 0;
		foreach (Peer allPeer in _AllPeers)
		{
			if (!allPeer.IsTimeout)
			{
				num++;
			}
		}
		return num;
	}

	public static void CreateLocalPeers()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_LocalPeer != (Object)null)
		{
			return;
		}
		MultiplayerMng.Log($"CreateLocalPeers {((object)Game.Mode/*cast due to constrained. prefix*/).ToString()} {MPExtensions.GetLocalPlayersCount(Game.Mode)}");
		MultiplayerMng.IsLocalPrefab = true;
		for (int i = 0; i < MPExtensions.GetLocalPlayersCount(Game.Mode); i++)
		{
			if (MPExtensions.IsMultiplayer(Game.Mode))
			{
				MultiplayerMng.GetLocalPeerId();
			}
			Peer localPeer = MultiplayerMng.Instantiate<Peer>(BoltPrefabs.Peer, (IProtocolToken)null);
			if ((Object)(object)_LocalPeer == (Object)null)
			{
				_LocalPeer = localPeer;
			}
		}
		MultiplayerMng.IsLocalPrefab = false;
	}

	public static void Link(Peer peer)
	{
		_AllPeers.Add(peer);
		_AllPeers.Sort((Peer a, Peer b) => a.Id.CompareTo(b.Id));
		if (peer.Id == 0)
		{
			_ServerPeer = peer;
		}
		IPlayer player = (peer.Player = PlayerUtilities.GetPlayerFromId(peer.Id));
		if (player != null)
		{
			player.Peer = peer;
		}
	}

	public static void Unlink(Peer peer)
	{
		if (peer.IsLocalPeer())
		{
			_LocalPeer = null;
		}
		if (peer.IsServerPeer())
		{
			_ServerPeer = null;
		}
		_AllPeers.Remove(peer);
	}

	public static void OnAllPeersPresent(Action action)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerUtilities.AllPeersPresent())
		{
			action?.Invoke();
		}
		Timing.RunCoroutine(OnAllPeersPresent_Seq(action));
	}

	private static IEnumerator<float> OnAllPeersPresent_Seq(Action action)
	{
		yield return BeamTiming.WaitForAllPeers();
		action?.Invoke();
	}

	public static void OnPeerPresent(int id, Action action)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)PlayerUtilities.GetPeerFromId(id) != (Object)null)
		{
			action?.Invoke();
		}
		else
		{
			Timing.RunCoroutine(OnPeerPresent_Sequence(id, action));
		}
	}

	private static IEnumerator<float> OnPeerPresent_Sequence(int id, Action action)
	{
		yield return BeamTiming.WaitForPeer(id);
		action?.Invoke();
	}

	public static void CreateLocalPlayers()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)"PlayerRegistry::CreatePlayers:: Creating players.");
		Rect[] screenLayoutForPlayers = Game.GetScreenLayoutForPlayers(MPExtensions.GetLocalPlayersCount(Game.Mode));
		MultiplayerMng.IsLocalPrefab = true;
		for (int i = 0; i < MPExtensions.GetLocalPlayersCount(Game.Mode); i++)
		{
			int num = (MPExtensions.IsMultiplayer(Game.Mode) ? MultiplayerMng.GetLocalPeerId() : i);
			MultiplayerMng.Log($"CreateLocalPlayer {num}");
			Player player = MultiplayerMng.Instantiate<Player>(BoltPrefabs.Player, (IProtocolToken)(object)new MultiplayerTokens.ID
			{
				Id = num
			});
			player.Id = num;
			if (i == 0)
			{
				_LocalPlayer = player;
			}
			Player player2 = ReInput.players.GetPlayer((!MPExtensions.IsMultiplayer(Game.Mode)) ? num : 0);
			player2.isPlaying = true;
			player.SetInput(player2);
			PlayerCamera playerCamera = PlayerCamera.Create(player);
			playerCamera.Camera.rect = screenLayoutForPlayers[(!MPExtensions.IsMultiplayer(Game.Mode)) ? num : 0];
			playerCamera.SetPlayer(player);
			player.SetUI(PlayerUI.Create(player));
			player.PlayerUI.SetPlayer(player);
			player.Initialize();
		}
		MultiplayerMng.IsLocalPrefab = false;
		((Component)_LocalPlayer.PlayerCamera.Camera).tag = "MainCamera";
	}

	public static void DestroyLocalPlayers()
	{
		foreach (IPlayer item in new List<IPlayer>(AllPlayers))
		{
			if (item.IsLocalPlayer())
			{
				MultiplayerMng.Log($"DestroyLocalPlayer Id:{item.Id}");
				if (!BeamTeam.IsNullOrDestroyed(item.PlayerCamera))
				{
					MultiplayerMng.DestroyMultiplayerEntity(((Component)item.PlayerCamera).gameObject, DestroyDelay.NOW);
				}
				if (!BeamTeam.IsNullOrDestroyed(item.PlayerUI))
				{
					BeamTeam.Destroy<GameObject>(((Component)item.PlayerUI).gameObject);
				}
				MultiplayerMng.DestroyMultiplayerEntity(((Component)(Player)item).gameObject, DestroyDelay.NOW);
			}
		}
	}

	public static void InitializeReplicants()
	{
		foreach (IPlayer allPlayer in _AllPlayers)
		{
			if (!allPlayer.IsInitialized)
			{
				PlayerCamera playerCameraFromId = PlayerUtilities.GetPlayerCameraFromId(allPlayer.Id);
				allPlayer.SetCamera(playerCameraFromId);
				playerCameraFromId.SetPlayer(allPlayer);
				allPlayer.Initialize();
			}
		}
	}

	public static void Link(IPlayer player)
	{
		_AllPlayers.Add(player);
		_AllPlayers.Sort((IPlayer a, IPlayer b) => a.Id.CompareTo(b.Id));
		Peer peer = (player.Peer = PlayerUtilities.GetPeerFromId(player.Id));
		if ((Object)(object)peer != (Object)null)
		{
			peer.Player = player;
		}
	}

	public static void Unlink(IPlayer player)
	{
		MultiplayerMng.Log($"Unlink player ID:{player.Id}");
		if (_AllPlayers.Remove(player))
		{
			if (Object.op_Implicit((Object)(object)player.Peer))
			{
				player.Peer.Player = null;
				player.Peer = null;
			}
			if (player.IsLocalPlayer())
			{
				_LocalPlayer = null;
			}
			if (player.Input != null)
			{
				player.Input.isPlaying = false;
			}
		}
	}

	public static IPlayer CreatePeerlessPlayer()
	{
		Player player = Resources.Load<Player>("Prefabs/Player/Player");
		if ((Object)(object)player == (Object)null)
		{
			Debug.LogError((object)"PlayerRegistry::CreatePlayer:: No Player prefab found!");
			return null;
		}
		return Object.Instantiate<Player>(player);
	}

	public static void CheckAndHandleGameOver()
	{
		if (!_AllPlayers.Any((IPlayer p) => !BeamTeam.IsNullOrDestroyed(p) && (!p.Statistics.IsKnockedOut || p.Statistics.IsReviving)))
		{
			GameOver();
		}
	}

	public static void GameOver()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (Time.unscaledTime - _postGameOverTime < 1f)
		{
			return;
		}
		_postGameOverTime = Time.unscaledTime;
		MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: true);
		EventManager.RaiseEvent(new TrackEvent(TrackType.Player_Death, TrackEventType.Play_Instantly));
		List<PlayerDeathEventData> list = new List<PlayerDeathEventData>();
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			PlayerDeathEventData playerDeathEventData = new PlayerDeathEventData();
			playerDeathEventData.StatusEffects = LocalPlayer.Statistics.GetNegativeStatusEffects();
			list.Add(playerDeathEventData);
		}
		else
		{
			for (int i = 0; i < _AllPlayers.Count; i++)
			{
				IPlayer player = _AllPlayers[i];
				PlayerDeathEventData playerDeathEventData2 = new PlayerDeathEventData();
				playerDeathEventData2.StatusEffects = player.Statistics.GetNegativeStatusEffects();
				list.Add(playerDeathEventData2);
			}
		}
		EventManager.RaiseEvent(new GameOverEvent(list));
	}

	public static JObject Save()
	{
		JObject jObject = new JObject();
		for (int i = 0; i < _AllPlayers.Count; i++)
		{
			JObject jObject2 = SavingUtilities.Save(_AllPlayers[i]);
			if (jObject2 != null)
			{
				jObject.Add(jObject2);
			}
		}
		return jObject;
	}

	public static void Load(JObject data)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		if (_AllPlayers == null || _AllPlayers.Count == 0)
		{
			Debug.LogError((object)"PlayerRegistry::Load:: No players exist!");
		}
		else
		{
			if (data == null || data.IsNull())
			{
				return;
			}
			int count = AllPlayers.Count;
			for (int i = 0; i < count; i++)
			{
				IPlayer player = _AllPlayers[i];
				JObject jObject = ((i < data.Children.Count) ? data.Children[i] : null);
				if (jObject == null)
				{
					if (player.Id == 0)
					{
						throw new Exception("Master player not found in save file");
					}
					player.SetTransformCloseTo(PlayerUtilities.GetPlayerFromId(0));
				}
				else
				{
					SavingUtilities.Load(player, jObject);
				}
			}
			if (MPExtensions.IsServer(Game.Mode) && _AllPlayers.Count == 1 && data.Children.Count == 2)
			{
				JObject jObject2 = data.Children[1];
				if (jObject2 != null)
				{
					JObject serverClientCopy = new JObject().Set("Persistent/PlayerRegistry/#1", jObject2).Clear("Persistent/PlayerRegistry/#1/Movement");
					LevelLoader.Instance.ServerClientCopy = serverClientCopy;
				}
			}
		}
	}
}
