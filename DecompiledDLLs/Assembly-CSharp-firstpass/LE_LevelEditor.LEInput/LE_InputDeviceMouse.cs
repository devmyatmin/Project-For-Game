using UnityEngine;

namespace LE_LevelEditor.LEInput;

public class LE_InputDeviceMouse : LE_InputDeviceBase
{
	private const float MOUSE_WHEEL_SENSITIVITY = 200f;

	private Vector3 m_mouseLookStart = Vector3.zero;

	private Vector3 m_lastMousePosition = Vector3.zero;

	public LE_InputDeviceMouse(LE_IInputHandler p_inputHandler)
		: base(p_inputHandler)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		m_lastMousePosition = Input.mousePosition;
	}

	public override void Update()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		if (m_lastMousePosition != Input.mousePosition)
		{
			m_inputHandler.SetCursorPosition(Input.mousePosition);
		}
		m_inputHandler.SetIsCursorAction(Input.GetMouseButton(0));
		Vector3 val;
		if (Input.GetMouseButton(2))
		{
			if (m_mouseLookStart.sqrMagnitude == 0f)
			{
				m_mouseLookStart = Input.mousePosition;
			}
			else
			{
				val = m_mouseLookStart - Input.mousePosition;
				if (val.magnitude > 0.0001f)
				{
					m_inputHandler.MoveCamera(Input.mousePosition, m_mouseLookStart);
					m_mouseLookStart = Input.mousePosition;
				}
			}
		}
		else if (Input.GetMouseButton(1))
		{
			if (m_mouseLookStart.sqrMagnitude == 0f)
			{
				m_mouseLookStart = Input.mousePosition;
			}
			else
			{
				val = m_mouseLookStart - Input.mousePosition;
				if (val.magnitude > 0.0001f)
				{
					if (Input.GetKey((KeyCode)308) || Input.GetKey((KeyCode)307) || Input.GetKey((KeyCode)313))
					{
						m_inputHandler.RotateCameraAroundPivot(Input.mousePosition, m_mouseLookStart);
					}
					else
					{
						m_inputHandler.RotateCamera(Input.mousePosition, m_mouseLookStart);
					}
					m_mouseLookStart = Input.mousePosition;
				}
			}
		}
		else
		{
			m_mouseLookStart = Vector3.zero;
		}
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (axis != 0f)
		{
			m_inputHandler.MoveCamera(Vector3.zero, Vector3.forward * axis * 200f, scrolling: true);
		}
	}

	public override void Destroy()
	{
	}
}
