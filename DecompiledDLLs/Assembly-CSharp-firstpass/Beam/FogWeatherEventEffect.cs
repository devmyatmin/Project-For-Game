using Beam.PostEffects;
using UnityEngine;

namespace Beam;

public class FogWeatherEventEffect : WeatherEventEffectBase
{
	[SerializeField]
	private AtmosphericFogController _fogController;

	[Range(0f, 1f)]
	[SerializeField]
	private float _affectFogPercent;

	[SerializeField]
	private float _fogVisibility;

	[SerializeField]
	private float _fogHeight = 80f;

	public override void UpdateEffect(float normalizedFade)
	{
		Shader.SetGlobalFloat("_StormFade", normalizedFade * _affectFogPercent);
		if ((Object)(object)_fogController != (Object)null)
		{
			_fogController.Height = Mathf.Lerp(_fogController.OriginalHeight, _fogHeight, normalizedFade);
			_fogController.StartDistance = Mathf.Lerp(_fogController.OriginalStartDistance, _fogVisibility, normalizedFade);
		}
	}

	public override void Cease()
	{
		base.Cease();
		Shader.SetGlobalFloat("_StormFade", 0f);
		if ((Object)(object)_fogController != (Object)null)
		{
			_fogController.Height = _fogController.OriginalHeight;
			_fogController.StartDistance = _fogController.OriginalStartDistance;
		}
	}
}
