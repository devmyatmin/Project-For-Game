using UnityEngine;

namespace RootMotion.Demos;

public class MotionAbsorbCharacter : MonoBehaviour
{
	public Animator animator;

	public MotionAbsorb motionAbsorb;

	public Transform cube;

	public float cubeRandomPosition = 0.1f;

	public AnimationCurve motionAbsorbWeight;

	private Vector3 cubeDefaultPosition;

	private AnimatorStateInfo info;

	private Rigidbody cubeRigidbody;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		cubeDefaultPosition = cube.position;
		cubeRigidbody = ((Component)cube).GetComponent<Rigidbody>();
	}

	private void Update()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		info = animator.GetCurrentAnimatorStateInfo(0);
		motionAbsorb.weight = motionAbsorbWeight.Evaluate(info.normalizedTime - (float)(int)info.normalizedTime);
	}

	private void SwingStart()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		cubeRigidbody.MovePosition(cubeDefaultPosition + Random.insideUnitSphere * cubeRandomPosition);
		cubeRigidbody.MoveRotation(Quaternion.identity);
		cubeRigidbody.velocity = Vector3.zero;
		cubeRigidbody.angularVelocity = Vector3.zero;
	}
}
