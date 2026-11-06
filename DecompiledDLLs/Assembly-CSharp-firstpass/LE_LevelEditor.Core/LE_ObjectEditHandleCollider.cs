using UnityEngine;

namespace LE_LevelEditor.Core;

public class LE_ObjectEditHandleCollider : MonoBehaviour
{
	[SerializeField]
	private LE_ObjectEditHandle m_parent;

	[SerializeField]
	private Vector3 m_axis;

	[SerializeField]
	private bool m_isRotationHandle;

	[SerializeField]
	private Vector3 m_rotationHandleDir;

	private Renderer m_renderer;

	private Collider[] m_colliders;

	private Color m_originalColor;

	private bool m_isDrag;

	private float m_lastCamDist = -1f;

	private Vector3 m_lastParentScale = Vector3.one;

	private Vector3 m_lastParentParentScale = Vector3.one;

	private bool m_isMouseDown;

	private bool m_isMouseDownOnMe;

	private Camera m_cam;

	public Vector3 Axis
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_axis;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_axis = value;
		}
	}

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

	private void Start()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		m_colliders = ((Component)this).GetComponentsInChildren<Collider>();
		m_renderer = ((Component)this).GetComponent<Renderer>();
		m_originalColor = m_renderer.material.color;
	}

	private void Update()
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		bool flag = (Input.GetMouseButton(0) && Input.touchCount < 2) || Input.touchCount == 1;
		if (!m_isMouseDown && !flag)
		{
			return;
		}
		if (m_isMouseDownOnMe && !flag)
		{
			OnMyMouseUp();
		}
		else if (m_isMouseDownOnMe & flag)
		{
			OnMyMouseDrag();
		}
		else
		{
			Camera cam = Cam;
			if (m_colliders != null && (Object)(object)cam != (Object)null)
			{
				RaycastHit val2 = default;
				for (int i = 0; i < m_colliders.Length; i++)
				{
					Collider val = m_colliders[i];
					if (!(((Object)(object)val != (Object)null && !m_isMouseDown) & flag) || m_parent.IsDrag)
					{
						continue;
					}
					if (!val.Raycast(cam.ScreenPointToRay(Input.mousePosition), ref val2, float.PositiveInfinity))
					{
						if (Input.touchCount != 1)
						{
							continue;
						}
						Touch touch = Input.GetTouch(0);
						if (!val.Raycast(cam.ScreenPointToRay(Vector2.op_Implicit(touch.position)), ref val2, float.PositiveInfinity))
						{
							continue;
						}
					}
					m_isMouseDownOnMe = true;
					OnMyMouseDown();
					break;
				}
			}
		}
		m_isMouseDown = flag;
		if (!m_isMouseDown)
		{
			m_isMouseDownOnMe = false;
		}
	}

	private void LateUpdate()
	{
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		Camera cam = Cam;
		if (!((Object)(object)cam != (Object)null))
		{
			return;
		}
		if (!m_isDrag && m_isRotationHandle)
		{
			Vector3 val = ((Component)this).transform.parent.InverseTransformPoint(((Component)cam).transform.position);
			Vector3 val2 = val - Vector3.Dot(m_axis, val) * m_axis;
			val2.Normalize();
			float num = Vector3.Dot(m_rotationHandleDir, val2);
			Vector3 val3 = Vector3.Cross(m_rotationHandleDir, val2);
			val3.Scale(m_axis);
			float num2 = 57.29578f * Mathf.Asin(val3.magnitude);
			val3.Normalize();
			if (num < 0f)
			{
				num2 = Mathf.Abs(-180f + num2);
			}
			val3 *= num2;
			((Component)this).transform.localRotation = Quaternion.Euler(val3);
		}
		Vector3 val4 = ((Component)cam).transform.position - ((Component)this).transform.position;
		float magnitude = val4.magnitude;
		if (!m_isDrag && (cam.orthographic || Mathf.Abs(magnitude - m_lastCamDist) > 0.0001f || m_lastParentScale != ((Component)m_parent).transform.localScale || m_lastParentParentScale != ((Component)m_parent).transform.parent.localScale))
		{
			float num3 = ((!cam.orthographic) ? (magnitude / 75f) : (cam.orthographicSize / 50f));
			((Component)this).transform.localScale = new Vector3(num3 / (((Component)m_parent).transform.parent.localScale.x * ((Component)m_parent).transform.localScale.x), num3 / (((Component)m_parent).transform.parent.localScale.y * ((Component)m_parent).transform.localScale.y), num3 / (((Component)m_parent).transform.parent.localScale.z * ((Component)m_parent).transform.localScale.z));
			m_lastCamDist = magnitude;
			m_lastParentScale = ((Component)m_parent).transform.localScale;
			m_lastParentParentScale = ((Component)m_parent).transform.parent.localScale;
		}
	}

	private void OnMyMouseDown()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		m_parent.OnMyMouseDrag(m_axis);
		m_renderer.material.color = Color.yellow;
		m_isDrag = true;
	}

	private void OnMyMouseDrag()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		m_parent.OnMyMouseDrag(m_axis);
		m_isDrag = true;
	}

	private void OnMyMouseUp()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		m_renderer.material.color = m_originalColor;
		((Component)m_parent).transform.localScale = new Vector3(1f / ((Component)m_parent).transform.parent.localScale.x, 1f / ((Component)m_parent).transform.parent.localScale.y, 1f / ((Component)m_parent).transform.parent.localScale.z);
		m_isDrag = false;
	}

	public LE_ObjectEditHandleCollider()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
	}
}
