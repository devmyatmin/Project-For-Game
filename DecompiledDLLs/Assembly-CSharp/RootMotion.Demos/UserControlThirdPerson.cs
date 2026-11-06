using UnityEngine;

namespace RootMotion.Demos;

public class UserControlThirdPerson : MonoBehaviour
{
	public struct State
	{
		public Vector3 move;

		public Vector3 lookPos;

		public bool crouch;

		public bool jump;

		public int actionIndex;
	}

	public bool walkByDefault;

	public bool canCrouch = true;

	public bool canJump = true;

	public State state;

	protected Transform cam;

	protected virtual void Start()
	{
		cam = ((Component)Camera.main).transform;
	}

	protected virtual void Update()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		state.crouch = canCrouch && Input.GetKey((KeyCode)99);
		state.jump = canJump && Input.GetButton("Jump");
		float axisRaw = Input.GetAxisRaw("Horizontal");
		float axisRaw2 = Input.GetAxisRaw("Vertical");
		Quaternion rotation = cam.rotation;
		Vector3 val = new Vector3(axisRaw, 0f, axisRaw2);
		Vector3 val2 = rotation * val.normalized;
		if (val2 != Vector3.zero)
		{
			Vector3 up = ((Component)this).transform.up;
			Vector3.OrthoNormalize(ref up, ref val2);
			state.move = val2;
		}
		else
		{
			state.move = Vector3.zero;
		}
		bool key = Input.GetKey((KeyCode)304);
		float num;
		if (walkByDefault)
		{
			num = (key ? 1f : 0.5f);
		}
		else
		{
			num = (key ? 0.5f : 1f);
		}
		ref Vector3 move = ref state.move;
		move *= num;
		state.lookPos = ((Component)this).transform.position + cam.forward * 100f;
	}
}
