using System.Collections.Generic;

namespace Beam;

public class BiomeFlockProvider : FlockProvider
{
	private Dictionary<Zone.BiomeType, List<FlockData>> _biomeFlocks;

	private Dictionary<string, List<FlockData>> _cachedFlocks = new Dictionary<string, List<FlockData>>();

	private string _previousZoneName;

	private void Start()
	{
		if ((bool)Singleton<StrandedWorld>.Instance)
		{
			Singleton<StrandedWorld>.Instance.ZoneEntered += StrandedWorld_ZoneEntered;
		}
	}

	private void OnDestroy()
	{
		if ((bool)Singleton<StrandedWorld>.Instance)
		{
			Singleton<StrandedWorld>.Instance.ZoneEntered -= StrandedWorld_ZoneEntered;
		}
	}

	private void StrandedWorld_ZoneEntered(Zone zone)
	{
		if (_cachedFlocks.ContainsKey(_previousZoneName))
		{
			return;
		}
		foreach (FlockData item in _cachedFlocks[_previousZoneName])
		{
			_ = item;
		}
	}

	public override IList<FlockData> GetFlocks()
	{
		Zone zone = StrandedWorld.GetZone(base.transform.position);
		if (_cachedFlocks.ContainsKey(zone.ZoneName))
		{
			return _cachedFlocks[zone.ZoneName];
		}
		return _biomeFlocks[zone.Biome];
	}

	public IDictionary<FlockSpecies, FlockData> GetFlockData()
	{
		Dictionary<FlockSpecies, FlockData> dictionary = new Dictionary<FlockSpecies, FlockData>();
		foreach (KeyValuePair<Zone.BiomeType, List<FlockData>> biomeFlock in _biomeFlocks)
		{
			foreach (FlockData item in biomeFlock.Value)
			{
				dictionary.Add(item.Species, item);
			}
		}
		return dictionary;
	}
}
