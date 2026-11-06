using System;
using System.Collections.Generic;
using Beam;
using Beam.Crafting;
using Beam.Utilities;
using Funlabs;
using MEC;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCallbacks : MonoBehaviour
{
	[SerializeField]
	private Tutorial _tutorial;

	[SerializeField]
	private InteractiveObject_LIFERAFT _raftPrefab;

	[SerializeField]
	private Interactive_PADDLE _paddlePrefab;

	[SerializeField]
	private Interactive_STORAGE _endgameRewardCrate;

	[SerializeField]
	private Interactive_STORAGE _endgameRewardCrate2;

	public static readonly Vector3 RaftPosition = new Vector3(60f, 0.2f, -60f);

	private static SceneCallbacks _instance;

	public static SceneCallbacks Instance => _instance;

	private void Awake()
	{
		if ((Object)(object)_instance != (Object)null)
		{
			throw new Exception("Multiple SceneCallbacks components into the world");
		}
		_instance = this;
		SceneManager.sceneLoaded += SceneManager_SceneLoaded;
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
		_instance = null;
	}

	private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Invalid comparison between Unknown and I4
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Invalid comparison between Unknown and I4
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (BeamTeam.IsNullOrDestroyed(PlayerRegistry.LocalPeer) || PlayerRegistry.LocalPeer.IsTimeout)
		{
			return;
		}
		if ((int)Game.State == 3 && scene.buildIndex == 3)
		{
			PlayerRegistry.CreateLocalPlayers();
			return;
		}
		if ((int)Game.State == 1 && scene.buildIndex == 2)
		{
			PlayerRegistry.CreateLocalPlayers();
		}
		else if ((int)Game.State == 2)
		{
			PlayerRegistry.CreateLocalPlayers();
			for (int i = 0; i < PlayerRegistry.AllPlayers.Count; i++)
			{
				IPlayer player = PlayerRegistry.AllPlayers[i];
				if (player.IsInitialized)
				{
					((Component)player.Movement).transform.position = RaftPosition + new Vector3(0f + (float)player.Id * 0.7f, 1f, 0f);
				}
			}
		}
		else if ((int)Game.State == 4)
		{
			_ = scene.buildIndex;
			_ = 4;
		}
		for (int j = 0; j < PlayerRegistry.AllPlayers.Count; j++)
		{
			IPlayer player2 = PlayerRegistry.AllPlayers[j];
			if (player2.IsInitialized)
			{
				player2.PlayerCamera.CameraMovement.SetPosition(player2.Character.CameraRigTransform);
				player2.PlayerCamera.CameraMovement.SetRotation(player2.Character.CameraRigTransform);
			}
		}
	}

	public CoroutineHandle Level_SceneLoaded(int scene)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Timing.RunCoroutine(Level_SceneLoaded_Sequence(scene));
	}

	public IEnumerator<float> Level_SceneLoaded_Sequence(int scene)
	{
		if ((int)Game.State == 3 && scene == 3)
		{
			yield return BeamTiming.WaitForAllPlayers();
			PlayerRegistry.InitializeReplicants();
			yield break;
		}
		if ((int)Game.State == 1 && scene == 2)
		{
			Singleton<GameTime>.Instance.MilitaryTime_NoEvents = 20f;
			Singleton<GameTime>.Instance.Paused = true;
			yield return BeamTiming.WaitForAllPlayers();
			PlayerRegistry.InitializeReplicants();
			for (int i = 0; i < PlayerRegistry.AllPlayers.Count; i++)
			{
				((Component)PlayerRegistry.AllPlayers[i].Movement).transform.position = new Vector3(0f, 10f, 0f);
			}
		}
		else if ((int)Game.State == 2)
		{
			Singleton<GameTime>.Instance.MilitaryTime_NoEvents = 9f;
			yield return BeamTiming.WaitForAllPlayers();
			PlayerRegistry.InitializeReplicants();
			if (MPExtensions.IsMaster(Game.Mode))
			{
				InteractiveObject_LIFERAFT interactiveObject_LIFERAFT = MultiplayerMng.Instantiate<InteractiveObject_LIFERAFT>(_raftPrefab.PrefabId, RaftPosition, Quaternion.identity, MiniGuid.LIFERAFT);
				Interactive_PADDLE obj = MultiplayerMng.Instantiate<Interactive_PADDLE>(_paddlePrefab.PrefabId, MiniGuid.PADDLE_RAFT);
				interactiveObject_LIFERAFT.PaddleHook.Attach(obj);
				if (PlayerUtilities.GetServerStartingCrate() == EGameStartingCrate.On)
				{
					MultiplayerMng.Instantiate<Interactive_STORAGE>(_endgameRewardCrate.PrefabId, new Vector3(60f, 2f, -60f), Quaternion.identity, MiniGuid.New());
					MultiplayerMng.Instantiate<Interactive_STORAGE>(_endgameRewardCrate2.PrefabId, new Vector3(60f, 4f, -60f), Quaternion.identity, MiniGuid.New());
				}
			}
			else
			{
				MultiplayerZoneSync.Instance.LinkData(MultiplayerZoneSync.Usage.PaddleRaft, MiniGuid.PADDLE_RAFT, (MultiplayerZoneSync.Command command) =>
				{
					//IL_002a: Unknown result type (might be due to invalid IL or missing references)
					Timing.RunCoroutine(MultiplayerMng.FindObject_Sequence(DisconnectDetail.Timeout_Liferaft, MiniGuid.LIFERAFT, (InteractiveObject_LIFERAFT raft) =>
					{
						raft.PaddleHook.Attach((Interactive_PADDLE)command.Instance);
					}));
				});
			}
			for (int num = 0; num < PlayerRegistry.AllPlayers.Count; num++)
			{
				((Component)PlayerRegistry.AllPlayers[num].Movement).transform.position = RaftPosition + new Vector3(0f + (float)num * 0.7f, 1f, 0f);
			}
		}
		else if ((int)Game.State == 4 && scene == 4)
		{
			Singleton<GameTime>.Instance.MilitaryTime_NoEvents = 11f;
			Singleton<GameTime>.Instance.Paused = true;
			Terrain.activeTerrain.heightmapPixelError = 25f;
		}
		for (int num2 = 0; num2 < PlayerRegistry.AllPlayers.Count; num2++)
		{
			IPlayer player = PlayerRegistry.AllPlayers[num2];
			player.PlayerCamera.CameraMovement.SetPosition(player.Character.CameraRigTransform);
			player.PlayerCamera.CameraMovement.SetRotation(player.Character.CameraRigTransform);
		}
	}

	static SceneCallbacks()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
	}
}
