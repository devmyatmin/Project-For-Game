using System.Collections.Generic;
using UnityEngine;

namespace Beam.Utilities;

public static class BeamPhysics
{
	public static IList<T> GetComponentsInBounds<T>(Collider[] colliders, int layermask) where T : Component
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		List<T> list = new List<T>();
		foreach (Collider val in colliders)
		{
			Bounds bounds = val.bounds;
			Vector3 min = bounds.min;
			bounds = val.bounds;
			Vector3 max = bounds.max;
			Vector3 val2 = min - max;
			float num = val2.magnitude * Mathf.Sqrt(2f) * 0.5f;
			bounds = val.bounds;
			Collider[] array = Physics.OverlapSphere(bounds.center, num, layermask);
			for (int j = 0; j < array.Length; j++)
			{
				bounds = val.bounds;
				if (bounds.Intersects(array[j].bounds))
				{
					T component = ((Component)array[j]).GetComponent<T>();
					if ((Object)(object)component != (Object)null && !list.Contains(component))
					{
						list.Add(component);
					}
				}
			}
		}
		return list;
	}

	public static IList<T> GetComponentsInSphere<T>(Vector3 position, float radius, int layermask) where T : Component
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		Collider[] array = Physics.OverlapSphere(position, radius, layermask, (QueryTriggerInteraction)0);
		List<T> list = new List<T>();
		Collider[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			T component = ((Component)array2[i]).GetComponent<T>();
			if ((Object)(object)component != (Object)null && !list.Contains(component))
			{
				list.Add(component);
			}
		}
		return list;
	}

	public static int GetComponentsInSphere_MinAlloc<T>(Vector3 position, float radius, int layermask, Collider[] colliders, T[] results) where T : Component
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (results == null || results.Length == 0 || colliders == null || colliders.Length == 0)
		{
			return 0;
		}
		int num = Physics.OverlapSphereNonAlloc(position, radius, colliders, layermask);
		if (num > 0)
		{
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				T component = ((Component)colliders[i]).GetComponent<T>();
				if (!((Object)(object)component != (Object)null))
				{
					continue;
				}
				bool flag = false;
				for (int j = 0; j < num2; j++)
				{
					if ((Object)(object)results[j] == (Object)(object)component)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					results[num2] = component;
					num2++;
					if (num2 >= results.Length)
					{
						break;
					}
				}
			}
			return num2;
		}
		return 0;
	}

	public static T GetComponentSphereCast<T>(Ray ray, float radius, float distance, int layermask) where T : Component
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default;
		if (Physics.SphereCast(ray, radius, ref val, distance, layermask))
		{
			return ((Component)val.collider).GetComponent<T>();
		}
		return default;
	}

	public static IList<T> GetComponentSphereCastAll<T>(Ray ray, float radius, float distance, int layermask) where T : Component
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit[] array = Physics.SphereCastAll(ray, radius, distance, layermask);
		List<T> list = new List<T>();
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit val = array2[i];
			T component = ((Component)val.collider).GetComponent<T>();
			if ((Object)(object)component != (Object)null && !list.Contains(component))
			{
				list.Add(component);
			}
		}
		return list;
	}

	public static T GetComponentRaycast<T>(Ray ray, float distance, int layermask, RaycastHitGetBody getBody = RaycastHitGetBody.Collider) where T : Component
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default;
		if (Physics.Raycast(ray, ref val, distance, layermask))
		{
			if (getBody == RaycastHitGetBody.Rigidbody && (Object)(object)val.rigidbody != (Object)null)
			{
				return ((Component)val.rigidbody).GetComponent<T>();
			}
			return ((Component)val.collider).GetComponent<T>();
		}
		return default;
	}

	public static T GetComponentRaycastAll<T>(Ray ray, float distance, int layermask) where T : Component
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit[] array = Physics.RaycastAll(ray, distance, layermask);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit val = array[i];
			T component = ((Component)val.collider).GetComponent<T>();
			if ((Object)(object)component != (Object)null)
			{
				return component;
			}
		}
		return default;
	}

	public static IList<T> GetComponentsRaycastAll<T>(Ray ray, float distance, int layermask) where T : Component
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit[] array = Physics.RaycastAll(ray, distance, layermask);
		if (array == null || array.Length == 0)
		{
			return null;
		}
		List<T> list = new List<T>();
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit val = array2[i];
			T component = ((Component)val.collider).GetComponent<T>();
			if ((Object)(object)component != (Object)null && !list.Contains(component))
			{
				list.Add(component);
			}
		}
		return list;
	}

	public static bool CheckComponentInSphere<T>(Vector3 position, float radius, int layermask) where T : Component
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		Collider[] array = Physics.OverlapSphere(position, radius, layermask);
		for (int i = 0; i < array.Length; i++)
		{
			if (Object.op_Implicit((Object)(object)((Component)array[i]).GetComponent<T>()))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CheckComponentRaycast<T>(Ray ray, float distance, int layermask) where T : Component
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default;
		if (Physics.Raycast(ray, ref val, distance, layermask) && Object.op_Implicit((Object)(object)((Component)val.collider).GetComponent<T>()))
		{
			return true;
		}
		return false;
	}

	public static bool CheckComponentSpherecast<T>(Ray ray, float radius, float distance, int layermask) where T : Component
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default;
		if (Physics.SphereCast(ray, radius, ref val, distance, layermask) && Object.op_Implicit((Object)(object)((Component)val.collider).GetComponent<T>()))
		{
			return true;
		}
		return false;
	}
}
