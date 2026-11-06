using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverAim : IKSolverHeuristic
{
	public Transform transform;

	public Vector3 axis = Vector3.forward;

	public Vector3 poleAxis = Vector3.up;

	public Vector3 polePosition;

	[Range(0f, 1f)]
	public float poleWeight;

	public Transform poleTarget;

	[Range(0f, 1f)]
	public float clampWeight = 0.1f;

	[Range(0f, 2f)]
	public int clampSmoothing = 2;

	public IterationDelegate OnPreIteration;

	private float step;

	private Vector3 clampedIKPosition;

	private RotationLimit transformLimit;

	private Transform lastTransform;

	public Vector3 transformAxis
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			return transform.rotation * axis;
		}
	}

	public Vector3 transformPoleAxis
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			return transform.rotation * poleAxis;
		}
	}

	protected override int minBones => 1;

	protected override Vector3 localDirection
	{
		get
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			return bones[0].transform.InverseTransformDirection(bones[bones.Length - 1].transform.forward);
		}
	}

	public float GetAngle()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Angle(transformAxis, IKPosition - transform.position);
	}

	protected override void OnInitiate()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if ((firstInitiation || !Application.isPlaying) && (Object)(object)transform != (Object)null)
		{
			IKPosition = transform.position + transformAxis * 3f;
			polePosition = transform.position + transformPoleAxis * 3f;
		}
		for (int i = 0; i < bones.Length; i++)
		{
			if ((Object)(object)bones[i].rotationLimit != (Object)null)
			{
				bones[i].rotationLimit.Disable();
			}
		}
		step = 1f / (float)bones.Length;
		if (Application.isPlaying)
		{
			axis = axis.normalized;
		}
	}

	protected override void OnUpdate()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		if (axis == Vector3.zero)
		{
			if (!Warning.logged)
			{
				LogWarning("IKSolverAim axis is Vector3.zero.");
			}
			return;
		}
		if (poleAxis == Vector3.zero && poleWeight > 0f)
		{
			if (!Warning.logged)
			{
				LogWarning("IKSolverAim poleAxis is Vector3.zero.");
			}
			return;
		}
		if ((Object)(object)target != (Object)null)
		{
			IKPosition = target.position;
		}
		if ((Object)(object)poleTarget != (Object)null)
		{
			polePosition = poleTarget.position;
		}
		if (XY)
		{
			IKPosition.z = bones[0].transform.position.z;
		}
		if (IKPositionWeight <= 0f)
		{
			return;
		}
		IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
		if ((Object)(object)transform != (Object)(object)lastTransform)
		{
			transformLimit = ((Component)transform).GetComponent<RotationLimit>();
			if ((Object)(object)transformLimit != (Object)null)
			{
				((Behaviour)transformLimit).enabled = false;
			}
			lastTransform = transform;
		}
		if ((Object)(object)transformLimit != (Object)null)
		{
			transformLimit.Apply();
		}
		if ((Object)(object)transform == (Object)null)
		{
			if (!Warning.logged)
			{
				LogWarning("Aim Transform unassigned in Aim IK solver. Please Assign a Transform (lineal descendant to the last bone in the spine) that you want to be aimed at IKPosition");
			}
			return;
		}
		clampWeight = Mathf.Clamp(clampWeight, 0f, 1f);
		clampedIKPosition = GetClampedIKPosition();
		Vector3 val = clampedIKPosition - transform.position;
		val = Vector3.Slerp(transformAxis * val.magnitude, val, IKPositionWeight);
		clampedIKPosition = transform.position + val;
		for (int i = 0; i < maxIterations && (i < 1 || !(tolerance > 0f) || !(GetAngle() < tolerance)); i++)
		{
			lastLocalDirection = localDirection;
			if (OnPreIteration != null)
			{
				OnPreIteration(i);
			}
			Solve();
		}
		lastLocalDirection = localDirection;
	}

	private void Solve()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < bones.Length - 1; i++)
		{
			RotateToTarget(clampedIKPosition, bones[i], step * (float)(i + 1) * IKPositionWeight * bones[i].weight);
		}
		RotateToTarget(clampedIKPosition, bones[bones.Length - 1], IKPositionWeight * bones[bones.Length - 1].weight);
	}

	private Vector3 GetClampedIKPosition()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		if (clampWeight <= 0f)
		{
			return IKPosition;
		}
		if (clampWeight >= 1f)
		{
			Vector3 position = transform.position;
			Vector3 val = transformAxis;
			Vector3 val2 = IKPosition - transform.position;
			return position + val * val2.magnitude;
		}
		float num = Vector3.Angle(transformAxis, IKPosition - transform.position);
		float num2 = 1f - num / 180f;
		float num3 = ((clampWeight > 0f) ? Mathf.Clamp(1f - (clampWeight - num2) / (1f - num2), 0f, 1f) : 1f);
		float num4 = ((clampWeight > 0f) ? Mathf.Clamp(num2 / clampWeight, 0f, 1f) : 1f);
		for (int i = 0; i < clampSmoothing; i++)
		{
			num4 = Mathf.Sin(num4 * (float)Math.PI * 0.5f);
		}
		return transform.position + Vector3.Slerp(transformAxis * 10f, IKPosition - transform.position, num4 * num3);
	}

	private void RotateToTarget(Vector3 targetPosition, Bone bone, float weight)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		if (XY)
		{
			if (weight >= 0f)
			{
				Vector3 val = transformAxis;
				Vector3 val2 = targetPosition - transform.position;
				float num = Mathf.Atan2(val.x, val.y) * 57.29578f;
				float num2 = Mathf.Atan2(val2.x, val2.y) * 57.29578f;
				bone.transform.rotation = Quaternion.AngleAxis(Mathf.DeltaAngle(num, num2), Vector3.back) * bone.transform.rotation;
			}
		}
		else
		{
			if (weight >= 0f)
			{
				Quaternion val3 = Quaternion.FromToRotation(transformAxis, targetPosition - transform.position);
				if (weight >= 1f)
				{
					bone.transform.rotation = val3 * bone.transform.rotation;
				}
				else
				{
					bone.transform.rotation = Quaternion.Lerp(Quaternion.identity, val3, weight) * bone.transform.rotation;
				}
			}
			if (poleWeight > 0f)
			{
				Vector3 val4 = polePosition - transform.position;
				Vector3 val5 = transformAxis;
				Vector3.OrthoNormalize(ref val5, ref val4);
				Quaternion val6 = Quaternion.FromToRotation(transformPoleAxis, val4);
				bone.transform.rotation = Quaternion.Lerp(Quaternion.identity, val6, weight * poleWeight) * bone.transform.rotation;
			}
		}
		if (useRotationLimits && (Object)(object)bone.rotationLimit != (Object)null)
		{
			bone.rotationLimit.Apply();
		}
	}

	public IKSolverAim()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
	}
}
