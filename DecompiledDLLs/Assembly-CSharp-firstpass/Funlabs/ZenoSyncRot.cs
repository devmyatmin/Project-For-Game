using UnityEngine;

namespace Funlabs;

public class ZenoSyncRot : ZenoSync<Quaternion>
{
	internal override Quaternion Lerp(Quaternion a, Quaternion b, float t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Slerp(a, b, t);
	}
}
