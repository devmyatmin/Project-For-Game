using System;

namespace Beam;

public interface IMissionEventBroadcaster
{
	event Action<IMissionDetailsProvider, bool> MissionStarted;

	event Action<IMissionDetailsProvider> MissionCompleted;

	event Action MissionFailed;

	event Action MissionEnded;

	event Action<bool> EnteredMissionProximity;

	event Action LeftMissionProximity;
}
