namespace Beam;

public class MissionInitializer : MissionInitializerBase
{
	protected override void WireMission()
	{
		MissionEventBroadcasterRelay.GetMissionEventListener().ListenToBroadcaster(Mission);
	}

	protected override void UnwireMission()
	{
		MissionEventBroadcasterRelay.GetMissionEventListener().UnlistenToBroadcaster(Mission);
	}
}
