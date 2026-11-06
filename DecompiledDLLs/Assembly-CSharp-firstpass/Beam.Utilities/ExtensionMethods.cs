using System;
using System.Collections.Generic;
using Beam.Crafting;
using Funlabs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Beam.Utilities;

public static class ExtensionMethods
{
	public static bool IsPrefab(this GameObject gameObject)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)gameObject == (Object)null)
		{
			throw new ArgumentNullException("gameObject");
		}
		Scene scene = gameObject.scene;
		if (!scene.IsValid())
		{
			scene = gameObject.scene;
			if (!scene.isLoaded)
			{
				return !((Enum)((Object)gameObject).hideFlags).HasFlag((Enum)(object)(HideFlags)1);
			}
		}
		return false;
	}

	public static void SetLayerRecursively(this GameObject go, int layer)
	{
		if (Object.op_Implicit((Object)(object)go))
		{
			Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				((Component)componentsInChildren[i]).gameObject.layer = layer;
			}
		}
	}

	public static TElement FirstOrDefault_NonAlloc<TElement>(this IList<TElement> list)
	{
		if (list != null && list.Count > 0)
		{
			return list[0];
		}
		return default;
	}

	public static TElement FirstOrDefault_NonAlloc<TElement>(this IEnumerable<TElement> enumerable)
	{
		if (enumerable == null)
		{
			return default;
		}
		using (IEnumerator<TElement> enumerator = enumerable.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		return default;
	}

	public static TElement FirstOrDefault_NonAlloc<TElement, TArg>(this IEnumerable<TElement> enumerable, Func<TElement, TArg, bool> predicate, TArg arg)
	{
		if (enumerable == null || predicate == null)
		{
			return default;
		}
		foreach (TElement item in enumerable)
		{
			if (predicate(item, arg))
			{
				return item;
			}
		}
		return default;
	}

	public static T LastOrDefault_NonAlloc<T>(this IList<T> list)
	{
		if (list != null && list.Count > 0)
		{
			return list[list.Count - 1];
		}
		return default;
	}

	public static bool IsLast<T>(this IList<T> list, int index)
	{
		return index == ((list != null) ? new int?(list.Count - 1) : ((int?)null));
	}

	public static void Shuffle<T>(this IList<T> list)
	{
		Random random = new Random();
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = random.Next(num + 1);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	public static bool Any_NonAlloc<TElement>(this IEnumerable<TElement> collection)
	{
		if (collection == null)
		{
			return false;
		}
		using (IEnumerator<TElement> enumerator = collection.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				return true;
			}
		}
		return false;
	}

	public static bool Any_NonAlloc<TElement, TArg>(this IEnumerable<TElement> collection, Func<TElement, TArg, bool> predicate, TArg arg)
	{
		if (collection == null || predicate == null)
		{
			return false;
		}
		foreach (TElement item in collection)
		{
			if (predicate(item, arg))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Any_NonAlloc<TElement, TArg>(this IList<TElement> list, Func<TElement, TArg, bool> predicate, TArg arg)
	{
		if (list == null || predicate == null)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			TElement arg2 = list[i];
			if (predicate(arg2, arg))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Any_NonAlloc<TElement>(this IList<TElement> list, Func<TElement, bool> predicate)
	{
		if (list == null || predicate == null)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			TElement arg = list[i];
			if (predicate(arg))
			{
				return true;
			}
		}
		return false;
	}

	public static bool All_NonAlloc<TElement>(this IEnumerable<TElement> collection, Func<TElement, bool> predicate)
	{
		if (collection == null || predicate == null)
		{
			return false;
		}
		foreach (TElement item in collection)
		{
			if (!predicate(item))
			{
				return false;
			}
		}
		return true;
	}

	public static void ForEach_NonAlloc<TElement>(this IEnumerable<TElement> enumerable, Action<TElement> del)
	{
		if (enumerable == null || del == null)
		{
			return;
		}
		foreach (TElement item in enumerable)
		{
			del(item);
		}
	}

	public static void ForEach_NonAlloc<TElement>(this IList<TElement> list, Action<TElement> del)
	{
		if (list != null && del != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				TElement obj = list[i];
				del(obj);
			}
		}
	}

	public static bool DoesntContain<T>(this IList<T> list, T item)
	{
		return !list.Contains(item);
	}

	public static List<T> Swap<T>(this List<T> list, int indexA, int indexB)
	{
		T value = list[indexA];
		list[indexA] = list[indexB];
		list[indexB] = value;
		return list;
	}

	public static bool Find<T>(this IList<T> list, T obj)
	{
		if (list == null)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Equals(obj))
			{
				return true;
			}
		}
		return false;
	}

	public static int FindIndex<T>(this IEnumerable<T> collection, Predicate<T> predicate)
	{
		if (collection == null || predicate == null)
		{
			return -1;
		}
		int num = 0;
		foreach (T item in collection)
		{
			if (predicate(item))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public static MiniGuid ToMiniGuid(this string v)
	{
		if (string.IsNullOrEmpty(v))
		{
			return default;
		}
		try
		{
			return new MiniGuid(new Guid(v));
		}
		catch (Exception)
		{
			Debug.LogError((object)(v + " - unable to covert to Guid"));
			return default;
		}
	}

	public static MiniGuid ToMiniGuid(this ulong v)
	{
		return new MiniGuid(v);
	}

	public static string ToSize(this int value, ESizeUnits unit)
	{
		return ((double)value / Math.Pow(1024.0, (double)unit)).ToString("0.00");
	}

	public static void CheckForProjectilesInChildren(this Transform transform)
	{
		BaseObject[] componentsInChildren = ((Component)transform).gameObject.GetComponentsInChildren<BaseObject>();
		foreach (BaseObject baseObject in componentsInChildren)
		{
			if (!((Object)(object)baseObject.transform == (Object)(object)transform) && ((Component)baseObject).gameObject.layer == Layers.PROJECTILES && baseObject is IProjectile projectile)
			{
				((Component)baseObject).gameObject.transform.parent = null;
				baseObject.TransformParent = null;
				baseObject.Parent();
				projectile.Reset();
			}
		}
	}

	public static void DestroyProjectilesInChildren(this Transform transform)
	{
		BaseObject[] componentsInChildren = ((Component)transform).gameObject.GetComponentsInChildren<BaseObject>();
		foreach (BaseObject baseObject in componentsInChildren)
		{
			if (!((Object)(object)baseObject.transform == (Object)(object)transform) && ((Component)baseObject).gameObject.layer == Layers.PROJECTILES && baseObject is IProjectile)
			{
				MultiplayerMng.Destroy(((Component)baseObject).GetComponent<SaveablePrefab>());
			}
		}
	}

	public static Transform FindNestedChild(this Transform parent, string name)
	{
		if (((Object)parent).name == name)
		{
			return parent;
		}
		for (int i = 0; i < parent.childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if (((Object)child).name == name)
			{
				return child;
			}
			Transform val = FindNestedChild(child, name);
			if ((Object)(object)val != (Object)null)
			{
				return val;
			}
		}
		return null;
	}

	public static bool ContainsNestedChild(this Transform parent, Transform child)
	{
		if ((Object)(object)parent == (Object)(object)child)
		{
			return Object.op_Implicit((Object)(object)parent);
		}
		for (int i = 0; i < parent.childCount; i++)
		{
			Transform child2 = parent.GetChild(i);
			if ((Object)(object)child2 == (Object)(object)child)
			{
				return true;
			}
			if (ContainsNestedChild(child2, child))
			{
				return true;
			}
		}
		return false;
	}

	public static Transform GetFirstChild(this Transform parent, bool active = false)
	{
		for (int i = 0; i < parent.childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if ((Object)(object)child != (Object)null && (!active || ((Component)child).gameObject.activeSelf))
			{
				return child;
			}
		}
		return null;
	}

	public static void ForEachChild(this Transform transform, Action<Transform> del)
	{
		if (transform.childCount != 0)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				del(child);
				ForEachChild(child, del);
			}
		}
	}
}
