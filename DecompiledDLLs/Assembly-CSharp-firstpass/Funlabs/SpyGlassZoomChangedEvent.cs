using Beam.Events;

namespace Funlabs;

public class SpyGlassZoomChangedEvent : IEvent
{
	public float ZoomLevel;

	public SpyGlassZoomChangedEvent(float zoomLevel)
	{
		ZoomLevel = zoomLevel;
	}
}
