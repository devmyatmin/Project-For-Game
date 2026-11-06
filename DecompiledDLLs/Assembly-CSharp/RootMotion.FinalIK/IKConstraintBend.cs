using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKConstraintBend
{
	public Transform bone1;

	public Transform bone2;

	public Transform bone3;

	public Transform bendGoal;

	public Vector3 direction = Vector3.right;

	public Quaternion rotationOffset;

	[Range(0f, 1f)]
	public float weight;

	public Vector3 defaultLocalDirection;

	public Vector3 defaultChildDirection;

	[NonSerialized]
	public float clampF = 0.505f;

	private int chainIndex1;

	private int nodeIndex1;

	private int chainIndex2;

	private int nodeIndex2;

	private int chainIndex3;

	private int nodeIndex3;

	private bool limbOrientationsSet;

	public bool initiated { get; private set; }

	public bool IsValid(IKSolverFullBody solver, Warning.Logger logger)
	{
		if ((Object)(object)bone1 == (Object)null || (Object)(object)bone2 == (Object)null || (Object)(object)bone3 == (Object)null)
		{
			logger?.Invoke("Bend Constraint contains a null reference.");
			return false;
		}
		if (solver.GetPoint(bone1) == null)
		{
			logger?.Invoke("Bend Constraint is referencing to a bone '" + ((Object)bone1).name + "' that does not excist in the Node Chain.");
			return false;
		}
		if (solver.GetPoint(bone2) == null)
		{
			logger?.Invoke("Bend Constraint is referencing to a bone '" + ((Object)bone2).name + "' that does not excist in the Node Chain.");
			return false;
		}
		if (solver.GetPoint(bone3) == null)
		{
			logger?.Invoke("Bend Constraint is referencing to a bone '" + ((Object)bone3).name + "' that does not excist in the Node Chain.");
			return false;
		}
		return true;
	}

	public IKConstraintBend()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}

	public IKConstraintBend(Transform bone1, Transform bone2, Transform bone3)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		SetBones(bone1, bone2, bone3);
	}

	public void SetBones(Transform bone1, Transform bone2, Transform bone3)
	{
		this.bone1 = bone1;
		this.bone2 = bone2;
		this.bone3 = bone3;
	}

	public void Initiate(IKSolverFullBody solver)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		solver.GetChainAndNodeIndexes(bone1, out chainIndex1, out nodeIndex1);
		solver.GetChainAndNodeIndexes(bone2, out chainIndex2, out nodeIndex2);
		solver.GetChainAndNodeIndexes(bone3, out chainIndex3, out nodeIndex3);
		direction = OrthoToBone1(solver, OrthoToLimb(solver, bone2.position - bone1.position));
		if (!limbOrientationsSet)
		{
			defaultLocalDirection = Quaternion.Inverse(bone1.rotation) * direction;
			Vector3 val = bone3.position - bone1.position;
			Vector3 val2 = Vector3.Cross(val.normalized, direction);
			defaultChildDirection = Quaternion.Inverse(bone3.rotation) * val2;
		}
		initiated = true;
	}

	public void SetLimbOrientation(Vector3 upper, Vector3 lower, Vector3 last)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (upper == Vector3.zero)
		{
			Debug.LogError((object)"Attempting to set limb orientation to Vector3.zero axis");
		}
		if (lower == Vector3.zero)
		{
			Debug.LogError((object)"Attempting to set limb orientation to Vector3.zero axis");
		}
		if (last == Vector3.zero)
		{
			Debug.LogError((object)"Attempting to set limb orientation to Vector3.zero axis");
		}
		defaultLocalDirection = upper.normalized;
		defaultChildDirection = last.normalized;
		limbOrientationsSet = true;
	}

	public void LimitBend(float solverWeight, float positionWeight)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		if (initiated)
		{
			Vector3 val = bone1.rotation * -defaultLocalDirection;
			Vector3 val2 = bone3.position - bone2.position;
			bool changed = false;
			Vector3 val3 = V3Tools.ClampDirection(val2, val, clampF * solverWeight, 0, out changed);
			Quaternion rotation = bone3.rotation;
			if (changed)
			{
				Quaternion val4 = Quaternion.FromToRotation(val2, val3);
				bone2.rotation = val4 * bone2.rotation;
			}
			if (positionWeight > 0f)
			{
				Vector3 val5 = bone2.position - bone1.position;
				Vector3 val6 = bone3.position - bone2.position;
				Vector3.OrthoNormalize(ref val5, ref val6);
				Quaternion val7 = Quaternion.FromToRotation(val6, val);
				bone2.rotation = Quaternion.Lerp(bone2.rotation, val7 * bone2.rotation, positionWeight * solverWeight);
			}
			if (changed || positionWeight > 0f)
			{
				bone3.rotation = rotation;
			}
		}
	}

	public Vector3 GetDir(IKSolverFullBody solver)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		if (!initiated)
		{
			return Vector3.zero;
		}
		float num = weight * solver.IKPositionWeight;
		if ((Object)(object)bendGoal != (Object)null)
		{
			Vector3 val = bendGoal.position - solver.GetNode(chainIndex1, nodeIndex1).solverPosition;
			if (val != Vector3.zero)
			{
				direction = val;
			}
		}
		if (num >= 1f)
		{
			return direction.normalized;
		}
		Vector3 val2 = solver.GetNode(chainIndex3, nodeIndex3).solverPosition - solver.GetNode(chainIndex1, nodeIndex1).solverPosition;
		Vector3 val3 = Quaternion.FromToRotation(bone3.position - bone1.position, val2) * (bone2.position - bone1.position);
		if (solver.GetNode(chainIndex3, nodeIndex3).effectorRotationWeight > 0f)
		{
			Vector3 val4 = -Vector3.Cross(val2, solver.GetNode(chainIndex3, nodeIndex3).solverRotation * defaultChildDirection);
			val3 = Vector3.Lerp(val3, val4, solver.GetNode(chainIndex3, nodeIndex3).effectorRotationWeight);
		}
		if (rotationOffset != Quaternion.identity)
		{
			val3 = Quaternion.FromToRotation(rotationOffset * val2, val2) * rotationOffset * val3;
		}
		if (num <= 0f)
		{
			return val3;
		}
		return Vector3.Lerp(val3, direction.normalized, num);
	}

	private Vector3 OrthoToLimb(IKSolverFullBody solver, Vector3 tangent)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = solver.GetNode(chainIndex3, nodeIndex3).solverPosition - solver.GetNode(chainIndex1, nodeIndex1).solverPosition;
		Vector3.OrthoNormalize(ref val, ref tangent);
		return tangent;
	}

	private Vector3 OrthoToBone1(IKSolverFullBody solver, Vector3 tangent)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = solver.GetNode(chainIndex2, nodeIndex2).solverPosition - solver.GetNode(chainIndex1, nodeIndex1).solverPosition;
		Vector3.OrthoNormalize(ref val, ref tangent);
		return tangent;
	}
}
