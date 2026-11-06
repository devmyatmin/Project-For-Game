using Beam;
using UnityEngine;

public class TerrainPointSampler : IPointSampler
{
	public void Init(Vector3 worldPosition)
	{
	}

	public bool TryGetSample(Vector3 worldPosition, out Sample sample)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		sample = Sample.Empty;
		if ((Object)(object)Terrain.activeTerrain != (Object)null)
		{
			sample = default;
			sample.Point = Singleton<StrandedWorld>.Instance.GetPoint(worldPosition);
			sample.Normal = Singleton<StrandedWorld>.Instance.GetNormal(worldPosition);
			return true;
		}
		return false;
	}
}
