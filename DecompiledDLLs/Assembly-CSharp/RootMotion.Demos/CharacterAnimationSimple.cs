using UnityEngine;

namespace RootMotion.Demos;

public class CharacterAnimationSimple : CharacterAnimationBase
{
	public CharacterThirdPerson characterController;

	public float pivotOffset;

	public AnimationCurve moveSpeed;

	private Animator animator;

	protected override void Start()
	{
		base.Start();
		animator = ((Component)this).GetComponentInChildren<Animator>();
	}

	public override Vector3 GetPivotPoint()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (pivotOffset == 0f)
		{
			return ((Component)this).transform.position;
		}
		return ((Component)this).transform.position + ((Component)this).transform.forward * pivotOffset;
	}

	private void Update()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		float num = moveSpeed.Evaluate(characterController.animState.moveDirection.z);
		animator.SetFloat("Speed", num);
		characterController.Move(((Component)characterController).transform.forward * Time.deltaTime * num, Quaternion.identity);
	}
}
