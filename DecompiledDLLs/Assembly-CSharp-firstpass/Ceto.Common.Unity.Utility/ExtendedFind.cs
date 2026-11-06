using System.Collections.Generic;
using UnityEngine;

namespace Ceto.Common.Unity.Utility;

public static class ExtendedFind
{
	public static T GetInterface<T>(GameObject obj) where T : class
	{
		Component[] components = obj.GetComponents<Component>();
		foreach (Component val in components)
		{
			if (val is T)
			{
				return val as T;
			}
		}
		return null;
	}

	public static T GetInterfaceInChildren<T>(GameObject obj) where T : class
	{
		Component[] componentsInChildren = obj.GetComponentsInChildren<Component>();
		foreach (Component val in componentsInChildren)
		{
			if (val is T)
			{
				return val as T;
			}
		}
		return null;
	}

	public static T GetInterfaceImmediateChildren<T>(GameObject obj) where T : class
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		foreach (Transform item in obj.transform)
		{
			Component[] components = ((Component)item).GetComponents<Component>();
			foreach (Component val in components)
			{
				if (val is T)
				{
					return val as T;
				}
			}
		}
		return null;
	}

	public static T[] GetInterfaces<T>(GameObject obj) where T : class
	{
		Component[] components = obj.GetComponents<Component>();
		List<T> list = new List<T>();
		Component[] array = components;
		foreach (Component val in array)
		{
			if (val is T)
			{
				list.Add(val as T);
			}
		}
		return list.ToArray();
	}

	public static T[] GetInterfacesInChildren<T>(GameObject obj) where T : class
	{
		Component[] componentsInChildren = obj.GetComponentsInChildren<Component>();
		List<T> list = new List<T>();
		Component[] array = componentsInChildren;
		foreach (Component val in array)
		{
			if (val is T)
			{
				list.Add(val as T);
			}
		}
		return list.ToArray();
	}

	public static T[] GetInterfacesImmediateChildren<T>(GameObject obj) where T : class
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		List<T> list = new List<T>();
		foreach (Transform item in obj.transform)
		{
			Component[] components = ((Component)item).GetComponents<Component>();
			foreach (Component val in components)
			{
				if (val is T)
				{
					list.Add(val as T);
				}
			}
		}
		return list.ToArray();
	}

	public static T GetComponetInImmediateParent<T>(GameObject obj) where T : Component
	{
		if ((Object)(object)obj.transform.parent == (Object)null)
		{
			return default;
		}
		return ((Component)obj.transform.parent).GetComponent<T>();
	}

	public static T[] GetComponentsInImmediateParent<T>(GameObject obj) where T : Component
	{
		if ((Object)(object)obj.transform.parent == (Object)null)
		{
			return new T[0];
		}
		return ((Component)obj.transform.parent).GetComponents<T>();
	}

	public static T GetComponetInImmediateChildren<T>(GameObject obj) where T : Component
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		foreach (Transform item in obj.transform)
		{
			T component = ((Component)item).GetComponent<T>();
			if ((Object)(object)component != (Object)null)
			{
				return component;
			}
		}
		return default;
	}

	public static T[] GetComponetsInImmediateChildren<T>(GameObject obj) where T : Component
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		List<T> list = new List<T>();
		foreach (Transform item2 in obj.transform)
		{
			T[] components = ((Component)item2).GetComponents<T>();
			foreach (T item in components)
			{
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	public static T FindComponentOnGameObject<T>(string name) where T : Component
	{
		GameObject val = GameObject.Find(name);
		if ((Object)(object)val == (Object)null)
		{
			return default;
		}
		return val.GetComponent<T>();
	}

	public static T[] FindComponentsOnGameObject<T>(string name) where T : Component
	{
		GameObject val = GameObject.Find(name);
		if ((Object)(object)val == (Object)null)
		{
			return new T[0];
		}
		return val.GetComponents<T>();
	}

	public static T FindInterfaceOnGameObject<T>(string name) where T : class
	{
		GameObject val = GameObject.Find(name);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		return GetInterface<T>(val);
	}

	public static T[] FindInterfacesOnGameObject<T>(string name) where T : class
	{
		GameObject val = GameObject.Find(name);
		if ((Object)(object)val == (Object)null)
		{
			return new T[0];
		}
		return GetInterfaces<T>(val);
	}
}
