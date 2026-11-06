using LE_LevelEditor.Core;
using LE_LevelEditor.Events;
using LE_LevelEditor.Logic;
using Rewired;
using UnityEngine;

namespace LE_LevelEditor.UI;

public class LE_GUI3dTerrain : LE_GUI3dBase
{
	public int TERRAIN_LAYER = 14;

	private Terrain m_terrain;

	private LE_TerrainManager m_terrainMgr;

	private TerrainData m_defaultTerrainDataPrefab;

	private Projector m_brushProjector;

	private Texture2D m_brushAlphaTexture;

	private LE_LogicTerrain.Heightmap _heightmap;

	private float m_amount = 0.1f;

	private float m_size = 0.1f;

	private float m_targetRelativeValue = 0.5f;

	private bool m_isReadingTerrainSetHeight;

	private int m_isTerrainSetHeightReadInFrame = -1;

	private bool m_isDirectedSmooth;

	private float m_directedSmoothAngle;

	private LE_ETerrainEditMode m_editMode;

	private Vector3 m_lastCursorActiveScreenCoords = -1f * Vector3.one;

	public Terrain TerrainInstance => m_terrain;

	public LE_TerrainManager TerrainManager => m_terrainMgr;

	public TerrainData DefaultTerrainDataPrefab
	{
		get
		{
			return m_defaultTerrainDataPrefab;
		}
		set
		{
			m_defaultTerrainDataPrefab = value;
		}
	}

	public Projector BrushProjector
	{
		get
		{
			return m_brushProjector;
		}
		set
		{
			m_brushProjector = value;
			m_brushProjector.material = Object.Instantiate<Material>(m_brushProjector.material);
		}
	}

	public Texture2D BrushAlphaTexture
	{
		get
		{
			return m_brushAlphaTexture;
		}
		set
		{
			m_brushAlphaTexture = value;
			m_brushProjector.material.mainTexture = (Texture)(object)m_brushAlphaTexture;
		}
	}

	public LE_LogicTerrain.Heightmap Heightmap
	{
		get
		{
			return _heightmap;
		}
		set
		{
			_heightmap = value;
		}
	}

	public float Amount
	{
		get
		{
			return m_amount;
		}
		set
		{
			m_amount = value;
		}
	}

	public float Size
	{
		get
		{
			return m_size;
		}
		set
		{
			m_size = value;
			if (m_terrainMgr != null)
			{
				float num = 1f / (float)Mathf.Min(m_terrainMgr.TerrainData.heightmapResolution - 1, m_terrainMgr.TerrainData.heightmapResolution - 1);
				m_size = Mathf.Floor(m_size / num) * num;
			}
		}
	}

	public float TargetRelativeValue
	{
		get
		{
			return m_targetRelativeValue;
		}
		set
		{
			m_targetRelativeValue = value;
		}
	}

	public bool IsReadingTerrainSetHeight
	{
		get
		{
			return m_isReadingTerrainSetHeight;
		}
		set
		{
			m_isReadingTerrainSetHeight = value;
			m_isTerrainSetHeightReadInFrame = -1;
		}
	}

	public bool IsDirectedSmooth
	{
		get
		{
			return m_isDirectedSmooth;
		}
		set
		{
			m_isDirectedSmooth = value;
		}
	}

