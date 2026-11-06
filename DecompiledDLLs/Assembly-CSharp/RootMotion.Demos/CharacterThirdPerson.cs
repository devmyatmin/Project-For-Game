using System;
using System.Collections;
using UnityEngine;

namespace RootMotion.Demos;

public class CharacterThirdPerson : CharacterBase
{
	[Serializable]
	public enum MoveMode
	{
		Directional,
		Strafe
	}

	public struct AnimState
	{
		public Vector3 moveDirection;

		public bool jump;

		public bool crouch;

		public bool onGround;

		public bool isStrafing;

		public float yVelocity;

		public bool doubleJump;
	}

	[Header("References")]
	public CharacterAnimationBase characterAnimation;

	public UserControlThirdPerson userControl;

	public CameraController cam;

	[Header("Movement")]
	public MoveMode moveMode;

	public bool smoothPhysics = true;

	public float smoothAccelerationTime = 0.2f;

	public float linearAccelerationSpeed = 3f;

	public float platformFriction = 7f;

	public float groundStickyEffect = 4f;

	public float maxVerticalVelocityOnGround = 3f;

	public float velocityToGroundTangentWeight;

	[Header("Rotation")]
	public bool lookInCameraDirection;

	public float turnSpeed = 5f;

	public float stationaryTurnSpeedMlp = 1f;

	[Header("Jumping and Falling")]
	public bool smoothJump = true;

	public float airSpeed = 6f;

	public float airControl = 2f;

	public float jumpPower = 12f;

	public float jumpRepeatDelayTime;

	public bool doubleJumpEnabled;

	public float doubleJumpPowerMlp = 1f;

	[Header("Wall Running")]
	public LayerMask wallRunLayers;

	public float wallRunMaxLength = 1f;

	public float wallRunMinMoveMag = 0.6f;

	public float wallRunMinVelocityY = -1f;

	public float wallRunRotationSpeed = 1.5f;

	public float wallRunMaxRotationAngle = 70f;

	public float wallRunWeightSpeed = 5f;

	[Header("Crouching")]
	public float crouchCapsuleScaleMlp = 0.6f;

	public AnimState animState;

	protected Vector3 moveDirection;

	private Animator animator;

	private Vector3 normal;

	private Vector3 platformVelocity;

	private Vector3 platformAngularVelocity;

	private RaycastHit hit;

	private float jumpLeg;

	private float jumpEndTime;

	private float forwardMlp;

	private float groundDistance;

	private float lastAirTime;

	private float stickyForce;

	private Vector3 wallNormal = Vector3.up;

	private Vector3 moveDirectionVelocity;

	private float wallRunWeight;

	private float lastWallRunWeight;

	private float fixedDeltaTime;

	private Vector3 fixedDeltaPosition;

	private Quaternion fixedDeltaRotation = Quaternion.identity;

	private bool fixedFrame;

	private float wallRunEndTime;

	private Vector3 gravity;

	private Vector3 verticalVelocity;

	private float velocityY;

	private bool doubleJumped;

	private bool jumpReleased;

	public bool fullRootMotion { get; set; }

	public bool onGround { get; private set; }

