using System.Collections.Generic;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam;

public static class PlayerUtilities
{
	public static int GetPlayerId_Safe(IPlayer player)
	{
		if (player.IsNullOrDestroyed())
		{
			return -1;
		}
		return player.Id;
	}

	public static Peer GetPeerFromId(int id)
	{
		return PlayerRegistry.AllPeers.FirstOrDefault_NonAlloc(IdMatches, id);
	}

	public static IPlayer GetPlayerFromId(int id)
	{
		return PlayerRegistry.AllPlayers.FirstOrDefault_NonAlloc(IdMatches, id);
	}

	public static PlayerCamera GetPlayerCameraFromId(int id)
	{
		return Cameras.AllCameras.FirstOrDefault_NonAlloc(IdMatches, id) as PlayerCamera;
	}

	public static IPlayer FindClosestPlayer(Vector3 position, float maxDistance = float.PositiveInfinity)
	{
		float distance;
		return FindClosestPlayer(position, out distance, maxDistance);
	}

	public static IPlayer FindClosestPlayer(Vector3 position, bool includeDeadPlayers)
	{
		float maxDistance = float.PositiveInfinity;
		float distance;
		return FindClosestPlayer(position, out distance, maxDistance, includeDeadPlayers);
	}

	public static IPlayer FindClosestPlayer(Vector3 position, out float distance, float maxDistance = float.PositiveInfinity, bool includeDeadPlayers = false)
	{
		IPlayer result = null;
		float num = maxDistance * maxDistance;
		for (int i = 0; i < PlayerRegistry.AllPlayers.Count; i++)
		{
			IPlayer player = PlayerRegistry.AllPlayers[i];
			if (player != null && (includeDeadPlayers || !player.Statistics.IsKnockedOut))
			{
				float sqrMagnitude = (player.transform.position - position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = player;
					num = sqrMagnitude;
				}
			}
		}
		distance = Mathf.Sqrt(num);
		return result;
	}

	public static float GetPlayerDistanceFromMission(Vector3 position, IPlayer player, float maxDistance = float.PositiveInfinity, bool includeDeadPlayers = false)
	{
		float num = maxDistance * maxDistance;
		float sqrMagnitude = (player.transform.position - position).sqrMagnitude;
		if (sqrMagnitude < num)
		{
			num = sqrMagnitude;
		}
		return Mathf.Sqrt(num);
	}

	public static IEnumerator<IPlayer> AllPlayers()
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			yield return allPlayer;
		}
	}

	public static IEnumerator<IPlayer> LocalPlayers()
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if (allPlayer.IsOwner)
			{
				yield return allPlayer;
			}
		}
	}

	public static int FindPlayers_NonAlloc(IPlayer[] buffer, Vector3 position, float maxDistance = float.PositiveInfinity, IEnumerator<IPlayer> players = null)
	{
		if (buffer == null || buffer.Length == 0)
		{
			return 0;
		}
		if (players.IsNullOrDestroyed())
		{
			players = AllPlayers();
		}
		float num = maxDistance * maxDistance;
		int result = 0;
		while (players.MoveNext())
		{
			IPlayer current = players.Current;
			if (current != null && (current.transform.position - position).sqrMagnitude < num)
			{
				buffer[result++] = current;
			}
		}
		return result;
	}

	public static bool IsLocalPlayer(this IPlayer player)
	{
		if (Game.Mode.IsLocalCoop())
		{
			return player != null;
		}
		return PlayerRegistry.LocalPlayer == player;
	}

	public static bool IsLocalPeer(this Peer peer)
	{
		return PlayerRegistry.LocalPeer == peer;
	}

	public static bool IsServerPeer(this Peer peer)
	{
		return peer.Id == 0;
	}

	private static bool IdMatches(Peer peer, int id)
	{
		return peer.Id == id;
	}

	private static bool IdMatches(IPlayer player, int id)
	{
		return GetPlayerId_Safe(player) == id;
	}

	private static bool IdMatches(IGameCamera camera, int id)
	{
		return camera.Id == id;
	}

	public static void SetPlayerBundlesActive(bool active)
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			allPlayer.transform.gameObject.SetActive(active);
			if (allPlayer.PlayerCamera != null)
			{
				allPlayer.PlayerCamera.transform.gameObject.SetActive(active);
			}
			if (allPlayer.PlayerUI != null)
			{
				allPlayer.PlayerUI.transform.gameObject.SetActive(active);
			}
		}
	}

	public static bool AllPlayersPresent()
	{
		int validPeersCount = PlayerRegistry.GetValidPeersCount();
		if (PlayerRegistry.AllPlayers.Count == Cameras.AllCameras.Count)
		{
			return PlayerRegistry.AllPlayers.Count == validPeersCount;
		}
		return false;
	}

	public static bool AnyPlayersPresent()
	{
		return PlayerRegistry.AllPlayers.Count != 0;
	}

	public static bool AllPeersPresent()
	{
		return PlayerRegistry.GetValidPeersCount() == Game.Mode.GetPlayersCount();
	}

	public static bool AnyPeersPresent()
	{
		return PlayerRegistry.GetValidPeersCount() != 0;
	}

	public static EGameDifficultyMode GetServerGameDifficulty()
	{
		if (!Game.Mode.IsMultiplayer())
		{
			return Options.CustomSettings.Difficulty;
		}
		return PlayerRegistry.ServerPeer.GameDifficulty;
	}

	public static EGameGenderMode GetServerCharacterGender()
	{
		if (!Game.Mode.IsMultiplayer())
		{
			return Options.CustomSettings.Gender;
		}
		return PlayerRegistry.ServerPeer.Gender;
	}

	public static EGameGenderMode GetClientCharacterGender()
	{
		if (!Game.Mode.IsMultiplayer())
		{
			return Options.CustomSettings.Gender2;
		}
		return PlayerRegistry.ServerPeer.Gender2;
	}

	public static EGameWildlifeMode GetServerWildlifeMode()
	{
		if (!Game.Mode.IsMultiplayer())
		{
			return Options.CustomSettings.Wildlife;
		}
		return PlayerRegistry.ServerPeer.Wildlife;
	}

	public static EGameStartingCrate GetServerStartingCrate()
	{
		if (!Game.Mode.IsMultiplayer())
		{
			return Options.CustomSettings.StartingCrate;
		}
		return PlayerRegistry.ServerPeer.StartingCrate;
	}
}
