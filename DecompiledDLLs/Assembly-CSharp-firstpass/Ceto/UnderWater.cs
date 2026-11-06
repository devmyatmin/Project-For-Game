using System;
using Ceto.Common.Unity.Utility;
using UnityEngine;
using UnityEngine.Rendering;

namespace Ceto;

[AddComponentMenu("Ceto/Components/UnderWater")]
[DisallowMultipleComponent]
[RequireComponent(typeof(Ocean))]
public class UnderWater : OceanComponent
{
	public const float MAX_REFRACTION_INTENSITY = 2f;

	public const float MAX_REFRACTION_DISORTION = 4f;

	private readonly float MAX_DEPTH_DIST = 500f;

	public UNDERWATER_MODE underwaterMode;

	public DEPTH_MODE depthMode;

	public LayerMask oceanDepthsMask = LayerMask.op_Implicit(1);

	public float bottomDepth = 500f;

	[Range(0f, 1f)]
	private float depthBlend = 0.2f;

	[Range(0f, 1f)]
	public float edgeFade = 0.2f;

	[Range(0f, 2f)]
	public float aboveRefractionIntensity = 0.5f;

	[Range(0f, 2f)]
	public float belowRefractionIntensity = 0.5f;

	[Range(0f, 4f)]
	public float refractionDistortion = 0.5f;

	[Range(0f, 1f)]
	public float absorptionR = 0.45f;

	[Range(0f, 1f)]
	public float absorptionG = 0.029f;

	[Range(0f, 1f)]
	public float absorptionB = 0.018f;

	public AbsorptionModifier aboveAbsorptionModifier = new AbsorptionModifier(2f, 1f, Color.white);

	public AbsorptionModifier belowAbsorptionModifier = new AbsorptionModifier(0.1f, 1f, Color.white);

	public AbsorptionModifier subSurfaceScatterModifier = new AbsorptionModifier(10f, 1.5f, Color32.op_Implicit(new Color32((byte)220, (byte)250, (byte)180, byte.MaxValue)));

	public InscatterModifier aboveInscatterModifier = new InscatterModifier(50f, 1f, Color32.op_Implicit(new Color32((byte)2, (byte)25, (byte)43, byte.MaxValue)), INSCATTER_MODE.EXP);

	public InscatterModifier belowInscatterModifier = new InscatterModifier(60f, 1f, Color32.op_Implicit(new Color32((byte)7, (byte)51, (byte)77, byte.MaxValue)), INSCATTER_MODE.EXP);

	private GameObject m_bottomMask;

	public int NumMeshSegmentsX = 32;

	public int NumMeshSegmentsY = 512;

	public Shader oceanBottomSdr;

	public Shader oceanDepthSdr;

	public Shader copyDepthSdr;

	public Shader oceanMaskSdr;

	public Shader oceanMaskFlippedSdr;

	public Shader normalFadeSdr;

	public bool DisableCopyDepthCmd { get; set; }

	public bool DisableNormalFadeCmd { get; set; }

