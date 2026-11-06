using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverVR : IKSolver
{
	[Serializable]
	public class Arm : BodyPart
	{
		[Serializable]
		public enum ShoulderRotationMode
		{
			YawPitch,
			FromTo
		}

		[LargeHeader("Hand")]
		[Tooltip("The hand target. This should not be the hand controller itself, but a child GameObject parented to it so you could adjust its position/rotation to match the orientation of the hand bone. The best practice for setup would be to move the hand controller to the avatar's hand as it it was held by the avatar, duplicate the avatar's hand bone and parent it to the hand controller. Then assign the duplicate to this slot.")]
		public Transform target;

		[Tooltip("Positional weight of the hand target. Note that if you have nulled the target, the hand will still be pulled to the last position of the target until you set this value to 0.")]
		[Range(0f, 1f)]
		public float positionWeight = 1f;

		[Tooltip("Rotational weight of the hand target. Note that if you have nulled the target, the hand will still be rotated to the last rotation of the target until you set this value to 0.")]
		[Range(0f, 1f)]
		public float rotationWeight = 1f;

		[LargeHeader("Shoulder")]
		[Tooltip("The weight of shoulder rotation")]
		[Range(0f, 1f)]
		public float shoulderRotationWeight = 1f;

		[Tooltip("Different techniques for shoulder bone rotation.")]
		[ShowIf("shoulderRotationWeight", 0f, float.PositiveInfinity, false, ShowIfMode.Hidden)]
		public ShoulderRotationMode shoulderRotationMode;

		[Tooltip("The weight of twisting the shoulders backwards when arms are lifted up.")]
		[ShowRangeIf(0f, 1f, "shoulderRotationWeight", 0f, float.PositiveInfinity, false, ShowIfMode.Hidden)]
		public float shoulderTwistWeight = 1f;

		[Tooltip("Tweak this value to adjust shoulder rotation around the yaw (up) axis.")]
		[ShowIf("shoulderRotationWeight", 0f, float.PositiveInfinity, false, ShowIfMode.Hidden)]
		public float shoulderYawOffset = 45f;

		[Tooltip("Tweak this value to adjust shoulder rotation around the pitch (forward) axis.")]
		[ShowIf("shoulderRotationWeight", 0f, float.PositiveInfinity, false, ShowIfMode.Hidden)]
		public float shoulderPitchOffset = -30f;

		[LargeHeader("Bending")]
		[Tooltip("The elbow will be bent towards this Transform if 'Bend Goal Weight' > 0.")]
		public Transform bendGoal;

		[Tooltip("If greater than 0, will bend the elbow towards the 'Bend Goal' Transform.")]
		[Range(0f, 1f)]
		public float bendGoalWeight;

		[Tooltip("Angular offset of the elbow bending direction.")]
		[Range(-180f, 180f)]
		public float swivelOffset;

		[Tooltip("Local axis of the hand bone that points from the wrist towards the palm. Used for defining hand bone orientation. If you have copied VRIK component from another avatar that has different bone orientations, right-click on VRIK header and select 'Guess Hand Orientations' from the context menu.")]
		public Vector3 wristToPalmAxis = Vector3.zero;

		[Tooltip("Local axis of the hand bone that points from the palm towards the thumb. Used for defining hand bone orientation. If you have copied VRIK component from another avatar that has different bone orientations, right-click on VRIK header and select 'Guess Hand Orientations' from the context menu.")]
		public Vector3 palmToThumbAxis = Vector3.zero;

		[LargeHeader("Stretching")]
		[Tooltip("Use this to make the arm shorter/longer. Works by displacement of hand and forearm localPosition.")]
		[Range(0.01f, 2f)]
		public float armLengthMlp = 1f;

		[Tooltip("'Time' represents (target distance / arm length) and 'value' represents the amount of stretching. So value at time 1 represents stretching amount at the point where distance to the target is equal to arm length. Value at time 2 represents stretching amount at the point where distance to the target is double the arm length. Linear stretching would be achieved with a linear curve going up by 45 degrees. Increase the range of stretching by moving the last key up and right by the same amount. Smoothing in the curve can help reduce elbow snapping (start stretching the arm slightly before target distance reaches arm length). To get a good optimal value for this curve, please go to the 'VRIK (Basic)' demo scene and copy the stretch curve over from the Pilot character.")]
		public AnimationCurve stretchCurve = new AnimationCurve();

		[NonSerialized]
		[HideInInspector]
		public Vector3 IKPosition;

		[NonSerialized]
		[HideInInspector]
		public Quaternion IKRotation = Quaternion.identity;

		[NonSerialized]
		[HideInInspector]
		public Vector3 bendDirection = Vector3.back;

		[NonSerialized]
		[HideInInspector]
		public Vector3 handPositionOffset;

		private bool hasShoulder;

		private Vector3 chestForwardAxis;

		private Vector3 chestUpAxis;

		private Quaternion chestRotation = Quaternion.identity;

		private Vector3 chestForward;

		private Vector3 chestUp;

		private Quaternion forearmRelToUpperArm = Quaternion.identity;

		private Vector3 upperArmBendAxis;

		public Vector3 position
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return field;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				field = value;
			}
		}

		public Quaternion rotation
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return field;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				field = value;
			}
		}

		private VirtualBone shoulder => bones[0];

		private VirtualBone upperArm => bones[hasShoulder ? 1 : 0];

		private VirtualBone forearm => bones[(!hasShoulder) ? 1 : 2];

		private VirtualBone hand => bones[hasShoulder ? 3 : 2];

		protected override void OnRead(Vector3[] positions, Quaternion[] rotations, bool hasChest, bool hasNeck, bool hasShoulders, bool hasToes, bool hasLegs, int rootIndex, int index)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0221: Unknown result type (might be due to invalid IL or missing references)
			//IL_0222: Unknown result type (might be due to invalid IL or missing references)
			//IL_0230: Unknown result type (might be due to invalid IL or missing references)
			//IL_0232: Unknown result type (might be due to invalid IL or missing references)
			//IL_0241: Unknown result type (might be due to invalid IL or missing references)
			//IL_0243: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_020f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0211: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = positions[index];
			Quaternion val2 = rotations[index];
			Vector3 val3 = positions[index + 1];
			Quaternion val4 = rotations[index + 1];
			Vector3 val5 = positions[index + 2];
			Quaternion val6 = rotations[index + 2];
			Vector3 iKPosition = positions[index + 3];
			Quaternion iKRotation = rotations[index + 3];
			if (!initiated)
			{
				IKPosition = iKPosition;
				IKRotation = iKRotation;
				rotation = IKRotation;
				hasShoulder = hasShoulders;
				bones = new VirtualBone[hasShoulder ? 4 : 3];
				if (hasShoulder)
				{
					bones[0] = new VirtualBone(val, val2);
					bones[1] = new VirtualBone(val3, val4);
					bones[2] = new VirtualBone(val5, val6);
					bones[3] = new VirtualBone(iKPosition, iKRotation);
				}
				else
				{
					bones[0] = new VirtualBone(val3, val4);
					bones[1] = new VirtualBone(val5, val6);
					bones[2] = new VirtualBone(iKPosition, iKRotation);
				}
				Vector3 val7 = rotations[0] * Vector3.forward;
				chestForwardAxis = Quaternion.Inverse(rootRotation) * val7;
				chestUpAxis = Quaternion.Inverse(rootRotation) * (rotations[0] * Vector3.up);
				Vector3 val8 = AxisTools.GetAxisVectorToDirection(val4, val7);
				if (Vector3.Dot(val4 * val8, val7) < 0f)
				{
					val8 = -val8;
				}
				upperArmBendAxis = Vector3.Cross(Quaternion.Inverse(val4) * (val5 - val3), val8);
				if (upperArmBendAxis == Vector3.zero)
				{
					Debug.LogWarning((object)"VRIK can not calculate which way to bend the arms because the arms are perfectly straight. Please rotate the elbow bones slightly in their natural bending direction in the Editor.");
				}
			}
			if (hasShoulder)
			{
				bones[0].Read(val, val2);
				bones[1].Read(val3, val4);
				bones[2].Read(val5, val6);
				bones[3].Read(iKPosition, iKRotation);
			}
			else
			{
				bones[0].Read(val3, val4);
				bones[1].Read(val5, val6);
				bones[2].Read(iKPosition, iKRotation);
			}
		}

		public override void PreSolve(float scale)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)target != (Object)null)
			{
				IKPosition = target.position;
				IKRotation = target.rotation;
			}
			position = V3Tools.Lerp(hand.solverPosition, IKPosition, positionWeight);
			rotation = QuaTools.Lerp(hand.solverRotation, IKRotation, rotationWeight);
			shoulder.axis = shoulder.axis.normalized;
			forearmRelToUpperArm = Quaternion.Inverse(upperArm.solverRotation) * forearm.solverRotation;
		}

		public override void ApplyOffsets(float scale)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			position += handPositionOffset;
		}

		private void Stretching()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			float num = upperArm.length + forearm.length;
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			if (armLengthMlp != 1f)
			{
				num *= armLengthMlp;
				zero = (forearm.solverPosition - upperArm.solverPosition) * (armLengthMlp - 1f);
				zero2 = (hand.solverPosition - forearm.solverPosition) * (armLengthMlp - 1f);
				VirtualBone virtualBone = forearm;
				virtualBone.solverPosition += zero;
				VirtualBone virtualBone2 = hand;
				virtualBone2.solverPosition += zero + zero2;
			}
			float num2 = Vector3.Distance(upperArm.solverPosition, position) / num;
			float num3 = stretchCurve.Evaluate(num2);
			num3 *= positionWeight;
			zero = (forearm.solverPosition - upperArm.solverPosition) * num3;
			zero2 = (hand.solverPosition - forearm.solverPosition) * num3;
			VirtualBone virtualBone3 = forearm;
			virtualBone3.solverPosition += zero;
			VirtualBone virtualBone4 = hand;
			virtualBone4.solverPosition += zero + zero2;
		}

		public void Solve(bool isLeft)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_06bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_06c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_06cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_06d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_06d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0707: Unknown result type (might be due to invalid IL or missing references)
			//IL_070c: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0420: Unknown result type (might be due to invalid IL or missing references)
			//IL_0425: Unknown result type (might be due to invalid IL or missing references)
			//IL_042d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0438: Unknown result type (might be due to invalid IL or missing references)
			//IL_043d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0442: Unknown result type (might be due to invalid IL or missing references)
			//IL_0446: Unknown result type (might be due to invalid IL or missing references)
			//IL_044c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0451: Unknown result type (might be due to invalid IL or missing references)
			//IL_0457: Unknown result type (might be due to invalid IL or missing references)
			//IL_0462: Unknown result type (might be due to invalid IL or missing references)
			//IL_0467: Unknown result type (might be due to invalid IL or missing references)
			//IL_046c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0471: Unknown result type (might be due to invalid IL or missing references)
			//IL_0473: Unknown result type (might be due to invalid IL or missing references)
			//IL_0478: Unknown result type (might be due to invalid IL or missing references)
			//IL_048d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0492: Unknown result type (might be due to invalid IL or missing references)
			//IL_049a: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_050b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0516: Unknown result type (might be due to invalid IL or missing references)
			//IL_051b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0520: Unknown result type (might be due to invalid IL or missing references)
			//IL_0525: Unknown result type (might be due to invalid IL or missing references)
			//IL_0530: Unknown result type (might be due to invalid IL or missing references)
			//IL_0535: Unknown result type (might be due to invalid IL or missing references)
			//IL_0542: Unknown result type (might be due to invalid IL or missing references)
			//IL_0548: Unknown result type (might be due to invalid IL or missing references)
			//IL_054d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0552: Unknown result type (might be due to invalid IL or missing references)
			//IL_0557: Unknown result type (might be due to invalid IL or missing references)
			//IL_0558: Unknown result type (might be due to invalid IL or missing references)
			//IL_0560: Unknown result type (might be due to invalid IL or missing references)
			//IL_0565: Unknown result type (might be due to invalid IL or missing references)
			//IL_056a: Unknown result type (might be due to invalid IL or missing references)
			//IL_056f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0577: Unknown result type (might be due to invalid IL or missing references)
			//IL_0582: Unknown result type (might be due to invalid IL or missing references)
			//IL_0587: Unknown result type (might be due to invalid IL or missing references)
			//IL_058c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0591: Unknown result type (might be due to invalid IL or missing references)
			//IL_0593: Unknown result type (might be due to invalid IL or missing references)
			//IL_059a: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_05b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0851: Unknown result type (might be due to invalid IL or missing references)
			//IL_0856: Unknown result type (might be due to invalid IL or missing references)
			//IL_073a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0740: Unknown result type (might be due to invalid IL or missing references)
			//IL_0745: Unknown result type (might be due to invalid IL or missing references)
			//IL_0750: Unknown result type (might be due to invalid IL or missing references)
			//IL_075b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0760: Unknown result type (might be due to invalid IL or missing references)
			//IL_0765: Unknown result type (might be due to invalid IL or missing references)
			//IL_076a: Unknown result type (might be due to invalid IL or missing references)
			//IL_076f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0770: Unknown result type (might be due to invalid IL or missing references)
			//IL_0775: Unknown result type (might be due to invalid IL or missing references)
			//IL_0777: Unknown result type (might be due to invalid IL or missing references)
			//IL_077e: Unknown result type (might be due to invalid IL or missing references)
			//IL_07a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_07bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_07c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_07cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_07d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_07dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_07e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_07e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_07f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_07fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0806: Unknown result type (might be due to invalid IL or missing references)
			//IL_0811: Unknown result type (might be due to invalid IL or missing references)
			//IL_0816: Unknown result type (might be due to invalid IL or missing references)
			//IL_081b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0820: Unknown result type (might be due to invalid IL or missing references)
			//IL_0829: Unknown result type (might be due to invalid IL or missing references)
			//IL_082b: Unknown result type (might be due to invalid IL or missing references)
			//IL_082d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0614: Unknown result type (might be due to invalid IL or missing references)
			//IL_0875: Unknown result type (might be due to invalid IL or missing references)
			//IL_087b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0886: Unknown result type (might be due to invalid IL or missing references)
			//IL_088b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0634: Unknown result type (might be due to invalid IL or missing references)
			//IL_0622: Unknown result type (might be due to invalid IL or missing references)
			//IL_0627: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0639: Unknown result type (might be due to invalid IL or missing references)
			//IL_063e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0649: Unknown result type (might be due to invalid IL or missing references)
			//IL_064e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0653: Unknown result type (might be due to invalid IL or missing references)
			//IL_0666: Unknown result type (might be due to invalid IL or missing references)
			//IL_0686: Unknown result type (might be due to invalid IL or missing references)
			//IL_0674: Unknown result type (might be due to invalid IL or missing references)
			//IL_0679: Unknown result type (might be due to invalid IL or missing references)
			//IL_068b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0690: Unknown result type (might be due to invalid IL or missing references)
			//IL_069b: Unknown result type (might be due to invalid IL or missing references)
			//IL_06a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_06a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_0205: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_021d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0223: Unknown result type (might be due to invalid IL or missing references)
			//IL_0232: Unknown result type (might be due to invalid IL or missing references)
			//IL_022b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_0242: Unknown result type (might be due to invalid IL or missing references)
			//IL_0247: Unknown result type (might be due to invalid IL or missing references)
			//IL_024c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0251: Unknown result type (might be due to invalid IL or missing references)
			//IL_0252: Unknown result type (might be due to invalid IL or missing references)
			//IL_0253: Unknown result type (might be due to invalid IL or missing references)
			//IL_0258: Unknown result type (might be due to invalid IL or missing references)
			//IL_0259: Unknown result type (might be due to invalid IL or missing references)
			//IL_025e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0260: Unknown result type (might be due to invalid IL or missing references)
			//IL_0267: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_030d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0318: Unknown result type (might be due to invalid IL or missing references)
			//IL_031d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0322: Unknown result type (might be due to invalid IL or missing references)
			//IL_0327: Unknown result type (might be due to invalid IL or missing references)
			//IL_0332: Unknown result type (might be due to invalid IL or missing references)
			//IL_0337: Unknown result type (might be due to invalid IL or missing references)
			//IL_037f: Unknown result type (might be due to invalid IL or missing references)
			//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_039f: Unknown result type (might be due to invalid IL or missing references)
			//IL_038d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0392: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03be: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03df: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0406: Unknown result type (might be due to invalid IL or missing references)
			//IL_040b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0410: Unknown result type (might be due to invalid IL or missing references)
			chestRotation = Quaternion.LookRotation(rootRotation * chestForwardAxis, rootRotation * chestUpAxis);
			chestForward = chestRotation * Vector3.forward;
			chestUp = chestRotation * Vector3.up;
			Vector3 val = Vector3.zero;
			if (hasShoulder && shoulderRotationWeight > 0f && LOD < 1)
			{
				switch (shoulderRotationMode)
				{
				case ShoulderRotationMode.YawPitch:
				{
					Vector3 val7 = position - shoulder.solverPosition;
					val7 = val7.normalized;
					float num4 = (isLeft ? shoulderYawOffset : (0f - shoulderYawOffset));
					Quaternion val8 = Quaternion.AngleAxis((isLeft ? (-90f) : 90f) + num4, chestUp) * chestRotation;
					Vector3 val9 = Quaternion.Inverse(val8) * val7;
					float num5 = Mathf.Atan2(val9.x, val9.z) * 57.29578f;
					float num6 = Vector3.Dot(val9, Vector3.up);
					num6 = 1f - Mathf.Abs(num6);
					num5 *= num6;
					num5 -= num4;
					float num7 = (isLeft ? (-20f) : (-50f));
					float num8 = (isLeft ? 50f : 20f);
					num5 = DamperValue(num5, num7 - num4, num8 - num4, 0.7f);
					Vector3 val10 = shoulder.solverRotation * shoulder.axis;
					Vector3 val11 = val8 * (Quaternion.AngleAxis(num5, Vector3.up) * Vector3.forward);
					Quaternion val12 = Quaternion.FromToRotation(val10, val11);
					val8 = Quaternion.AngleAxis(isLeft ? (-90f) : 90f, chestUp) * chestRotation;
					val8 = Quaternion.AngleAxis(isLeft ? shoulderPitchOffset : (0f - shoulderPitchOffset), chestForward) * val8;
					val7 = position - (shoulder.solverPosition + chestRotation * (isLeft ? Vector3.right : Vector3.left) * mag);
					val9 = Quaternion.Inverse(val8) * val7;
					float num9 = Mathf.Atan2(val9.y, val9.z) * 57.29578f;
					num9 -= shoulderPitchOffset;
					num9 = DamperValue(num9, -45f - shoulderPitchOffset, 45f - shoulderPitchOffset);
					Quaternion val13 = Quaternion.AngleAxis(0f - num9, val8 * Vector3.right) * val12;
					if (shoulderRotationWeight * positionWeight < 1f)
					{
						val13 = Quaternion.Lerp(Quaternion.identity, val13, shoulderRotationWeight * positionWeight);
					}
					VirtualBone.RotateBy(bones, val13);
					Stretching();
					val = GetBendNormal(position - upperArm.solverPosition);
					VirtualBone.SolveTrigonometric(bones, 1, 2, 3, position, val, positionWeight);
					float num10 = Mathf.Clamp(num9 * positionWeight * shoulderRotationWeight * shoulderTwistWeight * 2f, 0f, 180f);
					shoulder.solverRotation = Quaternion.AngleAxis(num10, shoulder.solverRotation * (isLeft ? shoulder.axis : (-shoulder.axis))) * shoulder.solverRotation;
					upperArm.solverRotation = Quaternion.AngleAxis(num10, upperArm.solverRotation * (isLeft ? upperArm.axis : (-upperArm.axis))) * upperArm.solverRotation;
					break;
				}
				case ShoulderRotationMode.FromTo:
				{
					Quaternion solverRotation = shoulder.solverRotation;
					Vector3 val2 = upperArm.solverPosition - shoulder.solverPosition;
					Quaternion val3 = Quaternion.FromToRotation(val2.normalized + chestForward, position - shoulder.solverPosition);
					val3 = Quaternion.Slerp(Quaternion.identity, val3, 0.5f * shoulderRotationWeight * positionWeight);
					VirtualBone.RotateBy(bones, val3);
					Stretching();
					VirtualBone.SolveTrigonometric(bones, 0, 2, 3, position, Vector3.Cross(forearm.solverPosition - shoulder.solverPosition, hand.solverPosition - shoulder.solverPosition), 0.5f * shoulderRotationWeight * positionWeight);
					val = GetBendNormal(position - upperArm.solverPosition);
					VirtualBone.SolveTrigonometric(bones, 1, 2, 3, position, val, positionWeight);
					Quaternion val4 = Quaternion.Inverse(Quaternion.LookRotation(chestUp, chestForward));
					Vector3 val5 = val4 * (solverRotation * shoulder.axis);
					Vector3 val6 = val4 * (shoulder.solverRotation * shoulder.axis);
					float num = Mathf.Atan2(val5.x, val5.z) * 57.29578f;
					float num2 = Mathf.Atan2(val6.x, val6.z) * 57.29578f;
					float num3 = Mathf.DeltaAngle(num, num2);
					if (isLeft)
					{
						num3 = 0f - num3;
					}
					num3 = Mathf.Clamp(num3 * shoulderRotationWeight * shoulderTwistWeight * 2f * positionWeight, 0f, 180f);
					shoulder.solverRotation = Quaternion.AngleAxis(num3, shoulder.solverRotation * (isLeft ? shoulder.axis : (-shoulder.axis))) * shoulder.solverRotation;
					upperArm.solverRotation = Quaternion.AngleAxis(num3, upperArm.solverRotation * (isLeft ? upperArm.axis : (-upperArm.axis))) * upperArm.solverRotation;
					break;
				}
				}
			}
			else
			{
				if (LOD < 1)
				{
					Stretching();
				}
				val = GetBendNormal(position - upperArm.solverPosition);
				if (hasShoulder)
				{
					VirtualBone.SolveTrigonometric(bones, 1, 2, 3, position, val, positionWeight);
				}
				else
				{
					VirtualBone.SolveTrigonometric(bones, 0, 1, 2, position, val, positionWeight);
				}
			}
			if (LOD < 1 && positionWeight > 0f)
			{
				Vector3 val14 = Quaternion.Inverse(Quaternion.LookRotation(upperArm.solverRotation * upperArmBendAxis, forearm.solverPosition - upperArm.solverPosition)) * val;
				float num11 = Mathf.Atan2(val14.x, val14.z) * 57.29578f;
				upperArm.solverRotation = Quaternion.AngleAxis(num11 * positionWeight, forearm.solverPosition - upperArm.solverPosition) * upperArm.solverRotation;
				Quaternion val15 = upperArm.solverRotation * forearmRelToUpperArm;
				Quaternion val16 = Quaternion.FromToRotation(val15 * forearm.axis, hand.solverPosition - forearm.solverPosition);
				RotateTo(forearm, val16 * val15, positionWeight);
			}
			if (rotationWeight >= 1f)
			{
				hand.solverRotation = rotation;
			}
			else if (rotationWeight > 0f)
			{
				hand.solverRotation = Quaternion.Lerp(hand.solverRotation, rotation, rotationWeight);
			}
		}

		public override void ResetOffsets()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			handPositionOffset = Vector3.zero;
		}

		public override void Write(ref Vector3[] solvedPositions, ref Quaternion[] solvedRotations)
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			if (hasShoulder)
			{
				solvedPositions[index] = shoulder.solverPosition;
				solvedRotations[index] = shoulder.solverRotation;
			}
			solvedPositions[index + 1] = upperArm.solverPosition;
			solvedPositions[index + 2] = forearm.solverPosition;
			solvedPositions[index + 3] = hand.solverPosition;
			solvedRotations[index + 1] = upperArm.solverRotation;
			solvedRotations[index + 2] = forearm.solverRotation;
			solvedRotations[index + 3] = hand.solverRotation;
		}

		private float DamperValue(float value, float min, float max, float weight = 1f)
		{
			float num = max - min;
			if (weight < 1f)
			{
				float num2 = max - num * 0.5f;
				float num3 = value - num2;
				num3 *= 0.5f;
				value = num2 + num3;
			}
			value -= min;
			float num4 = Interp.Float(Mathf.Clamp(value / num, 0f, 1f), InterpolationMode.InOutQuintic);
			return Mathf.Lerp(min, max, num4);
		}

		private Vector3 GetBendNormal(Vector3 dir)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)bendGoal != (Object)null)
			{
				bendDirection = bendGoal.position - bones[1].solverPosition;
			}
			Vector3 val = bones[0].solverRotation * bones[0].axis;
			Vector3 down = Vector3.down;
			Vector3 val2 = Quaternion.Inverse(chestRotation) * dir.normalized + Vector3.forward;
			Vector3 val3 = Quaternion.FromToRotation(down, val2) * Vector3.back;
			Vector3 val4 = Quaternion.Inverse(chestRotation) * val;
			val2 = Quaternion.Inverse(chestRotation) * dir;
			val3 = Quaternion.FromToRotation(val4, val2) * val3;
			val3 = chestRotation * val3;
			val3 += val;
			val3 -= rotation * wristToPalmAxis;
			val3 -= rotation * palmToThumbAxis * 0.5f;
			if (bendGoalWeight > 0f)
			{
				val3 = Vector3.Slerp(val3, bendDirection, bendGoalWeight);
			}
			if (swivelOffset != 0f)
			{
				val3 = Quaternion.AngleAxis(swivelOffset, -dir) * val3;
			}
			return Vector3.Cross(val3, dir);
		}

		private void Visualize(VirtualBone bone1, VirtualBone bone2, VirtualBone bone3, Color color)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			Debug.DrawLine(bone1.solverPosition, bone2.solverPosition, color);
			Debug.DrawLine(bone2.solverPosition, bone3.solverPosition, color);
		}

		public Arm()
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Expected Obj, but got Unknown
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public abstract class BodyPart
	{
		[HideInInspector]
		public VirtualBone[] bones = new VirtualBone[0];

		protected bool initiated;

		protected Vector3 rootPosition;

		protected Quaternion rootRotation = Quaternion.identity;

		protected int index = -1;

		protected int LOD;

		public float sqrMag { get; private set; }

		public float mag { get; private set; }

		protected abstract void OnRead(Vector3[] positions, Quaternion[] rotations, bool hasChest, bool hasNeck, bool hasShoulders, bool hasToes, bool hasLegs, int rootIndex, int index);

		public abstract void PreSolve(float scale);

		public abstract void Write(ref Vector3[] solvedPositions, ref Quaternion[] solvedRotations);

		public abstract void ApplyOffsets(float scale);

		public abstract void ResetOffsets();

		public void SetLOD(int LOD)
		{
			this.LOD = LOD;
		}

		public void Read(Vector3[] positions, Quaternion[] rotations, bool hasChest, bool hasNeck, bool hasShoulders, bool hasToes, bool hasLegs, int rootIndex, int index)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			this.index = index;
			rootPosition = positions[rootIndex];
			rootRotation = rotations[rootIndex];
			OnRead(positions, rotations, hasChest, hasNeck, hasShoulders, hasToes, hasLegs, rootIndex, index);
			mag = VirtualBone.PreSolve(ref bones);
			sqrMag = mag * mag;
			initiated = true;
		}

		public void MovePosition(Vector3 position)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = position - bones[0].solverPosition;
			VirtualBone[] array = bones;
			foreach (VirtualBone obj in array)
			{
				obj.solverPosition += val;
			}
		}

		public void MoveRotation(Quaternion rotation)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			Quaternion rotation2 = QuaTools.FromToRotation(bones[0].solverRotation, rotation);
			VirtualBone.RotateAroundPoint(bones, 0, bones[0].solverPosition, rotation2);
		}

		public void Translate(Vector3 position, Quaternion rotation)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			MovePosition(position);
			MoveRotation(rotation);
		}

		public void TranslateRoot(Vector3 newRootPos, Quaternion newRootRot)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = newRootPos - rootPosition;
			rootPosition = newRootPos;
			VirtualBone[] array = bones;
			foreach (VirtualBone obj in array)
			{
				obj.solverPosition += val;
			}
			Quaternion rotation = QuaTools.FromToRotation(rootRotation, newRootRot);
			rootRotation = newRootRot;
			VirtualBone.RotateAroundPoint(bones, 0, newRootPos, rotation);
		}

		public void RotateTo(VirtualBone bone, Quaternion rotation, float weight = 1f)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			if (weight <= 0f)
			{
				return;
			}
			Quaternion val = QuaTools.FromToRotation(bone.solverRotation, rotation);
			if (weight < 1f)
			{
				val = Quaternion.Slerp(Quaternion.identity, val, weight);
			}
			for (int i = 0; i < bones.Length; i++)
			{
				if (bones[i] == bone)
				{
					VirtualBone.RotateAroundPoint(bones, i, bones[i].solverPosition, val);
					break;
				}
			}
		}

		public void Visualize(Color color)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < bones.Length - 1; i++)
			{
				Debug.DrawLine(bones[i].solverPosition, bones[i + 1].solverPosition, color);
			}
		}

		public void Visualize()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			Visualize(Color.white);
		}

		protected BodyPart()
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public class Footstep
	{
		public float stepSpeed = 3f;

		public Vector3 characterSpaceOffset;

		public Vector3 position;

		public Quaternion rotation = Quaternion.identity;

		public Quaternion stepToRootRot = Quaternion.identity;

		public bool isSupportLeg;

		public bool relaxFlag;

		public Vector3 stepFrom;

		public Vector3 stepTo;

		public Quaternion stepFromRot = Quaternion.identity;

		public Quaternion stepToRot = Quaternion.identity;

		private Quaternion footRelativeToRoot = Quaternion.identity;

		private float supportLegW;

		private float supportLegWV;

		public bool isStepping => stepProgress < 1f;

		public float stepProgress { get; private set; }

		public Footstep(Quaternion rootRotation, Vector3 footPosition, Quaternion footRotation, Vector3 characterSpaceOffset)
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
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			this.characterSpaceOffset = characterSpaceOffset;
			Reset(rootRotation, footPosition, footRotation);
			footRelativeToRoot = Quaternion.Inverse(rootRotation) * rotation;
		}

		public void Reset(Quaternion rootRotation, Vector3 footPosition, Quaternion footRotation)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			position = footPosition;
			rotation = footRotation;
			stepFrom = position;
			stepTo = position;
			stepFromRot = rotation;
			stepToRot = rotation;
			stepToRootRot = rootRotation;
			stepProgress = 1f;
		}

		public void StepTo(Vector3 p, Quaternion rootRotation, float stepThreshold)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (relaxFlag)
			{
				stepThreshold = 0f;
				relaxFlag = false;
			}
			if (!(Vector3.Magnitude(p - stepTo) < stepThreshold) || !(Quaternion.Angle(rootRotation, stepToRootRot) < 25f))
			{
				stepFrom = position;
				stepTo = p;
				stepFromRot = rotation;
				stepToRootRot = rootRotation;
				stepToRot = rootRotation * footRelativeToRoot;
				stepProgress = 0f;
			}
		}

		public void UpdateStepping(Vector3 p, Quaternion rootRotation, float speed, float deltaTime)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			stepTo = Vector3.Lerp(stepTo, p, deltaTime * speed);
			stepToRot = Quaternion.Lerp(stepToRot, rootRotation * footRelativeToRoot, deltaTime * speed);
			stepToRootRot = stepToRot * Quaternion.Inverse(footRelativeToRoot);
		}

		public void UpdateStanding(Quaternion rootRotation, float minAngle, float speed, float deltaTime)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			if (!(speed <= 0f) && !(minAngle >= 180f))
			{
				Quaternion val = rootRotation * footRelativeToRoot;
				float num = Quaternion.Angle(rotation, val);
				if (num > minAngle)
				{
					rotation = Quaternion.RotateTowards(rotation, val, Mathf.Min(deltaTime * speed * (1f - supportLegW), num - minAngle));
				}
			}
		}

		public void Update(InterpolationMode interpolation, UnityEvent onStep, float deltaTime)
		{
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			float num = (isSupportLeg ? 1f : 0f);
			supportLegW = Mathf.SmoothDamp(supportLegW, num, ref supportLegWV, 0.2f);
			if (isStepping)
			{
				stepProgress = Mathf.MoveTowards(stepProgress, 1f, deltaTime * stepSpeed);
				if (stepProgress >= 1f)
				{
					onStep.Invoke();
				}
				float num2 = Interp.Float(stepProgress, interpolation);
				position = Vector3.Lerp(stepFrom, stepTo, num2);
				rotation = Quaternion.Lerp(stepFromRot, stepToRot, num2);
			}
		}
	}

	[Serializable]
	public class Leg : BodyPart
	{
		[LargeHeader("Foot/Toe")]
		[Tooltip("The foot/toe target. This should not be the foot tracker itself, but a child GameObject parented to it so you could adjust its position/rotation to match the orientation of the foot/toe bone. If a toe bone is assigned in the References, the solver will match the toe bone to this target. If no toe bone assigned, foot bone will be used instead.")]
		public Transform target;

		[Tooltip("Positional weight of the toe/foot target. Note that if you have nulled the target, the foot will still be pulled to the last position of the target until you set this value to 0.")]
		[Range(0f, 1f)]
		public float positionWeight;

		[Tooltip("Rotational weight of the toe/foot target. Note that if you have nulled the target, the foot will still be rotated to the last rotation of the target until you set this value to 0.")]
		[Range(0f, 1f)]
		public float rotationWeight;

		[LargeHeader("Bending")]
		[Tooltip("The knee will be bent towards this Transform if 'Bend Goal Weight' > 0.")]
		public Transform bendGoal;

		[Tooltip("If greater than 0, will bend the knee towards the 'Bend Goal' Transform.")]
		[Range(0f, 1f)]
		public float bendGoalWeight;

		[Tooltip("Angular offset of knee bending direction.")]
		[Range(-180f, 180f)]
		public float swivelOffset;

		[Tooltip("If 0, the bend plane will be locked to the rotation of the pelvis and rotating the foot will have no effect on the knee direction. If 1, to the target rotation of the leg so that the knee will bend towards the forward axis of the foot. Values in between will be slerped between the two.")]
		[Range(0f, 1f)]
		public float bendToTargetWeight = 0.5f;

		[LargeHeader("Stretching")]
		[Tooltip("Use this to make the leg shorter/longer. Works by displacement of foot and calf localPosition.")]
		[Range(0.01f, 2f)]
		public float legLengthMlp = 1f;

		[Tooltip("Evaluates stretching of the leg by target distance relative to leg length. Value at time 1 represents stretching amount at the point where distance to the target is equal to leg length. Value at time 1 represents stretching amount at the point where distance to the target is double the leg length. Value represents the amount of stretching. Linear stretching would be achieved with a linear curve going up by 45 degrees. Increase the range of stretching by moving the last key up and right at the same amount. Smoothing in the curve can help reduce knee snapping (start stretching the arm slightly before target distance reaches leg length). To get a good optimal value for this curve, please go to the 'VRIK (Basic)' demo scene and copy the stretch curve over from the Pilot character.")]
		public AnimationCurve stretchCurve = new AnimationCurve();

		[NonSerialized]
		[HideInInspector]
		public Vector3 IKPosition;

		[NonSerialized]
		[HideInInspector]
		public Quaternion IKRotation = Quaternion.identity;

		[NonSerialized]
		[HideInInspector]
		public Vector3 footPositionOffset;

		[NonSerialized]
		[HideInInspector]
		public Vector3 heelPositionOffset;

		[NonSerialized]
		[HideInInspector]
		public Quaternion footRotationOffset = Quaternion.identity;

		[NonSerialized]
		[HideInInspector]
		public float currentMag;

		[HideInInspector]
		public bool useAnimatedBendNormal;

		private Vector3 footPosition;

		private Quaternion footRotation = Quaternion.identity;

		private Vector3 bendNormal;

		private Quaternion calfRelToThigh = Quaternion.identity;

		private Quaternion thighRelToFoot = Quaternion.identity;

		public Vector3 position
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return field;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				field = value;
			}
		}

		public Quaternion rotation
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return field;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				field = value;
			}
		}

		public bool hasToes { get; private set; }

		public VirtualBone thigh => bones[0];

		private VirtualBone calf => bones[1];

		private VirtualBone foot => bones[2];

		private VirtualBone toes => bones[3];

		public VirtualBone lastBone => bones[bones.Length - 1];

		public Vector3 thighRelativeToPelvis
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return field;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				field = value;
			}
		}

		public Vector3 bendNormalRelToPelvis
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return field;
			}
			[CompilerGenerated]
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				field = value;
			}
		}

		public Vector3 bendNormalRelToTarget
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return field;
			}
			[CompilerGenerated]
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				field = value;
			}
		}

		protected override void OnRead(Vector3[] positions, Quaternion[] rotations, bool hasChest, bool hasNeck, bool hasShoulders, bool hasToes, bool hasLegs, int rootIndex, int index)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01de: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = positions[index];
			Quaternion val2 = rotations[index];
			Vector3 val3 = positions[index + 1];
			Quaternion val4 = rotations[index + 1];
			Vector3 val5 = positions[index + 2];
			Quaternion iKRotation = rotations[index + 2];
			Vector3 iKPosition = positions[index + 3];
			Quaternion iKRotation2 = rotations[index + 3];
			if (!initiated)
			{
				this.hasToes = hasToes;
				bones = new VirtualBone[hasToes ? 4 : 3];
				if (hasToes)
				{
					bones[0] = new VirtualBone(val, val2);
					bones[1] = new VirtualBone(val3, val4);
					bones[2] = new VirtualBone(val5, iKRotation);
					bones[3] = new VirtualBone(iKPosition, iKRotation2);
					IKPosition = iKPosition;
					IKRotation = iKRotation2;
				}
				else
				{
					bones[0] = new VirtualBone(val, val2);
					bones[1] = new VirtualBone(val3, val4);
					bones[2] = new VirtualBone(val5, iKRotation);
					IKPosition = val5;
					IKRotation = iKRotation;
				}
				bendNormal = Vector3.Cross(val3 - val, val5 - val3);
				bendNormalRelToPelvis = Quaternion.Inverse(rootRotation) * bendNormal;
				bendNormalRelToTarget = Quaternion.Inverse(IKRotation) * bendNormal;
				rotation = IKRotation;
			}
			if (hasToes)
			{
				bones[0].Read(val, val2);
				bones[1].Read(val3, val4);
				bones[2].Read(val5, iKRotation);
				bones[3].Read(iKPosition, iKRotation2);
			}
			else
			{
				bones[0].Read(val, val2);
				bones[1].Read(val3, val4);
				bones[2].Read(val5, iKRotation);
			}
		}

		public override void PreSolve(float scale)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0211: Unknown result type (might be due to invalid IL or missing references)
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01df: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_0205: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)target != (Object)null)
			{
				IKPosition = target.position;
				IKRotation = target.rotation;
			}
			footPosition = foot.solverPosition;
			footRotation = foot.solverRotation;
			position = lastBone.solverPosition;
			rotation = lastBone.solverRotation;
			if (rotationWeight > 0f)
			{
				ApplyRotationOffset(QuaTools.FromToRotation(rotation, IKRotation), rotationWeight);
			}
			if (positionWeight > 0f)
			{
				ApplyPositionOffset(IKPosition - position, positionWeight);
			}
			thighRelativeToPelvis = Quaternion.Inverse(rootRotation) * (thigh.solverPosition - rootPosition);
			calfRelToThigh = Quaternion.Inverse(thigh.solverRotation) * calf.solverRotation;
			thighRelToFoot = Quaternion.Inverse(lastBone.solverRotation) * thigh.solverRotation;
			if (useAnimatedBendNormal)
			{
				bendNormal = Vector3.Cross(calf.solverPosition - thigh.solverPosition, foot.solverPosition - calf.solverPosition);
			}
			else if (bendToTargetWeight <= 0f)
			{
				bendNormal = rootRotation * bendNormalRelToPelvis;
			}
			else if (bendToTargetWeight >= 1f)
			{
				bendNormal = rotation * bendNormalRelToTarget;
			}
			else
			{
				bendNormal = Vector3.Slerp(rootRotation * bendNormalRelToPelvis, rotation * bendNormalRelToTarget, bendToTargetWeight);
			}
			bendNormal = bendNormal.normalized;
		}

		public override void ApplyOffsets(float scale)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			ApplyPositionOffset(footPositionOffset, 1f);
			ApplyRotationOffset(footRotationOffset, 1f);
			Quaternion val = Quaternion.FromToRotation(footPosition - position, footPosition + heelPositionOffset - position);
			footPosition = position + val * (footPosition - position);
			footRotation = val * footRotation;
			float num = 0f;
			if ((Object)(object)bendGoal != (Object)null && bendGoalWeight > 0f)
			{
				Vector3 val2 = Vector3.Cross(bendGoal.position - thigh.solverPosition, position - thigh.solverPosition);
				Vector3 val3 = Quaternion.Inverse(Quaternion.LookRotation(bendNormal, thigh.solverPosition - foot.solverPosition)) * val2;
				num = Mathf.Atan2(val3.x, val3.z) * 57.29578f * bendGoalWeight;
			}
			float num2 = swivelOffset + num;
			if (num2 != 0f)
			{
				bendNormal = Quaternion.AngleAxis(num2, thigh.solverPosition - lastBone.solverPosition) * bendNormal;
				thigh.solverRotation = Quaternion.AngleAxis(0f - num2, thigh.solverRotation * thigh.axis) * thigh.solverRotation;
			}
		}

		private void ApplyPositionOffset(Vector3 offset, float weight)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (!(weight <= 0f))
			{
				offset *= weight;
				footPosition += offset;
				position += offset;
			}
		}

		private void ApplyRotationOffset(Quaternion offset, float weight)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			if (!(weight <= 0f))
			{
				if (weight < 1f)
				{
					offset = Quaternion.Lerp(Quaternion.identity, offset, weight);
				}
				footRotation = offset * footRotation;
				rotation = offset * rotation;
				bendNormal = offset * bendNormal;
				footPosition = position + offset * (footPosition - position);
			}
		}

		public void Solve(bool stretch)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			if (stretch && LOD < 1)
			{
				Stretching();
			}
			VirtualBone.SolveTrigonometric(bones, 0, 1, 2, footPosition, bendNormal, 1f);
			RotateTo(foot, footRotation);
			if (!hasToes)
			{
				FixTwistRotations();
				return;
			}
			Vector3 val = Vector3.Cross(foot.solverPosition - thigh.solverPosition, toes.solverPosition - foot.solverPosition);
			Vector3 normalized = val.normalized;
			VirtualBone.SolveTrigonometric(bones, 0, 2, 3, position, normalized, 1f);
			FixTwistRotations();
			toes.solverRotation = rotation;
		}

		private void FixTwistRotations()
		{
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			if (LOD >= 1)
			{
				return;
			}
			if (bendToTargetWeight > 0f)
			{
				Quaternion val = rotation * thighRelToFoot;
				Quaternion val2 = Quaternion.FromToRotation(val * thigh.axis, calf.solverPosition - thigh.solverPosition);
				if (bendToTargetWeight < 1f)
				{
					thigh.solverRotation = Quaternion.Slerp(thigh.solverRotation, val2 * val, bendToTargetWeight);
				}
				else
				{
					thigh.solverRotation = val2 * val;
				}
			}
			Quaternion val3 = thigh.solverRotation * calfRelToThigh;
			Quaternion val4 = Quaternion.FromToRotation(val3 * calf.axis, foot.solverPosition - calf.solverPosition);
			calf.solverRotation = val4 * val3;
		}

		private void Stretching()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_0169: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			float num = thigh.length + calf.length;
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			if (legLengthMlp != 1f)
			{
				num *= legLengthMlp;
				zero = (calf.solverPosition - thigh.solverPosition) * (legLengthMlp - 1f);
				zero2 = (foot.solverPosition - calf.solverPosition) * (legLengthMlp - 1f);
				VirtualBone virtualBone = calf;
				virtualBone.solverPosition += zero;
				VirtualBone virtualBone2 = foot;
				virtualBone2.solverPosition += zero + zero2;
				if (hasToes)
				{
					VirtualBone virtualBone3 = toes;
					virtualBone3.solverPosition += zero + zero2;
				}
			}
			float num2 = Vector3.Distance(thigh.solverPosition, footPosition) / num;
			float num3 = stretchCurve.Evaluate(num2);
			zero = (calf.solverPosition - thigh.solverPosition) * num3;
			zero2 = (foot.solverPosition - calf.solverPosition) * num3;
			VirtualBone virtualBone4 = calf;
			virtualBone4.solverPosition += zero;
			VirtualBone virtualBone5 = foot;
			virtualBone5.solverPosition += zero + zero2;
			if (hasToes)
			{
				VirtualBone virtualBone6 = toes;
				virtualBone6.solverPosition += zero + zero2;
			}
		}

		public override void Write(ref Vector3[] solvedPositions, ref Quaternion[] solvedRotations)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			solvedRotations[index] = thigh.solverRotation;
			solvedRotations[index + 1] = calf.solverRotation;
			solvedRotations[index + 2] = foot.solverRotation;
			solvedPositions[index] = thigh.solverPosition;
			solvedPositions[index + 1] = calf.solverPosition;
			solvedPositions[index + 2] = foot.solverPosition;
			if (hasToes)
			{
				solvedRotations[index + 3] = toes.solverRotation;
				solvedPositions[index + 3] = toes.solverPosition;
			}
		}

		public override void ResetOffsets()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			footPositionOffset = Vector3.zero;
			footRotationOffset = Quaternion.identity;
			heelPositionOffset = Vector3.zero;
		}

		public Leg()
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected Obj, but got Unknown
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public class Locomotion
	{
		[Serializable]
		public enum Mode
		{
			Procedural,
			Animated
		}

		[Tooltip("Procedural (legacy) or animated locomotion.")]
		public Mode mode;

		[Tooltip("Used for blending in/out of procedural/animated locomotion.")]
		[Range(0f, 1f)]
		public float weight = 1f;

		[Tooltip("Start moving (horizontal distance to HMD + HMD velocity) threshold.")]
		[ShowIf("mode", Mode.Animated, null, false, ShowIfMode.Hidden)]
		public float moveThreshold = 0.3f;

		[ShowLargeHeaderIf("Animation", "mode", Mode.Animated, null, false, ShowIfMode.Hidden)]
		[SerializeField]
		private byte animationHeader;

		[Tooltip("Minimum locomotion animation speed.")]
		[ShowRangeIf(0.1f, 1f, "mode", Mode.Animated, null, false, ShowIfMode.Hidden)]
		public float minAnimationSpeed = 0.2f;

		[Tooltip("Maximum locomotion animation speed.")]
		[ShowRangeIf(1f, 10f, "mode", Mode.Animated, null, false, ShowIfMode.Hidden)]
		public float maxAnimationSpeed = 3f;

		[Tooltip("Smoothing time for Vector3.SmoothDamping 'VRIK_Horizontal' and 'VRIK_Vertical' parameters. Larger values make animation smoother, but less responsive.")]
		[ShowRangeIf(0.05f, 0.2f, "mode", Mode.Animated, null, false, ShowIfMode.Hidden)]
		public float animationSmoothTime = 0.1f;

		[ShowLargeHeaderIf("Root Position", "mode", Mode.Animated, null, false, ShowIfMode.Hidden)]
		[SerializeField]
		private byte rootPositionHeader;

		[Tooltip("X and Z standing offset from the horizontal position of the HMD.")]
		[ShowIf("mode", Mode.Animated, null, false, ShowIfMode.Hidden)]
		public Vector2 standOffset;

		[Tooltip("Lerp root towards the horizontal position of the HMD with this speed while moving.")]
		[ShowRangeIf(0f, 50f, "mode", Mode.Animated, null, false, ShowIfMode.Hidden)]
		public float rootLerpSpeedWhileMoving = 30f;

		[Tooltip("Lerp root towards the horizontal position of the HMD with this speed while in transition from locomotion to idle state.")]
		[ShowRangeIf(0f, 50f, "mode", Mode.Animated, null, false, ShowIfMode.Hidden)]
		public float rootLerpSpeedWhileStopping = 10f;

		[Tooltip("Lerp root towards the horizontal position of the HMD with this speed while turning on spot.")]
		[ShowRangeIf(0f, 50f, "mode", Mode.Animated, null, false, ShowIfMode.Hidden)]
		public float rootLerpSpeedWhileTurning = 10f;

		[Tooltip("Max horizontal distance from the root to the HMD.")]
		[ShowIf("mode", Mode.Animated, null, false, ShowIfMode.Hidden)]
		public float maxRootOffset = 0.5f;

		[ShowLargeHeaderIf("Root Rotation", "mode", Mode.Animated, null, false, ShowIfMode.Hidden)]
		[SerializeField]
		private byte rootRotationHeader;

		[Tooltip("Max root angle from head forward while moving (ik.solver.spine.maxRootAngle).")]
		[ShowRangeIf(0f, 180f, "mode", Mode.Animated, null, false, ShowIfMode.Hidden)]
		public float maxRootAngleMoving = 10f;

		[Tooltip("Max root angle from head forward while standing (ik.solver.spine.maxRootAngle.")]
		[ShowRangeIf(0f, 180f, "mode", Mode.Animated, null, false, ShowIfMode.Hidden)]
		public float maxRootAngleStanding = 90f;

		[HideInInspector]
		[SerializeField]
		public float stepLengthMlp = 1f;

		private Animator animator;

		private Vector3 velocityLocal;

		private Vector3 velocityLocalV;

		private Vector3 lastCorrection;

		private Vector3 lastHeadTargetPos;

		private Vector3 lastSpeedRootPos;

		private Vector3 lastEndRootPos;

		private float rootLerpSpeed;

		private float rootVelocityV;

		private float animSpeed = 1f;

		private float animSpeedV;

		private float stopMoveTimer;

		private float turn;

		private float maxRootAngleV;

		private float currentAnimationSmoothTime = 0.05f;

		private bool isMoving;

		private bool firstFrame = true;

		private static int VRIK_Horizontal;

		private static int VRIK_Vertical;

		private static int VRIK_IsMoving;

		private static int VRIK_Speed;

		private static int VRIK_Turn;

		private static bool isHashed;

		private float lastVelLocalMag;

		[Tooltip("Tries to maintain this distance between the legs.")]
		[ShowIf("mode", Mode.Procedural, null, false, ShowIfMode.Hidden)]
		public float footDistance = 0.3f;

		[Tooltip("Makes a step only if step target position is at least this far from the current footstep or the foot does not reach the current footstep anymore or footstep angle is past the 'Angle Threshold'.")]
		[ShowIf("mode", Mode.Procedural, null, false, ShowIfMode.Hidden)]
		public float stepThreshold = 0.4f;

		[Tooltip("Makes a step only if step target position is at least 'Step Threshold' far from the current footstep or the foot does not reach the current footstep anymore or footstep angle is past this value.")]
		[ShowIf("mode", Mode.Procedural, null, false, ShowIfMode.Hidden)]
		public float angleThreshold = 60f;

		[Tooltip("Multiplies angle of the center of mass - center of pressure vector. Larger value makes the character step sooner if losing balance.")]
		[ShowIf("mode", Mode.Procedural, null, false, ShowIfMode.Hidden)]
		public float comAngleMlp = 1f;

		[Tooltip("Maximum magnitude of head/hand target velocity used in prediction.")]
		[ShowIf("mode", Mode.Procedural, null, false, ShowIfMode.Hidden)]
		public float maxVelocity = 0.4f;

		[Tooltip("The amount of head/hand target velocity prediction.")]
		[ShowIf("mode", Mode.Procedural, null, false, ShowIfMode.Hidden)]
		public float velocityFactor = 0.4f;

		[Tooltip("How much can a leg be extended before it is forced to step to another position? 1 means fully stretched.")]
		[ShowRangeIf(0.9f, 1f, "mode", Mode.Procedural, null, false, ShowIfMode.Hidden)]
		public float maxLegStretch = 1f;

		[Tooltip("The speed of lerping the root of the character towards the horizontal mid-point of the footsteps.")]
		[ShowIf("mode", Mode.Procedural, null, false, ShowIfMode.Hidden)]
		public float rootSpeed = 20f;

		[Tooltip("The speed of moving a foot to the next position.")]
		[ShowIf("mode", Mode.Procedural, null, false, ShowIfMode.Hidden)]
		public float stepSpeed = 3f;

		[Tooltip("The height of the foot by normalized step progress (0 - 1).")]
		[ShowIf("mode", Mode.Procedural, null, false, ShowIfMode.Hidden)]
		public AnimationCurve stepHeight;

		[Tooltip("Reduce this value if locomotion makes the head bob too much.")]
		[ShowIf("mode", Mode.Procedural, null, false, ShowIfMode.Hidden)]
		public float maxBodyYOffset = 0.05f;

		[Tooltip("The height offset of the heel by normalized step progress (0 - 1).")]
		[ShowIf("mode", Mode.Procedural, null, false, ShowIfMode.Hidden)]
		public AnimationCurve heelHeight;

		[Tooltip("Rotates the foot while the leg is not stepping to relax the twist rotation of the leg if ideal rotation is past this angle.")]
		[ShowRangeIf(0f, 180f, "mode", Mode.Procedural, null, false, ShowIfMode.Hidden)]
		public float relaxLegTwistMinAngle = 20f;

		[Tooltip("The speed of rotating the foot while the leg is not stepping to relax the twist rotation of the leg.")]
		[ShowIf("mode", Mode.Procedural, null, false, ShowIfMode.Hidden)]
		public float relaxLegTwistSpeed = 400f;

		[Tooltip("Interpolation mode of the step.")]
		[ShowIf("mode", Mode.Procedural, null, false, ShowIfMode.Hidden)]
		public InterpolationMode stepInterpolation = InterpolationMode.InOutSine;

		[Tooltip("Offset for the approximated center of mass.")]
		[ShowIf("mode", Mode.Procedural, null, false, ShowIfMode.Hidden)]
		public Vector3 offset;

		[HideInInspector]
		public bool blockingEnabled;

		[HideInInspector]
		public LayerMask blockingLayers;

		[HideInInspector]
		public float raycastRadius = 0.2f;

		[HideInInspector]
		public float raycastHeight = 0.2f;

		[HideInInspector]
		[SerializeField]
		public UnityEvent onLeftFootstep = new UnityEvent();

		[HideInInspector]
		[SerializeField]
		public UnityEvent onRightFootstep = new UnityEvent();

		private Footstep[] footsteps = new Footstep[0];

		private Vector3 lastComPosition;

		private Vector3 comVelocity;

		private int leftFootIndex;

		private int rightFootIndex;

		public Vector3 centerOfMass
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return field;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				field = value;
			}
		}

		public Vector3 leftFootstepPosition
		{
			get
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				return footsteps[0].position;
			}
		}

		public Vector3 rightFootstepPosition
		{
			get
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				return footsteps[1].position;
			}
		}

		public Quaternion leftFootstepRotation
		{
			get
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				return footsteps[0].rotation;
			}
		}

		public Quaternion rightFootstepRotation
		{
			get
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				return footsteps[1].rotation;
			}
		}

		public void Initiate(Animator animator, Vector3[] positions, Quaternion[] rotations, bool hasToes, float scale)
		{
			Initiate_Procedural(positions, rotations, hasToes, scale);
			Initiate_Animated(animator, positions);
		}

		public void Reset(Vector3[] positions, Quaternion[] rotations)
		{
			Reset_Procedural(positions, rotations);
			Reset_Animated(positions);
		}

		public void Relax()
		{
			Relax_Procedural();
		}

		public void AddDeltaRotation(Quaternion delta, Vector3 pivot)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			AddDeltaRotation_Procedural(delta, pivot);
			AddDeltaRotation_Animated(delta, pivot);
		}

		public void AddDeltaPosition(Vector3 delta)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			AddDeltaPosition_Procedural(delta);
			AddDeltaPosition_Animated(delta);
		}

		public void Initiate_Animated(Animator animator, Vector3[] positions)
		{
			this.animator = animator;
			if ((Object)(object)animator == (Object)null && mode == Mode.Animated)
			{
				Debug.LogError((object)"VRIK is in Animated locomotion mode, but cannot find Animator on the VRIK root gameobject.");
			}
			ResetParams(positions);
		}

		private void ResetParams(Vector3[] positions)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			lastHeadTargetPos = positions[5];
			lastSpeedRootPos = positions[0];
			lastEndRootPos = lastSpeedRootPos;
			lastCorrection = Vector3.zero;
			isMoving = false;
			currentAnimationSmoothTime = 0.05f;
			stopMoveTimer = 1f;
		}

		public void Reset_Animated(Vector3[] positions)
		{
			ResetParams(positions);
			if (!((Object)(object)animator == (Object)null))
			{
				if (!isHashed)
				{
					VRIK_Horizontal = Animator.StringToHash("VRIK_Horizontal");
					VRIK_Vertical = Animator.StringToHash("VRIK_Vertical");
					VRIK_IsMoving = Animator.StringToHash("VRIK_IsMoving");
					VRIK_Speed = Animator.StringToHash("VRIK_Speed");
					VRIK_Turn = Animator.StringToHash("VRIK_Turn");
					isHashed = true;
				}
				if (!firstFrame)
				{
					animator.SetFloat(VRIK_Horizontal, 0f);
					animator.SetFloat(VRIK_Vertical, 0f);
					animator.SetBool(VRIK_IsMoving, false);
					animator.SetFloat(VRIK_Speed, 1f);
					animator.SetFloat(VRIK_Turn, 0f);
				}
			}
		}

		private void AddDeltaRotation_Animated(Quaternion delta, Vector3 pivot)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = lastEndRootPos - pivot;
			lastEndRootPos = pivot + delta * val;
			Vector3 val2 = lastSpeedRootPos - pivot;
			lastSpeedRootPos = pivot + delta * val2;
			Vector3 val3 = lastHeadTargetPos - pivot;
			lastHeadTargetPos = pivot + delta * val3;
		}

		private void AddDeltaPosition_Animated(Vector3 delta)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			lastEndRootPos += delta;
			lastSpeedRootPos += delta;
			lastHeadTargetPos += delta;
		}

		public void Solve_Animated(IKSolverVR solver, float scale, float deltaTime)
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			//IL_021b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0220: Unknown result type (might be due to invalid IL or missing references)
			//IL_0222: Unknown result type (might be due to invalid IL or missing references)
			//IL_0224: Unknown result type (might be due to invalid IL or missing references)
			//IL_0229: Unknown result type (might be due to invalid IL or missing references)
			//IL_022e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0230: Unknown result type (might be due to invalid IL or missing references)
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0244: Unknown result type (might be due to invalid IL or missing references)
			//IL_027d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0282: Unknown result type (might be due to invalid IL or missing references)
			//IL_0296: Unknown result type (might be due to invalid IL or missing references)
			//IL_029b: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03be: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_048b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0490: Unknown result type (might be due to invalid IL or missing references)
			//IL_04fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0502: Unknown result type (might be due to invalid IL or missing references)
			//IL_0507: Unknown result type (might be due to invalid IL or missing references)
			//IL_0508: Unknown result type (might be due to invalid IL or missing references)
			//IL_050d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0513: Unknown result type (might be due to invalid IL or missing references)
			//IL_0518: Unknown result type (might be due to invalid IL or missing references)
			//IL_051d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0614: Unknown result type (might be due to invalid IL or missing references)
			//IL_061b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0620: Unknown result type (might be due to invalid IL or missing references)
			//IL_062b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0630: Unknown result type (might be due to invalid IL or missing references)
			//IL_063c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0642: Unknown result type (might be due to invalid IL or missing references)
			//IL_0647: Unknown result type (might be due to invalid IL or missing references)
			//IL_064c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0534: Unknown result type (might be due to invalid IL or missing references)
			//IL_0539: Unknown result type (might be due to invalid IL or missing references)
			//IL_0658: Unknown result type (might be due to invalid IL or missing references)
			//IL_065d: Unknown result type (might be due to invalid IL or missing references)
			//IL_057a: Unknown result type (might be due to invalid IL or missing references)
			//IL_057f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0581: Unknown result type (might be due to invalid IL or missing references)
			//IL_0586: Unknown result type (might be due to invalid IL or missing references)
			//IL_058b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0592: Unknown result type (might be due to invalid IL or missing references)
			//IL_0597: Unknown result type (might be due to invalid IL or missing references)
			//IL_059c: Unknown result type (might be due to invalid IL or missing references)
			//IL_059e: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0554: Unknown result type (might be due to invalid IL or missing references)
			//IL_0559: Unknown result type (might be due to invalid IL or missing references)
			//IL_0569: Unknown result type (might be due to invalid IL or missing references)
			//IL_056e: Unknown result type (might be due to invalid IL or missing references)
			//IL_05c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_05c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_05c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_05cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_05d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_05db: Unknown result type (might be due to invalid IL or missing references)
			//IL_05e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_05eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_05fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0602: Unknown result type (might be due to invalid IL or missing references)
			//IL_0607: Unknown result type (might be due to invalid IL or missing references)
			//IL_060c: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)animator == (Object)null)
			{
				Debug.LogError((object)"VRIK cannot find Animator on the VRIK root gameobject.", (Object)(object)solver.root);
			}
			else
			{
				if (deltaTime <= 0f)
				{
					return;
				}
				if (!((Behaviour)animator).enabled)
				{
					Debug.LogWarning((object)"Trying to use VRIK animated locomotion with a disabled animator!", (Object)(object)solver.root);
					return;
				}
				Vector3 val = solver.rootBone.solverRotation * Vector3.up;
				Vector3 val2 = solver.rootBone.solverPosition - lastEndRootPos;
				val2 -= animator.deltaPosition;
				Vector3 headPosition = solver.spine.headPosition;
				Vector3 val3 = solver.rootBone.solverRotation * new Vector3(standOffset.x, 0f, standOffset.y) * scale;
				headPosition += val3;
				if (firstFrame)
				{
					lastHeadTargetPos = headPosition;
					firstFrame = false;
				}
				Vector3 val4 = (headPosition - lastHeadTargetPos) / deltaTime;
				lastHeadTargetPos = headPosition;
				val4 = V3Tools.Flatten(val4, val);
				Vector3 val5 = headPosition - solver.rootBone.solverPosition;
				val5 -= val2;
				val5 -= lastCorrection;
				val5 = V3Tools.Flatten(val5, val);
				Vector3 val6 = solver.spine.IKRotationHead * solver.spine.anchorRelativeToHead * Vector3.forward;
				val6.y = 0f;
				Vector3 val7 = Quaternion.Inverse(solver.rootBone.solverRotation) * val6;
				float num = (Mathf.Atan2(val7.x, val7.z) * 57.29578f + solver.spine.rootHeadingOffset) / 90f;
				bool flag = true;
				if (Mathf.Abs(num) < 0.2f)
				{
					num = 0f;
					flag = false;
				}
				turn = Mathf.Lerp(turn, num, Time.deltaTime * 3f);
				animator.SetFloat(VRIK_Turn, turn * 2f);
				Vector3 val8 = Quaternion.Inverse(solver.readRotations[0]) * (val4 + val5);
				val8 *= weight * stepLengthMlp;
				float num2 = ((flag && !isMoving) ? 0.2f : animationSmoothTime);
				currentAnimationSmoothTime = Mathf.Lerp(currentAnimationSmoothTime, num2, deltaTime * 20f);
				velocityLocal = Vector3.SmoothDamp(velocityLocal, val8, ref velocityLocalV, currentAnimationSmoothTime, float.PositiveInfinity, deltaTime);
				float num3 = velocityLocal.magnitude / stepLengthMlp;
				animator.SetFloat(VRIK_Horizontal, velocityLocal.x / scale);
				animator.SetFloat(VRIK_Vertical, velocityLocal.z / scale);
				float num4 = moveThreshold * scale;
				if (isMoving)
				{
					num4 *= 0.9f;
				}
				bool flag2 = velocityLocal.sqrMagnitude > num4 * num4;
				if (flag2)
				{
					stopMoveTimer = 0f;
				}
				else
				{
					stopMoveTimer += deltaTime;
				}
				isMoving = stopMoveTimer < 0.05f;
				float num5 = (isMoving ? maxRootAngleMoving : maxRootAngleStanding);
				solver.spine.maxRootAngle = Mathf.SmoothDamp(solver.spine.maxRootAngle, num5, ref maxRootAngleV, 0.2f, float.PositiveInfinity, deltaTime);
				animator.SetBool(VRIK_IsMoving, isMoving);
				Vector3 val9 = (solver.rootBone.solverPosition - val2 - lastCorrection - lastSpeedRootPos) / deltaTime;
				lastSpeedRootPos = solver.rootBone.solverPosition;
				float magnitude = val9.magnitude;
				float num6 = minAnimationSpeed;
				if ((magnitude > 0f) & flag2)
				{
					num6 = animSpeed * (num3 / magnitude);
				}
				num6 = Mathf.Clamp(num6, minAnimationSpeed, maxAnimationSpeed);
				animSpeed = Mathf.SmoothDamp(animSpeed, num6, ref animSpeedV, 0.05f, float.PositiveInfinity, deltaTime);
				animSpeed = Mathf.Lerp(1f, animSpeed, weight);
				animator.SetFloat(VRIK_Speed, animSpeed);
				AnimatorTransitionInfo animatorTransitionInfo = animator.GetAnimatorTransitionInfo(0);
				bool flag3 = animatorTransitionInfo.IsUserName("VRIK_Stop");
				float num7 = 0f;
				if (isMoving)
				{
					num7 = rootLerpSpeedWhileMoving;
				}
				if (flag3)
				{
					num7 = rootLerpSpeedWhileStopping;
				}
				if (flag)
				{
					num7 = rootLerpSpeedWhileTurning;
				}
				num7 *= Mathf.Max(val4.magnitude, 0.2f);
				rootLerpSpeed = Mathf.Lerp(rootLerpSpeed, num7, deltaTime * 20f);
				headPosition += V3Tools.ExtractVertical(solver.rootBone.solverPosition - headPosition, val, 1f);
				if (maxRootOffset > 0f)
				{
					Vector3 solverPosition = solver.rootBone.solverPosition;
					if (rootLerpSpeed > 0f)
					{
						solver.rootBone.solverPosition = Vector3.Lerp(solver.rootBone.solverPosition, headPosition, rootLerpSpeed * deltaTime * weight);
					}
					lastCorrection = solver.rootBone.solverPosition - solverPosition;
					val5 = headPosition - solver.rootBone.solverPosition;
					val5 = V3Tools.Flatten(val5, val);
					float magnitude2 = val5.magnitude;
					if (magnitude2 > maxRootOffset)
					{
						lastCorrection += (val5 - val5 / magnitude2 * maxRootOffset) * weight;
						VirtualBone rootBone = solver.rootBone;
						rootBone.solverPosition += lastCorrection;
					}
				}
				else
				{
					lastCorrection = (headPosition - solver.rootBone.solverPosition) * weight;
					VirtualBone rootBone2 = solver.rootBone;
					rootBone2.solverPosition += lastCorrection;
				}
				lastEndRootPos = solver.rootBone.solverPosition;
			}
		}

		private void Initiate_Procedural(Vector3[] positions, Quaternion[] rotations, bool hasToes, float scale)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			leftFootIndex = (hasToes ? 17 : 16);
			rightFootIndex = (hasToes ? 21 : 20);
			footsteps = new Footstep[2]
			{
				new Footstep(rotations[0], positions[leftFootIndex], rotations[leftFootIndex], footDistance * scale * Vector3.left),
				new Footstep(rotations[0], positions[rightFootIndex], rotations[rightFootIndex], footDistance * scale * Vector3.right)
			};
		}

		private void Reset_Procedural(Vector3[] positions, Quaternion[] rotations)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			lastComPosition = Vector3.Lerp(positions[1], positions[5], 0.25f) + rotations[0] * offset;
			comVelocity = Vector3.zero;
			footsteps[0].Reset(rotations[0], positions[leftFootIndex], rotations[leftFootIndex]);
			footsteps[1].Reset(rotations[0], positions[rightFootIndex], rotations[rightFootIndex]);
		}

		private void Relax_Procedural()
		{
			footsteps[0].relaxFlag = true;
			footsteps[1].relaxFlag = true;
		}

		private void AddDeltaRotation_Procedural(Quaternion delta, Vector3 pivot)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = lastComPosition - pivot;
			lastComPosition = pivot + delta * val;
			Footstep[] array = footsteps;
			foreach (Footstep footstep in array)
			{
				footstep.rotation = delta * footstep.rotation;
				footstep.stepFromRot = delta * footstep.stepFromRot;
				footstep.stepToRot = delta * footstep.stepToRot;
				footstep.stepToRootRot = delta * footstep.stepToRootRot;
				Vector3 val2 = footstep.position - pivot;
				footstep.position = pivot + delta * val2;
				Vector3 val3 = footstep.stepFrom - pivot;
				footstep.stepFrom = pivot + delta * val3;
				Vector3 val4 = footstep.stepTo - pivot;
				footstep.stepTo = pivot + delta * val4;
			}
		}

		private void AddDeltaPosition_Procedural(Vector3 delta)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			lastComPosition += delta;
			Footstep[] array = footsteps;
			foreach (Footstep obj in array)
			{
				obj.position += delta;
				obj.stepFrom += delta;
				obj.stepTo += delta;
			}
		}

		public void Solve_Procedural(VirtualBone rootBone, Spine spine, Leg leftLeg, Leg rightLeg, Arm leftArm, Arm rightArm, int supportLegIndex, out Vector3 leftFootPosition, out Vector3 rightFootPosition, out Quaternion leftFootRotation, out Quaternion rightFootRotation, out float leftFootOffset, out float rightFootOffset, out float leftHeelOffset, out float rightHeelOffset, float scale, float deltaTime)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0208: Unknown result type (might be due to invalid IL or missing references)
			//IL_020f: Unknown result type (might be due to invalid IL or missing references)
			//IL_021b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0221: Unknown result type (might be due to invalid IL or missing references)
			//IL_0227: Unknown result type (might be due to invalid IL or missing references)
			//IL_022c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_024d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0253: Unknown result type (might be due to invalid IL or missing references)
			//IL_0258: Unknown result type (might be due to invalid IL or missing references)
			//IL_025f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0245: Unknown result type (might be due to invalid IL or missing references)
			//IL_0264: Unknown result type (might be due to invalid IL or missing references)
			//IL_026b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0270: Unknown result type (might be due to invalid IL or missing references)
			//IL_0277: Unknown result type (might be due to invalid IL or missing references)
			//IL_0282: Unknown result type (might be due to invalid IL or missing references)
			//IL_028d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0294: Unknown result type (might be due to invalid IL or missing references)
			//IL_0299: Unknown result type (might be due to invalid IL or missing references)
			//IL_029f: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02df: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0305: Unknown result type (might be due to invalid IL or missing references)
			//IL_030a: Unknown result type (might be due to invalid IL or missing references)
			//IL_030f: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0365: Unknown result type (might be due to invalid IL or missing references)
			//IL_0368: Unknown result type (might be due to invalid IL or missing references)
			//IL_0376: Unknown result type (might be due to invalid IL or missing references)
			//IL_037b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0380: Unknown result type (might be due to invalid IL or missing references)
			//IL_0385: Unknown result type (might be due to invalid IL or missing references)
			//IL_0391: Unknown result type (might be due to invalid IL or missing references)
			//IL_0396: Unknown result type (might be due to invalid IL or missing references)
			//IL_0399: Unknown result type (might be due to invalid IL or missing references)
			//IL_0712: Unknown result type (might be due to invalid IL or missing references)
			//IL_0717: Unknown result type (might be due to invalid IL or missing references)
			//IL_0726: Unknown result type (might be due to invalid IL or missing references)
			//IL_072b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0734: Unknown result type (might be due to invalid IL or missing references)
			//IL_073f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0744: Unknown result type (might be due to invalid IL or missing references)
			//IL_0745: Unknown result type (might be due to invalid IL or missing references)
			//IL_074a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0753: Unknown result type (might be due to invalid IL or missing references)
			//IL_075f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0764: Unknown result type (might be due to invalid IL or missing references)
			//IL_0765: Unknown result type (might be due to invalid IL or missing references)
			//IL_076a: Unknown result type (might be due to invalid IL or missing references)
			//IL_07f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_07f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0805: Unknown result type (might be due to invalid IL or missing references)
			//IL_080a: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0423: Unknown result type (might be due to invalid IL or missing references)
			//IL_0426: Unknown result type (might be due to invalid IL or missing references)
			//IL_0434: Unknown result type (might be due to invalid IL or missing references)
			//IL_0439: Unknown result type (might be due to invalid IL or missing references)
			//IL_043e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0443: Unknown result type (might be due to invalid IL or missing references)
			//IL_0670: Unknown result type (might be due to invalid IL or missing references)
			//IL_0673: Unknown result type (might be due to invalid IL or missing references)
			//IL_0681: Unknown result type (might be due to invalid IL or missing references)
			//IL_0686: Unknown result type (might be due to invalid IL or missing references)
			//IL_068b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0690: Unknown result type (might be due to invalid IL or missing references)
			//IL_06c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_06c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0461: Unknown result type (might be due to invalid IL or missing references)
			//IL_045e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0462: Unknown result type (might be due to invalid IL or missing references)
			//IL_046d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0472: Unknown result type (might be due to invalid IL or missing references)
			//IL_0487: Unknown result type (might be due to invalid IL or missing references)
			//IL_048a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0498: Unknown result type (might be due to invalid IL or missing references)
			//IL_049d: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0576: Unknown result type (might be due to invalid IL or missing references)
			//IL_0581: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04de: Unknown result type (might be due to invalid IL or missing references)
			//IL_0539: Unknown result type (might be due to invalid IL or missing references)
			//IL_053e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0549: Unknown result type (might be due to invalid IL or missing references)
			//IL_04fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0500: Unknown result type (might be due to invalid IL or missing references)
			//IL_0502: Unknown result type (might be due to invalid IL or missing references)
			//IL_0507: Unknown result type (might be due to invalid IL or missing references)
			//IL_0519: Unknown result type (might be due to invalid IL or missing references)
			//IL_051e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0520: Unknown result type (might be due to invalid IL or missing references)
			//IL_0525: Unknown result type (might be due to invalid IL or missing references)
			//IL_0616: Unknown result type (might be due to invalid IL or missing references)
			//IL_061b: Unknown result type (might be due to invalid IL or missing references)
			//IL_061e: Unknown result type (might be due to invalid IL or missing references)
			if (weight <= 0f || deltaTime <= 0f)
			{
				leftFootPosition = Vector3.zero;
				rightFootPosition = Vector3.zero;
				leftFootRotation = Quaternion.identity;
				rightFootRotation = Quaternion.identity;
				leftFootOffset = 0f;
				rightFootOffset = 0f;
				leftHeelOffset = 0f;
				rightHeelOffset = 0f;
				return;
			}
			Vector3 val = rootBone.solverRotation * Vector3.up;
			Vector3 val2 = spine.pelvis.solverPosition + spine.pelvis.solverRotation * leftLeg.thighRelativeToPelvis;
			Vector3 val3 = spine.pelvis.solverPosition + spine.pelvis.solverRotation * rightLeg.thighRelativeToPelvis;
			footsteps[0].characterSpaceOffset = footDistance * Vector3.left * scale;
			footsteps[1].characterSpaceOffset = footDistance * Vector3.right * scale;
			Vector3 faceDirection = spine.faceDirection;
			Vector3 val4 = V3Tools.ExtractVertical(faceDirection, val, 1f);
			Quaternion val5 = Quaternion.LookRotation(faceDirection - val4, val);
			if (spine.rootHeadingOffset != 0f)
			{
				val5 = Quaternion.AngleAxis(spine.rootHeadingOffset, val) * val5;
			}
			float num = 1f;
			float num2 = 1f;
			float num3 = 0.2f;
			float num4 = num + num2 + 2f * num3;
			centerOfMass = Vector3.zero;
			centerOfMass += spine.pelvis.solverPosition * num;
			centerOfMass += spine.head.solverPosition * num2;
			centerOfMass += leftArm.position * num3;
			centerOfMass += rightArm.position * num3;
			centerOfMass /= num4;
			centerOfMass += rootBone.solverRotation * offset;
			comVelocity = ((deltaTime > 0f) ? ((centerOfMass - lastComPosition) / deltaTime) : Vector3.zero);
			lastComPosition = centerOfMass;
			comVelocity = Vector3.ClampMagnitude(comVelocity, maxVelocity) * velocityFactor * scale;
			Vector3 val6 = centerOfMass + comVelocity;
			Vector3 val7 = V3Tools.PointToPlane(spine.pelvis.solverPosition, rootBone.solverPosition, val);
			Vector3 val8 = V3Tools.PointToPlane(val6, rootBone.solverPosition, val);
			Vector3 val9 = Vector3.Lerp(footsteps[0].position, footsteps[1].position, 0.5f);
			float num5 = Vector3.Angle(val6 - val9, rootBone.solverRotation * Vector3.up) * comAngleMlp;
			for (int i = 0; i < footsteps.Length; i++)
			{
				footsteps[i].isSupportLeg = supportLegIndex == i;
			}
			for (int j = 0; j < footsteps.Length; j++)
			{
				if (footsteps[j].isStepping)
				{
					Vector3 val10 = val8 + rootBone.solverRotation * footsteps[j].characterSpaceOffset;
					if (!StepBlocked(footsteps[j].stepFrom, val10, rootBone.solverPosition))
					{
						footsteps[j].UpdateStepping(val10, val5, 10f, deltaTime);
					}
				}
				else
				{
					footsteps[j].UpdateStanding(val5, relaxLegTwistMinAngle, relaxLegTwistSpeed, deltaTime);
				}
			}
			if (CanStep())
			{
				int num6 = -1;
				float num7 = float.NegativeInfinity;
				for (int k = 0; k < footsteps.Length; k++)
				{
					if (footsteps[k].isStepping)
					{
						continue;
					}
					Vector3 val11 = val8 + rootBone.solverRotation * footsteps[k].characterSpaceOffset;
					float num8 = ((k == 0) ? leftLeg.mag : rightLeg.mag);
					Vector3 val12 = ((k == 0) ? val2 : val3);
					float num9 = Vector3.Distance(footsteps[k].position, val12);
					bool flag = false;
					if (num9 >= num8 * maxLegStretch)
					{
						val11 = val7 + rootBone.solverRotation * footsteps[k].characterSpaceOffset;
						flag = true;
					}
					bool flag2 = false;
					for (int l = 0; l < footsteps.Length; l++)
					{
						if (l == k || flag)
						{
							continue;
						}
						if (Vector3.Distance(footsteps[k].position, footsteps[l].position) < 0.25f * scale)
						{
							Vector3 val13 = footsteps[k].position - val11;
							float sqrMagnitude = val13.sqrMagnitude;
							val13 = footsteps[l].position - val11;
							if (sqrMagnitude < val13.sqrMagnitude)
							{
								goto IL_055d;
							}
						}
						flag2 = GetLineSphereCollision(footsteps[k].position, val11, footsteps[l].position, 0.25f * scale);
						goto IL_055d;
						IL_055d:
						if (flag2)
						{
							break;
						}
					}
					float num10 = Quaternion.Angle(val5, footsteps[k].stepToRootRot);
					if (flag2 && !(num10 > angleThreshold))
					{
						continue;
					}
					float num11 = Vector3.Distance(footsteps[k].position, val11);
					float num12 = stepThreshold * scale;
					if (footsteps[k].relaxFlag)
					{
						num12 = 0f;
					}
					float num13 = Mathf.Lerp(num12, num12 * 0.1f, num5 * 0.015f);
					if (flag)
					{
						num13 *= 0.5f;
					}
					if (k == 0)
					{
						num13 *= 0.9f;
					}
					if (!StepBlocked(footsteps[k].position, val11, rootBone.solverPosition) && (num11 > num13 || num10 > angleThreshold))
					{
						float num14 = 0f;
						num14 -= num11;
						if (num14 > num7)
						{
							num6 = k;
							num7 = num14;
						}
					}
				}
				if (num6 != -1)
				{
					Vector3 p = val8 + rootBone.solverRotation * footsteps[num6].characterSpaceOffset;
					footsteps[num6].stepSpeed = Random.Range(stepSpeed, stepSpeed * 1.5f);
					footsteps[num6].StepTo(p, val5, stepThreshold * scale);
				}
			}
			footsteps[0].Update(stepInterpolation, onLeftFootstep, deltaTime);
			footsteps[1].Update(stepInterpolation, onRightFootstep, deltaTime);
			leftFootPosition = footsteps[0].position;
			rightFootPosition = footsteps[1].position;
			leftFootPosition = V3Tools.PointToPlane(leftFootPosition, leftLeg.lastBone.readPosition, val);
			rightFootPosition = V3Tools.PointToPlane(rightFootPosition, rightLeg.lastBone.readPosition, val);
			leftFootOffset = stepHeight.Evaluate(footsteps[0].stepProgress) * scale;
			rightFootOffset = stepHeight.Evaluate(footsteps[1].stepProgress) * scale;
			leftHeelOffset = heelHeight.Evaluate(footsteps[0].stepProgress) * scale;
			rightHeelOffset = heelHeight.Evaluate(footsteps[1].stepProgress) * scale;
			leftFootRotation = footsteps[0].rotation;
			rightFootRotation = footsteps[1].rotation;
		}

		private bool StepBlocked(Vector3 fromPosition, Vector3 toPosition, Vector3 rootPosition)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			if (LayerMask.op_Implicit(blockingLayers) == -1 || !blockingEnabled)
			{
				return false;
			}
			Vector3 val = fromPosition;
			val.y = rootPosition.y + raycastHeight + raycastRadius;
			Vector3 val2 = toPosition - val;
			val2.y = 0f;
			RaycastHit val3 = default;
			if (raycastRadius <= 0f)
			{
				return Physics.Raycast(val, val2, ref val3, val2.magnitude, LayerMask.op_Implicit(blockingLayers));
			}
			return Physics.SphereCast(val, raycastRadius, val2, ref val3, val2.magnitude, LayerMask.op_Implicit(blockingLayers));
		}

		private bool CanStep()
		{
			Footstep[] array = footsteps;
			foreach (Footstep footstep in array)
			{
				if (footstep.isStepping && footstep.stepProgress < 0.8f)
				{
					return false;
				}
			}
			return true;
		}

		private static bool GetLineSphereCollision(Vector3 lineStart, Vector3 lineEnd, Vector3 sphereCenter, float sphereRadius)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = lineEnd - lineStart;
			Vector3 val2 = sphereCenter - lineStart;
			float num = val2.magnitude - sphereRadius;
			if (num > val.magnitude)
			{
				return false;
			}
			Vector3 val3 = Quaternion.Inverse(Quaternion.LookRotation(val, val2)) * val2;
			if (val3.z < 0f)
			{
				return num < 0f;
			}
			return val3.y - sphereRadius < 0f;
		}

		public Locomotion()
		{
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Expected Obj, but got Unknown
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Expected Obj, but got Unknown
		}
	}

	[Serializable]
	public class Spine : BodyPart
	{
		[LargeHeader("Head")]
		[Tooltip("The head target. This should not be the camera Transform itself, but a child GameObject parented to it so you could adjust its position/rotation  to match the orientation of the head bone. The best practice for setup would be to move the camera to the avatar's eyes, duplicate the avatar's head bone and parent it to the camera. Then assign the duplicate to this slot.")]
		public Transform headTarget;

		[Tooltip("Positional weight of the head target. Note that if you have nulled the headTarget, the head will still be pulled to the last position of the headTarget until you set this value to 0.")]
		[Range(0f, 1f)]
		public float positionWeight = 1f;

		[Tooltip("Rotational weight of the head target. Note that if you have nulled the headTarget, the head will still be rotated to the last rotation of the headTarget until you set this value to 0.")]
		[Range(0f, 1f)]
		public float rotationWeight = 1f;

		[Tooltip("Clamps head rotation. Value of 0.5 allows 90 degrees of rotation for the head relative to the headTarget. Value of 0 allows 180 degrees and value of 1 means head rotation will be locked to the target.")]
		[Range(0f, 1f)]
		public float headClampWeight = 0.6f;

		[Tooltip("Minimum height of the head from the root of the character.")]
		public float minHeadHeight = 0.8f;

		[Tooltip("Allows for more natural locomotion animation for 3rd person networked avatars by inheriting vertical head bob motion from the animation while head target height is close to head bone height.")]
		[Range(0f, 1f)]
		public float useAnimatedHeadHeightWeight;

		[Tooltip("If abs(head target height - head bone height) < this value, will use head bone height as head target Y.")]
		[ShowIf("useAnimatedHeadHeightWeight", 0f, float.PositiveInfinity, false, ShowIfMode.Hidden)]
		public float useAnimatedHeadHeightRange = 0.1f;

		[Tooltip("Falloff range for the 'Use Animated Head Height Range' effect above. If head target height from head bone height is greater than useAnimatedHeadHeightRange + animatedHeadHeightBlend, then the head will be vertically locked to the head target again.")]
		[ShowIf("useAnimatedHeadHeightWeight", 0f, float.PositiveInfinity, false, ShowIfMode.Hidden)]
		public float animatedHeadHeightBlend = 0.3f;

		[LargeHeader("Pelvis")]
		[Tooltip("The pelvis target (optional), useful for seated rigs or if you had an additional tracker on the backpack or belt are. The best practice for setup would be to duplicate the avatar's pelvis bone and parenting it to the pelvis tracker. Then assign the duplicate to this slot.")]
		public Transform pelvisTarget;

		[Tooltip("Positional weight of the pelvis target. Note that if you have nulled the pelvisTarget, the pelvis will still be pulled to the last position of the pelvisTarget until you set this value to 0.")]
		[Range(0f, 1f)]
		public float pelvisPositionWeight;

		[Tooltip("Rotational weight of the pelvis target. Note that if you have nulled the pelvisTarget, the pelvis will still be rotated to the last rotation of the pelvisTarget until you set this value to 0.")]
		[Range(0f, 1f)]
		public float pelvisRotationWeight;

		[Tooltip("How much will the pelvis maintain its animated position?")]
		[Range(0f, 1f)]
		public float maintainPelvisPosition = 0.2f;

		[LargeHeader("Chest")]
		[Tooltip("If 'Chest Goal Weight' is greater than 0, the chest will be turned towards this Transform.")]
		public Transform chestGoal;

		[Tooltip("Weight of turning the chest towards the 'Chest Goal'.")]
		[Range(0f, 1f)]
		public float chestGoalWeight;

		[Tooltip("Clamps chest rotation. Value of 0.5 allows 90 degrees of rotation for the chest relative to the head. Value of 0 allows 180 degrees and value of 1 means the chest will be locked relative to the head.")]
		[Range(0f, 1f)]
		public float chestClampWeight = 0.5f;

		[Tooltip("The amount of rotation applied to the chest based on hand positions.")]
		[Range(0f, 1f)]
		public float rotateChestByHands = 1f;

		[LargeHeader("Spine")]
		[Tooltip("Determines how much the body will follow the position of the head.")]
		[Range(0f, 1f)]
		public float bodyPosStiffness = 0.55f;

		[Tooltip("Determines how much the body will follow the rotation of the head.")]
		[Range(0f, 1f)]
		public float bodyRotStiffness = 0.1f;

		[Tooltip("Determines how much the chest will rotate to the rotation of the head.")]
		[FormerlySerializedAs("chestRotationWeight")]
		[Range(0f, 1f)]
		public float neckStiffness = 0.2f;

		[Tooltip("Moves the body horizontally along -character.forward axis by that value when the player is crouching.")]
		public float moveBodyBackWhenCrouching = 0.5f;

		[LargeHeader("Root Rotation")]
		[Tooltip("Will automatically rotate the root of the character if the head target has turned past this angle.")]
		[Range(0f, 180f)]
		public float maxRootAngle = 25f;

		[Tooltip("Angular offset for root heading. Adjust this value to turn the root relative to the HMD around the vertical axis. Usefulf for fighting or shooting games where you would sometimes want the avatar to stand at an angled stance.")]
		[Range(-180f, 180f)]
		public float rootHeadingOffset;

		[NonSerialized]
		[HideInInspector]
		public Vector3 IKPositionHead;

		[NonSerialized]
		[HideInInspector]
		public Quaternion IKRotationHead = Quaternion.identity;

		[NonSerialized]
		[HideInInspector]
		public Vector3 IKPositionPelvis;

		[NonSerialized]
		[HideInInspector]
		public Quaternion IKRotationPelvis = Quaternion.identity;

		[NonSerialized]
		[HideInInspector]
		public Vector3 goalPositionChest;

		[NonSerialized]
		[HideInInspector]
		public Vector3 pelvisPositionOffset;

		[NonSerialized]
		[HideInInspector]
		public Vector3 chestPositionOffset;

		[NonSerialized]
		[HideInInspector]
		public Vector3 headPositionOffset;

		[NonSerialized]
		[HideInInspector]
		public Quaternion pelvisRotationOffset = Quaternion.identity;

		[NonSerialized]
		[HideInInspector]
		public Quaternion chestRotationOffset = Quaternion.identity;

		[NonSerialized]
		[HideInInspector]
		public Quaternion headRotationOffset = Quaternion.identity;

		[NonSerialized]
		[HideInInspector]
		public Vector3 faceDirection;

		[NonSerialized]
		[HideInInspector]
		internal Vector3 headPosition;

		private Quaternion headRotation = Quaternion.identity;

		private Quaternion pelvisRotation = Quaternion.identity;

		private Quaternion anchorRelativeToPelvis = Quaternion.identity;

		private Quaternion pelvisRelativeRotation = Quaternion.identity;

		private Quaternion chestRelativeRotation = Quaternion.identity;

		private Vector3 headDeltaPosition;

		private Quaternion pelvisDeltaRotation = Quaternion.identity;

		private Quaternion chestTargetRotation = Quaternion.identity;

		private int pelvisIndex;

		private int spineIndex = 1;

		private int chestIndex = -1;

		private int neckIndex = -1;

		private int headIndex = -1;

		private float length;

		private bool hasChest;

		private bool hasNeck;

		private bool hasLegs;

		private float headHeight;

		private float sizeMlp;

		private Vector3 chestForward;

		internal VirtualBone pelvis => bones[pelvisIndex];

		internal VirtualBone firstSpineBone => bones[spineIndex];

		internal VirtualBone chest
		{
			get
			{
				if (hasChest)
				{
					return bones[chestIndex];
				}
				return bones[spineIndex];
			}
		}

		internal VirtualBone head => bones[headIndex];

		private VirtualBone neck => bones[neckIndex];

		internal Quaternion anchorRotation
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return field;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				field = value;
			}
		}

		internal Quaternion anchorRelativeToHead
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return field;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				field = value;
			}
		}

		protected override void OnRead(Vector3[] positions, Quaternion[] rotations, bool hasChest, bool hasNeck, bool hasShoulders, bool hasToes, bool hasLegs, int rootIndex, int index)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0264: Unknown result type (might be due to invalid IL or missing references)
			//IL_0266: Unknown result type (might be due to invalid IL or missing references)
			//IL_026b: Unknown result type (might be due to invalid IL or missing references)
			//IL_026c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0271: Unknown result type (might be due to invalid IL or missing references)
			//IL_0277: Unknown result type (might be due to invalid IL or missing references)
			//IL_0279: Unknown result type (might be due to invalid IL or missing references)
			//IL_027e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0280: Unknown result type (might be due to invalid IL or missing references)
			//IL_0285: Unknown result type (might be due to invalid IL or missing references)
			//IL_028b: Unknown result type (might be due to invalid IL or missing references)
			//IL_028d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0294: Unknown result type (might be due to invalid IL or missing references)
			//IL_0299: Unknown result type (might be due to invalid IL or missing references)
			//IL_029e: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02db: Unknown result type (might be due to invalid IL or missing references)
			//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_030b: Unknown result type (might be due to invalid IL or missing references)
			//IL_030d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0314: Unknown result type (might be due to invalid IL or missing references)
			//IL_0315: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01da: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0213: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Unknown result type (might be due to invalid IL or missing references)
			//IL_021d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0222: Unknown result type (might be due to invalid IL or missing references)
			//IL_0228: Unknown result type (might be due to invalid IL or missing references)
			//IL_022a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0230: Unknown result type (might be due to invalid IL or missing references)
			//IL_0232: Unknown result type (might be due to invalid IL or missing references)
			//IL_0238: Unknown result type (might be due to invalid IL or missing references)
			//IL_0239: Unknown result type (might be due to invalid IL or missing references)
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0240: Unknown result type (might be due to invalid IL or missing references)
			//IL_0246: Unknown result type (might be due to invalid IL or missing references)
			//IL_024a: Unknown result type (might be due to invalid IL or missing references)
			//IL_024f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0254: Unknown result type (might be due to invalid IL or missing references)
			//IL_0259: Unknown result type (might be due to invalid IL or missing references)
			//IL_025e: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = positions[index];
			Quaternion val2 = rotations[index];
			Vector3 val3 = positions[index + 1];
			Quaternion val4 = rotations[index + 1];
			Vector3 val5 = positions[index + 2];
			Quaternion val6 = rotations[index + 2];
			Vector3 position = positions[index + 3];
			Quaternion rotation = rotations[index + 3];
			Vector3 val7 = positions[index + 4];
			Quaternion val8 = rotations[index + 4];
			this.hasLegs = hasLegs;
			if (!hasChest)
			{
				val5 = val3;
				val6 = val4;
			}
			if (!initiated)
			{
				this.hasChest = hasChest;
				this.hasNeck = hasNeck;
				Vector3 val9 = V3Tools.ExtractVertical(val7 - positions[0], rotations[0] * Vector3.up, 1f);
				headHeight = val9.magnitude;
				int num = 3;
				if (hasChest)
				{
					num++;
				}
				if (hasNeck)
				{
					num++;
				}
				bones = new VirtualBone[num];
				chestIndex = ((!hasChest) ? 1 : 2);
				neckIndex = 1;
				if (hasChest)
				{
					neckIndex++;
				}
				if (hasNeck)
				{
					neckIndex++;
				}
				headIndex = 2;
				if (hasChest)
				{
					headIndex++;
				}
				if (hasNeck)
				{
					headIndex++;
				}
				bones[0] = new VirtualBone(val, val2);
				bones[1] = new VirtualBone(val3, val4);
				if (hasChest)
				{
					bones[chestIndex] = new VirtualBone(val5, val6);
				}
				if (hasNeck)
				{
					bones[neckIndex] = new VirtualBone(position, rotation);
				}
				bones[headIndex] = new VirtualBone(val7, val8);
				pelvisRotationOffset = Quaternion.identity;
				chestRotationOffset = Quaternion.identity;
				headRotationOffset = Quaternion.identity;
				anchorRelativeToHead = Quaternion.Inverse(val8) * rotations[0];
				anchorRelativeToPelvis = Quaternion.Inverse(val2) * rotations[0];
				faceDirection = rotations[0] * Vector3.forward;
				IKPositionHead = val7;
				IKRotationHead = val8;
				IKPositionPelvis = val;
				IKRotationPelvis = val2;
				goalPositionChest = val5 + rotations[0] * Vector3.forward;
			}
			pelvisRelativeRotation = Quaternion.Inverse(val8) * val2;
			chestRelativeRotation = Quaternion.Inverse(val8) * val6;
			chestForward = Quaternion.Inverse(val6) * (rotations[0] * Vector3.forward);
			bones[0].Read(val, val2);
			bones[1].Read(val3, val4);
			if (hasChest)
			{
				bones[chestIndex].Read(val5, val6);
			}
			if (hasNeck)
			{
				bones[neckIndex].Read(position, rotation);
			}
			bones[headIndex].Read(val7, val8);
			float num2 = Vector3.Distance(val, val7);
			sizeMlp = num2 / 0.7f;
		}

		public override void PreSolve(float scale)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)headTarget != (Object)null)
			{
				IKPositionHead = headTarget.position;
				IKRotationHead = headTarget.rotation;
			}
			if ((Object)(object)chestGoal != (Object)null)
			{
				goalPositionChest = chestGoal.position;
			}
			if ((Object)(object)pelvisTarget != (Object)null)
			{
				IKPositionPelvis = pelvisTarget.position;
				IKRotationPelvis = pelvisTarget.rotation;
			}
			if (useAnimatedHeadHeightWeight > 0f && useAnimatedHeadHeightRange > 0f)
			{
				Vector3 verticalAxis = rootRotation * Vector3.up;
				if (animatedHeadHeightBlend > 0f)
				{
					Vector3 val = V3Tools.ExtractVertical(IKPositionHead - head.solverPosition, verticalAxis, 1f);
					float num = Mathf.Abs(val.magnitude);
					num = Mathf.Max(num - useAnimatedHeadHeightRange * scale, 0f);
					float num2 = Mathf.Lerp(0f, 1f, num / (animatedHeadHeightBlend * scale));
					num2 = Interp.Float(1f - num2, InterpolationMode.InOutSine);
					Vector3 v = head.solverPosition - IKPositionHead;
					IKPositionHead += V3Tools.ExtractVertical(v, verticalAxis, num2 * useAnimatedHeadHeightWeight);
				}
				else
				{
					IKPositionHead += V3Tools.ExtractVertical(head.solverPosition - IKPositionHead, verticalAxis, useAnimatedHeadHeightWeight);
				}
			}
			headPosition = V3Tools.Lerp(head.solverPosition, IKPositionHead, positionWeight);
			headRotation = QuaTools.Lerp(head.solverRotation, IKRotationHead, rotationWeight);
			pelvisRotation = QuaTools.Lerp(pelvis.solverRotation, IKRotationPelvis, rotationWeight);
		}

		public override void ApplyOffsets(float scale)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			headPosition += headPositionOffset;
			float num = minHeadHeight * scale;
			Vector3 val = rootRotation * Vector3.up;
			if (val == Vector3.up)
			{
				headPosition.y = Math.Max(rootPosition.y + num, headPosition.y);
			}
			else
			{
				Vector3 val2 = headPosition - rootPosition;
				Vector3 val3 = V3Tools.ExtractHorizontal(val2, val, 1f);
				Vector3 val4 = val2 - val3;
				if (Vector3.Dot(val4, val) > 0f)
				{
					if (val4.magnitude < num)
					{
						val4 = val4.normalized * num;
					}
				}
				else
				{
					val4 = -val4.normalized * num;
				}
				headPosition = rootPosition + val3 + val4;
			}
			headRotation = headRotationOffset * headRotation;
			headDeltaPosition = headPosition - head.solverPosition;
			pelvisDeltaRotation = QuaTools.FromToRotation(pelvis.solverRotation, headRotation * pelvisRelativeRotation);
			if (pelvisRotationWeight <= 0f)
			{
				anchorRotation = headRotation * anchorRelativeToHead;
			}
			else if (pelvisRotationWeight > 0f && pelvisRotationWeight < 1f)
			{
				anchorRotation = Quaternion.Lerp(headRotation * anchorRelativeToHead, pelvisRotation * anchorRelativeToPelvis, pelvisRotationWeight);
			}
			else if (pelvisRotationWeight >= 1f)
			{
				anchorRotation = pelvisRotation * anchorRelativeToPelvis;
			}
		}

		private void CalculateChestTargetRotation(VirtualBone rootBone, Arm[] arms)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			chestTargetRotation = headRotation * chestRelativeRotation;
			if (arms[0] != null)
			{
				AdjustChestByHands(ref chestTargetRotation, arms);
			}
			faceDirection = Vector3.Cross(anchorRotation * Vector3.right, rootBone.readRotation * Vector3.up) + anchorRotation * Vector3.forward;
		}

		public void Solve(Animator animator, VirtualBone rootBone, Leg[] legs, Arm[] arms, float scale)
		{
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0227: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Unknown result type (might be due to invalid IL or missing references)
			//IL_0241: Unknown result type (might be due to invalid IL or missing references)
			//IL_0242: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_0205: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			CalculateChestTargetRotation(rootBone, arms);
			if (maxRootAngle < 180f)
			{
				Vector3 val = faceDirection;
				if (rootHeadingOffset != 0f)
				{
					val = Quaternion.AngleAxis(rootHeadingOffset, Vector3.up) * val;
				}
				Vector3 val2 = Quaternion.Inverse(rootBone.solverRotation) * val;
				float num = Mathf.Atan2(val2.x, val2.z) * 57.29578f;
				float num2 = 0f;
				float num3 = maxRootAngle;
				if (num > num3)
				{
					num2 = num - num3;
				}
				if (num < 0f - num3)
				{
					num2 = num + num3;
				}
				Quaternion val3 = Quaternion.AngleAxis(num2, rootBone.readRotation * Vector3.up);
				if ((Object)(object)animator != (Object)null && ((Behaviour)animator).enabled)
				{
					Vector3 pivotPosition = animator.pivotPosition;
					Vector3 val4 = rootBone.solverPosition - pivotPosition;
					rootBone.solverPosition = pivotPosition + val3 * val4;
				}
				rootBone.solverRotation = val3 * rootBone.solverRotation;
			}
			Vector3 solverPosition = pelvis.solverPosition;
			Vector3 rootUp = rootBone.solverRotation * Vector3.up;
			TranslatePelvis(legs, headDeltaPosition, pelvisDeltaRotation, scale);
			FABRIKPass(solverPosition, rootUp, positionWeight);
			Bend(bones, pelvisIndex, chestIndex, chestTargetRotation, chestRotationOffset, chestClampWeight, uniformWeight: false, neckStiffness * rotationWeight);
			if (LOD < 1 && chestGoalWeight > 0f)
			{
				Quaternion targetRotation = Quaternion.FromToRotation(bones[chestIndex].solverRotation * chestForward, goalPositionChest - bones[chestIndex].solverPosition) * bones[chestIndex].solverRotation;
				Bend(bones, pelvisIndex, chestIndex, targetRotation, chestRotationOffset, chestClampWeight, uniformWeight: false, chestGoalWeight * rotationWeight);
			}
			InverseTranslateToHead(legs, limited: false, useCurrentLegMag: false, Vector3.zero, positionWeight);
			if (LOD < 1)
			{
				FABRIKPass(solverPosition, rootUp, positionWeight);
			}
			Bend(bones, neckIndex, headIndex, headRotation, headClampWeight, uniformWeight: true, rotationWeight);
			SolvePelvis();
		}

		private void FABRIKPass(Vector3 animatedPelvisPos, Vector3 rootUp, float weight)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			Vector3 startPosition = Vector3.Lerp(pelvis.solverPosition, animatedPelvisPos, maintainPelvisPosition) + pelvisPositionOffset;
			Vector3 targetPosition = headPosition - chestPositionOffset;
			Vector3 zero = Vector3.zero;
			float num = Vector3.Distance(bones[0].solverPosition, bones[bones.Length - 1].solverPosition);
			VirtualBone.SolveFABRIK(bones, startPosition, targetPosition, weight, 1f, 1, num, zero);
		}

		private void SolvePelvis()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0198: Unknown result type (might be due to invalid IL or missing references)
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			if (pelvisPositionWeight > 0f)
			{
				Quaternion solverRotation = head.solverRotation;
				Vector3 val = (IKPositionPelvis + pelvisPositionOffset - pelvis.solverPosition) * pelvisPositionWeight;
				VirtualBone[] array = bones;
				foreach (VirtualBone obj in array)
				{
					obj.solverPosition += val;
				}
				Vector3 bendNormal = anchorRotation * Vector3.right;
				if (hasChest && hasNeck)
				{
					VirtualBone.SolveTrigonometric(bones, spineIndex, chestIndex, headIndex, headPosition, bendNormal, pelvisPositionWeight * 0.9f);
					VirtualBone.SolveTrigonometric(bones, chestIndex, neckIndex, headIndex, headPosition, bendNormal, pelvisPositionWeight);
				}
				else if (hasChest && !hasNeck)
				{
					VirtualBone.SolveTrigonometric(bones, spineIndex, chestIndex, headIndex, headPosition, bendNormal, pelvisPositionWeight);
				}
				else if (!hasChest && hasNeck)
				{
					VirtualBone.SolveTrigonometric(bones, spineIndex, neckIndex, headIndex, headPosition, bendNormal, pelvisPositionWeight);
				}
				else if (!hasNeck && !hasChest)
				{
					VirtualBone.SolveTrigonometric(bones, pelvisIndex, spineIndex, headIndex, headPosition, bendNormal, pelvisPositionWeight);
				}
				head.solverRotation = solverRotation;
			}
		}

		public override void Write(ref Vector3[] solvedPositions, ref Quaternion[] solvedRotations)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			solvedPositions[index] = bones[0].solverPosition;
			solvedRotations[index] = bones[0].solverRotation;
			solvedRotations[index + 1] = bones[1].solverRotation;
			if (hasChest)
			{
				solvedRotations[index + 2] = bones[chestIndex].solverRotation;
			}
			if (hasNeck)
			{
				solvedRotations[index + 3] = bones[neckIndex].solverRotation;
			}
			solvedRotations[index + 4] = bones[headIndex].solverRotation;
		}

		public override void ResetOffsets()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
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
			pelvisPositionOffset = Vector3.zero;
			chestPositionOffset = Vector3.zero;
			headPositionOffset = Vector3.zero;
			pelvisRotationOffset = Quaternion.identity;
			chestRotationOffset = Quaternion.identity;
			headRotationOffset = Quaternion.identity;
		}

		private void AdjustChestByHands(ref Quaternion chestTargetRotation, Arm[] arms)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			if (LOD <= 0)
			{
				Quaternion val = Quaternion.Inverse(anchorRotation);
				Vector3 val2 = val * (arms[0].position - headPosition) / sizeMlp;
				Vector3 val3 = val * (arms[1].position - headPosition) / sizeMlp;
				Vector3 forward = Vector3.forward;
				forward.x += val2.x * Mathf.Abs(val2.x);
				forward.x += val2.z * Mathf.Abs(val2.z);
				forward.x += val3.x * Mathf.Abs(val3.x);
				forward.x -= val3.z * Mathf.Abs(val3.z);
				forward.x *= 5f * rotateChestByHands;
				Quaternion val4 = Quaternion.AngleAxis(Mathf.Atan2(forward.x, forward.z) * 57.29578f, rootRotation * Vector3.up);
				chestTargetRotation = val4 * chestTargetRotation;
				Vector3 up = Vector3.up;
				up.x += val2.y;
				up.x -= val3.y;
				up.x *= 0.5f * rotateChestByHands;
				val4 = Quaternion.AngleAxis(Mathf.Atan2(up.x, up.y) * 57.29578f, rootRotation * Vector3.back);
				chestTargetRotation = val4 * chestTargetRotation;
			}
		}

		public void InverseTranslateToHead(Leg[] legs, bool limited, bool useCurrentLegMag, Vector3 offset, float w)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = (headPosition + offset - head.solverPosition) * w;
			Vector3 val2 = pelvis.solverPosition + val;
			MovePosition(limited ? LimitPelvisPosition(legs, val2, useCurrentLegMag) : val2);
		}

		private void TranslatePelvis(Leg[] legs, Vector3 deltaPosition, Quaternion deltaRotation, float scale)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			Vector3 solverPosition = head.solverPosition;
			deltaRotation = QuaTools.ClampRotation(deltaRotation, chestClampWeight, 2);
			Quaternion val = Quaternion.Slerp(Quaternion.identity, deltaRotation, bodyRotStiffness * rotationWeight);
			val = Quaternion.Slerp(val, QuaTools.FromToRotation(pelvis.solverRotation, IKRotationPelvis), pelvisRotationWeight);
			VirtualBone.RotateAroundPoint(bones, 0, pelvis.solverPosition, pelvisRotationOffset * val);
			deltaPosition -= head.solverPosition - solverPosition;
			Vector3 val2 = rootRotation * Vector3.forward;
			Vector3 val3 = V3Tools.ExtractVertical(deltaPosition, rootRotation * Vector3.up, 1f);
			float num = val3.magnitude;
			if (scale > 0f)
			{
				num /= scale;
			}
			float num2 = num * (0f - moveBodyBackWhenCrouching) * headHeight;
			deltaPosition += val2 * num2;
			MovePosition(LimitPelvisPosition(legs, pelvis.solverPosition + deltaPosition * bodyPosStiffness * positionWeight, useCurrentLegMag: false));
		}

		private Vector3 LimitPelvisPosition(Leg[] legs, Vector3 pelvisPosition, bool useCurrentLegMag, int it = 2)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			if (!hasLegs)
			{
				return pelvisPosition;
			}
			if (useCurrentLegMag)
			{
				Leg[] array = legs;
				foreach (Leg leg in array)
				{
					leg.currentMag = Mathf.Max(Vector3.Distance(leg.thigh.solverPosition, leg.lastBone.solverPosition), leg.currentMag);
				}
			}
			for (int j = 0; j < it; j++)
			{
				Leg[] array = legs;
				foreach (Leg leg2 in array)
				{
					Vector3 val = pelvisPosition - pelvis.solverPosition;
					Vector3 val2 = leg2.thigh.solverPosition + val;
					Vector3 val3 = val2 - leg2.position;
					float num = (useCurrentLegMag ? leg2.currentMag : leg2.mag);
					Vector3 val4 = leg2.position + Vector3.ClampMagnitude(val3, num);
					pelvisPosition += val4 - val2;
				}
			}
			return pelvisPosition;
		}

		private void Bend(VirtualBone[] bones, int firstIndex, int lastIndex, Quaternion targetRotation, float clampWeight, bool uniformWeight, float w)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			if (w <= 0f || bones.Length == 0)
			{
				return;
			}
			int num = lastIndex + 1 - firstIndex;
			if (num < 1)
			{
				return;
			}
			Quaternion rotation = QuaTools.FromToRotation(bones[lastIndex].solverRotation, targetRotation);
			rotation = QuaTools.ClampRotation(rotation, clampWeight, 2);
			float num2 = (uniformWeight ? (1f / (float)num) : 0f);
			for (int i = firstIndex; i < lastIndex + 1; i++)
			{
				if (!uniformWeight)
				{
					num2 = Mathf.Clamp((float)((i - firstIndex + 1) / num), 0f, 1f);
				}
				VirtualBone.RotateAroundPoint(bones, i, bones[i].solverPosition, Quaternion.Slerp(Quaternion.identity, rotation, num2 * w));
			}
		}

		private void Bend(VirtualBone[] bones, int firstIndex, int lastIndex, Quaternion targetRotation, Quaternion rotationOffset, float clampWeight, bool uniformWeight, float w)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			if (w <= 0f || bones.Length == 0)
			{
				return;
			}
			int num = lastIndex + 1 - firstIndex;
			if (num < 1)
			{
				return;
			}
			Quaternion rotation = QuaTools.FromToRotation(bones[lastIndex].solverRotation, targetRotation);
			rotation = QuaTools.ClampRotation(rotation, clampWeight, 2);
			float num2 = (uniformWeight ? (1f / (float)num) : 0f);
			for (int i = firstIndex; i < lastIndex + 1; i++)
			{
				if (!uniformWeight)
				{
					if (num == 1)
					{
						num2 = 1f;
					}
					else if (num == 2)
					{
						num2 = ((i == 0) ? 0.2f : 0.8f);
					}
					else if (num == 3)
					{
						num2 = i switch
						{
							0 => 0.15f, 
							1 => 0.4f, 
							_ => 0.45f, 
						};
					}
					else if (num > 3)
					{
						num2 = 1f / (float)num;
					}
				}
				VirtualBone.RotateAroundPoint(bones, i, bones[i].solverPosition, Quaternion.Slerp(Quaternion.Slerp(Quaternion.identity, rotationOffset, num2), rotation, num2 * w));
			}
		}

		public Spine()
		{
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public enum PositionOffset
	{
		Pelvis,
		Chest,
		Head,
		LeftHand,
		RightHand,
		LeftFoot,
		RightFoot,
		LeftHeel,
		RightHeel
	}

	[Serializable]
	public enum RotationOffset
	{
		Pelvis,
		Chest,
		Head
	}

	[Serializable]
	public class VirtualBone
	{
		public Vector3 readPosition;

		public Quaternion readRotation;

		public Vector3 solverPosition;

		public Quaternion solverRotation;

		public float length;

		public float sqrMag;

		public Vector3 axis;

		public VirtualBone(Vector3 position, Quaternion rotation)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			Read(position, rotation);
		}

		public void Read(Vector3 position, Quaternion rotation)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			readPosition = position;
			readRotation = rotation;
			solverPosition = position;
			solverRotation = rotation;
		}

		public static void SwingRotation(VirtualBone[] bones, int index, Vector3 swingTarget, float weight = 1f)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			if (!(weight <= 0f))
			{
				Quaternion val = Quaternion.FromToRotation(bones[index].solverRotation * bones[index].axis, swingTarget - bones[index].solverPosition);
				if (weight < 1f)
				{
					val = Quaternion.Lerp(Quaternion.identity, val, weight);
				}
				for (int i = index; i < bones.Length; i++)
				{
					bones[i].solverRotation = val * bones[i].solverRotation;
				}
			}
		}

		public static float PreSolve(ref VirtualBone[] bones)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			float num = 0f;
			for (int i = 0; i < bones.Length; i++)
			{
				if (i < bones.Length - 1)
				{
					VirtualBone obj = bones[i];
					Vector3 val = bones[i + 1].solverPosition - bones[i].solverPosition;
					obj.sqrMag = val.sqrMagnitude;
					bones[i].length = Mathf.Sqrt(bones[i].sqrMag);
					num += bones[i].length;
					bones[i].axis = Quaternion.Inverse(bones[i].solverRotation) * (bones[i + 1].solverPosition - bones[i].solverPosition);
				}
				else
				{
					bones[i].sqrMag = 0f;
					bones[i].length = 0f;
				}
			}
			return num;
		}

		public static void RotateAroundPoint(VirtualBone[] bones, int index, Vector3 point, Quaternion rotation)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			for (int i = index; i < bones.Length; i++)
			{
				if (bones[i] != null)
				{
					Vector3 val = bones[i].solverPosition - point;
					bones[i].solverPosition = point + rotation * val;
					bones[i].solverRotation = rotation * bones[i].solverRotation;
				}
			}
		}

		public static void RotateBy(VirtualBone[] bones, int index, Quaternion rotation)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			for (int i = index; i < bones.Length; i++)
			{
				if (bones[i] != null)
				{
					Vector3 val = bones[i].solverPosition - bones[index].solverPosition;
					bones[i].solverPosition = bones[index].solverPosition + rotation * val;
					bones[i].solverRotation = rotation * bones[i].solverRotation;
				}
			}
		}

		public static void RotateBy(VirtualBone[] bones, Quaternion rotation)
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < bones.Length; i++)
			{
				if (bones[i] != null)
				{
					if (i > 0)
					{
						Vector3 val = bones[i].solverPosition - bones[0].solverPosition;
						bones[i].solverPosition = bones[0].solverPosition + rotation * val;
					}
					bones[i].solverRotation = rotation * bones[i].solverRotation;
				}
			}
		}

		public static void RotateTo(VirtualBone[] bones, int index, Quaternion rotation)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			Quaternion rotation2 = QuaTools.FromToRotation(bones[index].solverRotation, rotation);
			RotateAroundPoint(bones, index, bones[index].solverPosition, rotation2);
		}

		public static void SolveTrigonometric(VirtualBone[] bones, int first, int second, int third, Vector3 targetPosition, Vector3 bendNormal, float weight)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			if (weight <= 0f)
			{
				return;
			}
			targetPosition = Vector3.Lerp(bones[third].solverPosition, targetPosition, weight);
			Vector3 val = targetPosition - bones[first].solverPosition;
			float sqrMagnitude = val.sqrMagnitude;
			if (sqrMagnitude != 0f)
			{
				float directionMag = Mathf.Sqrt(sqrMagnitude);
				Vector3 val2 = bones[second].solverPosition - bones[first].solverPosition;
				float sqrMagnitude2 = val2.sqrMagnitude;
				val2 = bones[third].solverPosition - bones[second].solverPosition;
				float sqrMagnitude3 = val2.sqrMagnitude;
				Vector3 bendDirection = Vector3.Cross(val, bendNormal);
				Vector3 directionToBendPoint = GetDirectionToBendPoint(val, directionMag, bendDirection, sqrMagnitude2, sqrMagnitude3);
				Quaternion val3 = Quaternion.FromToRotation(bones[second].solverPosition - bones[first].solverPosition, directionToBendPoint);
				if (weight < 1f)
				{
					val3 = Quaternion.Lerp(Quaternion.identity, val3, weight);
				}
				RotateAroundPoint(bones, first, bones[first].solverPosition, val3);
				Quaternion val4 = Quaternion.FromToRotation(bones[third].solverPosition - bones[second].solverPosition, targetPosition - bones[second].solverPosition);
				if (weight < 1f)
				{
					val4 = Quaternion.Lerp(Quaternion.identity, val4, weight);
				}
				RotateAroundPoint(bones, second, bones[second].solverPosition, val4);
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

		public static void SolveFABRIK(VirtualBone[] bones, Vector3 startPosition, Vector3 targetPosition, float weight, float minNormalizedTargetDistance, int iterations, float length, Vector3 startOffset)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			if (weight <= 0f)
			{
				return;
			}
			if (minNormalizedTargetDistance > 0f)
			{
				Vector3 val = targetPosition - startPosition;
				float magnitude = val.magnitude;
				Vector3 val2 = startPosition + val / magnitude * Mathf.Max(length * minNormalizedTargetDistance, magnitude);
				targetPosition = Vector3.Lerp(targetPosition, val2, weight);
			}
			for (int i = 0; i < iterations; i++)
			{
				bones[^1].solverPosition = Vector3.Lerp(bones[^1].solverPosition, targetPosition, weight);
				for (int num = bones.Length - 2; num > -1; num--)
				{
					bones[num].solverPosition = SolveFABRIKJoint(bones[num].solverPosition, bones[num + 1].solverPosition, bones[num].length);
				}
				if (i == 0)
				{
					foreach (VirtualBone obj in bones)
					{
						obj.solverPosition += startOffset;
					}
				}
				bones[0].solverPosition = startPosition;
				for (int k = 1; k < bones.Length; k++)
				{
					bones[k].solverPosition = SolveFABRIKJoint(bones[k].solverPosition, bones[k - 1].solverPosition, bones[k - 1].length);
				}
			}
			for (int l = 0; l < bones.Length - 1; l++)
			{
				SwingRotation(bones, l, bones[l + 1].solverPosition);
			}
		}

		private static Vector3 SolveFABRIKJoint(Vector3 pos1, Vector3 pos2, float length)
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

		public static void SolveCCD(VirtualBone[] bones, Vector3 targetPosition, float weight, int iterations)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			if (weight <= 0f)
			{
				return;
			}
			for (int i = 0; i < iterations; i++)
			{
				for (int num = bones.Length - 2; num > -1; num--)
				{
					Vector3 val = bones[^1].solverPosition - bones[num].solverPosition;
					Vector3 val2 = targetPosition - bones[num].solverPosition;
					Quaternion val3 = Quaternion.FromToRotation(val, val2);
					if (weight >= 1f)
					{
						RotateBy(bones, num, val3);
					}
					else
					{
						RotateBy(bones, num, Quaternion.Lerp(Quaternion.identity, val3, weight));
					}
				}
			}
		}
	}

	private Transform[] solverTransforms = new Transform[0];

	private bool hasChest;

	private bool hasNeck;

	private bool hasShoulders;

	private bool hasToes;

	private bool hasLegs;

	private bool hasArms;

	private Vector3[] readPositions = new Vector3[0];

	private Quaternion[] readRotations = new Quaternion[0];

	private Vector3[] solvedPositions = new Vector3[22];

	private Quaternion[] solvedRotations = new Quaternion[22];

	private Quaternion[] defaultLocalRotations = new Quaternion[21];

	private Vector3[] defaultLocalPositions = new Vector3[21];

	private Vector3 rootV;

	private Vector3 rootVelocity;

	private Vector3 bodyOffset;

	private int supportLegIndex;

	private int lastLOD;

	private float lastLocomotionWeight;

	[Tooltip("LOD 0: Full quality solving. LOD 1: Shoulder solving, stretching plant feet disabled, spine solving quality reduced. This provides about 30% of performance gain. LOD 2: Culled, but updating root position and rotation if locomotion is enabled.")]
	[Range(0f, 2f)]
	public int LOD;

	[Tooltip("Scale of the character. Value of 1 means normal adult human size.")]
	public float scale = 1f;

	[Tooltip("If true, will keep the toes planted even if head target is out of reach, so this can cause the camera to exit the head if it is too high for the model to reach. Enabling this increases the cost of the solver as the legs will have to be solved multiple times.")]
	public bool plantFeet = true;

	[Tooltip("The spine solver.")]
	public Spine spine = new Spine();

	[Tooltip("The left arm solver.")]
	public Arm leftArm = new Arm();

	[Tooltip("The right arm solver.")]
	public Arm rightArm = new Arm();

	[Tooltip("The left leg solver.")]
	public Leg leftLeg = new Leg();

	[Tooltip("The right leg solver.")]
	public Leg rightLeg = new Leg();

	[Tooltip("Procedural leg shuffling for stationary VR games. Not designed for roomscale and thumbstick locomotion. For those it would be better to use a strafing locomotion blend tree to make the character follow the horizontal direction towards the HMD by root motion or script.")]
	public Locomotion locomotion = new Locomotion();

	private Leg[] legs = new Leg[2];

	private Arm[] arms = new Arm[2];

	private Vector3 headPosition;

	private Vector3 headDeltaPosition;

	private Vector3 raycastOriginPelvis;

	private Vector3 lastOffset;

	private Vector3 debugPos1;

	private Vector3 debugPos2;

	private Vector3 debugPos3;

	private Vector3 debugPos4;

	public Animator animator { get; private set; }

	[HideInInspector]
	public VirtualBone rootBone { get; private set; }

	public void SetToReferences(VRIK.References references)
	{
		if (!references.isFilled)
		{
			Debug.LogError((object)"Invalid references, one or more Transforms are missing.");
			return;
		}
		animator = ((Component)references.root).GetComponent<Animator>();
		solverTransforms = references.GetTransforms();
		hasChest = (Object)(object)solverTransforms[3] != (Object)null;
		hasNeck = (Object)(object)solverTransforms[4] != (Object)null;
		hasShoulders = (Object)(object)solverTransforms[6] != (Object)null && (Object)(object)solverTransforms[10] != (Object)null;
		hasToes = (Object)(object)solverTransforms[17] != (Object)null && (Object)(object)solverTransforms[21] != (Object)null;
		hasLegs = (Object)(object)solverTransforms[14] != (Object)null;
		hasArms = (Object)(object)solverTransforms[7] != (Object)null;
		readPositions = new Vector3[solverTransforms.Length];
		readRotations = new Quaternion[solverTransforms.Length];
		DefaultAnimationCurves();
		GuessHandOrientations(references, onlyIfZero: true);
	}

	public void GuessHandOrientations(VRIK.References references, bool onlyIfZero)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		if (!references.isFilled)
		{
			Debug.LogWarning((object)"VRIK References are not filled in, can not guess hand orientations. Right-click on VRIK header and slect 'Guess Hand Orientations' when you have filled in the References.", (Object)(object)references.root);
			return;
		}
		if (leftArm.wristToPalmAxis == Vector3.zero || !onlyIfZero)
		{
			leftArm.wristToPalmAxis = VRIKCalibrator.GuessWristToPalmAxis(references.leftHand, references.leftForearm);
		}
		if (leftArm.palmToThumbAxis == Vector3.zero || !onlyIfZero)
		{
			leftArm.palmToThumbAxis = VRIKCalibrator.GuessPalmToThumbAxis(references.leftHand, references.leftForearm);
		}
		if (rightArm.wristToPalmAxis == Vector3.zero || !onlyIfZero)
		{
			rightArm.wristToPalmAxis = VRIKCalibrator.GuessWristToPalmAxis(references.rightHand, references.rightForearm);
		}
		if (rightArm.palmToThumbAxis == Vector3.zero || !onlyIfZero)
		{
			rightArm.palmToThumbAxis = VRIKCalibrator.GuessPalmToThumbAxis(references.rightHand, references.rightForearm);
		}
	}

	public void DefaultAnimationCurves()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected Obj, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected Obj, but got Unknown
		if (locomotion.stepHeight == null)
		{
			locomotion.stepHeight = new AnimationCurve();
		}
		if (locomotion.heelHeight == null)
		{
			locomotion.heelHeight = new AnimationCurve();
		}
		if (locomotion.stepHeight.keys.Length == 0)
		{
			locomotion.stepHeight.keys = GetSineKeyframes(0.03f);
		}
		if (locomotion.heelHeight.keys.Length == 0)
		{
			locomotion.heelHeight.keys = GetSineKeyframes(0.03f);
		}
	}

	public void AddPositionOffset(PositionOffset positionOffset, Vector3 value)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		switch (positionOffset)
		{
		case PositionOffset.Pelvis:
		{
			Spine spine3 = this.spine;
			spine3.pelvisPositionOffset += value;
			break;
		}
		case PositionOffset.Chest:
		{
			Spine spine2 = this.spine;
			spine2.chestPositionOffset += value;
			break;
		}
		case PositionOffset.Head:
		{
			Spine spine = this.spine;
			spine.headPositionOffset += value;
			break;
		}
		case PositionOffset.LeftHand:
		{
			Arm arm2 = leftArm;
			arm2.handPositionOffset += value;
			break;
		}
		case PositionOffset.RightHand:
		{
			Arm arm = rightArm;
			arm.handPositionOffset += value;
			break;
		}
		case PositionOffset.LeftFoot:
		{
			Leg leg4 = leftLeg;
			leg4.footPositionOffset += value;
			break;
		}
		case PositionOffset.RightFoot:
		{
			Leg leg3 = rightLeg;
			leg3.footPositionOffset += value;
			break;
		}
		case PositionOffset.LeftHeel:
		{
			Leg leg2 = leftLeg;
			leg2.heelPositionOffset += value;
			break;
		}
		case PositionOffset.RightHeel:
		{
			Leg leg = rightLeg;
			leg.heelPositionOffset += value;
			break;
		}
		}
	}

	public void AddRotationOffset(RotationOffset rotationOffset, Vector3 value)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		AddRotationOffset(rotationOffset, Quaternion.Euler(value));
	}

	public void AddRotationOffset(RotationOffset rotationOffset, Quaternion value)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		switch (rotationOffset)
		{
		case RotationOffset.Pelvis:
			spine.pelvisRotationOffset = value * spine.pelvisRotationOffset;
			break;
		case RotationOffset.Chest:
			spine.chestRotationOffset = value * spine.chestRotationOffset;
			break;
		case RotationOffset.Head:
			spine.headRotationOffset = value * spine.headRotationOffset;
			break;
		}
	}

	public void AddPlatformMotion(Vector3 deltaPosition, Quaternion deltaRotation, Vector3 platformPivot)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		locomotion.AddDeltaPosition(deltaPosition);
		raycastOriginPelvis += deltaPosition;
		locomotion.AddDeltaRotation(deltaRotation, platformPivot);
		spine.faceDirection = deltaRotation * spine.faceDirection;
	}

	public void Reset()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (initiated)
		{
			UpdateSolverTransforms();
			Read(readPositions, readRotations, hasChest, hasNeck, hasShoulders, hasToes, hasLegs, hasArms);
			spine.faceDirection = rootBone.readRotation * Vector3.forward;
			if (hasLegs)
			{
				locomotion.Reset(readPositions, readRotations);
				raycastOriginPelvis = spine.pelvis.readPosition;
			}
		}
	}

	public override void StoreDefaultLocalState()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 1; i < solverTransforms.Length; i++)
		{
			if ((Object)(object)solverTransforms[i] != (Object)null)
			{
				defaultLocalPositions[i - 1] = solverTransforms[i].localPosition;
				defaultLocalRotations[i - 1] = solverTransforms[i].localRotation;
			}
		}
	}

	public override void FixTransforms()
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (!initiated || LOD >= 2)
		{
			return;
		}
		for (int i = 1; i < solverTransforms.Length; i++)
		{
			if ((Object)(object)solverTransforms[i] != (Object)null)
			{
				bool flag = i == 1;
				bool flag2 = i == 8 || i == 9 || i == 12 || i == 13;
				bool flag3 = (i >= 15 && i <= 17) || (i >= 19 && i <= 21);
				if (flag | flag2 | flag3)
				{
					solverTransforms[i].localPosition = defaultLocalPositions[i - 1];
				}
				solverTransforms[i].localRotation = defaultLocalRotations[i - 1];
			}
		}
	}

	public override Point[] GetPoints()
	{
		Debug.LogError((object)"GetPoints() is not applicable to IKSolverVR.");
		return null;
	}

	public override Point GetPoint(Transform transform)
	{
		Debug.LogError((object)"GetPoint is not applicable to IKSolverVR.");
		return null;
	}

	public override bool IsValid(ref string message)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (solverTransforms == null || solverTransforms.Length == 0)
		{
			message = "Trying to initiate IKSolverVR with invalid bone references.";
			return false;
		}
		if (leftArm.wristToPalmAxis == Vector3.zero)
		{
			message = "Left arm 'Wrist To Palm Axis' needs to be set in VRIK. Please select the hand bone, set it to the axis that points from the wrist towards the palm. If the arrow points away from the palm, axis must be negative.";
			return false;
		}
		if (rightArm.wristToPalmAxis == Vector3.zero)
		{
			message = "Right arm 'Wrist To Palm Axis' needs to be set in VRIK. Please select the hand bone, set it to the axis that points from the wrist towards the palm. If the arrow points away from the palm, axis must be negative.";
			return false;
		}
		if (leftArm.palmToThumbAxis == Vector3.zero)
		{
			message = "Left arm 'Palm To Thumb Axis' needs to be set in VRIK. Please select the hand bone, set it to the axis that points from the palm towards the thumb. If the arrow points away from the thumb, axis must be negative.";
			return false;
		}
		if (rightArm.palmToThumbAxis == Vector3.zero)
		{
			message = "Right arm 'Palm To Thumb Axis' needs to be set in VRIK. Please select the hand bone, set it to the axis that points from the palm towards the thumb. If the arrow points away from the thumb, axis must be negative.";
			return false;
		}
		return true;
	}

	private Vector3 GetNormal(Transform[] transforms)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		Vector3 val2 = Vector3.zero;
		for (int i = 0; i < transforms.Length; i++)
		{
			val2 += transforms[i].position;
		}
		val2 /= (float)transforms.Length;
		for (int j = 0; j < transforms.Length - 1; j++)
		{
			Vector3 val3 = val;
			Vector3 val4 = Vector3.Cross(transforms[j].position - val2, transforms[j + 1].position - val2);
			val = val3 + val4.normalized;
		}
		return val;
	}

	private static Keyframe[] GetSineKeyframes(float mag)
	{
		Keyframe[] array = new Keyframe[3];
		array[0].time = 0f;
		array[0].value = 0f;
		array[1].time = 0.5f;
		array[1].value = mag;
		array[2].time = 1f;
		array[2].value = 0f;
		return array;
	}

	private void UpdateSolverTransforms()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < solverTransforms.Length; i++)
		{
			if ((Object)(object)solverTransforms[i] != (Object)null)
			{
				readPositions[i] = solverTransforms[i].position;
				readRotations[i] = solverTransforms[i].rotation;
			}
		}
	}

	protected override void OnInitiate()
	{
		UpdateSolverTransforms();
		Read(readPositions, readRotations, hasChest, hasNeck, hasShoulders, hasToes, hasLegs, hasArms);
	}

	protected override void OnUpdate()
	{
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		if (IKPositionWeight > 0f)
		{
			if (LOD < 2)
			{
				bool flag = false;
				if (lastLOD != LOD && lastLOD == 2)
				{
					spine.faceDirection = rootBone.readRotation * Vector3.forward;
					if (hasLegs)
					{
						if (locomotion.weight > 0f)
						{
							root.position = new Vector3(spine.headTarget.position.x, root.position.y, spine.headTarget.position.z);
							Vector3 faceDirection = spine.faceDirection;
							faceDirection.y = 0f;
							root.rotation = Quaternion.LookRotation(faceDirection, root.up);
							UpdateSolverTransforms();
							Read(readPositions, readRotations, hasChest, hasNeck, hasShoulders, hasToes, hasLegs, hasArms);
							flag = true;
							locomotion.Reset(readPositions, readRotations);
						}
						raycastOriginPelvis = spine.pelvis.readPosition;
					}
				}
				if (!flag)
				{
					UpdateSolverTransforms();
					Read(readPositions, readRotations, hasChest, hasNeck, hasShoulders, hasToes, hasLegs, hasArms);
				}
				Solve();
				Write();
				WriteTransforms();
			}
			else if (locomotion.weight > 0f)
			{
				root.position = new Vector3(spine.headTarget.position.x, root.position.y, spine.headTarget.position.z);
				Vector3 val = spine.headTarget.rotation * spine.anchorRelativeToHead * Vector3.forward;
				val.y = 0f;
				root.rotation = Quaternion.LookRotation(val, root.up);
			}
		}
		lastLOD = LOD;
	}

	private void WriteTransforms()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < solverTransforms.Length; i++)
		{
			if (!((Object)(object)solverTransforms[i] != (Object)null))
			{
				continue;
			}
			bool flag = i < 2;
			bool flag2 = i == 8 || i == 9 || i == 12 || i == 13;
			bool flag3 = (i >= 15 && i <= 17) || (i >= 19 && i <= 21);
			if (LOD > 0)
			{
				flag2 = false;
				flag3 = false;
			}
			if (flag)
			{
				solverTransforms[i].position = V3Tools.Lerp(solverTransforms[i].position, GetPosition(i), IKPositionWeight);
			}
			if (flag2 | flag3)
			{
				if (IKPositionWeight < 1f)
				{
					Vector3 localPosition = solverTransforms[i].localPosition;
					solverTransforms[i].position = V3Tools.Lerp(solverTransforms[i].position, GetPosition(i), IKPositionWeight);
					solverTransforms[i].localPosition = Vector3.Project(solverTransforms[i].localPosition, localPosition);
				}
				else
				{
					solverTransforms[i].position = V3Tools.Lerp(solverTransforms[i].position, GetPosition(i), IKPositionWeight);
				}
			}
			solverTransforms[i].rotation = QuaTools.Lerp(solverTransforms[i].rotation, GetRotation(i), IKPositionWeight);
		}
	}

	private void Read(Vector3[] positions, Quaternion[] rotations, bool hasChest, bool hasNeck, bool hasShoulders, bool hasToes, bool hasLegs, bool hasArms)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		if (rootBone == null)
		{
			rootBone = new VirtualBone(positions[0], rotations[0]);
		}
		else
		{
			rootBone.Read(positions[0], rotations[0]);
		}
		spine.Read(positions, rotations, hasChest, hasNeck, hasShoulders, hasToes, hasLegs, 0, 1);
		if (hasArms)
		{
			leftArm.Read(positions, rotations, hasChest, hasNeck, hasShoulders, hasToes, hasLegs, hasChest ? 3 : 2, 6);
			rightArm.Read(positions, rotations, hasChest, hasNeck, hasShoulders, hasToes, hasLegs, hasChest ? 3 : 2, 10);
		}
		if (hasLegs)
		{
			leftLeg.Read(positions, rotations, hasChest, hasNeck, hasShoulders, hasToes, hasLegs, 1, 14);
			rightLeg.Read(positions, rotations, hasChest, hasNeck, hasShoulders, hasToes, hasLegs, 1, 18);
		}
		for (int i = 0; i < rotations.Length; i++)
		{
			solvedPositions[i] = positions[i];
			solvedRotations[i] = rotations[i];
		}
		if (!initiated)
		{
			if (hasLegs)
			{
				legs = new Leg[2] { leftLeg, rightLeg };
			}
			if (hasArms)
			{
				arms = new Arm[2] { leftArm, rightArm };
			}
			if (hasLegs)
			{
				locomotion.Initiate(animator, positions, rotations, hasToes, scale);
			}
			raycastOriginPelvis = spine.pelvis.readPosition;
			spine.faceDirection = readRotations[0] * Vector3.forward;
		}
	}

	private void Solve()
	{
		//IL_0809: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0482: Unknown result type (might be due to invalid IL or missing references)
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0497: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04da: Unknown result type (might be due to invalid IL or missing references)
		//IL_04df: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0503: Unknown result type (might be due to invalid IL or missing references)
		//IL_0508: Unknown result type (might be due to invalid IL or missing references)
		//IL_050d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0515: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_052b: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0540: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Unknown result type (might be due to invalid IL or missing references)
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_0549: Unknown result type (might be due to invalid IL or missing references)
		//IL_0564: Unknown result type (might be due to invalid IL or missing references)
		//IL_0569: Unknown result type (might be due to invalid IL or missing references)
		//IL_0571: Unknown result type (might be due to invalid IL or missing references)
		//IL_0573: Unknown result type (might be due to invalid IL or missing references)
		//IL_057a: Unknown result type (might be due to invalid IL or missing references)
		//IL_057f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0587: Unknown result type (might be due to invalid IL or missing references)
		//IL_058c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0592: Unknown result type (might be due to invalid IL or missing references)
		//IL_059c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0606: Unknown result type (might be due to invalid IL or missing references)
		//IL_060d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0619: Unknown result type (might be due to invalid IL or missing references)
		//IL_061e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0624: Unknown result type (might be due to invalid IL or missing references)
		//IL_062a: Unknown result type (might be due to invalid IL or missing references)
		//IL_063a: Unknown result type (might be due to invalid IL or missing references)
		//IL_063f: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0839: Unknown result type (might be due to invalid IL or missing references)
		//IL_0849: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0904: Unknown result type (might be due to invalid IL or missing references)
		//IL_0910: Unknown result type (might be due to invalid IL or missing references)
		//IL_091b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0920: Unknown result type (might be due to invalid IL or missing references)
		//IL_0925: Unknown result type (might be due to invalid IL or missing references)
		//IL_071e: Unknown result type (might be due to invalid IL or missing references)
		//IL_072e: Unknown result type (might be due to invalid IL or missing references)
		//IL_077c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0959: Unknown result type (might be due to invalid IL or missing references)
		//IL_096e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0973: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b4: Unknown result type (might be due to invalid IL or missing references)
		if (scale <= 0f)
		{
			Debug.LogError((object)"VRIK solver scale <= 0, can not solve!");
			return;
		}
		if (lastLocomotionWeight <= 0f && locomotion.weight > 0f)
		{
			locomotion.Reset(readPositions, readRotations);
		}
		this.spine.SetLOD(LOD);
		if (hasArms)
		{
			Arm[] array = arms;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetLOD(LOD);
			}
		}
		if (hasLegs)
		{
			Leg[] array2 = legs;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].SetLOD(LOD);
			}
		}
		this.spine.PreSolve(scale);
		if (hasArms)
		{
			Arm[] array = arms;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].PreSolve(scale);
			}
		}
		if (hasLegs)
		{
			Leg[] array2 = legs;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].PreSolve(scale);
			}
		}
		if (hasArms)
		{
			Arm[] array = arms;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ApplyOffsets(scale);
			}
		}
		this.spine.ApplyOffsets(scale);
		this.spine.Solve(animator, rootBone, legs, arms, scale);
		if (hasLegs && this.spine.pelvisPositionWeight > 0f && plantFeet)
		{
			Warning.Log("If VRIK 'Pelvis Position Weight' is > 0, 'Plant Feet' should be disabled to improve performance and stability.", root);
		}
		float deltaTime = Time.deltaTime;
		if (hasLegs)
		{
			if (locomotion.weight > 0f)
			{
				switch (locomotion.mode)
				{
				case Locomotion.Mode.Procedural:
				{
					Vector3 leftFootPosition = Vector3.zero;
					Vector3 rightFootPosition = Vector3.zero;
					Quaternion leftFootRotation = Quaternion.identity;
					Quaternion rightFootRotation = Quaternion.identity;
					float leftFootOffset = 0f;
					float rightFootOffset = 0f;
					float leftHeelOffset = 0f;
					float rightHeelOffset = 0f;
					locomotion.Solve_Procedural(rootBone, this.spine, leftLeg, rightLeg, leftArm, rightArm, supportLegIndex, out leftFootPosition, out rightFootPosition, out leftFootRotation, out rightFootRotation, out leftFootOffset, out rightFootOffset, out leftHeelOffset, out rightHeelOffset, scale, deltaTime);
					leftFootPosition += root.up * leftFootOffset;
					rightFootPosition += root.up * rightFootOffset;
					Leg leg = leftLeg;
					leg.footPositionOffset += (leftFootPosition - leftLeg.lastBone.solverPosition) * IKPositionWeight * (1f - leftLeg.positionWeight) * locomotion.weight;
					Leg leg2 = rightLeg;
					leg2.footPositionOffset += (rightFootPosition - rightLeg.lastBone.solverPosition) * IKPositionWeight * (1f - rightLeg.positionWeight) * locomotion.weight;
					Leg leg3 = leftLeg;
					leg3.heelPositionOffset += root.up * leftHeelOffset * locomotion.weight;
					Leg leg4 = rightLeg;
					leg4.heelPositionOffset += root.up * rightHeelOffset * locomotion.weight;
					Quaternion val = QuaTools.FromToRotation(leftLeg.lastBone.solverRotation, leftFootRotation);
					Quaternion val2 = QuaTools.FromToRotation(rightLeg.lastBone.solverRotation, rightFootRotation);
					val = Quaternion.Lerp(Quaternion.identity, val, IKPositionWeight * (1f - leftLeg.rotationWeight) * locomotion.weight);
					val2 = Quaternion.Lerp(Quaternion.identity, val2, IKPositionWeight * (1f - rightLeg.rotationWeight) * locomotion.weight);
					leftLeg.footRotationOffset = val * leftLeg.footRotationOffset;
					rightLeg.footRotationOffset = val2 * rightLeg.footRotationOffset;
					Vector3 point = Vector3.Lerp(leftLeg.position + leftLeg.footPositionOffset, rightLeg.position + rightLeg.footPositionOffset, 0.5f);
					point = V3Tools.PointToPlane(point, rootBone.solverPosition, root.up);
					Vector3 val3 = rootBone.solverPosition + rootVelocity * deltaTime * 2f * locomotion.weight;
					val3 = Vector3.Lerp(val3, point, deltaTime * locomotion.rootSpeed * locomotion.weight);
					rootBone.solverPosition = val3;
					rootVelocity += (point - rootBone.solverPosition) * deltaTime * 10f;
					Vector3 val4 = V3Tools.ExtractVertical(rootVelocity, root.up, 1f);
					rootVelocity -= val4;
					float num = Mathf.Min(leftFootOffset + rightFootOffset, locomotion.maxBodyYOffset * scale);
					bodyOffset = Vector3.Lerp(bodyOffset, root.up * num, deltaTime * 3f);
					bodyOffset = Vector3.Lerp(Vector3.zero, bodyOffset, locomotion.weight);
					break;
				}
				case Locomotion.Mode.Animated:
					if (lastLocomotionWeight <= 0f)
					{
						locomotion.Reset_Animated(readPositions);
					}
					locomotion.Solve_Animated(this, scale, deltaTime);
					break;
				}
			}
			else if (lastLocomotionWeight > 0f)
			{
				locomotion.Reset_Animated(readPositions);
			}
		}
		lastLocomotionWeight = locomotion.weight;
		if (hasLegs)
		{
			Leg[] array2 = legs;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].ApplyOffsets(scale);
			}
			if (!plantFeet || LOD > 0)
			{
				this.spine.InverseTranslateToHead(legs, limited: false, useCurrentLegMag: false, bodyOffset, 1f);
				array2 = legs;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].TranslateRoot(this.spine.pelvis.solverPosition, this.spine.pelvis.solverRotation);
				}
				array2 = legs;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].Solve(stretch: true);
				}
			}
			else
			{
				for (int j = 0; j < 2; j++)
				{
					this.spine.InverseTranslateToHead(legs, limited: true, useCurrentLegMag: true, bodyOffset, 1f);
					array2 = legs;
					for (int i = 0; i < array2.Length; i++)
					{
						array2[i].TranslateRoot(this.spine.pelvis.solverPosition, this.spine.pelvis.solverRotation);
					}
					array2 = legs;
					for (int i = 0; i < array2.Length; i++)
					{
						array2[i].Solve(j == 0);
					}
				}
			}
		}
		else
		{
			this.spine.InverseTranslateToHead(legs, limited: false, useCurrentLegMag: false, bodyOffset, 1f);
		}
		if (hasArms)
		{
			for (int k = 0; k < arms.Length; k++)
			{
				arms[k].TranslateRoot(this.spine.chest.solverPosition, this.spine.chest.solverRotation);
			}
			for (int l = 0; l < arms.Length; l++)
			{
				arms[l].Solve(l == 0);
			}
		}
		this.spine.ResetOffsets();
		if (hasLegs)
		{
			Leg[] array2 = legs;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].ResetOffsets();
			}
		}
		if (hasArms)
		{
			Arm[] array = arms;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ResetOffsets();
			}
		}
		if (hasLegs)
		{
			Spine spine = this.spine;
			spine.pelvisPositionOffset += GetPelvisOffset(deltaTime);
			Spine spine2 = this.spine;
			spine2.chestPositionOffset += this.spine.pelvisPositionOffset;
		}
		Write();
		if (!hasLegs)
		{
			return;
		}
		supportLegIndex = -1;
		float num2 = float.PositiveInfinity;
		for (int m = 0; m < legs.Length; m++)
		{
			float num3 = Vector3.SqrMagnitude(legs[m].lastBone.solverPosition - legs[m].bones[0].solverPosition);
			if (num3 < num2)
			{
				supportLegIndex = m;
				num2 = num3;
			}
		}
	}

	private Vector3 GetPosition(int index)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return solvedPositions[index];
	}

	private Quaternion GetRotation(int index)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return solvedRotations[index];
	}

	private void Write()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		solvedPositions[0] = rootBone.solverPosition;
		solvedRotations[0] = rootBone.solverRotation;
		spine.Write(ref solvedPositions, ref solvedRotations);
		if (hasLegs)
		{
			Leg[] array = legs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Write(ref solvedPositions, ref solvedRotations);
			}
		}
		if (hasArms)
		{
			Arm[] array2 = arms;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Write(ref solvedPositions, ref solvedRotations);
			}
		}
	}

	private Vector3 GetPelvisOffset(float deltaTime)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		if (locomotion.weight <= 0f)
		{
			return Vector3.zero;
		}
		if (LayerMask.op_Implicit(locomotion.blockingLayers) == -1)
		{
			return Vector3.zero;
		}
		Vector3 val = raycastOriginPelvis;
		val.y = spine.pelvis.solverPosition.y;
		Vector3 val2 = spine.pelvis.readPosition;
		val2.y = spine.pelvis.solverPosition.y;
		Vector3 val3 = val2 - val;
		RaycastHit val4 = default;
		if (locomotion.raycastRadius <= 0f)
		{
			if (Physics.Raycast(val, val3, ref val4, val3.magnitude * 1.1f, LayerMask.op_Implicit(locomotion.blockingLayers)))
			{
				val2 = val4.point;
			}
		}
		else if (Physics.SphereCast(val, locomotion.raycastRadius * 1.1f, val3, ref val4, val3.magnitude, LayerMask.op_Implicit(locomotion.blockingLayers)))
		{
			val2 = val + val3.normalized * val4.distance / 1.1f;
		}
		Vector3 val5 = spine.pelvis.solverPosition;
		val3 = val5 - val2;
		if (locomotion.raycastRadius <= 0f)
		{
			if (Physics.Raycast(val2, val3, ref val4, val3.magnitude, LayerMask.op_Implicit(locomotion.blockingLayers)))
			{
				val5 = val4.point;
			}
		}
		else if (Physics.SphereCast(val2, locomotion.raycastRadius, val3, ref val4, val3.magnitude, LayerMask.op_Implicit(locomotion.blockingLayers)))
		{
			val5 = val2 + val3.normalized * val4.distance;
		}
		lastOffset = Vector3.Lerp(lastOffset, Vector3.zero, deltaTime * 3f);
		val5 += Vector3.ClampMagnitude(lastOffset, 0.75f);
		val5.y = spine.pelvis.solverPosition.y;
		lastOffset = Vector3.Lerp(lastOffset, val5 - spine.pelvis.solverPosition, deltaTime * 15f);
		return lastOffset;
	}
}
