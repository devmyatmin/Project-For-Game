using UnityEngine;

namespace RootMotion.FinalIK;

public class BodyTilt : OffsetModifier
{
	[Tooltip("Speed of tilting")]
	public float tiltSpeed = 6f;

	[Tooltip("Sensitivity of tilting")]
	public float tiltSensitivity = 0.07f;

	[Tooltip("The OffsetPose components")]
	public OffsetPose poseLeft;

	[Tooltip("The OffsetPose components")]
	public OffsetPose poseRight;

	private float tiltAngle;

	private Vector3 lastForward;

	protected override void Start()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		base.Start();
		lastForward = ((Component)this).transform.forward;
	}

	protected override void OnModifyOffset()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.FromToRotation(lastForward, ((Component)this).transform.forward);
		float num = 0f;
		Vector3 zero = Vector3.zero;
		val.ToAngleAxis(ref num, ref zero);
		if (zero.y > 0f)
		{
			num = 0f - num;
		}
		num *= tiltSensitivity * 0.01f;
		num /= deltaTime;
		num = Mathf.Clamp(num, -1f, 1f);
		tiltAngle = Mathf.Lerp(tiltAngle, num, deltaTime * tiltSpeed);
		float num2 = Mathf.Abs(tiltAngle) / 1f;
		if (tiltAngle < 0f)
		{
			poseRight.Apply(ik.solver, num2);
		}
		else
		{
			poseLeft.Apply(ik.solver, num2);
		}
		lastForward = ((Component)this).transform.forward;
	}
}
