using UnityEngine;

public class ZoneTerrainPointSampler : IPointSampler
{
	private Zone _zone;

	public void Init(Vector3 worldPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		_zone = StrandedWorld.GetZone(worldPosition);
	}

	public bool TryGetSample(Vector3 worldPosition, out Sample sample)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		sample = Sample.Empty;
		if ((Object)(object)_zone == (Object)null)
		{
			return false;
		}
		Terrain terrain = _zone.Terrain;
		if ((Object)(object)terrain == (Object)null)
		{
			return false;
		}
		sample = default;
		sample.Point = worldPosition;
		sample.Point.y = terrain.SampleHeight(worldPosition) + -100f;
		Vector3 val = worldPosition - ((Component)terrain).transform.position;
		sample.Normal = terrain.terrainData.GetInterpolatedNormal(Mathf.InverseLerp(0f, terrain.terrainData.size.x, val.x), Mathf.InverseLerp(0f, terrain.terrainData.size.z, val.z));
		return true;
	}
}
