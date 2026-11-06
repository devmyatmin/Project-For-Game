using System.Collections.Generic;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam;

public static class PlayerUtilities
{
	public static int GetPlayerId_Safe(IPlayer player)
	{
		if (BeamTeam.IsNullOrDestroyed(player))
		{
			return -1;
		}
		return player.Id;
	}

	public static Peer GetPeerFromId(int id)
	{
		return Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(PlayerRegistry.AllPeers, IdMatches, id);
	}

	public static IPlayer GetPlayerFromId(int id)
	{
		return Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(PlayerRegistry.AllPlayers, IdMatches, id);
	}

	public static PlayerCamera GetPlayerCameraFromId(int id)
	{
		return Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(Cameras.AllCameras, IdMatches, id) as PlayerCamera;
	}

	public static IPlayer FindClosestPlayer(Vector3 position, float maxDistance = float.PositiveInfinity)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		float distance;
		return FindClosestPlayer(position, out distance, maxDistance);
	}

	public static IPlayer FindClosestPlayer(Vector3 position, bool includeDeadPlayers)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		float maxDistance = float.PositiveInfinity;
		float distance;
		return FindClosestPlayer(position, out distance, maxDistance, includeDeadPlayers);
	}

	public static IPlayer FindClosestPlayer(Vector3 position, out float distance, float maxDistance = float.PositiveInfinity, bool includeDeadPlayers = false)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		IPlayer result = null;
		float num = maxDistance * maxDistance;
		for (int i = 0; i < PlayerRegistry.AllPlayers.Count; i++)
		{
			IPlayer player = PlayerRegistry.AllPlayers[i];
			if (player != null && (includeDeadPlayers || !player.Statistics.IsKnockedOut))
			{
				Vector3 val = player.transform.position - position;
				float sqrMagnitude = val.sqrMagnitude;
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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		float num = maxDistance * maxDistance;
		Vector3 val = player.transform.position - position;
		float sqrMagnitude = val.sqrMagnitude;
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
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (buffer == null || buffer.Length == 0)
		{
			return 0;
		}
		if (BeamTeam.IsNullOrDestroyed(players))
		{
			players = AllPlayers();
		}
		float num = maxDistance * maxDistance;
		int result = 0;
		while (players.MoveNext())
		{
			IPlayer current = players.Current;
			if (current != null)
			{
				Vector3 val = current.transform.position - position;
				if (val.sqrMagnitude < num)
				{
					buffer[result++] = current;
				}
			}
		}
		return result;
	}

	public static bool IsLocalPlayer(this IPlayer player)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsLocalCoop(Game.Mode))
		{
			return player != null;
		}
		return PlayerRegistry.LocalPlayer == player;
	}

	public static bool IsLocalPeer(this Peer peer)
	{
		return (Object)(object)PlayerRegistry.LocalPeer == (Object)(object)peer;
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
			((Component)allPlayer.transform).gameObject.SetActive(active);
			if ((Object)(object)allPlayer.PlayerCamera != (Object)null)
			{
				((Component)((Component)allPlayer.PlayerCamera).transform).gameObject.SetActive(active);
			}
			if ((Object)(object)allPlayer.PlayerUI != (Object)null)
			{
				((Component)((Component)allPlayer.PlayerUI).transform).gameObject.SetActive(active);
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
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return PlayerRegistry.GetValidPeersCount() == MPExtensions.GetPlayersCount(Game.Mode);
	}

	public static bool AnyPeersPresent()
	{
		return PlayerRegistry.GetValidPeersCount() != 0;
	}

	public static EGameDifficultyMode GetServerGameDifficulty()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsMultiplayer(Game.Mode))
		{
			return Options.CustomSettings.Difficulty;
		}
		return PlayerRegistry.ServerPeer.GameDifficulty;
	}

	public static EGameGenderMode GetServerCharacterGender()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsMultiplayer(Game.Mode))
		{
			return Options.CustomSettings.Gender;
		}
		return PlayerRegistry.ServerPeer.Gender;
	}

	public static EGameGenderMode GetClientCharacterGender()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsMultiplayer(Game.Mode))
		{
			return Options.CustomSettings.Gender2;
		}
		return PlayerRegistry.ServerPeer.Gender2;
	}

	public static EGameWildlifeMode GetServerWildlifeMode()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsMultiplayer(Game.Mode))
		{
			return Options.CustomSettings.Wildlife;
		}
		return PlayerRegistry.ServerPeer.Wildlife;
	}

	public static EGameStartingCrate GetServerStartingCrate()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsMultiplayer(Game.Mode))
		{
			return Options.CustomSettings.StartingCrate;
		}
		return PlayerRegistry.ServerPeer.StartingCrate;
	}
}
