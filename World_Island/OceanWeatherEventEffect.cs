using System;
using Ceto;
using UnityEngine;

namespace Beam;

public class OceanWeatherEventEffect : WeatherEventEffectBase
{
	[Serializable]
	public struct OceanProperties
	{
		public float WaveAge;

		public float WindSpeed;

		public float FoamAmount;
	}

	[SerializeField]
	private OceanProperties _oceanProperties;

	private OceanProperties _previousOceanProperties;

	private WaveSpectrum _spectrum;

	public override void InitializeEffect(WeatherEvent weather)
	{
		base.InitializeEffect(weather);
		if (!(Ceto.Ocean.Instance == null))
		{
			if (_spectrum == null)
			{
				_spectrum = Ceto.Ocean.Instance.Spectrum;
			}
			_previousOceanProperties = default(OceanProperties);
			_previousOceanProperties.WaveAge = _spectrum.waveAge;
			_previousOceanProperties.WindSpeed = _spectrum.windSpeed;
			_previousOceanProperties.FoamAmount = _spectrum.foamAmount;
		}
	}

	public override void UpdateEffect(float normalizedFade)
	{
		if (!(Ceto.Ocean.Instance == null) && !_spectrum.IsCreatingNewCondition)
		{
			float num = Mathf.Lerp(_previousOceanProperties.WindSpeed, _oceanProperties.WindSpeed, normalizedFade);
			num = Mathf.Round(num / 0.05f) * 0.05f;
			if (!Mathf.Approximately(_spectrum.windSpeed, num))
			{
				_spectrum.windSpeed = num;
			}
			float num2 = Mathf.Lerp(_previousOceanProperties.WaveAge, _oceanProperties.WaveAge, normalizedFade);
			num2 = Mathf.Round(num2 / 0.02f) * 0.02f;
			if (!Mathf.Approximately(_spectrum.waveAge, num2))
			{
				_spectrum.waveAge = num2;
			}
			_spectrum.foamAmount = Mathf.Lerp(_previousOceanProperties.FoamAmount, _oceanProperties.FoamAmount, normalizedFade);
		}
	}

	public override void Cease()
	{
		if (!(Ceto.Ocean.Instance == null))
		{
			_spectrum.waveAge = _previousOceanProperties.WaveAge;
			_spectrum.windSpeed = _previousOceanProperties.WindSpeed;
			_spectrum.foamAmount = _previousOceanProperties.FoamAmount;
		}
	}
}
