using System;
using UnityEngine;

namespace S_SnapTools;

[Serializable]
public class S_SnapToObjectPrefab
{
	[SerializeField]
	private bool m_isCollidersDisabledInPreview = true;

	[SerializeField]
	private Vector3 m_previewScale = Vector3.one * 0.25f;

	[SerializeField]
	private Vector3 m_previewEulerRotation = Vector3.zero;

	[SerializeField]
	private Vector3 m_localPosition = Vector3.zero;

	[SerializeField]
	private Vector3 m_localEulerRotation = Vector3.zero;

	[SerializeField]
	private Vector3 m_localScale = Vector3.one;

	[SerializeField]
	private SaveablePrefab m_prefabResourcePath;

	[NonSerialized]
	public GameObject m_currentInstance;

	[NonSerialized]
	public S_SnapToObjectPreview m_currentInstancePreviewScript;

	public bool IsCollidersDisabledInPreview
	{
		get
		{
			return m_isCollidersDisabledInPreview;
		}
		set
		{
			m_isCollidersDisabledInPreview = value;
		}
	}

	public Vector3 PreviewScale
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_previewScale;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_previewScale = value;
		}
	}

	public Vector3 PreviewEulerRotation
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_previewEulerRotation;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_previewEulerRotation = value;
		}
	}

	public Vector3 LocalPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_localPosition;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_localPosition = value;
		}
	}

	public Vector3 LocalEulerRotation
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_localEulerRotation;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_localEulerRotation = value;
		}
	}

	public Vector3 LocalScale
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_localScale;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_localScale = value;
		}
	}

	public SaveablePrefab PrefabResourcePath
	{
		get
		{
			return m_prefabResourcePath;
		}
		set
		{
			m_prefabResourcePath = value;
		}
	}

	public S_SnapToObjectPrefab()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
	}
}
