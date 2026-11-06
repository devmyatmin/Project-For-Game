using UnityEngine;

[ImageEffectAllowedInSceneView]
[HelpURL("http://www.thomashourdel.com/ssaopro/doc/")]
[ExecuteInEditMode]
[AddComponentMenu("Image Effects/SSAO Pro")]
[RequireComponent(typeof(Camera))]
public class SSAOPro : MonoBehaviour
{
	public enum BlurMode
	{
		None,
		Gaussian,
		HighQualityBilateral
	}

	public enum SampleCount
	{
		VeryLow,
		Low,
		Medium,
		High,
		Ultra
	}

	protected enum Pass
	{
		Clear = 0,
		GaussianBlur = 5,
		HighQualityBilateralBlur = 6,
		Composite = 7
	}

	public Texture2D NoiseTexture;

	public bool UseHighPrecisionDepthMap;

	public SampleCount Samples = SampleCount.Medium;

	[Range(1f, 4f)]
	public int Downsampling = 1;

	[Range(0.01f, 1.25f)]
	public float Radius = 0.12f;

	[Range(0f, 16f)]
	public float Intensity = 2.5f;

	[Range(0f, 10f)]
	public float Distance = 1f;

	[Range(0f, 1f)]
	public float Bias = 0.1f;

	[Range(0f, 1f)]
	public float LumContribution = 0.5f;

	[ColorUsage(false)]
	public Color OcclusionColor = Color.black;

	public float CutoffDistance = 150f;

	public float CutoffFalloff = 50f;

	public BlurMode Blur = BlurMode.HighQualityBilateral;

	public bool BlurDownsampling;

	[Range(1f, 4f)]
	public int BlurPasses = 1;

	[Range(1f, 20f)]
	public float BlurBilateralThreshold = 10f;

	public bool DebugAO;

	protected Shader m_ShaderSSAO;

	protected Material m_Material;

	protected Camera m_Camera;