	protected override void Start()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		base.Start();
		animator = ((Component)this).GetComponent<Animator>();
		if ((Object)(object)animator == (Object)null)
		{
			animator = ((Component)characterAnimation).GetComponent<Animator>();
		}
		wallNormal = -gravity.normalized;
		onGround = true;
		animState.onGround = true;
		if ((Object)(object)cam != (Object)null)
		{
			((Behaviour)cam).enabled = false;
		}
	}

	private void OnAnimatorMove()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Move(animator.deltaPosition, animator.deltaRotation);
	}

	public override void Move(Vector3 deltaPosition, Quaternion deltaRotation)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		fixedDeltaTime += Time.deltaTime;
		fixedDeltaPosition += deltaPosition;
		fixedDeltaRotation *= deltaRotation;
	}

	private void FixedUpdate()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		gravity = (fullRootMotion ? Vector3.zero : GetGravity());
		verticalVelocity = V3Tools.ExtractVertical(r.velocity, gravity, 1f);
		velocityY = verticalVelocity.magnitude;
		if (Vector3.Dot(verticalVelocity, gravity) > 0f)
		{
			velocityY = 0f - velocityY;
		}
		r.interpolation = (RigidbodyInterpolation)(smoothPhysics ? 1 : 0);
		characterAnimation.smoothFollow = smoothPhysics;
		MoveFixed(fixedDeltaPosition);
		fixedDeltaTime = 0f;
		fixedDeltaPosition = Vector3.zero;
		r.MoveRotation(((Component)this).transform.rotation * fixedDeltaRotation);
		fixedDeltaRotation = Quaternion.identity;
		Rotate();
		GroundCheck();
		if (userControl.state.move == Vector3.zero && groundDistance < airborneThreshold * 0.5f)
		{
			HighFriction();
		}
		else
		{
			ZeroFriction();
		}
		int num;
		if (!fullRootMotion && onGround && userControl.state.move == Vector3.zero)
		{
			Vector3 velocity = r.velocity;
			if (velocity.magnitude < 0.5f)
			{
				num = ((groundDistance < airborneThreshold * 0.5f) ? 1 : 0);
				goto IL_018f;
			}
		}
		num = 0;
		goto IL_018f;
		IL_018f:
		bool flag = (byte)num != 0;
		if ((Object)(object)gravityTarget != (Object)null)
		{
			r.useGravity = false;
			if (!flag)
			{
				r.AddForce(gravity);
			}
		}
		if (flag)
		{
			r.useGravity = false;
			r.velocity = Vector3.zero;
		}
		else if ((Object)(object)gravityTarget == (Object)null)
		{
			r.useGravity = true;
		}
		if (onGround)
		{
			animState.jump = Jump();
			jumpReleased = false;
			doubleJumped = false;
		}
		else
		{
			if (!userControl.state.jump)
			{
				jumpReleased = true;
			}
			if (jumpReleased && userControl.state.jump && !doubleJumped && doubleJumpEnabled)
			{
				jumpEndTime = Time.time + 0.1f;
				animState.doubleJump = true;
				Vector3 velocity2 = userControl.state.move * airSpeed;
				r.velocity = velocity2;
				Rigidbody val = r;
				val.velocity += ((Component)this).transform.up * jumpPower * doubleJumpPowerMlp;
				doubleJumped = true;
			}
		}
		ScaleCapsule(userControl.state.crouch ? crouchCapsuleScaleMlp : 1f);
		fixedFrame = true;
	}

	protected virtual void Update()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		animState.onGround = onGround;
		animState.moveDirection = GetMoveDirection();
		animState.yVelocity = Mathf.Lerp(animState.yVelocity, velocityY, Time.deltaTime * 10f);
		animState.crouch = userControl.state.crouch;
		animState.isStrafing = moveMode == MoveMode.Strafe;
	}

	protected virtual void LateUpdate()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)cam == (Object)null))
		{
			cam.UpdateInput();
			if (fixedFrame || (int)r.interpolation != 0)
			{
				cam.UpdateTransform(((int)r.interpolation == 0) ? Time.fixedDeltaTime : Time.deltaTime);
				fixedFrame = false;
			}
		}
	}

	private void MoveFixed(Vector3 deltaPosition)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		WallRun();
		Vector3 val = ((fixedDeltaTime > 0f) ? (deltaPosition / fixedDeltaTime) : Vector3.zero);
		if (!fullRootMotion)
		{
			val += V3Tools.ExtractHorizontal(platformVelocity, gravity, 1f);
			if (onGround)
			{
				if (velocityToGroundTangentWeight > 0f)
				{
					Quaternion val2 = Quaternion.FromToRotation(((Component)this).transform.up, normal);
					val = Quaternion.Lerp(Quaternion.identity, val2, velocityToGroundTangentWeight) * val;
				}
			}
			else
			{
				Vector3 val3 = V3Tools.ExtractHorizontal(userControl.state.move * airSpeed, gravity, 1f);
				val = Vector3.Lerp(r.velocity, val3, Time.deltaTime * airControl);
			}
			if (onGround && Time.time > jumpEndTime)
			{
				r.velocity -= ((Component)this).transform.up * stickyForce * Time.deltaTime;
			}
			Vector3 val4 = V3Tools.ExtractVertical(r.velocity, gravity, 1f);
			Vector3 val5 = V3Tools.ExtractHorizontal(val, gravity, 1f);
			if (onGround && Vector3.Dot(val4, gravity) < 0f)
			{
				val4 = Vector3.ClampMagnitude(val4, maxVerticalVelocityOnGround);
			}
			r.velocity = val5 + val4;
		}
		else
		{
			r.velocity = val;
		}
		forwardMlp = 1f;
	}

	private void WallRun()
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		bool flag = CanWallRun();
		if (wallRunWeight > 0f && !flag)
		{
			wallRunEndTime = Time.time;
		}
		if (Time.time < wallRunEndTime + 0.5f)
		{
			flag = false;
		}
		wallRunWeight = Mathf.MoveTowards(wallRunWeight, flag ? 1f : 0f, Time.deltaTime * wallRunWeightSpeed);
		if (wallRunWeight <= 0f && lastWallRunWeight > 0f)
		{
			Vector3 val = V3Tools.ExtractHorizontal(((Component)this).transform.forward, gravity, 1f);
			((Component)this).transform.rotation = Quaternion.LookRotation(val, -gravity);
			wallNormal = -gravity.normalized;
		}
		lastWallRunWeight = wallRunWeight;
		if (!(wallRunWeight <= 0f))
		{
			if (onGround && velocityY < 0f)
			{
				r.velocity = V3Tools.ExtractHorizontal(r.velocity, gravity, 1f);
			}
			Vector3 val2 = V3Tools.ExtractHorizontal(((Component)this).transform.forward, gravity, 1f);
			RaycastHit val3 = default;
			val3.normal = -gravity.normalized;
			Vector3 val4;
			if (!onGround)
			{
				Bounds bounds = ((Collider)capsule).bounds;
				val4 = bounds.center;
			}
			else
			{
				val4 = ((Component)this).transform.position;
			}
			Physics.Raycast(val4, val2, ref val3, 3f, LayerMask.op_Implicit(wallRunLayers));
			wallNormal = Vector3.Lerp(wallNormal, val3.normal, Time.deltaTime * wallRunRotationSpeed);
			wallNormal = Vector3.RotateTowards(-gravity.normalized, wallNormal, wallRunMaxRotationAngle * ((float)Math.PI / 180f), 0f);
			Vector3 forward = ((Component)this).transform.forward;
			Vector3 val5 = wallNormal;
			Vector3.OrthoNormalize(ref val5, ref forward);
			((Component)this).transform.rotation = Quaternion.Slerp(Quaternion.LookRotation(val2, -gravity), Quaternion.LookRotation(forward, wallNormal), wallRunWeight);
		}
	}

	private bool CanWallRun()
	{
		if (fullRootMotion)
		{
			return false;
		}
		if (Time.time < jumpEndTime - 0.1f)
		{
			return false;
		}
		if (Time.time > jumpEndTime - 0.1f + wallRunMaxLength)
		{
			return false;
		}
		if (velocityY < wallRunMinVelocityY)
		{
			return false;
		}
		if (userControl.state.move.magnitude < wallRunMinMoveMag)
		{
			return false;
		}
		return true;
	}

	private Vector3 GetMoveDirection()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		switch (moveMode)
		{
		case MoveMode.Directional:
			moveDirection = Vector3.SmoothDamp(moveDirection, new Vector3(0f, 0f, userControl.state.move.magnitude), ref moveDirectionVelocity, smoothAccelerationTime);
			moveDirection = Vector3.MoveTowards(moveDirection, new Vector3(0f, 0f, userControl.state.move.magnitude), Time.deltaTime * linearAccelerationSpeed);
			return moveDirection * forwardMlp;
		case MoveMode.Strafe:
			moveDirection = Vector3.SmoothDamp(moveDirection, userControl.state.move, ref moveDirectionVelocity, smoothAccelerationTime);
			moveDirection = Vector3.MoveTowards(moveDirection, userControl.state.move, Time.deltaTime * linearAccelerationSpeed);
			return ((Component)this).transform.InverseTransformDirection(moveDirection);
		default:
			return Vector3.zero;
		}
	}

	protected virtual void Rotate()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)gravityTarget != (Object)null)
		{
			r.MoveRotation(Quaternion.FromToRotation(((Component)this).transform.up, ((Component)this).transform.position - gravityTarget.position) * ((Component)this).transform.rotation);
		}
		if (platformAngularVelocity != Vector3.zero)
		{
			r.MoveRotation(Quaternion.Euler(platformAngularVelocity) * ((Component)this).transform.rotation);
		}
		float num = GetAngleFromForward(GetForwardDirection());
		if (userControl.state.move == Vector3.zero)
		{
			num *= (1.01f - Mathf.Abs(num) / 180f) * stationaryTurnSpeedMlp;
		}
		r.MoveRotation(Quaternion.AngleAxis(num * Time.deltaTime * turnSpeed, ((Component)this).transform.up) * r.rotation);
	}

	private Vector3 GetForwardDirection()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		bool flag = userControl.state.move != Vector3.zero;
		switch (moveMode)
		{
		case MoveMode.Directional:
			if (flag)
			{
				return userControl.state.move;
			}
			if (!lookInCameraDirection)
			{
				return ((Component)this).transform.forward;
			}
			return userControl.state.lookPos - r.position;
		case MoveMode.Strafe:
			if (flag)
			{
				return userControl.state.lookPos - r.position;
			}
			if (!lookInCameraDirection)
			{
				return ((Component)this).transform.forward;
			}
			return userControl.state.lookPos - r.position;
		default:
			return Vector3.zero;
		}
	}

	protected virtual bool Jump()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		if (!userControl.state.jump)
		{
			return false;
		}
		if (userControl.state.crouch)
		{
			return false;
		}
		if (!characterAnimation.animationGrounded)
		{
			return false;
		}
		if (Time.time < lastAirTime + jumpRepeatDelayTime)
		{
			return false;
		}
		onGround = false;
		jumpEndTime = Time.time + 0.1f;
		Vector3 val = userControl.state.move * airSpeed;
		val += ((Component)this).transform.up * jumpPower;
		if (smoothJump)
		{
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(JumpSmooth(val - r.velocity));
		}
		else
		{
			r.velocity = val;
		}
		return true;
	}

	private IEnumerator JumpSmooth(Vector3 jumpVelocity)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		int steps = 0;
		int stepsToTake = 3;
		while (steps < stepsToTake)
		{
			r.AddForce(jumpVelocity / (float)stepsToTake, (ForceMode)2);
			steps++;
			yield return (object)new WaitForFixedUpdate();
		}
	}

	private void GroundCheck()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		platformAngularVelocity = Vector3.zero;
		float num = 0f;
		hit = GetSpherecastHit();
		normal = ((Component)this).transform.up;
		Vector3 val2 = Vector3.Project(r.position - hit.point, ((Component)this).transform.up);
		groundDistance = val2.magnitude;
		if (Time.time > jumpEndTime && velocityY < jumpPower * 0.5f)
		{
			bool flag = onGround;
			onGround = false;
			float num2 = ((!flag) ? (airborneThreshold * 0.5f) : airborneThreshold);
			Vector3 val3 = V3Tools.ExtractHorizontal(r.velocity, gravity, 1f);
			float magnitude = val3.magnitude;
			if (groundDistance < num2)
			{
				num = groundStickyEffect * magnitude * num2;
				if ((Object)(object)hit.rigidbody != (Object)null)
				{
					val = hit.rigidbody.GetPointVelocity(hit.point);
					platformAngularVelocity = Vector3.Project(hit.rigidbody.angularVelocity, ((Component)this).transform.up);
				}
				onGround = true;
			}
		}
		platformVelocity = Vector3.Lerp(platformVelocity, val, Time.deltaTime * platformFriction);
		if (fullRootMotion)
		{
			stickyForce = 0f;
		}
		stickyForce = num;
		if (!onGround)
		{
			lastAirTime = Time.time;
		}
	}

	public CharacterThirdPerson()
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
	}
}
