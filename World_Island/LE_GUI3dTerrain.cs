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
			m_brushProjector.material = Object.Instantiate(m_brushProjector.material);
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
			m_brushProjector.material.mainTexture = m_brushAlphaTexture;
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
			m_directedSmoothAngle = value;
			if (m_brushProjector.transform.childCount > 0)
			{
				m_brushProjector.transform.GetChild(0).transform.localRotation = Quaternion.Euler(Vector3.back * m_directedSmoothAngle);
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
			Debug.LogError("LE_GUI3dTerrain: SetTerrain: a terrain manager was already set and will be overwritten! Use 'RemoveTerrainManager' to reset the instance.");
		}
		m_terrain = p_terrain;
		m_terrainMgr = new LE_TerrainManager(p_terrain.terrainData);
		LE_EventInterface.LevelDataChanged(m_terrain.gameObject);
	}

	public void RemoveTerrainManager()
	{
		if (m_terrain != null)
		{
			LE_EventInterface.LevelDataChanged(m_terrain.gameObject);
		}
		m_terrain = null;
		m_terrainMgr = null;
	}

	public override void SetCursorPosition(Vector3 p_cursorScreenCoords)
	{
		m_cursorScreenCoords = p_cursorScreenCoords;
		m_cursorRay = Camera.main.ScreenPointToRay(p_cursorScreenCoords);
		if (m_terrain != null)
		{
			SetIsCursorOverSomething(m_terrain.GetComponent<Collider>().Raycast(m_cursorRay, out m_cursorHitInfo, float.MaxValue));
		}
		else
		{
			SetIsCursorOverSomething(p_isHit: false);
		}
	}

	public override void SetIsCursorAction(bool p_isCursorAction)
	{
		if (m_isTerrainSetHeightReadInFrame != -1 && !p_isCursorAction && m_isTerrainSetHeightReadInFrame + 1 < Time.frameCount)
		{
			m_isReadingTerrainSetHeight = false;
			m_isTerrainSetHeightReadInFrame = -1;
		}
		if (base.IsCursorOverSomething && base.IsInteractable && p_isCursorAction && m_lastCursorActiveScreenCoords != m_cursorScreenCoords)
		{
			m_lastCursorActiveScreenCoords = m_cursorScreenCoords;
			if (!(m_cursorHitInfo.transform.gameObject.GetComponent<Terrain>() != null))
			{
				return;
			}
			if (m_isReadingTerrainSetHeight)
			{
				Terrain component = m_cursorHitInfo.transform.gameObject.GetComponent<Terrain>();
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
				Debug.LogError("LE_GUI3dTerrain: unknown EditMode!");
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
		if (BrushProjector.transform.childCount > 0)
		{
			BrushProjector.transform.GetChild(0).gameObject.SetActive(value: false);
		}
		SetIsCursorOverSomething(p_isHit: false);
	}

	private void Start()
	{
		if (m_brushProjector == null)
		{
			Debug.LogError("LE_GUI3dTerrain: m_brushProjector was not initialized!");
		}
		if (m_brushAlphaTexture == null)
		{
			Debug.LogError("LE_GUI3dTerrain: m_brushAlphaTexture was not initialized!");
		}
	}

	private void Update()
	{
		UpdateBrushProjector(base.IsCursorOverSomething, m_cursorHitInfo);
		if (ReInput.players.GetPlayer(0).GetButtonDown(53))
		{
			Focus();
		}
	}

	public void Focus()
	{
		if (base.IsCursorOverSomething)
		{
			Vector3 point = m_cursorHitInfo.point;
			Vector3 vector = new Vector3(m_brushProjector.orthographicSize, m_brushProjector.orthographicSize * 0.7f, m_brushProjector.orthographicSize);
			Vector3 vector2 = Camera.main.transform.position - point;
			float num = Mathf.Max(vector.x, vector.y, vector.z) * 3f;
			Camera.main.transform.position = point + vector2.normalized * num;
			if (Camera.main.transform.position.y < point.y)
			{
				Vector3 position = Camera.main.transform.position;
				position.y = point.y;
				Camera.main.transform.position = position;
			}
			Camera.main.transform.LookAt(point, Vector3.up);
		}
	}

	private void UpdateBrushProjector(bool p_isHit, RaycastHit p_hitInfo)
	{
		if (p_isHit)
		{
			Vector3 size = m_terrainMgr.TerrainData.size;
			Vector2 relativeLocalLocation = GetRelativeLocalLocation(p_hitInfo);
			BrushProjector.transform.position = new Vector3(p_hitInfo.transform.position.x + relativeLocalLocation.y * size.x, p_hitInfo.point.y, p_hitInfo.transform.position.z + relativeLocalLocation.x * size.z);
			BrushProjector.orthographicSize = m_terrainMgr.TerrainData.size.z * m_size * 0.5f;
			BrushProjector.aspectRatio = m_terrainMgr.TerrainData.size.x / m_terrainMgr.TerrainData.size.z;
			if (BrushProjector.transform.childCount > 0)
			{
				BrushProjector.transform.GetChild(0).gameObject.SetActive(m_editMode == LE_ETerrainEditMode.SMOOTH_HEIGHT && IsDirectedSmooth);
			}
		}
		else
		{
			BrushProjector.orthographicSize = 0f;
			if (BrushProjector.transform.childCount > 0)
			{
				BrushProjector.transform.GetChild(0).gameObject.SetActive(value: false);
			}
		}
	}

	private Vector2 GetRelativeLocalLocation(RaycastHit hit)
	{
		Terrain component = hit.transform.gameObject.GetComponent<Terrain>();
		Vector3 size = component.terrainData.size;
		Vector3 vector = hit.transform.InverseTransformPoint(hit.point);
		Vector2 zero = Vector2.zero;
		zero.x = vector.z / size.z;
		zero.y = vector.x / size.x;
		float num = 1f / (float)(component.terrainData.heightmapResolution - 1);
		float num2 = 1f / (float)(component.terrainData.heightmapResolution - 1);
		zero.x = Mathf.Round(zero.x / num) * num;
		zero.y = Mathf.Round(zero.y / num2) * num2;
		return zero;
	}
}
