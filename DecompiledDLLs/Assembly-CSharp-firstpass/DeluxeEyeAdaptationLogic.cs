using System;
using UnityEngine;

[Serializable]
public class DeluxeEyeAdaptationLogic
{
	[SerializeField]
	public float m_MinimumExposure = 0.4f;

	[SerializeField]
	public float m_MaximumExposure = 4f;

	[SerializeField]
	public float m_Range = 4f;

	[SerializeField]
	public float m_AdaptationSpeedUp = 1f;

	[SerializeField]
	public float m_AdaptationSpeedDown = 1f;

	[SerializeField]
	public float m_BrightnessMultiplier = 1f;

	[SerializeField]
	public float m_ExposureOffset;

	[SerializeField]
	public float m_AverageThresholdMin = 0.8f;

	[SerializeField]
	public float m_AverageThresholdMax = 0.98f;

	private const float m_HistMin = 0.005f;

	[SerializeField]
	private float m_HistLogMin = Mathf.Log(0.005f, 2f);

	[SerializeField]
	private float m_HistLogMax = Mathf.Log(5f, 2f);

	[SerializeField]
	public Vector4 m_HistCoefs;

	[SerializeField]
	public float m_Attenuation = 1f;

	private const int m_BinCount = 64;

	private const float m_BinTexStep = 1f / 32f;

	private const float m_BinTexStart = -63f / 64f;

	private Mesh[] m_Meshes;

	public int m_CurrentWidth;

	public int m_CurrentHeight;

	private const int HIST_COUNT = 1;

	private int m_CurrentHistogram;

	public RenderTexture[] m_HistogramList = new RenderTexture[1];

	public RenderTexture m_BrightnessRT;

	private RenderTexture m_PreviousBrightnessRT;

	private Texture2D m_LocalHistogram;

	private Texture2D m_LocalBrightness;

	public bool m_SetTargetExposure;

	private const int m_Height = 1;

