using Beam.Events;
using Ceto;
using UnityEngine;

namespace Beam;

public class CetoSettings : Singleton<CetoSettings>
{
	private float _aboveVisibility;

	private float _belowVisibility;

	[SerializeField]
	private AnimationCurve _aboveVisibilityCurve;

	[SerializeField]
	private AnimationCurve _belowVisibilityCurve;

	private Color _currentWaterColor;

	private Color _currentUnderwaterColor;

	private Color _currentScatterColor;

	private Color _currentUnderwaterInscatterColor;

	[SerializeField]
	private Color _morningWaterColor;

	[SerializeField]
	private Color _middayWaterColor;

	[SerializeField]
	private Color _morningUnderwaterColor;

	[SerializeField]
	private Color _middayUnderwaterColor;

	[SerializeField]
	private Color _morningScatterColor;

	[SerializeField]
	private Color _middayScatterColor;

	[SerializeField]
	private Color _morningUnderwaterInscatterColor;

	[SerializeField]
	private Color _middayUnderwaterInscatterColor;

	[SerializeField]
	private Color _weatherScatterColor;

	[SerializeField]
	private Color _weatherUnderwaterColor;

	[SerializeField]
	private Texture2D _causticsTexture;

	private UnderWater _underwater;

	private WaveSpectrum _waveSpectrum;

	public float BelowVisibility => _belowVisibility;

