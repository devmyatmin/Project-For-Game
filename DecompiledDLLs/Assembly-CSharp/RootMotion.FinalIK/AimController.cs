using System.Collections;
using UnityEngine;

namespace RootMotion.FinalIK;

public class AimController : MonoBehaviour
{
	[Tooltip("Reference to the AimIK component.")]
	public AimIK ik;

	[Tooltip("Master weight of the IK solver.")]
	[Range(0f, 1f)]
	public float weight = 1f;

	[Header("Target Smoothing")]
	[Tooltip("The target to aim at. Do not use the Target transform that is assigned to AimIK. Set to null if you wish to stop aiming.")]
	public Transform target;

	[Tooltip("The time it takes to switch targets.")]
	public float targetSwitchSmoothTime = 0.3f;

	[Tooltip("The time it takes to blend in/out of AimIK weight.")]
	public float weightSmoothTime = 0.3f;

	[Header("Turning Towards The Target")]
	[Tooltip("Enables smooth turning towards the target according to the parameters under this header.")]
	public bool smoothTurnTowardsTarget = true;

	[Tooltip("Speed of turning towards the target using Vector3.RotateTowards.")]
	public float maxRadiansDelta = 3f;

	[Tooltip("Speed of moving towards the target using Vector3.RotateTowards.")]
	public float maxMagnitudeDelta = 3f;

	[Tooltip("Speed of slerping towards the target.")]
	public float slerpSpeed = 3f;

	[Tooltip("Smoothing time for turning towards the yaw and pitch of the target using Mathf.SmoothDampAngle. Value of 0 means smooth damping is disabled.")]
	public float smoothDampTime;

	[Tooltip("The position of the pivot that the aim target is rotated around relative to the root of the character.")]
	public Vector3 pivotOffsetFromRoot = Vector3.up;

	[Tooltip("Minimum distance of aiming from the first bone. Keeps the solver from failing if the target is too close.")]
	public float minDistance = 1f;

	[Tooltip("Offset applied to the target in world space. Convenient for scripting aiming inaccuracy.")]
	public Vector3 offset;

	[Header("RootRotation")]
	[Tooltip("Character root will be rotate around the Y axis to keep root forward within this angle from the aiming direction.")]
	[Range(0f, 180f)]
	public float maxRootAngle = 45f;

	[Tooltip("If enabled, aligns the root forward to target direction after 'Max Root Angle' has been exceeded.")]
	public bool turnToTarget;

	[Tooltip("The time of turning towards the target direction if 'Max Root Angle has been exceeded and 'Turn To Target' is enabled.")]
	public float turnToTargetTime = 0.2f;

	[Header("Mode")]
	[Tooltip("If true, AimIK will consider whatever the current direction of the weapon to be the forward aiming direction and work additively on top of that. This enables you to use recoil and reloading animations seamlessly with AimIK. Adjust the Vector3 value below if the weapon is not aiming perfectly forward in the aiming animation clip.")]
	public bool useAnimatedAimDirection;

	[Tooltip("The direction of the animated weapon aiming in character space. Tweak this value to adjust the aiming. 'Use Animated Aim Direction' must be enabled for this property to work.")]
	public Vector3 animatedAimDirection = Vector3.forward;

	private Transform lastTarget;

	private float switchWeight;

	private float switchWeightV;

	private float weightV;

	private Vector3 lastPosition;

	private Vector3 dir;

	private bool lastSmoothTowardsTarget;

	private bool turningToTarget;

	private float turnToTargetMlp = 1f;

	private float turnToTargetMlpV;

	private float yawV;

	private float pitchV;

	private float dirMagV;

