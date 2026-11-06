using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverTrigonometric : IKSolver
{
	[Serializable]
	public class TrigonometricBone : Bone
	{
		private Quaternion targetToLocalSpace;

		private Vector3 defaultLocalBendNormal;

		public void Initiate(Vector3 childPosition, Vector3 bendNormal)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			Quaternion rotation = Quaternion.LookRotation(childPosition - transform.position, bendNormal);
			targetToLocalSpace = QuaTools.RotationToLocalSpace(transform.rotation, rotation);
			defaultLocalBendNormal = Quaternion.Inverse(transform.rotation) * bendNormal;
		}

		public Quaternion GetRotation(Vector3 direction, Vector3 bendNormal)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			return Quaternion.LookRotation(direction, bendNormal) * targetToLocalSpace;
		}

		public Vector3 GetBendNormalFromCurrentRotation()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			return transform.rotation * defaultLocalBendNormal;
		}
	}

	public Transform target;

	[Range(0f, 1f)]
	public float IKRotationWeight = 1f;

	public Quaternion IKRotation = Quaternion.identity;

	public Vector3 bendNormal = Vector3.right;

	public TrigonometricBone bone1 = new TrigonometricBone();

	public TrigonometricBone bone2 = new TrigonometricBone();

	public TrigonometricBone bone3 = new TrigonometricBone();

	protected Vector3 weightIKPosition;

	protected bool directHierarchy = true;

	public void SetBendGoalPosition(Vector3 goalPosition, float weight)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (!initiated || weight <= 0f)
		{
			return;
		}
		Vector3 val = Vector3.Cross(goalPosition - bone1.transform.position, IKPosition - bone1.transform.position);
		if (val != Vector3.zero)
		{
			if (weight >= 1f)
			{
				bendNormal = val;
			}
			else
			{
				bendNormal = Vector3.Lerp(bendNormal, val, weight);
			}
		}
	}

	public void SetBendPlaneToCurrent()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (initiated)
		{
			Vector3 val = Vector3.Cross(bone2.transform.position - bone1.transform.position, bone3.transform.position - bone2.transform.position);
			if (val != Vector3.zero)
			{
				bendNormal = val;
			}
		}
	}

	public void SetIKRotation(Quaternion rotation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		IKRotation = rotation;
	}

	public void SetIKRotationWeight(float weight)
	{
		IKRotationWeight = Mathf.Clamp(weight, 0f, 1f);
	}

	public Quaternion GetIKRotation()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return IKRotation;
	}

	public float GetIKRotationWeight()
	{
		return IKRotationWeight;
	}

	public override Point[] GetPoints()
	{
		return new Point[3] { bone1, bone2, bone3 };
	}

	public override Point GetPoint(Transform transform)
	{
		if ((Object)(object)bone1.transform == (Object)(object)transform)
		{
			return bone1;
		}
		if ((Object)(object)bone2.transform == (Object)(object)transform)
		{
			return bone2;
		}
		if ((Object)(object)bone3.transform == (Object)(object)transform)
		{
			return bone3;
		}
		return null;
	}

	public override void StoreDefaultLocalState()
	{
		bone1.StoreDefaultLocalState();
		bone2.StoreDefaultLocalState();
		bone3.StoreDefaultLocalState();
	}

	public override void FixTransforms()
	{
		if (initiated)
		{
			bone1.FixTransform();
			bone2.FixTransform();
			bone3.FixTransform();
		}
	}

	public override bool IsValid(ref string message)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected Obj, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)bone1.transform == (Object)null || (Object)(object)bone2.transform == (Object)null || (Object)(object)bone3.transform == (Object)null)
		{
			message = "Please assign all Bones to the IK solver.";
			return false;
		}
		Object[] objects = (Object[])(object)new Transform[3] { bone1.transform, bone2.transform, bone3.transform };
		Transform val = (Transform)Hierarchy.ContainsDuplicate(objects);
		if ((Object)(object)val != (Object)null)
		{
			message = ((Object)val).name + " is represented multiple times in the Bones.";
			return false;
		}
		if (bone1.transform.position == bone2.transform.position)
		{
			message = "first bone position is the same as second bone position.";
			return false;
		}
		if (bone2.transform.position == bone3.transform.position)
		{
			message = "second bone position is the same as third bone position.";
			return false;
		}
		return true;
	}

	public bool SetChain(Transform bone1, Transform bone2, Transform bone3, Transform root)
	{
		this.bone1.transform = bone1;
		this.bone2.transform = bone2;
		this.bone3.transform = bone3;
		Initiate(root);
		return initiated;
	}

	public static void Solve(Transform bone1, Transform bone2, Transform bone3, Vector3 targetPosition, Vector3 bendNormal, float weight)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		if (weight <= 0f)
		{
			return;
		}
		targetPosition = Vector3.Lerp(bone3.position, targetPosition, weight);
		Vector3 val = targetPosition - bone1.position;
		float magnitude = val.magnitude;
		if (magnitude != 0f)
		{
			Vector3 val2 = bone2.position - bone1.position;
			float sqrMagnitude = val2.sqrMagnitude;
			val2 = bone3.position - bone2.position;
			float sqrMagnitude2 = val2.sqrMagnitude;
			Vector3 bendDirection = Vector3.Cross(val, bendNormal);
			Vector3 directionToBendPoint = GetDirectionToBendPoint(val, magnitude, bendDirection, sqrMagnitude, sqrMagnitude2);
			Quaternion val3 = Quaternion.FromToRotation(bone2.position - bone1.position, directionToBendPoint);
			if (weight < 1f)
			{
				val3 = Quaternion.Lerp(Quaternion.identity, val3, weight);
			}
			bone1.rotation = val3 * bone1.rotation;
			Quaternion val4 = Quaternion.FromToRotation(bone3.position - bone2.position, targetPosition - bone2.position);
			if (weight < 1f)
			{
				val4 = Quaternion.Lerp(Quaternion.identity, val4, weight);
			}
			bone2.rotation = val4 * bone2.rotation;
		}
	}

	private static Vector3 GetDirectionToBendPoint(Vector3 direction, float directionMag, Vector3 bendDirection, float sqrMag1, float sqrMag2)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		float num = (directionMag * directionMag + (sqrMag1 - sqrMag2)) / 2f / directionMag;
		float num2 = (float)Math.Sqrt(Mathf.Clamp(sqrMag1 - num * num, 0f, float.PositiveInfinity));
		if (direction == Vector3.zero)
		{
			return Vector3.zero;
		}
		return Quaternion.LookRotation(direction, bendDirection) * new Vector3(0f, num2, num);
	}

	protected override void OnInitiate()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (bendNormal == Vector3.zero)
		{
			bendNormal = Vector3.right;
		}
		OnInitiateVirtual();
		IKPosition = bone3.transform.position;
		IKRotation = bone3.transform.rotation;
		InitiateBones();
		directHierarchy = IsDirectHierarchy();
	}

	private bool IsDirectHierarchy()
	{
		if ((Object)(object)bone3.transform.parent != (Object)(object)bone2.transform)
		{
			return false;
		}
		if ((Object)(object)bone2.transform.parent != (Object)(object)bone1.transform)
		{
			return false;
		}
		return true;
	}

	public void InitiateBones()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		bone1.Initiate(bone2.transform.position, bendNormal);
		bone2.Initiate(bone3.transform.position, bendNormal);
		SetBendPlaneToCurrent();
	}

	protected override void OnUpdate()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
		IKRotationWeight = Mathf.Clamp(IKRotationWeight, 0f, 1f);
		if ((Object)(object)target != (Object)null)
		{
			IKPosition = target.position;
			IKRotation = target.rotation;
		}
		OnUpdateVirtual();
		if (IKPositionWeight > 0f)
		{
			if (!directHierarchy)
			{
				bone1.Initiate(bone2.transform.position, bendNormal);
				bone2.Initiate(bone3.transform.position, bendNormal);
			}
			TrigonometricBone trigonometricBone = bone1;
			Vector3 val = bone2.transform.position - bone1.transform.position;
			trigonometricBone.sqrMag = val.sqrMagnitude;
			TrigonometricBone trigonometricBone2 = bone2;
			val = bone3.transform.position - bone2.transform.position;
			trigonometricBone2.sqrMag = val.sqrMagnitude;
			if (bendNormal == Vector3.zero && !Warning.logged)
			{
				LogWarning("IKSolverTrigonometric Bend Normal is Vector3.zero.");
			}
			weightIKPosition = Vector3.Lerp(bone3.transform.position, IKPosition, IKPositionWeight);
			Vector3 val2 = Vector3.Lerp(bone1.GetBendNormalFromCurrentRotation(), bendNormal, IKPositionWeight);
			Vector3 val3 = Vector3.Lerp(bone2.transform.position - bone1.transform.position, GetBendDirection(weightIKPosition, val2), IKPositionWeight);
			if (val3 == Vector3.zero)
			{
				val3 = bone2.transform.position - bone1.transform.position;
			}
			bone1.transform.rotation = bone1.GetRotation(val3, val2);
			bone2.transform.rotation = bone2.GetRotation(weightIKPosition - bone2.transform.position, bone2.GetBendNormalFromCurrentRotation());
		}
		if (IKRotationWeight > 0f)
		{
			bone3.transform.rotation = Quaternion.Slerp(bone3.transform.rotation, IKRotation, IKRotationWeight);
		}
		OnPostSolveVirtual();
	}

	protected virtual void OnInitiateVirtual()
	{
	}

	protected virtual void OnUpdateVirtual()
	{
	}

	protected virtual void OnPostSolveVirtual()
	{
	}

	protected Vector3 GetBendDirection(Vector3 IKPosition, Vector3 bendNormal)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = IKPosition - bone1.transform.position;
		if (val == Vector3.zero)
		{
			return Vector3.zero;
		}
		float sqrMagnitude = val.sqrMagnitude;
		float num = (float)Math.Sqrt(sqrMagnitude);
		float num2 = (sqrMagnitude + bone1.sqrMag - bone2.sqrMag) / 2f / num;
		float num3 = (float)Math.Sqrt(Mathf.Clamp(bone1.sqrMag - num2 * num2, 0f, float.PositiveInfinity));
		Vector3 val2 = Vector3.Cross(val / num, bendNormal);
		return Quaternion.LookRotation(val, val2) * new Vector3(0f, num3, num2);
	}

	public IKSolverTrigonometric()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
	}
}
