using System;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[AddComponentMenu("AFS/Setup Advanced Foliage Shader")]
public class SetupAdvancedFoliageShader : MonoBehaviour
{
	public bool isLinear;

	public bool AmbientLightingSH;

	public GameObject TerrianLight0;

	public GameObject TerrianLight1;

	public GameObject DirectionalLightReference;

	private Vector3 DirectlightDir;

	private Light Sunlight;

	private Vector3 SunLightCol;

	private float SunLuminance;

	public bool GrassApproxTrans;

	public int AFSFog_Mode = 2;

	public string AFSShader_Folder = "Assets/Advanced Foliage Shader v4/Shaders/";

	public bool disableFoginShader;

	public Vector4 Wind = new Vector4(0.85f, 0.075f, 0.4f, 0.5f);

	[Range(0.01f, 2f)]
	public float WindFrequency = 0.25f;

	[Range(0.1f, 25f)]
	public float WaveSizeFoliageShader = 10f;

	[Range(0f, 10f)]
	public float LeafTurbulenceFoliageShader = 4f;

	[Range(0.01f, 1f)]
	public float WindMultiplierForGrassshader = 1f;

	[Range(0f, 10f)]
	public float WaveSizeForGrassshader = 1f;

	[Range(0f, 1f)]
	public float WindJitterFrequencyForGrassshader = 0.25f;

	[Range(0f, 1f)]
	public float WindJitterScaleForGrassshader = 0.15f;

	public bool SyncWindDir;

	[Range(0f, 10f)]
	public float WindMuliplierForTreeShaderPrimary = 1f;

	[Range(0f, 10f)]
	public float WindMuliplierForTreeShaderSecondary = 1f;

	public Vector4 WindMuliplierForTreeShader = new Vector4(1f, 1f, 1f, 1f);

	private float temp_WindFrequency = 0.25f;

	private float temp_WindJitterFrequency = 0.25f;

	private const float TwoPI = (float)Math.PI * 2f;

	private float freqSpeed = 0.05f;

	private float domainTime_Wind;

	private float domainTime_2ndBending;

	private float domainTime_Grass;

	[Range(0f, 1f)]
	public float RainAmount;

	[Range(0f, 1f)]
	public float VertexLitAlphaCutOff = 0.3f;

	public Color VertexLitTranslucencyColor = new Color(0.73f, 0.85f, 0.4f, 1f);

	[Range(0f, 1f)]
	public float VertexLitTranslucencyViewDependency = 0.7f;

	[Range(0f, 1f)]
	public float VertexLitShadowStrength = 0.8f;

	public Color VertexLitSpecularReflectivity = new Color(0.2f, 0.2f, 0.2f, 1f);

	[Range(0f, 100f)]
	public Vector2 AfsSpecFade = new Vector2(30f, 10f);

	public Texture TerrainFoliageNrmSpecMap;

	public bool AutoSyncToTerrain;

	public Terrain SyncedTerrain;

	public bool AutoSyncInPlaymode;

	public float DetailDistanceForGrassShader = 80f;

	[Range(0f, 1000f)]
	public float BillboardStart = 50f;

	[Range(0f, 30f)]
	public float BillboardFadeLenght = 5f;

	public bool GrassAnimateNormal;

	public Color GrassWavingTint = Color.white;

	public Color AFSTreeColor = Color.black;

	private Camera MainCam;

	public bool TreeBillboardShadows;

	[Range(10f, 100f)]
	public float BillboardFadeOutLength = 60f;

	public bool BillboardAdjustToCamera = true;

	[Range(10f, 90f)]
	public float BillboardAngleLimit = 30f;

	public Color BillboardShadowColor;

	[Range(0f, 4f)]
	public float BillboardAmbientLightFactor = 1f;

	[Range(0f, 2f)]
	public float BillboardAmbientLightDesaturationFactor = 0.7f;

	public bool AutosyncShadowColor;

	public bool EnableCameraLayerCulling;

	[Range(10f, 300f)]
	public int SmallDetailsDistance = 70;

	[Range(10f, 300f)]
	public int MediumDetailsDistance = 90;

	public bool AllGrassObjectsCombined;

	private Vector4 TempWind;

	private float GrassWindSpeed;

	private float SinusWave;

