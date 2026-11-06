using System;
using System.Collections.Generic;
using MyUtility;
using UnityEngine;

namespace S_SnapTools;

public class S_SnapToObjectPreview : MonoBehaviour
{
	private Camera m_cam;

	private SphereCollider m_collider;

	private bool m_isDrawUI;

	private float m_relativeScreenSize = 0.2f;

	private Action m_onClick;

	private Material m_uiMaterialLine;

	private Material m_uiMaterialFill;

	private Mesh m_uiMesh;

	private Vector3[] m_uiVerts = new Vector3[30];

	private bool m_isUiMeshIndicesSet;

	private float m_worldRadius = 1f;

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

	public float WorldRadius => m_worldRadius;

	public void Init(bool p_isDrawUI, float p_relativeScreenSize, Action p_onClick, Material p_uiMaterialLine, Material p_uiMaterialFill)
	{
		m_isDrawUI = p_isDrawUI;
		m_relativeScreenSize = p_relativeScreenSize;
		m_onClick = p_onClick;
		m_uiMaterialLine = p_uiMaterialLine;
		m_uiMaterialFill = p_uiMaterialFill;
		if (m_isDrawUI && ((Object)(object)m_uiMaterialLine == (Object)null || (Object)(object)m_uiMaterialFill == (Object)null))
		{
			Debug.LogError((object)"S_SnapToObjectPreview: Init: IsDrawUI is set to 'true', but no UImaterial was passed!");
		}
	}

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		Renderer[] componentsInChildren = ((Component)this).GetComponentsInChildren<Renderer>();
		Vector3 val = Vector3.zero;
		Renderer[] array = componentsInChildren;
		foreach (Renderer val2 in array)
		{
			Vector3 val3 = val;
			Bounds bounds = val2.bounds;
			val = val3 + bounds.center;
		}
		val *= 1f / (float)componentsInChildren.Length;
		m_collider = ((Component)this).gameObject.AddComponent<SphereCollider>();
		m_collider.center = ((Component)this).transform.InverseTransformPoint(val);
		if (m_isDrawUI)
		{
			CreateUIObject();
		}
		if (m_onClick != null)
		{
			UtilityClickTouchDetector utilityClickTouchDetector = ((Component)this).gameObject.AddComponent<UtilityClickTouchDetector>();
			utilityClickTouchDetector.ColliderInstance = (Collider)(object)m_collider;
			utilityClickTouchDetector.m_onClick = (Action)Delegate.Combine(utilityClickTouchDetector.m_onClick, m_onClick);
		}
		else
		{
			Debug.LogError((object)"S_SnapToObjectPreview: Start: you have to call 'Init' before 'Start' and pass a valid p_onClick parameter!");
		}
	}

	private void Update()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Cam != (Object)null)
		{
			Vector3 val = Cam.ScreenToWorldPoint(Cam.WorldToScreenPoint(((Component)this).transform.position) + (float)Screen.height * Vector3.left * m_relativeScreenSize);
			Vector3 val2 = ((Component)this).transform.position - val;
			m_worldRadius = val2.magnitude;
			SphereCollider collider = m_collider;
			val2 = ((Component)this).transform.lossyScale;
			collider.radius = 1f / val2.magnitude * m_worldRadius;
			UpdateUIVertices();
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)m_uiMesh != (Object)null)
		{
			Object.Destroy((Object)(object)m_uiMesh);
		}
	}

	private void CreateUIObject()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected Obj, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("S_SnapToObjectPreview UI");
		val.transform.parent = ((Component)this).transform;
		val.transform.localPosition = Vector3.zero;
		val.transform.localRotation = Quaternion.identity;
		val.transform.localScale = Vector3.one;
		m_uiMesh = new Mesh();
		val.AddComponent<MeshFilter>().mesh = m_uiMesh;
		((Renderer)val.AddComponent<MeshRenderer>()).sharedMaterials = new Material[2] { m_uiMaterialFill, m_uiMaterialLine };
	}

	private void UpdateUIVertices()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)m_uiMesh != (Object)null) || !((Object)(object)m_collider != (Object)null) || !((Object)(object)Cam != (Object)null))
		{
			return;
		}
		Vector3 val = ((Component)this).transform.lossyScale;
		float magnitude = val.magnitude;
		float num = 1.5f * magnitude;
		Vector3 val2 = ((Component)this).transform.TransformPoint(m_collider.center);
		val = val2 - ((Component)Cam).transform.position;
		Vector3 val3 = val2 + val.normalized * magnitude;
		float num2 = (float)Math.PI / 180f * (360f / (float)m_uiVerts.Length);
		for (int i = 0; i < m_uiVerts.Length; i++)
		{
			float num3 = (float)i * num2;
			m_uiVerts[i] = ((Component)this).transform.InverseTransformPoint(val3 + ((Component)Cam).transform.TransformDirection(new Vector3(Mathf.Sin(num3), Mathf.Cos(num3), 0f)) * num);
		}
		m_uiMesh.vertices = m_uiVerts;
		if (!m_isUiMeshIndicesSet)
		{
			m_isUiMeshIndicesSet = true;
			m_uiMesh.subMeshCount = 2;
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			for (int j = 0; j < m_uiVerts.Length; j++)
			{
				if (j + 2 < m_uiVerts.Length)
				{
					list.Add(0);
					list.Add(j + 1);
					list.Add(j + 2);
				}
				list2.Add(j);
			}
			list2.Add(0);
			m_uiMesh.SetIndices(list.ToArray(), (MeshTopology)0, 0);
			m_uiMesh.SetIndices(list2.ToArray(), (MeshTopology)4, 1);
		}
		m_uiMesh.RecalculateBounds();
	}
}
