using UnityEngine;

[ExecuteInEditMode]
public class DeluxeEyeAdaptation : MonoBehaviour
{
	[SerializeField]
	public DeluxeEyeAdaptationLogic m_Logic;

	[SerializeField]
	public bool m_LowResolution;

	[SerializeField]
	public bool m_ShowHistogram;

	private bool m_LastShowHistogram;

	[SerializeField]
	public bool m_ShowExposure;

	[SerializeField]
	public float m_HistogramSize = 0.5f;

	private Material m_HistogramMaterial;

	private Shader m_HistogramShader;

	private Material m_BrightnessMaterial;

	private Shader m_BrightnessShader;

	public bool m_LastUseGLSLFix;

	public bool m_UseGLSLFix;

	public bool m_ExposureOpened = true;

	public bool m_SpeedOpened = true;

	public bool m_HistogramOpened = true;

	public bool m_OptimizationOpened = true;

	public bool m_DebugOpened = true;

	public bool m_BannerIsOpened = true;

	public bool RenderDebugInfos
	{
		get
		{
			if (!m_ShowExposure)
			{
				return m_ShowHistogram;
			}
			return true;
		}
	}

	private Material CreateMaterial(Shader shader)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected Obj, but got Unknown
		if (!Object.op_Implicit((Object)(object)shader))
		{
			return null;
		}
		return new Material(shader)
		{
			hideFlags = (HideFlags)61
		};
	}

	private void DestroyMaterial(Material mat)
	{
		if (Object.op_Implicit((Object)(object)mat))
		{
			Object.DestroyImmediate((Object)(object)mat);
			mat = null;
		}
	}

	public void OnDisable()
	{
		if (m_Logic != null)
		{
			m_Logic.ClearMeshes();
		}
		DestroyMaterial(m_HistogramMaterial);
		m_HistogramMaterial = null;
		m_HistogramShader = null;
		DestroyMaterial(m_BrightnessMaterial);
		m_BrightnessMaterial = null;
		m_BrightnessShader = null;
	}

	private void CreateMaterials()
	{
		if (m_LastUseGLSLFix != m_UseGLSLFix)
		{
			m_HistogramShader = null;
			m_HistogramMaterial = null;
		}
		m_LastUseGLSLFix = m_UseGLSLFix;
		if ((Object)(object)m_HistogramShader == (Object)null)
		{
			if (m_UseGLSLFix)
			{
				m_HistogramShader = Shader.Find("Hidden/Deluxe/EyeAdaptationGLSLFix");
			}
			else
			{
				m_HistogramShader = Shader.Find("Hidden/Deluxe/EyeAdaptation");
			}
			m_LastShowHistogram = !m_ShowHistogram;
		}
		if ((Object)(object)m_HistogramMaterial == (Object)null && (Object)(object)m_HistogramShader != (Object)null && m_HistogramShader.isSupported)
		{
			m_HistogramMaterial = CreateMaterial(m_HistogramShader);
		}
		if ((Object)(object)m_BrightnessShader == (Object)null)
		{
			m_BrightnessShader = Shader.Find("Hidden/Deluxe/EyeAdaptationBright");
		}
		if ((Object)(object)m_BrightnessMaterial == (Object)null && (Object)(object)m_BrightnessShader != (Object)null && m_BrightnessShader.isSupported)
		{
			m_BrightnessMaterial = CreateMaterial(m_BrightnessShader);
		}
		else if ((Object)(object)m_BrightnessShader == (Object)null)
		{
			Debug.LogError((object)"Cant find brightness shader");
		}
		else if (!m_BrightnessShader.isSupported)
		{
			Debug.LogError((object)"Brightness shader unsupported");
		}
	}

	public void OnEnable()
	{
		CreateMaterials();
		VerifyAndUpdateShaderVariations(forceUpdate: true);
	}

	public void VerifyAndUpdateShaderVariations(bool forceUpdate)
	{
		if ((Object)(object)m_HistogramMaterial == (Object)null)
		{
			return;
		}
		if ((m_ShowHistogram != m_LastShowHistogram) | forceUpdate)
		{
			m_HistogramMaterial.DisableKeyword("DLX_DEBUG_HISTOGRAM");
			if (m_ShowHistogram)
			{
				m_HistogramMaterial.EnableKeyword("DLX_DEBUG_HISTOGRAM");
			}
		}
		m_LastShowHistogram = m_ShowHistogram;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		CreateMaterials();
		VerifyAndUpdateShaderVariations(forceUpdate: false);
		if (m_Logic == null)
		{
			m_Logic = new DeluxeEyeAdaptationLogic();
		}
		((Component)this).GetComponent<DeluxeTonemapper>();
		RenderTexture temporary = RenderTexture.GetTemporary(((Texture)source).width / 2, ((Texture)source).height / 2, 0, source.format);
		RenderTexture temporary2 = RenderTexture.GetTemporary(((Texture)temporary).width / 2, ((Texture)temporary).height / 2, 0, source.format);
		if ((Object)(object)m_BrightnessMaterial == (Object)null)
		{
			Graphics.Blit((Texture)(object)source, destination);
			return;
		}
		m_BrightnessMaterial.SetTexture("_UpTex", (Texture)(object)source);
		((Texture)source).filterMode = (FilterMode)1;
		m_BrightnessMaterial.SetVector("_PixelSize", new Vector4(1f / (float)((Texture)source).width * 0.5f, 1f / (float)((Texture)source).height * 0.5f));
		Graphics.Blit((Texture)(object)source, temporary, m_BrightnessMaterial, 1);
		m_BrightnessMaterial.SetTexture("_UpTex", (Texture)(object)temporary);
		((Texture)temporary).filterMode = (FilterMode)1;
		m_BrightnessMaterial.SetVector("_PixelSize", new Vector4(1f / (float)((Texture)temporary).width * 0.5f, 1f / (float)((Texture)temporary).height * 0.5f));
		Graphics.Blit((Texture)(object)temporary, temporary2, m_BrightnessMaterial, 1);
		((Texture)source).filterMode = (FilterMode)0;
		if (m_LowResolution)
		{
			RenderTexture temporary3 = RenderTexture.GetTemporary(((Texture)temporary2).width / 2, ((Texture)temporary2).height / 2, 0, source.format);
			m_BrightnessMaterial.SetTexture("_UpTex", (Texture)(object)temporary2);
			((Texture)temporary2).filterMode = (FilterMode)1;
			m_BrightnessMaterial.SetVector("_PixelSize", new Vector4(1f / (float)((Texture)temporary2).width * 0.5f, 1f / (float)((Texture)temporary2).height * 0.5f));
			Graphics.Blit((Texture)(object)temporary2, temporary3, m_BrightnessMaterial, 1);
			m_HistogramMaterial.SetTexture("_FrameTex", (Texture)(object)temporary3);
			m_Logic.ComputeExposure(((Texture)temporary3).width, ((Texture)temporary3).height, m_HistogramMaterial);
			RenderTexture.ReleaseTemporary(temporary3);
		}
		else
		{
			m_HistogramMaterial.SetTexture("_FrameTex", (Texture)(object)temporary2);
			m_Logic.ComputeExposure(((Texture)temporary2).width, ((Texture)temporary2).height, m_HistogramMaterial);
		}
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
		if (!Application.isPlaying)
		{
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = m_Logic.m_BrightnessRT;
			GL.Clear(false, true, Color.white);
			RenderTexture.active = active;
		}
		m_BrightnessMaterial.SetFloat("_ExposureOffset", m_Logic.m_ExposureOffset);
		m_BrightnessMaterial.SetFloat("_BrightnessMultiplier", m_Logic.m_BrightnessMultiplier);
		m_BrightnessMaterial.SetTexture("_BrightnessTex", (Texture)(object)m_Logic.m_BrightnessRT);
		m_BrightnessMaterial.SetTexture("_ColorTex", (Texture)(object)source);
		if (RenderDebugInfos)
		{
			m_BrightnessMaterial.SetFloat("_VisualizeExposure", m_ShowExposure ? 1f : 0f);
			m_BrightnessMaterial.SetFloat("_VisualizeHistogram", m_ShowHistogram ? 1f : 0f);
			m_BrightnessMaterial.SetVector("_MinMaxSpeedDt", m_Logic.MinMaxSpeedDT);
			m_BrightnessMaterial.SetTexture("_Histogram", (Texture)(object)m_Logic.m_HistogramList[0]);
			m_BrightnessMaterial.SetTexture("_ColorTex", (Texture)(object)source);
			m_BrightnessMaterial.SetFloat("_LuminanceRange", m_Logic.m_Range);
			m_BrightnessMaterial.SetVector("_HistogramCoefs", m_Logic.m_HistCoefs);
			m_BrightnessMaterial.SetVector("_MinMax", new Vector4(0.01f, m_HistogramSize * 0.75f, 0.01f, m_HistogramSize * 0.5f));
			m_BrightnessMaterial.SetFloat("_TotalPixelNumber", (float)(m_Logic.m_CurrentWidth * m_Logic.m_CurrentHeight));
			Graphics.Blit((Texture)(object)source, destination, m_BrightnessMaterial, 2);
		}
		else
		{
			Graphics.Blit((Texture)(object)source, destination, m_BrightnessMaterial, 0);
		}
	}
}
