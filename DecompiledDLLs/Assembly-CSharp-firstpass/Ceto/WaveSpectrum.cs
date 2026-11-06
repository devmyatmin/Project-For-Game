using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Ceto.Common.Containers.Interpolation;
using Ceto.Common.Containers.Queues;
using Ceto.Common.Threading.Scheduling;
using Ceto.Common.Threading.Tasks;
using Ceto.Common.Unity.Utility;
using UnityEngine;

namespace Ceto;

[AddComponentMenu("Ceto/Components/WaveSpectrum")]
[DisallowMultipleComponent]
[RequireComponent(typeof(Ocean))]
public class WaveSpectrum : OceanComponent
{
	private struct BufferSettings
	{
		public bool beenCreated;

		public bool isCpu;

		public int size;
	}

	public const float MAX_CHOPPYNESS = 1.2f;

	public const float MAX_FOAM_AMOUNT = 6f;

	public const float MAX_FOAM_COVERAGE = 0.5f;

	public const float MAX_WIND_SPEED = 30f;

	public const float MIN_WAVE_AGE = 0.5f;

	public const float MAX_WAVE_AGE = 1f;

	public const float MAX_WAVE_SPEED = 10f;

	public const float MIN_GRID_SCALE = 0.1f;

	public const float MAX_GRID_SCALE = 1f;

	public const float MAX_WAVE_SMOOTHING = 6f;

	public const float MIN_WAVE_SMOOTHING = 1f;

	public const float MAX_SLOPE_SMOOTHING = 6f;

	public const float MIN_SLOPE_SMOOTHING = 1f;

	public const float MAX_FOAM_SMOOTHING = 6f;

	public const float MIN_FOAM_SMOOTHING = 1f;

	public FOURIER_SIZE fourierSize = FOURIER_SIZE.MEDIUM_64_CPU;

	public SPECTRUM_TYPE spectrumType;

	[Range(1f, 4f)]
	public int numberOfGrids = 4;

	public bool disableReadBack = true;

	public bool disableDisplacements;

	public bool disableSlopes;

	public bool disableFoam;

	public bool textureFoam = true;

	[Range(0f, 1.2f)]
	public float choppyness = 0.8f;

	[Range(0f, 6f)]
	public float foamAmount = 1f;

	[Range(0f, 0.5f)]
	public float foamCoverage = 0.1f;

	[Range(0f, 30f)]
	public float windSpeed = 8f;

	[Range(0.5f, 1f)]
	public float waveAge = 0.64f;

	[Range(0f, 10f)]
	public float waveSpeed = 1f;

	[Range(0.1f, 1f)]
	public float gridScale = 0.5f;

	[Range(1f, 6f)]
	public float waveSmoothing = 2f;

	[Range(1f, 6f)]
	private float slopeSmoothing = 1f;

	[Range(1f, 6f)]
	private float foamSmoothing = 2f;

	private RenderTexture[] m_displacementMaps;

	private RenderTexture[] m_slopeMaps;

	private RenderTexture[] m_foamMaps;

	private Material m_slopeCopyMat;

	private Material m_displacementCopyMat;

	private Material m_foamCopyMat;

	private Material m_slopeInitMat;

	private Material m_displacementInitMat;

	private Material m_foamInitMat;

	private Vector4 m_gridSizes = Vector4.one;

	private Vector4 m_choppyness = Vector4.one;

	private Scheduler m_scheduler;

	private WaveSpectrumCondition[] m_conditions;

	private WaveSpectrumBuffer m_displacementBuffer;

	private WaveSpectrumBuffer m_slopeBuffer;

	private WaveSpectrumBuffer m_jacobianBuffer;

	private FindRangeTask m_findRangeTask;

	private ComputeBuffer m_readBuffer;

	private QueryGridScaling m_queryScaling = new QueryGridScaling();

	private BufferSettings m_bufferSettings;

	private DictionaryQueue<WaveSpectrumConditionKey, WaveSpectrumCondition> m_conditionCache = new DictionaryQueue<WaveSpectrumConditionKey, WaveSpectrumCondition>();

	private int m_maxConditionCacheSize = 10;

	[HideInInspector]
	public Shader initSlopeSdr;

	[HideInInspector]
	public Shader initDisplacementSdr;

	[HideInInspector]
	public Shader initJacobianSdr;

	[HideInInspector]
	public Shader fourierSdr;

	[HideInInspector]
	public Shader slopeCopySdr;

