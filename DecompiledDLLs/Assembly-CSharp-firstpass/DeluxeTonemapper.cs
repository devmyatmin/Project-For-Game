using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DeluxeTonemapper : MonoBehaviour
{
	public enum Mode
	{
		Color,
		Luminance,
		ExtendedLuminance,
		ColorHD
	}

	[SerializeField]
	public FilmicCurve m_MainCurve = new FilmicCurve();

	[SerializeField]
	public Color m_Tint = new Color(1f, 1f, 1f, 1f);

	[SerializeField]
	public float m_MoodAmount;

	[SerializeField]
	public float m_Sharpness;

	[SerializeField]
	public float m_SharpnessKernel = 0.5f;

	[SerializeField]
	public Mode m_Mode = Mode.ColorHD;

	public bool m_EnableLut0;

	public float m_Saturation = 1f;

	public bool m_DisableColorGrading;

	private bool m_LastDisableColorGrading;

	public float m_Lut0Amount = 1f;

	public Texture2D m_2DLut0;

	public Texture3D m_3DLut0;

	private FTDLut m_LutTool = new FTDLut();

	public bool m_FilmicCurveOpened = true;

	public bool m_ColorGradingOpened;

	public bool m_HDREffectsgOpened;

	public bool m_BannerIsOpened = true;

	public bool m_Use2DLut;

	private bool m_LastUse2DLut;

	[SerializeField]
	private bool m_IsInitialized;

	private Mode m_LastMode;

	private Material m_Material;

	private Shader m_Shader;

	public FTDLut LutTool => m_LutTool;

	public Shader TonemappingShader => m_Shader;

	private void DestroyMaterial(Material mat)
	{
		if (Object.op_Implicit((Object)(object)mat))
		{
			Object.DestroyImmediate((Object)(object)mat);
			mat = null;
		}
	}

	private void CreateMaterials()
	{
		if ((Object)(object)m_Shader == (Object)null)
		{
			if (m_Mode == Mode.Color)
			{
				m_Shader = Shader.Find("Hidden/Deluxe/TonemapperColor");
			}
			if (m_Mode == Mode.Luminance)
			{
				m_Shader = Shader.Find("Hidden/Deluxe/TonemapperLuminosity");
			}
			if (m_Mode == Mode.ExtendedLuminance)
			{
				m_Shader = Shader.Find("Hidden/Deluxe/TonemapperLuminosityExtended");
			}
			if (m_Mode == Mode.ColorHD)
			{
				m_Shader = Shader.Find("Hidden/Deluxe/TonemapperColorHD");
			}
		}
		if ((Object)(object)m_Material == (Object)null && (Object)(object)m_Shader != (Object)null && m_Shader.isSupported)
		{
			m_Material = CreateMaterial(m_Shader);
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

	private void OnDisable()
	{
		DestroyMaterial(m_Material);
		m_Material = null;
		m_Shader = null;
	}

	public void ClearLutTextures()
	{
		if ((Object)(object)m_3DLut0 != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)m_3DLut0);
			m_3DLut0 = null;
		}
		m_2DLut0 = null;
	}

	public void ConvertAndApply2dLut()
	{
		if ((Object)(object)m_2DLut0 != (Object)null)
		{
			m_3DLut0 = LutTool.Allocate3DLut(m_2DLut0);
			LutTool.ConvertLUT(m_2DLut0, m_3DLut0);
		}
	}

	public void SetLut0(Texture2D lut0)
	{
		ClearLutTextures();
		m_2DLut0 = lut0;
		ConvertAndApply2dLut();
	}

	public void VerifyAndUpdateShaderVariations(bool forceUpdate)
	{
		if ((Object)(object)m_Material == (Object)null)
		{
			return;
		}
		if (!SystemInfo.supports3DTextures)
		{
			m_Use2DLut = true;
		}
		if ((m_DisableColorGrading != m_LastDisableColorGrading) | forceUpdate)
		{
			m_Material.DisableKeyword("FTD_DISABLE_CGRADING");
			if (m_DisableColorGrading)
			{
				m_Material.EnableKeyword("FTD_DISABLE_CGRADING");
			}
		}
		if ((m_Use2DLut != m_LastUse2DLut) | forceUpdate)
		{
			m_Material.DisableKeyword("FTD_2D_LUT");
			if (m_Use2DLut)
			{
				m_Material.EnableKeyword("FTD_2D_LUT");
			}
		}
		m_LastUse2DLut = m_Use2DLut;
		m_LastDisableColorGrading = m_DisableColorGrading;
	}

	public void UpdateCoefficients()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Invalid comparison between Unknown and I4
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		if (m_Mode == Mode.ColorHD)
		{
			m_MainCurve.m_UseLegacyCurve = false;
		}
		else
		{
			m_MainCurve.m_UseLegacyCurve = true;
		}
		m_MainCurve.UpdateCoefficients();
		if (!((Object)(object)m_Material == (Object)null))
		{
			if ((Object)(object)m_2DLut0 == (Object)null)
			{
				ClearLutTextures();
			}
			m_Material.SetFloat("_K", m_MainCurve.m_k);
			m_Material.SetFloat("_Crossover", m_MainCurve.m_CrossOverPoint);
			m_Material.SetVector("_Toe", m_MainCurve.m_ToeCoef);
			m_Material.SetVector("_Shoulder", m_MainCurve.m_ShoulderCoef);
			m_Material.SetFloat("_LuminosityWhite", m_MainCurve.m_LuminositySaturationPoint * m_MainCurve.m_LuminositySaturationPoint);
			m_Material.SetFloat("_Sharpness", m_Sharpness);
			m_Material.SetFloat("_Saturation", m_Saturation);
			if ((Object)(object)m_2DLut0 != (Object)null && (Object)(object)m_3DLut0 == (Object)null && LutTool.Validate2DLut(m_2DLut0))
			{
				ConvertAndApply2dLut();
			}
			float num = (((int)QualitySettings.activeColorSpace == 1) ? 0.5f : 1f);
			if (m_Use2DLut && (Object)(object)m_2DLut0 != (Object)null)
			{
				((Texture)m_2DLut0).filterMode = (FilterMode)1;
				float num2 = Mathf.Sqrt((float)((Texture)m_2DLut0).width);
				m_Material.SetTexture("_2dLut0", (Texture)(object)m_2DLut0);
				m_Material.SetVector("_Lut0Params", new Vector4(m_Lut0Amount, 1f / (float)((Texture)m_2DLut0).width, 1f / (float)((Texture)m_2DLut0).height, num2 - 1f));
				m_Material.SetFloat("_IsLinear", num);
			}
			else if ((Object)(object)m_3DLut0 != (Object)null)
			{
				int width = ((Texture)m_3DLut0).width;
				float num3 = (float)(width - 1) / (1f * (float)width);
				float num4 = 1f / (2f * (float)width);
				((Texture)m_3DLut0).filterMode = (FilterMode)2;
				m_Material.SetVector("_Lut0Params", new Vector4(m_Lut0Amount, num3, num4, 0f));
				m_Material.SetTexture("_Lut0", (Texture)(object)m_3DLut0);
				m_Material.SetFloat("_IsLinear", num);
			}
			else
			{
				m_Material.SetVector("_Lut0Params", Vector4.zero);
			}
		}
	}

	public void ReloadShaders()
	{
		OnDisable();
	}

	public void SetCurveParameters(float range, float crossover, float toe, float shoulder)
	{
		SetCurveParameters(0f, range, crossover, toe, shoulder);
	}

	public void SetCurveParameters(float crossover, float toe, float shoulder)
	{
		SetCurveParameters(0f, 1f, crossover, toe, shoulder);
	}

	public void SetCurveParameters(float blackPoint, float whitePoint, float crossover, float toe, float shoulder)
	{
		m_MainCurve.m_BlackPoint = blackPoint;
		m_MainCurve.m_WhitePoint = whitePoint;
		m_MainCurve.m_ToeStrength = -1f * toe;
		m_MainCurve.m_ShoulderStrength = shoulder;
		m_MainCurve.m_CrossOverPoint = crossover * (whitePoint - blackPoint) + blackPoint;
		UpdateCoefficients();
	}

	private void OnEnable()
	{
		if (m_MainCurve == null)
		{
			m_MainCurve = new FilmicCurve();
		}
		CreateMaterials();
		UpdateCoefficients();
		VerifyAndUpdateShaderVariations(forceUpdate: true);
	}

	private void Initialize()
	{
	}

	[ImageEffectTransformsToLDR]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		Initialize();
		if (m_Mode != m_LastMode)
		{
			ReloadShaders();
			CreateMaterials();
			VerifyAndUpdateShaderVariations(forceUpdate: true);
		}
		m_LastMode = m_Mode;
		UpdateCoefficients();
		VerifyAndUpdateShaderVariations(forceUpdate: false);
		m_Material.SetVector("_Tint", Color.op_Implicit(m_Tint));
		m_Material.SetVector("_Offsets", new Vector4(m_SharpnessKernel / (float)((Texture)source).width, m_SharpnessKernel / (float)((Texture)source).height));
		if (m_EnableLut0)
		{
			Graphics.Blit((Texture)(object)source, destination, m_Material, 1);
		}
		else
		{
			Graphics.Blit((Texture)(object)source, destination, m_Material, 0);
		}
	}

	public DeluxeTonemapper()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
	}
}
