using UnityEngine.Events;

namespace Beam;

public interface IMissionEventListener
{
	MissionStartDetailsUnityEvent MissionStarted { get; }

	MissionDetailsUnityEvent MissionCompleted { get; }

	UnityEvent MissionFailed { get; }

	UnityEvent MissionEnded { get; }

	UnityEvent<bool> EnteredMissionProximity { get; }

	UnityEvent LeftMissionProximity { get; }

	void MissionEventBroadcaster_MissionStarted(IMissionDetailsProvider missionData, bool alreadyStarted);

	void MissionEventBroadcaster_MissionCompleted(IMissionDetailsProvider missionData);

	void MissionEventBroadcaster_MissionFailed();

	void MissionEventBroadcaster_MissionEnded();

	void MissionEventBroadcaster_EnteredMissionProximity(bool showHUD);

	void MissionEventBroadcaster_LeftMissionProximity();
}
