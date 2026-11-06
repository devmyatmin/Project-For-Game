using System;
using System.Collections;
using UnityEngine;

namespace RootMotion.FinalIK;

public abstract class OffsetModifierVRIK : MonoBehaviour
{
	[Tooltip("The master weight")]
	public float weight = 1f;

	[Tooltip("Reference to the VRIK component")]
	public VRIK ik;

	private float lastTime;

	protected float deltaTime => Time.time - lastTime;

	protected abstract void OnModifyOffset();

	protected virtual void Start()
	{
		((MonoBehaviour)this).StartCoroutine(Initiate());
	}

	private IEnumerator Initiate()
	{
		while ((Object)(object)ik == (Object)null)
		{
			yield return null;
		}
		IKSolverVR solver = ik.solver;
		solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPreUpdate, new IKSolver.UpdateDelegate(ModifyOffset));
		lastTime = Time.time;
	}

	private void ModifyOffset()
	{
		if (((Behaviour)this).enabled && !(weight <= 0f) && !(deltaTime <= 0f) && !((Object)(object)ik == (Object)null))
		{
			weight = Mathf.Clamp(weight, 0f, 1f);
			OnModifyOffset();
			lastTime = Time.time;
		}
	}

	protected virtual void OnDestroy()
	{
		if ((Object)(object)ik != (Object)null)
		{
			IKSolverVR solver = ik.solver;
			solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPreUpdate, new IKSolver.UpdateDelegate(ModifyOffset));
		}
	}
}
