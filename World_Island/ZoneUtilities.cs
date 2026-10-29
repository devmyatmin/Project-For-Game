using System.Collections.Generic;
using System.Linq;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam;

public static class ZoneUtilities
{
	public static T FindObject<T>(this IEnumerable<Zone> zones, MiniGuid referenceId) where T : ISaveableReference
	{
		return zones.Select((Zone z) => z.SaveContainer.FindObject<T>(referenceId)).FirstOrDefault_NonAlloc();
	}

	public static T FindObjectRanged<T>(this IEnumerable<Zone> zones, MiniGuid referenceId, int startIndex, int range = -1) where T : ISaveableReference
	{
		return zones.Select((Zone z) => z.SaveContainer.FindObjectRanged<T>(referenceId, startIndex, range)).FirstOrDefault_NonAlloc();
	}

	public static T FindObject<T>(this Transform transform, MiniGuid referenceId, bool includeInactive = false) where T : ISaveableReference
	{
		if (referenceId.IsDefault())
		{
			Debug.LogError("FindObject:: Reference id was empty!");
			return default(T);
		}
		return transform.GetComponentsInChildren<T>(includeInactive).FirstOrDefault_NonAlloc((T obj, MiniGuid id) => obj.ReferenceId.Equals(id), referenceId);
	}

	public static T FindObjectRanged<T>(this Transform transform, MiniGuid referenceId, int startIndex, int range = -1) where T : ISaveableReference
	{
		if (referenceId.IsDefault())
		{
			Debug.LogError("FindObjectRanged:: Reference id was empty!");
			return default(T);
		}
		int childCount = transform.childCount;
		int num = Mathf.Max(0, startIndex);
		int num2 = ((range == -1) ? childCount : Mathf.Min(startIndex + range, childCount));
		for (int i = num; i < num2; i++)
		{
			T component = transform.GetChild(i).GetComponent<T>();
			if (component != null && component.ReferenceId.Equals(referenceId))
			{
				return component;
			}
		}
		return default(T);
	}
}
