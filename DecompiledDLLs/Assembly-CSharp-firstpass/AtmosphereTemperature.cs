using Beam;
using UnityEngine;

public class AtmosphereTemperature : Singleton<AtmosphereTemperature>
{
	[SerializeField]
	private AnimationCurve _oceanTemperatureThermocline;

	private float _oceanTemperature;

	private void Awake()
	{
		_oceanTemperature = 78f;
		InitializeOceanTemperature();
	}

	private void InitializeOceanTemperature()
	{
		_oceanTemperatureThermocline.keys = null;
		float num = 0.00014285714f;
		float num2 = 1f / 75f;
		_oceanTemperatureThermocline.AddKey(0f, 75f * num2);
		_oceanTemperatureThermocline.AddKey(500f * num, 68f * num2);
		_oceanTemperatureThermocline.AddKey(700f * num, 53.6f * num2);
		_oceanTemperatureThermocline.AddKey(900f * num, 46.4f * num2);
		_oceanTemperatureThermocline.AddKey(1250f * num, 41.8f * num2);
		_oceanTemperatureThermocline.AddKey(1500f * num, 41f * num2);
		_oceanTemperatureThermocline.AddKey(2000f * num, 40.1f * num2);
		_oceanTemperatureThermocline.AddKey(3500f * num, 39.2f * num2);
		_oceanTemperatureThermocline.AddKey(7000f * num, 33.8f * num2);
	}

	public float GetOceanTemperatureForDepth(float depth)
	{
		return _oceanTemperature * ((depth < 0f) ? _oceanTemperatureThermocline.Evaluate(Mathf.Abs(depth * (1f / 128f))) : 1f);
	}

	public UVIndex GetUVIndex()
	{
		return UVIndex.High;
	}
}
