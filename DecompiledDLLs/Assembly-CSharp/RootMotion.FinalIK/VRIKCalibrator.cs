using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public static class VRIKCalibrator
{
	[Serializable]
	public class Settings
	{
		[Tooltip("Multiplies character scale")]
		public float scaleMlp = 1f;

		[Tooltip("Local axis of the HMD facing forward.")]
		public Vector3 headTrackerForward = Vector3.forward;

		[Tooltip("Local axis of the HMD facing up.")]
		public Vector3 headTrackerUp = Vector3.up;

		[Tooltip("Local axis of the hand trackers pointing from the wrist towards the palm.")]
		public Vector3 handTrackerForward = Vector3.forward;

		[Tooltip("Local axis of the hand trackers pointing in the direction of the surface normal of the back of the hand.")]
		public Vector3 handTrackerUp = Vector3.up;

		[Tooltip("Local axis of the foot trackers towards the player's forward direction.")]
		public Vector3 footTrackerForward = Vector3.forward;

		[Tooltip("Local axis of the foot tracker towards the up direction.")]
		public Vector3 footTrackerUp = Vector3.up;

		[Space(10f)]
		[Tooltip("Offset of the head bone from the HMD in (headTrackerForward, headTrackerUp) space relative to the head tracker.")]
		public Vector3 headOffset;

		[Tooltip("Offset of the hand bones from the hand trackers in (handTrackerForward, handTrackerUp) space relative to the hand trackers.")]
		public Vector3 handOffset;

		[Tooltip("Forward offset of the foot bones from the foot trackers.")]
		public float footForwardOffset;

		[Tooltip("Inward offset of the foot bones from the foot trackers.")]
		public float footInwardOffset;

		[Tooltip("Used for adjusting foot heading relative to the foot trackers.")]
		[Range(-180f, 180f)]
		public float footHeadingOffset;

		[Range(0f, 1f)]
		public float pelvisPositionWeight = 1f;

		[Range(0f, 1f)]
		public float pelvisRotationWeight = 1f;

		public Settings()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public class CalibrationData
	{
		[Serializable]
		public class Target
		{
			public bool used;

			public Vector3 localPosition;

			public Quaternion localRotation;

			public Target(Transform t)
			{
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_002f: Unknown result type (might be due to invalid IL or missing references)
				used = (Object)(object)t != (Object)null;
				if (used)
				{
					localPosition = t.localPosition;
					localRotation = t.localRotation;
				}
			}

			public void SetTo(Transform t)
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				if (used)
				{
					t.localPosition = localPosition;
					t.localRotation = localRotation;
				}
			}
		}

		public float scale;

		public Target head;

		public Target leftHand;

		public Target rightHand;

		public Target pelvis;

		public Target leftFoot;

		public Target rightFoot;

		public Target leftLegGoal;

		public Target rightLegGoal;

		public Vector3 pelvisTargetRight;

		public float pelvisPositionWeight;

		public float pelvisRotationWeight;
	}

	public static void RecalibrateScale(VRIK ik, CalibrationData data, Settings settings)
	{
		RecalibrateScale(ik, data, settings.scaleMlp);
	}

	public static void RecalibrateScale(VRIK ik, CalibrationData data, float scaleMlp)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		CalibrateScale(ik, scaleMlp);
		data.scale = ik.references.root.localScale.y;
	}

	private static void CalibrateScale(VRIK ik, Settings settings)
	{
		CalibrateScale(ik, settings.scaleMlp);
	}

	private static void CalibrateScale(VRIK ik, float scaleMlp = 1f)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		float num = (ik.solver.spine.headTarget.position.y - ik.references.root.position.y) / (ik.references.head.position.y - ik.references.root.position.y);
		Transform root = ik.references.root;
		root.localScale *= num * scaleMlp;
	}

	public static CalibrationData Calibrate(VRIK ik, Settings settings, Transform headTracker, Transform bodyTracker = null, Transform leftHandTracker = null, Transform rightHandTracker = null, Transform leftFootTracker = null, Transform rightFootTracker = null)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_0477: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_050e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0510: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0632: Unknown result type (might be due to invalid IL or missing references)
		//IL_0707: Unknown result type (might be due to invalid IL or missing references)
		//IL_0802: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0807: Unknown result type (might be due to invalid IL or missing references)
		if (!ik.solver.initiated)
		{
			Debug.LogError((object)"Can not calibrate before VRIK has initiated.");
			return null;
		}
		if ((Object)(object)headTracker == (Object)null)
		{
			Debug.LogError((object)"Can not calibrate VRIK without the head tracker.");
			return null;
		}
		CalibrationData calibrationData = new CalibrationData();
		ik.solver.FixTransforms();
		Vector3 val = headTracker.position + headTracker.rotation * Quaternion.LookRotation(settings.headTrackerForward, settings.headTrackerUp) * settings.headOffset;
		ik.references.root.position = new Vector3(val.x, ik.references.root.position.y, val.z);
		Vector3 val2 = headTracker.rotation * settings.headTrackerForward;
		val2.y = 0f;
		ik.references.root.rotation = Quaternion.LookRotation(val2);
		Transform val3 = (((Object)(object)ik.solver.spine.headTarget == (Object)null) ? new GameObject("Head Target").transform : ik.solver.spine.headTarget);
		val3.position = val;
		val3.rotation = ik.references.head.rotation;
		val3.parent = headTracker;
		ik.solver.spine.headTarget = val3;
		float num = (val3.position.y - ik.references.root.position.y) / (ik.references.head.position.y - ik.references.root.position.y);
		Transform root = ik.references.root;
		root.localScale *= num * settings.scaleMlp;
		if ((Object)(object)bodyTracker != (Object)null)
		{
			Transform val4 = (((Object)(object)ik.solver.spine.pelvisTarget == (Object)null) ? new GameObject("Pelvis Target").transform : ik.solver.spine.pelvisTarget);
			val4.position = ik.references.pelvis.position;
			val4.rotation = ik.references.pelvis.rotation;
			val4.parent = bodyTracker;
			ik.solver.spine.pelvisTarget = val4;
			ik.solver.spine.pelvisPositionWeight = settings.pelvisPositionWeight;
			ik.solver.spine.pelvisRotationWeight = settings.pelvisRotationWeight;
			ik.solver.plantFeet = false;
			ik.solver.spine.maxRootAngle = 180f;
		}
		else if ((Object)(object)leftFootTracker != (Object)null && (Object)(object)rightFootTracker != (Object)null)
		{
			ik.solver.spine.maxRootAngle = 0f;
		}
		if ((Object)(object)leftHandTracker != (Object)null)
		{
			Transform val5 = (((Object)(object)ik.solver.leftArm.target == (Object)null) ? new GameObject("Left Hand Target").transform : ik.solver.leftArm.target);
			val5.position = leftHandTracker.position + leftHandTracker.rotation * Quaternion.LookRotation(settings.handTrackerForward, settings.handTrackerUp) * settings.handOffset;
			val5.rotation = QuaTools.MatchRotation(axis2: Vector3.Cross(ik.solver.leftArm.wristToPalmAxis, ik.solver.leftArm.palmToThumbAxis), targetRotation: leftHandTracker.rotation * Quaternion.LookRotation(settings.handTrackerForward, settings.handTrackerUp), targetAxis1: settings.handTrackerForward, targetAxis2: settings.handTrackerUp, axis1: ik.solver.leftArm.wristToPalmAxis);
			val5.parent = leftHandTracker;
			ik.solver.leftArm.target = val5;
			ik.solver.leftArm.positionWeight = 1f;
			ik.solver.leftArm.rotationWeight = 1f;
		}
		else
		{
			ik.solver.leftArm.positionWeight = 0f;
			ik.solver.leftArm.rotationWeight = 0f;
		}
		if ((Object)(object)rightHandTracker != (Object)null)
		{
			Transform val6 = (((Object)(object)ik.solver.rightArm.target == (Object)null) ? new GameObject("Right Hand Target").transform : ik.solver.rightArm.target);
			val6.position = rightHandTracker.position + rightHandTracker.rotation * Quaternion.LookRotation(settings.handTrackerForward, settings.handTrackerUp) * settings.handOffset;
			val6.rotation = QuaTools.MatchRotation(axis2: -Vector3.Cross(ik.solver.rightArm.wristToPalmAxis, ik.solver.rightArm.palmToThumbAxis), targetRotation: rightHandTracker.rotation * Quaternion.LookRotation(settings.handTrackerForward, settings.handTrackerUp), targetAxis1: settings.handTrackerForward, targetAxis2: settings.handTrackerUp, axis1: ik.solver.rightArm.wristToPalmAxis);
			val6.parent = rightHandTracker;
			ik.solver.rightArm.target = val6;
			ik.solver.rightArm.positionWeight = 1f;
			ik.solver.rightArm.rotationWeight = 1f;
		}
		else
		{
			ik.solver.rightArm.positionWeight = 0f;
			ik.solver.rightArm.rotationWeight = 0f;
		}
		if ((Object)(object)leftFootTracker != (Object)null)
		{
			CalibrateLeg(settings, leftFootTracker, ik.solver.leftLeg, ((Object)(object)ik.references.leftToes != (Object)null) ? ik.references.leftToes : ik.references.leftFoot, ik.references.root.forward, isLeft: true);
		}
		if ((Object)(object)rightFootTracker != (Object)null)
		{
			CalibrateLeg(settings, rightFootTracker, ik.solver.rightLeg, ((Object)(object)ik.references.rightToes != (Object)null) ? ik.references.rightToes : ik.references.rightFoot, ik.references.root.forward, isLeft: false);
		}
		bool num2 = (Object)(object)bodyTracker != (Object)null || ((Object)(object)leftFootTracker != (Object)null && (Object)(object)rightFootTracker != (Object)null);
		VRIKRootController vRIKRootController = ((Component)ik.references.root).GetComponent<VRIKRootController>();
		if (num2)
		{
			if ((Object)(object)vRIKRootController == (Object)null)
			{
				vRIKRootController = ((Component)ik.references.root).gameObject.AddComponent<VRIKRootController>();
			}
			vRIKRootController.Calibrate();
		}
		else if ((Object)(object)vRIKRootController != (Object)null)
		{
			Object.Destroy((Object)(object)vRIKRootController);
		}
		ik.solver.spine.minHeadHeight = 0f;
		ik.solver.locomotion.weight = (((Object)(object)bodyTracker == (Object)null && (Object)(object)leftFootTracker == (Object)null && (Object)(object)rightFootTracker == (Object)null) ? 1f : 0f);
		calibrationData.scale = ik.references.root.localScale.y;
		calibrationData.head = new CalibrationData.Target(ik.solver.spine.headTarget);
		calibrationData.pelvis = new CalibrationData.Target(ik.solver.spine.pelvisTarget);
		calibrationData.leftHand = new CalibrationData.Target(ik.solver.leftArm.target);
		calibrationData.rightHand = new CalibrationData.Target(ik.solver.rightArm.target);
		calibrationData.leftFoot = new CalibrationData.Target(ik.solver.leftLeg.target);
		calibrationData.rightFoot = new CalibrationData.Target(ik.solver.rightLeg.target);
		calibrationData.leftLegGoal = new CalibrationData.Target(ik.solver.leftLeg.bendGoal);
		calibrationData.rightLegGoal = new CalibrationData.Target(ik.solver.rightLeg.bendGoal);
		calibrationData.pelvisTargetRight = (((Object)(object)vRIKRootController != (Object)null) ? vRIKRootController.pelvisTargetRight : Vector3.zero);
		calibrationData.pelvisPositionWeight = ik.solver.spine.pelvisPositionWeight;
		calibrationData.pelvisRotationWeight = ik.solver.spine.pelvisRotationWeight;
		return calibrationData;
	}

	private static void CalibrateLeg(Settings settings, Transform tracker, IKSolverVR.Leg leg, Transform lastBone, Vector3 rootForward, bool isLeft)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		string text = (isLeft ? "Left" : "Right");
		Transform val = (((Object)(object)leg.target == (Object)null) ? new GameObject(text + " Foot Target").transform : leg.target);
		Quaternion val2 = tracker.rotation * Quaternion.LookRotation(settings.footTrackerForward, settings.footTrackerUp);
		Vector3 val3 = val2 * Vector3.forward;
		val3.y = 0f;
		val2 = Quaternion.LookRotation(val3);
		float num = (isLeft ? settings.footInwardOffset : (0f - settings.footInwardOffset));
		val.position = tracker.position + val2 * new Vector3(num, 0f, settings.footForwardOffset);
		val.position = new Vector3(val.position.x, lastBone.position.y, val.position.z);
		val.rotation = lastBone.rotation;
		Vector3 val4 = AxisTools.GetAxisVectorToDirection(lastBone, rootForward);
		if (Vector3.Dot(lastBone.rotation * val4, rootForward) < 0f)
		{
			val4 = -val4;
		}
		Vector3 val5 = Quaternion.Inverse(Quaternion.LookRotation(val.rotation * val4)) * val3;
		float num2 = Mathf.Atan2(val5.x, val5.z) * 57.29578f;
		float num3 = (isLeft ? settings.footHeadingOffset : (0f - settings.footHeadingOffset));
		val.rotation = Quaternion.AngleAxis(num2 + num3, Vector3.up) * val.rotation;
		val.parent = tracker;
		leg.target = val;
		leg.positionWeight = 1f;
		leg.rotationWeight = 1f;
		Transform val6 = (((Object)(object)leg.bendGoal == (Object)null) ? new GameObject(text + " Leg Bend Goal").transform : leg.bendGoal);
		val6.position = lastBone.position + val2 * Vector3.forward + val2 * Vector3.up;
		val6.parent = tracker;
		leg.bendGoal = val6;
		leg.bendGoalWeight = 1f;
	}

	public static void Calibrate(VRIK ik, CalibrationData data, Transform headTracker, Transform bodyTracker = null, Transform leftHandTracker = null, Transform rightHandTracker = null, Transform leftFootTracker = null, Transform rightFootTracker = null)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
		if (!ik.solver.initiated)
		{
			Debug.LogError((object)"Can not calibrate before VRIK has initiated.");
			return;
		}
		if ((Object)(object)headTracker == (Object)null)
		{
			Debug.LogError((object)"Can not calibrate VRIK without the head tracker.");
			return;
		}
		ik.solver.FixTransforms();
		Transform val = (((Object)(object)ik.solver.spine.headTarget == (Object)null) ? new GameObject("Head Target").transform : ik.solver.spine.headTarget);
		val.parent = headTracker;
		data.head.SetTo(val);
		ik.solver.spine.headTarget = val;
		ik.references.root.localScale = data.scale * Vector3.one;
		if ((Object)(object)bodyTracker != (Object)null && data.pelvis != null)
		{
			Transform val2 = (((Object)(object)ik.solver.spine.pelvisTarget == (Object)null) ? new GameObject("Pelvis Target").transform : ik.solver.spine.pelvisTarget);
			val2.parent = bodyTracker;
			data.pelvis.SetTo(val2);
			ik.solver.spine.pelvisTarget = val2;
			ik.solver.spine.pelvisPositionWeight = data.pelvisPositionWeight;
			ik.solver.spine.pelvisRotationWeight = data.pelvisRotationWeight;
			ik.solver.plantFeet = false;
			ik.solver.spine.maxRootAngle = 180f;
		}
		else if ((Object)(object)leftFootTracker != (Object)null && (Object)(object)rightFootTracker != (Object)null)
		{
			ik.solver.spine.maxRootAngle = 0f;
		}
		if ((Object)(object)leftHandTracker != (Object)null)
		{
			Transform val3 = (((Object)(object)ik.solver.leftArm.target == (Object)null) ? new GameObject("Left Hand Target").transform : ik.solver.leftArm.target);
			val3.parent = leftHandTracker;
			data.leftHand.SetTo(val3);
			ik.solver.leftArm.target = val3;
			ik.solver.leftArm.positionWeight = 1f;
			ik.solver.leftArm.rotationWeight = 1f;
		}
		else
		{
			ik.solver.leftArm.positionWeight = 0f;
			ik.solver.leftArm.rotationWeight = 0f;
		}
		if ((Object)(object)rightHandTracker != (Object)null)
		{
			Transform val4 = (((Object)(object)ik.solver.rightArm.target == (Object)null) ? new GameObject("Right Hand Target").transform : ik.solver.rightArm.target);
			val4.parent = rightHandTracker;
			data.rightHand.SetTo(val4);
			ik.solver.rightArm.target = val4;
			ik.solver.rightArm.positionWeight = 1f;
			ik.solver.rightArm.rotationWeight = 1f;
		}
		else
		{
			ik.solver.rightArm.positionWeight = 0f;
			ik.solver.rightArm.rotationWeight = 0f;
		}
		if ((Object)(object)leftFootTracker != (Object)null)
		{
			CalibrateLeg(data, leftFootTracker, ik.solver.leftLeg, ((Object)(object)ik.references.leftToes != (Object)null) ? ik.references.leftToes : ik.references.leftFoot, ik.references.root.forward, isLeft: true);
		}
		if ((Object)(object)rightFootTracker != (Object)null)
		{
			CalibrateLeg(data, rightFootTracker, ik.solver.rightLeg, ((Object)(object)ik.references.rightToes != (Object)null) ? ik.references.rightToes : ik.references.rightFoot, ik.references.root.forward, isLeft: false);
		}
		bool num = (Object)(object)bodyTracker != (Object)null || ((Object)(object)leftFootTracker != (Object)null && (Object)(object)rightFootTracker != (Object)null);
		VRIKRootController vRIKRootController = ((Component)ik.references.root).GetComponent<VRIKRootController>();
		if (num)
		{
			if ((Object)(object)vRIKRootController == (Object)null)
			{
				vRIKRootController = ((Component)ik.references.root).gameObject.AddComponent<VRIKRootController>();
			}
			vRIKRootController.Calibrate(data);
		}
		else if ((Object)(object)vRIKRootController != (Object)null)
		{
			Object.Destroy((Object)(object)vRIKRootController);
		}
		ik.solver.spine.minHeadHeight = 0f;
		ik.solver.locomotion.weight = (((Object)(object)bodyTracker == (Object)null && (Object)(object)leftFootTracker == (Object)null && (Object)(object)rightFootTracker == (Object)null) ? 1f : 0f);
	}

	private static void CalibrateLeg(CalibrationData data, Transform tracker, IKSolverVR.Leg leg, Transform lastBone, Vector3 rootForward, bool isLeft)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		if ((!isLeft || data.leftFoot != null) && (isLeft || data.rightFoot != null))
		{
			string text = (isLeft ? "Left" : "Right");
			Transform val = (((Object)(object)leg.target == (Object)null) ? new GameObject(text + " Foot Target").transform : leg.target);
			val.parent = tracker;
			if (isLeft)
			{
				data.leftFoot.SetTo(val);
			}
			else
			{
				data.rightFoot.SetTo(val);
			}
			leg.target = val;
			leg.positionWeight = 1f;
			leg.rotationWeight = 1f;
			Transform val2 = (((Object)(object)leg.bendGoal == (Object)null) ? new GameObject(text + " Leg Bend Goal").transform : leg.bendGoal);
			val2.parent = tracker;
			if (isLeft)
			{
				data.leftLegGoal.SetTo(val2);
			}
			else
			{
				data.rightLegGoal.SetTo(val2);
			}
			leg.bendGoal = val2;
			leg.bendGoalWeight = 1f;
		}
	}

	public static CalibrationData Calibrate(VRIK ik, Transform centerEyeAnchor, Transform leftHandAnchor, Transform rightHandAnchor, Vector3 centerEyePositionOffset, Vector3 centerEyeRotationOffset, Vector3 handPositionOffset, Vector3 handRotationOffset, float scaleMlp = 1f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		CalibrateHead(ik, centerEyeAnchor, centerEyePositionOffset, centerEyeRotationOffset);
		CalibrateHands(ik, leftHandAnchor, rightHandAnchor, handPositionOffset, handRotationOffset);
		CalibrateScale(ik, scaleMlp);
		return new CalibrationData
		{
			scale = ik.references.root.localScale.y,
			head = new CalibrationData.Target(ik.solver.spine.headTarget),
			leftHand = new CalibrationData.Target(ik.solver.leftArm.target),
			rightHand = new CalibrationData.Target(ik.solver.rightArm.target)
		};
	}

	public static void CalibrateHead(VRIK ik, Transform centerEyeAnchor, Vector3 anchorPositionOffset, Vector3 anchorRotationOffset)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ik.solver.spine.headTarget == (Object)null)
		{
			ik.solver.spine.headTarget = new GameObject("Head IK Target").transform;
		}
		Vector3 val = Quaternion.Inverse(ik.references.head.rotation) * ik.references.root.forward;
		Vector3 val2 = Quaternion.Inverse(ik.references.head.rotation) * ik.references.root.up;
		Quaternion val3 = Quaternion.LookRotation(val, val2);
		Vector3 val4 = ik.references.head.position + ik.references.head.rotation * val3 * anchorPositionOffset;
		Quaternion val5 = Quaternion.Inverse(ik.references.head.rotation * val3 * Quaternion.Euler(anchorRotationOffset));
		ik.solver.spine.headTarget.parent = centerEyeAnchor;
		ik.solver.spine.headTarget.localPosition = val5 * (ik.references.head.position - val4);
		ik.solver.spine.headTarget.localRotation = val5 * ik.references.head.rotation;
	}

	public static void CalibrateBody(VRIK ik, Transform pelvisTracker, Vector3 trackerPositionOffset, Vector3 trackerRotationOffset)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ik.solver.spine.pelvisTarget == (Object)null)
		{
			ik.solver.spine.pelvisTarget = new GameObject("Pelvis IK Target").transform;
		}
		ik.solver.spine.pelvisTarget.position = ik.references.pelvis.position + ik.references.root.rotation * trackerPositionOffset;
		ik.solver.spine.pelvisTarget.rotation = ik.references.root.rotation * Quaternion.Euler(trackerRotationOffset);
		ik.solver.spine.pelvisTarget.parent = pelvisTracker;
	}

	public static void CalibrateHands(VRIK ik, Transform leftHandAnchor, Transform rightHandAnchor, Vector3 anchorPositionOffset, Vector3 anchorRotationOffset)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ik.solver.leftArm.target == (Object)null)
		{
			ik.solver.leftArm.target = new GameObject("Left Hand IK Target").transform;
		}
		if ((Object)(object)ik.solver.rightArm.target == (Object)null)
		{
			ik.solver.rightArm.target = new GameObject("Right Hand IK Target").transform;
		}
		CalibrateHand(ik, leftHandAnchor, anchorPositionOffset, anchorRotationOffset, isLeft: true);
		CalibrateHand(ik, rightHandAnchor, anchorPositionOffset, anchorRotationOffset, isLeft: false);
	}

	private static void CalibrateHand(VRIK ik, Transform anchor, Vector3 positionOffset, Vector3 rotationOffset, bool isLeft)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		if (isLeft)
		{
			positionOffset.x = 0f - positionOffset.x;
			rotationOffset.y = 0f - rotationOffset.y;
			rotationOffset.z = 0f - rotationOffset.z;
		}
		Transform val = (isLeft ? ik.references.leftHand : ik.references.rightHand);
		Transform forearm = (isLeft ? ik.references.leftForearm : ik.references.rightForearm);
		Transform val2 = (isLeft ? ik.solver.leftArm.target : ik.solver.rightArm.target);
		Vector3 val3 = (isLeft ? ik.solver.leftArm.wristToPalmAxis : ik.solver.rightArm.wristToPalmAxis);
		if (val3 == Vector3.zero)
		{
			val3 = GuessWristToPalmAxis(val, forearm);
		}
		Vector3 val4 = (isLeft ? ik.solver.leftArm.palmToThumbAxis : ik.solver.rightArm.palmToThumbAxis);
		if (val4 == Vector3.zero)
		{
			val4 = GuessPalmToThumbAxis(val, forearm);
		}
		Quaternion val5 = Quaternion.LookRotation(val3, val4);
		Vector3 val6 = val.position + val.rotation * val5 * positionOffset;
		Quaternion val7 = Quaternion.Inverse(val.rotation * val5 * Quaternion.Euler(rotationOffset));
		val2.parent = anchor;
		val2.localPosition = val7 * (val.position - val6);
		val2.localRotation = val7 * val.rotation;
	}

	public static Vector3 GuessWristToPalmAxis(Transform hand, Transform forearm)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = forearm.position - hand.position;
		Vector3 val2 = AxisTools.ToVector3(AxisTools.GetAxisToDirection(hand, val));
		if (Vector3.Dot(val, hand.rotation * val2) > 0f)
		{
			val2 = -val2;
		}
		return val2;
	}

	public static Vector3 GuessPalmToThumbAxis(Transform hand, Transform forearm)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (hand.childCount == 0)
		{
			Debug.LogWarning((object)("Hand " + ((Object)hand).name + " does not have any fingers, VRIK can not guess the hand bone's orientation. Please assign 'Wrist To Palm Axis' and 'Palm To Thumb Axis' manually for both arms in VRIK settings."), (Object)(object)hand);
			return Vector3.zero;
		}
		float num = float.PositiveInfinity;
		int num2 = 0;
		for (int i = 0; i < hand.childCount; i++)
		{
			float num3 = Vector3.SqrMagnitude(hand.GetChild(i).position - hand.position);
			if (num3 < num)
			{
				num = num3;
				num2 = i;
			}
		}
		Vector3 val = Vector3.Cross(Vector3.Cross(hand.position - forearm.position, hand.GetChild(num2).position - hand.position), hand.position - forearm.position);
		Vector3 val2 = AxisTools.ToVector3(AxisTools.GetAxisToDirection(hand, val));
		if (Vector3.Dot(val, hand.rotation * val2) < 0f)
		{
			val2 = -val2;
		}
		return val2;
	}
}