	private Vector4 TriangleWaves;

	private float Oscillation;

	private Vector3 CameraForward = new Vector3(0f, 0f, 0f);

	private Vector3 ShadowCameraForward = new Vector3(0f, 0f, 0f);

	private Vector3 CameraForwardVec;

	private float rollingX;

	private float rollingXShadow;

	private Vector3 lightDir;

	private Vector3 templightDir;

	private float CameraAngle;

	private Terrain[] allTerrains;

	private Vector3[] fLight = new Vector3[9];

	private Vector4[] vCoeff = new Vector4[3];

	private void Awake()
	{
		afsSyncFrequencies();
		afsCheckColorSpace();
		afsLightingSettings();
		afsUpdateWind();
		afsUpdateRain();
		afsSetupTerrainEngine();
		afsAutoSyncToTerrain();
		afsUpdateGrassTreesBillboards();
		afsSetupCameraLayerCulling();
		afsFogSettings();
		afsSetupGrassShader();
	}

	public void Update()
	{
		afsLightingSettings();
		afsUpdateRain();
		afsAutoSyncToTerrain();
		afsUpdateGrassTreesBillboards();
	}

	public void FixedUpdate()
	{
		afsUpdateWind();
	}

	private void afsSyncFrequencies()
	{
		temp_WindFrequency = WindFrequency;
		temp_WindJitterFrequency = WindJitterFrequencyForGrassshader;
		domainTime_Wind = 0f;
		domainTime_2ndBending = 0f;
		domainTime_Grass = 0f;
	}

	private void afsCheckColorSpace()
	{
	}

