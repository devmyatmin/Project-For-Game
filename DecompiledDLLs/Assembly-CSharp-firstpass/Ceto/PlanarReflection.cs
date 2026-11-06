using System;
using Ceto.Common.Unity.Utility;
using UnityEngine;

namespace Ceto;

[AddComponentMenu("Ceto/Components/PlanarReflection")]
[DisallowMultipleComponent]
[RequireComponent(typeof(Ocean))]
public class PlanarReflection : OceanComponent
{
	public const float MAX_REFLECTION_INTENSITY = 2f;

	public const float MAX_REFLECTION_DISORTION = 4f;

	public LayerMask reflectionMask = LayerMask.op_Implicit(1);

	public REFLECTION_RESOLUTION reflectionResolution = REFLECTION_RESOLUTION.HALF;

	public float clipPlaneOffset = 0.07f;

	public bool fogInReflection;

	public bool skyboxInReflection = true;

	public bool copyCullDistances;

	public ImageBlur.BLUR_MODE blurMode;

	[Range(0f, 4f)]
	public int blurIterations = 1;

	[Range(0.5f, 1f)]
	private float blurSpread = 0.6f;

	public Color reflectionTint = Color.white;

	[Range(0f, 2f)]
	public float reflectionIntensity = 0.6f;

	[Range(0f, 4f)]
	public float reflectionDistortion = 0.5f;

	public Func<GameObject, RenderTexture> RenderReflectionCustom;

	private GameObject m_dummy;

	private ImageBlur m_imageBlur;

	public Shader blurShader;

	private void Start()
	{
		Update();
		((Behaviour)this).enabled = false;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Shader.EnableKeyword("CETO_REFLECTION_ON");
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Shader.DisableKeyword("CETO_REFLECTION_ON");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		try
		{
			if ((Object)(object)m_dummy != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)m_dummy);
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
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		reflectionIntensity = Mathf.Max(0f, reflectionIntensity);
		Shader.SetGlobalVector("Ceto_ReflectionTint", Color.op_Implicit(reflectionTint * reflectionIntensity));
		Shader.SetGlobalFloat("Ceto_ReflectionDistortion", reflectionDistortion * 0.05f);
	}

