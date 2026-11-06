using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beam.UI;
using MEC;
using UnityEngine;

namespace Beam.Utilities;

public static class BeamTeam
{
	public static CoroutineHandle RunCoroutine(this Timing timing, IEnumerator<float> enumerator, Action<Exception> onComplete)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return timing.RunCoroutineOnInstance(RunThrowingIterator(enumerator, onComplete));
	}

	public static IEnumerator<float> RunThrowingIterator(IEnumerator<float> enumerator, Action<Exception> onComplete)
	{
		while (true)
		{
			float current;
			try
			{
				if (!enumerator.MoveNext())
				{
					break;
				}
				current = enumerator.Current;
			}
			catch (Exception obj)
			{
				onComplete(obj);
				break;
			}
			onComplete(null);
			yield return current;
		}
	}

	public static float WaitForTask<T>(Func<Task<T>> task, Action<T> callback)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Timing.WaitUntilDone(Timing.RunCoroutine(WaitForTask_Sequence(task, callback)));
	}

	private static IEnumerator<float> WaitForTask_Sequence<T>(Func<Task<T>> task, Action<T> callback)
	{
		Task<T> t = Task.Run(task);
		while (!t.IsCompleted)
		{
			yield return 0f;
		}
		callback?.Invoke(t.Result);
	}

	public static void InvokeDelayed(float delay, Action callback)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (callback != null)
		{
			Timing.RunCoroutine(InvokeDelayed_Sequence(delay, callback));
		}
	}

	private static IEnumerator<float> InvokeDelayed_Sequence(float delay, Action callback)
	{
		yield return float.NegativeInfinity;
		float timer = 0f;
		while (timer < delay)
		{
			timer += Time.unscaledDeltaTime;
			yield return 0f;
		}
		callback?.Invoke();
	}

	public static void CopyDirectory(string srcDir, string dstDir, bool overwrite = false)
	{
		if (!Directory.Exists(srcDir))
		{
			Debug.LogError((object)(srcDir + " does not exist!"));
			return;
		}
		if (!Directory.Exists(dstDir))
		{
			Directory.CreateDirectory(dstDir);
		}
		foreach (string item in from name in Directory.EnumerateFiles(srcDir)
			where !name.EndsWith(".meta", StringComparison.InvariantCultureIgnoreCase)
			select name)
		{
			File.Copy(item, Path.Combine(dstDir, Path.GetFileName(item)), overwrite);
		}
		string[] directories = Directory.GetDirectories(srcDir);
		foreach (string text in directories)
		{
			CopyDirectory(text, Path.Combine(dstDir, Path.GetFileName(text)));
		}
	}

	public static float LinearToDecibel(float value)
	{
		if (!(value > 0f))
		{
			return -144f;
		}
		return 20f * Mathf.Log10(value);
	}

	public static void SafeStopCoroutine(this MonoBehaviour behaviour, Coroutine coroutine)
	{
		if (coroutine != null)
		{
			behaviour.StopCoroutine(coroutine);
		}
	}

	public static TInterface GetInterface<TInterface>(this GameObject gameObject) where TInterface : class
	{
		return gameObject.GetComponent(typeof(TInterface)) as TInterface;
	}

	public static TInterface GetInterface<TInterface>(this Component behaviour) where TInterface : class
	{
		return behaviour.GetComponent(typeof(TInterface)) as TInterface;
	}

	public static TInterface GetInterfaceInChildren<TInterface>(this Component behaviour, bool includeInactive) where TInterface : class
	{
		return behaviour.GetComponentInChildren(typeof(TInterface), includeInactive) as TInterface;
	}

	public static TInterface[] GetInterfaces<TInterface>(this Component behaviour) where TInterface : class
	{
		return behaviour.GetComponents(typeof(TInterface)) as TInterface[];
	}

	public static TInterface[] GetInterfacesInChildren<TInterface>(this Component behaviour, bool includeInactive) where TInterface : class
	{
		return behaviour.GetComponentsInChildren(typeof(TInterface), includeInactive) as TInterface[];
	}

	public static Transform FindChildWithTag(this Transform transform, string tag)
	{
		Transform[] componentsInChildren = ((Component)transform).GetComponentsInChildren<Transform>(true);
		foreach (Transform val in componentsInChildren)
		{
			if (((Component)val).CompareTag(tag))
			{
				return val;
			}
		}
		return null;
	}

	public static bool IsNullOrDestroyed(this object value)
	{
		return value?.Equals(null) ?? true;
	}

	public static bool IsValid(this object value)
	{
		return !IsNullOrDestroyed(value);
	}

	public static void Destroy<T>(T component) where T : Object
	{
		if (Object.op_Implicit((Object)(object)component))
		{
			Object.Destroy((Object)(object)component);
			component = default;
		}
	}

	public static void DestroyAll<T>(T[] components) where T : Object
	{
		for (int num = components.Length - 1; num >= 0; num--)
		{
			Destroy(components[num]);
		}
	}

	public static void DestroyImmediate<T>(T component, bool sceneObjects = false) where T : Object
	{
		if (Object.op_Implicit((Object)(object)component))
		{
			Object.DestroyImmediate((Object)(object)component, sceneObjects);
			component = default;
		}
	}

	public static void DestroyAllImmediate<T>(T[] components, bool sceneObjects = false) where T : Object
	{
		for (int num = components.Length - 1; num >= 0; num--)
		{
			DestroyImmediate(components[num], sceneObjects);
		}
	}

	public static void DestroyAllChildren(Transform transform)
	{
		if ((Object)(object)transform == (Object)null)
		{
			return;
		}
		Transform[] componentsInChildren = ((Component)transform).GetComponentsInChildren<Transform>(true);
		for (int num = componentsInChildren.Length - 1; num >= 0; num--)
		{
			Transform val = componentsInChildren[num];
			if (!((Object)(object)val == (Object)(object)transform) && !IsNullOrDestroyed(val))
			{
				BeamTeam.Destroy<GameObject>(((Component)val).gameObject);
			}
		}
	}

	public static void DestroyAllChildrenImmediate(Transform transform)
	{
		if ((Object)(object)transform == (Object)null)
		{
			return;
		}
		Transform[] componentsInChildren = ((Component)transform).GetComponentsInChildren<Transform>(true);
		for (int num = componentsInChildren.Length - 1; num >= 0; num--)
		{
			Transform val = componentsInChildren[num];
			if (!((Object)(object)val == (Object)(object)transform) && !IsNullOrDestroyed(val))
			{
				BeamTeam.DestroyImmediate<GameObject>(((Component)val).gameObject, false);
			}
		}
	}

	public static void DestroyAllChildren(ITransformViewAdapter transform)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (transform == null || (Object)(object)transform.Transform == (Object)null)
		{
			return;
		}
		foreach (Transform item in transform.Transform)
		{
			BeamTeam.Destroy<GameObject>(((Component)item).gameObject);
		}
	}
}
