using Beam.Events;
using Beam.Serialization.Json;

namespace Funlabs;

public class ZoneDownloadedEvent : IEvent
{
	public int ZoneId;

	public JObject Zone;

	public ZoneDownloadedEvent(int zoneId, JObject zone)
	{
		ZoneId = zoneId;
		Zone = zone;
	}
}