	public Color CurrentWaterColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _currentWaterColor;
		}
	}

	public Color CurrentUnderwaterColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _currentUnderwaterColor;
		}
	}

	public Color CurrentScatterColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _currentScatterColor;
		}
	}

	public Color CurrentUnderwaterInscatterColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _currentUnderwaterInscatterColor;
		}
	}

	public Color MorningWaterColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _morningWaterColor;
		}
	}

	public Color MiddayWaterColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _middayWaterColor;
		}
	}

	public Color MorningUnderwaterColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _morningUnderwaterColor;
		}
	}

	public Color MiddayUnderwaterColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _middayUnderwaterColor;
		}
	}

	public Color MorningScatterColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _morningScatterColor;
		}
	}

	public Color MiddayScatterColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _middayScatterColor;
		}
	}

	public Color MorningUnderwaterInscatterColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _morningUnderwaterInscatterColor;
		}
	}

	public Color MiddayUnderwaterInscatterColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _middayUnderwaterInscatterColor;
		}
	}

	public UnderWater Underwater
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_underwater))
			{
				_underwater = Ceto.Ocean.Instance.UnderWater;
			}
			return _underwater;
		}
	}

	public WaveSpectrum WaveSpectrum
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_waveSpectrum))
			{
				_waveSpectrum = Ceto.Ocean.Instance.Spectrum;
			}
			return _waveSpectrum;
		}
	}

	private void Start()
	{
		Ceto.Ocean.Instance.windDir = Wind.Angle;
		EventManager.AddListener<OptionsAppliedEvent>(OnOptionsApplied);
		LoadOptions();
	}

	protected override void OnDestroy()
	{
		EventManager.RemoveListener<OptionsAppliedEvent>(OnOptionsApplied);
		base.OnDestroy();
	}

	private void OnOptionsApplied(OptionsAppliedEvent e)
	{
		LoadOptions();
	}

	public void LoadOptions()
	{
		WaveSpectrum spectrum = Ceto.Ocean.Instance.Spectrum;
		ProjectedGrid grid = Ceto.Ocean.Instance.Grid;
		switch (Options.GraphicsSettings.WaterQuality)
		{
		case Quality_LAPTOP_ULTRA.ULTRA:
			grid.resolution = MESH_RESOLUTION.HIGH;
			spectrum.fourierSize = FOURIER_SIZE.MEDIUM_64_CPU;
			spectrum.disableDisplacements = false;
			spectrum.disableReadBack = false;
			spectrum.disableFoam = false;
			break;
		case Quality_LAPTOP_ULTRA.HIGH:
			grid.resolution = MESH_RESOLUTION.MEDIUM;
			spectrum.fourierSize = FOURIER_SIZE.MEDIUM_64_CPU;
			spectrum.disableDisplacements = false;
			spectrum.disableReadBack = false;
			spectrum.disableFoam = false;
			break;
		case Quality_LAPTOP_ULTRA.MEDIUM:
			grid.resolution = MESH_RESOLUTION.MEDIUM;
			spectrum.fourierSize = FOURIER_SIZE.LOW_32_CPU;
			spectrum.disableDisplacements = false;
			spectrum.disableReadBack = false;
			spectrum.disableFoam = false;
			break;
		case Quality_LAPTOP_ULTRA.LOW:
			grid.resolution = MESH_RESOLUTION.LOW;
			spectrum.fourierSize = FOURIER_SIZE.LOW_32_CPU;
			spectrum.disableDisplacements = false;
			spectrum.disableReadBack = false;
			spectrum.disableFoam = true;
			break;
		case Quality_LAPTOP_ULTRA.LAPTOP:
			grid.resolution = MESH_RESOLUTION.LOW;
			spectrum.fourierSize = FOURIER_SIZE.LOW_32_CPU;
			spectrum.disableDisplacements = true;
			spectrum.disableReadBack = true;
			spectrum.disableFoam = true;
			break;
		}
	}

	public void SetCaustics(bool caustics)
	{
		Shader.EnableKeyword(caustics ? "BEAM_CAUSTICS_ON" : "BEAM_CAUSTICS_OFF");
		Shader.DisableKeyword(caustics ? "BEAM_CAUSTICS_OFF" : "BEAM_CAUSTICS_ON");
	}

	private void Update()
	{
		UpdateWaterVisibility(Singleton<GameTime>.Instance.NormalizedTime);
		UpdateWaterColor(Singleton<Atmosphere>.Instance.CurrentIlluminary.Light, Singleton<Atmosphere>.Instance.Altitude);
	}

	public void UpdateWaterVisibility(float normalizedTime)
	{
		_aboveVisibility = _aboveVisibilityCurve.Evaluate(normalizedTime);
		_belowVisibility = _belowVisibilityCurve.Evaluate(normalizedTime);
	}

	public void UpdateWaterColor(Light light, float altitude)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		WeatherEvent weatherEvent = Singleton<AtmosphereStorm>.Instance?.CurrentWeatherEvent;
		float num = (((Object)(object)weatherEvent != (Object)null && weatherEvent.HasStarted) ? (1f - weatherEvent.NormalizedFadeValue) : 1f);
		float num2 = Mathf.Max(0f, altitude) * num;
		float num3 = Mathf.Pow(num2, 0.6f);
		bool flag = altitude > 0f;
		_currentWaterColor = Color.Lerp(_morningWaterColor, _middayWaterColor, num3);
		_currentUnderwaterColor = Color.Lerp(_morningUnderwaterColor, _middayUnderwaterColor, num3);
		_currentScatterColor = (flag ? Color.Lerp(_morningScatterColor, _middayScatterColor, num2) : Color.black);
		_currentUnderwaterInscatterColor = (flag ? Color.Lerp(_morningUnderwaterInscatterColor, _middayUnderwaterInscatterColor, num2) : Color.black);
		if ((Object)(object)weatherEvent != (Object)null && weatherEvent.HasStarted)
		{
			Color val = Color.Lerp(Color.black, _weatherScatterColor, altitude);
			Color val2 = Color.Lerp(Color.black, _weatherUnderwaterColor, altitude);
			_currentScatterColor = Color.Lerp(_currentScatterColor, val, weatherEvent.NormalizedFadeValue);
			_currentUnderwaterColor = Color.Lerp(_currentUnderwaterColor, val2, weatherEvent.NormalizedFadeValue);
		}
		Ceto.Ocean.Instance.foamIntensity = Mathf.Lerp(0.05f, 2f, Mathf.Pow(Mathf.Max(0f, altitude), 0.6f));
		Ceto.Ocean.Instance.m_sun = ((Component)light).gameObject;
		Ceto.Ocean.Instance.UnderWater.aboveAbsorptionModifier.tint = light.color;
		Ceto.Ocean.Instance.UnderWater.aboveInscatterModifier.color = _currentWaterColor;
		Shader.SetGlobalFloat("_SunFade", Mathf.Clamp01(num2));
		Shader.SetGlobalFloat("_AboveVisibility", _aboveVisibility);
		Shader.SetGlobalFloat("_BelowVisibility", _belowVisibility);
		Shader.SetGlobalColor("_Beam_OceanColor", _currentWaterColor.linear);
		Shader.SetGlobalColor("_Beam_OceanScatterColor", _currentScatterColor.linear);
		Shader.SetGlobalColor("_Beam_UnderwaterColor", _currentUnderwaterColor.linear);
		Shader.SetGlobalColor("_Beam_UnderwaterInscatterColor", _currentUnderwaterInscatterColor.linear);
		Shader.SetGlobalTexture("Beam_CausticsTex", (Texture)(object)_causticsTexture);
	}
}
