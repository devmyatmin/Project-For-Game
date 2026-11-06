using UnityEngine;

namespace RootMotion.Demos;

public class MechSpiderController : MonoBehaviour
{
	public MechSpider mechSpider;

	public Transform cameraTransform;

	public float speed = 6f;

	public float turnSpeed = 30f;

	public Vector3 inputVector
	{
		get
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
		}
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = cameraTransform.forward;
		Vector3 up = ((Component)this).transform.up;
		Vector3.OrthoNormalize(ref up, ref forward);
		Quaternion val = Quaternion.LookRotation(forward, ((Component)this).transform.up);
		Transform transform = ((Component)this).transform;
		Vector3 val2 = inputVector;
		transform.Translate(val * val2.normalized * Time.deltaTime * speed * mechSpider.scale, (Space)0);
		((Component)this).transform.rotation = Quaternion.RotateTowards(((Component)this).transform.rotation, val, Time.deltaTime * turnSpeed);
	}
}
