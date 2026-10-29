using UnityEngine;

public class ZoneTerrainPointSampler : IPointSampler
{
	private Zone _zone;

	public void Init(Vector3 worldPosition)
	{
		_zone = StrandedWorld.GetZone(worldPosition);
	}

	public bool TryGetSample(Vector3 worldPosition, out Sample sample)
	{
		sample = Sample.Empty;
		if (_zone == null)
		{
			return false;
		}
		Terrain terrain = _zone.Terrain;
		if (terrain == null)
		{
			return false;
		}
		sample = default(Sample);
		sample.Point = worldPosition;
		sample.Point.y = terrain.SampleHeight(worldPosition) + -100f;
		Vector3 vector = worldPosition - terrain.transform.position;
		sample.Normal = terrain.terrainData.GetInterpolatedNormal(Mathf.InverseLerp(0f, terrain.terrainData.size.x, vector.x), Mathf.InverseLerp(0f, terrain.terrainData.size.z, vector.z));
		return true;
	}
}
