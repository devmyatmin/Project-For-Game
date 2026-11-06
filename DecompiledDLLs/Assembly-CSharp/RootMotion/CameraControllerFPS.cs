using UnityEngine;

namespace RootMotion;

public class CameraControllerFPS : MonoBehaviour
{
	public float rotationSensitivity = 3f;

	public float yMinLimit = -89f;

	public float yMaxLimit = 89f;

	private float x;

	private float y;

	private void Awake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Vector3 eulerAngles = ((Component)this).transform.eulerAngles;
		x = eulerAngles.y;
		y = eulerAngles.x;
	}

	public void LateUpdate()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		Cursor.lockState = (CursorLockMode)1;
		x += Input.GetAxis("Mouse X") * rotationSensitivity;
		y = ClampAngle(y - Input.GetAxis("Mouse Y") * rotationSensitivity, yMinLimit, yMaxLimit);
		((Component)this).transform.rotation = Quaternion.AngleAxis(x, Vector3.up) * Quaternion.AngleAxis(y, Vector3.right);
	}

	private float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}
}
