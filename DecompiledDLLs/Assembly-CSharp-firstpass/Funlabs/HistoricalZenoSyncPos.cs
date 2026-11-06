using UnityEngine;

namespace Funlabs;

public class HistoricalZenoSyncPos : HistoricalZenoSync<Vector3>
{
	internal override float Distance(Vector3 a, Vector3 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(a, b);
	}

	internal override Vector3 Lerp(Vector3 a, Vector3 b, float t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Lerp(a, b, t);
	}
}
