using System;
using UnityEngine;

namespace Beam.Serialization;

public static class LoadingActionUtilities
{
	public static void ExecuteDeferredAction(Action action)
	{
		if (action == null)
		{
			Debug.LogError("LoadingActionUtilities::ExecuteDeferredAction:: Action is null.");
			return;
		}
		try
		{
			action();
		}
		catch (Exception ex)
		{
			Debug.LogError("LoadingActionUtilities::ExecuteDeferredAction:: " + ex);
		}
	}
}
