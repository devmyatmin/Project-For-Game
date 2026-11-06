using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace AmplifyImpostors;

public class AmplifyImpostor : MonoBehaviour
{
	private const string ShaderGUID = "e82933f4c0eb9ba42aab0739f48efe21";

	private const string DilateGUID = "57c23892d43bc9f458360024c5985405";

	private const string PackerGUID = "31bd3cd74692f384a916d9d7ea87710d";

	private const string ShaderOctaGUID = "572f9be5706148142b8da6e9de53acdb";

	private const string StandardPreset = "e4786beb7716da54dbb02a632681cc37";

	private const string LWPreset = "089f3a2f6b5f48348a48c755f8d9a7a2";

	private const string LWShaderOctaGUID = "94e2ddcdfb3257a43872042f97e2fb01";

	private const string LWShaderGUID = "990451a2073f6994ebf9fd6f90a842b3";

	private const string HDPreset = "47b6b3dcefe0eaf4997acf89caf8c75e";

	private const string HDShaderOctaGUID = "56236dc63ad9b7949b63a27f0ad180b3";

	private const string HDShaderGUID = "175c951fec709c44fa2f26b8ab78b8dd";

	private const string UPreset = "0403878495ffa3c4e9d4bcb3eac9b559";

	private const string UShaderOctaGUID = "83dd8de9a5c14874884f9012def4fdcc";

	private const string UShaderGUID = "da79d698f4bf0164e910ad798d07efdf";

	[SerializeField]
	private AmplifyImpostorAsset m_data;

	[SerializeField]
	private Transform m_rootTransform;

	[SerializeField]
	private LODGroup m_lodGroup;

	[SerializeField]
	private Renderer[] m_renderers;

	public LODReplacement m_lodReplacement = LODReplacement.ReplaceLast;

	[SerializeField]
	public RenderPipelineInUse m_renderPipelineInUse;

	public int m_insertIndex = 1;

	[SerializeField]
	public GameObject m_lastImpostor;

	[SerializeField]
	public string m_folderPath;

	[NonSerialized]
	public string m_impostorName = string.Empty;

	[SerializeField]
	public CutMode m_cutMode;

	[NonSerialized]
	private const float StartXRotation = -90f;

	[NonSerialized]
	private const float StartYRotation = 90f;

	[NonSerialized]
	private const int MinAlphaResolution = 256;

	[NonSerialized]
	private RenderTexture[] m_rtGBuffers;

	[NonSerialized]
	private RenderTexture[] m_alphaGBuffers;

	[NonSerialized]
	private RenderTexture m_trueDepth;

	[NonSerialized]
	public Texture2D m_alphaTex;

	[NonSerialized]
	private float m_xyFitSize;

	[NonSerialized]
	private float m_depthFitSize;

	[NonSerialized]
	private Vector2 m_pixelOffset = Vector2.zero;

	[NonSerialized]
	private Bounds m_originalBound;

	[NonSerialized]
	private Vector3 m_oriPos = Vector3.zero;

	[NonSerialized]
	private Quaternion m_oriRot = Quaternion.identity;

	[NonSerialized]
	private Vector3 m_oriSca = Vector3.one;

	[NonSerialized]
	private const int BlockSize = 65536;

	public AmplifyImpostorAsset Data
	{
		get
		{
			return m_data;
		}
		set
		{
			m_data = value;
		}
	}

	public Transform RootTransform
	{
		get
		{
			return m_rootTransform;
		}
		set
		{
			m_rootTransform = value;
		}
	}

	public LODGroup LodGroup
	{
		get
		{
			return m_lodGroup;
		}
		set
		{
			m_lodGroup = value;
		}
	}

	public Renderer[] Renderers
	{
		get
		{
			return m_renderers;
		}
		set
		{
			m_renderers = value;
		}
	}

