using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Ceto.Common.Threading.Scheduling;
using Ceto.Common.Unity.Utility;
using UnityEngine;

namespace Ceto;

[AddComponentMenu("Ceto/Components/Ocean")]
[DisallowMultipleComponent]
public class Ocean : MonoBehaviour, ICoroutine
{
	public static readonly bool DISABLE_FOURIER_MULTITHREADING = false;

	public static readonly bool DISABLE_PROJECTED_GRID_BORDER = false;

	public static bool DISABLE_ALL_MULTITHREADING = false;

	public static readonly bool DISABLE_PROJECTION_FLIPPING = true;

	public static readonly bool DISABLE_PROJECT_SCENE_VIEW = false;

	public static readonly string VERSION = "1.1.3";

	public static string OCEAN_LAYER = "Water";

	public static readonly string REFLECTION_TEXTURE_NAME0 = "Ceto_Reflections0";

	public static readonly string REFLECTION_TEXTURE_NAME1 = "Ceto_Reflections1";

	public static readonly string REFRACTION_GRAB_TEXTURE_NAME = "Ceto_RefractionGrab";

	public static readonly string DEPTH_GRAB_TEXTURE_NAME = "Ceto_DepthBuffer";

	public static readonly string OCEAN_DEPTH_TEXTURE_NAME0 = "Ceto_OceanDepth0";

	public static readonly string OCEAN_DEPTH_TEXTURE_NAME1 = "Ceto_OceanDepth1";

	public static readonly string OCEAN_MASK_TEXTURE_NAME0 = "Ceto_OceanMask0";

	public static readonly string OCEAN_MASK_TEXTURE_NAME1 = "Ceto_OceanMask1";

	public static readonly string NORMAL_FADE_TEXTURE_NAME = "Ceto_NormalFade";

	public const float MAX_SPECTRUM_WAVE_HEIGHT = 40f;

	public const float MAX_OVERLAY_WAVE_HEIGHT = 20f;

	public bool disableWarnings;

	public bool disableInfo;

	public bool doublePrecisionProjection = true;

	public bool tightProjectionFit = true;

	public GameObject m_sun;

	public float level;

	public OVERLAY_MAP_SIZE heightOverlaySize = OVERLAY_MAP_SIZE.HALF;

	public OVERLAY_MAP_SIZE normalOverlaySize = OVERLAY_MAP_SIZE.FULL;

	public OVERLAY_MAP_SIZE foamOverlaySize = OVERLAY_MAP_SIZE.FULL;

	public OVERLAY_MAP_SIZE clipOverlaySize = OVERLAY_MAP_SIZE.HALF;

	public OVERLAY_BLEND_MODE heightBlendMode;

	public OVERLAY_BLEND_MODE foamBlendMode;

	public Color defaultSkyColor = Color32.op_Implicit(new Color32((byte)96, (byte)147, (byte)210, byte.MaxValue));

	public Color defaultOceanColor = Color32.op_Implicit(new Color32((byte)0, (byte)19, (byte)30, byte.MaxValue));

	[Range(0f, 360f)]
	public float windDir;

	[Range(0f, 1f)]
	public float specularRoughness = 0.2f;

	[Range(0f, 1f)]
	public float specularIntensity = 0.2f;

	[Range(0f, 10f)]
	public float fresnelPower = 5f;

	[Range(0f, 1f)]
	public float minFresnel = 0.02f;

	public Color foamTint = Color.white;

	[Range(0f, 3f)]
	public float foamIntensity = 1f;

	public FoamTexture foamTexture0;

	public FoamTexture foamTexture1;

	[HideInInspector]
	public Shader waveOverlaySdr;

	private Vector3 m_positionOffset;

	private Dictionary<Camera, CameraData> m_cameraData = new Dictionary<Camera, CameraData>();

	private Material m_waveOverlayMat;

	private Scheduler m_scheduler;

	private WaveQuery m_query = new WaveQuery(0f, 0f);

	public static Ocean Instance { get; private set; }

	public static float MAX_WAVE_HEIGHT => 60f;

