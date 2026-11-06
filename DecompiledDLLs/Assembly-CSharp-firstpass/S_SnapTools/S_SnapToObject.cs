using System;
using Beam.Serialization;
using MyUtility;
using UnityEngine;

namespace S_SnapTools;

public class S_SnapToObject : MonoBehaviour
{
	private enum EAnimationState
	{
		ZOOM_IN,
		ZOOM_OUT,
		IDLE
	}

	public static EventHandler<S_SnapToObjectEventArgs> OnNewObjectSnapped;

	public static EventHandler<S_SnapToObjectEventArgs> OnPreviewObjectInstantiated;

	private static S_SnapToObject s_openSelection;

	private const float ZOOM_OUT_TIME = 0.4f;

	private const float ZOOM_IN_TIME = 0.2f;

	private readonly Vector3 MIN_SCALE = Vector3.one * 0.0001f;

	[SerializeField]
	private float PREVIEW_RELATIVE_SCREEN_DISTANCE = 0.3f;

	[SerializeField]
	private float PREVIEW_RELATIVE_SCREEN_CLICK_AREA = 0.2f;

	[SerializeField]
	private float PREVIEW_RELATIVE_MOVE_TO_CAMERA_DISTANCE = 0.5f;

	[SerializeField]
	private S_SnapToObjectPrefab[] m_prefabs = new S_SnapToObjectPrefab[1];

	[SerializeField]
	private bool m_isDeactivatedAfterSnap;

	[SerializeField]
	private bool m_isDestroyedAfterSnap = true;

	[SerializeField]
	private bool m_isSetNameToResourcePath = true;

	[SerializeField]
	private bool m_isDrawUI = true;

	[SerializeField]
	private Material m_uiMaterialLine;

	[SerializeField]
	private Material m_uiMaterialFill;

	private int m_snapCounter;

	private bool m_isSelectionOpen;

	private EAnimationState m_animState = EAnimationState.IDLE;

	private float m_zoomAnimationTime = -1f;

	private Transform m_previewRoot;

	private Camera m_cam;

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

	public bool IsDeactivatedAfterSnap
	{
		get
		{
			return m_isDeactivatedAfterSnap;
		}
		set
		{
			m_isDeactivatedAfterSnap = value;
		}
	}

	public bool IsDestroyedAfterSnap
	{
		get
		{
			return m_isDestroyedAfterSnap;
		}
		set
		{
			m_isDestroyedAfterSnap = value;
		}
	}

	public bool IsSetNameToResourcePath
	{
		get
		{
			return m_isSetNameToResourcePath;
		}
		set
		{
			m_isSetNameToResourcePath = value;
		}
	}

	public bool IsDrawUI
	{
		get
		{
			return m_isDrawUI;
		}
		set
		{
			m_isDrawUI = value;
		}
	}

	public Material UImaterialLine
	{
		get
		{
			return m_uiMaterialLine;
		}
		set
		{
			m_uiMaterialLine = value;
		}
	}

	public Material UImaterialFill
	{
		get
		{
			return m_uiMaterialFill;
		}
		set
		{
			m_uiMaterialFill = value;
		}
	}

	public int SnapCounter => m_snapCounter;

	public bool IsPrefabSelectionOpen => m_isSelectionOpen;