	private void afsLightingSettings()
	{
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)DirectionalLightReference != (Object)null)
		{
			DirectlightDir = DirectionalLightReference.transform.forward;
			if ((Object)(object)Sunlight == (Object)null)
			{
				Sunlight = DirectionalLightReference.GetComponent<Light>();
			}
			if (!((Behaviour)Sunlight).enabled)
			{
				Shader.SetGlobalVector("_AfsDirectSunDir", new Vector4(0f, 0f, 0f, 0f));
			}
			else
			{
				SunLightCol = new Vector3(Sunlight.color.r, Sunlight.color.g, Sunlight.color.b) * Sunlight.intensity;
				SunLuminance = Vector3.Dot(SunLightCol, new Vector3(0.22f, 0.707f, 0.071f));
				Shader.SetGlobalVector("_AfsDirectSunDir", new Vector4(DirectlightDir.x, DirectlightDir.y, DirectlightDir.z, SunLuminance));
				Shader.SetGlobalVector("_AfsDirectSunCol", Vector4.op_Implicit(SunLightCol));
			}
		}
		else
		{
			Shader.SetGlobalVector("_AfsDirectSunDir", new Vector4(0f, 0f, 0f, 0f));
			Shader.SetGlobalVector("_AfsDirectSunCol", Vector4.op_Implicit(new Vector3(0f, 0f, 0f)));
		}
		UpdateLightingForClassicBillboards();
		Shader.SetGlobalVector("_AfsSpecFade", new Vector4(AfsSpecFade.x, AfsSpecFade.y, 1f, 1f));
		if (GrassApproxTrans)
		{
			Shader.EnableKeyword("_AFS_GRASS_APPROXTRANS");
		}
		else
		{
			Shader.DisableKeyword("_AFS_GRASS_APPROXTRANS");
		}
	}

	private void afsFogSettings()
	{
		if (disableFoginShader)
		{
			Shader.EnableKeyword("_AFS_DEFERRED");
		}
		else
		{
			Shader.DisableKeyword("_AFS_DEFERRED");
		}
	}

	private void afsSetupGrassShader()
	{
		if (Application.isPlaying || AllGrassObjectsCombined)
		{
			Shader.DisableKeyword("IN_EDITMODE");
			Shader.EnableKeyword("IN_PLAYMODE");
		}
		else
		{
			Shader.DisableKeyword("IN_PLAYMODE");
			Shader.EnableKeyword("IN_EDITMODE");
		}
	}

	private void afsSetupTerrainEngine()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		Shader.SetGlobalFloat("_AfsAlphaCutOff", VertexLitAlphaCutOff);
		Shader.SetGlobalFloat("_AfsTranslucencyViewDependency", VertexLitTranslucencyViewDependency);
		Shader.SetGlobalFloat("_AfsShadowStrength", VertexLitShadowStrength);
		Shader.SetGlobalColor("_AfsTranslucencyColor", VertexLitTranslucencyColor);
		if (isLinear)
		{
			Shader.SetGlobalColor("_AfsSpecularReflectivity", VertexLitSpecularReflectivity.linear);
		}
		else
		{
			Shader.SetGlobalColor("_AfsSpecularReflectivity", VertexLitSpecularReflectivity);
		}
		if ((Object)(object)TerrainFoliageNrmSpecMap != (Object)null)
		{
			Shader.SetGlobalTexture("_TerrianBumpTransSpecMap", TerrainFoliageNrmSpecMap);
		}
		else
		{
			Shader.SetGlobalTexture("_TerrianBumpTransSpecMap", (Texture)null);
		}
	}

	private void afsUpdateWind()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (SyncWindDir)
		{
			Wind = new Vector4(((Component)this).transform.forward.x, ((Component)this).transform.forward.y, ((Component)this).transform.forward.z, Wind.w);
		}
		TempWind = Wind;
		TempWind.w *= 2f;
		Shader.SetGlobalVector("_Wind", TempWind);
		Shader.SetGlobalFloat("_WindFrequency", WindFrequency);
		domainTime_Wind += temp_WindFrequency * Time.deltaTime * 2f;
		domainTime_2ndBending += Time.deltaTime;
		Shader.SetGlobalVector("_AfsTimeFrequency", new Vector4(domainTime_Wind, domainTime_2ndBending, 0.375f * (1f + temp_WindFrequency * LeafTurbulenceFoliageShader), 0.193f * (1f + temp_WindFrequency * LeafTurbulenceFoliageShader)));
		temp_WindFrequency = Mathf.MoveTowards(temp_WindFrequency, WindFrequency, freqSpeed);
		SinusWave = Mathf.Sin(domainTime_Wind);
		TriangleWaves = SmoothTriangleWave(new Vector4(SinusWave, SinusWave * 0.8f, 0f, 0f));
		Oscillation = TriangleWaves.x + TriangleWaves.y * TriangleWaves.y;
		Oscillation = (Oscillation + 8f) * 0.125f * TempWind.w;
		TempWind.x *= Oscillation;
		TempWind.z *= Oscillation;
		Shader.SetGlobalFloat("_AfsWaveSize", 0.5f / WaveSizeFoliageShader);
		Shader.SetGlobalFloat("_AfsWindJitterScale", WindJitterScaleForGrassshader * 10f);
		Shader.SetGlobalVector("_AfsGrassWind", TempWind * WindMultiplierForGrassshader);
		domainTime_Grass += temp_WindJitterFrequency * Time.deltaTime;
		temp_WindJitterFrequency = Mathf.MoveTowards(temp_WindJitterFrequency, WindJitterFrequencyForGrassshader, freqSpeed);
		GrassWindSpeed = domainTime_Grass * 0.1f;
		Shader.SetGlobalVector("_AfsWaveAndDistance", new Vector4(GrassWindSpeed, WaveSizeForGrassshader, TempWind.w, DetailDistanceForGrassShader * DetailDistanceForGrassShader));
		Shader.SetGlobalVector("_AFSWindMuliplier", WindMuliplierForTreeShader);
	}

	private void afsUpdateRain()
	{
	}

	private void afsAutoSyncToTerrain()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (AutoSyncToTerrain && (Object)(object)SyncedTerrain != (Object)null)
		{
			DetailDistanceForGrassShader = SyncedTerrain.detailObjectDistance;
			BillboardStart = SyncedTerrain.treeBillboardDistance;
			if (Application.isPlaying && !TreeBillboardShadows)
			{
				BillboardFadeLenght = SyncedTerrain.treeCrossFadeLength;
			}
			GrassWavingTint = SyncedTerrain.terrainData.wavingGrassTint;
		}
	}

	private void afsUpdateGrassTreesBillboards()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		Shader.SetGlobalColor("_AfsWavingTint", GrassWavingTint);
		Shader.SetGlobalColor("_AfsTreeColor", AFSTreeColor);
		Shader.SetGlobalVector("_AfsTerrainTrees", new Vector4(BillboardStart, BillboardFadeLenght, BillboardFadeOutLength, 0f));
		if (BillboardAdjustToCamera)
		{
			if (Object.op_Implicit((Object)(object)Camera.main))
			{
				MainCam = Camera.main;
				CameraForward = ((Component)MainCam).transform.forward;
				ShadowCameraForward = CameraForward;
				rollingX = ((Component)MainCam).transform.eulerAngles.x;
				if (rollingX >= 270f)
				{
					rollingX -= 270f;
					rollingX = 90f - rollingX;
					rollingXShadow = rollingX;
				}
				else
				{
					rollingXShadow = 0f - rollingX;
					if (rollingX > BillboardAngleLimit)
					{
						rollingX = Mathf.Lerp(rollingX, 0f, rollingX / BillboardAngleLimit - 1f);
					}
					rollingX *= -1f;
				}
			}
		}
		else
		{
			rollingX = 0f;
			rollingXShadow = 0f;
		}
		CameraForward *= rollingX / 90f;
		ShadowCameraForward *= rollingXShadow / 90f;
		Shader.SetGlobalVector("_AfsBillboardCameraForward", new Vector4(CameraForward.x, CameraForward.y, CameraForward.z, 1f));
		Shader.SetGlobalVector("_AfsBillboardShadowCameraForward", new Vector4(ShadowCameraForward.x, ShadowCameraForward.y, ShadowCameraForward.z, 1f));
	}

	private void afsSetupCameraLayerCulling()
	{
		if (EnableCameraLayerCulling)
		{
			for (int i = 0; i < Camera.allCameras.Length; i++)
			{
				float[] array = new float[32];
				array = Camera.allCameras[i].layerCullDistances;
				array[8] = SmallDetailsDistance;
				array[9] = MediumDetailsDistance;
				Camera.allCameras[i].layerCullDistances = array;
				array = null;
			}
		}
	}

	private void UpdateLightingForClassicBillboards()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Invalid comparison between Unknown and I4
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if ((int)RenderSettings.ambientMode == 0)
		{
			AmbientLightingSH = true;
			UdpateSHLightingforBillboards();
			Shader.EnableKeyword("AFS_SH_AMBIENT");
			Shader.DisableKeyword("AFS_COLOR_AMBIENT");
			Shader.DisableKeyword("AFS_GRADIENT_AMBIENT");
		}
		else if ((int)RenderSettings.ambientMode == 1)
		{
			AmbientLightingSH = false;
			if (isLinear)
			{
				Color val = RenderSettings.ambientSkyColor;
				Shader.SetGlobalColor("_AfsSkyColor", val.linear);
				val = RenderSettings.ambientGroundColor;
				Shader.SetGlobalColor("_AfsGroundColor", val.linear);
				val = RenderSettings.ambientEquatorColor;
				Shader.SetGlobalColor("_AfsEquatorColor", val.linear);
			}
			else
			{
				Shader.SetGlobalColor("_AfsSkyColor", RenderSettings.ambientSkyColor);
				Shader.SetGlobalColor("_AfsGroundColor", RenderSettings.ambientGroundColor);
				Shader.SetGlobalColor("_AfsEquatorColor", RenderSettings.ambientEquatorColor);
			}
			Shader.DisableKeyword("AFS_SH_AMBIENT");
			Shader.DisableKeyword("AFS_COLOR_AMBIENT");
			Shader.EnableKeyword("AFS_GRADIENT_AMBIENT");
		}
		else if ((int)RenderSettings.ambientMode == 3)
		{
			AmbientLightingSH = false;
			Shader.SetGlobalVector("_AfsAmbientColor", Color.op_Implicit(RenderSettings.ambientLight));
			Shader.DisableKeyword("AFS_SH_AMBIENT");
			Shader.EnableKeyword("AFS_COLOR_AMBIENT");
			Shader.DisableKeyword("AFS_GRADIENT_AMBIENT");
		}
	}

	private void UdpateSHLightingforBillboards()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0607: Unknown result type (might be due to invalid IL or missing references)
		//IL_0611: Unknown result type (might be due to invalid IL or missing references)
		//IL_06af: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b9: Unknown result type (might be due to invalid IL or missing references)
		ref Vector3 reference = ref fLight[0];
		SphericalHarmonicsL2 ambientProbe = RenderSettings.ambientProbe;
		reference.x = ambientProbe[0, 0];
		ref Vector3 reference2 = ref fLight[0];
		ambientProbe = RenderSettings.ambientProbe;
		reference2.y = ambientProbe[1, 0];
		ref Vector3 reference3 = ref fLight[0];
		ambientProbe = RenderSettings.ambientProbe;
		reference3.z = ambientProbe[2, 0];
		ref Vector3 reference4 = ref fLight[1];
		ambientProbe = RenderSettings.ambientProbe;
		reference4.x = ambientProbe[0, 1];
		ref Vector3 reference5 = ref fLight[1];
		ambientProbe = RenderSettings.ambientProbe;
		reference5.y = ambientProbe[1, 1];
		ref Vector3 reference6 = ref fLight[1];
		ambientProbe = RenderSettings.ambientProbe;
		reference6.z = ambientProbe[2, 1];
		ref Vector3 reference7 = ref fLight[2];
		ambientProbe = RenderSettings.ambientProbe;
		reference7.x = ambientProbe[0, 2];
		ref Vector3 reference8 = ref fLight[2];
		ambientProbe = RenderSettings.ambientProbe;
		reference8.y = ambientProbe[1, 2];
		ref Vector3 reference9 = ref fLight[2];
		ambientProbe = RenderSettings.ambientProbe;
		reference9.z = ambientProbe[2, 2];
		ref Vector3 reference10 = ref fLight[3];
		ambientProbe = RenderSettings.ambientProbe;
		reference10.x = ambientProbe[0, 3];
		ref Vector3 reference11 = ref fLight[3];
		ambientProbe = RenderSettings.ambientProbe;
		reference11.y = ambientProbe[1, 3];
		ref Vector3 reference12 = ref fLight[3];
		ambientProbe = RenderSettings.ambientProbe;
		reference12.z = ambientProbe[2, 3];
		ref Vector3 reference13 = ref fLight[4];
		ambientProbe = RenderSettings.ambientProbe;
		reference13.x = ambientProbe[0, 4];
		ref Vector3 reference14 = ref fLight[4];
		ambientProbe = RenderSettings.ambientProbe;
		reference14.y = ambientProbe[1, 4];
		ref Vector3 reference15 = ref fLight[4];
		ambientProbe = RenderSettings.ambientProbe;
		reference15.z = ambientProbe[2, 4];
		ref Vector3 reference16 = ref fLight[5];
		ambientProbe = RenderSettings.ambientProbe;
		reference16.x = ambientProbe[0, 5];
		ref Vector3 reference17 = ref fLight[5];
		ambientProbe = RenderSettings.ambientProbe;
		reference17.y = ambientProbe[1, 5];
		ref Vector3 reference18 = ref fLight[5];
		ambientProbe = RenderSettings.ambientProbe;
		reference18.z = ambientProbe[2, 5];
		ref Vector3 reference19 = ref fLight[6];
		ambientProbe = RenderSettings.ambientProbe;
		reference19.x = ambientProbe[0, 6];
		ref Vector3 reference20 = ref fLight[6];
		ambientProbe = RenderSettings.ambientProbe;
		reference20.y = ambientProbe[1, 6];
		ref Vector3 reference21 = ref fLight[6];
		ambientProbe = RenderSettings.ambientProbe;
		reference21.z = ambientProbe[2, 6];
		ref Vector3 reference22 = ref fLight[7];
		ambientProbe = RenderSettings.ambientProbe;
		reference22.x = ambientProbe[0, 7];
		ref Vector3 reference23 = ref fLight[7];
		ambientProbe = RenderSettings.ambientProbe;
		reference23.y = ambientProbe[1, 7];
		ref Vector3 reference24 = ref fLight[7];
		ambientProbe = RenderSettings.ambientProbe;
		reference24.z = ambientProbe[2, 7];
		ref Vector3 reference25 = ref fLight[8];
		ambientProbe = RenderSettings.ambientProbe;
		reference25.x = ambientProbe[0, 8];
		ref Vector3 reference26 = ref fLight[8];
		ambientProbe = RenderSettings.ambientProbe;
		reference26.y = ambientProbe[1, 8];
		ref Vector3 reference27 = ref fLight[8];
		ambientProbe = RenderSettings.ambientProbe;
		reference27.z = ambientProbe[2, 8];
		float num = 3.5449078f;
		float num2 = 1f / (2f * num);
		float num3 = Mathf.Sqrt(3f) / (3f * num);
		float num4 = Mathf.Sqrt(15f) / (8f * num);
		float num5 = Mathf.Sqrt(5f) / (16f * num);
		float num6 = 0.5f * num4;
		for (int i = 0; i < 3; i++)
		{
			vCoeff[i].x = (0f - num3) * fLight[3][i];
			vCoeff[i].y = (0f - num3) * fLight[1][i];
			vCoeff[i].z = num3 * fLight[2][i];
			vCoeff[i].w = num2 * fLight[0][i] - num5 * fLight[6][i];
		}
		Shader.SetGlobalVector("afs_SHAr", vCoeff[0] * RenderSettings.ambientIntensity);
		Shader.SetGlobalVector("afs_SHAg", vCoeff[1] * RenderSettings.ambientIntensity);
		Shader.SetGlobalVector("afs_SHAb", vCoeff[2] * RenderSettings.ambientIntensity);
		for (int i = 0; i < 3; i++)
		{
			vCoeff[i].x = num4 * fLight[4][i];
			vCoeff[i].y = (0f - num4) * fLight[5][i];
			vCoeff[i].z = 3f * num5 * fLight[6][i];
			vCoeff[i].w = (0f - num4) * fLight[7][i];
		}
		Shader.SetGlobalVector("afs_SHBr", vCoeff[0] * RenderSettings.ambientIntensity);
		Shader.SetGlobalVector("afs_SHBg", vCoeff[1] * RenderSettings.ambientIntensity);
		Shader.SetGlobalVector("afs_SHBb", vCoeff[2] * RenderSettings.ambientIntensity);
		vCoeff[0].x = num6 * fLight[8][0];
		vCoeff[0].y = num6 * fLight[8][1];
		vCoeff[0].z = num6 * fLight[8][2];
		vCoeff[0].w = 1f;
		Shader.SetGlobalVector("afs_SHC", vCoeff[0] * RenderSettings.ambientIntensity);
	}

	private Color Desaturate(float r, float g, float b)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.3f * r + 0.59f * g + 0.11f * b;
		r = num * BillboardAmbientLightDesaturationFactor + r * (1f - BillboardAmbientLightDesaturationFactor);
		g = num * BillboardAmbientLightDesaturationFactor + g * (1f - BillboardAmbientLightDesaturationFactor);
		b = num * BillboardAmbientLightDesaturationFactor + b * (1f - BillboardAmbientLightDesaturationFactor);
		return new Color(r, g, b, 1f);
	}

	private float CubicSmooth(float x)
	{
		return x * x * (3f - 2f * x);
	}

	private float TriangleWave(float x)
	{
		return Mathf.Abs((x + 0.5f) % 1f * 2f - 1f);
	}

	private float SmoothTriangleWave(float x)
	{
		return CubicSmooth(TriangleWave(x));
	}

	private Vector4 CubicSmooth(Vector4 x)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		x = Vector4.Scale(x, x);
		x = Vector4.Scale(x, new Vector4(3f, 3f, 3f, 3f) - 2f * x);
		return x;
	}

	private Vector4 TriangleWave(Vector4 x)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		x = (x + new Vector4(0.5f, 0.5f, 0.5f, 0.5f)) * 2f - new Vector4(1f, 1f, 1f, 1f);
		return AbsVecFour(x);
	}

	private Vector4 SmoothTriangleWave(Vector4 x)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return CubicSmooth(TriangleWave(x));
	}

	private Vector4 FracVecFour(Vector4 a)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		a.x %= 1f;
		a.y %= 1f;
		a.z %= 1f;
		a.w %= 1f;
		return a;
	}

	private Vector4 AbsVecFour(Vector4 a)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		a.x = Mathf.Abs(a.x);
		a.y = Mathf.Abs(a.y);
		a.z = Mathf.Abs(a.z);
		a.w = Mathf.Abs(a.w);
		return a;
	}

	public SetupAdvancedFoliageShader()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
	}
}
