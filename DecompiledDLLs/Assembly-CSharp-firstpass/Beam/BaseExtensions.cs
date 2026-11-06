using UnityEngine;

namespace Beam;

public static class BaseExtensions
{
	public static void SetLayerRecursively(this Transform transform, int layer, int ignoreLayerMask = 0)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected Obj, but got Unknown
		if ((ignoreLayerMask & (1 << ((Component)transform).gameObject.layer)) == 0)
		{
			((Component)transform).gameObject.layer = layer;
		}
		foreach (Transform item in transform)
		{
			SetLayerRecursively(item, layer, ignoreLayerMask);
		}
	}
}
