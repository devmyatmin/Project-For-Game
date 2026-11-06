using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class MissionEventBroadcasterRelay : MonoBehaviour, IMissionEventListener
{
	private static MissionEventBroadcasterRelay _Instance;

	private static bool _errorShown;

	[SerializeField]
	private MissionStartDetailsUnityEvent _missionStarted;

	[SerializeField]
	private MissionDetailsUnityEvent _missionCompleted;

	[SerializeField]
	private UnityEvent _missionFailed;

	[SerializeField]
	private UnityEvent _missionEnded;

	[SerializeField]
	private UnityEvent<bool> _enteredMissionProximity;

	[SerializeField]
	private UnityEvent _leftMissionProximity;

	public MissionStartDetailsUnityEvent MissionStarted => _missionStarted;

	public MissionDetailsUnityEvent MissionCompleted => _missionCompleted;

	public UnityEvent MissionFailed => _missionFailed;

	public UnityEvent MissionEnded => _missionEnded;

	public UnityEvent<bool> EnteredMissionProximity => _enteredMissionProximity;

	public UnityEvent LeftMissionProximity => _leftMissionProximity;

	public static IMissionEventListener GetMissionEventListener()
	{
		if ((Object)(object)_Instance == (Object)null)
		{
			_Instance = Object.FindObjectOfType<MissionEventBroadcasterRelay>();
			if ((Object)(object)_Instance == (Object)null)
			{
				if (!_errorShown)
				{
					_errorShown = true;
					Debug.LogError((object)"MissionEventBroadcasterRelay::Get:: No MissionEventBroadcasterRelay found!");
				}
				return NullObject<NullMissionEventListener>.Instance;
			}
		}
		return _Instance;
	}

	public void MissionEventBroadcaster_MissionStarted(IMissionDetailsProvider missionData, bool alreadyStarted)
	{
		((UnityEvent<IMissionDetailsProvider, bool>)_missionStarted)?.Invoke(missionData, alreadyStarted);
	}

	public void MissionEventBroadcaster_MissionCompleted(IMissionDetailsProvider missionData)
	{
		((UnityEvent<IMissionDetailsProvider>)_missionCompleted)?.Invoke(missionData);
	}

	public void MissionEventBroadcaster_MissionFailed()
	{
		UnityEvent missionFailed = _missionFailed;
		if (missionFailed != null)
		{
			missionFailed.Invoke();
		}
	}

	public void MissionEventBroadcaster_MissionEnded()
	{
		UnityEvent missionEnded = _missionEnded;
		if (missionEnded != null)
		{
			missionEnded.Invoke();
		}
	}

	public void MissionEventBroadcaster_EnteredMissionProximity(bool showHUD)
	{
		_enteredMissionProximity?.Invoke(showHUD);
	}

	public void MissionEventBroadcaster_LeftMissionProximity()
	{
		UnityEvent leftMissionProximity = _leftMissionProximity;
		if (leftMissionProximity != null)
		{
			leftMissionProximity.Invoke();
		}
	}

	private void OnDestroy()
	{
		((UnityEventBase)_missionStarted).RemoveAllListeners();
		((UnityEventBase)_missionCompleted).RemoveAllListeners();
		((UnityEventBase)_missionFailed).RemoveAllListeners();
		((UnityEventBase)_missionEnded).RemoveAllListeners();
		((UnityEventBase)_enteredMissionProximity).RemoveAllListeners();
		((UnityEventBase)_leftMissionProximity).RemoveAllListeners();
		_Instance = null;
	}
}
