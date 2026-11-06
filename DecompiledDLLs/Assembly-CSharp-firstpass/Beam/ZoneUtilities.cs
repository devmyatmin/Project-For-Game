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
		return Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(zones.Select((Zone z) => z.SaveContainer.FindObject<T>(referenceId)));
	}

	public static T FindObjectRanged<T>(this IEnumerable<Zone> zones, MiniGuid referenceId, int startIndex, int range = -1) where T : ISaveableReference
	{
		return Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(zones.Select((Zone z) => z.SaveContainer.FindObjectRanged<T>(referenceId, startIndex, range)));
	}

	public static T FindObject<T>(this Transform transform, MiniGuid referenceId, bool includeInactive = false) where T : ISaveableReference
	{
		if (referenceId.IsDefault())
		{
			Debug.LogError((object)"FindObject:: Reference id was empty!");
			return default;
		}
		return Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(((Component)transform).GetComponentsInChildren<T>(includeInactive), (T obj, MiniGuid id) => obj.ReferenceId.Equals(id), referenceId);
	}

	public static T FindObjectRanged<T>(this Transform transform, MiniGuid referenceId, int startIndex, int range = -1) where T : ISaveableReference
	{
		if (referenceId.IsDefault())
		{
			Debug.LogError((object)"FindObjectRanged:: Reference id was empty!");
			return default;
		}
		int childCount = transform.childCount;
		int num = Mathf.Max(0, startIndex);
		int num2 = ((range == -1) ? childCount : Mathf.Min(startIndex + range, childCount));
		for (int i = num; i < num2; i++)
		{
			T component = ((Component)transform.GetChild(i)).GetComponent<T>();
			if (component != null && component.ReferenceId.Equals(referenceId))
			{
				return component;
			}
		}
		return default;
	}
}
