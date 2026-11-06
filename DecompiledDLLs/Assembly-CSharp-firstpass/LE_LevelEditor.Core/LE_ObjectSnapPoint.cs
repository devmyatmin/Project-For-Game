using System;
using S_SnapTools;
using UnityEngine;

namespace LE_LevelEditor.Core;

[Serializable]
public class LE_ObjectSnapPoint
{
	private const string BUTTON_VISUALS_NAME = "LE_ObjectSnapPoint.Button.Visuals";

	[SerializeField]
	private Transform m_point;

	[SerializeField]
	private S_SnapToObjectPrefab[] m_prefabs = new S_SnapToObjectPrefab[1];

	[SerializeField]
	private Vector3 m_snapButtonLocalOffset = Vector3.zero;

	[SerializeField]
	private float m_snapButtonSize = 1f;

	private S_SnapToObject m_snapSysInstance;

	public Transform Point
	{
		get
		{
			return m_point;
		}
		set
		{
			m_point = value;
		}
	}

	public S_SnapToObjectPrefab[] Prefabs
	{
		get
		{
			return m_prefabs;
		}
		set
		{
			m_prefabs = value;
		}
	}

	public Vector3 SnapButtonLocalOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_snapButtonLocalOffset;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_snapButtonLocalOffset = value;
		}
	}

	public float SnapButtonSize
	{
		get
		{
			return m_snapButtonSize;
		}
		set
		{
			m_snapButtonSize = value;
		}
	}

	public S_SnapToObject SnapSystemInstance
	{
		get
		{
			return m_snapSysInstance;
		}
		set
		{
			m_snapSysInstance = value;
		}
	}

	public LE_ObjectSnapPoint()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (m_prefabs != null && m_prefabs.Length == 1)
		{
			m_prefabs[0] = new S_SnapToObjectPrefab();
		}
	}

	public S_SnapToObject InstatiateSnapSystem(GameObject p_buttonVisuals, bool p_isDrawUI, Material p_materialLine, Material p_materialFill)
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_point == (Object)null)
		{
			Debug.LogError((object)"LE_ObjectSnapPoint: InstatiateSnapSystem: the Transform property 'Point' is null! Set it in the inspector!");
			return null;
		}
		if (m_prefabs == null || m_prefabs.Length == 0)
		{
			Debug.LogError((object)"LE_ObjectSnapPoint: InstatiateSnapSystem: the Prefabs property array is null or has no entries!");
			return null;
		}
		if ((Object)(object)m_snapSysInstance != (Object)null)
		{
			Debug.LogError((object)"LE_ObjectSnapPoint: InstatiateSnapSystem: the system was already initialized!");
			Object.Destroy((Object)(object)m_snapSysInstance);
		}
		m_snapSysInstance = ((Component)Point).gameObject.AddComponent<S_SnapToObject>();
		m_snapSysInstance.Prefabs = Prefabs;
		m_snapSysInstance.IsDeactivatedAfterSnap = true;
		m_snapSysInstance.IsDestroyedAfterSnap = false;
		m_snapSysInstance.IsDrawUI = p_isDrawUI;
		m_snapSysInstance.UImaterialLine = p_materialLine;
		m_snapSysInstance.UImaterialFill = p_materialFill;
		SphereCollider val = ((Component)Point).gameObject.AddComponent<SphereCollider>();
		val.radius = SnapButtonSize * 0.5f;
		val.center = SnapButtonLocalOffset;
		((Object)p_buttonVisuals).name = "LE_ObjectSnapPoint.Button.Visuals";
		if ((Object)(object)p_buttonVisuals != (Object)null)
		{
			p_buttonVisuals.transform.parent = Point;
			p_buttonVisuals.transform.localPosition = m_snapButtonLocalOffset;
			p_buttonVisuals.transform.localScale = Vector3.one * m_snapButtonSize;
			p_buttonVisuals.transform.localRotation = Quaternion.identity;
		}
		else
		{
			Debug.LogWarning((object)"LE_ObjectSnapPoint: InstatiateSnapSystem: p_buttonVisuals is null! The snap button will be invisible for the user!");
		}
		return m_snapSysInstance;
	}

	public static void DestroySnapSystem(S_SnapToObject p_system)
	{
		if (!((Object)(object)p_system != (Object)null))
		{
			return;
		}
		Object.Destroy((Object)(object)p_system);
		Object.Destroy((Object)(object)((Component)p_system).GetComponent<SphereCollider>());
		Transform[] componentsInChildren = ((Component)p_system).GetComponentsInChildren<Transform>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (((Object)componentsInChildren[i]).name == "LE_ObjectSnapPoint.Button.Visuals")
			{
				Object.Destroy((Object)(object)((Component)componentsInChildren[i]).gameObject);
			}
		}
	}
}
