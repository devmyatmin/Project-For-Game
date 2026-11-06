using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class CharacterAnimationThirdPersonIK : CharacterAnimationThirdPerson
{
	private FullBodyBipedIK ik;

	protected override void Start()
	{
		base.Start();
		ik = ((Component)this).GetComponent<FullBodyBipedIK>();
	}

	protected override void LateUpdate()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		base.LateUpdate();
		if (!(Vector3.Angle(((Component)this).transform.up, Vector3.up) <= 0.01f))
		{
			Quaternion rotation = Quaternion.FromToRotation(((Component)this).transform.up, Vector3.up);
			RotateEffector(ik.solver.bodyEffector, rotation, 0.1f);
			RotateEffector(ik.solver.leftShoulderEffector, rotation, 0.2f);
			RotateEffector(ik.solver.rightShoulderEffector, rotation, 0.2f);
			RotateEffector(ik.solver.leftHandEffector, rotation, 0.1f);
			RotateEffector(ik.solver.rightHandEffector, rotation, 0.1f);
		}
	}

	private void RotateEffector(IKEffector effector, Quaternion rotation, float mlp)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = effector.bone.position - ((Component)this).transform.position;
		Vector3 val2 = rotation * val - val;
		effector.positionOffset += val2 * mlp;
	}
}