	[HideInInspector]
	public Shader displacementCopySdr;

	[HideInInspector]
	public Shader foamCopySdr;

	[HideInInspector]
	public ComputeShader readSdr;

	public IList<RenderTexture> DisplacementMaps => m_displacementMaps;

	public IList<RenderTexture> SlopeMaps => m_slopeMaps;

	public IList<RenderTexture> FoamMaps => m_foamMaps;

	public Vector2 MaxDisplacement
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public bool IsCreatingNewCondition
	{
		get
		{
			if (m_conditions != null)
			{
				return m_conditions[1] != null;
			}
			return false;
		}
	}

	public Vector4 GridSizes
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_gridSizes;
		}
	}

	public Vector4 Choppyness
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_choppyness;
		}
	}

	public int MaxConditionCacheSize
	{
		get
		{
			return m_maxConditionCacheSize;
		}
		set
		{
			m_maxConditionCacheSize = value;
		}
	}

	public IDisplacementBuffer DisplacementBuffer => m_displacementBuffer as IDisplacementBuffer;

	public ICustomWaveSpectrum CustomWaveSpectrum { get; set; }

	private void Start()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected Obj, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected Obj, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected Obj, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected Obj, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected Obj, but got Unknown
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected Obj, but got Unknown
		try
		{
			Shader.SetGlobalTexture("Ceto_SlopeMap0", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_SlopeMap1", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_DisplacementMap0", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_DisplacementMap1", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_DisplacementMap2", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_DisplacementMap3", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_FoamMap0", (Texture)(object)Texture2D.blackTexture);
			m_slopeCopyMat = new Material(slopeCopySdr);
			m_displacementCopyMat = new Material(displacementCopySdr);
			m_foamCopyMat = new Material(foamCopySdr);
			m_slopeInitMat = new Material(initSlopeSdr);
			m_displacementInitMat = new Material(initDisplacementSdr);
			m_foamInitMat = new Material(initJacobianSdr);
			m_scheduler = new Scheduler();
			CreateBuffers();
			CreateRenderTextures();
			CreateConditions();
			UpdateQueryScaling();
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
		Shader.DisableKeyword("CETO_USE_4_SPECTRUM_GRIDS");
		Shader.SetGlobalTexture("Ceto_SlopeMap0", (Texture)(object)Texture2D.blackTexture);
		Shader.SetGlobalTexture("Ceto_SlopeMap1", (Texture)(object)Texture2D.blackTexture);
		Shader.SetGlobalTexture("Ceto_DisplacementMap0", (Texture)(object)Texture2D.blackTexture);
		Shader.SetGlobalTexture("Ceto_DisplacementMap1", (Texture)(object)Texture2D.blackTexture);
		Shader.SetGlobalTexture("Ceto_DisplacementMap2", (Texture)(object)Texture2D.blackTexture);
		Shader.SetGlobalTexture("Ceto_DisplacementMap3", (Texture)(object)Texture2D.blackTexture);
		Shader.SetGlobalTexture("Ceto_FoamMap0", (Texture)(object)Texture2D.blackTexture);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		try
		{
			if (m_scheduler != null)
			{
				m_scheduler.ShutingDown = true;
				m_scheduler.CancelAllTasks();
			}
			if (m_conditionCache != null)
			{
				foreach (WaveSpectrumCondition item in m_conditionCache)
				{
					item.Release();
				}
				m_conditionCache.Clear();
				m_conditionCache = null;
			}
			Release();
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	private void Release()
	{
		if (m_displacementBuffer != null)
		{
			m_displacementBuffer.Release();
			m_displacementBuffer = null;
		}
		if (m_slopeBuffer != null)
		{
			m_slopeBuffer.Release();
			m_slopeBuffer = null;
		}
		if (m_jacobianBuffer != null)
		{
			m_jacobianBuffer.Release();
			m_jacobianBuffer = null;
		}
		if (m_readBuffer != null)
		{
			m_readBuffer.Release();
			m_readBuffer = null;
		}
		if (m_conditions != null && m_conditions[0] != null && m_conditions[0].Done)
		{
			CacheCondition(m_conditions[0]);
			if (m_conditionCache == null || !m_conditionCache.ContainsKey(m_conditions[0].Key))
			{
				m_conditions[0].Release();
				m_conditions[0] = null;
			}
		}
		if (m_conditions != null && m_conditions[1] != null && m_conditions[1].Done && (m_conditionCache == null || !m_conditionCache.ContainsKey(m_conditions[1].Key)))
		{
			m_conditions[1].Release();
			m_conditions[1] = null;
		}
		m_conditions = null;
		m_findRangeTask = null;
		RTUtility.ReleaseAndDestroy((IList<RenderTexture>)m_displacementMaps);
		m_displacementMaps = null;
		RTUtility.ReleaseAndDestroy((IList<RenderTexture>)m_slopeMaps);
		m_slopeMaps = null;
		RTUtility.ReleaseAndDestroy((IList<RenderTexture>)m_foamMaps);
		m_foamMaps = null;
	}

	private void Update()
	{
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			gridScale = Mathf.Clamp(gridScale, 0.1f, 1f);
			windSpeed = Mathf.Clamp(windSpeed, 0f, 30f);
			waveAge = Mathf.Clamp(waveAge, 0.5f, 1f);
			waveSpeed = Mathf.Clamp(waveSpeed, 0f, 10f);
			foamAmount = Mathf.Clamp(foamAmount, 0f, 6f);
			foamCoverage = Mathf.Clamp(foamCoverage, 0f, 0.5f);
			waveSmoothing = Mathf.Clamp(waveSmoothing, 1f, 6f);
			slopeSmoothing = Mathf.Clamp(slopeSmoothing, 1f, 6f);
			foamSmoothing = Mathf.Clamp(foamSmoothing, 1f, 6f);
			numberOfGrids = Mathf.Clamp(numberOfGrids, 1, 4);
			float time = m_ocean.OceanTime.Now * waveSpeed;
			CreateBuffers();
			CreateRenderTextures();
			CreateConditions();
			if (m_conditions[0].Key.NumGrids > 2)
			{
				Shader.EnableKeyword("CETO_USE_4_SPECTRUM_GRIDS");
			}
			else
			{
				Shader.DisableKeyword("CETO_USE_4_SPECTRUM_GRIDS");
			}
			UpdateQueryScaling();
			Shader.SetGlobalVector("Ceto_GridSizes", GridSizes);
			Shader.SetGlobalVector("Ceto_GridScale", Vector4.op_Implicit(new Vector2(gridScale, gridScale)));
			Shader.SetGlobalVector("Ceto_Choppyness", Choppyness);
			Shader.SetGlobalFloat("Ceto_MapSize", (float)m_bufferSettings.size);
			Shader.SetGlobalFloat("Ceto_WaveSmoothing", waveSmoothing);
			Shader.SetGlobalFloat("Ceto_SlopeSmoothing", slopeSmoothing);
			Shader.SetGlobalFloat("Ceto_FoamSmoothing", foamSmoothing);
			Shader.SetGlobalFloat("Ceto_TextureWaveFoam", textureFoam ? 1f : 0f);
			UpdateSpectrumScheduler();
			GenerateDisplacement(time);
			GenerateSlopes(time);
			GenerateFoam(time);
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	private void UpdateSpectrumScheduler()
	{
		try
		{
			m_scheduler.DisableMultithreading = Ocean.DISABLE_ALL_MULTITHREADING;
			m_scheduler.CheckForException();
			m_scheduler.Update();
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	private void UpdateQueryScaling()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		m_choppyness = m_conditions[0].Choppyness * Mathf.Clamp(choppyness, 0f, 1.2f);
		m_gridSizes = m_conditions[0].GridSizes;
		Vector4 invGridSizes = new Vector4
		{
			x = 1f / (GridSizes.x * gridScale),
			y = 1f / (GridSizes.y * gridScale),
			z = 1f / (GridSizes.z * gridScale),
			w = 1f / (GridSizes.w * gridScale)
		};
		m_queryScaling.invGridSizes = invGridSizes;
		m_queryScaling.scaleY = gridScale;
		m_queryScaling.choppyness = Choppyness * gridScale;
		m_queryScaling.offset = m_ocean.PositionOffset;
		m_queryScaling.numGrids = m_conditions[0].Key.NumGrids;
	}

	private void GenerateSlopes(float time)
	{
		if (!disableSlopes && SystemInfo.graphicsShaderLevel < 30)
		{
			Ocean.LogWarning("Spectrum slopes needs at least SM3 to run. Disabling slopes.");
			disableSlopes = true;
		}
		if (disableSlopes)
		{
			m_slopeBuffer.DisableBuffer(-1);
		}
		else
		{
			m_slopeBuffer.EnableBuffer(-1);
		}
		if (m_slopeBuffer.EnabledBuffers() == 0)
		{
			Shader.SetGlobalTexture("Ceto_SlopeMap0", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_SlopeMap1", (Texture)(object)Texture2D.blackTexture);
			return;
		}
		int numGrids = m_conditions[0].Key.NumGrids;
		if (numGrids <= 2)
		{
			m_slopeBuffer.DisableBuffer(1);
		}
		if (!m_slopeBuffer.HasRun || m_slopeBuffer.TimeValue != time)
		{
			m_slopeBuffer.InitMaterial = m_slopeInitMat;
			m_slopeBuffer.InitPass = numGrids - 1;
			m_slopeBuffer.Run(m_conditions[0], time);
		}
		if (!m_slopeBuffer.BeenSampled)
		{
			m_slopeBuffer.EnableSampling();
			if (numGrids > 0)
			{
				m_slopeCopyMat.SetTexture("Ceto_SlopeBuffer", m_slopeBuffer.GetTexture(0));
				Graphics.Blit((Texture)null, m_slopeMaps[0], m_slopeCopyMat, 0);
				Shader.SetGlobalTexture("Ceto_SlopeMap0", (Texture)(object)m_slopeMaps[0]);
			}
			else
			{
				Shader.SetGlobalTexture("Ceto_SlopeMap0", (Texture)(object)Texture2D.blackTexture);
			}
			if (numGrids > 2)
			{
				m_slopeCopyMat.SetTexture("Ceto_SlopeBuffer", m_slopeBuffer.GetTexture(1));
				Graphics.Blit((Texture)null, m_slopeMaps[1], m_slopeCopyMat, 0);
				Shader.SetGlobalTexture("Ceto_SlopeMap1", (Texture)(object)m_slopeMaps[1]);
			}
			else
			{
				Shader.SetGlobalTexture("Ceto_SlopeMap1", (Texture)(object)Texture2D.blackTexture);
			}
			m_slopeBuffer.DisableSampling();
			m_slopeBuffer.BeenSampled = true;
		}
	}

	private void GenerateDisplacement(float time)
	{
		if (!disableDisplacements && SystemInfo.graphicsShaderLevel < 30 && m_displacementBuffer.IsGPU)
		{
			Ocean.LogWarning("Spectrum displacements needs at least SM3 to run on GPU. Disabling displacement.");
			disableDisplacements = true;
		}
		m_displacementBuffer.EnableBuffer(-1);
		if (disableDisplacements)
		{
			m_displacementBuffer.DisableBuffer(-1);
		}
		if (!disableDisplacements && choppyness == 0f)
		{
			m_displacementBuffer.DisableBuffer(1);
			m_displacementBuffer.DisableBuffer(2);
		}
		if (!disableDisplacements && choppyness > 0f)
		{
			m_displacementBuffer.EnableBuffer(1);
			m_displacementBuffer.EnableBuffer(2);
		}
		if (m_displacementBuffer.EnabledBuffers() == 0)
		{
			Shader.SetGlobalTexture("Ceto_DisplacementMap0", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_DisplacementMap1", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_DisplacementMap2", (Texture)(object)Texture2D.blackTexture);
			Shader.SetGlobalTexture("Ceto_DisplacementMap3", (Texture)(object)Texture2D.blackTexture);
		}
		else
		{
			if (!m_displacementBuffer.Done)
			{
				return;
			}
			int numGrids = m_conditions[0].Key.NumGrids;
			if (numGrids <= 2)
			{
				m_displacementBuffer.DisableBuffer(2);
			}
			if (!m_displacementBuffer.HasRun || m_displacementBuffer.TimeValue != time)
			{
				m_displacementBuffer.InitMaterial = m_displacementInitMat;
				m_displacementBuffer.InitPass = numGrids - 1;
				m_displacementBuffer.Run(m_conditions[0], time);
			}
			if (!m_displacementBuffer.BeenSampled)
			{
				m_displacementBuffer.EnableSampling();
				m_displacementCopyMat.SetTexture("Ceto_HeightBuffer", m_displacementBuffer.GetTexture(0));
				m_displacementCopyMat.SetTexture("Ceto_DisplacementBuffer", m_displacementBuffer.GetTexture(1));
				if (numGrids > 0)
				{
					Graphics.Blit((Texture)null, m_displacementMaps[0], m_displacementCopyMat, (numGrids == 1) ? 4 : 0);
					Shader.SetGlobalTexture("Ceto_DisplacementMap0", (Texture)(object)m_displacementMaps[0]);
				}
				else
				{
					Shader.SetGlobalTexture("Ceto_DisplacementMap0", (Texture)(object)Texture2D.blackTexture);
				}
				if (numGrids > 1)
				{
					Graphics.Blit((Texture)null, m_displacementMaps[1], m_displacementCopyMat, 1);
					Shader.SetGlobalTexture("Ceto_DisplacementMap1", (Texture)(object)m_displacementMaps[1]);
				}
				else
				{
					Shader.SetGlobalTexture("Ceto_DisplacementMap1", (Texture)(object)Texture2D.blackTexture);
				}
				m_displacementCopyMat.SetTexture("Ceto_DisplacementBuffer", m_displacementBuffer.GetTexture(2));
				if (numGrids > 2)
				{
					Graphics.Blit((Texture)null, m_displacementMaps[2], m_displacementCopyMat, 2);
					Shader.SetGlobalTexture("Ceto_DisplacementMap2", (Texture)(object)m_displacementMaps[2]);
				}
				else
				{
					Shader.SetGlobalTexture("Ceto_DisplacementMap2", (Texture)(object)Texture2D.blackTexture);
				}
				if (numGrids > 3)
				{
					Graphics.Blit((Texture)null, m_displacementMaps[3], m_displacementCopyMat, 3);
					Shader.SetGlobalTexture("Ceto_DisplacementMap3", (Texture)(object)m_displacementMaps[3]);
				}
				else
				{
					Shader.SetGlobalTexture("Ceto_DisplacementMap3", (Texture)(object)Texture2D.blackTexture);
				}
				m_displacementBuffer.DisableSampling();
				m_displacementBuffer.BeenSampled = true;
				if (m_displacementBuffer.IsGPU)
				{
					ReadFromGPU(numGrids);
				}
				FindRanges();
			}
		}
	}

	private void ReadFromGPU(int numGrids)
	{
		if (!disableReadBack && (Object)(object)readSdr == (Object)null)
		{
			Ocean.LogWarning("Trying to read GPU displacement data but the read shader is null");
		}
		bool flag = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;
		if ((!disableReadBack && (Object)(object)readSdr != (Object)null && m_readBuffer != null) & flag)
		{
			InterpolatedArray2f[] readDisplacements = DisplacementBuffer.GetReadDisplacements();
			if (numGrids > 0)
			{
				CBUtility.ReadFromRenderTexture(m_displacementMaps[0], 3, m_readBuffer, readSdr);
				m_readBuffer.GetData((Array)readDisplacements[0].Data);
			}
			else
			{
				readDisplacements[0].Clear();
			}
			if (numGrids > 1)
			{
				CBUtility.ReadFromRenderTexture(m_displacementMaps[1], 3, m_readBuffer, readSdr);
				m_readBuffer.GetData((Array)readDisplacements[1].Data);
			}
			else
			{
				readDisplacements[1].Clear();
			}
			if (numGrids > 2)
			{
				CBUtility.ReadFromRenderTexture(m_displacementMaps[2], 3, m_readBuffer, readSdr);
				m_readBuffer.GetData((Array)readDisplacements[2].Data);
			}
			else
			{
				readDisplacements[2].Clear();
			}
			_ = 3;
		}
	}

	private void FindRanges()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (disableReadBack && DisplacementBuffer.IsGPU)
		{
			MaxDisplacement = new Vector2(0f, 40f * gridScale);
		}
		else if (m_findRangeTask == null || m_findRangeTask.Done)
		{
			if (m_findRangeTask == null)
			{
				m_findRangeTask = new FindRangeTask(this);
			}
			m_findRangeTask.Reset();
			m_scheduler.Run(m_findRangeTask);
		}
	}

	private void GenerateFoam(float time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		Vector4 val = Choppyness;
		if (!disableFoam && SystemInfo.graphicsShaderLevel < 30)
		{
			Ocean.LogWarning("Spectrum foam needs at least SM3 to run. Disabling foam.");
			disableFoam = true;
		}
		float sqrMagnitude = val.sqrMagnitude;
		m_jacobianBuffer.EnableBuffer(-1);
		if (disableFoam || foamAmount == 0f || sqrMagnitude == 0f || !m_conditions[0].SupportsJacobians)
		{
			m_jacobianBuffer.DisableBuffer(-1);
		}
		if (m_jacobianBuffer.EnabledBuffers() == 0)
		{
			Shader.SetGlobalTexture("Ceto_FoamMap0", (Texture)(object)Texture2D.blackTexture);
			return;
		}
		int numGrids = m_conditions[0].Key.NumGrids;
		switch (numGrids)
		{
		case 1:
			m_jacobianBuffer.DisableBuffer(1);
			m_jacobianBuffer.DisableBuffer(2);
			break;
		case 2:
			m_jacobianBuffer.DisableBuffer(2);
			break;
		}
		if (!m_jacobianBuffer.HasRun || m_jacobianBuffer.TimeValue != time)
		{
			m_foamInitMat.SetFloat("Ceto_FoamAmount", foamAmount);
			m_jacobianBuffer.InitMaterial = m_foamInitMat;
			m_jacobianBuffer.InitPass = numGrids - 1;
			m_jacobianBuffer.Run(m_conditions[0], time);
		}
		if (!m_jacobianBuffer.BeenSampled)
		{
			m_jacobianBuffer.EnableSampling();
			m_foamCopyMat.SetTexture("Ceto_JacobianBuffer0", m_jacobianBuffer.GetTexture(0));
			m_foamCopyMat.SetTexture("Ceto_JacobianBuffer1", m_jacobianBuffer.GetTexture(1));
			m_foamCopyMat.SetTexture("Ceto_JacobianBuffer2", m_jacobianBuffer.GetTexture(2));
			m_foamCopyMat.SetTexture("Ceto_HeightBuffer", m_displacementBuffer.GetTexture(0));
			m_foamCopyMat.SetVector("Ceto_FoamChoppyness", val);
			m_foamCopyMat.SetFloat("Ceto_FoamCoverage", foamCoverage);
			Graphics.Blit((Texture)null, m_foamMaps[0], m_foamCopyMat, numGrids - 1);
			Shader.SetGlobalTexture("Ceto_FoamMap0", (Texture)(object)m_foamMaps[0]);
			m_jacobianBuffer.DisableSampling();
			m_jacobianBuffer.BeenSampled = true;
		}
	}

	public void QueryWaves(WaveQuery query)
	{
		if (((Behaviour)this).enabled)
		{
			IDisplacementBuffer displacementBuffer = DisplacementBuffer;
			if (displacementBuffer != null && (!disableReadBack || !displacementBuffer.IsGPU) && (query.mode == QUERY_MODE.DISPLACEMENT || query.mode == QUERY_MODE.POSITION) && (query.sampleSpectrum[0] || query.sampleSpectrum[1] || query.sampleSpectrum[2] || query.sampleSpectrum[3]))
			{
				displacementBuffer.QueryWaves(query, m_queryScaling);
			}
		}
	}

	private void CreateBuffers()
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected Obj, but got Unknown
		GetFourierSize(out var size, out var isCpu);
		if (m_bufferSettings.beenCreated)
		{
			if (m_bufferSettings.size == size && m_bufferSettings.isCpu == isCpu)
			{
				return;
			}
			while (m_scheduler.HasTasks())
			{
				UpdateSpectrumScheduler();
			}
			Release();
			m_bufferSettings.beenCreated = false;
		}
		if (isCpu)
		{
			m_displacementBuffer = new DisplacementBufferCPU(size, m_scheduler);
		}
		else
		{
			m_displacementBuffer = new DisplacementBufferGPU(size, fourierSdr);
		}
		m_slopeBuffer = new WaveSpectrumBufferGPU(size, fourierSdr, 2);
		m_jacobianBuffer = new WaveSpectrumBufferGPU(size, fourierSdr, 3);
		m_readBuffer = new ComputeBuffer(size * size, 12);
		m_conditions = new WaveSpectrumCondition[2];
		m_displacementMaps = new RenderTexture[4];
		m_slopeMaps = new RenderTexture[2];
		m_foamMaps = new RenderTexture[1];
		m_bufferSettings.beenCreated = true;
		m_bufferSettings.size = size;
		m_bufferSettings.isCpu = isCpu;
	}

	private void CreateRenderTextures()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		int size = m_bufferSettings.size;
		int ansio = 9;
		RenderTextureFormat format = (RenderTextureFormat)11;
		for (int i = 0; i < m_displacementMaps.Length; i++)
		{
			CreateMap(ref m_displacementMaps[i], "Displacement", format, size, ansio);
		}
		for (int j = 0; j < m_slopeMaps.Length; j++)
		{
			CreateMap(ref m_slopeMaps[j], "Slope", format, size, ansio);
		}
		for (int k = 0; k < m_foamMaps.Length; k++)
		{
			CreateMap(ref m_foamMaps[k], "Foam", format, size, ansio);
		}
	}

	private void CreateMap(ref RenderTexture map, string name, RenderTextureFormat format, int size, int ansio)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected Obj, but got Unknown
		if ((Object)(object)map != (Object)null)
		{
			if (!map.IsCreated())
			{
				map.Create();
			}
			return;
		}
		map = new RenderTexture(size, size, 0, format, (RenderTextureReadWrite)1);
		((Texture)map).filterMode = (FilterMode)2;
		((Texture)map).wrapMode = (TextureWrapMode)0;
		((Texture)map).anisoLevel = ansio;
		map.useMipMap = true;
		((Object)map).hideFlags = (HideFlags)61;
		((Object)map).name = "Ceto Wave Spectrum " + name + " Texture";
		map.Create();
	}

	private void CreateConditions()
	{
		int size = m_bufferSettings.size;
		WaveSpectrumConditionKey waveSpectrumConditionKey = NewSpectrumConditionKey(size, windSpeed, m_ocean.windDir, waveAge);
		if (m_conditions[0] == null)
		{
			if (m_conditionCache.ContainsKey(waveSpectrumConditionKey))
			{
				m_conditions[0] = m_conditionCache[waveSpectrumConditionKey];
				return;
			}
			m_conditions[0] = NewSpectrumCondition(size, windSpeed, m_ocean.windDir, waveAge);
			SpectrumTask createSpectrumConditionTask = m_conditions[0].GetCreateSpectrumConditionTask();
			((IThreadedTask)createSpectrumConditionTask).Start();
			((IThreadedTask)createSpectrumConditionTask).Run();
			((IThreadedTask)createSpectrumConditionTask).End();
			CacheCondition(m_conditions[0]);
		}
		else if (m_conditions[1] != null && m_conditions[1].Done)
		{
			m_conditions[0] = m_conditions[1];
			m_conditions[1] = null;
			CacheCondition(m_conditions[0]);
		}
		else if (m_conditions[1] == null && m_conditions[0].Done && waveSpectrumConditionKey != m_conditions[0].Key)
		{
			if (m_conditionCache.ContainsKey(waveSpectrumConditionKey))
			{
				m_conditions[0] = m_conditionCache[waveSpectrumConditionKey];
				return;
			}
			m_conditions[1] = NewSpectrumCondition(size, windSpeed, m_ocean.windDir, waveAge);
			IThreadedTask createSpectrumConditionTask2 = m_conditions[1].GetCreateSpectrumConditionTask();
			m_scheduler.Add(createSpectrumConditionTask2);
		}
	}

	private void CacheCondition(WaveSpectrumCondition condition)
	{
		if (condition != null && m_conditionCache != null && !m_conditionCache.ContainsKey(condition.Key))
		{
			m_conditionCache.AddFirst(condition.Key, condition);
			while (m_conditionCache.Count != 0 && m_conditionCache.Count > MaxConditionCacheSize)
			{
				m_conditionCache.RemoveLast().Release();
			}
		}
	}

	public void CreateAndCacheCondition(int fourierSize, float windSpeed, float windDir, float waveAge)
	{
		if (m_conditionCache == null)
		{
			return;
		}
		if (m_conditionCache.Count >= MaxConditionCacheSize)
		{
			Ocean.LogWarning("Condition cache full. Condition not cached.");
			return;
		}
		if (!Mathf.IsPowerOfTwo(fourierSize) || fourierSize < 32 || fourierSize > 512)
		{
			Ocean.LogWarning("Fourier size must be a pow2 number from 32 to 512. Condition not cached.");
			return;
		}
		WaveSpectrumCondition waveSpectrumCondition = NewSpectrumCondition(fourierSize, windSpeed, windDir, waveAge);
		if (!m_conditionCache.ContainsKey(waveSpectrumCondition.Key))
		{
			SpectrumTask createSpectrumConditionTask = waveSpectrumCondition.GetCreateSpectrumConditionTask();
			((IThreadedTask)createSpectrumConditionTask).Start();
			((IThreadedTask)createSpectrumConditionTask).Run();
			((IThreadedTask)createSpectrumConditionTask).End();
			m_conditionCache.AddFirst(waveSpectrumCondition.Key, waveSpectrumCondition);
		}
	}

	private WaveSpectrumCondition NewSpectrumCondition(int fourierSize, float windSpeed, float windDir, float waveAge)
	{
		WaveSpectrumCondition waveSpectrumCondition = null;
		switch (spectrumType)
		{
		case SPECTRUM_TYPE.UNIFIED:
			return new UnifiedSpectrumCondition(fourierSize, windSpeed, windDir, waveAge, numberOfGrids);
		case SPECTRUM_TYPE.UNIFIED_PHILLIPS:
			return new UnifiedPhillipsSpectrumCondition(fourierSize, windSpeed, windDir, waveAge, numberOfGrids);
		case SPECTRUM_TYPE.PHILLIPS:
			return new PhillipsSpectrumCondition(fourierSize, windSpeed, windDir, waveAge, numberOfGrids);
		case SPECTRUM_TYPE.CUSTOM:
			if (CustomWaveSpectrum == null)
			{
				Ocean.LogWarning("Custom spectrum type selected but no custom spectrum interface has been added to the wave spectrum. Defaulting to Unified Spectrum");
				spectrumType = SPECTRUM_TYPE.UNIFIED;
				return new UnifiedSpectrumCondition(fourierSize, windSpeed, windDir, waveAge, numberOfGrids);
			}
			return new CustomWaveSpectrumCondition(CustomWaveSpectrum, fourierSize, windDir, numberOfGrids);
		default:
			throw new InvalidOperationException("Invalid spectrum type = " + spectrumType);
		}
	}

	private WaveSpectrumConditionKey NewSpectrumConditionKey(int fourierSize, float windSpeed, float windDir, float waveAge)
	{
		WaveSpectrumConditionKey waveSpectrumConditionKey = null;
		switch (spectrumType)
		{
		case SPECTRUM_TYPE.UNIFIED:
			return new UnifiedSpectrumConditionKey(windSpeed, waveAge, fourierSize, windDir, spectrumType, numberOfGrids);
		case SPECTRUM_TYPE.UNIFIED_PHILLIPS:
			return new UnifiedSpectrumConditionKey(windSpeed, waveAge, fourierSize, windDir, spectrumType, numberOfGrids);
		case SPECTRUM_TYPE.PHILLIPS:
			return new PhillipsSpectrumConditionKey(windSpeed, fourierSize, windDir, spectrumType, numberOfGrids);
		case SPECTRUM_TYPE.CUSTOM:
			if (CustomWaveSpectrum == null)
			{
				Ocean.LogWarning("Custom spectrum type selected but no custom spectrum interface has been added to the wave spectrum. Defaulting to Unified Spectrum");
				spectrumType = SPECTRUM_TYPE.UNIFIED;
				return new UnifiedSpectrumConditionKey(windSpeed, waveAge, fourierSize, windDir, spectrumType, numberOfGrids);
			}
			return CustomWaveSpectrum.CreateKey(fourierSize, windDir, spectrumType, numberOfGrids);
		default:
			throw new InvalidOperationException("Invalid spectrum type = " + spectrumType);
		}
	}

	private void GetFourierSize(out int size, out bool isCpu)
	{
		switch ((int)fourierSize)
		{
		case 0:
			size = 32;
			isCpu = true;
			break;
		case 1:
			size = 32;
			isCpu = false;
			break;
		case 2:
			size = 64;
			isCpu = true;
			break;
		case 3:
			size = 64;
			isCpu = false;
			break;
		case 4:
			size = 128;
			isCpu = true;
			break;
		case 5:
			size = 128;
			isCpu = false;
			break;
		case 6:
			size = 256;
			isCpu = false;
			break;
		case 7:
			size = 512;
			isCpu = false;
			break;
		default:
			size = 64;
			isCpu = true;
			break;
		}
		bool flag = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;
		if (!isCpu && !disableReadBack && !flag)
		{
			Ocean.LogWarning("You card does not support dx11. Fourier can not be GPU. Changing to CPU. Disable read backs to use GPU but with no height querys.");
			fourierSize = FOURIER_SIZE.MEDIUM_64_CPU;
			size = 64;
			isCpu = true;
		}
	}

	public WaveSpectrum()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
	}
}