	public Material Material
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected Obj, but got Unknown
			if ((Object)(object)m_Material == (Object)null)
			{
				m_Material = new Material(ShaderSSAO)
				{
					hideFlags = (HideFlags)61
				};
			}
			return m_Material;
		}
	}

	public Shader ShaderSSAO
	{
		get
		{
			if ((Object)(object)m_ShaderSSAO == (Object)null)
			{
				m_ShaderSSAO = Shader.Find("Hidden/SSAO Pro V2");
			}
			return m_ShaderSSAO;
		}
	}

	private void OnEnable()
	{
		m_Camera = ((Component)this).GetComponent<Camera>();
		if ((Object)(object)ShaderSSAO == (Object)null)
		{
			Debug.LogWarning((object)"Missing shader (SSAO).");
			((Behaviour)this).enabled = false;
		}
		else if (!ShaderSSAO.isSupported)
		{
			Debug.LogWarning((object)"Unsupported shader (SSAO).");
			((Behaviour)this).enabled = false;
		}
		else if (!SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat)1))
		{
			Debug.LogWarning((object)"Depth textures aren't supported on this device.");
			((Behaviour)this).enabled = false;
		}
	}

	private void OnPreRender()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Camera camera = m_Camera;
		camera.depthTextureMode = (DepthTextureMode)(camera.depthTextureMode | 3);
	}

	private void OnDisable()
	{
		if ((Object)(object)m_Material != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)m_Material);
		}
		m_Material = null;
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ShaderSSAO == (Object)null || Mathf.Approximately(Intensity, 0f))
		{
			Graphics.Blit((Texture)(object)source, destination);
			return;
		}
		Material.shaderKeywords = null;
		switch (Samples)
		{
		case SampleCount.Low:
			Material.EnableKeyword("SAMPLES_LOW");
			break;
		case SampleCount.Medium:
			Material.EnableKeyword("SAMPLES_MEDIUM");
			break;
		case SampleCount.High:
			Material.EnableKeyword("SAMPLES_HIGH");
			break;
		case SampleCount.Ultra:
			Material.EnableKeyword("SAMPLES_ULTRA");
			break;
		}
		int num = 0;
		if ((Object)(object)NoiseTexture != (Object)null)
		{
			num = 1;
		}
		if (!Mathf.Approximately(LumContribution, 0f))
		{
			num += 2;
		}
		num++;
		Material material = Material;
		Matrix4x4 val = m_Camera.projectionMatrix * m_Camera.worldToCameraMatrix;
		material.SetMatrix("_InverseViewProject", val.inverse);
		Material.SetMatrix("_CameraModelView", m_Camera.cameraToWorldMatrix);
		Material.SetTexture("_NoiseTex", (Texture)(object)NoiseTexture);
		Material.SetVector("_Params1", new Vector4(((Object)(object)NoiseTexture == (Object)null) ? 0f : ((float)((Texture)NoiseTexture).width), Radius, Intensity, Distance));
		Material.SetVector("_Params2", new Vector4(Bias, LumContribution, CutoffDistance, CutoffFalloff));
		Material.SetColor("_OcclusionColor", OcclusionColor);
		if (Blur == BlurMode.None)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(((Texture)source).width / Downsampling, ((Texture)source).height / Downsampling, 0, (RenderTextureFormat)0);
			Graphics.Blit((Texture)(object)temporary, temporary, Material, 0);
			if (DebugAO)
			{
				Graphics.Blit((Texture)(object)source, temporary, Material, num);
				Graphics.Blit((Texture)(object)temporary, destination);
				RenderTexture.ReleaseTemporary(temporary);
			}
			else
			{
				Graphics.Blit((Texture)(object)source, temporary, Material, num);
				Material.SetTexture("_SSAOTex", (Texture)(object)temporary);
				Graphics.Blit((Texture)(object)source, destination, Material, 7);
				RenderTexture.ReleaseTemporary(temporary);
			}
			return;
		}
		Pass pass = ((Blur == BlurMode.HighQualityBilateral) ? Pass.HighQualityBilateralBlur : Pass.GaussianBlur);
		int num2 = ((!BlurDownsampling) ? 1 : Downsampling);
		RenderTexture temporary2 = RenderTexture.GetTemporary(((Texture)source).width / num2, ((Texture)source).height / num2, 0, (RenderTextureFormat)0);
		RenderTexture temporary3 = RenderTexture.GetTemporary(((Texture)source).width / Downsampling, ((Texture)source).height / Downsampling, 0, (RenderTextureFormat)0);
		Graphics.Blit((Texture)(object)temporary2, temporary2, Material, 0);
		Graphics.Blit((Texture)(object)source, temporary2, Material, num);
		Material.SetFloat("_BilateralThreshold", BlurBilateralThreshold * 5f);
		for (int i = 0; i < BlurPasses; i++)
		{
			Material.SetVector("_Direction", Vector4.op_Implicit(new Vector2(1f / (float)((Texture)source).width, 0f)));
			Graphics.Blit((Texture)(object)temporary2, temporary3, Material, (int)pass);
			temporary2.DiscardContents();
			Material.SetVector("_Direction", Vector4.op_Implicit(new Vector2(0f, 1f / (float)((Texture)source).height)));
			Graphics.Blit((Texture)(object)temporary3, temporary2, Material, (int)pass);
			temporary3.DiscardContents();
		}
		if (!DebugAO)
		{
			Material.SetTexture("_SSAOTex", (Texture)(object)temporary2);
			Graphics.Blit((Texture)(object)source, destination, Material, 7);
		}
		else
		{
			Graphics.Blit((Texture)(object)temporary2, destination);
		}
		RenderTexture.ReleaseTemporary(temporary2);
		RenderTexture.ReleaseTemporary(temporary3);
	}

	public SSAOPro()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
	}
}
