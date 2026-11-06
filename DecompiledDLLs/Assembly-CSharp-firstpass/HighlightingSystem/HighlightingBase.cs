using System.Collections.Generic;
using Beam.Events;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace HighlightingSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class HighlightingBase : MonoBehaviour
{
	protected static readonly Color colorClear = new Color(0f, 0f, 0f, 0f);

	protected static readonly string renderBufferName = "HighlightingSystem";

	protected static readonly Matrix4x4 identityMatrix = Matrix4x4.identity;

	protected static readonly string keywordStraightDirections = "STRAIGHT_DIRECTIONS";

	protected static readonly string keywordAllDirections = "ALL_DIRECTIONS";

	protected const CameraEvent queue = (CameraEvent)12;

	private static ShaderPropertyID _shaderPropertyID;

	protected CommandBuffer renderBuffer;

	protected int cachedWidth = -1;

	protected int cachedHeight = -1;

	protected int cachedAA = -1;

	[FormerlySerializedAs("downsampleFactor")]
	[SerializeField]
	protected int _downsampleFactor = 4;

	[FormerlySerializedAs("iterations")]
	[SerializeField]
	protected int _iterations = 2;

	[FormerlySerializedAs("blurMinSpread")]
	[SerializeField]
	protected float _blurMinSpread = 0.65f;

	[FormerlySerializedAs("blurSpread")]
	[SerializeField]
	protected float _blurSpread = 0.25f;

	[SerializeField]
	protected float _blurIntensity = 0.3f;

	[SerializeField]
	protected BlurDirections _blurDirections;

	[SerializeField]
	protected HighlightingBlitter _blitter;

	[SerializeField]
	protected AntiAliasing _antiAliasing;

	protected RenderTargetIdentifier highlightingBufferID;

	protected RenderTargetIdentifier blur1ID;

	protected RenderTargetIdentifier blur2ID;

	protected RenderTexture highlightingBuffer;

	protected Camera cam;

	protected const int BLUR = 0;

	protected const int CUT = 1;

	protected const int COMP = 2;

	protected static readonly string[] shaderPaths = new string[3] { "Hidden/Highlighted/Blur", "Hidden/Highlighted/Cut", "Hidden/Highlighted/Composite" };

	protected static Shader[] shaders;

	protected static Material[] materials;

	protected Material blurMaterial;

	protected Material cutMaterial;

	protected Material compMaterial;

	protected static bool initialized = false;

	protected static HashSet<Camera> cameras = new HashSet<Camera>();

	public static ShaderPropertyID shaderPropertyID
	{
		get
		{
			if (_shaderPropertyID == null)
			{
				_shaderPropertyID = new ShaderPropertyID();
			}
			return _shaderPropertyID;
		}
	}

	public bool isSupported => CheckSupported(verbose: false);

	public int downsampleFactor
	{
		get
		{
			return _downsampleFactor;
		}
		set
		{
			if (_downsampleFactor != value)
			{
				if (value != 0 && (value & (value - 1)) == 0)
				{
					_downsampleFactor = value;
				}
				else
				{
					Debug.LogWarning((object)"HighlightingSystem : Prevented attempt to set incorrect downsample factor value.");
				}
			}
		}
	}

	public int iterations
	{
		get
		{
			return _iterations;
		}
		set
		{
			if (_iterations != value)
			{
				_iterations = value;
			}
		}
	}

	public float blurMinSpread
	{
		get
		{
			return _blurMinSpread;
		}
		set
		{
			if (_blurMinSpread != value)
			{
				_blurMinSpread = value;
			}
		}
	}

	public float blurSpread
	{
		get
		{
			return _blurSpread;
		}
		set
		{
			if (_blurSpread != value)
			{
				_blurSpread = value;
			}
		}
	}

	public float blurIntensity
	{
		get
		{
			return _blurIntensity;
		}
		set
		{
			if (_blurIntensity != value)
			{
				_blurIntensity = value;
				if (Application.isPlaying)
				{
					blurMaterial.SetFloat(shaderPropertyID._Intensity, _blurIntensity);
				}
			}
		}
	}

	public BlurDirections blurDirections
	{
		get
		{
			return _blurDirections;
		}
		set
		{
			if (_blurDirections != value)
			{
				_blurDirections = value;
				if (Application.isPlaying)
				{
					SetKeyword(blurMaterial, keywordStraightDirections, _blurDirections == BlurDirections.Straight);
					SetKeyword(blurMaterial, keywordAllDirections, _blurDirections == BlurDirections.All);
				}
			}
		}
	}

	public HighlightingBlitter blitter
	{
		get
		{
			return _blitter;
		}
		set
		{
			if ((Object)(object)_blitter != (Object)(object)value)
			{
				if ((Object)(object)_blitter != (Object)null)
				{
					_blitter.Unregister(this);
				}
				_blitter = value;
				if ((Object)(object)_blitter != (Object)null)
				{
					_blitter.Register(this);
				}
			}
		}
	}

	public AntiAliasing antiAliasing
	{
		get
		{
			return _antiAliasing;
		}
		set
		{
			if (_antiAliasing != value)
			{
				_antiAliasing = value;
			}
		}
	}

	protected virtual void Start()
	{
		EventManager.AddListener<OptionsAppliedEvent>(EventManager_LoadOptions);
		LoadOptions();
	}

	private void EventManager_LoadOptions(OptionsAppliedEvent e)
	{
		LoadOptions();
	}

	private void LoadOptions()
	{
		Quality_OFF_ON showHighlighting = Options.GeneralSettings.ShowHighlighting;
		((Behaviour)this).enabled = showHighlighting == Quality_OFF_ON.ON;
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_LoadOptions);
	}

	protected virtual void OnEnable()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected Obj, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected Obj, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected Obj, but got Unknown
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected Obj, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		Initialize();
		if (!CheckSupported(verbose: true))
		{
			((Behaviour)this).enabled = false;
			Debug.LogError((object)"HighlightingSystem : Highlighting System has been disabled due to unsupported Unity features on the current platform!");
			return;
		}
		blur1ID = new RenderTargetIdentifier(shaderPropertyID._HighlightingBlur1);
		blur2ID = new RenderTargetIdentifier(shaderPropertyID._HighlightingBlur2);
		blurMaterial = new Material(materials[0]);
		cutMaterial = new Material(materials[1]);
		compMaterial = new Material(materials[2]);
		SetKeyword(blurMaterial, keywordStraightDirections, _blurDirections == BlurDirections.Straight);
		SetKeyword(blurMaterial, keywordAllDirections, _blurDirections == BlurDirections.All);
		blurMaterial.SetFloat(shaderPropertyID._Intensity, _blurIntensity);
		renderBuffer = new CommandBuffer();
		renderBuffer.name = renderBufferName;
		cam = ((Component)this).GetComponent<Camera>();
		cameras.Add(cam);
		Camera val = cam;
		val.depthTextureMode = (DepthTextureMode)(val.depthTextureMode | 1);
		cam.AddCommandBuffer((CameraEvent)12, renderBuffer);
		if ((Object)(object)_blitter != (Object)null)
		{
			_blitter.Register(this);
		}
	}

	protected virtual void OnDisable()
	{
		cameras.Remove(cam);
		if (renderBuffer != null)
		{
			cam.RemoveCommandBuffer((CameraEvent)12, renderBuffer);
			renderBuffer = null;
		}
		if ((Object)(object)highlightingBuffer != (Object)null && highlightingBuffer.IsCreated())
		{
			highlightingBuffer.Release();
			highlightingBuffer = null;
		}
		if ((Object)(object)_blitter != (Object)null)
		{
			_blitter.Unregister(this);
		}
	}

	protected virtual void OnPreRender()
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected Obj, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		int aA = GetAA();
		if ((Object)(object)highlightingBuffer == (Object)null || cam.pixelWidth != cachedWidth || cam.pixelHeight != cachedHeight || aA != cachedAA)
		{
			if ((Object)(object)highlightingBuffer != (Object)null)
			{
				if (highlightingBuffer.IsCreated())
				{
					highlightingBuffer.Release();
				}
				highlightingBuffer = null;
			}
			cachedWidth = cam.pixelWidth;
			cachedHeight = cam.pixelHeight;
			cachedAA = aA;
			highlightingBuffer = new RenderTexture(cachedWidth, cachedHeight, 24, (RenderTextureFormat)0, (RenderTextureReadWrite)0);
			highlightingBuffer.antiAliasing = cachedAA;
			((Texture)highlightingBuffer).filterMode = (FilterMode)0;
			highlightingBuffer.useMipMap = false;
			((Texture)highlightingBuffer).wrapMode = (TextureWrapMode)1;
			if (!highlightingBuffer.Create())
			{
				Debug.LogError((object)"HighlightingSystem : UpdateHighlightingBuffer() : Failed to create highlightingBuffer RenderTexture!");
			}
			highlightingBufferID = new RenderTargetIdentifier((Texture)(object)highlightingBuffer);
		}
		Shader.SetGlobalTexture(shaderPropertyID._HighlightingBuffer, (Texture)(object)highlightingBuffer);
		RebuildCommandBuffer();
	}

	public static bool IsHighlightingCamera(Camera cam)
	{
		return cameras.Contains(cam);
	}

	protected static void Initialize()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected Obj, but got Unknown
		if (!initialized)
		{
			int num = shaderPaths.Length;
			shaders = new Shader[num];
			materials = new Material[num];
			for (int i = 0; i < num; i++)
			{
				Shader val = Shader.Find(shaderPaths[i]);
				shaders[i] = val;
				Material val2 = new Material(val);
				materials[i] = val2;
			}
			initialized = true;
		}
	}

	protected virtual int GetAA()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between Unknown and I4
		int num = 1;
		switch (_antiAliasing)
		{
		case AntiAliasing.QualitySettings:
			if ((int)cam.actualRenderingPath == 2 || (int)cam.actualRenderingPath == 3)
			{
				num = 1;
				break;
			}
			num = QualitySettings.antiAliasing;
			if (num == 0)
			{
				num = 1;
			}
			break;
		case AntiAliasing.Disabled:
			num = 1;
			break;
		case AntiAliasing.MSAA2x:
			num = 2;
			break;
		case AntiAliasing.MSAA4x:
			num = 4;
			break;
		case AntiAliasing.MSAA8x:
			num = 8;
			break;
		}
		return num;
	}

	protected virtual bool CheckSupported(bool verbose)
	{
		bool result = true;
		if (!SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat)0))
		{
			if (verbose)
			{
				Debug.LogError((object)"HighlightingSystem : RenderTextureFormat.ARGB32 is not supported on this platform!");
			}
			result = false;
		}
		if (!Highlighter.opaqueShader.isSupported)
		{
			if (verbose)
			{
				Debug.LogError((object)"HighlightingSystem : HighlightingOpaque shader is not supported on this platform!");
			}
			result = false;
		}
		if (!Highlighter.transparentShader.isSupported)
		{
			if (verbose)
			{
				Debug.LogError((object)"HighlightingSystem : HighlightingTransparent shader is not supported on this platform!");
			}
			result = false;
		}
		for (int i = 0; i < shaders.Length; i++)
		{
			Shader val = shaders[i];
			if (!val.isSupported)
			{
				if (verbose)
				{
					Debug.LogError((object)("HighlightingSystem : Shader '" + ((Object)val).name + "' is not supported on this platform!"));
				}
				result = false;
			}
		}
		return result;
	}

	protected virtual void RebuildCommandBuffer()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		renderBuffer.Clear();
		renderBuffer.SetRenderTarget(highlightingBufferID);
		renderBuffer.ClearRenderTarget(true, true, colorClear);
		Highlighter.FillBuffer(renderBuffer);
		int num = ((Texture)highlightingBuffer).width / _downsampleFactor;
		int num2 = ((Texture)highlightingBuffer).height / _downsampleFactor;
		renderBuffer.GetTemporaryRT(shaderPropertyID._HighlightingBlur1, num, num2, 0, (FilterMode)1, (RenderTextureFormat)0, (RenderTextureReadWrite)0);
		renderBuffer.GetTemporaryRT(shaderPropertyID._HighlightingBlur2, num, num2, 0, (FilterMode)1, (RenderTextureFormat)0, (RenderTextureReadWrite)0);
		renderBuffer.Blit(highlightingBufferID, blur1ID);
		bool flag = true;
		for (int i = 0; i < _iterations; i++)
		{
			float num3 = _blurMinSpread + _blurSpread * (float)i;
			renderBuffer.SetGlobalFloat(shaderPropertyID._HighlightingBlurOffset, num3);
			if (flag)
			{
				renderBuffer.Blit(blur1ID, blur2ID, blurMaterial);
			}
			else
			{
				renderBuffer.Blit(blur2ID, blur1ID, blurMaterial);
			}
			flag = !flag;
		}
		renderBuffer.Blit(flag ? blur1ID : blur2ID, highlightingBufferID, cutMaterial);
		renderBuffer.ReleaseTemporaryRT(shaderPropertyID._HighlightingBlur1);
		renderBuffer.ReleaseTemporaryRT(shaderPropertyID._HighlightingBlur2);
	}

	public static void SetKeyword(Material mat, string keyword, bool state)
	{
		if (state != mat.IsKeywordEnabled(keyword))
		{
			if (state)
			{
				mat.EnableKeyword(keyword);
			}
			else
			{
				mat.DisableKeyword(keyword);
			}
		}
	}

	static HighlightingBase()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
	}
}
