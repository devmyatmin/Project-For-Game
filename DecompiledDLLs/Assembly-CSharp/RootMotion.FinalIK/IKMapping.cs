using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKMapping
{
	[Serializable]
	public class BoneMap
	{
		public Transform transform;

		public int chainIndex = -1;

		public int nodeIndex = -1;

		public Vector3 defaultLocalPosition;

		public Quaternion defaultLocalRotation;

		public Vector3 localSwingAxis;

		public Vector3 localTwistAxis;

		public Vector3 planePosition;

		public Vector3 ikPosition;

		public Quaternion defaultLocalTargetRotation;

		private Quaternion maintainRotation;

		public float length;

		public Quaternion animatedRotation;

		private Transform planeBone1;

		private Transform planeBone2;

		private Transform planeBone3;

		private int plane1ChainIndex = -1;

		private int plane1NodeIndex = -1;

		private int plane2ChainIndex = -1;

		private int plane2NodeIndex = -1;

		private int plane3ChainIndex = -1;

		private int plane3NodeIndex = -1;

		public Vector3 swingDirection
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				return transform.rotation * localSwingAxis;
			}
		}

		public bool isNodeBone => nodeIndex != -1;

		private Quaternion lastAnimatedTargetRotation
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0034: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Unknown result type (might be due to invalid IL or missing references)
				//IL_004f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0054: Unknown result type (might be due to invalid IL or missing references)
				//IL_0059: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				if (planeBone1.position == planeBone3.position)
				{
					return Quaternion.identity;
				}
				return Quaternion.LookRotation(planeBone2.position - planeBone1.position, planeBone3.position - planeBone1.position);
			}
		}

		public void Initiate(Transform transform, IKSolverFullBody solver)
		{
			this.transform = transform;
			solver.GetChainAndNodeIndexes(transform, out chainIndex, out nodeIndex);
		}

		public void StoreDefaultLocalState()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			defaultLocalPosition = transform.localPosition;
			defaultLocalRotation = transform.localRotation;
		}

		public void FixTransform(bool position)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (position)
			{
				transform.localPosition = defaultLocalPosition;
			}
			transform.localRotation = defaultLocalRotation;
		}

		public void SetLength(BoneMap nextBone)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			length = Vector3.Distance(transform.position, nextBone.transform.position);
		}

		public void SetLocalSwingAxis(BoneMap swingTarget)
		{
			SetLocalSwingAxis(swingTarget, this);
		}

		public void SetLocalSwingAxis(BoneMap bone1, BoneMap bone2)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			localSwingAxis = Quaternion.Inverse(transform.rotation) * (bone1.transform.position - bone2.transform.position);
		}

		public void SetLocalTwistAxis(Vector3 twistDirection, Vector3 normalDirection)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			Vector3.OrthoNormalize(ref normalDirection, ref twistDirection);
			localTwistAxis = Quaternion.Inverse(transform.rotation) * twistDirection;
		}

		public void SetPlane(IKSolverFullBody solver, Transform planeBone1, Transform planeBone2, Transform planeBone3)
		{
			this.planeBone1 = planeBone1;
			this.planeBone2 = planeBone2;
			this.planeBone3 = planeBone3;
			solver.GetChainAndNodeIndexes(planeBone1, out plane1ChainIndex, out plane1NodeIndex);
			solver.GetChainAndNodeIndexes(planeBone2, out plane2ChainIndex, out plane2NodeIndex);
			solver.GetChainAndNodeIndexes(planeBone3, out plane3ChainIndex, out plane3NodeIndex);
			UpdatePlane(rotation: true, position: true);
		}

		public void UpdatePlane(bool rotation, bool position)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			Quaternion val = lastAnimatedTargetRotation;
			if (rotation)
			{
				defaultLocalTargetRotation = QuaTools.RotationToLocalSpace(transform.rotation, val);
			}
			if (position)
			{
				planePosition = Quaternion.Inverse(val) * (transform.position - planeBone1.position);
			}
		}

		public void SetIKPosition()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			ikPosition = transform.position;
		}

		public void MaintainRotation()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			maintainRotation = transform.rotation;
		}

		public void SetToIKPosition()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			transform.position = ikPosition;
		}

		public void FixToNode(IKSolverFullBody solver, float weight, IKSolver.Node fixNode = null)
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			if (fixNode == null)
			{
				fixNode = solver.GetNode(chainIndex, nodeIndex);
			}
			if (weight >= 1f)
			{
				transform.position = fixNode.solverPosition;
			}
			else
			{
				transform.position = Vector3.Lerp(transform.position, fixNode.solverPosition, weight);
			}
		}

		public Vector3 GetPlanePosition(IKSolverFullBody solver)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			return solver.GetNode(plane1ChainIndex, plane1NodeIndex).solverPosition + GetTargetRotation(solver) * planePosition;
		}

		public void PositionToPlane(IKSolverFullBody solver)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			transform.position = GetPlanePosition(solver);
		}

		public void RotateToPlane(IKSolverFullBody solver, float weight)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			Quaternion val = GetTargetRotation(solver) * defaultLocalTargetRotation;
			if (weight >= 1f)
			{
				transform.rotation = val;
			}
			else
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, val, weight);
			}
		}

		public void Swing(Vector3 swingTarget, float weight)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			Swing(swingTarget, transform.position, weight);
		}

		public void Swing(Vector3 pos1, Vector3 pos2, float weight)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			Quaternion val = Quaternion.FromToRotation(transform.rotation * localSwingAxis, pos1 - pos2) * transform.rotation;
			if (weight >= 1f)
			{
				transform.rotation = val;
			}
			else
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, val, weight);
			}
		}

		public void Twist(Vector3 twistDirection, Vector3 normalDirection, float weight)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			Vector3.OrthoNormalize(ref normalDirection, ref twistDirection);
			Quaternion val = Quaternion.FromToRotation(transform.rotation * localTwistAxis, twistDirection) * transform.rotation;
			if (weight >= 1f)
			{
				transform.rotation = val;
			}
			else
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, val, weight);
			}
		}

		public void RotateToMaintain(float weight)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			if (!(weight <= 0f))
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, maintainRotation, weight);
			}
		}

		public void RotateToEffector(IKSolverFullBody solver, float weight)
		{
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			if (!isNodeBone)
			{
				return;
			}
			float num = weight * solver.GetNode(chainIndex, nodeIndex).effectorRotationWeight;
			if (!(num <= 0f))
			{
				if (num >= 1f)
				{
					transform.rotation = solver.GetNode(chainIndex, nodeIndex).solverRotation;
				}
				else
				{
					transform.rotation = Quaternion.Lerp(transform.rotation, solver.GetNode(chainIndex, nodeIndex).solverRotation, num);
				}
			}
		}

		private Quaternion GetTargetRotation(IKSolverFullBody solver)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			Vector3 solverPosition = solver.GetNode(plane1ChainIndex, plane1NodeIndex).solverPosition;
			Vector3 solverPosition2 = solver.GetNode(plane2ChainIndex, plane2NodeIndex).solverPosition;
			Vector3 solverPosition3 = solver.GetNode(plane3ChainIndex, plane3NodeIndex).solverPosition;
			if (solverPosition == solverPosition3)
			{
				return Quaternion.identity;
			}
			return Quaternion.LookRotation(solverPosition2 - solverPosition, solverPosition3 - solverPosition);
		}
	}

	public virtual bool IsValid(IKSolver solver, ref string message)
	{
		return true;
	}

	public virtual void Initiate(IKSolverFullBody solver)
	{
	}

	protected bool BoneIsValid(Transform bone, IKSolver solver, ref string message, Warning.Logger logger = null)
	{
		if ((Object)(object)bone == (Object)null)
		{
			message = "IKMappingLimb contains a null reference.";
			logger?.Invoke(message);
			return false;
		}
		if (solver.GetPoint(bone) == null)
		{
			message = "IKMappingLimb is referencing to a bone '" + ((Object)bone).name + "' that does not excist in the Node Chain.";
			logger?.Invoke(message);
			return false;
		}
		return true;
	}

	protected Vector3 SolveFABRIKJoint(Vector3 pos1, Vector3 pos2, float length)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = pos1 - pos2;
		return pos2 + val.normalized * length;
	}
}
