using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class InteractionLookAt
{
	[Tooltip("(Optional) reference to the LookAtIK component that will be used to make the character look at the objects that it is interacting with.")]
	public LookAtIK ik;

	[Tooltip("Interpolation speed of the LookAtIK target.")]
	public float lerpSpeed = 5f;

	[Tooltip("Interpolation speed of the LookAtIK weight.")]
	public float weightSpeed = 1f;

	[HideInInspector]
	public bool isPaused;

	private Transform lookAtTarget;

	private float stopLookTime;

	private float weight;

	private bool firstFBBIKSolve;

	public void Look(Transform target, float time)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ik == (Object)null))
		{
			if (ik.solver.IKPositionWeight <= 0f)
			{
				ik.solver.IKPosition = ik.solver.GetRoot().position + ik.solver.GetRoot().forward * 3f;
			}
			lookAtTarget = target;
			stopLookTime = time;
		}
	}

	public void OnFixTransforms()
	{
		if (!((Object)(object)ik == (Object)null) && ik.fixTransforms)
		{
			ik.solver.FixTransforms();
		}
	}

	public void Update()
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ik == (Object)null)
		{
			return;
		}
		if (((Behaviour)ik).enabled)
		{
			((Behaviour)ik).enabled = false;
		}
		if (!((Object)(object)lookAtTarget == (Object)null))
		{
			if (isPaused)
			{
				stopLookTime += Time.deltaTime;
			}
			float num = ((Time.time < stopLookTime) ? weightSpeed : (0f - weightSpeed));
			weight = Mathf.Clamp(weight + num * Time.deltaTime, 0f, 1f);
			ik.solver.IKPositionWeight = Interp.Float(weight, InterpolationMode.InOutQuintic);
			ik.solver.IKPosition = Vector3.Lerp(ik.solver.IKPosition, lookAtTarget.position, lerpSpeed * Time.deltaTime);
			if (weight <= 0f)
			{
				lookAtTarget = null;
			}
			firstFBBIKSolve = true;
		}
	}

	public void SolveSpine()
	{
		if (!((Object)(object)ik == (Object)null) && firstFBBIKSolve)
		{
			float headWeight = ik.solver.headWeight;
			float eyesWeight = ik.solver.eyesWeight;
			ik.solver.headWeight = 0f;
			ik.solver.eyesWeight = 0f;
			ik.solver.Update();
			ik.solver.headWeight = headWeight;
			ik.solver.eyesWeight = eyesWeight;
		}
	}

	public void SolveHead()
	{
		if (!((Object)(object)ik == (Object)null) && firstFBBIKSolve)
		{
			float bodyWeight = ik.solver.bodyWeight;
			ik.solver.bodyWeight = 0f;
			ik.solver.Update();
			ik.solver.bodyWeight = bodyWeight;
			firstFBBIKSolve = false;
		}
	}
}