	public Vector4 MinMaxSpeedDT
	{
		get
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			return new Vector4(1f / m_MaximumExposure, 1f / m_MinimumExposure, m_AdaptationSpeedDown * Time.deltaTime, m_AdaptationSpeedUp * Time.deltaTime);
		}
	}

	private void RenderHistogram(Material histogramMaterial)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		ComputeHistogramCoefs();
		RenderTexture.active = m_HistogramList[m_CurrentHistogram];
		GL.Clear(true, true, Color.black);
		histogramMaterial.SetFloat("_ValueRange", m_Range);
		histogramMaterial.SetFloat("_StepSize", 1f / 32f);
		histogramMaterial.SetFloat("_BinCount", 64f);
		histogramMaterial.SetVector("_HistogramCoefs", m_HistCoefs);
		if (histogramMaterial.SetPass(0))
		{
			for (int i = 0; i < m_Meshes.Length; i++)
			{
				Graphics.DrawMeshNow(m_Meshes[i], Matrix4x4.identity);
			}
		}
		RenderTexture val = m_HistogramList[0];
		float num = m_CurrentWidth * m_CurrentHeight;
		histogramMaterial.SetFloat("_90PixelCount", num * m_AverageThresholdMin);
		histogramMaterial.SetFloat("_98PixelCount", num * m_AverageThresholdMax);
		histogramMaterial.SetVector("_Coefs", m_HistCoefs);
		histogramMaterial.SetTexture("_HistogramTex", (Texture)(object)val);
		histogramMaterial.SetTexture("_PreviousBrightness", (Texture)(object)m_PreviousBrightnessRT);
		histogramMaterial.SetFloat("_ExposureOffset", -1f * m_ExposureOffset);
		histogramMaterial.SetFloat("_Attenuation", m_Attenuation);
		_ = Time.deltaTime;
		histogramMaterial.SetVector("_MinMaxSpeedDt", MinMaxSpeedDT);
		((Texture)val).filterMode = (FilterMode)0;
		RenderTexture.active = m_BrightnessRT;
		GL.Clear(true, true, Color.black);
		Graphics.Blit((Texture)(object)val, m_BrightnessRT, histogramMaterial, 1);
		RenderTexture brightnessRT = m_BrightnessRT;
		m_BrightnessRT = m_PreviousBrightnessRT;
		m_PreviousBrightnessRT = brightnessRT;
	}

	private void ComputeHistogramCoefs()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		m_HistLogMax = Mathf.Log(m_Range, 2f);
		float num = m_HistLogMax - m_HistLogMin;
		float num2 = 1f / num;
		float num3 = 0f - num2 * m_HistLogMin;
		m_HistCoefs = new Vector4(num2, num3, 0.005f, m_Range);
	}

	public void ComputeExposure(int screenWidth, int screenHeight, Material histogramMaterial)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected Obj, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected Obj, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected Obj, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 1; i++)
		{
			if ((Object)(object)m_HistogramList[i] == (Object)null)
			{
				m_HistogramList[i] = new RenderTexture(64, 1, 0, (RenderTextureFormat)11);
				((Object)m_HistogramList[i]).hideFlags = (HideFlags)61;
			}
		}
		if ((Object)(object)m_BrightnessRT == (Object)null)
		{
			m_BrightnessRT = new RenderTexture(1, 1, 0, (RenderTextureFormat)11);
			((Object)m_BrightnessRT).hideFlags = (HideFlags)61;
			RenderTexture.active = m_BrightnessRT;
			GL.Clear(false, true, Color.white);
		}
		if ((Object)(object)m_PreviousBrightnessRT == (Object)null)
		{
			m_PreviousBrightnessRT = new RenderTexture(1, 1, 0, (RenderTextureFormat)11);
			((Object)m_PreviousBrightnessRT).hideFlags = (HideFlags)61;
			RenderTexture.active = m_PreviousBrightnessRT;
			GL.Clear(false, true, Color.white);
		}
		RebuildMeshIfNeeded(screenWidth, screenHeight);
		RenderHistogram(histogramMaterial);
	}

	public void RebuildMeshIfNeeded(int width, int height)
	{
		if (m_CurrentWidth == width && m_CurrentHeight == height && m_Meshes != null)
		{
			return;
		}
		if (m_Meshes != null)
		{
			Mesh[] meshes = m_Meshes;
			for (int i = 0; i < meshes.Length; i++)
			{
				Object.DestroyImmediate((Object)(object)meshes[i], true);
			}
		}
		m_Meshes = null;
		BuildMeshes(width, height);
	}

	public void BuildMeshes(int width, int height)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected Obj, but got Unknown
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		int num = 21666;
		int num2 = width * height;
		int num3 = Mathf.CeilToInt(1f * (float)num2 / (1f * (float)num));
		m_Meshes = new Mesh[num3];
		int num4 = num2;
		m_CurrentWidth = width;
		m_CurrentHeight = height;
		int num5 = 0;
		float num6 = 1f / 32f;
		float num7 = 2f;
		Vector2 val = new Vector2(1f / (float)m_CurrentWidth * 0.5f, 1f / (float)m_CurrentHeight * 0.5f);
		for (int i = 0; i < num3; i++)
		{
			Mesh val2 = new Mesh();
			((Object)val2).hideFlags = (HideFlags)61;
			int num8 = num4;
			if (num4 > num)
			{
				num8 = num;
			}
			num4 -= num8;
			Vector3[] vertices = new Vector3[num8 * 3];
			int[] triangles = new int[num8 * 3];
			Vector2[] array = new Vector2[num8 * 3];
			Vector2[] uv = new Vector2[num8 * 3];
			Vector3[] normals = new Vector3[num8 * 3];
			Color[] colors = new Color[num8 * 3];
			for (int j = 0; j < num8; j++)
			{
				int num9 = num5 % width;
				int num10 = (num5 - num9) / width;
				SetupSprite(j, num9, num10, vertices, triangles, array, uv, normals, colors, new Vector2((float)num9 / (float)width + val.x, 1f - ((float)num10 / (float)height + val.y)), num6 * 0.5f, num7 * 0.5f);
				num5++;
			}
			val2.vertices = vertices;
			val2.triangles = triangles;
			val2.colors = null;
			val2.uv = array;
			val2.uv2 = null;
			val2.normals = normals;
			val2.RecalculateBounds();
			val2.UploadMeshData(true);
			m_Meshes[i] = val2;
		}
	}

	public void SetupSprite(int idx, int x, int y, Vector3[] vertices, int[] triangles, Vector2[] uv0, Vector2[] uv1, Vector3[] normals, Color[] colors, Vector2 targetPixelUV, float halfWidth, float halfHeight)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		int num = idx * 3;
		int num2 = idx * 3;
		triangles[num2] = num;
		triangles[num2 + 1] = num + 2;
		triangles[num2 + 2] = num + 1;
		float num3 = 0f;
		vertices[num] = new Vector3(-1f + num3, -1f, 0f);
		vertices[num + 2] = new Vector3(-1f + num3, 1f, 0f);
		vertices[num + 1] = new Vector3(-61f / 64f + num3, 1f, 0f);
		normals[num] = -Vector3.forward;
		normals[num + 1] = -Vector3.forward;
		normals[num + 2] = -Vector3.forward;
		uv0[num] = targetPixelUV;
		uv0[num + 1] = targetPixelUV;
		uv0[num + 2] = targetPixelUV;
	}

	public void ClearMeshes()
	{
		if (m_Meshes != null)
		{
			Mesh[] meshes = m_Meshes;
			for (int i = 0; i < meshes.Length; i++)
			{
				Object.DestroyImmediate((Object)(object)meshes[i], true);
			}
		}
		m_Meshes = null;
		for (int j = 0; j < 1; j++)
		{
			if ((Object)(object)m_HistogramList[j] == (Object)null)
			{
				Object.DestroyImmediate((Object)(object)m_HistogramList[j]);
				m_HistogramList[j] = null;
			}
		}
		if ((Object)(object)m_BrightnessRT != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)m_BrightnessRT);
			m_BrightnessRT = null;
		}
		if ((Object)(object)m_PreviousBrightnessRT != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)m_PreviousBrightnessRT);
			m_PreviousBrightnessRT = null;
		}
		if ((Object)(object)m_LocalHistogram != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)m_LocalHistogram);
			m_LocalHistogram = null;
		}
		if ((Object)(object)m_LocalBrightness != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)m_LocalBrightness);
			m_LocalBrightness = null;
		}
	}
}
