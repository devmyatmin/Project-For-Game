using System.Collections.Generic;
using Beam;
using Beam.Events;
using MEC;
using UnityEngine;

public class MissionAudio : MonoBehaviour
{
	private TrackType _missionBattleIntroTrack;

	private TrackType _missionBattleLoopTrack;

	private float _missionBattleIntroTrackLength;

	private CoroutineHandle _battleAudioSequence;

	private bool _enteredProximity;

	public void MissionEventBroadcaster_MissionStarted(IMissionDetailsProvider missionDetailsProvider)
	{
		_missionBattleIntroTrack = missionDetailsProvider.MissionData.IntroTrack;
		_missionBattleLoopTrack = missionDetailsProvider.MissionData.LoopTrack;
		_missionBattleIntroTrackLength = missionDetailsProvider.MissionData.IntroTrackLength;
	}

	public void MissionEventBroadcaster_MissionCompleted(IMissionDetailsProvider missionDetailsProvider)
	{
		PlayTrack(TrackType.Boss_Victory);
	}

	public void MissionEventBroadcaster_MissionFailed()
	{
		PlayTrack(TrackType.Boss_Failed);
	}

	public void MissionEventBroadcaster_EnteredProximity()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!_enteredProximity)
		{
			_battleAudioSequence = Timing.RunCoroutine(BattleAudio_Sequence());
			_enteredProximity = true;
		}
	}

	private IEnumerator<float> BattleAudio_Sequence()
	{
		PlayTrack(_missionBattleIntroTrack);
		float timer = 0f;
		while (timer < _missionBattleIntroTrackLength)
		{
			timer += Time.deltaTime;
			yield return 0f;
		}
		PlayTrackInstantly(_missionBattleLoopTrack);
	}

	public void MissionEventBroadcaster_MissionEnded()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_battleAudioSequence);
		_enteredProximity = false;
	}

	public void PlayTrackInstantly(TrackType track)
	{
		EventManager.RaiseEvent(new TrackEvent(track, TrackEventType.Play_Instantly));
	}

	public void PlayTrack(TrackType track)
	{
		EventManager.RaiseEvent(new TrackEvent(track, TrackEventType.Play));
	}

	public void StopTrack(TrackType track)
	{
		EventManager.RaiseEvent(new TrackEvent(track, TrackEventType.Stop));
	}
}