	private void Start()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected Obj, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected Obj, but got Unknown
		try
		{
			Mesh val = CreateBottomMesh(NumMeshSegmentsX, NumMeshSegmentsY);
			m_bottomMask = new GameObject("Ceto Bottom Mask Gameobject");
			MeshFilter val2 = m_bottomMask.AddComponent<MeshFilter>();
			MeshRenderer val3 = m_bottomMask.AddComponent<MeshRenderer>();
			NotifyOnWillRender notifyOnWillRender = m_bottomMask.AddComponent<NotifyOnWillRender>();
			val2.sharedMesh = val;
			((Renderer)val3).receiveShadows = false;
			((Renderer)val3).shadowCastingMode = (ShadowCastingMode)0;
			((Renderer)val3).reflectionProbeUsage = (ReflectionProbeUsage)0;
			((Renderer)val3).material = new Material(oceanBottomSdr);
			notifyOnWillRender.AddAction(m_ocean.RenderWaveOverlays);
			notifyOnWillRender.AddAction(m_ocean.RenderOceanMask);
			notifyOnWillRender.AddAction(m_ocean.RenderOceanDepth);
			m_bottomMask.layer = LayerMask.NameToLayer(Ocean.OCEAN_LAYER);
			((Object)m_bottomMask).hideFlags = (HideFlags)61;
			UpdateBottomBounds();
			Object.Destroy((Object)(object)val);
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		try
		{
			Shader.EnableKeyword("CETO_UNDERWATER_ON");
			SetBottomActive(m_bottomMask, active: true);
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		try
		{
			Shader.DisableKeyword("CETO_UNDERWATER_ON");
			SetBottomActive(m_bottomMask, active: false);
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		try
		{
			if ((Object)(object)m_bottomMask != (Object)null)
			{
				Mesh mesh = m_bottomMask.GetComponent<MeshFilter>().mesh;
				Object.Destroy((Object)(object)m_bottomMask);
				Object.Destroy((Object)(object)mesh);
			}
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	private void Update()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Vector4 val = new Vector4(absorptionR, absorptionG, absorptionB, 1f);
			Vector4 val2 = val;
			Vector4 val3 = val;
			val.w = Mathf.Max(0f, aboveAbsorptionModifier.scale);
			val2.w = Mathf.Max(0f, subSurfaceScatterModifier.scale);
			val3.w = Mathf.Max(0f, belowAbsorptionModifier.scale);
			Color val4 = aboveAbsorptionModifier.tint * Mathf.Max(0f, aboveAbsorptionModifier.intensity);
			Color val5 = subSurfaceScatterModifier.tint * Mathf.Max(0f, subSurfaceScatterModifier.intensity);
			Color val6 = belowAbsorptionModifier.tint * Mathf.Max(0f, belowAbsorptionModifier.intensity);
			Shader.SetGlobalVector("Ceto_AbsCof", val);
			Shader.SetGlobalColor("Ceto_AbsTint", val4);
			Shader.SetGlobalVector("Ceto_SSSCof", val2);
			Shader.SetGlobalColor("Ceto_SSSTint", val5);
			Shader.SetGlobalVector("Ceto_BelowCof", val3);
			Shader.SetGlobalColor("Ceto_BelowTint", val6);
			Color color = aboveInscatterModifier.color;
			color.a = Mathf.Clamp01(aboveInscatterModifier.intensity);
			Shader.SetGlobalFloat("Ceto_AboveInscatterScale", Mathf.Max(0.1f, aboveInscatterModifier.scale));
			Shader.SetGlobalVector("Ceto_AboveInscatterMode", Vector4.op_Implicit(InscatterModeToMask(aboveInscatterModifier.mode)));
			Shader.SetGlobalColor("Ceto_AboveInscatterColor", color);
			Color color2 = belowInscatterModifier.color;
			color2.a = Mathf.Clamp01(belowInscatterModifier.intensity);
			Shader.SetGlobalFloat("Ceto_BelowInscatterScale", Mathf.Max(0.1f, belowInscatterModifier.scale));
			Shader.SetGlobalVector("Ceto_BelowInscatterMode", Vector4.op_Implicit(InscatterModeToMask(belowInscatterModifier.mode)));
			Shader.SetGlobalColor("Ceto_BelowInscatterColor", color2);
			Shader.SetGlobalFloat("Ceto_AboveRefractionIntensity", Mathf.Max(0f, aboveRefractionIntensity));
			Shader.SetGlobalFloat("Ceto_BelowRefractionIntensity", Mathf.Max(0f, belowRefractionIntensity));
			Shader.SetGlobalFloat("Ceto_RefractionDistortion", refractionDistortion * 0.05f);
			Shader.SetGlobalFloat("Ceto_MaxDepthDist", Mathf.Max(0f, MAX_DEPTH_DIST));
			Shader.SetGlobalFloat("Ceto_DepthBlend", Mathf.Clamp01(depthBlend));
			Shader.SetGlobalFloat("Ceto_EdgeFade", Mathf.Lerp(20f, 2f, Mathf.Clamp01(edgeFade)));
			float num = Ocean.Instance.FindMaxDisplacement(inculdeOverlays: false);
			float num2 = Ocean.Instance.level - num;
			Shader.SetGlobalFloat("Ceto_MaxWaveDisplacement", num);
			Shader.SetGlobalFloat("Ceto_MinWaveHeight", num2);
			if (depthMode == DEPTH_MODE.USE_OCEAN_DEPTH_PASS)
			{
				Shader.EnableKeyword("CETO_USE_OCEAN_DEPTHS_BUFFER");
				if (underwaterMode == UNDERWATER_MODE.ABOVE_ONLY)
				{
					SetBottomActive(m_bottomMask, active: false);
					return;
				}
				SetBottomActive(m_bottomMask, active: true);
				UpdateBottomBounds();
			}
			else
			{
				Shader.DisableKeyword("CETO_USE_OCEAN_DEPTHS_BUFFER");
				if (underwaterMode == UNDERWATER_MODE.ABOVE_ONLY)
				{
					SetBottomActive(m_bottomMask, active: false);
					return;
				}
				SetBottomActive(m_bottomMask, active: true);
				UpdateBottomBounds();
			}
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	private void SetBottomActive(GameObject bottom, bool active)
	{
		if ((Object)(object)bottom != (Object)null)
		{
			bottom.SetActive(active);
		}
	}

	private Vector3 InscatterModeToMask(INSCATTER_MODE mode)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		return mode switch
		{
			INSCATTER_MODE.LINEAR => new Vector3(1f, 0f, 0f), 
			INSCATTER_MODE.EXP => new Vector3(0f, 1f, 0f), 
			INSCATTER_MODE.EXP2 => new Vector3(0f, 0f, 1f), 
			_ => new Vector3(0f, 0f, 1f), 
		};
	}

	private void FitBottomToCamera()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled && !((Object)(object)m_bottomMask == (Object)null))
		{
			Camera current = Camera.current;
			Vector3 position = ((Component)current).transform.position;
			float num = current.farClipPlane * 0.85f;
			m_bottomMask.transform.localScale = new Vector3(num, bottomDepth, num);
			float num2 = 0f;
			m_bottomMask.transform.localPosition = new Vector3(position.x, 0f - bottomDepth + m_ocean.level - num2, position.z);
		}
	}

	private void UpdateBottomBounds()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		float num = 100000000f;
		float level = m_ocean.level;
		if ((Object)(object)m_bottomMask != (Object)null && m_bottomMask.activeSelf)
		{
			Bounds bounds = new Bounds(new Vector3(0f, level, 0f), new Vector3(num, bottomDepth, num));
			m_bottomMask.GetComponent<MeshFilter>().mesh.bounds = bounds;
		}
	}

