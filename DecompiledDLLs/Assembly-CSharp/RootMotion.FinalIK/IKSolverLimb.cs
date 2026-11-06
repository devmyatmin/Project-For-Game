using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverLimb : IKSolverTrigonometric
{
	[Serializable]
	public enum BendModifier
	{
		Animation,
		Target,
		Parent,
		Arm,
		Goal
	}

	[Serializable]
	public struct AxisDirection(Vector3 direction, Vector3 axis)
	{
		public Vector3 direction = direction.normalized;

		public Vector3 axis = axis.normalized;

		public float dot = 0f;
	}

	public AvatarIKGoal goal;

	public BendModifier bendModifier;

	[Range(0f, 1f)]
	public float maintainRotationWeight;

	[Range(0f, 1f)]
	public float bendModifierWeight = 1f;

	public Transform bendGoal;

	private bool maintainBendFor1Frame;

	private bool maintainRotationFor1Frame;

	private Quaternion defaultRootRotation;

	private Quaternion parentDefaultRotation;

	private Quaternion bone3RotationBeforeSolve;

	private Quaternion maintainRotation;

	private Quaternion bone3DefaultRotation;

	private Vector3 _bendNormal;

	private Vector3 animationNormal;

	private AxisDirection[] axisDirectionsLeft = new AxisDirection[4];

	private AxisDirection[] axisDirectionsRight = new AxisDirection[4];

	private AxisDirection[] axisDirections
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			if ((int)goal == 2)
			{
				return axisDirectionsLeft;
			}
			return axisDirectionsRight;
		}
	}

	public void MaintainRotation()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (initiated)
		{
			maintainRotation = bone3.transform.rotation;
			maintainRotationFor1Frame = true;
		}
	}

	public void MaintainBend()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (initiated)
		{
			animationNormal = bone1.GetBendNormalFromCurrentRotation();
			maintainBendFor1Frame = true;
		}
	}

	protected override void OnInitiateVirtual()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		defaultRootRotation = root.rotation;
		if ((Object)(object)bone1.transform.parent != (Object)null)
		{
			parentDefaultRotation = Quaternion.Inverse(defaultRootRotation) * bone1.transform.parent.rotation;
		}
		if ((Object)(object)bone3.rotationLimit != (Object)null)
		{
			bone3.rotationLimit.Disable();
		}
		bone3DefaultRotation = bone3.transform.rotation;
		Vector3 val = Vector3.Cross(bone2.transform.position - bone1.transform.position, bone3.transform.position - bone2.transform.position);
		if (val != Vector3.zero)
		{
			bendNormal = val;
		}
		animationNormal = bendNormal;
		StoreAxisDirections(ref axisDirectionsLeft);
		StoreAxisDirections(ref axisDirectionsRight);
	}

	protected override void OnUpdateVirtual()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (IKPositionWeight > 0f)
		{
			bendModifierWeight = Mathf.Clamp(bendModifierWeight, 0f, 1f);
			maintainRotationWeight = Mathf.Clamp(maintainRotationWeight, 0f, 1f);
			_bendNormal = bendNormal;
			bendNormal = GetModifiedBendNormal();
		}
		if (maintainRotationWeight * IKPositionWeight > 0f)
		{
			bone3RotationBeforeSolve = (maintainRotationFor1Frame ? maintainRotation : bone3.transform.rotation);
			maintainRotationFor1Frame = false;
		}
	}

	protected override void OnPostSolveVirtual()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (IKPositionWeight > 0f)
		{
			bendNormal = _bendNormal;
		}
		if (maintainRotationWeight * IKPositionWeight > 0f)
		{
			bone3.transform.rotation = Quaternion.Slerp(bone3.transform.rotation, bone3RotationBeforeSolve, maintainRotationWeight * IKPositionWeight);
		}
	}

	public IKSolverLimb()
	{
	}

	public IKSolverLimb(AvatarIKGoal goal)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		this.goal = goal;
	}

	private void StoreAxisDirections(ref AxisDirection[] axisDirections)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		axisDirections[0] = new AxisDirection(Vector3.zero, new Vector3(-1f, 0f, 0f));
		axisDirections[1] = new AxisDirection(new Vector3(0.5f, 0f, -0.2f), new Vector3(-0.5f, -1f, 1f));
		axisDirections[2] = new AxisDirection(new Vector3(-0.5f, -1f, -0.2f), new Vector3(0f, 0.5f, -1f));
		axisDirections[3] = new AxisDirection(new Vector3(-0.5f, -0.5f, 1f), new Vector3(-1f, -1f, -1f));
	}

	private Vector3 GetModifiedBendNormal()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Invalid comparison between Unknown and I4
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Invalid comparison between Unknown and I4
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Invalid comparison between Unknown and I4
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		float num = bendModifierWeight;
		if (num <= 0f)
		{
			return bendNormal;
		}
		switch (bendModifier)
		{
		case BendModifier.Animation:
			if (!maintainBendFor1Frame)
			{
				MaintainBend();
			}
			maintainBendFor1Frame = false;
			return Vector3.Lerp(bendNormal, animationNormal, num);
		case BendModifier.Parent:
		{
			if ((Object)(object)bone1.transform.parent == (Object)null)
			{
				return bendNormal;
			}
			Quaternion val6 = bone1.transform.parent.rotation * Quaternion.Inverse(parentDefaultRotation);
			return Quaternion.Slerp(Quaternion.identity, val6 * Quaternion.Inverse(defaultRootRotation), num) * bendNormal;
		}
		case BendModifier.Target:
		{
			Quaternion val5 = IKRotation * Quaternion.Inverse(bone3DefaultRotation);
			return Quaternion.Slerp(Quaternion.identity, val5, num) * bendNormal;
		}
		case BendModifier.Arm:
		{
			if ((Object)(object)bone1.transform.parent == (Object)null)
			{
				return bendNormal;
			}
			if ((int)goal == 0 || (int)goal == 1)
			{
				if (!Warning.logged)
				{
					LogWarning("Trying to use the 'Arm' bend modifier on a leg.");
				}
				return bendNormal;
			}
			Vector3 val2 = IKPosition - bone1.transform.position;
			Vector3 normalized = val2.normalized;
			normalized = Quaternion.Inverse(bone1.transform.parent.rotation * Quaternion.Inverse(parentDefaultRotation)) * normalized;
			if ((int)goal == 2)
			{
				normalized.x = 0f - normalized.x;
			}
			for (int i = 1; i < axisDirections.Length; i++)
			{
				axisDirections[i].dot = Mathf.Clamp(Vector3.Dot(axisDirections[i].direction, normalized), 0f, 1f);
				axisDirections[i].dot = Interp.Float(axisDirections[i].dot, InterpolationMode.InOutQuintic);
			}
			Vector3 val3 = axisDirections[0].axis;
			for (int j = 1; j < axisDirections.Length; j++)
			{
				val3 = Vector3.Slerp(val3, axisDirections[j].axis, axisDirections[j].dot);
			}
			if ((int)goal == 2)
			{
				val3.x = 0f - val3.x;
				val3 = -val3;
			}
			Vector3 val4 = bone1.transform.parent.rotation * Quaternion.Inverse(parentDefaultRotation) * val3;
			if (num >= 1f)
			{
				return val4;
			}
			return Vector3.Lerp(bendNormal, val4, num);
		}
		case BendModifier.Goal:
		{
			if ((Object)(object)bendGoal == (Object)null)
			{
				if (!Warning.logged)
				{
					LogWarning("Trying to use the 'Goal' Bend Modifier, but the Bend Goal is unassigned.");
				}
				return bendNormal;
			}
			Vector3 val = Vector3.Cross(bendGoal.position - bone1.transform.position, IKPosition - bone1.transform.position);
			if (val == Vector3.zero)
			{
				return bendNormal;
			}
			if (num >= 1f)
			{
				return val;
			}
			return Vector3.Lerp(bendNormal, val, num);
		}
		default:
			return bendNormal;
		}
	}
}
