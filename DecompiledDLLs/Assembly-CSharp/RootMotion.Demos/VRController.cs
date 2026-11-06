using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class VRController : MonoBehaviour
{
	[Serializable]
	public enum InputMode
	{
		Input,
		WASDOnly
	}

	public InputMode inputMode;

	public VRIK ik;

	public Transform centerEyeAnchor;

	public float walkSpeed = 1f;

	public float runSpeed = 3f;

	public float walkForwardSpeedMlp = 1f;

	public float runForwardSpeedMlp = 1f;

	private Vector3 smoothInput;

	private Vector3 smoothInputV;

	private void Update()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		Vector3 input = GetInput();
		input *= ik.solver.scale;
		bool flag = Vector3.Dot(input, Vector3.forward) > 0f;
		float num = walkSpeed;
		if (Input.GetKey((KeyCode)304))
		{
			num = runSpeed;
			if (flag)
			{
				num *= runForwardSpeedMlp;
			}
		}
		else if (flag)
		{
			num *= walkForwardSpeedMlp;
		}
		smoothInput = Vector3.SmoothDamp(smoothInput, input * num, ref smoothInputV, 0.1f);
		Vector3 forward = centerEyeAnchor.forward;
		forward.y = 0f;
		Quaternion val = Quaternion.LookRotation(forward);
		Transform transform = ((Component)this).transform;
		transform.position += val * smoothInput * Time.deltaTime;
	}

	private Vector3 GetInput()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		switch (inputMode)
		{
		case InputMode.Input:
		{
			Vector3 val2 = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
			if (val2.sqrMagnitude < 0.3f)
			{
				return Vector3.zero;
			}
			return val2.normalized;
		}
		case InputMode.WASDOnly:
		{
			Vector3 val = Vector3.zero;
			if (Input.GetKey((KeyCode)119))
			{
				val += Vector3.forward;
			}
			if (Input.GetKey((KeyCode)115))
			{
				val += Vector3.back;
			}
			if (Input.GetKey((KeyCode)97))
			{
				val += Vector3.left;
			}
			if (Input.GetKey((KeyCode)100))
			{
				val += Vector3.right;
			}
			return val.normalized;
		}
		default:
			return Vector3.zero;
		}
	}
}