	public float DirectedSmoothAngle
	{
		get
		{
			return m_directedSmoothAngle;
		}
		set
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			m_directedSmoothAngle = value;
			if (((Component)m_brushProjector).transform.childCount > 0)
			{
				((Component)((Component)m_brushProjector).transform.GetChild(0)).transform.localRotation = Quaternion.Euler(Vector3.back * m_directedSmoothAngle);
			}
		}
	}

	public LE_ETerrainEditMode EditMode
	{
		get
		{
			return m_editMode;
		}
		set
		{
			m_editMode = value;
			if (m_editMode != LE_ETerrainEditMode.CHANGE_HEIGHT_TO_TARGET_VALUE)
			{
				m_isReadingTerrainSetHeight = false;
				m_isTerrainSetHeightReadInFrame = -1;
			}
		}
	}

	public override LE_EEditMode ActiveEditMode => LE_EEditMode.TERRAIN;

	public void SetTerrain(Terrain p_terrain)
	{
		if (m_terrainMgr != null)
		{
			Debug.LogError((object)"LE_GUI3dTerrain: SetTerrain: a terrain manager was already set and will be overwritten! Use 'RemoveTerrainManager' to reset the instance.");
		}
		m_terrain = p_terrain;
		m_terrainMgr = new LE_TerrainManager(p_terrain.terrainData);
		LE_EventInterface.LevelDataChanged(((Component)m_terrain).gameObject);
	}

	public void RemoveTerrainManager()
	{
		if ((Object)(object)m_terrain != (Object)null)
		{
			LE_EventInterface.LevelDataChanged(((Component)m_terrain).gameObject);
		}
		m_terrain = null;
		m_terrainMgr = null;
	}

	public override void SetCursorPosition(Vector3 p_cursorScreenCoords)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		m_cursorScreenCoords = p_cursorScreenCoords;
		m_cursorRay = Camera.main.ScreenPointToRay(p_cursorScreenCoords);
		if ((Object)(object)m_terrain != (Object)null)
		{
			SetIsCursorOverSomething(((Component)m_terrain).GetComponent<Collider>().Raycast(m_cursorRay, ref m_cursorHitInfo, float.MaxValue));
		}
		else
		{
			SetIsCursorOverSomething(p_isHit: false);
		}
	}

	public override void SetIsCursorAction(bool p_isCursorAction)
	{
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		if (m_isTerrainSetHeightReadInFrame != -1 && !p_isCursorAction && m_isTerrainSetHeightReadInFrame + 1 < Time.frameCount)
		{
			m_isReadingTerrainSetHeight = false;
			m_isTerrainSetHeightReadInFrame = -1;
		}
		if (((IsCursorOverSomething && IsInteractable) & p_isCursorAction) && m_lastCursorActiveScreenCoords != m_cursorScreenCoords)
		{
			m_lastCursorActiveScreenCoords = m_cursorScreenCoords;
			if (!((Object)(object)((Component)m_cursorHitInfo.transform).gameObject.GetComponent<Terrain>() != (Object)null))
			{
				return;
			}
			if (m_isReadingTerrainSetHeight)
			{
				Terrain component = ((Component)m_cursorHitInfo.transform).gameObject.GetComponent<Terrain>();
				float num = component.SampleHeight(m_cursorHitInfo.point);
				m_targetRelativeValue = Mathf.Clamp01(num / component.terrainData.size.y);
				m_isTerrainSetHeightReadInFrame = Time.frameCount;
				return;
			}
			switch (m_editMode)
			{
			case LE_ETerrainEditMode.CHANGE_HEIGHT:
				m_terrainMgr.ChangeHeight(Mathf.Sign(Amount) * Mathf.Max(0.002f, Amount * Amount) * Time.deltaTime * 2f, BrushAlphaTexture, Size, GetRelativeLocalLocation(m_cursorHitInfo));
				break;
			case LE_ETerrainEditMode.CHANGE_HEIGHT_TO_TARGET_VALUE:
				m_terrainMgr.ChangeHeight(Mathf.Max(0.002f, Amount * Amount) * Time.deltaTime * 2f, TargetRelativeValue, BrushAlphaTexture, Size, GetRelativeLocalLocation(m_cursorHitInfo));
				break;
			case LE_ETerrainEditMode.SMOOTH_HEIGHT:
			{
				int p_neighbourCount = 3 + 2 * Mathf.RoundToInt(Mathf.Abs(Amount) * 3f);
				m_terrainMgr.SmoothHeight(Time.deltaTime * 2f, p_neighbourCount, BrushAlphaTexture, Size, GetRelativeLocalLocation(m_cursorHitInfo), m_isDirectedSmooth, m_directedSmoothAngle);
				break;
			}
			default:
				Debug.LogError((object)"LE_GUI3dTerrain: unknown EditMode!");
				break;
			case LE_ETerrainEditMode.PROCEDURAL:
				break;
			}
		}
		else
		{
			m_lastCursorActiveScreenCoords = -1f * Vector3.one;
		}
	}

	public void HideCursor()
	{
		BrushProjector.orthographicSize = 0f;
		if (((Component)BrushProjector).transform.childCount > 0)
		{
			((Component)((Component)BrushProjector).transform.GetChild(0)).gameObject.SetActive(false);
		}
		SetIsCursorOverSomething(p_isHit: false);
	}

	private void Start()
	{
		if ((Object)(object)m_brushProjector == (Object)null)
		{
			Debug.LogError((object)"LE_GUI3dTerrain: m_brushProjector was not initialized!");
		}
		if ((Object)(object)m_brushAlphaTexture == (Object)null)
		{
			Debug.LogError((object)"LE_GUI3dTerrain: m_brushAlphaTexture was not initialized!");
		}
	}

	private void Update()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		UpdateBrushProjector(IsCursorOverSomething, m_cursorHitInfo);
		if (ReInput.players.GetPlayer(0).GetButtonDown(53))
		{
			Focus();
		}
	}

	public void Focus()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		if (IsCursorOverSomething)
		{
			Vector3 point = m_cursorHitInfo.point;
			Vector3 val = new Vector3(m_brushProjector.orthographicSize, m_brushProjector.orthographicSize * 0.7f, m_brushProjector.orthographicSize);
			Vector3 val2 = ((Component)Camera.main).transform.position - point;
			float num = Mathf.Max(new float[3] { val.x, val.y, val.z }) * 3f;
			((Component)Camera.main).transform.position = point + val2.normalized * num;
			if (((Component)Camera.main).transform.position.y < point.y)
			{
				Vector3 position = ((Component)Camera.main).transform.position;
				position.y = point.y;
				((Component)Camera.main).transform.position = position;
			}
			((Component)Camera.main).transform.LookAt(point, Vector3.up);
		}
	}

	private void UpdateBrushProjector(bool p_isHit, RaycastHit p_hitInfo)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		if (p_isHit)
		{
			Vector3 size = m_terrainMgr.TerrainData.size;
			Vector2 relativeLocalLocation = GetRelativeLocalLocation(p_hitInfo);
			((Component)BrushProjector).transform.position = new Vector3(p_hitInfo.transform.position.x + relativeLocalLocation.y * size.x, p_hitInfo.point.y, p_hitInfo.transform.position.z + relativeLocalLocation.x * size.z);
			BrushProjector.orthographicSize = m_terrainMgr.TerrainData.size.z * m_size * 0.5f;
			BrushProjector.aspectRatio = m_terrainMgr.TerrainData.size.x / m_terrainMgr.TerrainData.size.z;
			if (((Component)BrushProjector).transform.childCount > 0)
			{
				((Component)((Component)BrushProjector).transform.GetChild(0)).gameObject.SetActive(m_editMode == LE_ETerrainEditMode.SMOOTH_HEIGHT && IsDirectedSmooth);
			}
		}
		else
		{
			BrushProjector.orthographicSize = 0f;
			if (((Component)BrushProjector).transform.childCount > 0)
			{
				((Component)((Component)BrushProjector).transform.GetChild(0)).gameObject.SetActive(false);
			}
		}
	}

	private Vector2 GetRelativeLocalLocation(RaycastHit hit)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		Terrain component = ((Component)hit.transform).gameObject.GetComponent<Terrain>();
		Vector3 size = component.terrainData.size;
		Vector3 val = hit.transform.InverseTransformPoint(hit.point);
		Vector2 zero = Vector2.zero;
		zero.x = val.z / size.z;
		zero.y = val.x / size.x;
		float num = 1f / (float)(component.terrainData.heightmapResolution - 1);
		float num2 = 1f / (float)(component.terrainData.heightmapResolution - 1);
		zero.x = Mathf.Round(zero.x / num) * num;
		zero.y = Mathf.Round(zero.y / num2) * num2;
		return zero;
	}

	public LE_GUI3dTerrain()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
	}
}
