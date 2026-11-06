using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class NullMissionEventListener : NullObject<NullMissionEventListener>, IMissionEventListener
{
	public MissionStartDetailsUnityEvent MissionStarted { get; } = new MissionStartDetailsUnityEvent();

	public MissionDetailsUnityEvent MissionCompleted { get; } = new MissionDetailsUnityEvent();

	public UnityEvent MissionFailed { get; } = new UnityEvent();

	public UnityEvent MissionEnded { get; } = new UnityEvent();

	public UnityEvent<bool> EnteredMissionProximity { get; } = new UnityEvent<bool>();

	public UnityEvent LeftMissionProximity { get; } = new UnityEvent();

	public void MissionEventBroadcaster_MissionStarted(IMissionDetailsProvider missionData, bool alreadyStarted)
	{
		Debug.LogError((object)"NullMissionEventListener:: Not implemented.");
	}

	public void MissionEventBroadcaster_MissionCompleted(IMissionDetailsProvider missionData)
	{
		Debug.LogError((object)"NullMissionEventListener:: Not implemented.");
	}

	public void MissionEventBroadcaster_MissionFailed()
	{
		Debug.LogError((object)"NullMissionEventListener:: Not implemented.");
	}

	public void MissionEventBroadcaster_MissionEnded()
	{
		Debug.LogError((object)"NullMissionEventListener:: Not implemented.");
	}

	public void MissionEventBroadcaster_EnteredMissionProximity(bool showHUD)
	{
		Debug.LogError((object)"NullMissionEventListener:: Not implemented.");
	}

	public void MissionEventBroadcaster_LeftMissionProximity()
	{
		Debug.LogError((object)"NullMissionEventListener:: Not implemented.");
	}

	public NullMissionEventListener()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected Obj, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected Obj, but got Unknown
	}
}
