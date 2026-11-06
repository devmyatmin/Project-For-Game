using System;
using Beam;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace kode80.Clouds;

public class kode80Clouds : MonoBehaviour
{
	public enum SubPixelSize
	{
		Sub1x1,
		Sub2x2,
		Sub4x4,
		Sub8x8
	}

	public enum RenderSize
	{
		CameraDimensions,
		FixedDimensions
	}

	public delegate void OnValidateDelegate();

	public OnValidateDelegate onValidateDelegate;

	[Header("Render")]
	public Camera targetCamera;

	public SubPixelSize subPixelSize = SubPixelSize.Sub2x2;

	public int maxIterations = 128;

	[Space(10f)]
	public RenderSize renderSize;

	public int fixedWidth;

	public int fixedHeight;

	[Range(1f, 8f)]
	public int downsample = 2;

	[Header("Coverage")]
	public Texture cloudCoverage;

	[Range(0f, 1f)]
	public float coverageOffsetX;

	[Range(0f, 1f)]
	public float coverageOffsetY;

	[Range(0f, 1f)]
	public float horizonCoverageStart = 0.3f;

	[Range(0f, 1f)]
	public float horizonCoverageEnd = 0.4f;

	[Header("Lighting")]
	public Color cloudBaseColor = Color32.op_Implicit(new Color32((byte)132, (byte)170, (byte)208, byte.MaxValue));

