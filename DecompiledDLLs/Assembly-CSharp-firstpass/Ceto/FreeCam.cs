using UnityEngine;

namespace Ceto;

public class FreeCam : MonoBehaviour
{
	public enum RotationAxes
	{
		MouseXAndY,
		MouseX,
		MouseY
	}

	public float m_speed = 50f;

	public RotationAxes axes;

	public float sensitivityX = 15f;

	public float sensitivityY = 15f;

	public float minimumX = -360f;

	public float maximumX = 360f;

	public float minimumY = -60f;

	public float maximumY = 60f;

	public float rotationY;

	private bool m_takeMouseInput = true;

	private void Start()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localEulerAngles = new Vector3(0f - rotationY, ((Component)this).transform.localEulerAngles.y, 0f);
	}

	private void OnGUI()
	{
		if (Event.current != null)
		{
			if (Event.current.isMouse)
			{
				m_takeMouseInput = true;
			}
			else
			{
				m_takeMouseInput = false;
			}
		}
	}

	private void Update()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		float num = m_speed;
		if (Input.GetKey((KeyCode)32))
		{
			num *= 10f;
		}
		Vector3 val = new Vector3(0f, 0f, 0f);
		if (Input.GetKey((KeyCode)97))
		{
			val = new Vector3(-1f, 0f, 0f) * Time.deltaTime * num;
		}
		if (Input.GetKey((KeyCode)100))
		{
			val = new Vector3(1f, 0f, 0f) * Time.deltaTime * num;
		}
		if (Input.GetKey((KeyCode)119))
		{
			val = new Vector3(0f, 0f, 1f) * Time.deltaTime * num;
		}
		if (Input.GetKey((KeyCode)115))
		{
			val = new Vector3(0f, 0f, -1f) * Time.deltaTime * num;
		}
		if (Input.GetKey((KeyCode)101))
		{
			val = new Vector3(0f, -1f, 0f) * Time.deltaTime * num;
		}
		if (Input.GetKey((KeyCode)113))
		{
			val = new Vector3(0f, 1f, 0f) * Time.deltaTime * num;
		}
		((Component)this).transform.Translate(val);
		if (m_takeMouseInput)
		{
			if (axes == RotationAxes.MouseXAndY)
			{
				float num2 = ((Component)this).transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
				rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
				rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
				((Component)this).transform.localEulerAngles = new Vector3(0f - rotationY, num2, 0f);
			}
			else if (axes == RotationAxes.MouseX)
			{
				((Component)this).transform.Rotate(0f, Input.GetAxis("Mouse X") * sensitivityX, 0f);
			}
			else
			{
				rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
				rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
				((Component)this).transform.localEulerAngles = new Vector3(0f - rotationY, ((Component)this).transform.localEulerAngles.y, 0f);
			}
		}
	}
}