	private Vector3 pivot
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)ik).transform.position + ((Component)ik).transform.rotation * pivotOffsetFromRoot;
		}
	}

	private void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		lastPosition = ik.solver.IKPosition;
		dir = ik.solver.IKPosition - pivot;
		ik.solver.target = null;
	}

	private void LateUpdate()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target != (Object)(object)lastTarget)
		{
			if ((Object)(object)lastTarget == (Object)null && (Object)(object)target != (Object)null && ik.solver.IKPositionWeight <= 0f)
			{
				lastPosition = target.position;
				dir = target.position - pivot;
				ik.solver.IKPosition = target.position + offset;
			}
			else
			{
				lastPosition = ik.solver.IKPosition;
				dir = ik.solver.IKPosition - pivot;
			}
			switchWeight = 0f;
			lastTarget = target;
		}
		float num = (((Object)(object)target != (Object)null) ? weight : 0f);
		ik.solver.IKPositionWeight = Mathf.SmoothDamp(ik.solver.IKPositionWeight, num, ref weightV, weightSmoothTime);
		if (ik.solver.IKPositionWeight >= 0.999f && num > ik.solver.IKPositionWeight)
		{
			ik.solver.IKPositionWeight = 1f;
		}
		if (ik.solver.IKPositionWeight <= 0.001f && num < ik.solver.IKPositionWeight)
		{
			ik.solver.IKPositionWeight = 0f;
		}
		if (ik.solver.IKPositionWeight <= 0f)
		{
			return;
		}
		switchWeight = Mathf.SmoothDamp(switchWeight, 1f, ref switchWeightV, targetSwitchSmoothTime);
		if (switchWeight >= 0.999f)
		{
			switchWeight = 1f;
		}
		if ((Object)(object)target != (Object)null)
		{
			ik.solver.IKPosition = Vector3.Lerp(lastPosition, target.position + offset, switchWeight);
		}
		if (smoothTurnTowardsTarget != lastSmoothTowardsTarget)
		{
			dir = ik.solver.IKPosition - pivot;
			lastSmoothTowardsTarget = smoothTurnTowardsTarget;
		}
		if (smoothTurnTowardsTarget)
		{
			Vector3 val = ik.solver.IKPosition - pivot;
			if (slerpSpeed > 0f)
			{
				dir = Vector3.Slerp(dir, val, Time.deltaTime * slerpSpeed);
			}
			if (maxRadiansDelta > 0f || maxMagnitudeDelta > 0f)
			{
				dir = Vector3.RotateTowards(dir, val, Time.deltaTime * maxRadiansDelta, maxMagnitudeDelta);
			}
			if (smoothDampTime > 0f)
			{
				float yaw = V3Tools.GetYaw(dir);
				float yaw2 = V3Tools.GetYaw(val);
				float num2 = Mathf.SmoothDampAngle(yaw, yaw2, ref yawV, smoothDampTime);
				float pitch = V3Tools.GetPitch(dir);
				float pitch2 = V3Tools.GetPitch(val);
				float num3 = Mathf.SmoothDampAngle(pitch, pitch2, ref pitchV, smoothDampTime);
				float num4 = Mathf.SmoothDamp(dir.magnitude, val.magnitude, ref dirMagV, smoothDampTime);
				dir = Quaternion.Euler(num3, num2, 0f) * Vector3.forward * num4;
			}
			ik.solver.IKPosition = pivot + dir;
		}
		ApplyMinDistance();
		RootRotation();
		if (useAnimatedAimDirection)
		{
			ik.solver.axis = ik.solver.transform.InverseTransformVector(((Component)ik).transform.rotation * animatedAimDirection);
		}
	}

	private void ApplyMinDistance()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = pivot;
		Vector3 val2 = ik.solver.IKPosition - val;
		val2 = val2.normalized * Mathf.Max(val2.magnitude, minDistance);
		ik.solver.IKPosition = val + val2;
	}

	private void RootRotation()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Lerp(180f, maxRootAngle * turnToTargetMlp, ik.solver.IKPositionWeight);
		if (!(num < 180f))
		{
			return;
		}
		Vector3 val = Quaternion.Inverse(((Component)ik).transform.rotation) * (ik.solver.IKPosition - pivot);
		float num2 = Mathf.Atan2(val.x, val.z) * 57.29578f;
		float num3 = 0f;
		if (num2 > num)
		{
			num3 = num2 - num;
			if (!turningToTarget && turnToTarget)
			{
				((MonoBehaviour)this).StartCoroutine(TurnToTarget());
			}
		}
		if (num2 < 0f - num)
		{
			num3 = num2 + num;
			if (!turningToTarget && turnToTarget)
			{
				((MonoBehaviour)this).StartCoroutine(TurnToTarget());
			}
		}
		((Component)ik).transform.rotation = Quaternion.AngleAxis(num3, ((Component)ik).transform.up) * ((Component)ik).transform.rotation;
	}

	private IEnumerator TurnToTarget()
	{
		turningToTarget = true;
		while (turnToTargetMlp > 0f)
		{
			turnToTargetMlp = Mathf.SmoothDamp(turnToTargetMlp, 0f, ref turnToTargetMlpV, turnToTargetTime);
			if (turnToTargetMlp < 0.01f)
			{
				turnToTargetMlp = 0f;
			}
			yield return null;
		}
		turnToTargetMlp = 1f;
		turningToTarget = false;
	}

	public AimController()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
	}
}
