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
			if ((object)peerFromId == null)
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
		Debug.Log("PlayerRegistry::Unload:: Success.");
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
		if (_LocalPeer != null)
		{
			return;
		}
		MultiplayerMng.Log($"CreateLocalPeers {Game.Mode.ToString()} {Game.Mode.GetLocalPlayersCount()}");
		MultiplayerMng.IsLocalPrefab = true;
		for (int i = 0; i < Game.Mode.GetLocalPlayersCount(); i++)
		{
			if (Game.Mode.IsMultiplayer())
			{
				MultiplayerMng.GetLocalPeerId();
			}
			Peer localPeer = MultiplayerMng.Instantiate<Peer>(BoltPrefabs.Peer);
			if (_LocalPeer == null)
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
		if (PlayerUtilities.GetPeerFromId(id) != null)
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
		Debug.Log("PlayerRegistry::CreatePlayers:: Creating players.");
		Rect[] screenLayoutForPlayers = Game.GetScreenLayoutForPlayers(Game.Mode.GetLocalPlayersCount());
		MultiplayerMng.IsLocalPrefab = true;
		for (int i = 0; i < Game.Mode.GetLocalPlayersCount(); i++)
		{
			int num = (Game.Mode.IsMultiplayer() ? MultiplayerMng.GetLocalPeerId() : i);
			MultiplayerMng.Log($"CreateLocalPlayer {num}");
			Player player = MultiplayerMng.Instantiate<Player>(BoltPrefabs.Player, new MultiplayerTokens.ID
			{
				Id = num
			});
			player.Id = num;
			if (i == 0)
			{
				_LocalPlayer = player;
			}
			Rewired.Player player2 = ReInput.players.GetPlayer((!Game.Mode.IsMultiplayer()) ? num : 0);
			player2.isPlaying = true;
			player.SetInput(player2);
			PlayerCamera playerCamera = PlayerCamera.Create(player);
			playerCamera.Camera.rect = screenLayoutForPlayers[(!Game.Mode.IsMultiplayer()) ? num : 0];
			playerCamera.SetPlayer(player);
			player.SetUI(PlayerUI.Create(player));
			player.PlayerUI.SetPlayer(player);
			player.Initialize();
		}
		MultiplayerMng.IsLocalPrefab = false;
		_LocalPlayer.PlayerCamera.Camera.tag = "MainCamera";
	}

	public static void DestroyLocalPlayers()
	{
		foreach (IPlayer item in new List<IPlayer>(AllPlayers))
		{
			if (item.IsLocalPlayer())
			{
				MultiplayerMng.Log($"DestroyLocalPlayer Id:{item.Id}");
				if (!item.PlayerCamera.IsNullOrDestroyed())
				{
					MultiplayerMng.DestroyMultiplayerEntity(item.PlayerCamera.gameObject, DestroyDelay.NOW);
				}
				if (!item.PlayerUI.IsNullOrDestroyed())
				{
					BeamTeam.Destroy(item.PlayerUI.gameObject);
				}
				MultiplayerMng.DestroyMultiplayerEntity(((Player)item).gameObject, DestroyDelay.NOW);
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
		if (peer != null)
		{
			peer.Player = player;
		}
	}

	public static void Unlink(IPlayer player)
	{
		MultiplayerMng.Log($"Unlink player ID:{player.Id}");
		if (_AllPlayers.Remove(player))
		{
			if ((bool)player.Peer)
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
		if (player == null)
		{
			Debug.LogError("PlayerRegistry::CreatePlayer:: No Player prefab found!");
			return null;
		}
		return UnityEngine.Object.Instantiate(player);
	}

	public static void CheckAndHandleGameOver()
	{
		if (!_AllPlayers.Any((IPlayer p) => !p.IsNullOrDestroyed() && (!p.Statistics.IsKnockedOut || p.Statistics.IsReviving)))
		{
			GameOver();
		}
	}

	public static void GameOver()
	{
		if (Time.unscaledTime - _postGameOverTime < 1f)
		{
			return;
		}
		_postGameOverTime = Time.unscaledTime;
		MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: true);
		EventManager.RaiseEvent(new TrackEvent(TrackType.Player_Death, TrackEventType.Play_Instantly));
		List<PlayerDeathEventData> list = new List<PlayerDeathEventData>();
		if (Game.Mode.IsMultiplayer())
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
		if (_AllPlayers == null || _AllPlayers.Count == 0)
		{
			Debug.LogError("PlayerRegistry::Load:: No players exist!");
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
			if (Game.Mode.IsServer() && _AllPlayers.Count == 1 && data.Children.Count == 2)
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
