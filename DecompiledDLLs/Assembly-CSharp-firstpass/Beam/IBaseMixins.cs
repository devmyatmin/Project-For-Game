using UnityEngine;

namespace Beam;

public static class IBaseMixins
{
	public static void ParentTo(this IBase obj, Transform transform)
	{
		obj.transform.parent = null;
		obj.TransformParent = transform;
		obj.Parent();
	}
}
