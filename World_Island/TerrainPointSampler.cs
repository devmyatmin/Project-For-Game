using Beam;
using UnityEngine;

public class TerrainPointSampler : IPointSampler
{
	public void Init(Vector3 worldPosition)
	{
	}

	public bool TryGetSample(Vector3 worldPosition, out Sample sample)
	{
		sample = Sample.Empty;
		if (Terrain.activeTerrain != null)
		{
			sample = default(Sample);
			sample.Point = Singleton<StrandedWorld>.Instance.GetPoint(worldPosition);
			sample.Normal = Singleton<StrandedWorld>.Instance.GetNormal(worldPosition);
			return true;
		}
		return false;
	}
}
