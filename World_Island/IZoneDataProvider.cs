using Beam.Serialization.Json;

namespace Beam;

public interface IZoneDataProvider
{
	JObject GetZoneData(Zone zone);
}
