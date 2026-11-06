using System;
using System.Collections;
using System.Collections.Generic;
using Beam.Serialization.Json;
using Beam.UI;
using Funlabs;
using UnityEngine;

namespace Beam;

public class MissionControllerBase : SaveablePrefab, IMissionEventBroadcaster, IMissionDetailsProvider
{
	public class ReplicateMissionGoal_Completed : MultiplayerMessage
	{
		[Replicate]
		public SaveablePrefab MissionSaveablePrefab;

		[Replicate]
		public IPlayer Player;

		public override void OnPeer()
		{
			((MissionControllerBase)MissionSaveablePrefab).MissionGoal_CompletedCore(Player);
		}
	}

	public class ReplicateMissionStarted : MultiplayerMessage
	{
		[Replicate]
		public SaveablePrefab MissionSaveablePrefab;

		[Replicate]
		public IPlayer Player;

		[Replicate]
		public bool AlreadyStarted;

		public override void OnPeer()
		{
			((MissionControllerBase)MissionSaveablePrefab).ComputeMissionStarted(Player, AlreadyStarted);
		}
	}

	public class ReplicateMissionFailed : MultiplayerMessage
	{
		[Replicate]
		public SaveablePrefab MissionSaveablePrefab;

		[Replicate]
		public IPlayer Player;

		[Replicate]
		public bool OnePlayerLeavesMission;

		public override void OnPeer()
		{
			((MissionControllerBase)MissionSaveablePrefab).ComputeMissionFailed(Player, OnePlayerLeavesMission);
		}
	}

	public class ReplicateEnteredMission : MultiplayerMessage
	{
		[Replicate]
		public SaveablePrefab MissionSaveablePrefab;

		[Replicate]
		public IPlayer Player;

		public override void OnPeer()
		{
			((MissionControllerBase)MissionSaveablePrefab).ComputeEnteredMission(Player);
		}
	}

	public class ReplicateLeftMission : MultiplayerMessage
	{
		[Replicate]
		public SaveablePrefab MissionSaveablePrefab;

		[Replicate]
		public IPlayer Player;

		public override void OnPeer()
		{
			((MissionControllerBase)MissionSaveablePrefab).ComputeLeftMission(Player);
		}
	}

	public class ReplicateOnePlayerLeftMission : MultiplayerMessage
	{
		[Replicate]
		public SaveablePrefab MissionSaveablePrefab;

		[Replicate]
		public IPlayer Player;

		public override void OnPeer()
		{
			((MissionControllerBase)MissionSaveablePrefab).ComputeOnePlayerLeftMission(Player);
		}
	}

	public List<IPlayer> ActivePlayers = new List<IPlayer>();

	private const float START_DISTANCE = 40f;

	private const float FAIL_DISTANCE = 80f;

	private const float PROXIMITY_DISTANCE = 15f;

	[SerializeField]
	private MissionAssetLoader _missionAssetLoader;

	[SerializeField]
	private MissionData _missionData;

	[SerializeField]
	private bool _missionActivated;

	[SerializeField]
	private bool _missionLeave;

	private bool _missionCompleted;

	private bool _inProximity;

	public IMissionData MissionData => _missionData;

	public IMissionGoal MissionGoal { get; set; }

	public event Action<IMissionDetailsProvider, bool> MissionStarted;

	public event Action<IMissionDetailsProvider> MissionCompleted;

	public event Action MissionFailed;

	public event Action MissionEnded;

	public event Action<bool> EnteredMissionProximity;

	public event Action LeftMissionProximity;

