using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class KissingRig : MonoBehaviour
{
	[Serializable]
	public class Partner
	{
		public FullBodyBipedIK ik;

		public Transform mouth;

		public Transform mouthTarget;

		public Transform touchTargetLeftHand;

		public Transform touchTargetRightHand;

		public float bodyWeightHorizontal = 0.4f;

		public float bodyWeightVertical = 1f;

		public float neckRotationWeight = 0.3f;

		public float headTiltAngle = 10f;

		public Vector3 headTiltAxis;

		private Quaternion neckRotation;

		private Transform neck => ik.solver.spineMapping.spineBones[ik.solver.spineMapping.spineBones.Length - 1];

		public void Initiate()
		{
			((Behaviour)ik).enabled = false;
		}

		public void Update(float weight)
		{
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0203: Unknown result type (might be due to invalid IL or missing references)
			//IL_021e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0223: Unknown result type (might be due to invalid IL or missing references)
			//IL_023e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0243: Unknown result type (might be due to invalid IL or missing references)
			//IL_025e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0263: Unknown result type (might be due to invalid IL or missing references)
			//IL_027e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0283: Unknown result type (might be due to invalid IL or missing references)
			//IL_028f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0294: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0305: Unknown result type (might be due to invalid IL or missing references)
			//IL_030a: Unknown result type (might be due to invalid IL or missing references)
			ik.solver.leftShoulderEffector.positionWeight = weight;
			ik.solver.rightShoulderEffector.positionWeight = weight;
			ik.solver.leftHandEffector.positionWeight = weight;
			ik.solver.rightHandEffector.positionWeight = weight;
			ik.solver.leftHandEffector.rotationWeight = weight;
			ik.solver.rightHandEffector.rotationWeight = weight;
			ik.solver.bodyEffector.positionWeight = weight;
			InverseTransformEffector(FullBodyBipedEffector.LeftShoulder, mouth, mouthTarget.position, weight);
			InverseTransformEffector(FullBodyBipedEffector.RightShoulder, mouth, mouthTarget.position, weight);
			InverseTransformEffector(FullBodyBipedEffector.Body, mouth, mouthTarget.position, weight);
			ik.solver.bodyEffector.position = Vector3.Lerp(new Vector3(ik.solver.bodyEffector.position.x, ik.solver.bodyEffector.bone.position.y, ik.solver.bodyEffector.position.z), ik.solver.bodyEffector.position, bodyWeightVertical * weight);
			ik.solver.bodyEffector.position = Vector3.Lerp(new Vector3(ik.solver.bodyEffector.bone.position.x, ik.solver.bodyEffector.position.y, ik.solver.bodyEffector.bone.position.z), ik.solver.bodyEffector.position, bodyWeightHorizontal * weight);
			ik.solver.leftHandEffector.position = touchTargetLeftHand.position;
			ik.solver.rightHandEffector.position = touchTargetRightHand.position;
			ik.solver.leftHandEffector.rotation = touchTargetLeftHand.rotation;
			ik.solver.rightHandEffector.rotation = touchTargetRightHand.rotation;
			neckRotation = neck.rotation;
			ik.solver.Update();
			neck.rotation = Quaternion.Slerp(neck.rotation, neckRotation, neckRotationWeight * weight);
			ik.references.head.localRotation = Quaternion.AngleAxis(headTiltAngle * weight, headTiltAxis) * ik.references.head.localRotation;
		}

		private void InverseTransformEffector(FullBodyBipedEffector effector, Transform target, Vector3 targetPosition, float weight)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = ik.solver.GetEffector(effector).bone.position - target.position;
			ik.solver.GetEffector(effector).position = Vector3.Lerp(ik.solver.GetEffector(effector).bone.position, targetPosition + val, weight);
		}
	}

	public Partner partner1;

	public Partner partner2;

	public float weight;

	public int iterations = 3;

	private void Start()
	{
		partner1.Initiate();
		partner2.Initiate();
	}

	private void LateUpdate()
	{
		for (int i = 0; i < iterations; i++)
		{
			partner1.Update(weight);
			partner2.Update(weight);
		}
	}
}
