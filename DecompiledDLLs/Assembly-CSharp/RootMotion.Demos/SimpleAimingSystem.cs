using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class SimpleAimingSystem : MonoBehaviour
{
	[Tooltip("AimPoser is a tool that returns an animation name based on direction.")]
	public AimPoser aimPoser;

	[Tooltip("Reference to the AimIK component.")]
	public AimIK aim;

	[Tooltip("Reference to the LookAt component (only used for the head in this instance).")]
	public LookAtIK lookAt;

	[Tooltip("Reference to the Animator component.")]
	public Animator animator;

	[Tooltip("Time of cross-fading from pose to pose.")]
	public float crossfadeTime = 0.2f;

	[Tooltip("Will keep the aim target at a distance.")]
	public float minAimDistance = 0.5f;

	private AimPoser.Pose aimPose;

	private AimPoser.Pose lastPose;

	private void Start()
	{
		((Behaviour)aim).enabled = false;
		((Behaviour)lookAt).enabled = false;
	}

	private void LateUpdate()
	{
		Pose();
		aim.solver.Update();
		if ((Object)(object)lookAt != (Object)null)
		{
			lookAt.solver.Update();
		}
	}

	private void Pose()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		LimitAimTarget();
		Vector3 val = aim.solver.IKPosition - aim.solver.bones[0].transform.position;
		Vector3 localDirection = ((Component)this).transform.InverseTransformDirection(val);
		aimPose = aimPoser.GetPose(localDirection);
		if (aimPose != lastPose)
		{
			aimPoser.SetPoseActive(aimPose);
			lastPose = aimPose;
		}
		AimPoser.Pose[] poses = aimPoser.poses;
		foreach (AimPoser.Pose pose in poses)
		{
			if (pose == aimPose)
			{
				DirectCrossFade(pose.name, 1f);
			}
			else
			{
				DirectCrossFade(pose.name, 0f);
			}
		}
	}

	private void LimitAimTarget()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = aim.solver.bones[0].transform.position;
		Vector3 val = aim.solver.IKPosition - position;
		val = val.normalized * Mathf.Max(val.magnitude, minAimDistance);
		aim.solver.IKPosition = position + val;
	}

	private void DirectCrossFade(string state, float target)
	{
		float num = Mathf.MoveTowards(animator.GetFloat(state), target, Time.deltaTime * (1f / crossfadeTime));
		animator.SetFloat(state, num);
	}
}