	private LayerMask GetOceanDepthsLayermask(OceanCameraSettings settings)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)settings != (Object)null))
		{
			return oceanDepthsMask;
		}
		return settings.oceanDepthsMask;
	}

	private bool GetDisableUnderwater(OceanCameraSettings settings)
	{
		if (!((Object)(object)settings != (Object)null))
		{
			return false;
		}
		return settings.disableUnderwater;
	}

	public void RenderOceanMask(GameObject go)
	{
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!((Behaviour)this).enabled || (Object)(object)oceanMaskSdr == (Object)null)
			{
				return;
			}
			Camera current = Camera.current;
			if ((Object)(object)current == (Object)null)
			{
				return;
			}
			CameraData cameraData = m_ocean.FindCameraData(current);
			if (cameraData.mask == null)
			{
				cameraData.mask = new MaskData();
			}
			if (cameraData.mask.IsViewUpdated(current))
			{
				return;
			}
			if (((Object)current).name == "SceneCamera" || SystemInfo.graphicsShaderLevel < 30 || GetDisableUnderwater(cameraData.settings))
			{
				Shader.SetGlobalTexture(Ocean.OCEAN_MASK_TEXTURE_NAME0, (Texture)(object)Texture2D.blackTexture);
				Shader.SetGlobalTexture(Ocean.OCEAN_MASK_TEXTURE_NAME1, (Texture)(object)Texture2D.blackTexture);
				cameraData.mask.SetViewAsUpdated(current);
				return;
			}
			CreateMaskCameraFor(current, cameraData.mask);
			FitBottomToCamera();
			Shader sdr = (m_ocean.Projection.IsFlipped ? oceanMaskFlippedSdr : oceanMaskSdr);
			if (current.stereoEnabled)
			{
				RenderSteroOceanMask(cameraData.mask, current, sdr);
			}
			else
			{
				Shader.DisableKeyword("CETO_STERO_CAMERA");
				RenderOceanMask(cameraData.mask, cameraData.mask.target0, ((Component)current).transform.position, ((Component)current).transform.rotation, current.projectionMatrix, sdr);
				Shader.SetGlobalTexture(Ocean.OCEAN_MASK_TEXTURE_NAME0, (Texture)(object)cameraData.mask.target0);
			}
			cameraData.mask.SetViewAsUpdated(current);
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	private void RenderSteroOceanMask(MaskData data, Camera cam, Shader sdr)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Shader.DisableKeyword("CETO_STERO_CAMERA");
		RenderOceanMask(data, data.target0, ((Component)cam).transform.position, ((Component)cam).transform.rotation, cam.projectionMatrix, sdr);
		Shader.SetGlobalTexture(Ocean.OCEAN_MASK_TEXTURE_NAME0, (Texture)(object)data.target0);
	}

	private void RenderOceanMask(MaskData data, RenderTexture target, Vector3 position, Quaternion rotation, Matrix4x4 projection, Shader sdr)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		NotifyOnEvent.Disable = true;
		data.cam.ResetWorldToCameraMatrix();
		((Component)data.cam).transform.position = position;
		((Component)data.cam).transform.rotation = rotation;
		data.cam.projectionMatrix = projection;
		data.cam.targetTexture = target;
		data.cam.RenderWithShader(sdr, "OceanMask");
		data.cam.targetTexture = null;
		NotifyOnEvent.Disable = false;
	}

	public void RenderOceanDepth(GameObject go)
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!((Behaviour)this).enabled)
			{
				return;
			}
			Camera current = Camera.current;
			if ((Object)(object)current == (Object)null)
			{
				return;
			}
			CameraData cameraData = m_ocean.FindCameraData(current);
			if (cameraData.depth == null)
			{
				cameraData.depth = new DepthData();
			}
			if (cameraData.depth.IsViewUpdated(current))
			{
				return;
			}
			if (GetDisableUnderwater(cameraData.settings))
			{
				Shader.DisableKeyword("CETO_UNDERWATER_ON");
				cameraData.depth.SetViewAsUpdated(current);
				return;
			}
			Shader.EnableKeyword("CETO_UNDERWATER_ON");
			if (SystemInfo.graphicsShaderLevel < 30)
			{
				Shader.SetGlobalTexture(Ocean.OCEAN_DEPTH_TEXTURE_NAME0, (Texture)(object)Texture2D.whiteTexture);
				Shader.SetGlobalTexture(Ocean.OCEAN_DEPTH_TEXTURE_NAME1, (Texture)(object)Texture2D.whiteTexture);
				Shader.SetGlobalTexture(Ocean.DEPTH_GRAB_TEXTURE_NAME, (Texture)(object)Texture2D.whiteTexture);
				Shader.SetGlobalTexture(Ocean.NORMAL_FADE_TEXTURE_NAME, (Texture)(object)Texture2D.blackTexture);
				BindIVPMatrix(current.projectionMatrix, current.worldToCameraMatrix);
				cameraData.depth.SetViewAsUpdated(current);
			}
			else if (depthMode == DEPTH_MODE.USE_DEPTH_BUFFER)
			{
				Shader.SetGlobalTexture(Ocean.OCEAN_DEPTH_TEXTURE_NAME0, (Texture)(object)Texture2D.whiteTexture);
				Shader.SetGlobalTexture(Ocean.OCEAN_DEPTH_TEXTURE_NAME1, (Texture)(object)Texture2D.whiteTexture);
				current.depthTextureMode = (DepthTextureMode)(current.depthTextureMode | 1);
				current.depthTextureMode = (DepthTextureMode)(current.depthTextureMode | 2);
				CreateRefractionCommand(current, cameraData.depth);
				BindIVPMatrix(current.projectionMatrix, current.worldToCameraMatrix);
				cameraData.depth.SetViewAsUpdated(current);
			}
			else if (depthMode == DEPTH_MODE.USE_OCEAN_DEPTH_PASS)
			{
				Shader.SetGlobalTexture(Ocean.DEPTH_GRAB_TEXTURE_NAME, (Texture)(object)Texture2D.whiteTexture);
				Shader.SetGlobalTexture(Ocean.NORMAL_FADE_TEXTURE_NAME, (Texture)(object)Texture2D.blackTexture);
				CreateDepthCameraFor(current, cameraData.depth);
				CreateRefractionCommand(current, cameraData.depth);
				cameraData.depth.cam.cullingMask = LayerMask.op_Implicit(GetOceanDepthsLayermask(cameraData.settings));
				cameraData.depth.cam.cullingMask = OceanUtility.HideLayer(cameraData.depth.cam.cullingMask, Ocean.OCEAN_LAYER);
				if (current.stereoEnabled)
				{
					RenderSteroOceanDepth(cameraData.depth, current);
				}
				else
				{
					Shader.DisableKeyword("CETO_STERO_CAMERA");
					RenderOceanDepth(cameraData.depth, cameraData.depth.target0, ((Component)current).transform.position, ((Component)current).transform.rotation, current.projectionMatrix);
					Shader.SetGlobalTexture(Ocean.OCEAN_DEPTH_TEXTURE_NAME0, (Texture)(object)cameraData.depth.target0);
				}
				cameraData.depth.SetViewAsUpdated(current);
			}
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	private void BindIVPMatrix(Matrix4x4 projection, Matrix4x4 view)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 val = projection * view;
		Shader.SetGlobalMatrix("Ceto_Camera_IVP0", val.inverse);
	}

	private void RenderSteroOceanDepth(DepthData data, Camera cam)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Shader.DisableKeyword("CETO_STERO_CAMERA");
		RenderOceanDepth(data, data.target0, ((Component)cam).transform.position, ((Component)cam).transform.rotation, cam.projectionMatrix);
		Shader.SetGlobalTexture(Ocean.OCEAN_DEPTH_TEXTURE_NAME0, (Texture)(object)data.target0);
	}

	private void RenderOceanDepth(DepthData data, RenderTexture target, Vector3 position, Quaternion rotation, Matrix4x4 projection)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		NotifyOnEvent.Disable = true;
		data.cam.ResetWorldToCameraMatrix();
		((Component)data.cam).transform.position = position;
		((Component)data.cam).transform.rotation = rotation;
		data.cam.projectionMatrix = projection;
		data.cam.targetTexture = target;
		data.cam.RenderWithShader(oceanDepthSdr, "RenderType");
		data.cam.targetTexture = null;
		NotifyOnEvent.Disable = false;
	}

	private void CreateMaskCameraFor(Camera cam, MaskData data)
	{
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected Obj, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Expected Obj, but got Unknown
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Expected Obj, but got Unknown
		if ((Object)(object)data.cam == (Object)null)
		{
			GameObject val = new GameObject("Ceto Mask Camera: " + ((Object)cam).name);
			((Object)val).hideFlags = (HideFlags)61;
			val.AddComponent<IgnoreOceanEvents>();
			val.AddComponent<DisableFog>();
			val.AddComponent<DisableShadows>();
			data.cam = val.AddComponent<Camera>();
			data.cam.clearFlags = (CameraClearFlags)2;
			data.cam.backgroundColor = new Color(0f, 1f, 0f, 0f);
			data.cam.cullingMask = 1 << LayerMask.NameToLayer(Ocean.OCEAN_LAYER);
			((Behaviour)data.cam).enabled = false;
			data.cam.renderingPath = (RenderingPath)1;
			data.cam.targetTexture = null;
			data.cam.useOcclusionCulling = false;
			data.cam.RemoveAllCommandBuffers();
			data.cam.targetTexture = null;
		}
		data.cam.fieldOfView = cam.fieldOfView;
		data.cam.nearClipPlane = cam.nearClipPlane;
		data.cam.farClipPlane = cam.farClipPlane;
		data.cam.orthographic = cam.orthographic;
		data.cam.aspect = cam.aspect;
		data.cam.orthographicSize = cam.orthographicSize;
		data.cam.rect = new Rect(0f, 0f, 1f, 1f);
		if ((Object)(object)data.target0 == (Object)null || ((Texture)data.target0).width != cam.pixelWidth || ((Texture)data.target0).height != cam.pixelHeight)
		{
			data.DestroyTargets();
			int pixelWidth = cam.pixelWidth;
			int pixelHeight = cam.pixelHeight;
			int num = 32;
			RenderTextureFormat val2 = (RenderTextureFormat)13;
			if (!SystemInfo.SupportsRenderTextureFormat(val2))
			{
				val2 = (RenderTextureFormat)2;
			}
			data.target0 = new RenderTexture(pixelWidth, pixelHeight, num, val2, (RenderTextureReadWrite)1);
			((Texture)data.target0).filterMode = (FilterMode)0;
			((Object)data.target0).hideFlags = (HideFlags)52;
			((Object)data.target0).name = "Ceto Mask Render Target0: " + ((Object)cam).name;
			if (cam.stereoEnabled)
			{
				data.target1 = new RenderTexture(pixelWidth, pixelHeight, num, val2, (RenderTextureReadWrite)1);
				((Texture)data.target1).filterMode = (FilterMode)0;
				((Object)data.target1).hideFlags = (HideFlags)52;
				((Object)data.target1).name = "Ceto Mask Render Target1: " + ((Object)cam).name;
			}
		}
	}

	private void CreateDepthCameraFor(Camera cam, DepthData data)
	{
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected Obj, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Expected Obj, but got Unknown
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Expected Obj, but got Unknown
		if ((Object)(object)data.cam == (Object)null)
		{
			GameObject val = new GameObject("Ceto Depth Camera: " + ((Object)cam).name);
			((Object)val).hideFlags = (HideFlags)61;
			val.AddComponent<IgnoreOceanEvents>();
			val.AddComponent<DisableFog>();
			val.AddComponent<DisableShadows>();
			data.cam = val.AddComponent<Camera>();
			data.cam.clearFlags = (CameraClearFlags)2;
			data.cam.backgroundColor = Color.white;
			((Behaviour)data.cam).enabled = false;
			data.cam.renderingPath = (RenderingPath)1;
			data.cam.targetTexture = null;
			data.cam.useOcclusionCulling = false;
			data.cam.RemoveAllCommandBuffers();
			data.cam.targetTexture = null;
		}
		data.cam.fieldOfView = cam.fieldOfView;
		data.cam.nearClipPlane = cam.nearClipPlane;
		data.cam.farClipPlane = cam.farClipPlane;
		data.cam.orthographic = cam.orthographic;
		data.cam.aspect = cam.aspect;
		data.cam.orthographicSize = cam.orthographicSize;
		data.cam.rect = new Rect(0f, 0f, 1f, 1f);
		data.cam.layerCullDistances = cam.layerCullDistances;
		data.cam.layerCullSpherical = cam.layerCullSpherical;
		if ((Object)(object)data.target0 == (Object)null || ((Texture)data.target0).width != cam.pixelWidth || ((Texture)data.target0).height != cam.pixelHeight)
		{
			data.DestroyTargets();
			int pixelWidth = cam.pixelWidth;
			int pixelHeight = cam.pixelHeight;
			int num = 24;
			RenderTextureFormat val2 = (RenderTextureFormat)12;
			if (!SystemInfo.SupportsRenderTextureFormat(val2))
			{
				val2 = (RenderTextureFormat)13;
			}
			if (!SystemInfo.SupportsRenderTextureFormat(val2))
			{
				val2 = (RenderTextureFormat)2;
			}
			data.target0 = new RenderTexture(pixelWidth, pixelHeight, num, val2, (RenderTextureReadWrite)1);
			((Texture)data.target0).filterMode = (FilterMode)0;
			((Object)data.target0).hideFlags = (HideFlags)52;
			((Object)data.target0).name = "Ceto Ocean Depths Render Target0: " + ((Object)cam).name;
			if (cam.stereoEnabled)
			{
				data.target1 = new RenderTexture(pixelWidth, pixelHeight, num, val2, (RenderTextureReadWrite)1);
				((Texture)data.target1).filterMode = (FilterMode)0;
				((Object)data.target1).hideFlags = (HideFlags)52;
				((Object)data.target1).name = "Ceto Ocean Depths Render Target1: " + ((Object)cam).name;
			}
		}
	}

	private void CreateRefractionCommand(Camera cam, DepthData data)
	{
		if (depthMode == DEPTH_MODE.USE_DEPTH_BUFFER)
		{
			if (data.refractionCommand == null)
			{
				data.refractionCommand = new RefractionCommand(cam, copyDepthSdr, normalFadeSdr);
			}
			if (!data.refractionCommand.DisableCopyDepthCmd && DisableCopyDepthCmd)
			{
				Shader.SetGlobalTexture(Ocean.DEPTH_GRAB_TEXTURE_NAME, (Texture)(object)Texture2D.whiteTexture);
			}
			if (!data.refractionCommand.DisableNormalFadeCmd && DisableNormalFadeCmd)
			{
				Shader.SetGlobalTexture(Ocean.NORMAL_FADE_TEXTURE_NAME, (Texture)(object)Texture2D.blackTexture);
			}
			data.refractionCommand.DisableCopyDepthCmd = DisableCopyDepthCmd;
			data.refractionCommand.DisableNormalFadeCmd = DisableNormalFadeCmd;
			data.refractionCommand.UpdateCommands();
		}
		else if (data.refractionCommand != null)
		{
			data.refractionCommand.ClearCommands();
			data.refractionCommand = null;
		}
	}

	private Mesh CreateBottomMesh(int segementsX, int segementsY)
	{
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Expected Obj, but got Unknown
		Vector3[] array = new Vector3[segementsX * segementsY];
		Vector2[] uv = new Vector2[segementsX * segementsY];
		float num = (float)Math.PI * 2f;
		for (int i = 0; i < segementsX; i++)
		{
			for (int j = 0; j < segementsY; j++)
			{
				float num2 = (float)i / (float)(segementsX - 1);
				array[i + j * segementsX].x = num2 * Mathf.Cos(num * (float)j / (float)(segementsY - 1));
				array[i + j * segementsX].y = 0f;
				array[i + j * segementsX].z = num2 * Mathf.Sin(num * (float)j / (float)(segementsY - 1));
				if (i == segementsX - 1)
				{
					array[i + j * segementsX].y = 1f;
				}
			}
		}
		int[] array2 = new int[segementsX * segementsY * 6];
		int num3 = 0;
		for (int k = 0; k < segementsX - 1; k++)
		{
			for (int l = 0; l < segementsY - 1; l++)
			{
				array2[num3++] = k + l * segementsX;
				array2[num3++] = k + (l + 1) * segementsX;
				array2[num3++] = k + 1 + l * segementsX;
				array2[num3++] = k + (l + 1) * segementsX;
				array2[num3++] = k + 1 + (l + 1) * segementsX;
				array2[num3++] = k + 1 + l * segementsX;
			}
		}
		Mesh val = new Mesh
		{
			vertices = array,
			uv = uv,
			triangles = array2,
			name = "Ceto Bottom Mesh",
			hideFlags = (HideFlags)61
		};
		val.RecalculateNormals();
		return val;
	}

	public UnderWater()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
	}
}
