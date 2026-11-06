using Beam.Events;

namespace Beam;

public class TrackEvent : IEvent
{
	private readonly TrackType _trackType;

	private readonly TrackEventType _eventType;

	public TrackType TrackType => _trackType;

	public TrackEventType EventType => _eventType;

	public TrackEvent(TrackType trackType, TrackEventType eventType)
	{
		_trackType = trackType;
		_eventType = eventType;
	}
}
