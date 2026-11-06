using UnityEngine;

namespace RootMotion.FinalIK;

public class LookAtController : MonoBehaviour
{
	public LookAtIK ik;

	[Header("Target Smoothing")]
	[Tooltip("The target to look at. Do not use the Target transform that is assigned to LookAtIK. Set to null if you wish to stop looking.")]
	public Transform target;

	[Range(0f, 1f)]
	public float weight = 1f;

	public Vector3 offset;

	[Tooltip("The time it takes to switch targets.")]
	public float targetSwitchSmoothTime = 0.3f;

	[Tooltip("The time it takes to blend in/out of LookAtIK weight.")]
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

	[Tooltip("The position of the pivot that the look at target is rotated around relative to the root of the character.")]
	public Vector3 pivotOffsetFromRoot = Vector3.up;

	[Tooltip("Minimum distance of looking from the first bone. Keeps the solver from failing if the target is too close.")]
	public float minDistance = 1f;

	[Header("RootRotation")]
	[Tooltip("Character root will be rotate around the Y axis to keep root forward within this angle from the look direction.")]
	[Range(0f, 180f)]
	public float maxRootAngle = 45f;

	private Transform lastTarget;

	private float switchWeight;

	private float switchWeightV;

	private float weightV;

	private Vector3 lastPosition;

	private Vector3 dir;

	private bool lastSmoothTowardsTarget;

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
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
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
		if (!(ik.solver.IKPositionWeight <= 0f))
		{
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
				dir = Vector3.Slerp(dir, val, Time.deltaTime * slerpSpeed);
				dir = Vector3.RotateTowards(dir, val, Time.deltaTime * maxRadiansDelta, maxMagnitudeDelta);
				ik.solver.IKPosition = pivot + dir;
			}
			ApplyMinDistance();
			RootRotation();
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
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Lerp(180f, maxRootAngle, ik.solver.IKPositionWeight);
		if (num < 180f)
		{
			Vector3 val = Quaternion.Inverse(((Component)ik).transform.rotation) * (ik.solver.IKPosition - pivot);
			float num2 = Mathf.Atan2(val.x, val.z) * 57.29578f;
			float num3 = 0f;
			if (num2 > num)
			{
				num3 = num2 - num;
			}
			if (num2 < 0f - num)
			{
				num3 = num2 + num;
			}
			((Component)ik).transform.rotation = Quaternion.AngleAxis(num3, ((Component)ik).transform.up) * ((Component)ik).transform.rotation;
		}
	}

	public LookAtController()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
	}
}
