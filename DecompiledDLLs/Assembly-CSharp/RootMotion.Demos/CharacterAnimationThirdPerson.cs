using UnityEngine;

namespace RootMotion.Demos;

public class CharacterAnimationThirdPerson : CharacterAnimationBase
{
	public CharacterThirdPerson characterController;

	[SerializeField]
	private float turnSensitivity = 0.2f;

	[SerializeField]
	private float turnSpeed = 5f;

	[SerializeField]
	private float runCycleLegOffset = 0.2f;

	[Range(0.1f, 3f)]
	[SerializeField]
	private float animSpeedMultiplier = 1f;

	protected Animator animator;

	private Vector3 lastForward;

	private const string groundedDirectional = "Grounded Directional";

	private const string groundedStrafe = "Grounded Strafe";

	private float deltaAngle;

	private float jumpLeg;

	private bool lastJump;

	public override bool animationGrounded
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
			if (!currentAnimatorStateInfo.IsName("Grounded Directional"))
			{
				currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
				return currentAnimatorStateInfo.IsName("Grounded Strafe");
			}
			return true;
		}
	}

	protected override void Start()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		base.Start();
		animator = ((Component)this).GetComponent<Animator>();
		lastForward = ((Component)this).transform.forward;
	}

	public override Vector3 GetPivotPoint()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return animator.pivotPosition;
	}

	protected virtual void Update()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (Time.deltaTime != 0f)
		{
			animatePhysics = (int)animator.updateMode == 1;
			if (characterController.animState.jump && !lastJump)
			{
				AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
				float num = (float)((Mathf.Repeat(currentAnimatorStateInfo.normalizedTime + runCycleLegOffset, 1f) < 0.5f) ? 1 : (-1)) * characterController.animState.moveDirection.z;
				animator.SetFloat("JumpLeg", num);
			}
			lastJump = characterController.animState.jump;
			float num2 = 0f - GetAngleFromForward(lastForward) - deltaAngle;
			deltaAngle = 0f;
			lastForward = ((Component)this).transform.forward;
			num2 *= turnSensitivity * 0.01f;
			num2 = Mathf.Clamp(num2 / Time.deltaTime, -1f, 1f);
			animator.SetFloat("Turn", Mathf.Lerp(animator.GetFloat("Turn"), num2, Time.deltaTime * turnSpeed));
			animator.SetFloat("Forward", characterController.animState.moveDirection.z);
			animator.SetFloat("Right", characterController.animState.moveDirection.x);
			animator.SetBool("Crouch", characterController.animState.crouch);
			animator.SetBool("OnGround", characterController.animState.onGround);
			animator.SetBool("IsStrafing", characterController.animState.isStrafing);
			if (!characterController.animState.onGround)
			{
				animator.SetFloat("Jump", characterController.animState.yVelocity);
			}
			if (characterController.doubleJumpEnabled)
			{
				animator.SetBool("DoubleJump", characterController.animState.doubleJump);
			}
			characterController.animState.doubleJump = false;
			if (characterController.animState.onGround && characterController.animState.moveDirection.z > 0f)
			{
				animator.speed = animSpeedMultiplier;
			}
			else
			{
				animator.speed = 1f;
			}
		}
	}

	private void OnAnimatorMove()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = animator.deltaRotation * Vector3.forward;
		deltaAngle += Mathf.Atan2(val.x, val.z) * 57.29578f;
		if (characterController.fullRootMotion)
		{
			Transform transform = ((Component)characterController).transform;
			transform.position += animator.deltaPosition;
			Transform transform2 = ((Component)characterController).transform;
			transform2.rotation *= animator.deltaRotation;
		}
		else
		{
			characterController.Move(animator.deltaPosition, animator.deltaRotation);
		}
	}
}