	public Color cloudTopColor = Color32.op_Implicit(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));

	public Light sunLight;

	[Range(0f, 5f)]
	public float sunScalar = 1f;

	[Range(0f, 5f)]
	public float ambientScalar = 1f;

	[Range(0f, 1f)]
	public float sunRayLength = 0.08f;

	[Range(0f, 1f)]
	public float coneRadius = 0.08f;

	[Range(0f, 30f)]
	public float density = 1f;

	[Range(0f, 1f)]
	public float forwardScatteringG = 0.8f;

	[Range(0f, -1f)]
	public float backwardScatteringG = -0.5f;

	[Range(0f, 1f)]
	public float darkOutlineScalar = 1f;

	[Header("Animation")]
	[Range(-10f, 10f)]
	public float animationScale = 1f;

	public Vector2 coverageOffsetPerFrame;

	public Vector3 baseOffsetPerFrame = new Vector3(0f, -0.001f, 0f);

	public Vector3 detailOffsetPerFrame;

	[Header("Modeling (Base)")]
	public float baseScale = 1f;

	public Gradient cloudGradient1;

	public Gradient cloudGradient2;

	public Gradient cloudGradient3;

	[Space(10f)]
	[Range(0f, 1f)]
	public float sampleScalar = 1f;

	[Range(0f, 1f)]
	public float sampleThreshold = 0.05f;

	[Range(0f, 1f)]
	public float cloudBottomFade = 0.3f;

	[Header("Modeling (Detail)")]
	public float detailScale = 8f;

	[Space(10f)]
	[Range(0f, 1f)]
	public float erosionEdgeSize = 0.5f;

	[Range(0f, 1f)]
	public float cloudDistortion = 0.45f;

	public float cloudDistortionScale = 0.5f;

	[Header("Optimization")]
	[Range(0f, 1f)]
	public float lodDistance = 0.3f;

	[Range(0f, 1f)]
	public float horizonLevel;

	[Range(0f, 1f)]
	public float horizonFade = 0.25f;

	[Range(0f, 1f)]
	public float horizonFadeStartAlpha = 0.9f;

	[Header("Atmosphere:")]
	public float horizonDistance = 35000f;

	public float atmosphereStartHeight = 1500f;

	public float atmosphereEndHeight = 4000f;

	public Vector3 cameraPositionScaler = new Vector3(1f, 1f, 1f);

	private Material _cloudMaterial;

	private Material _cloudCombinerMaterial;

	private Material _cloudBlenderMaterial;

	private Material _cloudShadowPassMaterial;

	private Material _depthMaskDilateMaterial;

	private Shader _linearDepthShader;

	private bool _renderingDepth;

	private Camera _camera;

	private Texture3D _perlin3D;

	private Texture3D _detail3D;

	private Texture2D _curlTexture;

	private Vector2 _coverageOffset;

	private Vector3 _baseOffset;

	private Vector3 _detailOffset;

	private RenderTexture _subFrame;

	private RenderTexture _previousFrame;

	private Vector4 _cloudGradientVector1;

	private Vector4 _cloudGradientVector2;

	private Vector4 _cloudGradientVector3;

	private Vector3[] _randomVectors;

	private bool _isFirstFrame;

	private SharedProperties _cloudsSharedProperties;

	private FullScreenQuad _fullScreenQuad;

	private Camera _depthCamera;

	[SerializeField]
	private Renderer _renderer;

	public Vector2 coverageOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _coverageOffset;
		}
	}

	public RenderTexture currentFrame => _previousFrame;

	public SharedProperties cloudsSharedProperties => _cloudsSharedProperties;

	public FullScreenQuad fullScreenQuad => _fullScreenQuad;

	public bool DoDepth { get; set; }

	private void OnEnable()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		Camera.onPreCull = (CameraCallback)Delegate.Combine((Delegate?)(object)Camera.onPreCull, (Delegate?)new CameraCallback(CloudsOnPreCull));
		CreateDepthCameraIfNeeded();
		CreateMaterialsIfNeeded();
		CreateRenderTextures();
		CreateFullscreenQuad();
	}

	private void OnDrawGizmos()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		float earthRadius = cloudsSharedProperties.earthRadius;
		float num = cloudsSharedProperties.atmosphereStartHeight;
		float num2 = cloudsSharedProperties.atmosphereEndHeight;
		float num3 = earthRadius + num;
		float num4 = earthRadius + num2;
		float num5 = cloudsSharedProperties.maxDistance - cloudsSharedProperties.maxRayDistance;
		Vector3 position = ((Component)this).transform.position;
		Vector3 val = position;
		val.y -= earthRadius;
		Gizmos.color = Color.gray;
		Gizmos.DrawWireSphere(val, num3);
		Gizmos.color = Color.black;
		Gizmos.DrawWireSphere(val, num4);
		Gizmos.color = Color.white;
		Gizmos.DrawLine(position + new Vector3(0f, num, 0f), position + new Vector3(0f, num2, 0f));
		Gizmos.color = Color.red;
		Gizmos.DrawLine(position, position + new Vector3(num5, 0f, 0f));
		Gizmos.color = Color.green;
		Gizmos.DrawLine(position, position + new Vector3(0f, cloudsSharedProperties.atmosphereStartHeight, 0f));
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(position, position + new Vector3(0f, 0f, num5));
	}

	private void OnDisable()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		Camera.onPreCull = (CameraCallback)Delegate.Remove((Delegate?)(object)Camera.onPreCull, (Delegate?)new CameraCallback(CloudsOnPreCull));
		DestroyMaterials();
		DestroyRenderTextures();
		DestroyFullscreenQuad();
		DestroyDepthCamera();
	}

	private void OnValidate()
	{
		if (_cloudsSharedProperties == null)
		{
			_cloudsSharedProperties = new SharedProperties();
		}
		_coverageOffset.Set(coverageOffsetX, coverageOffsetY);
		UpdateGradientVectors();
		UpdateSharedFromPublicProperties();
		if (onValidateDelegate != null)
		{
			onValidateDelegate();
		}
	}

	private void Reset()
	{
		cloudGradient1 = cloudGradient1 ?? CreateCloudGradient(0.011f, 0.098f, 0.126f, 0.225f);
		cloudGradient2 = cloudGradient2 ?? CreateCloudGradient(0f, 0.096f, 0.311f, 0.506f);
		cloudGradient3 = cloudGradient3 ?? CreateCloudGradient(0f, 0.087f, 0.749f, 1f);
		UpdateGradientVectors();
		_cloudsSharedProperties = new SharedProperties();
		UpdateSharedFromPublicProperties();
	}

	private void Awake()
	{
		UpdateGradientVectors();
		_cloudsSharedProperties = new SharedProperties();
		UpdateSharedFromPublicProperties();
	}

	private void Start()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Invalid comparison between Unknown and I4
		targetCamera = (MPExtensions.IsMultiplayer(Game.Mode) ? PlayerRegistry.LocalPlayer.PlayerCamera.Camera : Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(Cameras.AllCameras)?.Camera);
		if ((Object)(object)targetCamera == (Object)null)
		{
			if ((int)Game.State != 4)
			{
				Debug.LogError((object)"VolumetricClouds:: No player camera found!");
			}
			targetCamera = Cameras.Main;
		}
		SetCamera(targetCamera);
		_cloudsSharedProperties = new SharedProperties();
		UpdateSharedFromPublicProperties();
		CreateMaterialsIfNeeded();
		CreateRenderTextures();
		CreateFullscreenQuad();
	}

	private void LateUpdate()
	{
		if (!LevelLoader.IsLoading())
		{
			_renderer.sharedMaterial.SetTexture("_CloudTex", (Texture)(object)currentFrame);
		}
	}

	public void UpdateAnimatedProperties()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if (Time.timeScale != 0f && animationScale != 0f)
		{
			_coverageOffset += coverageOffsetPerFrame * animationScale;
			_baseOffset += baseOffsetPerFrame * animationScale;
			_detailOffset += detailOffsetPerFrame * animationScale;
		}
	}

	private void CloudsOnPreCull(Camera currentCamera)
	{
		if (!_renderingDepth && (Object)(object)_camera != (Object)null && (Object)(object)currentCamera == (Object)(object)_camera)
		{
			UpdateAnimatedProperties();
			RenderClouds();
		}
	}

	public void CopyPropertiesToRenderSettings(RenderSettings settings)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		settings.sunColor = sunLight.color;
		settings.sunDirection = ((Component)sunLight).transform.eulerAngles;
		settings.cloudBaseColor = cloudBaseColor;
		settings.cloudTopColor = cloudTopColor;
	}

	public void CopyRenderSettingsToProperties(RenderSettings settings)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		sunLight.color = settings.sunColor;
		((Component)sunLight).transform.eulerAngles = settings.sunDirection;
		cloudBaseColor = settings.cloudBaseColor;
		cloudTopColor = settings.cloudTopColor;
	}

	public void SetCamera(Camera theCamera)
	{
		if ((Object)(object)theCamera != (Object)(object)_camera)
		{
			_camera = theCamera;
			if ((Object)(object)_fullScreenQuad != (Object)null)
			{
				_fullScreenQuad.targetCamera = theCamera;
			}
		}
	}

	public void UpdateSharedFromPublicProperties()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		float num = _cloudsSharedProperties.CalculatePlanetRadius(atmosphereStartHeight, horizonDistance);
		_coverageOffset.Set(coverageOffsetX, coverageOffsetY);
		_cloudsSharedProperties.earthRadius = num;
		_cloudsSharedProperties.atmosphereStartHeight = atmosphereStartHeight;
		_cloudsSharedProperties.atmosphereEndHeight = atmosphereEndHeight;
		_cloudsSharedProperties.cameraPosition = new Vector3(0f, num, 0f);
		_cloudsSharedProperties.subPixelSize = SubPixelSizeToInt(subPixelSize);
		_cloudsSharedProperties.downsample = downsample;
		_cloudsSharedProperties.useFixedDimensions = renderSize == RenderSize.FixedDimensions;
		_cloudsSharedProperties.fixedWidth = fixedWidth;
		_cloudsSharedProperties.fixedHeight = fixedHeight;
	}

	private void UpdateGradientVectors()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_cloudGradientVector1 = CloudHeightGradient(cloudGradient1);
		_cloudGradientVector2 = CloudHeightGradient(cloudGradient2);
		_cloudGradientVector3 = CloudHeightGradient(cloudGradient3);
	}

	private int SubPixelSizeToInt(SubPixelSize size)
	{
		int result = 2;
		switch (size)
		{
		case SubPixelSize.Sub1x1:
			result = 1;
			break;
		case SubPixelSize.Sub2x2:
			result = 2;
			break;
		case SubPixelSize.Sub4x4:
			result = 4;
			break;
		case SubPixelSize.Sub8x8:
			result = 8;
			break;
		}
		return result;
	}

	public void RenderClouds()
	{
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_camera == (Object)null))
		{
			CreateMaterialsIfNeeded();
			_cloudsSharedProperties.BeginFrame(_camera);
			if ((Object)(object)_subFrame == (Object)null || (Object)(object)_previousFrame == (Object)null || _cloudsSharedProperties.dimensionsChangedSinceLastFrame)
			{
				DestroyRenderTextures();
				CreateRenderTextures();
			}
			if (DoDepth)
			{
				CreateDepthCameraIfNeeded();
				_depthCamera.CopyFrom(_camera);
				((Behaviour)_depthCamera).enabled = false;
				_depthCamera.allowHDR = false;
				int num = 4;
				RenderTexture temporary = RenderTexture.GetTemporary(((Texture)_subFrame).width / num, ((Texture)_subFrame).height / num, _subFrame.depth, (RenderTextureFormat)16);
				_depthCamera.targetTexture = temporary;
				_depthCamera.clearFlags = (CameraClearFlags)2;
				_depthCamera.backgroundColor = new Color(1f, 1f, 1f, 1f);
				_depthCamera.cullingMask = (1 << Layers.TERRAIN_OBJECTS) | (1 << Layers.TERRAIN) | (1 << Layers.CONSTRUCTIONS);
				_depthCamera.renderingPath = (RenderingPath)1;
				_depthCamera.SetReplacementShader(_linearDepthShader, (string)null);
				_depthCamera.Render();
				_depthCamera.targetTexture = null;
				RenderTexture temporary2 = RenderTexture.GetTemporary(((Texture)temporary).width, ((Texture)temporary).height, temporary.depth, temporary.format);
				((Texture)temporary).filterMode = (FilterMode)0;
				((Texture)temporary2).filterMode = (FilterMode)0;
				Graphics.Blit((Texture)(object)temporary, temporary2, _depthMaskDilateMaterial);
				Graphics.Blit((Texture)(object)temporary2, temporary, _depthMaskDilateMaterial);
				RenderTexture.ReleaseTemporary(temporary2);
				RenderTexture.ReleaseTemporary(temporary);
			}
			Vector3 cameraPosition = Vector3.Scale(((Component)_camera).transform.position - ((Component)this).transform.position, cameraPositionScaler);
			cameraPosition.y += _cloudsSharedProperties.earthRadius;
			_cloudsSharedProperties.cameraPosition = cameraPosition;
			_cloudsSharedProperties.ApplyToMaterial(_cloudMaterial, jitterProjection: true);
			UpdateMaterialsPublicProperties();
			RenderTexture active = RenderTexture.active;
			Graphics.Blit((Texture)null, _subFrame, _cloudMaterial);
			if (_isFirstFrame)
			{
				Graphics.Blit((Texture)(object)_subFrame, _previousFrame);
				_isFirstFrame = false;
			}
			_cloudCombinerMaterial.SetTexture("_SubFrame", (Texture)(object)_subFrame);
			_cloudCombinerMaterial.SetTexture("_PrevFrame", (Texture)(object)_previousFrame);
			_cloudsSharedProperties.ApplyToMaterial(_cloudCombinerMaterial);
			RenderTextureFormat val = (RenderTextureFormat)(_camera.allowHDR ? 9 : 7);
			RenderTexture temporary3 = RenderTexture.GetTemporary(((Texture)_previousFrame).width, ((Texture)_previousFrame).height, 0, val, (RenderTextureReadWrite)1);
			Graphics.Blit((Texture)null, temporary3, _cloudCombinerMaterial);
			Graphics.Blit((Texture)(object)temporary3, _previousFrame);
			RenderTexture.active = active;
			RenderTexture.ReleaseTemporary(temporary3);
			_cloudsSharedProperties.EndFrame();
			_cloudBlenderMaterial.SetTexture("_MainTex", (Texture)(object)currentFrame);
			_cloudBlenderMaterial.SetInt("_IsGamma", ((int)QualitySettings.activeColorSpace == 0) ? 1 : 0);
		}
	}

	private Gradient CreateCloudGradient(float position0, float position1, float position2, float position3)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected Obj, but got Unknown
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		Gradient val = new Gradient();
		val.colorKeys = new GradientColorKey[4]
		{
			new GradientColorKey(Color.black, position0),
			new GradientColorKey(Color.white, position1),
			new GradientColorKey(Color.white, position2),
			new GradientColorKey(Color.black, position3)
		};
		return val;
	}

	private void CreateDepthCameraIfNeeded()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_depthCamera == (Object)null)
		{
			_depthCamera = new GameObject().AddComponent<Camera>();
			((Object)((Component)_depthCamera).gameObject).hideFlags = (HideFlags)61;
			((Behaviour)_depthCamera).enabled = false;
		}
	}

	private void CreateMaterialsIfNeeded()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected Obj, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected Obj, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected Obj, but got Unknown
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Expected Obj, but got Unknown
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Expected Obj, but got Unknown
		if (_randomVectors == null || _randomVectors.Length < 1)
		{
			_randomVectors = new Vector3[6]
			{
				Random.onUnitSphere,
				Random.onUnitSphere,
				Random.onUnitSphere,
				Random.onUnitSphere,
				Random.onUnitSphere,
				Random.onUnitSphere
			};
		}
		if ((Object)(object)_cloudMaterial == (Object)null)
		{
			_cloudMaterial = new Material(Shader.Find("Hidden/kode80/VolumeClouds"));
			((Object)_cloudMaterial).hideFlags = (HideFlags)61;
		}
		if ((Object)(object)_cloudCombinerMaterial == (Object)null)
		{
			_cloudCombinerMaterial = new Material(Shader.Find("Hidden/kode80/CloudCombiner"));
			((Object)_cloudCombinerMaterial).hideFlags = (HideFlags)61;
		}
		if ((Object)(object)_cloudBlenderMaterial == (Object)null)
		{
			_cloudBlenderMaterial = new Material(Shader.Find("Hidden/kode80/CloudBlender"));
			((Object)_cloudBlenderMaterial).hideFlags = (HideFlags)61;
		}
		if ((Object)(object)_cloudShadowPassMaterial == (Object)null)
		{
			_cloudShadowPassMaterial = new Material(Shader.Find("Hidden/kode80/CloudShadowPass"));
			((Object)_cloudShadowPassMaterial).hideFlags = (HideFlags)61;
		}
		if ((Object)(object)_depthMaskDilateMaterial == (Object)null)
		{
			_depthMaskDilateMaterial = new Material(Shader.Find("Hidden/kode80/DepthMaskDilate"));
			((Object)_depthMaskDilateMaterial).hideFlags = (HideFlags)61;
		}
		if ((Object)(object)_perlin3D == (Object)null)
		{
			_perlin3D = Load3DTexture("kode80Clouds/noise", 128, (TextureFormat)4);
		}
		if ((Object)(object)_detail3D == (Object)null)
		{
			_detail3D = Load3DTexture("kode80Clouds/noise_detail", 32, (TextureFormat)3);
		}
		if ((Object)(object)_curlTexture == (Object)null)
		{
			Object val = Resources.Load("kode80Clouds/CurlNoise");
			_curlTexture = (Texture2D)(object)((val is Texture2D) ? val : null);
		}
		if ((Object)(object)_linearDepthShader == (Object)null)
		{
			_linearDepthShader = Shader.Find("Hidden/kode80/LinearDepth");
		}
	}

	private void CreateFullscreenQuad()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected Obj, but got Unknown
		if (Application.isPlaying && (Object)(object)_fullScreenQuad == (Object)null)
		{
			GameObject val = new GameObject("CloudsFullscreenQuad", new Type[1] { typeof(FullScreenQuad) });
			((Object)val).hideFlags = (HideFlags)61;
			_fullScreenQuad = val.GetComponent<FullScreenQuad>();
			_fullScreenQuad.material = _cloudBlenderMaterial;
			_fullScreenQuad.renderWhenPlaying = true;
		}
	}

	private void DestroyDepthCamera()
	{
		if (Object.op_Implicit((Object)(object)_depthCamera))
		{
			Object.DestroyImmediate((Object)(object)((Component)_depthCamera).gameObject);
			_depthCamera = null;
		}
	}

	private void DestroyFullscreenQuad()
	{
		if ((Object)(object)_fullScreenQuad != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)((Component)_fullScreenQuad).gameObject);
			_fullScreenQuad = null;
		}
	}

	private Texture3D Load3DTexture(string name, int size, TextureFormat format)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Invalid comparison between Unknown and I4
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected Obj, but got Unknown
		int num = size * size * size;
		TextAsset val = Resources.Load<TextAsset>(name);
		Color32[] array = new Color32[num];
		byte[] bytes = val.bytes;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			array[i].r = bytes[num2++];
			array[i].g = bytes[num2++];
			array[i].b = bytes[num2++];
			array[i].a = (((int)format == 4) ? bytes[num2++] : byte.MaxValue);
		}
		Texture3D val2 = new Texture3D(size, size, size, format, true)
		{
			hideFlags = (HideFlags)61,
			wrapMode = (TextureWrapMode)0,
			filterMode = (FilterMode)1
		};
		val2.SetPixels32(array, 0);
		val2.Apply();
		return val2;
	}

	private void DestroyMaterials()
	{
		Object.DestroyImmediate((Object)(object)_cloudMaterial);
		_cloudMaterial = null;
		Object.DestroyImmediate((Object)(object)_cloudCombinerMaterial);
		_cloudCombinerMaterial = null;
		Object.DestroyImmediate((Object)(object)_cloudBlenderMaterial);
		_cloudBlenderMaterial = null;
		Object.DestroyImmediate((Object)(object)_cloudShadowPassMaterial);
		_cloudShadowPassMaterial = null;
		Object.DestroyImmediate((Object)(object)_depthMaskDilateMaterial);
		_depthMaskDilateMaterial = null;
		_linearDepthShader = null;
		Object.DestroyImmediate((Object)(object)_perlin3D);
		_perlin3D = null;
		Object.DestroyImmediate((Object)(object)_detail3D);
		_detail3D = null;
		Resources.UnloadAsset((Object)(object)_curlTexture);
		_curlTexture = null;
	}

	private void CreateRenderTextures()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected Obj, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected Obj, but got Unknown
		if ((Object)(object)_subFrame == (Object)null && (Object)(object)_camera != (Object)null)
		{
			RenderTextureFormat val = (RenderTextureFormat)(_camera.allowHDR ? 9 : 7);
			_subFrame = new RenderTexture(_cloudsSharedProperties.subFrameWidth, _cloudsSharedProperties.subFrameHeight, 0, val, (RenderTextureReadWrite)1);
			((Texture)_subFrame).filterMode = (FilterMode)1;
			((Object)_subFrame).hideFlags = (HideFlags)61;
			_isFirstFrame = true;
		}
		if ((Object)(object)_previousFrame == (Object)null && (Object)(object)_camera != (Object)null)
		{
			RenderTextureFormat val2 = (RenderTextureFormat)(_camera.allowHDR ? 9 : 7);
			_previousFrame = new RenderTexture(_cloudsSharedProperties.frameWidth, _cloudsSharedProperties.frameHeight, 0, val2, (RenderTextureReadWrite)1);
			((Texture)_previousFrame).filterMode = (FilterMode)1;
			((Object)_previousFrame).hideFlags = (HideFlags)61;
			_isFirstFrame = true;
		}
	}

	private void DestroyRenderTextures()
	{
		Object.DestroyImmediate((Object)(object)_subFrame);
		_subFrame = null;
		Object.DestroyImmediate((Object)(object)_previousFrame);
		_previousFrame = null;
	}

	private Light FindDirectionalLightInScene()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		Light[] array = Object.FindObjectsOfType<Light>();
		foreach (Light val in array)
		{
			if ((int)val.type == 1)
			{
				return val;
			}
		}
		return null;
	}

	private void UpdateMaterialsPublicProperties()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_0463: Unknown result type (might be due to invalid IL or missing references)
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)_cloudMaterial) && Object.op_Implicit((Object)(object)_camera))
		{
			Vector3 forward = ((Component)sunLight).transform.forward;
			_cloudMaterial.SetFloat("_CloudBottomFade", cloudBottomFade);
			_cloudMaterial.SetFloat("_MaxIterations", (float)maxIterations);
			_cloudMaterial.SetFloat("_SampleScalar", sampleScalar);
			_cloudMaterial.SetFloat("_SampleThreshold", sampleThreshold);
			_cloudMaterial.SetFloat("_LODDistance", lodDistance);
			_cloudMaterial.SetFloat("_RayMinimumY", horizonLevel);
			_cloudMaterial.SetFloat("_DetailScale", detailScale);
			_cloudMaterial.SetFloat("_ErosionEdgeSize", erosionEdgeSize);
			_cloudMaterial.SetFloat("_CloudDistortion", cloudDistortion);
			_cloudMaterial.SetFloat("_CloudDistortionScale", cloudDistortionScale);
			_cloudMaterial.SetFloat("_HorizonFadeScalar", horizonFade);
			_cloudMaterial.SetFloat("_HorizonFadeStartAlpha", horizonFadeStartAlpha);
			_cloudMaterial.SetFloat("_OneMinusHorizonFadeStartAlpha", 1f - horizonFadeStartAlpha);
			_cloudMaterial.SetTexture("_Perlin3D", (Texture)(object)_perlin3D);
			_cloudMaterial.SetTexture("_Detail3D", (Texture)(object)_detail3D);
			_cloudMaterial.SetVector("_BaseOffset", Vector4.op_Implicit(_baseOffset));
			_cloudMaterial.SetVector("_DetailOffset", Vector4.op_Implicit(_detailOffset));
			_cloudMaterial.SetFloat("_BaseScale", 1f / atmosphereEndHeight * baseScale);
			_cloudMaterial.SetFloat("_LightScalar", sunScalar);
			_cloudMaterial.SetFloat("_AmbientScalar", ambientScalar);
			_cloudMaterial.SetVector("_CloudHeightGradient1", _cloudGradientVector1);
			_cloudMaterial.SetVector("_CloudHeightGradient2", _cloudGradientVector2);
			_cloudMaterial.SetVector("_CloudHeightGradient3", _cloudGradientVector3);
			_cloudMaterial.SetTexture("_Coverage", cloudCoverage);
			_cloudMaterial.SetVector("_LightDirection", Vector4.op_Implicit(forward));
			_cloudMaterial.SetColor("_LightColor", sunLight.color);
			_cloudMaterial.SetColor("_CloudBaseColor", cloudBaseColor);
			_cloudMaterial.SetColor("_CloudTopColor", cloudTopColor);
			_cloudMaterial.SetFloat("_HorizonCoverageStart", horizonCoverageStart);
			_cloudMaterial.SetFloat("_HorizonCoverageEnd", horizonCoverageEnd);
			_cloudMaterial.SetFloat("_Density", density);
			_cloudMaterial.SetFloat("_ForwardScatteringG", forwardScatteringG);
			_cloudMaterial.SetFloat("_BackwardScatteringG", backwardScatteringG);
			_cloudMaterial.SetFloat("_DarkOutlineScalar", darkOutlineScalar);
			float num = atmosphereEndHeight - atmosphereStartHeight;
			_cloudMaterial.SetFloat("_SunRayLength", sunRayLength * num);
			_cloudMaterial.SetFloat("_ConeRadius", coneRadius * num);
			_cloudMaterial.SetFloat("_RayStepLength", num / Mathf.Floor((float)maxIterations / 2f));
			_cloudMaterial.SetTexture("_Curl2D", (Texture)(object)_curlTexture);
			_cloudMaterial.SetFloat("_CoverageScale", 1f / _cloudsSharedProperties.maxDistance);
			_cloudMaterial.SetVector("_CoverageOffset", Vector4.op_Implicit(_coverageOffset));
			_cloudMaterial.SetFloat("_MaxRayDistance", _cloudsSharedProperties.maxRayDistance);
			_cloudMaterial.SetVector("_Random0", Vector4.op_Implicit(_randomVectors[0]));
			_cloudMaterial.SetVector("_Random1", Vector4.op_Implicit(_randomVectors[1]));
			_cloudMaterial.SetVector("_Random2", Vector4.op_Implicit(_randomVectors[2]));
			_cloudMaterial.SetVector("_Random3", Vector4.op_Implicit(_randomVectors[3]));
			_cloudMaterial.SetVector("_Random4", Vector4.op_Implicit(_randomVectors[4]));
			_cloudMaterial.SetVector("_Random5", Vector4.op_Implicit(_randomVectors[5]));
		}
	}

	private Vector4 CloudHeightGradient(Gradient gradient)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		int num = gradient.colorKeys.Length;
		float num2 = ((num > 0) ? gradient.colorKeys[0].time : 0f);
		float num3 = ((num > 1) ? gradient.colorKeys[1].time : num2);
		float num4 = ((num > 2) ? gradient.colorKeys[2].time : num3);
		float num5 = ((num > 3) ? gradient.colorKeys[3].time : num4);
		return new Vector4(num2, num3, num4, num5);
	}

	public kode80Clouds()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
	}
}
