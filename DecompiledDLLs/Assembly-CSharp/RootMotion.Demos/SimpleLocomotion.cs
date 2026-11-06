using System;
using UnityEngine;

namespace RootMotion.Demos;

public class SimpleLocomotion : MonoBehaviour
{
	[Serializable]
	public enum RotationMode
	{
		Smooth,
		Linear
	}

	[Tooltip("The component that updates the camera.")]
	public CameraController cameraController;

	[Tooltip("Acceleration of movement.")]
	public float accelerationTime = 0.2f;

	[Tooltip("Turning speed.")]
	public float turnTime = 0.2f;

	[Tooltip("If true, will run on left shift, if not will walk on left shift.")]
	public bool walkByDefault = true;

	[Tooltip("Smooth or linear rotation.")]
	public RotationMode rotationMode;

	[Tooltip("Procedural motion speed (if not using root motion).")]
	public float moveSpeed = 3f;

	private Animator animator;

	private float speed;

	private float angleVel;

	private float speedVel;

	private Vector3 linearTargetDirection;

	private CharacterController characterController;

	public bool isGrounded { get; private set; }

	private void Start()
	{
		animator = ((Component)this).GetComponent<Animator>();
		characterController = ((Component)this).GetComponent<CharacterController>();
		((Behaviour)cameraController).enabled = false;
	}

	private void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		isGrounded = ((Component)this).transform.position.y < 0.1f;
		Rotate();
		Move();
	}

	private void LateUpdate()
	{
		cameraController.UpdateInput();
		cameraController.UpdateTransform();
	}

	private void Rotate()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		if (!isGrounded)
		{
			return;
		}
		Vector3 inputVector = GetInputVector();
		if (inputVector == Vector3.zero)
		{
			return;
		}
		Vector3 forward = ((Component)this).transform.forward;
		switch (rotationMode)
		{
		case RotationMode.Smooth:
		{
			Vector3 val = ((Component)cameraController).transform.rotation * inputVector;
			float num = Mathf.Atan2(forward.x, forward.z) * 57.29578f;
			float num2 = Mathf.Atan2(val.x, val.z) * 57.29578f;
			float num3 = Mathf.SmoothDampAngle(num, num2, ref angleVel, turnTime);
			((Component)this).transform.rotation = Quaternion.AngleAxis(num3, Vector3.up);
			break;
		}
		case RotationMode.Linear:
		{
			Vector3 inputVectorRaw = GetInputVectorRaw();
			if (inputVectorRaw != Vector3.zero)
			{
				linearTargetDirection = ((Component)cameraController).transform.rotation * inputVectorRaw;
			}
			forward = Vector3.RotateTowards(forward, linearTargetDirection, Time.deltaTime * (1f / turnTime), 1f);
			forward.y = 0f;
			((Component)this).transform.rotation = Quaternion.LookRotation(forward);
			break;
		}
		}
	}

	private void Move()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		float num;
		if (walkByDefault)
		{
			num = (Input.GetKey((KeyCode)304) ? 1f : 0.5f);
		}
		else
		{
			num = (Input.GetKey((KeyCode)304) ? 0.5f : 1f);
		}
		speed = Mathf.SmoothDamp(speed, num, ref speedVel, accelerationTime);
		Vector3 inputVector = GetInputVector();
		float num2 = inputVector.magnitude * speed;
		animator.SetFloat("Speed", num2);
		if (!animator.hasRootMotion && isGrounded)
		{
			Vector3 val = ((Component)this).transform.forward * num2 * moveSpeed;
			if ((Object)(object)characterController != (Object)null)
			{
				characterController.SimpleMove(val);
				return;
			}
			Transform transform = ((Component)this).transform;
			transform.position += val * Time.deltaTime;
		}
	}

	private Vector3 GetInputVector()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
		val.z += Mathf.Abs(val.x) * 0.05f;
		val.x -= Mathf.Abs(val.z) * 0.05f;
		return val;
	}

	private Vector3 GetInputVectorRaw()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
	}
}
