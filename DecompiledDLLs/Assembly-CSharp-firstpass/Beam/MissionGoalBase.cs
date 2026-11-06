using System;
using UnityEngine;

namespace Beam;

public abstract class MissionGoalBase : MonoBehaviour, IMissionGoal
{
	public abstract string Name { get; }

	public abstract Vector3 Location { get; }

	public event Action UpdateCompletion;

	public event Action Completed;

	public virtual void OnUpdateCompletion(float value)
	{
		UpdateCompletion?.Invoke();
	}

	public virtual void OnCompleted()
	{
		Completed?.Invoke();
	}

	public abstract float GetCompletionPercent();

	public abstract void Reset();

	protected virtual void OnDestroy()
	{
		UpdateCompletion = null;
		Completed = null;
	}
}
