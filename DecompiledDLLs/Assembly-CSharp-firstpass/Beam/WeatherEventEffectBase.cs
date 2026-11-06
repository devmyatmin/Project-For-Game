using System;
using UnityEngine;

namespace Beam;

[Serializable]
public abstract class WeatherEventEffectBase : MonoBehaviour
{
	private WeatherEvent _weatherEvent;

	protected WeatherEvent WeatherEvent => _weatherEvent;

	public abstract void UpdateEffect(float normalizedFade);

	public virtual void InitializeEffect(WeatherEvent weather)
	{
		_weatherEvent = weather;
	}

	public virtual void Cease()
	{
	}
}
