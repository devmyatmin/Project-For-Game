using System;
using UnityEngine;

namespace Beam;

public class MissionTrigger : MonoBehaviour
{
	protected bool Enabled { get; set; } = true;

	public event Action Triggered;

	public virtual void OnTriggered()
	{
		if (Enabled)
		{
			Triggered?.Invoke();
		}
	}

	protected virtual void OnDestroy()
	{
		Triggered = null;
	}

	public virtual void Enable()
	{
		Enabled = true;
	}

	public virtual void Disable()
	{
		Enabled = false;
	}
}
