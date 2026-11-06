using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverHeuristic : IKSolver
{
	public Transform target;

	public float tolerance;

	public int maxIterations = 4;

	public bool useRotationLimits = true;

	public bool XY;

	public Bone[] bones = new Bone[0];

	protected Vector3 lastLocalDirection;

	protected float chainLength;

	protected virtual int minBones => 2;

	protected virtual bool boneLengthCanBeZero => true;

	protected virtual bool allowCommonParent => false;

	protected virtual Vector3 localDirection
	{
		get
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			return bones[0].transform.InverseTransformDirection(bones[bones.Length - 1].transform.position - bones[0].transform.position);
		}
	}

	protected float positionOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return Vector3.SqrMagnitude(localDirection - lastLocalDirection);
		}
	}

	public bool SetChain(Transform[] hierarchy, Transform root)
	{
		if (bones == null || bones.Length != hierarchy.Length)
		{
			bones = new Bone[hierarchy.Length];
		}
		for (int i = 0; i < hierarchy.Length; i++)
		{
			if (bones[i] == null)
			{
				bones[i] = new Bone();
			}
			bones[i].transform = hierarchy[i];
		}
		Initiate(root);
		return initiated;
	}

	public void AddBone(Transform bone)
	{
		Transform[] array = new Transform[bones.Length + 1];
		for (int i = 0; i < bones.Length; i++)
		{
			array[i] = bones[i].transform;
		}
		array[^1] = bone;
		SetChain(array, root);
	}

	public override void StoreDefaultLocalState()
	{
		for (int i = 0; i < bones.Length; i++)
		{
			bones[i].StoreDefaultLocalState();
		}
	}

	public override void FixTransforms()
	{
		if (initiated && !(IKPositionWeight <= 0f))
		{
			for (int i = 0; i < bones.Length; i++)
			{
				bones[i].FixTransform();
			}
		}
	}

	public override bool IsValid(ref string message)
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		if (bones.Length == 0)
		{
			message = "IK chain has no Bones.";
			return false;
		}
		if (bones.Length < minBones)
		{
			message = "IK chain has less than " + minBones + " Bones.";
			return false;
		}
		Bone[] array = bones;
		for (int i = 0; i < array.Length; i++)
		{
			if ((Object)(object)array[i].transform == (Object)null)
			{
				message = "One of the Bones is null.";
				return false;
			}
		}
		Transform val = IKSolver.ContainsDuplicateBone(bones);
		if ((Object)(object)val != (Object)null)
		{
			message = ((Object)val).name + " is represented multiple times in the Bones.";
			return false;
		}
		if (!allowCommonParent && !IKSolver.HierarchyIsValid(bones))
		{
			message = "Invalid bone hierarchy detected. IK requires for its bones to be parented to each other in descending order.";
			return false;
		}
		if (!boneLengthCanBeZero)
		{
			for (int j = 0; j < bones.Length - 1; j++)
			{
				Vector3 val2 = bones[j].transform.position - bones[j + 1].transform.position;
				if (val2.magnitude == 0f)
				{
					message = "Bone " + j + " length is zero.";
					return false;
				}
			}
		}
		return true;
	}

	public override Point[] GetPoints()
	{
		return bones;
	}

	public override Point GetPoint(Transform transform)
	{
		for (int i = 0; i < bones.Length; i++)
		{
			if ((Object)(object)bones[i].transform == (Object)(object)transform)
			{
				return bones[i];
			}
		}
		return null;
	}

	protected override void OnInitiate()
	{
	}

	protected override void OnUpdate()
	{
	}

	protected void InitiateBones()
	{
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		chainLength = 0f;
		for (int i = 0; i < bones.Length; i++)
		{
			if (i < bones.Length - 1)
			{
				Bone obj = bones[i];
				Vector3 val = bones[i].transform.position - bones[i + 1].transform.position;
				obj.length = val.magnitude;
				chainLength += bones[i].length;
				Vector3 position = bones[i + 1].transform.position;
				bones[i].axis = Quaternion.Inverse(bones[i].transform.rotation) * (position - bones[i].transform.position);
				if ((Object)(object)bones[i].rotationLimit != (Object)null)
				{
					if (XY && !(bones[i].rotationLimit is RotationLimitHinge))
					{
						Warning.Log("Only Hinge Rotation Limits should be used on 2D IK solvers.", bones[i].transform);
					}
					bones[i].rotationLimit.Disable();
				}
			}
			else
			{
				bones[i].axis = Quaternion.Inverse(bones[i].transform.rotation) * (bones[bones.Length - 1].transform.position - bones[0].transform.position);
			}
		}
	}

	protected Vector3 GetSingularityOffset()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if (!SingularityDetected())
		{
			return Vector3.zero;
		}
		Vector3 val = IKPosition - bones[0].transform.position;
		Vector3 normalized = val.normalized;
		Vector3 val2 = new Vector3(normalized.y, normalized.z, normalized.x);
		if (useRotationLimits && (Object)(object)bones[bones.Length - 2].rotationLimit != (Object)null && bones[bones.Length - 2].rotationLimit is RotationLimitHinge)
		{
			val2 = bones[bones.Length - 2].transform.rotation * bones[bones.Length - 2].rotationLimit.axis;
		}
		return Vector3.Cross(normalized, val2) * bones[bones.Length - 2].length * 0.5f;
	}

	private bool SingularityDetected()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (!initiated)
		{
			return false;
		}
		Vector3 val = bones[bones.Length - 1].transform.position - bones[0].transform.position;
		Vector3 val2 = IKPosition - bones[0].transform.position;
		float magnitude = val.magnitude;
		float magnitude2 = val2.magnitude;
		if (magnitude < magnitude2)
		{
			return false;
		}
		if (magnitude < chainLength - bones[bones.Length - 2].length * 0.1f)
		{
			return false;
		}
		if (magnitude == 0f)
		{
			return false;
		}
		if (magnitude2 == 0f)
		{
			return false;
		}
		if (magnitude2 > magnitude)
		{
			return false;
		}
		if (Vector3.Dot(val / magnitude, val2 / magnitude2) < 0.999f)
		{
			return false;
		}
		return true;
	}
}