	public Transform PreviewRoot
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)m_previewRoot == (Object)null)
			{
				m_previewRoot = new GameObject("S_SnapToObject Preview Root").transform;
				m_previewRoot.position = ((Component)this).transform.position;
			}
			return m_previewRoot;
		}
	}

	private Camera Cam
	{
		get
		{
			if ((Object)(object)m_cam == (Object)null)
			{
				m_cam = Camera.main;
				if ((Object)(object)m_cam == (Object)null)
				{
					Debug.LogError((object)"S_SnapToObject: could not find main camera!");
				}
			}
			return m_cam;
		}
	}

	public S_SnapToObject()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (m_prefabs != null && m_prefabs.Length == 1)
		{
			m_prefabs[0] = new S_SnapToObjectPrefab();
		}
	}

	public void IncSnapCounter()
	{
		if (m_isDeactivatedAfterSnap)
		{
			((Component)this).gameObject.SetActive(false);
		}
		else if (m_isDestroyedAfterSnap)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		m_snapCounter++;
	}

	public void DecSnapCounter()
	{
		if (m_snapCounter > 0)
		{
			m_snapCounter--;
			if (m_isDeactivatedAfterSnap && m_snapCounter == 0)
			{
				((Component)this).gameObject.SetActive(true);
			}
		}
	}

	public void OpenSelection()
	{
		if ((Object)(object)s_openSelection != (Object)null)
		{
			s_openSelection.CloseSelection();
		}
		s_openSelection = this;
		m_isSelectionOpen = true;
		m_animState = EAnimationState.ZOOM_OUT;
		m_zoomAnimationTime = 0f;
		for (int i = 0; i < m_prefabs.Length; i++)
		{
			CreatePrefabPreview(m_prefabs[i]);
		}
	}

	public void CloseSelection()
	{
		if ((Object)(object)s_openSelection == (Object)(object)this)
		{
			s_openSelection = null;
		}
		m_isSelectionOpen = false;
		m_animState = EAnimationState.ZOOM_IN;
		m_zoomAnimationTime = 0f;
	}

	private void Start()
	{
		UtilityClickTouchDetector utilityClickTouchDetector = ((Component)this).gameObject.AddComponent<UtilityClickTouchDetector>();
		utilityClickTouchDetector.m_onClick = (Action)Delegate.Combine(utilityClickTouchDetector.m_onClick, new Action(OnClicked));
	}

	private void Update()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (IsPrefabSelectionOpen)
		{
			PreviewRoot.position = ((Component)this).transform.position;
		}
		else if ((Object)(object)m_previewRoot != (Object)null && m_animState == EAnimationState.IDLE)
		{
			Object.Destroy((Object)(object)((Component)m_previewRoot).gameObject);
			m_previewRoot = null;
		}
		if (m_animState == EAnimationState.ZOOM_OUT)
		{
			float p_fade = UpdateAnimationTimeFadeState(0.4f);
			UpdateZoomAnimation(p_fade);
		}
		else if (m_animState == EAnimationState.ZOOM_IN)
		{
			float num = UpdateAnimationTimeFadeState(0.2f);
			if (num < 1f)
			{
				UpdateZoomAnimation(1f - num);
			}
			else
			{
				DestroyAllPreviews();
			}
		}
		else if (IsPrefabSelectionOpen)
		{
			UpdateZoomAnimation(1f);
		}
	}

	private void DestroyAllPreviews()
	{
		for (int i = 0; i < m_prefabs.Length; i++)
		{
			Object.Destroy((Object)(object)m_prefabs[i].m_currentInstance);
		}
	}

	private float UpdateAnimationTimeFadeState(float p_animTime)
	{
		m_zoomAnimationTime += Time.deltaTime;
		if (m_zoomAnimationTime >= p_animTime)
		{
			m_animState = EAnimationState.IDLE;
			m_zoomAnimationTime = -1f;
			return 1f;
		}
		return m_zoomAnimationTime / p_animTime;
	}

	private void UpdateZoomAnimation(float p_fade)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Cam != (Object)null))
		{
			return;
		}
		Vector3 val = Cam.WorldToScreenPoint(((Component)this).transform.position);
		float num = 360f / (float)m_prefabs.Length;
		for (int i = 0; i < m_prefabs.Length; i++)
		{
			GameObject currentInstance = m_prefabs[i].m_currentInstance;
			if ((Object)(object)currentInstance != (Object)null && (Object)(object)m_prefabs[i].m_currentInstancePreviewScript != (Object)null)
			{
				Vector3 val2 = (float)Screen.height * PREVIEW_RELATIVE_SCREEN_DISTANCE * new Vector3(Mathf.Cos((float)Math.PI / 180f * num * (float)i * p_fade), Mathf.Sin((float)Math.PI / 180f * num * (float)i * p_fade), 0f);
				val2 = Cam.ScreenToWorldPoint(val + val2);
				val2 = val2 * (1f - PREVIEW_RELATIVE_MOVE_TO_CAMERA_DISTANCE) + ((Component)Cam).transform.position * PREVIEW_RELATIVE_MOVE_TO_CAMERA_DISTANCE;
				currentInstance.transform.localScale = Vector3.Lerp(MIN_SCALE, m_prefabs[i].PreviewScale * m_prefabs[i].m_currentInstancePreviewScript.WorldRadius, p_fade);
				currentInstance.transform.position = Vector3.Lerp(((Component)this).transform.position, val2, p_fade);
				Quaternion val3 = Quaternion.LookRotation(((Component)Cam).transform.position - currentInstance.transform.position, Vector3.up);
				val3 *= Quaternion.Euler(m_prefabs[i].PreviewEulerRotation);
				currentInstance.transform.rotation = val3;
			}
		}
	}

	private void OnClicked()
	{
		if (!m_isSelectionOpen)
		{
			OpenSelection();
		}
		else
		{
			CloseSelection();
		}
	}

	private void OnDisable()
	{
		if ((Object)(object)m_previewRoot != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)m_previewRoot).gameObject);
			m_previewRoot = null;
		}
		DestroyAllPreviews();
		if ((Object)(object)s_openSelection == (Object)(object)this)
		{
			s_openSelection = null;
		}
		m_isSelectionOpen = false;
		m_animState = EAnimationState.IDLE;
		m_zoomAnimationTime = -1f;
	}

	private void CreatePrefabPreview(S_SnapToObjectPrefab p_prefab)
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)p_prefab.m_currentInstance != (Object)null)
		{
			Debug.LogWarning((object)("S_SnapToObject: CreatePrefabPreview: there was already an instance of this prefab '" + ((Object)p_prefab.m_currentInstance).name + "'! Old instance will be destroyed..."));
			Object.Destroy((Object)(object)p_prefab.m_currentInstance);
		}
		if ((Object)(object)p_prefab.PrefabResourcePath != (Object)null)
		{
			p_prefab.m_currentInstance = Object.Instantiate<GameObject>(((Component)Beam.Serialization.Prefabs.CreatePrefabFromId(p_prefab.PrefabResourcePath.PrefabId)).gameObject);
			p_prefab.m_currentInstance.transform.parent = PreviewRoot;
			p_prefab.m_currentInstance.transform.localPosition = Vector3.zero;
			p_prefab.m_currentInstance.transform.localScale = Vector3.one;
			if (p_prefab.IsCollidersDisabledInPreview)
			{
				Collider[] componentsInChildren = p_prefab.m_currentInstance.GetComponentsInChildren<Collider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = false;
				}
			}
			p_prefab.m_currentInstance.transform.localScale = MIN_SCALE;
			p_prefab.m_currentInstancePreviewScript = p_prefab.m_currentInstance.AddComponent<S_SnapToObjectPreview>();
			p_prefab.m_currentInstancePreviewScript.Init(m_isDrawUI, PREVIEW_RELATIVE_SCREEN_CLICK_AREA, () =>
			{
				PlacePrefab(p_prefab);
			}, m_uiMaterialLine, m_uiMaterialFill);
			if (OnPreviewObjectInstantiated != null)
			{
				OnPreviewObjectInstantiated(this, new S_SnapToObjectEventArgs(this, p_prefab.m_currentInstance));
			}
		}
		else
		{
			Debug.LogError((object)"S_SnapToObject: CreatePrefabPreview: no prefab assigned!");
		}
	}

	private void PlacePrefab(S_SnapToObjectPrefab p_prefab)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (m_isSelectionOpen)
		{
			DestroyAllPreviews();
			if ((Object)(object)s_openSelection == (Object)(object)this)
			{
				s_openSelection = null;
			}
			m_isSelectionOpen = false;
			m_animState = EAnimationState.IDLE;
			m_zoomAnimationTime = -1f;
		}
		GameObject val = Object.Instantiate<GameObject>(((Component)Beam.Serialization.Prefabs.CreatePrefabFromId(p_prefab.PrefabResourcePath.PrefabId)).gameObject);
		val.transform.localRotation = ((Component)this).transform.rotation;
		val.transform.Rotate(p_prefab.LocalEulerRotation);
		val.transform.localScale = Vector3.Scale(p_prefab.LocalScale, ((Component)this).transform.lossyScale);
		val.transform.position = ((Component)this).transform.position + val.transform.TransformPoint(p_prefab.LocalPosition);
		IncSnapCounter();
		if (OnNewObjectSnapped != null)
		{
			OnNewObjectSnapped(this, new S_SnapToObjectEventArgs(this, val));
		}
	}
}
