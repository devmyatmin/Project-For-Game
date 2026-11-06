using UnityEngine;

namespace RootMotion.Demos;

public class UserControlAI : UserControlThirdPerson
{
	public Transform moveTarget;

	public float stoppingDistance = 0.5f;

	public float stoppingThreshold = 1.5f;

	public Navigator navigator;

	protected override void Start()
	{
		base.Start();
		navigator.Initiate(((Component)this).transform);
	}

	protected override void Update()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		float num = (walkByDefault ? 0.5f : 1f);
		if (navigator.activeTargetSeeking)
		{
			navigator.Update(moveTarget.position);
			state.move = navigator.normalizedDeltaPosition * num;
			return;
		}
		Vector3 val = moveTarget.position - ((Component)this).transform.position;
		float magnitude = val.magnitude;
		Vector3 up = ((Component)this).transform.up;
		Vector3.OrthoNormalize(ref up, ref val);
		float num2 = ((state.move != Vector3.zero) ? stoppingDistance : (stoppingDistance * stoppingThreshold));
		state.move = ((magnitude > num2) ? (val * num) : Vector3.zero);
		state.lookPos = moveTarget.position;
	}

	private void OnDrawGizmos()
	{
		if (navigator.activeTargetSeeking)
		{
			navigator.Visualize();
		}
	}
}
