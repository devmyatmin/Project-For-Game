using UnityEngine;

namespace RootMotion.Demos;

public class CharacterController3rdPerson : MonoBehaviour
{
	public CameraController cam;

	private AnimatorController3rdPerson animatorController;

	private static Vector3 inputVector
	{
		get
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
		}
	}

	private static Vector3 inputVectorRaw
	{
		get
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
		}
	}

	private void Start()
	{
		animatorController = ((Component)this).GetComponent<AnimatorController3rdPerson>();
		((Behaviour)cam).enabled = false;
	}

	private void LateUpdate()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		cam.UpdateInput();
		cam.UpdateTransform();
		Vector3 moveInput = inputVector;
		bool isMoving = inputVector != Vector3.zero || inputVectorRaw != Vector3.zero;
		Vector3 forward = ((Component)cam).transform.forward;
		Vector3 aimTarget = ((Component)cam).transform.position + forward * 10f;
		animatorController.Move(moveInput, isMoving, forward, aimTarget);
	}
}