	private void GenerateTextures(List<TextureOutput> outputList, bool standardRendering)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected Obj, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected Obj, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected Obj, but got Unknown
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected Obj, but got Unknown
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Expected Obj, but got Unknown
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Expected Obj, but got Unknown
		m_rtGBuffers = new RenderTexture[outputList.Count];
		if (standardRendering && m_renderPipelineInUse == RenderPipelineInUse.HD)
		{
			GraphicsFormat val = (GraphicsFormat)4;
			GraphicsFormat val2 = (GraphicsFormat)8;
			m_rtGBuffers[0] = new RenderTexture((int)m_data.TexSize.x, (int)m_data.TexSize.y, 16, val);
			m_rtGBuffers[0].Create();
			m_rtGBuffers[1] = new RenderTexture((int)m_data.TexSize.x, (int)m_data.TexSize.y, 16, val2);
			m_rtGBuffers[1].Create();
			m_rtGBuffers[2] = new RenderTexture((int)m_data.TexSize.x, (int)m_data.TexSize.y, 16, val2);
			m_rtGBuffers[2].Create();
			m_rtGBuffers[3] = new RenderTexture((int)m_data.TexSize.x, (int)m_data.TexSize.y, 16, val2);
			m_rtGBuffers[3].Create();
		}
		else
		{
			for (int i = 0; i < m_rtGBuffers.Length; i++)
			{
				m_rtGBuffers[i] = new RenderTexture((int)m_data.TexSize.x, (int)m_data.TexSize.y, 16, (RenderTextureFormat)((!outputList[i].SRGB) ? 2 : 0));
				m_rtGBuffers[i].Create();
			}
		}
		m_trueDepth = new RenderTexture((int)m_data.TexSize.x, (int)m_data.TexSize.y, 16, (RenderTextureFormat)1);
		m_trueDepth.Create();
	}

	private void GenerateAlphaTextures(List<TextureOutput> outputList)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected Obj, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		m_alphaGBuffers = new RenderTexture[outputList.Count];
		for (int i = 0; i < m_alphaGBuffers.Length; i++)
		{
			m_alphaGBuffers[i] = new RenderTexture(256, 256, 16, (RenderTextureFormat)((!outputList[i].SRGB) ? 2 : 0));
			m_alphaGBuffers[i].Create();
		}
		m_trueDepth = new RenderTexture(256, 256, 16, (RenderTextureFormat)1);
		m_trueDepth.Create();
	}

	private void ClearBuffers()
	{
		RenderTexture.active = null;
		RenderTexture[] rtGBuffers = m_rtGBuffers;
		for (int i = 0; i < rtGBuffers.Length; i++)
		{
			rtGBuffers[i].Release();
		}
		m_rtGBuffers = null;
	}

	private void ClearAlphaBuffers()
	{
		RenderTexture.active = null;
		RenderTexture[] alphaGBuffers = m_alphaGBuffers;
		for (int i = 0; i < alphaGBuffers.Length; i++)
		{
			alphaGBuffers[i].Release();
		}
		m_alphaGBuffers = null;
	}

	public void RenderImpostor(ImpostorType impostorType, int targetAmount, bool impostorMaps = true, bool combinedAlphas = false, bool useMinResolution = false, Shader customShader = null)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected Obj, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected Obj, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Invalid comparison between Unknown and I4
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_04af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04da: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_050b: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_045e: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_060e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0613: Unknown result type (might be due to invalid IL or missing references)
		//IL_061d: Expected Obj, but got Unknown
		//IL_067b: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a3: Unknown result type (might be due to invalid IL or missing references)
		if ((!impostorMaps && !combinedAlphas) || targetAmount <= 0)
		{
			return;
		}
		bool flag = (Object)(object)customShader == (Object)null;
		Dictionary<Material, Material> dictionary = new Dictionary<Material, Material>();
		CommandBuffer val = new CommandBuffer();
		if (impostorMaps)
		{
			val.name = "GBufferCatcher";
			RenderTargetIdentifier[] array = new RenderTargetIdentifier[targetAmount];
			for (int i = 0; i < targetAmount; i++)
			{
				array[i] = RenderTargetIdentifier.op_Implicit((Texture)(object)m_rtGBuffers[i]);
			}
			val.SetRenderTarget(array, RenderTargetIdentifier.op_Implicit((Texture)(object)m_trueDepth));
			val.ClearRenderTarget(true, true, Color.clear, 1f);
		}
		CommandBuffer val2 = new CommandBuffer();
		if (combinedAlphas)
		{
			val2.name = "DepthAlphaCatcher";
			RenderTargetIdentifier[] array2 = new RenderTargetIdentifier[targetAmount];
			for (int j = 0; j < targetAmount; j++)
			{
				array2[j] = RenderTargetIdentifier.op_Implicit((Texture)(object)m_alphaGBuffers[j]);
			}
			val2.SetRenderTarget(array2, RenderTargetIdentifier.op_Implicit((Texture)(object)m_trueDepth));
			val2.ClearRenderTarget(true, true, Color.clear, 1f);
		}
		int horizontalFrames = m_data.HorizontalFrames;
		int num = m_data.HorizontalFrames;
		if (impostorType == ImpostorType.Spherical)
		{
			num = m_data.HorizontalFrames - 1;
			if (m_data.DecoupleAxisFrames)
			{
				num = m_data.VerticalFrames - 1;
			}
		}
		List<MeshFilter> list = new List<MeshFilter>();
		for (int k = 0; k < Renderers.Length; k++)
		{
			if ((Object)(object)Renderers[k] == (Object)null || !Renderers[k].enabled || (int)Renderers[k].shadowCastingMode == 3)
			{
				list.Add(null);
				continue;
			}
			MeshFilter component = ((Component)Renderers[k]).GetComponent<MeshFilter>();
			if ((Object)(object)component == (Object)null || (Object)(object)component.sharedMesh == (Object)null)
			{
				list.Add(null);
			}
			else
			{
				list.Add(component);
			}
		}
		int count = list.Count;
		for (int l = 0; l < horizontalFrames; l++)
		{
			for (int m = 0; m <= num; m++)
			{
				Bounds val3 = default;
				Matrix4x4 cameraRotationMatrix = GetCameraRotationMatrix(impostorType, horizontalFrames, num, l, m);
				for (int n = 0; n < count; n++)
				{
					if (!((Object)(object)list[n] == (Object)null))
					{
						if (val3.size == Vector3.zero)
						{
							val3 = BoundsEx.Transform(list[n].sharedMesh.bounds, m_rootTransform.worldToLocalMatrix * Renderers[n].localToWorldMatrix);
						}
						else
						{
							val3.Encapsulate(BoundsEx.Transform(list[n].sharedMesh.bounds, m_rootTransform.worldToLocalMatrix * Renderers[n].localToWorldMatrix));
						}
					}
				}
				if (l == 0 && m == 0)
				{
					m_originalBound = val3;
				}
				val3 = BoundsEx.Transform(val3, cameraRotationMatrix);
				Matrix4x4 val4 = cameraRotationMatrix.inverse * Matrix4x4.LookAt(val3.center - new Vector3(0f, 0f, m_depthFitSize * 0.5f), val3.center, Vector3.up);
				float num2 = m_xyFitSize * 0.5f;
				Matrix4x4 val5 = Matrix4x4.Ortho(0f - num2 + m_pixelOffset.x, num2 + m_pixelOffset.x, 0f - num2 + m_pixelOffset.y, num2 + m_pixelOffset.y, 0f, 0f - m_depthFitSize);
				val4 = val4.inverse * m_rootTransform.worldToLocalMatrix;
				if (flag && m_renderPipelineInUse == RenderPipelineInUse.HD)
				{
					val5 = GL.GetGPUProjectionMatrix(val5, true);
				}
				if (impostorMaps)
				{
					val.SetViewProjectionMatrices(val4, val5);
					val.SetViewport(new Rect(m_data.TexSize.x / (float)horizontalFrames * (float)l, m_data.TexSize.y / (float)(num + ((impostorType == ImpostorType.Spherical) ? 1 : 0)) * (float)m, m_data.TexSize.x / (float)m_data.HorizontalFrames, m_data.TexSize.y / (float)m_data.VerticalFrames));
					if (flag && m_renderPipelineInUse == RenderPipelineInUse.HD)
					{
						val.SetGlobalMatrix("_ViewMatrix", val4);
						val.SetGlobalMatrix("_InvViewMatrix", val4.inverse);
						val.SetGlobalMatrix("_ProjMatrix", val5);
						val.SetGlobalMatrix("_ViewProjMatrix", val5 * val4);
						val.SetGlobalVector("_WorldSpaceCameraPos", Vector4.zero);
					}
				}
				if (combinedAlphas)
				{
					val2.SetViewProjectionMatrices(val4, val5);
					val2.SetViewport(new Rect(0f, 0f, 256f, 256f));
					if (flag && m_renderPipelineInUse == RenderPipelineInUse.HD)
					{
						val2.SetGlobalMatrix("_ViewMatrix", val4);
						val2.SetGlobalMatrix("_InvViewMatrix", val4.inverse);
						val2.SetGlobalMatrix("_ProjMatrix", val5);
						val2.SetGlobalMatrix("_ViewProjMatrix", val5 * val4);
						val2.SetGlobalVector("_WorldSpaceCameraPos", Vector4.zero);
					}
				}
				for (int num3 = 0; num3 < count; num3++)
				{
					if ((Object)(object)list[num3] == (Object)null)
					{
						continue;
					}
					Material[] sharedMaterials = Renderers[num3].sharedMaterials;
					for (int num4 = 0; num4 < sharedMaterials.Length; num4++)
					{
						Material value = null;
						_ = list[num3].sharedMesh;
						int num5 = 0;
						int num6 = 0;
						if (flag)
						{
							value = sharedMaterials[num4];
							num5 = value.FindPass("DEFERRED");
							if (num5 == -1)
							{
								num5 = value.FindPass("Deferred");
							}
							if (num5 == -1)
							{
								num5 = value.FindPass("GBuffer");
							}
							num6 = value.FindPass("DepthOnly");
							if (num5 == -1)
							{
								num5 = 0;
								for (int num7 = 0; num7 < value.passCount; num7++)
								{
									if (value.GetTag("LightMode", true).Equals("Deferred"))
									{
										num5 = num7;
										break;
									}
								}
							}
							val.EnableShaderKeyword("UNITY_HDR_ON");
						}
						else
						{
							num6 = -1;
							if (!dictionary.TryGetValue(sharedMaterials[num4], out value))
							{
								value = new Material(customShader)
								{
									hideFlags = (HideFlags)61
								};
								dictionary.Add(sharedMaterials[num4], value);
							}
						}
						bool flag2 = Renderers[num3].lightmapIndex > -1;
						bool flag3 = Renderers[num3].realtimeLightmapIndex > -1;
						if ((flag2 | flag3) && !flag)
						{
							val.EnableShaderKeyword("LIGHTMAP_ON");
							if (flag2)
							{
								val.SetGlobalVector("unity_LightmapST", Renderers[num3].lightmapScaleOffset);
							}
							if (flag3)
							{
								val.EnableShaderKeyword("DYNAMICLIGHTMAP_ON");
								val.SetGlobalVector("unity_DynamicLightmapST", Renderers[num3].realtimeLightmapScaleOffset);
							}
							else
							{
								val.DisableShaderKeyword("DYNAMICLIGHTMAP_ON");
							}
							if (flag2 & flag3)
							{
								val.EnableShaderKeyword("DIRLIGHTMAP_COMBINED");
							}
							else
							{
								val.DisableShaderKeyword("DIRLIGHTMAP_COMBINED");
							}
						}
						else
						{
							val.DisableShaderKeyword("LIGHTMAP_ON");
							val.DisableShaderKeyword("DYNAMICLIGHTMAP_ON");
							val.DisableShaderKeyword("DIRLIGHTMAP_COMBINED");
						}
						val.DisableShaderKeyword("LIGHTPROBE_SH");
						if (impostorMaps)
						{
							if (num6 > -1)
							{
								val.DrawRenderer(Renderers[num3], value, num4, num6);
							}
							val.DrawRenderer(Renderers[num3], value, num4, num5);
						}
						if (combinedAlphas)
						{
							if (num6 > -1)
							{
								val2.DrawRenderer(Renderers[num3], value, num4, num6);
							}
							val2.DrawRenderer(Renderers[num3], value, num4, num5);
						}
					}
				}
				if (impostorMaps)
				{
					Graphics.ExecuteCommandBuffer(val);
				}
				if (combinedAlphas)
				{
					Graphics.ExecuteCommandBuffer(val2);
				}
			}
		}
		list.Clear();
		foreach (KeyValuePair<Material, Material> item in dictionary)
		{
			Material value2 = item.Value;
			if ((Object)(object)value2 != (Object)null)
			{
				if (!Application.isPlaying)
				{
					Object.DestroyImmediate((Object)(object)value2);
				}
				value2 = null;
			}
		}
		dictionary.Clear();
		val.Release();
		val = null;
		val2.Release();
		val2 = null;
	}

	private Matrix4x4 GetCameraRotationMatrix(ImpostorType impostorType, int hframes, int vframes, int x, int y)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 result = Matrix4x4.identity;
		Matrix4x4 val2;
		switch (impostorType)
		{
		case ImpostorType.Spherical:
		{
			float num = 0f;
			if (vframes > 0)
			{
				num = 0f - 180f / (float)vframes;
			}
			Quaternion val4 = Quaternion.Euler(num * (float)y + 90f, 0f, 0f);
			Quaternion val5 = Quaternion.Euler(0f, 360f / (float)hframes * (float)x + -90f, 0f);
			result = Matrix4x4.Rotate(val4 * val5);
			break;
		}
		case ImpostorType.Octahedron:
		{
			Vector3 val3 = OctahedronToVector((float)x / ((float)hframes - 1f) * 2f - 1f, (float)y / ((float)vframes - 1f) * 2f - 1f);
			val2 = Matrix4x4.Rotate(Quaternion.LookRotation(new Vector3(val3.x * -1f, val3.z * -1f, val3.y * -1f), Vector3.up));
			result = val2.inverse;
			break;
		}
		case ImpostorType.HemiOctahedron:
		{
			Vector3 val = HemiOctahedronToVector((float)x / ((float)hframes - 1f) * 2f - 1f, (float)y / ((float)vframes - 1f) * 2f - 1f);
			val2 = Matrix4x4.Rotate(Quaternion.LookRotation(new Vector3(val.x * -1f, val.z * -1f, val.y * -1f), Vector3.up));
			result = val2.inverse;
			break;
		}
		}
		return result;
	}

	private Vector3 OctahedronToVector(Vector2 oct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = new Vector3(oct.x, oct.y, 1f - Mathf.Abs(oct.x) - Mathf.Abs(oct.y));
		float num = Mathf.Clamp01(0f - val.z);
		val.Set(val.x + ((val.x >= 0f) ? (0f - num) : num), val.y + ((val.y >= 0f) ? (0f - num) : num), val.z);
		val = Vector3.Normalize(val);
		return val;
	}

	private Vector3 OctahedronToVector(float x, float y)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = new Vector3(x, y, 1f - Mathf.Abs(x) - Mathf.Abs(y));
		float num = Mathf.Clamp01(0f - val.z);
		val.Set(val.x + ((val.x >= 0f) ? (0f - num) : num), val.y + ((val.y >= 0f) ? (0f - num) : num), val.z);
		val = Vector3.Normalize(val);
		return val;
	}

	private Vector3 HemiOctahedronToVector(float x, float y)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		float num = x;
		float num2 = y;
		x = (num + num2) * 0.5f;
		y = (num - num2) * 0.5f;
		return Vector3.Normalize(new Vector3(x, y, 1f - Mathf.Abs(x) - Mathf.Abs(y)));
	}

	public void GenerateAutomaticMesh(AmplifyImpostorAsset data)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		SpriteUtilityEx.GenerateOutline(rect: new Rect(0f, 0f, (float)((Texture)m_alphaTex).width, (float)((Texture)m_alphaTex).height), texture: m_alphaTex, detail: data.Tolerance, alphaTolerance: 254, holeDetection: false, paths: out var paths);
		int num = 0;
		for (int i = 0; i < paths.Length; i++)
		{
			num += paths[i].Length;
		}
		data.ShapePoints = new Vector2[num];
		int num2 = 0;
		for (int j = 0; j < paths.Length; j++)
		{
			for (int k = 0; k < paths[j].Length; k++)
			{
				data.ShapePoints[num2] = paths[j][k] + new Vector2((float)((Texture)m_alphaTex).width * 0.5f, (float)((Texture)m_alphaTex).height * 0.5f);
				data.ShapePoints[num2] = Vector2.Scale(data.ShapePoints[num2], new Vector2(1f / (float)((Texture)m_alphaTex).width, 1f / (float)((Texture)m_alphaTex).height));
				num2++;
			}
		}
		data.ShapePoints = Vector2Ex.ConvexHull(data.ShapePoints);
		data.ShapePoints = Vector2Ex.ReduceVertices(data.ShapePoints, data.MaxVertices);
		data.ShapePoints = Vector2Ex.ScaleAlongNormals(data.ShapePoints, data.NormalScale);
		for (int l = 0; l < data.ShapePoints.Length; l++)
		{
			data.ShapePoints[l].x = Mathf.Clamp01(data.ShapePoints[l].x);
			data.ShapePoints[l].y = Mathf.Clamp01(data.ShapePoints[l].y);
		}
		data.ShapePoints = Vector2Ex.ConvexHull(data.ShapePoints);
		for (int m = 0; m < data.ShapePoints.Length; m++)
		{
			data.ShapePoints[m] = new Vector2(data.ShapePoints[m].x, 1f - data.ShapePoints[m].y);
		}
	}

	public Mesh GenerateMesh(Vector2[] points, Vector3 offset, float width = 1f, float height = 1f, bool invertY = true)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Expected Obj, but got Unknown
		Vector2[] array = new Vector2[points.Length];
		Vector2[] array2 = new Vector2[points.Length];
		Array.Copy(points, array, points.Length);
		float num = width * 0.5f;
		float num2 = height * 0.5f;
		if (invertY)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new Vector2(array[i].x, 1f - array[i].y);
			}
		}
		Array.Copy(array, array2, array.Length);
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = new Vector2(array[j].x * width - num + m_pixelOffset.x, array[j].y * height - num2 + m_pixelOffset.y);
		}
		Triangulator triangulator = new Triangulator(array);
		int[] triangles = triangulator.Triangulate();
		Vector3[] array3 = new Vector3[triangulator.Points.Count];
		for (int k = 0; k < array3.Length; k++)
		{
			array3[k] = new Vector3(triangulator.Points[k].x, triangulator.Points[k].y, 0f);
		}
		Mesh val = new Mesh
		{
			vertices = array3,
			uv = array2,
			triangles = triangles
		};
		val.RecalculateNormals();
		val.bounds = new Bounds(offset, m_originalBound.size);
		return val;
	}

	public AmplifyImpostor()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
	}
}
