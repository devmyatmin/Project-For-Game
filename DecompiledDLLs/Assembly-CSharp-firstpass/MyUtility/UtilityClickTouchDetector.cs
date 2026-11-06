using System;
using UnityEngine;

namespace MyUtility;

public class UtilityClickTouchDetector : MonoBehaviour
{
	private enum ECursorState
	{
		DOWN_FRAME,
		UP_FRAME,
		HELD
	}

	public Action m_onClick;

	private Collider m_collider;

	private Camera m_camera;

	private bool m_isMouseDown;

	private bool m_isHeldDown;

	public Collider ColliderInstance
	{
		get
		{
			return m_collider;
		}
		set
		{
			m_collider = value;
		}
	}

	public Camera CameraInstance
	{
		get
		{
			return m_camera;
		}
		set
		{
			m_camera = value;
		}
	}

	private void Start()
	{
		if ((Object)(object)m_collider == (Object)null)
		{
			m_collider = ((Component)this).GetComponent<Collider>();
			if ((Object)(object)m_collider == (Object)null)
			{
				Debug.LogError((object)"UtilityClickTouchDetector: could not find a collider!");
				((Behaviour)this).enabled = false;
			}
		}
		if ((Object)(object)m_camera == (Object)null)
		{
			m_camera = Camera.main;
			if ((Object)(object)m_camera == (Object)null)
			{
				Debug.LogError((object)"UtilityClickTouchDetector: could not find a camera!");
				((Behaviour)this).enabled = false;
			}
		}
	}

	private void Update()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected I4, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_collider != (Object)null && (Object)(object)m_camera != (Object)null)
		{
			bool mouseButton = Input.GetMouseButton(0);
			if (mouseButton || m_isMouseDown)
			{
				ECursorState p_state = ((!mouseButton) ? ECursorState.UP_FRAME : (m_isMouseDown ? ECursorState.HELD : ECursorState.DOWN_FRAME));
				RaycastCursor(Input.mousePosition, p_state);
				m_isMouseDown = mouseButton;
			}
			Touch[] touches = Input.touches;
			for (int i = 0; i < touches.Length; i++)
			{
				TouchPhase phase = touches[i].phase;
				ECursorState p_state2;
				switch ((int)phase)
				{
				case 0:
					p_state2 = ECursorState.DOWN_FRAME;
					break;
				case 3:
					p_state2 = ECursorState.UP_FRAME;
					break;
				case 1:
				case 2:
					p_state2 = ECursorState.HELD;
					break;
				default:
					continue;
				}
				RaycastCursor(Vector2.op_Implicit(touches[i].position), p_state2);
			}
		}
		else if ((Object)(object)m_collider == (Object)null)
		{
			Debug.LogError((object)"UtilityClickTouchDetector: Update: lost reference to collider!");
			((Behaviour)this).enabled = false;
		}
		else
		{
			Debug.LogError((object)"UtilityClickTouchDetector: Update: lost reference to camera!");
			((Behaviour)this).enabled = false;
		}
	}

	private void OnDestroy()
	{
		m_onClick = null;
	}

	private void RaycastCursor(Vector3 p_screenPos, ECursorState p_state)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default;
		if (m_collider.Raycast(m_camera.ScreenPointToRay(p_screenPos), ref val, float.MaxValue))
		{
			if (m_isHeldDown)
			{
				switch (p_state)
				{
				case ECursorState.DOWN_FRAME:
					m_isHeldDown = true;
					break;
				case ECursorState.UP_FRAME:
					m_isHeldDown = false;
					if (m_onClick != null)
					{
						m_onClick();
					}
					break;
				}
			}
			else if (p_state == ECursorState.DOWN_FRAME)
			{
				m_isHeldDown = true;
			}
		}
		else if (m_isHeldDown && (uint)p_state <= 1u)
		{
			m_isHeldDown = false;
		}
	}
}
