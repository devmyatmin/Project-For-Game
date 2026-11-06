using UnityEngine;

namespace RootMotion.Demos;

public class AnimatorController3rdPerson : MonoBehaviour
{
	public float rotateSpeed = 7f;

	public float blendSpeed = 10f;

	public float maxAngle = 90f;

	public float moveSpeed = 1.5f;

	public float rootMotionWeight;

	protected Animator animator;

	protected Vector3 moveBlend;

	protected Vector3 moveInput;

	protected Vector3 velocity;

	protected virtual void Start()
	{
		animator = ((Component)this).GetComponent<Animator>();
	}

	private void OnAnimatorMove()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		velocity = Vector3.Lerp(velocity, ((Component)this).transform.rotation * Vector3.ClampMagnitude(moveInput, 1f) * moveSpeed, Time.deltaTime * blendSpeed);
		Transform transform = ((Component)this).transform;
		transform.position += Vector3.Lerp(velocity * Time.deltaTime, animator.deltaPosition, rootMotionWeight);
	}

	public virtual void Move(Vector3 moveInput, bool isMoving, Vector3 faceDirection, Vector3 aimTarget)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		this.moveInput = moveInput;
		Vector3 val = ((Component)this).transform.InverseTransformDirection(faceDirection);
		float num = Mathf.Atan2(val.x, val.z) * 57.29578f;
		float num2 = num * Time.deltaTime * rotateSpeed;
		if (num > maxAngle)
		{
			num2 = Mathf.Clamp(num2, num - maxAngle, num2);
		}
		if (num < 0f - maxAngle)
		{
			num2 = Mathf.Clamp(num2, num2, num + maxAngle);
		}
		((Component)this).transform.Rotate(Vector3.up, num2);
		moveBlend = Vector3.Lerp(moveBlend, moveInput, Time.deltaTime * blendSpeed);
		animator.SetFloat("X", moveBlend.x);
		animator.SetFloat("Z", moveBlend.z);
		animator.SetBool("IsMoving", isMoving);
	}
}