	public Vector3 WindDirVector
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public ProjectedGrid Grid { get; set; }

	public PlanarReflection Reflection { get; set; }

	public WaveSpectrum Spectrum { get; set; }

	public UnderWater UnderWater { get; set; }

	public IOceanTime OceanTime { get; set; }

	public IProjection Projection { get; private set; }

	public Vector3 PositionOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_positionOffset;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			m_positionOffset = value;
			Shader.SetGlobalVector("Ceto_PosOffset", Vector4.op_Implicit(m_positionOffset));
		}
	}

	public int CameraCount => m_cameraData.Count;

	public OverlayManager OverlayManager { get; private set; }

	public bool WasError { get; private set; }

	private void Awake()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected Obj, but got Unknown
		try
		{
			if ((Object)(object)Instance != (Object)null)
			{
				throw new InvalidOperationException("There can only be one ocean instance.");
			}
			Instance = this;
			if (DISABLE_ALL_MULTITHREADING)
			{
				LogInfo("Disabled multithreading is on");
			}
			if (DISABLE_FOURIER_MULTITHREADING)
			{
				LogInfo("Disabled Fourier multithreading is on");
			}
			if (DISABLE_PROJECTED_GRID_BORDER)
			{
				LogInfo("Disabled projection border is on");
			}
			if (DISABLE_PROJECTION_FLIPPING)
			{
				LogInfo("Disabled flipping is on");
			}
			if (DISABLE_PROJECT_SCENE_VIEW)
			{
				LogInfo("Disabled project scene is on");
			}
			WindDirVector = CalculateWindDirVector();
			if (doublePrecisionProjection)
			{
				Projection = new Projection3d(this);
			}
			else
			{
				Projection = new Projection3f(this);
			}
			OceanTime = new OceanTime();
			m_waveOverlayMat = new Material(waveOverlaySdr);
			OverlayManager = new OverlayManager(m_waveOverlayMat);
			m_scheduler = new Scheduler(100, 100, this);
		}
		catch (Exception ex)
		{
			LogError(ex.ToString());
			DisableOcean();
		}
	}

	private void Start()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Matrix4x4 identity = Matrix4x4.identity;
			identity.m00 = 2f;
			identity.m03 = -1f;
			identity.m11 = 2f;
			identity.m13 = -1f;
			for (int i = 0; i < 4; i++)
			{
				identity[1, i] = 0f - identity[1, i];
			}
			Shader.SetGlobalMatrix("Ceto_T2S", identity);
			Shader.SetGlobalTexture("Ceto_Overlay_NormalMap", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_Overlay_HeightMap", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_Overlay_FoamMap", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_Overlay_ClipMap", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture(REFRACTION_GRAB_TEXTURE_NAME, (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture(DEPTH_GRAB_TEXTURE_NAME, (Texture)(object)Texture2D.whiteTexture);
			Shader.SetGlobalTexture(OCEAN_MASK_TEXTURE_NAME0, (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture(OCEAN_MASK_TEXTURE_NAME1, (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture(OCEAN_DEPTH_TEXTURE_NAME0, (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture(OCEAN_DEPTH_TEXTURE_NAME1, (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture(NORMAL_FADE_TEXTURE_NAME, (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_SlopeMap0", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_SlopeMap1", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_DisplacementMap0", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_DisplacementMap1", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_DisplacementMap2", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_DisplacementMap3", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_FoamMap0", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_FoamMap1", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalVector("Ceto_GridSizes", Vector4.one);
			Shader.SetGlobalVector("Ceto_GridScale", Vector4.one);
			Shader.SetGlobalVector("Ceto_Choppyness", Vector4.one);
			Shader.SetGlobalFloat("Ceto_MapSize", 1f);
			Shader.SetGlobalColor("Ceto_FoamTint", Color.white);
			Shader.SetGlobalTexture("Ceto_FoamTexture0", (Texture)(object)Texture2D.whiteTexture);
			Shader.SetGlobalTexture("Ceto_FoamTexture1", (Texture)(object)Texture2D.whiteTexture);
			Shader.SetGlobalVector("Ceto_FoamTextureScale0", Vector4.one);
			Shader.SetGlobalVector("Ceto_FoamTextureScale1", Vector4.one);
			Shader.SetGlobalFloat("Ceto_MaxWaveHeight", 40f);
			Shader.SetGlobalVector("Ceto_PosOffset", Vector4.op_Implicit(Vector3.zero));
			Shader.SetGlobalTexture("Ceto_CausticTexture", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalVector("Ceto_CausticTextureScale", new Vector4(1f, 1f, 0f, 0f));
			Shader.SetGlobalColor("Ceto_CausticTint", Color.white);
			Shader.SetGlobalFloat("Ceto_Stero_Enabled", 0f);
		}
		catch (Exception ex)
		{
			LogError(ex.ToString());
			DisableOcean();
		}
	}

	private void OnEnable()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected Obj, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected Obj, but got Unknown
		if (WasError)
		{
			DisableOcean();
		}
		Camera.onPostRender = (CameraCallback)Delegate.Combine((Delegate?)(object)Camera.onPostRender, (Delegate?)new CameraCallback(OceanOnPostRender));
	}

	private void OnDisable()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected Obj, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected Obj, but got Unknown
		if (!WasError)
		{
			((Behaviour)this).enabled = true;
		}
		Camera.onPostRender = (CameraCallback)Delegate.Remove((Delegate?)(object)Camera.onPostRender, (Delegate?)new CameraCallback(OceanOnPostRender));
	}

	private void DisableOcean()
	{
		WasError = true;
		((Behaviour)this).enabled = false;
		((Component)this).gameObject.AddComponent<DisableGameObject>();
	}

	private void Update()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			WindDirVector = CalculateWindDirVector();
			Projection.TightFit = tightProjectionFit;
			UpdateOceanScheduler();
			OverlayManager.Update();
			specularRoughness = Mathf.Clamp01(specularRoughness);
			specularIntensity = Mathf.Max(0f, specularIntensity);
			minFresnel = Mathf.Clamp01(minFresnel);
			fresnelPower = Mathf.Max(0f, fresnelPower);
			foamIntensity = Math.Max(0f, foamIntensity);
			float num = Mathf.Lerp(2E-05f, 0.02f, specularRoughness);
			Shader.SetGlobalColor("Ceto_DefaultSkyColor", defaultSkyColor);
			Shader.SetGlobalColor("Ceto_DefaultOceanColor", defaultOceanColor);
			Shader.SetGlobalFloat("Ceto_SpecularRoughness", num);
			Shader.SetGlobalFloat("Ceto_FresnelPower", fresnelPower);
			Shader.SetGlobalFloat("Ceto_SpecularIntensity", specularIntensity);
			Shader.SetGlobalFloat("Ceto_MinFresnel", minFresnel);
			Shader.SetGlobalFloat("Ceto_OceanLevel", level);
			Shader.SetGlobalFloat("Ceto_MaxWaveHeight", 40f);
			Shader.SetGlobalColor("Ceto_FoamTint", foamTint * foamIntensity);
			Shader.SetGlobalVector("Ceto_SunDir", Vector4.op_Implicit(SunDir()));
			Shader.SetGlobalVector("Ceto_SunColor", Color.op_Implicit(SunColor()));
			Vector4 val = new Vector4
			{
				x = ((foamTexture0.scale.x != 0f) ? (1f / foamTexture0.scale.x) : 1f),
				y = ((foamTexture0.scale.y != 0f) ? (1f / foamTexture0.scale.y) : 1f),
				z = foamTexture0.scrollSpeed * OceanTime.Now,
				w = 0f
			};
			Vector4 val2 = new Vector4
			{
				x = ((foamTexture1.scale.x != 0f) ? (1f / foamTexture1.scale.x) : 1f),
				y = ((foamTexture1.scale.y != 0f) ? (1f / foamTexture1.scale.y) : 1f),
				z = foamTexture1.scrollSpeed * OceanTime.Now,
				w = 0f
			};
			Shader.SetGlobalTexture("Ceto_FoamTexture0", (Texture)(((Object)(object)foamTexture0.tex != (Object)null) ? ((object)foamTexture0.tex) : ((object)Texture2D.whiteTexture)));
			Shader.SetGlobalVector("Ceto_FoamTextureScale0", val);
			Shader.SetGlobalTexture("Ceto_FoamTexture1", (Texture)(((Object)(object)foamTexture1.tex != (Object)null) ? ((object)foamTexture1.tex) : ((object)Texture2D.whiteTexture)));
			Shader.SetGlobalVector("Ceto_FoamTextureScale1", val2);
			Dictionary<Camera, CameraData>.Enumerator enumerator = m_cameraData.GetEnumerator();
			while (enumerator.MoveNext())
			{
				CameraData value = enumerator.Current.Value;
				if (value.mask != null)
				{
					value.mask.ClearUpdatedViews();
				}
				if (value.depth != null)
				{
					value.depth.ClearUpdatedViews();
				}
				if (value.overlay != null)
				{
					value.overlay.ClearUpdatedViews();
				}
				if (value.reflection != null)
				{
					value.reflection.ClearUpdatedViews();
				}
				if (value.projection != null)
				{
					value.projection.ClearUpdatedViews();
					value.projection.checkedForFlipping = false;
				}
			}
		}
		catch (Exception ex)
		{
			LogError(ex.ToString());
			DisableOcean();
		}
	}

	private void OnDestroy()
	{
		try
		{
			Instance = null;
			if (OverlayManager != null)
			{
				OverlayManager.Release();
			}
			if (m_scheduler != null)
			{
				m_scheduler.ShutingDown = true;
				m_scheduler.CancelAllTasks();
			}
			foreach (Camera item in new List<Camera>(m_cameraData.Keys))
			{
				RemoveCameraData(item);
			}
		}
		catch (Exception ex)
		{
			LogError(ex.ToString());
			DisableOcean();
		}
	}

	private void OnDrawGizmos()
	{
	}

	public void Register(OceanComponent component)
	{
		if (component is ProjectedGrid)
		{
			Grid = component as ProjectedGrid;
			return;
		}
		if (component is WaveSpectrum)
		{
			Spectrum = component as WaveSpectrum;
			return;
		}
		if (component is PlanarReflection)
		{
			Reflection = component as PlanarReflection;
			return;
		}
		if (component is UnderWater)
		{
			UnderWater = component as UnderWater;
			return;
		}
		throw new InvalidCastException("Could not cast ocean component " + ((object)component).GetType());
	}

	public void Deregister(OceanComponent component)
	{
		if (component is ProjectedGrid)
		{
			Grid = null;
			return;
		}
		if (component is WaveSpectrum)
		{
			Spectrum = null;
			return;
		}
		if (component is PlanarReflection)
		{
			Reflection = null;
			return;
		}
		if (component is UnderWater)
		{
			UnderWater = null;
			return;
		}
		throw new InvalidCastException("Could not cast ocean component " + ((object)component).GetType());
	}

	public static void LogError(string msg)
	{
		Debug.Log((object)("<color=red>Ceto (" + VERSION + ") Error:</color> " + msg));
	}

	public static void LogWarning(string msg)
	{
		if (!((Object)(object)Instance != (Object)null) || !Instance.disableWarnings)
		{
			Debug.Log((object)("<color=yellow>Ceto (" + VERSION + ") Warning:</color> " + msg));
		}
	}

	public static void LogInfo(string msg)
	{
		if (!((Object)(object)Instance != (Object)null) || !Instance.disableInfo)
		{
			Debug.Log((object)("<color=cyan>Ceto (" + VERSION + ") Info:</color> " + msg));
		}
	}

	public void RunCoroutine(IEnumerator e)
	{
		((MonoBehaviour)this).StartCoroutine(e);
	}

	private void UpdateOceanScheduler()
	{
		try
		{
			if (m_scheduler != null)
			{
				m_scheduler.DisableMultithreading = DISABLE_ALL_MULTITHREADING;
				m_scheduler.CheckForException();
				m_scheduler.Update();
			}
		}
		catch (Exception ex)
		{
			LogError(ex.ToString());
			DisableOcean();
		}
	}

	public Vector3 CalculateWindDirVector()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		float num = windDir * (float)Math.PI / 180f;
		float num2 = 0f - Mathf.Cos(num);
		float num3 = Mathf.Sin(num);
		Vector3 val = new Vector3(num2, 0f, num3);
		return val.normalized;
	}

	public Vector3 SunDir()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_sun == (Object)null || (Object)(object)m_sun.GetComponent<Light>() == (Object)null)
		{
			return Vector3.up;
		}
		return m_sun.transform.forward * -1f;
	}

	public Color SunColor()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_sun == (Object)null || (Object)(object)m_sun.GetComponent<Light>() == (Object)null)
		{
			return Color.white;
		}
		return m_sun.GetComponent<Light>().color;
	}

	public CameraData FindCameraData(Camera cam)
	{
		if ((Object)(object)cam == (Object)null)
		{
			throw new InvalidOperationException("Can not find camera data for null camera");
		}
		CameraData value = null;
		if (!m_cameraData.TryGetValue(cam, out value))
		{
			value = new CameraData();
			m_cameraData.Add(cam, value);
		}
		if ((Object)(object)value.settings == (Object)null && !value.checkedForSettings)
		{
			value.settings = ((Component)cam).GetComponent<OceanCameraSettings>();
			value.checkedForSettings = true;
		}
		return value;
	}

	public void RemoveCameraData(Camera cam)
	{
		if (m_cameraData.ContainsKey(cam))
		{
			CameraData cameraData = m_cameraData[cam];
			if (cameraData.overlay != null)
			{
				OverlayManager.DestroyBuffers(cameraData.overlay);
			}
			if (cameraData.reflection != null)
			{
				cameraData.reflection.DestroyTargets();
				cameraData.reflection.DestroyCamera();
			}
			if (cameraData.depth != null)
			{
				cameraData.depth.DestroyTargets();
				cameraData.depth.DestroyCamera();
			}
			if (cameraData.mask != null)
			{
				cameraData.mask.DestroyTargets();
				cameraData.mask.DestroyCamera();
			}
			m_cameraData.Remove(cam);
		}
	}

	public float FindMaxDisplacement(bool inculdeOverlays)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		if (!((Behaviour)this).enabled)
		{
			return num;
		}
		if ((Object)(object)Spectrum != (Object)null && ((Behaviour)Spectrum).enabled)
		{
			num += Spectrum.MaxDisplacement.y;
		}
		if (inculdeOverlays && OverlayManager != null)
		{
			num += OverlayManager.MaxDisplacement;
		}
		return Mathf.Max(0f, num);
	}

	public void SetQueryWavesSampling(bool overlays, bool grid0, bool grid1, bool grid2, bool grid3)
	{
		m_query.sampleOverlay = overlays;
		m_query.sampleSpectrum[0] = grid0;
		m_query.sampleSpectrum[1] = grid1;
		m_query.sampleSpectrum[2] = grid2;
		m_query.sampleSpectrum[3] = grid3;
	}

	public Vector3 QueryNormal(float x, float z)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.5f;
		float num2 = QueryWaves(x - num, z);
		float num3 = QueryWaves(x, z - num);
		float num4 = QueryWaves(x + num, z);
		float num5 = QueryWaves(x, z + num);
		float num6 = num2 - num4;
		float num7 = num3 - num5;
		return new Vector3(num6, Mathf.Sqrt(1f - num6 * num6 - num7 * num7), num7);
	}

	public float QueryWaves(float x, float z)
	{
		m_query.result.Clear();
		m_query.posX = x;
		m_query.posZ = z;
		if (((Behaviour)this).enabled)
		{
			if ((Object)(object)Spectrum != (Object)null)
			{
				Spectrum.QueryWaves(m_query);
			}
			if (OverlayManager != null)
			{
				OverlayManager.QueryWaves(m_query);
			}
		}
		return m_query.result.height + level;
	}

	public float QueryWaves(float x, float z, out bool isClipped)
	{
		m_query.result.Clear();
		m_query.posX = x;
		m_query.posZ = z;
		if (((Behaviour)this).enabled)
		{
			if ((Object)(object)Spectrum != (Object)null)
			{
				Spectrum.QueryWaves(m_query);
			}
			if (OverlayManager != null)
			{
				OverlayManager.QueryWaves(m_query);
			}
		}
		isClipped = m_query.result.isClipped;
		return m_query.result.height + level;
	}

	public void QueryWaves(WaveQuery query)
	{
		query.result.Clear();
		if (((Behaviour)this).enabled)
		{
			if ((Object)(object)Spectrum != (Object)null)
			{
				Spectrum.QueryWaves(query);
			}
			if (OverlayManager != null)
			{
				OverlayManager.QueryWaves(query);
			}
		}
		query.result.height += level;
	}

	public void QueryWaves(IEnumerable<WaveQuery> querys)
	{
		foreach (WaveQuery query in querys)
		{
			query.result.Clear();
			if (((Behaviour)this).enabled)
			{
				if ((Object)(object)Spectrum != (Object)null)
				{
					Spectrum.QueryWaves(query);
				}
				if (OverlayManager != null)
				{
					OverlayManager.QueryWaves(query);
				}
			}
			query.result.height += level;
		}
	}

	public void QueryWavesWithAsyncTask(WaveQueryTask task)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (task != null && m_scheduler != null && (!task.IsScheduled || task.Done) && ((Behaviour)this).enabled)
		{
			task.Reset(Spectrum, PositionOffset, level);
			if (task.SupportsOverlays)
			{
				task.OverlaySampler = OverlayManager;
			}
			else
			{
				task.OverlaySampler = null;
			}
			task.IsScheduled = true;
			m_scheduler.Run(task);
		}
	}

	private void OceanOnPreRender(Camera cam)
	{
		if (((Behaviour)cam).enabled)
		{
			CameraData data = FindCameraData(cam);
			if ((Object)(object)Grid != (Object)null)
			{
				Grid.OceanOnPreRender(cam, data);
			}
			if ((Object)(object)Spectrum != (Object)null)
			{
				Spectrum.OceanOnPreRender(cam, data);
			}
			if ((Object)(object)Reflection != (Object)null)
			{
				Reflection.OceanOnPreRender(cam, data);
			}
			if ((Object)(object)UnderWater != (Object)null)
			{
				UnderWater.OceanOnPreRender(cam, data);
			}
		}
	}

	private void OceanOnPreCull(Camera cam)
	{
		if (((Behaviour)cam).enabled)
		{
			CameraData data = FindCameraData(cam);
			if ((Object)(object)Grid != (Object)null)
			{
				Grid.OceanOnPreCull(cam, data);
			}
			if ((Object)(object)Spectrum != (Object)null)
			{
				Spectrum.OceanOnPreCull(cam, data);
			}
			if ((Object)(object)Reflection != (Object)null)
			{
				Reflection.OceanOnPreCull(cam, data);
			}
			if ((Object)(object)UnderWater != (Object)null)
			{
				UnderWater.OceanOnPreCull(cam, data);
			}
		}
	}

	private void OceanOnPostRender(Camera cam)
	{
		if (((Behaviour)cam).enabled)
		{
			CameraData data = FindCameraData(cam);
			if ((Object)(object)Grid != (Object)null)
			{
				Grid.OceanOnPostRender(cam, data);
			}
			if ((Object)(object)Spectrum != (Object)null)
			{
				Spectrum.OceanOnPostRender(cam, data);
			}
			if ((Object)(object)Reflection != (Object)null)
			{
				Reflection.OceanOnPostRender(cam, data);
			}
			if ((Object)(object)UnderWater != (Object)null)
			{
				UnderWater.OceanOnPostRender(cam, data);
			}
		}
	}

	private bool GetDisableAllOverlays(OceanCameraSettings settings)
	{
		if (!((Object)(object)settings != (Object)null))
		{
			return false;
		}
		return settings.disableAllOverlays;
	}

	public void RenderWaveOverlays(GameObject go)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!((Behaviour)this).enabled)
			{
				return;
			}
			Camera current = Camera.current;
			if (!m_cameraData.ContainsKey(current))
			{
				m_cameraData.Add(current, new CameraData());
			}
			CameraData cameraData = m_cameraData[current];
			if (cameraData.overlay == null)
			{
				cameraData.overlay = new WaveOverlayData();
			}
			if (cameraData.projection == null)
			{
				cameraData.projection = new ProjectionData();
			}
			if (!cameraData.overlay.IsViewUpdated(current))
			{
				if (!cameraData.projection.IsViewUpdated(current))
				{
					Projection.UpdateProjection(current, cameraData);
					Shader.SetGlobalMatrix("Ceto_Interpolation", cameraData.projection.interpolation);
					Shader.SetGlobalMatrix("Ceto_ProjectorVP", cameraData.projection.projectorVP);
				}
				if (GetDisableAllOverlays(cameraData.settings))
				{
					OverlayManager.DestroyBuffers(cameraData.overlay);
					Shader.SetGlobalTexture("Ceto_Overlay_NormalMap", (Texture)(object)Texture2D.blackTexture);
					Shader.SetGlobalTexture("Ceto_Overlay_HeightMap", (Texture)(object)Texture2D.blackTexture);
					Shader.SetGlobalTexture("Ceto_Overlay_FoamMap", (Texture)(object)Texture2D.blackTexture);
					Shader.SetGlobalTexture("Ceto_Overlay_ClipMap", (Texture)(object)Texture2D.blackTexture);
				}
				else
				{
					OVERLAY_MAP_SIZE oVERLAY_MAP_SIZE = (((Object)(object)cameraData.settings != (Object)null) ? cameraData.settings.normalOverlaySize : normalOverlaySize);
					OVERLAY_MAP_SIZE oVERLAY_MAP_SIZE2 = (((Object)(object)cameraData.settings != (Object)null) ? cameraData.settings.heightOverlaySize : heightOverlaySize);
					OVERLAY_MAP_SIZE oVERLAY_MAP_SIZE3 = (((Object)(object)cameraData.settings != (Object)null) ? cameraData.settings.foamOverlaySize : foamOverlaySize);
					OVERLAY_MAP_SIZE oVERLAY_MAP_SIZE4 = (((Object)(object)cameraData.settings != (Object)null) ? cameraData.settings.clipOverlaySize : clipOverlaySize);
					OverlayManager.CreateOverlays(current, cameraData.overlay, oVERLAY_MAP_SIZE, oVERLAY_MAP_SIZE2, oVERLAY_MAP_SIZE3, oVERLAY_MAP_SIZE4);
					OverlayManager.HeightOverlayBlendMode = heightBlendMode;
					OverlayManager.FoamOverlayBlendMode = foamBlendMode;
					OverlayManager.RenderWaveOverlays(current, cameraData.overlay);
				}
				cameraData.overlay.SetViewAsUpdated(current);
			}
		}
		catch (Exception ex)
		{
			LogError(ex.ToString());
			DisableOcean();
		}
	}

	public void RenderReflection(GameObject go)
	{
		if (((Behaviour)this).enabled && !((Object)(object)Reflection == (Object)null))
		{
			Reflection.RenderReflection(go);
		}
	}

	public void RenderOceanMask(GameObject go)
	{
		if (((Behaviour)this).enabled && !((Object)(object)UnderWater == (Object)null))
		{
			UnderWater.RenderOceanMask(go);
		}
	}

	public void RenderOceanDepth(GameObject go)
	{
		if (((Behaviour)this).enabled && !((Object)(object)UnderWater == (Object)null))
		{
			UnderWater.RenderOceanDepth(go);
		}
	}

	public Ocean()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
	}
}
