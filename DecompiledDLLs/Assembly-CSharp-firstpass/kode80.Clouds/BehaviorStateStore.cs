using System;
using System.Linq;
using UnityEngine;

namespace kode80.Clouds;

[Serializable]
public class BehaviorStateStore
{
	[SerializeField]
	private string[] _names;

	[SerializeField]
	private bool[] _enabledStates;

	public void StoreStates(MonoBehaviour[] behaviors)
	{
		_names = behaviors.Select((MonoBehaviour x) => ((object)x).GetType().Name).ToArray();
		_enabledStates = behaviors.Select((MonoBehaviour x) => ((Behaviour)x).enabled).ToArray();
	}

	public void ApplyStates(MonoBehaviour[] behaviors)
	{
		if (_names == null || _enabledStates == null)
		{
			return;
		}
		int num = _names.GetLowerBound(0) - 1;
		foreach (MonoBehaviour val in behaviors)
		{
			int num2 = Array.IndexOf(_names, ((object)val).GetType().Name);
			if (num2 > num)
			{
				((Behaviour)val).enabled = _enabledStates[num2];
			}
		}
	}
}
