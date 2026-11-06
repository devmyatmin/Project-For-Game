using Beam.Events;
using UnityEngine;

namespace Beam;

public class SoundtrackTrigger : MonoBehaviour
{
	[SerializeField]
	private TrackType _trackType;

	public void Play()
	{
		EventManager.RaiseEvent(new TrackEvent(_trackType, TrackEventType.Play));
	}

	public void Stop()
	{
		EventManager.RaiseEvent(new TrackEvent(_trackType, TrackEventType.Stop));
	}
}
