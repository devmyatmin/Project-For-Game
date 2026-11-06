using System;
using UnityEngine;

namespace RootMotion;

public class Hierarchy
{
	public static bool HierarchyIsValid(Transform[] bones)
	{
		for (int i = 1; i < bones.Length; i++)
		{
			if (!IsAncestor(bones[i], bones[i - 1]))
			{
				return false;
			}
		}
		return true;
	}

	public static Object ContainsDuplicate(Object[] objects)
	{
		for (int i = 0; i < objects.Length; i++)
		{
			for (int j = 0; j < objects.Length; j++)
			{
				if (i != j && objects[i] == objects[j])
				{
					return objects[i];
				}
			}
		}
		return null;
	}

	public static bool IsAncestor(Transform transform, Transform ancestor)
	{
		if ((Object)(object)transform == (Object)null)
		{
			return true;
		}
		if ((Object)(object)ancestor == (Object)null)
		{
			return true;
		}
		if ((Object)(object)transform.parent == (Object)null)
		{
			return false;
		}
		if ((Object)(object)transform.parent == (Object)(object)ancestor)
		{
			return true;
		}
		return IsAncestor(transform.parent, ancestor);
	}

	public static bool ContainsChild(Transform transform, Transform child)
	{
		if ((Object)(object)transform == (Object)(object)child)
		{
			return true;
		}
		Transform[] componentsInChildren = ((Component)transform).GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if ((Object)(object)componentsInChildren[i] == (Object)(object)child)
			{
				return true;
			}
		}
		return false;
	}

	public static void AddAncestors(Transform transform, Transform blocker, ref Transform[] array)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform.parent != (Object)null && (Object)(object)transform.parent != (Object)(object)blocker)
		{
			if (transform.parent.position != transform.position && transform.parent.position != blocker.position)
			{
				Array.Resize(ref array, array.Length + 1);
				array[array.Length - 1] = transform.parent;
			}
			AddAncestors(transform.parent, blocker, ref array);
		}
	}

	public static Transform GetAncestor(Transform transform, int minChildCount)
	{
		if ((Object)(object)transform == (Object)null)
		{
			return null;
		}
		if ((Object)(object)transform.parent != (Object)null)
		{
			if (transform.parent.childCount >= minChildCount)
			{
				return transform.parent;
			}
			return GetAncestor(transform.parent, minChildCount);
		}
		return null;
	}

	public static Transform GetFirstCommonAncestor(Transform t1, Transform t2)
	{
		if ((Object)(object)t1 == (Object)null)
		{
			return null;
		}
		if ((Object)(object)t2 == (Object)null)
		{
			return null;
		}
		if ((Object)(object)t1.parent == (Object)null)
		{
			return null;
		}
		if ((Object)(object)t2.parent == (Object)null)
		{
			return null;
		}
		if (IsAncestor(t2, t1.parent))
		{
			return t1.parent;
		}
		return GetFirstCommonAncestor(t1.parent, t2);
	}

	public static Transform GetFirstCommonAncestor(Transform[] transforms)
	{
		if (transforms == null)
		{
			Debug.LogWarning((object)"Transforms is null.");
			return null;
		}
		if (transforms.Length == 0)
		{
			Debug.LogWarning((object)"Transforms.Length is 0.");
			return null;
		}
		for (int i = 0; i < transforms.Length; i++)
		{
			if ((Object)(object)transforms[i] == (Object)null)
			{
				return null;
			}
			if (IsCommonAncestor(transforms[i], transforms))
			{
				return transforms[i];
			}
		}
		return GetFirstCommonAncestorRecursive(transforms[0], transforms);
	}

	public static Transform GetFirstCommonAncestorRecursive(Transform transform, Transform[] transforms)
	{
		if ((Object)(object)transform == (Object)null)
		{
			Debug.LogWarning((object)"Transform is null.");
			return null;
		}
		if (transforms == null)
		{
			Debug.LogWarning((object)"Transforms is null.");
			return null;
		}
		if (transforms.Length == 0)
		{
			Debug.LogWarning((object)"Transforms.Length is 0.");
			return null;
		}
		if (IsCommonAncestor(transform, transforms))
		{
			return transform;
		}
		if ((Object)(object)transform.parent == (Object)null)
		{
			return null;
		}
		return GetFirstCommonAncestorRecursive(transform.parent, transforms);
	}

	public static bool IsCommonAncestor(Transform transform, Transform[] transforms)
	{
		if ((Object)(object)transform == (Object)null)
		{
			Debug.LogWarning((object)"Transform is null.");
			return false;
		}
		for (int i = 0; i < transforms.Length; i++)
		{
			if ((Object)(object)transforms[i] == (Object)null)
			{
				Debug.Log((object)("Transforms[" + i + "] is null."));
				return false;
			}
			if (!IsAncestor(transforms[i], transform) && (Object)(object)transforms[i] != (Object)(object)transform)
			{
				return false;
			}
		}
		return true;
	}
}