	private LayerMask GetReflectionLayermask(OceanCameraSettings settings)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)settings != (Object)null))
		{
			return reflectionMask;
		}
		return settings.reflectionMask;
	}

	private bool GetDisableReflections(OceanCameraSettings settings)
	{
		if (!((Object)(object)settings != (Object)null))
		{
			return false;
		}
		return settings.disableReflections;
	}

	private REFLECTION_RESOLUTION GetReflectionResolution(OceanCameraSettings settings)
	{
		if (!((Object)(object)settings != (Object)null))
		{
			return reflectionResolution;
		}
		return settings.reflectionResolution;
	}

	public void RenderReflection(GameObject go)
	{
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Expected Obj, but got Unknown
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
			if (cameraData.reflection == null)
			{
				cameraData.reflection = new ReflectionData();
			}
			if (cameraData.reflection.IsViewUpdated(current))
			{
				return;
			}
			if (GetDisableReflections(cameraData.settings))
			{
				Shader.DisableKeyword("CETO_REFLECTION_ON");
				cameraData.reflection.SetViewAsUpdated(current);
				return;
			}
			Shader.EnableKeyword("CETO_REFLECTION_ON");
			RenderTexture val = null;
			RenderTexture val2 = null;
			if ((Object)(object)cameraData.reflection.cam != (Object)null)
			{
				DisableFog component = ((Component)cameraData.reflection.cam).GetComponent<DisableFog>();
				if ((Object)(object)component != (Object)null)
				{
					((Behaviour)component).enabled = !fogInReflection;
				}
			}
			if (RenderReflectionCustom != null)
			{
				if ((Object)(object)cameraData.reflection.cam != (Object)null)
				{
					cameraData.reflection.DestroyTargets();
				}
				CreateRenderTarget(cameraData.reflection, current.pixelWidth, current.pixelHeight, current.allowHDR, stero: false, cameraData.settings);
				if ((Object)(object)m_dummy == (Object)null)
				{
					m_dummy = new GameObject("Ceto Reflection Dummy Gameobject");
					((Object)m_dummy).hideFlags = (HideFlags)61;
				}
				m_dummy.transform.position = new Vector3(0f, m_ocean.level, 0f);
				Graphics.Blit((Texture)(object)RenderReflectionCustom(m_dummy), cameraData.reflection.target0);
				val = cameraData.reflection.target0;
				val2 = null;
			}
			else
			{
				CreateReflectionCameraFor(current, cameraData.reflection);
				CreateRenderTarget(cameraData.reflection, current.pixelWidth, current.pixelHeight, current.allowHDR, current.stereoEnabled, cameraData.settings);
				if (current.stereoEnabled)
				{
					RenderSteroReflection(cameraData.reflection, current, cameraData.settings);
					val = cameraData.reflection.target0;
					val2 = cameraData.reflection.target1;
				}
				else
				{
					Shader.DisableKeyword("CETO_STERO_CAMERA");
					RenderReflection(cameraData.reflection.cam, cameraData.reflection.target0, ((Component)current).transform.position, ((Component)current).transform.rotation, current.projectionMatrix, cameraData.settings);
					val = cameraData.reflection.target0;
					val2 = null;
				}
			}
			if ((Object)(object)val != (Object)null)
			{
				m_imageBlur.BlurIterations = blurIterations;
				m_imageBlur.BlurMode = blurMode;
				m_imageBlur.BlurSpread = blurSpread;
				m_imageBlur.Blur(val);
				Shader.SetGlobalTexture(Ocean.REFLECTION_TEXTURE_NAME0, (Texture)(object)val);
			}
			if ((Object)(object)val2 != (Object)null)
			{
				m_imageBlur.BlurIterations = blurIterations;
				m_imageBlur.BlurMode = blurMode;
				m_imageBlur.BlurSpread = blurSpread;
				m_imageBlur.Blur(val2);
				Shader.SetGlobalTexture(Ocean.REFLECTION_TEXTURE_NAME1, (Texture)(object)val2);
			}
			cameraData.reflection.SetViewAsUpdated(current);
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	private void RenderSteroReflection(ReflectionData data, Camera cam, OceanCameraSettings settings)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Shader.DisableKeyword("CETO_STERO_CAMERA");
		RenderReflection(data.cam, data.target0, ((Component)cam).transform.position, ((Component)cam).transform.rotation, cam.projectionMatrix, settings);
	}

	private void CreateReflectionCameraFor(Camera cam, ReflectionData data)
	{
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected Obj, but got Unknown
		if ((Object)(object)data.cam == (Object)null)
		{
			GameObject val = new GameObject("Ceto Reflection Camera: " + ((Object)cam).name);
			val.transform.SetParent(((Component)this).transform, false);
			((Object)val).hideFlags = (HideFlags)52;
			((Behaviour)val.AddComponent<DisableFog>()).enabled = !fogInReflection;
			data.cam = val.AddComponent<Camera>();
			data.cam.depthTextureMode = (DepthTextureMode)0;
			data.cam.renderingPath = (RenderingPath)1;
			((Behaviour)data.cam).enabled = false;
			data.cam.allowHDR = cam.allowHDR;
			data.cam.targetTexture = null;
			data.cam.useOcclusionCulling = false;
			if (copyCullDistances)
			{
				data.cam.layerCullDistances = cam.layerCullDistances;
				data.cam.layerCullSpherical = true;
			}
		}
		data.cam.fieldOfView = cam.fieldOfView;
		data.cam.nearClipPlane = cam.nearClipPlane;
		data.cam.farClipPlane = cam.farClipPlane;
		data.cam.orthographic = cam.orthographic;
		data.cam.aspect = cam.aspect;
		data.cam.orthographicSize = cam.orthographicSize;
		data.cam.rect = new Rect(0f, 0f, 1f, 1f);
		data.cam.backgroundColor = Color.black;
		data.cam.clearFlags = (CameraClearFlags)(skyboxInReflection ? 1 : 2);
	}

	private void CreateRenderTarget(ReflectionData data, int width, int height, bool isHdr, bool stero, OceanCameraSettings settings)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Invalid comparison between Unknown and I4
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected Obj, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected Obj, but got Unknown
		int num = ResolutionToNumber(GetReflectionResolution(settings));
		width /= num;
		height /= num;
		if (!((Object)(object)data.target0 != (Object)null) || ((Texture)data.target0).width != width || ((Texture)data.target0).height != height)
		{
			RenderTextureFormat val = (((!isHdr && (int)QualitySettings.activeColorSpace != 1) || !SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat)2)) ? ((RenderTextureFormat)0) : ((RenderTextureFormat)2));
			data.DestroyTargets();
			data.target0 = new RenderTexture(width, height, 16, val, (RenderTextureReadWrite)0);
			((Texture)data.target0).filterMode = (FilterMode)1;
			((Texture)data.target0).wrapMode = (TextureWrapMode)1;
			data.target0.useMipMap = false;
			((Texture)data.target0).anisoLevel = 0;
			((Object)data.target0).hideFlags = (HideFlags)61;
			((Object)data.target0).name = "Ceto Reflection Render Target0";
			if (stero)
			{
				data.target1 = new RenderTexture(width, height, 16, val, (RenderTextureReadWrite)0);
				((Texture)data.target1).filterMode = (FilterMode)1;
				((Texture)data.target1).wrapMode = (TextureWrapMode)1;
				data.target1.useMipMap = false;
				((Texture)data.target1).anisoLevel = 0;
				((Object)data.target1).hideFlags = (HideFlags)61;
				((Object)data.target1).name = "Ceto Reflection Render Target1";
			}
		}
	}

	private int ResolutionToNumber(REFLECTION_RESOLUTION resolution)
	{
		return resolution switch
		{
			REFLECTION_RESOLUTION.FULL => 1, 
			REFLECTION_RESOLUTION.HALF => 2, 
			REFLECTION_RESOLUTION.QUARTER => 4, 
			_ => 2, 
		};
	}

	private void RenderReflection(Camera reflectionCam, RenderTexture target, Vector3 position, Quaternion rotation, Matrix4x4 projection, OceanCameraSettings settings)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		reflectionCam.ResetWorldToCameraMatrix();
		((Component)reflectionCam).transform.position = position;
		((Component)reflectionCam).transform.rotation = rotation;
		reflectionCam.projectionMatrix = projection;
		reflectionCam.targetTexture = target;
		float level = m_ocean.level;
		Vector3 pos = new Vector3(0f, level, 0f);
		Vector3 up = Vector3.up;
		Vector4 plane = Plane(pos, up);
		reflectionCam.worldToCameraMatrix *= CalculateReflectionMatrix(plane);
		Vector4 val = CameraSpacePlane(reflectionCam, pos, up);
		reflectionCam.projectionMatrix = reflectionCam.CalculateObliqueMatrix(val);
		reflectionCam.cullingMask = LayerMask.op_Implicit(GetReflectionLayermask(settings));
		reflectionCam.cullingMask = OceanUtility.HideLayer(reflectionCam.cullingMask, Ocean.OCEAN_LAYER);
		bool invertCulling = GL.invertCulling;
		GL.invertCulling = !invertCulling;
		NotifyOnEvent.Disable = true;
		reflectionCam.Render();
		NotifyOnEvent.Disable = false;
		GL.invertCulling = invertCulling;
		reflectionCam.targetTexture = null;
	}

	private Vector4 Plane(Vector3 pos, Vector3 normal)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector4(normal.x, normal.y, normal.z, 0f - Vector3.Dot(pos, normal));
	}

	private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
		Vector3 pos2 = worldToCameraMatrix.MultiplyPoint(pos);
		Vector3 val = worldToCameraMatrix.MultiplyVector(normal);
		Vector3 normalized = val.normalized;
		return Plane(pos2, normalized);
	}

	private Matrix4x4 CalculateReflectionMatrix(Vector4 plane)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 identity = Matrix4x4.identity;
		identity.m00 = 1f - 2f * plane[0] * plane[0];
		identity.m01 = -2f * plane[0] * plane[1];
		identity.m02 = -2f * plane[0] * plane[2];
		identity.m03 = -2f * plane[3] * plane[0];
		identity.m10 = -2f * plane[1] * plane[0];
		identity.m11 = 1f - 2f * plane[1] * plane[1];
		identity.m12 = -2f * plane[1] * plane[2];
		identity.m13 = -2f * plane[3] * plane[1];
		identity.m20 = -2f * plane[2] * plane[0];
		identity.m21 = -2f * plane[2] * plane[1];
		identity.m22 = 1f - 2f * plane[2] * plane[2];
		identity.m23 = -2f * plane[3] * plane[2];
		identity.m30 = 0f;
		identity.m31 = 0f;
		identity.m32 = 0f;
		identity.m33 = 1f;
		return identity;
	}

	public PlanarReflection()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
	}
}
