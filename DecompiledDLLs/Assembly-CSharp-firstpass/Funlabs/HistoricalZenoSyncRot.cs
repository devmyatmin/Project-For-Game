using UnityEngine;

namespace Funlabs;

public class HistoricalZenoSyncRot : HistoricalZenoSync<Quaternion>
{
	internal override float Distance(Quaternion a, Quaternion b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Angle(a, b);
	}

	internal override Quaternion Lerp(Quaternion a, Quaternion b, float t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Slerp(a, b, t);
	}
}
