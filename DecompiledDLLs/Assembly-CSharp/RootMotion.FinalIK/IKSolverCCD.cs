using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverCCD : IKSolverHeuristic
{
	public IterationDelegate OnPreIteration;

	public void FadeOutBoneWeights()
	{
		if (bones.Length >= 2)
		{
			bones[0].weight = 1f;
			float num = 1f / (float)(bones.Length - 1);
			for (int i = 1; i < bones.Length; i++)
			{
				bones[i].weight = num * (float)(bones.Length - 1 - i);
			}
		}
	}

	protected override void OnInitiate()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (firstInitiation || !Application.isPlaying)
		{
			IKPosition = bones[bones.Length - 1].transform.position;
		}
		InitiateBones();
	}

	protected override void OnUpdate()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		if (IKPositionWeight <= 0f)
		{
			return;
		}
		IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
		if ((Object)(object)target != (Object)null)
		{
			IKPosition = target.position;
		}
		if (XY)
		{
			IKPosition.z = bones[0].transform.position.z;
		}
		Vector3 val = ((maxIterations > 1) ? GetSingularityOffset() : Vector3.zero);
		for (int i = 0; i < maxIterations && (!(val == Vector3.zero) || i < 1 || !(tolerance > 0f) || !(positionOffset < tolerance * tolerance)); i++)
		{
			lastLocalDirection = localDirection;
			if (OnPreIteration != null)
			{
				OnPreIteration(i);
			}
			Solve(IKPosition + ((i == 0) ? val : Vector3.zero));
		}
		lastLocalDirection = localDirection;
	}

	protected void Solve(Vector3 targetPosition)
	{
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		if (XY)
		{
			for (int num = bones.Length - 2; num > -1; num--)
			{
				float num2 = bones[num].weight * IKPositionWeight;
				if (num2 > 0f)
				{
					Vector3 val = bones[bones.Length - 1].transform.position - bones[num].transform.position;
					Vector3 val2 = targetPosition - bones[num].transform.position;
					float num3 = Mathf.Atan2(val.x, val.y) * 57.29578f;
					float num4 = Mathf.Atan2(val2.x, val2.y) * 57.29578f;
					bones[num].transform.rotation = Quaternion.AngleAxis(Mathf.DeltaAngle(num3, num4) * num2, Vector3.back) * bones[num].transform.rotation;
				}
				if (useRotationLimits && (Object)(object)bones[num].rotationLimit != (Object)null)
				{
					bones[num].rotationLimit.Apply();
				}
			}
			return;
		}
		for (int num5 = bones.Length - 2; num5 > -1; num5--)
		{
			float num6 = bones[num5].weight * IKPositionWeight;
			if (num6 > 0f)
			{
				Vector3 val3 = bones[bones.Length - 1].transform.position - bones[num5].transform.position;
				Vector3 val4 = targetPosition - bones[num5].transform.position;
				Quaternion val5 = Quaternion.FromToRotation(val3, val4) * bones[num5].transform.rotation;
				if (num6 >= 1f)
				{
					bones[num5].transform.rotation = val5;
				}
				else
				{
					bones[num5].transform.rotation = Quaternion.Lerp(bones[num5].transform.rotation, val5, num6);
				}
			}
			if (useRotationLimits && (Object)(object)bones[num5].rotationLimit != (Object)null)
			{
				bones[num5].rotationLimit.Apply();
			}
		}
	}
}
