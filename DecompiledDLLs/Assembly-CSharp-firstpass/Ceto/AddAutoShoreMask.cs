using Ceto.Common.Containers.Interpolation;
using UnityEngine;

namespace Ceto;

[AddComponentMenu("Ceto/Overlays/AddAutoShoreMask")]
[DisallowMultipleComponent]
public class AddAutoShoreMask : AddWaveOverlayBase
{
	public bool ignoreQuerys;

	public bool textureFoam = true;

	[Range(0.1f, 100f)]
	public float heightSpread = 10f;

	[Range(0.1f, 10f)]
	public float foamSpread = 2f;

	[Range(0.1f, 10f)]
	public float clipOffset = 4f;

	public int resolution = 1024;

	public bool useHeightMask = true;

	[Range(0f, 1f)]
	public float heightAlpha = 0.9f;

	public bool useNormalMask = true;

	[Range(0f, 1f)]
	public float normalAlpha = 0.8f;

	public bool useEdgeFoam = true;

	[Range(0f, 10f)]
	public float edgeFoamAlpha = 1f;

	public bool useFoamMask = true;

	[Range(0f, 1f)]
	public float foamMaskAlpha = 1f;

	public bool useClipMask = true;

	private bool m_registered;

	private float m_width;

	private float m_height;

	private Texture2D m_heightMask;

	private Texture2D m_edgeFoam;

	private Texture2D m_clipMask;

	private float m_heightSpread;

	private float m_foamSpread;

	private float m_clipOffset;

	private float m_resolution;

	protected override void Start()
	{
		m_overlays.Add(new WaveOverlay());
		UpdateOverlay();
	}

	protected override void Update()
	{
		if (m_overlays != null && m_overlays.Count == 1)
		{
			UpdateOverlay();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Release();
	}

	private void UpdateOverlay()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Ocean.Instance != (Object)null && (!m_registered || SettingsChanged()))
		{
			CreateShoreMasks();
		}
		Vector2 val = new Vector2(m_width * 0.5f, m_height * 0.5f);
		Vector3 position = ((Component)this).transform.position;
		position.x += val.x;
		position.z += val.y;
		m_overlays[0].Position = position;
		m_overlays[0].HalfSize = val;
		m_overlays[0].HeightTex.maskAlpha = (useHeightMask ? heightAlpha : 0f);
		m_overlays[0].HeightTex.ignoreQuerys = ignoreQuerys;
		m_overlays[0].NormalTex.maskAlpha = (useNormalMask ? normalAlpha : 0f);
		m_overlays[0].NormalTex.maskMode = OVERLAY_MASK_MODE.WAVES_AND_OVERLAY_BLEND;
		m_overlays[0].FoamTex.alpha = (useEdgeFoam ? edgeFoamAlpha : 0f);
		m_overlays[0].FoamTex.textureFoam = textureFoam;
		m_overlays[0].FoamTex.maskAlpha = (useFoamMask ? foamMaskAlpha : 0f);
		m_overlays[0].ClipTex.alpha = (useClipMask ? 1f : 0f);
		m_overlays[0].ClipTex.ignoreQuerys = ignoreQuerys;
		m_overlays[0].UpdateOverlay();
	}

	private void CreateShoreMasks()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Release();
		Terrain component = ((Component)this).GetComponent<Terrain>();
		if ((Object)(object)component == (Object)null)
		{
			Ocean.LogWarning("The AddAutoShoreMask script must be attached to a component with a Terrain. The shore mask will not be created.");
			((Behaviour)this).enabled = false;
			return;
		}
		if ((Object)(object)component.terrainData == (Object)null)
		{
			Ocean.LogWarning("The terrain data is null. The shore mask will not be created.");
			((Behaviour)this).enabled = false;
			return;
		}
		Vector3 size = component.terrainData.size;
		resolution = Mathf.Clamp(resolution, 32, 4096);
		m_width = size.x;
		m_height = size.z;
		float level = Ocean.Instance.level;
		float[] data = ShoreMaskGenerator.CreateHeightMap(component);
		int heightmapResolution = component.terrainData.heightmapResolution;
		InterpolatedArray2f heightMap = new InterpolatedArray2f(data, heightmapResolution, heightmapResolution, 1, wrap: false);
		if (useHeightMask || useNormalMask || useFoamMask)
		{
			m_heightMask = ShoreMaskGenerator.CreateMask(heightMap, resolution, resolution, level, heightSpread, (TextureFormat)5);
		}
		if (useEdgeFoam)
		{
			m_edgeFoam = ShoreMaskGenerator.CreateMask(heightMap, resolution, resolution, level, foamSpread, (TextureFormat)5);
		}
		if (useClipMask)
		{
			m_clipMask = ShoreMaskGenerator.CreateClipMask(heightMap, resolution, resolution, level + clipOffset, (TextureFormat)5);
		}
		if (useHeightMask)
		{
			m_overlays[0].HeightTex.mask = (Texture)(object)m_heightMask;
		}
		if (useNormalMask)
		{
			m_overlays[0].NormalTex.mask = (Texture)(object)m_heightMask;
		}
		if (useFoamMask)
		{
			m_overlays[0].FoamTex.mask = (Texture)(object)m_heightMask;
		}
		if (useEdgeFoam)
		{
			m_overlays[0].FoamTex.tex = (Texture)(object)m_edgeFoam;
		}
		if (useClipMask)
		{
			m_overlays[0].ClipTex.tex = (Texture)(object)m_clipMask;
		}
		if (!m_registered)
		{
			Ocean.Instance.OverlayManager.Add(m_overlays[0]);
			m_registered = true;
		}
		m_heightSpread = heightSpread;
		m_foamSpread = foamSpread;
		m_clipOffset = clipOffset;
		m_resolution = resolution;
	}

	private bool SettingsChanged()
	{
		if (m_heightSpread != heightSpread)
		{
			return true;
		}
		if (m_foamSpread != foamSpread)
		{
			return true;
		}
		if (m_clipOffset != clipOffset)
		{
			return true;
		}
		if (m_resolution != (float)resolution)
		{
			return true;
		}
		return false;
	}

	private void Release()
	{
		if ((Object)(object)m_heightMask != (Object)null)
		{
			Object.Destroy((Object)(object)m_heightMask);
			m_heightMask = null;
		}
		if ((Object)(object)m_edgeFoam != (Object)null)
		{
			Object.Destroy((Object)(object)m_edgeFoam);
			m_edgeFoam = null;
		}
		if ((Object)(object)m_clipMask != (Object)null)
		{
			Object.Destroy((Object)(object)m_clipMask);
			m_clipMask = null;
		}
		if (m_overlays != null && m_overlays.Count == 1)
		{
			m_overlays[0].HeightTex.mask = null;
			m_overlays[0].NormalTex.mask = null;
			m_overlays[0].FoamTex.mask = null;
			m_overlays[0].FoamTex.tex = null;
			m_overlays[0].ClipTex.tex = null;
		}
	}
}
