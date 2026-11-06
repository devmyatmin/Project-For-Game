namespace Beam;

public static class IMissionEventListenerMixins
{
	public static void ListenToBroadcaster(this IMissionEventListener listener, IMissionEventBroadcaster broadcaster)
	{
		broadcaster.MissionStarted += listener.MissionEventBroadcaster_MissionStarted;
		broadcaster.MissionCompleted += listener.MissionEventBroadcaster_MissionCompleted;
		broadcaster.MissionFailed += listener.MissionEventBroadcaster_MissionFailed;
		broadcaster.MissionEnded += listener.MissionEventBroadcaster_MissionEnded;
		broadcaster.EnteredMissionProximity += listener.MissionEventBroadcaster_EnteredMissionProximity;
		broadcaster.LeftMissionProximity += listener.MissionEventBroadcaster_LeftMissionProximity;
	}

	public static void UnlistenToBroadcaster(this IMissionEventListener listener, IMissionEventBroadcaster broadcaster)
	{
		broadcaster.MissionStarted -= listener.MissionEventBroadcaster_MissionStarted;
		broadcaster.MissionCompleted -= listener.MissionEventBroadcaster_MissionCompleted;
		broadcaster.MissionFailed -= listener.MissionEventBroadcaster_MissionFailed;
		broadcaster.MissionEnded -= listener.MissionEventBroadcaster_MissionEnded;
		broadcaster.EnteredMissionProximity -= listener.MissionEventBroadcaster_EnteredMissionProximity;
		broadcaster.LeftMissionProximity -= listener.MissionEventBroadcaster_LeftMissionProximity;
	}
}
