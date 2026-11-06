using System;
using UnityEngine;

namespace CPG_CameraPerspective;

public class CPG_CameraPerspectiveGizmo : MonoBehaviour
{
	public enum EButtonTypes
	{
		TOGGLE_ORTHO,
		X_POS,
		X_NEG,
		Y_POS,
		Y_NEG,
		Z_POS,
		Z_NEG
	}

	[SerializeField]
	private Vector2 m_relativeScreenPos = Vector2.one * 0.85f;

	[SerializeField]
	private float m_relativeScreenSize = 0.1f;

	[SerializeField]
	private int m_layerToUse = 31;

	[SerializeField]
	private Vector3 m_pivot = Vector3.zero;

	[SerializeField]
	private float m_minDistToPivot = 15f;

	[SerializeField]
	private float m_orthoOffset;

	private Camera m_targetCamera;

	private Camera m_ownCamera;

	public EventHandler m_onBeforeSwitchToOrthographic;

	public EventHandler m_onAfterSwitchToOrthographic;

	public EventHandler m_onBeforeSwitchToPerspective;

	public EventHandler m_onAfterSwitchToPerspective;

	public Vector2 RelativeScreenPos
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_relativeScreenPos;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_relativeScreenPos = value;
		}
	}

	public float RelativeScreenSize
	{
		get
		{
			return m_relativeScreenSize;
		}
		set
		{
			m_relativeScreenSize = value;
		}
	}

	public int LayerToUse
	{
		get
		{
			return m_layerToUse;
		}
		set
		{
			m_layerToUse = value;
		}
	}

	public Vector3 Pivot
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_pivot;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_pivot = value;
		}
	}

	public float MinDistToPivot
	{
		get
		{
			return m_minDistToPivot;
		}
		set
		{
			m_minDistToPivot = value;
		}
	}

	public float OrthoOffset
	{
		get
		{
			return m_orthoOffset;
		}
		set
		{
			m_orthoOffset = value;
		}
	}

	public Camera TargetCamera
	{
		get
		{
			if ((Object)(object)m_targetCamera == (Object)null)
			{
				m_targetCamera = Camera.main;
			}
			return m_targetCamera;
		}
		set
		{
			m_targetCamera = value;
		}
	}

	public Camera OwnCamera
	{
		get
		{
			if ((Object)(object)m_ownCamera == (Object)null)
			{
				m_ownCamera = ((Component)this).GetComponentInChildren<Camera>();
			}
			return m_ownCamera;
		}
	}

	public static CPG_CameraPerspectiveGizmo Create(Camera p_targetCamera, int p_layerToUse)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)p_targetCamera != (Object)null)
		{
			Object val = Resources.Load("CPG_CameraPerspectiveGizmo");
			if (val != (Object)null)
			{
				CPG_CameraPerspectiveGizmo component = ((GameObject)Object.Instantiate(val)).GetComponent<CPG_CameraPerspectiveGizmo>();
				component.TargetCamera = p_targetCamera;
				component.LayerToUse = p_layerToUse;
				return component;
			}
			Debug.LogError((object)"CPG_CameraPerspectiveGizmo: Create: prefab 'CPG_CameraPerspectiveGizmo' not found!");
		}
		else
		{
			Debug.LogError((object)"CPG_CameraPerspectiveGizmo: Create: p_targetCamera is null!");
		}
		return null;
	}

	public void ReportClick(EButtonTypes p_type)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		switch (p_type)
		{
		case EButtonTypes.TOGGLE_ORTHO:
			ToggleOrtho();
			break;
		case EButtonTypes.X_POS:
			ToogleAxis(Vector3.right);
			break;
		case EButtonTypes.X_NEG:
			ToogleAxis(Vector3.left);
			break;
		case EButtonTypes.Y_POS:
			ToogleAxis(Vector3.up);
			break;
		case EButtonTypes.Y_NEG:
			ToogleAxis(Vector3.down);
			break;
		case EButtonTypes.Z_POS:
			ToogleAxis(Vector3.forward);
			break;
		case EButtonTypes.Z_NEG:
			ToogleAxis(Vector3.back);
			break;
		}
	}

	private void ToggleOrtho()
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		Camera targetCamera = TargetCamera;
		if ((Object)(object)targetCamera != (Object)null)
		{
			if (targetCamera.orthographic && m_onBeforeSwitchToPerspective != null)
			{
				m_onBeforeSwitchToPerspective(this, EventArgs.Empty);
			}
			if (!targetCamera.orthographic && m_onBeforeSwitchToOrthographic != null)
			{
				m_onBeforeSwitchToOrthographic(this, EventArgs.Empty);
			}
			if (targetCamera.orthographic)
			{
				Transform transform = ((Component)targetCamera).transform;
				transform.position += ((Component)targetCamera).transform.forward * m_orthoOffset;
			}
			else
			{
				Transform transform2 = ((Component)targetCamera).transform;
				transform2.position -= ((Component)targetCamera).transform.forward * m_orthoOffset;
			}
			targetCamera.orthographic = !targetCamera.orthographic;
			if (targetCamera.orthographic && m_onAfterSwitchToOrthographic != null)
			{
				m_onAfterSwitchToOrthographic(this, EventArgs.Empty);
			}
			if (!targetCamera.orthographic && m_onAfterSwitchToPerspective != null)
			{
				m_onAfterSwitchToPerspective(this, EventArgs.Empty);
			}
		}
	}

	private void ToogleAxis(Vector3 p_axis)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		Camera targetCamera = TargetCamera;
		if ((Object)(object)targetCamera != (Object)null)
		{
			Vector3 val = ((Component)targetCamera).transform.position - Pivot;
			float num = val.magnitude;
			if (num == 0f)
			{
				num = 1f;
			}
			float num2 = m_minDistToPivot;
			if (targetCamera.orthographic)
			{
				num2 += m_orthoOffset;
			}
			((Component)targetCamera).transform.position = Pivot + Mathf.Max(num2, num) * p_axis;
			((Component)targetCamera).transform.LookAt(m_pivot);
		}
	}

	private void Start()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		MoveToLayer(((Component)this).transform, m_layerToUse);
		m_ownCamera = ((Component)this).GetComponentInChildren<Camera>();
		if ((Object)(object)m_ownCamera != (Object)null)
		{
			m_ownCamera.aspect = 1f;
			Camera targetCamera = TargetCamera;
			if ((Object)(object)targetCamera != (Object)null)
			{
				Rect pixelRect = targetCamera.pixelRect;
				float xMax = pixelRect.xMax;
				pixelRect = targetCamera.pixelRect;
				if (xMax - pixelRect.xMin > 1f)
				{
					pixelRect = targetCamera.pixelRect;
					float yMax = pixelRect.yMax;
					pixelRect = targetCamera.pixelRect;
					if (yMax - pixelRect.yMin > 1f)
					{
						Rect rect = targetCamera.rect;
						float num = m_relativeScreenSize / targetCamera.aspect * rect.width;
						float num2 = m_relativeScreenSize * rect.height;
						Vector2 val = new Vector2(rect.x, rect.y) + m_relativeScreenPos - new Vector2(1f - rect.xMax, 1f - rect.yMax) - 0.5f * new Vector2(num, num2);
						m_ownCamera.rect = new Rect(val.x, val.y, num, num2);
						goto IL_0160;
					}
				}
			}
			Debug.LogError((object)"CPG_CameraPerspectiveGizmo: Start: could not find any camera with a non empty view rect! Will destroy self now!");
			Object.Destroy((Object)(object)((Component)this).gameObject);
			goto IL_0160;
		}
		Debug.LogError((object)"CPG_CameraPerspectiveGizmo: Start: CPG_CameraPerspectiveGizmo prefab seems to be broken it needs a camera in its children!");
		return;
		IL_0160:
		m_ownCamera.cullingMask = 1 << m_layerToUse;
	}

	private void Update()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Camera targetCamera = TargetCamera;
		if ((Object)(object)targetCamera != (Object)null && (Object)(object)m_ownCamera != (Object)null)
		{
			m_ownCamera.orthographic = targetCamera.orthographic;
			((Component)m_ownCamera).transform.localPosition = -5.2f * ((Component)targetCamera).transform.forward;
			((Component)m_ownCamera).transform.rotation = ((Component)targetCamera).transform.rotation;
		}
		else
		{
			Debug.LogWarning((object)"CPG_CameraPerspectiveGizmo: Update: lost reference to camera ! Will destroy self now!");
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void OnDestroy()
	{
		m_onBeforeSwitchToOrthographic = null;
		m_onBeforeSwitchToPerspective = null;
		m_onAfterSwitchToOrthographic = null;
		m_onAfterSwitchToPerspective = null;
	}

	private void MoveToLayer(Transform root, int layer)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		((Component)root).gameObject.layer = layer;
		foreach (Transform item in root)
		{
			Transform root2 = item;
			MoveToLayer(root2, layer);
		}
	}

	public CPG_CameraPerspectiveGizmo()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
	}
}
