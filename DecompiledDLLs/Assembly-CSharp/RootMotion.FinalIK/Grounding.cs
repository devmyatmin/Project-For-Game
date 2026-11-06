using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class Grounding
{
	[Serializable]
	public enum Quality
	{
		Fastest,
		Simple,
		Best
	}

	public delegate bool OnRaycastDelegate(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction);

	public delegate bool OnCapsuleCastDelegate(Vector3 point1, Vector3 point2, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction);

	public delegate bool OnSphereCastDelegate(Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction);

	public class Leg
	{
		public Quaternion rotationOffset = Quaternion.identity;

		public bool invertFootCenter;

		private Grounding grounding;

		private float lastTime;

		private float deltaTime;

		private Vector3 lastPosition;

		private Quaternion toHitNormal;

		private Quaternion r;

		private Vector3 up = Vector3.up;

		private bool doOverrideFootPosition;

		private Vector3 overrideFootPosition;

		private Vector3 transformPosition;

		public bool isGrounded { get; private set; }

		public Vector3 IKPosition
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

		public bool initiated { get; private set; }

		public float heightFromGround { get; private set; }

		public Vector3 velocity
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

		public Transform transform { get; private set; }

		public float IKOffset { get; private set; }

		public RaycastHit heelHit
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

		public RaycastHit capsuleHit
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

		public RaycastHit GetHitPoint
		{
			get
			{
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				if (grounding.quality == Quality.Best)
				{
					return capsuleHit;
				}
				return heelHit;
			}
		}

		public float stepHeightFromGround => Mathf.Clamp(heightFromGround, 0f - grounding.maxStep, grounding.maxStep);

		private float rootYOffset
		{
			get
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Unknown result type (might be due to invalid IL or missing references)
				return grounding.GetVerticalOffset(transformPosition, grounding.root.position - up * grounding.heightOffset);
			}
		}

		public void SetFootPosition(Vector3 position)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			doOverrideFootPosition = true;
			overrideFootPosition = position;
		}

		public void Initiate(Grounding grounding, Transform transform)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			initiated = false;
			this.grounding = grounding;
			this.transform = transform;
			up = Vector3.up;
			IKPosition = transform.position;
			rotationOffset = Quaternion.identity;
			initiated = true;
			OnEnable();
		}

		public void OnEnable()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			if (initiated)
			{
				lastPosition = transform.position;
				lastTime = Time.deltaTime;
			}
		}

		public void Reset()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			lastPosition = transform.position;
			lastTime = Time.deltaTime;
			IKOffset = 0f;
			IKPosition = transform.position;
			rotationOffset = Quaternion.identity;
		}

		public void Process()
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_029a: Unknown result type (might be due to invalid IL or missing references)
			//IL_039d: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0402: Unknown result type (might be due to invalid IL or missing references)
			//IL_040d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0412: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02be: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_0448: Unknown result type (might be due to invalid IL or missing references)
			//IL_0433: Unknown result type (might be due to invalid IL or missing references)
			//IL_0439: Unknown result type (might be due to invalid IL or missing references)
			//IL_0440: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_020f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0213: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Unknown result type (might be due to invalid IL or missing references)
			//IL_021f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0225: Unknown result type (might be due to invalid IL or missing references)
			//IL_022a: Unknown result type (might be due to invalid IL or missing references)
			//IL_022e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0233: Unknown result type (might be due to invalid IL or missing references)
			//IL_0238: Unknown result type (might be due to invalid IL or missing references)
			//IL_023d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0241: Unknown result type (might be due to invalid IL or missing references)
			//IL_0246: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_024b: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_044d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0266: Unknown result type (might be due to invalid IL or missing references)
			//IL_0269: Unknown result type (might be due to invalid IL or missing references)
			//IL_026e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0272: Unknown result type (might be due to invalid IL or missing references)
			//IL_0278: Unknown result type (might be due to invalid IL or missing references)
			//IL_027d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0281: Unknown result type (might be due to invalid IL or missing references)
			//IL_025c: Unknown result type (might be due to invalid IL or missing references)
			//IL_025e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0263: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0304: Unknown result type (might be due to invalid IL or missing references)
			//IL_0308: Unknown result type (might be due to invalid IL or missing references)
			//IL_030e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0313: Unknown result type (might be due to invalid IL or missing references)
			//IL_0317: Unknown result type (might be due to invalid IL or missing references)
			//IL_031d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0322: Unknown result type (might be due to invalid IL or missing references)
			//IL_0326: Unknown result type (might be due to invalid IL or missing references)
			if (!initiated || grounding.maxStep <= 0f)
			{
				return;
			}
			transformPosition = (doOverrideFootPosition ? overrideFootPosition : transform.position);
			doOverrideFootPosition = false;
			deltaTime = Time.time - lastTime;
			lastTime = Time.time;
			if (deltaTime == 0f)
			{
				return;
			}
			up = grounding.up;
			heightFromGround = float.PositiveInfinity;
			velocity = (transformPosition - lastPosition) / deltaTime;
			lastPosition = transformPosition;
			Vector3 val = velocity * grounding.prediction;
			if (grounding.footRadius <= 0f)
			{
				grounding.quality = Quality.Fastest;
			}
			isGrounded = false;
			RaycastHit val2;
			Vector3 normal;
			Vector3 point4;
			switch (grounding.quality)
			{
			case Quality.Fastest:
			{
				RaycastHit raycastHit3 = GetRaycastHit(val);
				SetFootToPoint(raycastHit3.normal, raycastHit3.point);
				if ((Object)(object)raycastHit3.collider != (Object)null)
				{
					isGrounded = true;
				}
				break;
			}
			case Quality.Simple:
			{
				heelHit = GetRaycastHit(Vector3.zero);
				Vector3 val3 = grounding.GetFootCenterOffset();
				if (invertFootCenter)
				{
					val3 = -val3;
				}
				RaycastHit raycastHit = GetRaycastHit(val3 + val);
				RaycastHit raycastHit2 = GetRaycastHit(grounding.root.right * grounding.footRadius * 0.5f);
				val2 = heelHit;
				if ((Object)(object)val2.collider != (Object)null || (Object)(object)raycastHit.collider != (Object)null || (Object)(object)raycastHit2.collider != (Object)null)
				{
					isGrounded = true;
				}
				Vector3 point = raycastHit.point;
				val2 = heelHit;
				Vector3 val4 = point - val2.point;
				Vector3 point2 = raycastHit2.point;
				val2 = heelHit;
				Vector3 val5 = Vector3.Cross(val4, point2 - val2.point);
				Vector3 val6 = val5.normalized;
				if (Vector3.Dot(val6, up) < 0f)
				{
					val6 = -val6;
				}
				Vector3 planeNormal = val6;
				val2 = heelHit;
				Vector3 point3 = val2.point;
				val2 = heelHit;
				SetFootToPlane(planeNormal, point3, val2.point);
				break;
			}
			case Quality.Best:
				{
					heelHit = GetRaycastHit(invertFootCenter ? (-grounding.GetFootCenterOffset()) : Vector3.zero);
					capsuleHit = GetCapsuleHit(val);
					val2 = heelHit;
					if (!((Object)(object)val2.collider != (Object)null))
					{
						val2 = capsuleHit;
						if (!((Object)(object)val2.collider != (Object)null))
						{
							goto IL_02fd;
						}
					}
					isGrounded = true;
					goto IL_02fd;
				}
				IL_02fd:
				val2 = capsuleHit;
				normal = val2.normal;
				val2 = capsuleHit;
				point4 = val2.point;
				val2 = heelHit;
				SetFootToPlane(normal, point4, val2.point);
				break;
			}
			float num = stepHeightFromGround;
			if (!grounding.rootGrounded)
			{
				num = 0f;
			}
			IKOffset = Interp.LerpValue(IKOffset, num, grounding.footSpeed, grounding.footSpeed);
			IKOffset = Mathf.Lerp(IKOffset, num, deltaTime * grounding.footSpeed);
			float verticalOffset = grounding.GetVerticalOffset(transformPosition, grounding.root.position);
			float num2 = Mathf.Clamp(grounding.maxStep - verticalOffset, 0f, grounding.maxStep);
			IKOffset = Mathf.Clamp(IKOffset, 0f - num2, IKOffset);
			RotateFoot();
			IKPosition = transformPosition - up * IKOffset;
			float footRotationWeight = grounding.footRotationWeight;
			rotationOffset = ((footRotationWeight >= 1f) ? r : Quaternion.Slerp(Quaternion.identity, r, footRotationWeight));
		}

		private RaycastHit GetCapsuleHit(Vector3 offsetFromHeel)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0193: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			RaycastHit hitInfo = default;
			Vector3 val = grounding.GetFootCenterOffset();
			if (invertFootCenter)
			{
				val = -val;
			}
			Vector3 val2 = transformPosition + val;
			if (grounding.overstepFallsDown)
			{
				hitInfo.point = val2 - up * grounding.maxStep;
			}
			else
			{
				hitInfo.point = new Vector3(val2.x, grounding.root.position.y, val2.z);
			}
			hitInfo.normal = up;
			Vector3 val3 = val2 + grounding.maxStep * up;
			Vector3 point = val3 + offsetFromHeel;
			if (grounding.CapsuleCast(val3, point, grounding.footRadius, -up, out hitInfo, grounding.maxStep * 2f, LayerMask.op_Implicit(grounding.layers), (QueryTriggerInteraction)1) && float.IsNaN(hitInfo.point.x))
			{
				hitInfo.point = val2 - up * grounding.maxStep * 2f;
				hitInfo.normal = up;
			}
			if (hitInfo.point == Vector3.zero && hitInfo.normal == Vector3.zero)
			{
				if (grounding.overstepFallsDown)
				{
					hitInfo.point = val2 - up * grounding.maxStep;
				}
				else
				{
					hitInfo.point = new Vector3(val2.x, grounding.root.position.y, val2.z);
				}
			}
			return hitInfo;
		}

		private RaycastHit GetRaycastHit(Vector3 offsetFromHeel)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			RaycastHit hitInfo = default;
			Vector3 val = transformPosition + offsetFromHeel;
			if (grounding.overstepFallsDown)
			{
				hitInfo.point = val - up * grounding.maxStep;
			}
			else
			{
				hitInfo.point = new Vector3(val.x, grounding.root.position.y, val.z);
			}
			hitInfo.normal = up;
			if (grounding.maxStep <= 0f)
			{
				return hitInfo;
			}
			grounding.Raycast(val + grounding.maxStep * up, -up, out hitInfo, grounding.maxStep * 2f, LayerMask.op_Implicit(grounding.layers), (QueryTriggerInteraction)1);
			if (hitInfo.point == Vector3.zero && hitInfo.normal == Vector3.zero)
			{
				if (grounding.overstepFallsDown)
				{
					hitInfo.point = val - up * grounding.maxStep;
				}
				else
				{
					hitInfo.point = new Vector3(val.x, grounding.root.position.y, val.z);
				}
			}
			return hitInfo;
		}

		private Vector3 RotateNormal(Vector3 normal)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (grounding.quality == Quality.Best)
			{
				return normal;
			}
			return Vector3.RotateTowards(up, normal, grounding.maxFootRotationAngle * ((float)Math.PI / 180f), deltaTime);
		}

		private void SetFootToPoint(Vector3 normal, Vector3 point)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			toHitNormal = Quaternion.FromToRotation(up, RotateNormal(normal));
			heightFromGround = GetHeightFromGround(point);
		}

		private void SetFootToPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 heelHitPoint)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			planeNormal = RotateNormal(planeNormal);
			toHitNormal = Quaternion.FromToRotation(up, planeNormal);
			Vector3 hitPoint = V3Tools.LineToPlane(transformPosition + up * grounding.maxStep, -up, planeNormal, planePoint);
			heightFromGround = GetHeightFromGround(hitPoint);
			float num = GetHeightFromGround(heelHitPoint);
			heightFromGround = Mathf.Clamp(heightFromGround, float.NegativeInfinity, num);
		}

		private float GetHeightFromGround(Vector3 hitPoint)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return grounding.GetVerticalOffset(transformPosition, hitPoint) - rootYOffset;
		}

		private void RotateFoot()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			Quaternion rotationOffsetTarget = GetRotationOffsetTarget();
			r = Quaternion.Slerp(r, rotationOffsetTarget, deltaTime * grounding.footRotationSpeed);
		}

		private Quaternion GetRotationOffsetTarget()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			if (grounding.maxFootRotationAngle <= 0f)
			{
				return Quaternion.identity;
			}
			if (grounding.maxFootRotationAngle >= 180f)
			{
				return toHitNormal;
			}
			return Quaternion.RotateTowards(Quaternion.identity, toHitNormal, grounding.maxFootRotationAngle);
		}

		public Leg()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	public class Pelvis
	{
		private Grounding grounding;

		private Vector3 lastRootPosition;

		private float damperF;

		private bool initiated;

		private float lastTime;

		public Vector3 IKOffset
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

		public float heightOffset { get; private set; }

		public void Initiate(Grounding grounding)
		{
			this.grounding = grounding;
			initiated = true;
			OnEnable();
		}

		public void Reset()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			lastRootPosition = ((Component)grounding.root).transform.position;
			lastTime = Time.deltaTime;
			IKOffset = Vector3.zero;
			heightOffset = 0f;
		}

		public void OnEnable()
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if (initiated)
			{
				lastRootPosition = ((Component)grounding.root).transform.position;
				lastTime = Time.time;
			}
		}

		public void Process(float lowestOffset, float highestOffset, bool isGrounded)
		{
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			if (!initiated)
			{
				return;
			}
			float num = Time.time - lastTime;
			lastTime = Time.time;
			if (!(num <= 0f))
			{
				float num2 = lowestOffset + highestOffset;
				if (!grounding.rootGrounded)
				{
					num2 = 0f;
				}
				heightOffset = Mathf.Lerp(heightOffset, num2, num * grounding.pelvisSpeed);
				Vector3 p = grounding.root.position - lastRootPosition;
				lastRootPosition = grounding.root.position;
				damperF = Interp.LerpValue(damperF, isGrounded ? 1f : 0f, 1f, 10f);
				heightOffset -= grounding.GetVerticalOffset(p, Vector3.zero) * grounding.pelvisDamper * damperF;
				IKOffset = grounding.up * heightOffset;
			}
		}
	}

	[Tooltip("Layers to ground the character to. Make sure to exclude the layer of the character controller.")]
	public LayerMask layers;

	[Tooltip("Max step height. Maximum vertical distance of Grounding from the root of the character.")]
	public float maxStep = 0.5f;

	[Tooltip("The height offset of the root.")]
	public float heightOffset;

	[Tooltip("The speed of moving the feet up/down.")]
	public float footSpeed = 2.5f;

	[Tooltip("CapsuleCast radius. Should match approximately with the size of the feet.")]
	public float footRadius = 0.15f;

	[Tooltip("Offset of the foot center along character forward axis.")]
	[HideInInspector]
	public float footCenterOffset;

	[Tooltip("Amount of velocity based prediction of the foot positions.")]
	public float prediction = 0.05f;

	[Tooltip("Weight of rotating the feet to the ground normal offset.")]
	[Range(0f, 1f)]
	public float footRotationWeight = 1f;

	[Tooltip("Speed of slerping the feet to their grounded rotations.")]
	public float footRotationSpeed = 7f;

	[Tooltip("Max Foot Rotation Angle. Max angular offset from the foot's rotation.")]
	[Range(0f, 90f)]
	public float maxFootRotationAngle = 45f;

	[Tooltip("If true, solver will rotate with the character root so the character can be grounded for example to spherical planets. For performance reasons leave this off unless needed.")]
	public bool rotateSolver;

	[Tooltip("The speed of moving the character up/down.")]
	public float pelvisSpeed = 5f;

	[Tooltip("Used for smoothing out vertical pelvis movement (range 0 - 1).")]
	[Range(0f, 1f)]
	public float pelvisDamper;

	[Tooltip("The weight of lowering the pelvis to the lowest foot.")]
	public float lowerPelvisWeight = 1f;

	[Tooltip("The weight of lifting the pelvis to the highest foot. This is useful when you don't want the feet to go too high relative to the body when crouching.")]
	public float liftPelvisWeight;

	[Tooltip("The radius of the spherecast from the root that determines whether the character root is grounded.")]
	public float rootSphereCastRadius = 0.1f;

	[Tooltip("If false, keeps the foot that is over a ledge at the root level. If true, lowers the overstepping foot and body by the 'Max Step' value.")]
	public bool overstepFallsDown = true;

	[Tooltip("The raycasting quality. Fastest is a single raycast per foot, Simple is three raycasts, Best is one raycast and a capsule cast per foot.")]
	public Quality quality = Quality.Best;

	public OnRaycastDelegate Raycast = Physics.Raycast;

	public OnCapsuleCastDelegate CapsuleCast = Physics.CapsuleCast;

	public OnSphereCastDelegate SphereCast = Physics.SphereCast;

	private bool initiated;

	public Leg[] legs { get; private set; }

	public Pelvis pelvis { get; private set; }

	public bool isGrounded { get; private set; }

	public Transform root { get; private set; }

	public RaycastHit rootHit
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

	public bool rootGrounded
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			RaycastHit val = rootHit;
			return val.distance < maxStep * 2f;
		}
	}

	public Vector3 up
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			if (!useRootRotation)
			{
				return Vector3.up;
			}
			return root.up;
		}
	}

	private bool useRootRotation
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			if (!rotateSolver)
			{
				return false;
			}
			if (root.up == Vector3.up)
			{
				return false;
			}
			return true;
		}
	}

	public RaycastHit GetRootHit(float maxDistanceMlp = 10f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit hitInfo = default;
		Vector3 val = up;
		Vector3 val2 = Vector3.zero;
		Leg[] array = legs;
		foreach (Leg leg in array)
		{
			val2 += leg.transform.position;
		}
		val2 /= (float)legs.Length;
		hitInfo.point = val2 - val * maxStep * 10f;
		float num = maxDistanceMlp + 1f;
		hitInfo.distance = maxStep * num;
		if (maxStep <= 0f)
		{
			return hitInfo;
		}
		if (quality != Quality.Best)
		{
			Raycast(val2 + val * maxStep, -val, out hitInfo, maxStep * num, LayerMask.op_Implicit(layers), (QueryTriggerInteraction)1);
		}
		else
		{
			SphereCast(val2 + val * maxStep, rootSphereCastRadius, -up, out hitInfo, maxStep * num, LayerMask.op_Implicit(layers), (QueryTriggerInteraction)1);
		}
		return hitInfo;
	}

	public bool IsValid(ref string errorMessage)
	{
		if ((Object)(object)root == (Object)null)
		{
			errorMessage = "Root transform is null. Can't initiate Grounding.";
			return false;
		}
		if (legs == null)
		{
			errorMessage = "Grounding legs is null. Can't initiate Grounding.";
			return false;
		}
		if (pelvis == null)
		{
			errorMessage = "Grounding pelvis is null. Can't initiate Grounding.";
			return false;
		}
		if (legs.Length == 0)
		{
			errorMessage = "Grounding has 0 legs. Can't initiate Grounding.";
			return false;
		}
		return true;
	}

	public void Initiate(Transform root, Transform[] feet)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		this.root = root;
		initiated = false;
		rootHit = default;
		if (legs == null)
		{
			legs = new Leg[feet.Length];
		}
		if (legs.Length != feet.Length)
		{
			legs = new Leg[feet.Length];
		}
		for (int i = 0; i < feet.Length; i++)
		{
			if (legs[i] == null)
			{
				legs[i] = new Leg();
			}
		}
		if (pelvis == null)
		{
			pelvis = new Pelvis();
		}
		string errorMessage = string.Empty;
		if (!IsValid(ref errorMessage))
		{
			Warning.Log(errorMessage, root);
		}
		else if (Application.isPlaying)
		{
			for (int j = 0; j < feet.Length; j++)
			{
				legs[j].Initiate(this, feet[j]);
			}
			pelvis.Initiate(this);
			initiated = true;
		}
	}

	public void Update()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		if (!initiated)
		{
			return;
		}
		if (LayerMask.op_Implicit(layers) == 0)
		{
			LogWarning("Grounding layers are set to nothing. Please add a ground layer.");
		}
		maxStep = Mathf.Clamp(maxStep, 0f, maxStep);
		footRadius = Mathf.Clamp(footRadius, 0.0001f, maxStep);
		pelvisDamper = Mathf.Clamp(pelvisDamper, 0f, 1f);
		rootSphereCastRadius = Mathf.Clamp(rootSphereCastRadius, 0.0001f, rootSphereCastRadius);
		maxFootRotationAngle = Mathf.Clamp(maxFootRotationAngle, 0f, 90f);
		prediction = Mathf.Clamp(prediction, 0f, prediction);
		footSpeed = Mathf.Clamp(footSpeed, 0f, footSpeed);
		rootHit = GetRootHit();
		float num = float.NegativeInfinity;
		float num2 = float.PositiveInfinity;
		isGrounded = false;
		Leg[] array = legs;
		foreach (Leg leg in array)
		{
			leg.Process();
			if (leg.IKOffset > num)
			{
				num = leg.IKOffset;
			}
			if (leg.IKOffset < num2)
			{
				num2 = leg.IKOffset;
			}
			if (leg.isGrounded)
			{
				isGrounded = true;
			}
		}
		num = Mathf.Max(num, 0f);
		num2 = Mathf.Min(num2, 0f);
		pelvis.Process((0f - num) * lowerPelvisWeight, (0f - num2) * liftPelvisWeight, isGrounded);
	}

	public Vector3 GetLegsPlaneNormal()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (!initiated)
		{
			return Vector3.up;
		}
		Vector3 val = up;
		Vector3 val2 = val;
		for (int i = 0; i < legs.Length; i++)
		{
			Vector3 val3 = legs[i].IKPosition - root.position;
			Vector3 val4 = val;
			Vector3 val5 = val3;
			Vector3.OrthoNormalize(ref val4, ref val5);
			val2 = Quaternion.FromToRotation(val5, val3) * val2;
		}
		return val2;
	}

	public void Reset()
	{
		if (Application.isPlaying)
		{
			pelvis.Reset();
			Leg[] array = legs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Reset();
			}
		}
	}

	public void LogWarning(string message)
	{
		Warning.Log(message, root);
	}

	public float GetVerticalOffset(Vector3 p1, Vector3 p2)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (useRootRotation)
		{
			return (Quaternion.Inverse(root.rotation) * (p1 - p2)).y;
		}
		return p1.y - p2.y;
	}

	public Vector3 Flatten(Vector3 v)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (useRootRotation)
		{
			Vector3 val = v;
			Vector3 val2 = root.up;
			Vector3.OrthoNormalize(ref val2, ref val);
			return Vector3.Project(v, val);
		}
		v.y = 0f;
		return v;
	}

	public Vector3 GetFootCenterOffset()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		return root.forward * footRadius + root.forward * footCenterOffset;
	}
}
