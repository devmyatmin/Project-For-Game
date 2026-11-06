using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class Finger
{
	[Serializable]
	public enum DOF
	{
		One,
		Three
	}

	[Tooltip("Master Weight for the finger.")]
	[Range(0f, 1f)]
	public float weight = 1f;

	[Tooltip("The weight of rotating the finger tip and bending the finger to the target.")]
	[Range(0f, 1f)]
	public float rotationWeight = 1f;

	[Tooltip("Rotational degrees of freedom. When set to 'One' the fingers will be able to be rotated only around a single axis. When 3, all 3 axes are free to rotate around.")]
	public DOF rotationDOF;

	[Tooltip("If enabled, keeps bone1 twist angle fixed relative to bone2.")]
	public bool fixBone1Twist;

	[Tooltip("The first bone of the finger.")]
	public Transform bone1;

	[Tooltip("The second bone of the finger.")]
	public Transform bone2;

	[Tooltip("The (optional) third bone of the finger. This can be ignored for thumbs.")]
	public Transform bone3;

	[Tooltip("The fingertip object. If your character doesn't have tip bones, you can create an empty GameObject and parent it to the last bone in the finger. Place it to the tip of the finger.")]
	public Transform tip;

	[Tooltip("The IK target (optional, can use IKPosition and IKRotation directly).")]
	public Transform target;

	private IKSolverLimb solver;

	private Quaternion bone3RelativeToTarget;

	private Vector3 bone3DefaultLocalPosition;

	private Quaternion bone3DefaultLocalRotation;

	private Vector3 bone1Axis;

	private Vector3 tipAxis;

	private Vector3 bone1TwistAxis;

	private Vector3 defaultBendNormal;

	public bool initiated { get; private set; }

	public Vector3 IKPosition
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return solver.IKPosition;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			solver.IKPosition = value;
		}
	}

	public Quaternion IKRotation
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return solver.IKRotation;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			solver.IKRotation = value;
		}
	}

	public bool IsValid(ref string errorMessage)
	{
		if ((Object)(object)bone1 == (Object)null || (Object)(object)bone2 == (Object)null || (Object)(object)tip == (Object)null)
		{
			errorMessage = "One of the bones in the Finger Rig is null, can not initiate solvers.";
			return false;
		}
		return true;
	}

	public void Initiate(Transform hand, int index)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		initiated = false;
		string errorMessage = string.Empty;
		if (!IsValid(ref errorMessage))
		{
			Warning.Log(errorMessage, hand);
			return;
		}
		solver = new IKSolverLimb();
		solver.IKPositionWeight = weight;
		solver.bendModifier = IKSolverLimb.BendModifier.Target;
		solver.bendModifierWeight = 1f;
		Vector3 val = Vector3.Cross(tip.position - bone1.position, bone2.position - bone1.position);
		defaultBendNormal = -val.normalized;
		solver.bendNormal = defaultBendNormal;
		Vector3 val2 = Vector3.Cross(bone2.position - bone1.position, tip.position - bone1.position);
		bone1Axis = Quaternion.Inverse(bone1.rotation) * val2;
		tipAxis = Quaternion.Inverse(tip.rotation) * val2;
		Vector3 val3 = bone2.position - bone1.position;
		Vector3 val4 = -Vector3.Cross(tip.position - bone1.position, bone2.position - bone1.position);
		Vector3.OrthoNormalize(ref val3, ref val4);
		bone1TwistAxis = Quaternion.Inverse(bone1.rotation) * val4;
		IKPosition = tip.position;
		IKRotation = tip.rotation;
		if ((Object)(object)bone3 != (Object)null)
		{
			bone3RelativeToTarget = Quaternion.Inverse(IKRotation) * bone3.rotation;
			bone3DefaultLocalPosition = bone3.localPosition;
			bone3DefaultLocalRotation = bone3.localRotation;
		}
		solver.SetChain(bone1, bone2, tip, hand);
		solver.Initiate(hand);
		initiated = true;
	}

	public void FixTransforms()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (initiated && !(weight <= 0f))
		{
			solver.FixTransforms();
			if ((Object)(object)bone3 != (Object)null)
			{
				bone3.localPosition = bone3DefaultLocalPosition;
				bone3.localRotation = bone3DefaultLocalRotation;
			}
		}
	}

	public void StoreDefaultLocalState()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (initiated)
		{
			solver.StoreDefaultLocalState();
			if ((Object)(object)bone3 != (Object)null)
			{
				bone3DefaultLocalPosition = bone3.localPosition;
				bone3DefaultLocalRotation = bone3.localRotation;
			}
		}
	}

	public void Update(float masterWeight)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		if (!initiated)
		{
			return;
		}
		float num = weight * masterWeight;
		if (num <= 0f)
		{
			return;
		}
		solver.target = target;
		if ((Object)(object)target != (Object)null)
		{
			IKPosition = target.position;
			IKRotation = target.rotation;
		}
		if (rotationDOF == DOF.One)
		{
			Quaternion val = Quaternion.FromToRotation(IKRotation * tipAxis, bone1.rotation * bone1Axis);
			IKRotation = val * IKRotation;
		}
		if ((Object)(object)bone3 != (Object)null)
		{
			if (num * rotationWeight >= 1f)
			{
				bone3.rotation = IKRotation * bone3RelativeToTarget;
			}
			else
			{
				bone3.rotation = Quaternion.Lerp(bone3.rotation, IKRotation * bone3RelativeToTarget, num * rotationWeight);
			}
		}
		solver.IKPositionWeight = num;
		solver.IKRotationWeight = rotationWeight;
		solver.Update();
		if (fixBone1Twist)
		{
			Quaternion rotation = bone2.rotation;
			Vector3 val2 = Quaternion.Inverse(Quaternion.LookRotation(bone1.rotation * bone1TwistAxis, bone2.position - bone1.position)) * solver.bendNormal;
			float num2 = Mathf.Atan2(val2.x, val2.z) * 57.29578f;
			bone1.rotation = Quaternion.AngleAxis(num2, bone2.position - bone1.position) * bone1.rotation;
			bone2.rotation = rotation;
		}
	}
}
