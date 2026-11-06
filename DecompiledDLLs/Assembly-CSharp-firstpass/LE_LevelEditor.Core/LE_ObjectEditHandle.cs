using System;
using Funlabs;
using UnityEngine;

namespace LE_LevelEditor.Core;

public class LE_ObjectEditHandle : MonoBehaviour
{
	private const int SKIP_FRAMES = 0;

	private const float MIN_SCALE_VALUE = 0.05f;

	[SerializeField]
	private LE_EObjectEditMode m_editMode = LE_EObjectEditMode.MOVE;

	private Vector3 m_activeEditAxis = Vector3.zero;

	private int m_lastDragFrame = -1;

	private Vector3 m_lastCursorPos = -1f * Vector3.one;

	private int m_dragSkipCounter;

	private Camera m_cam;

	public EventHandler m_onTransformed;

	public LE_EObjectEditMode EditMode => m_editMode;

	private Camera Cam
	{
		get
		{
			if ((Object)(object)m_cam == (Object)null)
			{
				m_cam = Camera.main;
			}
			return m_cam;
		}
	}

	public bool IsDrag => m_lastDragFrame == Time.frameCount;

	public void OnMyMouseDrag(Vector3 p_axis)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		m_activeEditAxis = p_axis;
		m_lastDragFrame = Time.frameCount;
	}

	public void DisableAxisX()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		DisableAxis(Vector3.right);
	}

	public void DisableAxisY()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		DisableAxis(Vector3.up);
	}

	public void DisableAxisZ()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		DisableAxis(Vector3.forward);
	}

	private void DisableAxis(Vector3 p_axis)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		LE_ObjectEditHandleCollider[] componentsInChildren = ((Component)this).GetComponentsInChildren<LE_ObjectEditHandleCollider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].Axis == p_axis)
			{
				Object.Destroy((Object)(object)((Component)componentsInChildren[i]).gameObject);
			}
			else if (Vector3.Dot(componentsInChildren[i].Axis, p_axis) > 0.0001f)
			{
				Vector3 axis = componentsInChildren[i].Axis;
				axis.Scale(Vector3.one - p_axis);
				componentsInChildren[i].Axis = axis;
			}
		}
	}

	private void LateUpdate()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (m_lastDragFrame < Time.frameCount)
		{
			m_activeEditAxis = Vector3.zero;
			m_dragSkipCounter = 0;
		}
		if ((double)m_activeEditAxis.sqrMagnitude > 0.01)
		{
			switch (m_editMode)
			{
			case LE_EObjectEditMode.MOVE:
				Move();
				break;
			case LE_EObjectEditMode.ROTATE:
				Rotate();
				break;
			case LE_EObjectEditMode.SCALE:
				Scale();
				break;
			}
		}
		m_lastCursorPos = GetCursorPosition();
	}

	private void OnDestroy()
	{
		m_onTransformed = null;
	}

	private void Move()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		float editDelta = GetEditDelta();
		if (m_dragSkipCounter == 0)
		{
			float num = 15625f;
			Vector3 val = ((Component)this).transform.TransformDirection(m_activeEditAxis);
			if (MPExtensions.XZSqr(val * editDelta + ((Component)this).transform.position) < num)
			{
				Transform parent = ((Component)this).transform.parent;
				parent.position += val * editDelta;
			}
			Vector3 position = ((Component)this).transform.parent.position;
			position.y = Mathf.Clamp(((Component)this).transform.parent.position.y, -25f, 300f);
			((Component)this).transform.parent.position = position;
			if (m_onTransformed != null)
			{
				m_onTransformed(this, EventArgs.Empty);
			}
		}
		else if (Mathf.Abs(editDelta) > 0.0005f)
		{
			m_dragSkipCounter++;
		}
	}

	private void Scale()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		float editDelta = GetEditDelta();
		if (m_dragSkipCounter == 0)
		{
			Transform parent = ((Component)this).transform.parent;
			parent.localScale += m_activeEditAxis * editDelta;
			((Component)this).transform.parent.localScale = Vector3.Max(Vector3.one * 0.05f, ((Component)this).transform.parent.localScale);
			if (m_onTransformed != null)
			{
				m_onTransformed(this, EventArgs.Empty);
			}
		}
		else if (Mathf.Abs(editDelta) > 0.0005f)
		{
			m_dragSkipCounter++;
		}
	}

	private float GetEditDelta()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		Camera cam = Cam;
		if ((Object)(object)cam != (Object)null)
		{
			Vector3 val = cam.WorldToScreenPoint(((Component)this).transform.position + ((Component)this).transform.TransformDirection(m_activeEditAxis)) - cam.WorldToScreenPoint(((Component)this).transform.position);
			val.z *= val.z;
			val.z *= val.z;
			float num = 1f / (1f - Mathf.Clamp(val.z, 0f, 0.8f));
			val.z = 0f;
			val.Normalize();
			if (cam.orthographic)
			{
				return GetEditDeltaOrthogonal(val, num, cam);
			}
			Vector3 val2 = ((Component)cam).transform.position - ((Component)this).transform.position;
			float magnitude = val2.magnitude;
			return GetEditDeltaPerspective(val, num * magnitude, cam);
		}
		return 0f;
	}

	private void Rotate()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		float editDeltaRotation = GetEditDeltaRotation();
		if (m_dragSkipCounter == 0)
		{
			((Component)this).transform.parent.Rotate(m_activeEditAxis * editDeltaRotation);
			if (m_onTransformed != null)
			{
				m_onTransformed(this, EventArgs.Empty);
			}
		}
		else if (Mathf.Abs(editDeltaRotation) > 0.0005f)
		{
			m_dragSkipCounter++;
		}
	}

	private float GetEditDeltaRotation()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		Camera cam = Cam;
		if ((Object)(object)cam != (Object)null)
		{
			Vector3 val = ((Component)this).transform.TransformDirection(m_activeEditAxis);
			Vector3 val2 = cam.WorldToScreenPoint(((Component)this).transform.position);
			val2.z = 0f;
			Vector3 val4;
			if (Mathf.Abs(Vector3.Dot(val, ((Component)cam).transform.forward)) > 0.5f)
			{
				float num = Mathf.Sign(Vector3.Dot(((Component)cam).transform.forward, val));
				if (num == 0f)
				{
					num = 1f;
				}
				Vector3 val3 = GetCursorPosition() - val2;
				Vector3 normalized = val3.normalized;
				val4 = new Vector3(0f - Mathf.Sign(normalized.y), Mathf.Sign(normalized.x), 0f);
				val4 *= num;
			}
			else
			{
				val4 = Vector3.Cross(((Component)cam).transform.forward, val);
				val4 = ((Component)cam).transform.InverseTransformDirection(val4);
				val4.z = 0f;
			}
			val4.Normalize();
			if (cam.orthographic)
			{
				return GetEditDeltaOrthogonal(val4, 360f / cam.orthographicSize, cam);
			}
			return GetEditDeltaPerspective(val4, 360f, cam);
		}
		return 0f;
	}

	private float GetEditDeltaPerspective(Vector3 p_screenAxis, float p_multiplier, Camera p_cam)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Dot(p_screenAxis, GetCursorPosition() - m_lastCursorPos);
		Ray val = p_cam.ScreenPointToRay(m_lastCursorPos + num * p_screenAxis);
		Ray val2 = p_cam.ScreenPointToRay(m_lastCursorPos);
		Vector3 direction = val.direction;
		Vector3 direction2 = val2.direction;
		Vector3 val3 = direction - direction2;
		return val3.magnitude * Mathf.Sign(num) * p_multiplier;
	}

	private float GetEditDeltaOrthogonal(Vector3 p_screenAxis, float p_multiplier, Camera p_cam)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Dot(p_screenAxis, GetCursorPosition() - m_lastCursorPos);
		Vector3 val = p_cam.ScreenToWorldPoint(m_lastCursorPos + num * p_screenAxis);
		Vector3 val2 = p_cam.ScreenToWorldPoint(m_lastCursorPos);
		Vector3 val3 = val - val2;
		return val3.magnitude * Mathf.Sign(num) * p_multiplier;
	}

	private Vector3 GetCursorPosition()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);
			return Vector2.op_Implicit(touch.position);
		}
		return Input.mousePosition;
	}

	public LE_ObjectEditHandle()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
	}
}