	protected virtual void OnMissionStarted(IMissionDetailsProvider missionDetailsProvider, bool alreadyStarted)
	{
		MissionStarted?.Invoke(missionDetailsProvider, alreadyStarted);
		MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: true);
	}

	protected virtual void OnMissionCompleted(IMissionDetailsProvider missionDetailsProvider)
	{
		MissionCompleted?.Invoke(missionDetailsProvider);
		MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: false);
	}

	protected virtual void OnMissionFailed()
	{
		MissionFailed?.Invoke();
		MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: false);
	}

	protected virtual void OnMissionEnded()
	{
		MissionEnded?.Invoke();
		MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: false);
	}

	protected virtual void OnEnteredMissionProximity(bool showHUD)
	{
		EnteredMissionProximity?.Invoke(showHUD);
	}

	protected virtual void OnLeftMissionProximity()
	{
		LeftMissionProximity?.Invoke();
	}

	private void Start()
	{
		((MonoBehaviour)this).StartCoroutine((IEnumerator)Initialize_Sequence());
	}

	private IEnumerator<float> Initialize_Sequence()
	{
		_missionAssetLoader.LoadedGoal += MissionAssetLoader_LoadedGoal;
		_missionAssetLoader.LoadFailed += MissionAssetLoader_LoadFailed;
		_missionAssetLoader.Load();
		while (LevelLoader.IsLoading())
		{
			yield return 0f;
		}
		((MonoBehaviour)this).InvokeRepeating("PollActivation_InvokeRepeating", 0f, 1f);
	}

	private void OnDestroy()
	{
		MissionStarted = null;
		MissionCompleted = null;
		MissionFailed = null;
		MissionEnded = null;
		EnteredMissionProximity = null;
		LeftMissionProximity = null;
	}

	private void PollActivation_InvokeRepeating()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		if (_missionCompleted)
		{
			((MonoBehaviour)this).CancelInvoke("PollActivation_InvokeRepeating");
		}
		else
		{
			if (MissionGoal == null || LevelLoader.IsServerJoinInProgress)
			{
				return;
			}
			IPlayer player = PlayerUtilities.FindClosestPlayer(((Component)this).transform.position, out var distance);
			float playerDistanceFromMission = PlayerUtilities.GetPlayerDistanceFromMission(((Component)this).transform.position, PlayerRegistry.LocalPlayer);
			if (distance < 40f && (!_missionActivated || (!ActivePlayers.Contains(PlayerRegistry.LocalPlayer) && playerDistanceFromMission < 40f)))
			{
				if (!ActivePlayers.Contains(PlayerRegistry.LocalPlayer) && playerDistanceFromMission < 40f)
				{
					ReplicatedComputeMissionStarted(PlayerRegistry.LocalPlayer, _missionActivated);
				}
				else if (!_missionActivated)
				{
					ReplicatedComputeMissionStarted(player, _missionActivated);
				}
			}
			else if ((distance > 80f || (ActivePlayers.Contains(PlayerRegistry.LocalPlayer) && playerDistanceFromMission > 80f && !_missionLeave)) && _missionActivated)
			{
				if (ActivePlayers.Count == 2 && ActivePlayers.Contains(PlayerRegistry.LocalPlayer) && playerDistanceFromMission > 80f)
				{
					ReplicatedComputeMissionFailed(PlayerRegistry.LocalPlayer, onePlayerLeavesMission: true);
				}
				else
				{
					ReplicatedComputeMissionFailed(player, onePlayerLeavesMission: false);
				}
				return;
			}
			float playerDistanceFromMission2 = PlayerUtilities.GetPlayerDistanceFromMission(MissionGoal.Location, PlayerRegistry.LocalPlayer);
			if (playerDistanceFromMission2 < 15f && !_inProximity)
			{
				ReplicatedComputeEnteredMission(PlayerRegistry.LocalPlayer);
			}
			else if (playerDistanceFromMission2 > 15f && _inProximity)
			{
				ReplicatedComputeLeftMission(PlayerRegistry.LocalPlayer);
			}
		}
	}

	private void ComputeMissionStarted(IPlayer player, bool alreadyStarted)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)$"<color=#FF0000>Player {player.Id} starts Mission</color>");
		Zone zone = StrandedWorld.GetZone(player.transform.position);
		Zone zone2 = StrandedWorld.GetZone(((Component)this).transform.position);
		Zone zone3 = StrandedWorld.GetZone(PlayerRegistry.LocalPlayer.transform.position);
		if (!ActivePlayers.Contains(player))
		{
			ActivePlayers.Add(player);
		}
		if (alreadyStarted)
		{
			_missionLeave = false;
		}
		if ((Object)(object)zone == (Object)(object)zone2 && (PlayerRegistry.LocalPlayer.CurrentMissionGoal == null || (Object)(object)zone3 == (Object)(object)zone2))
		{
			PlayerRegistry.LocalPlayer.CurrentMissionGoal = MissionGoal;
			OnMissionStarted(this, alreadyStarted);
		}
		_missionActivated = true;
	}

	private void ComputeMissionFailed(IPlayer player, bool onePlayerLeavesMission)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)$"<color=#FF0000>Player {player.Id} failed Mission</color>");
		Zone zone = StrandedWorld.GetZone(player.transform.position);
		Zone zone2 = StrandedWorld.GetZone(((Component)this).transform.position);
		Zone zone3 = StrandedWorld.GetZone(PlayerRegistry.LocalPlayer.transform.position);
		if (!onePlayerLeavesMission)
		{
			ActivePlayers.Clear();
		}
		if ((Object)(object)zone == (Object)(object)zone2 && PlayerRegistry.LocalPlayer.CurrentMissionGoal == MissionGoal && (Object)(object)zone3 == (Object)(object)zone2)
		{
			PlayerRegistry.LocalPlayer.CurrentMissionGoal = null;
			if (!onePlayerLeavesMission)
			{
				MissionGoal.Reset();
				_missionActivated = false;
			}
			OnMissionFailed();
			OnMissionEnded();
		}
	}

	private void ComputeEnteredMission(IPlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Zone zone = StrandedWorld.GetZone(player.transform.position);
		Zone zone2 = StrandedWorld.GetZone(((Component)this).transform.position);
		Zone zone3 = StrandedWorld.GetZone(PlayerRegistry.LocalPlayer.transform.position);
		if ((Object)(object)zone == (Object)(object)zone2 && (PlayerRegistry.LocalPlayer.CurrentMissionGoal == MissionGoal || (Object)(object)zone3 == (Object)(object)zone2))
		{
			OnEnteredMissionProximity((Object)(object)zone3 == (Object)(object)zone2);
			_inProximity = true;
		}
	}

	private void ComputeLeftMission(IPlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Zone zone = StrandedWorld.GetZone(player.transform.position);
		Zone zone2 = StrandedWorld.GetZone(((Component)this).transform.position);
		Zone zone3 = StrandedWorld.GetZone(PlayerRegistry.LocalPlayer.transform.position);
		if ((Object)(object)zone == (Object)(object)zone2 && (PlayerRegistry.LocalPlayer.CurrentMissionGoal == MissionGoal || (Object)(object)zone3 == (Object)(object)zone2))
		{
			OnLeftMissionProximity();
			_inProximity = false;
		}
	}

	private void ComputeOnePlayerLeftMission(IPlayer player)
	{
		ActivePlayers.Remove(player);
		if (player == PlayerRegistry.LocalPlayer)
		{
			_missionLeave = true;
		}
	}

	private void MissionAssetLoader_LoadedGoal(IMissionGoal goal)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		MissionGoal = goal;
		if (MPExtensions.IsMaster(Game.Mode))
		{
			MissionGoal.Completed += MissionGoal_Completed;
		}
	}

	private void MissionAssetLoader_LoadFailed()
	{
		LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
		localizedNotification.Priority = NotificationPriority.Immediate;
		localizedNotification.Duration = 4f;
		localizedNotification.MessageText.SetTerm("MISSIONS_MISSION_ASSET_LOADER_FAILED");
		localizedNotification.Raise();
	}

	private void MissionGoal_Completed()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		ReplicatedMissionGoal_CompletedCore(PlayerUtilities.FindClosestPlayer(((Component)this).transform.position));
	}

	private void MissionGoal_CompletedCore(IPlayer player)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		_missionCompleted = true;
		_missionActivated = false;
		MissionData.RewardProvider.DoRewards(PlayerRegistry.LocalPlayer);
		OnMissionCompleted(this);
		Zone zone = StrandedWorld.GetZone(player.transform.position);
		Zone zone2 = StrandedWorld.GetZone(((Component)this).transform.position);
		Zone zone3 = StrandedWorld.GetZone(PlayerRegistry.LocalPlayer.transform.position);
		if ((Object)(object)zone == (Object)(object)zone2 && (PlayerRegistry.LocalPlayer.CurrentMissionGoal == null || PlayerRegistry.LocalPlayer.CurrentMissionGoal == MissionGoal || (Object)(object)zone3 == (Object)(object)zone2))
		{
			PlayerRegistry.LocalPlayer.CurrentMissionGoal = null;
			OnMissionEnded();
		}
	}

	private void ReplicatedMissionGoal_CompletedCore(IPlayer player)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode))
		{
			ReplicateMissionGoal_Completed replicateMissionGoal_Completed = new ReplicateMissionGoal_Completed();
			replicateMissionGoal_Completed.MissionSaveablePrefab = this;
			replicateMissionGoal_Completed.Player = player;
			replicateMissionGoal_Completed.Post();
		}
		MissionGoal_CompletedCore(player);
	}

	private void ReplicatedComputeMissionStarted(IPlayer player, bool alreadyStarted)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode) | alreadyStarted)
		{
			ReplicateMissionStarted replicateMissionStarted = new ReplicateMissionStarted();
			replicateMissionStarted.MissionSaveablePrefab = this;
			replicateMissionStarted.Player = player;
			replicateMissionStarted.AlreadyStarted = alreadyStarted;
			replicateMissionStarted.Post();
		}
		ComputeMissionStarted(player, alreadyStarted);
	}

	private void ReplicatedComputeMissionFailed(IPlayer player, bool onePlayerLeavesMission)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode) && !onePlayerLeavesMission)
		{
			ReplicateMissionFailed replicateMissionFailed = new ReplicateMissionFailed();
			replicateMissionFailed.MissionSaveablePrefab = this;
			replicateMissionFailed.Player = player;
			replicateMissionFailed.OnePlayerLeavesMission = onePlayerLeavesMission;
			replicateMissionFailed.Post();
		}
		else if (onePlayerLeavesMission)
		{
			ReplicatedOnePlayerLeftMission(player);
		}
		ComputeMissionFailed(player, onePlayerLeavesMission);
	}

	private void ReplicatedComputeEnteredMission(IPlayer player)
	{
		ComputeEnteredMission(player);
	}

	private void ReplicatedComputeLeftMission(IPlayer player)
	{
		ComputeLeftMission(player);
	}

	private void ReplicatedOnePlayerLeftMission(IPlayer player)
	{
		ReplicateOnePlayerLeftMission replicateOnePlayerLeftMission = new ReplicateOnePlayerLeftMission();
		replicateOnePlayerLeftMission.MissionSaveablePrefab = this;
		replicateOnePlayerLeftMission.Player = player;
		replicateOnePlayerLeftMission.Post();
		ComputeOnePlayerLeftMission(player);
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		JObject data = JSerializer.Serialize((Component)(object)((Component)this).transform);
		jObject.AddField("Transform", data);
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JSerializer.DeserializeInto(data.GetField("Transform"), (Component)(object)((Component)this).transform);
		CompleteTransformSerialization();
	}
}
