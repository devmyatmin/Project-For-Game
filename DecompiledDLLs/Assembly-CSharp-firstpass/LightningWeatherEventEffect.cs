using System;
using System.Collections.Generic;
using Beam;
using MEC;
using UnityEngine;

public class LightningWeatherEventEffect : WeatherEventEffectBase
{
	[Serializable]
	public class LightningEffect
	{
		public AnimationCurve LightCurve;

		public float LightIntensity;

		public AudioClip Clip;

		public float Duration;

		public float Interval;
	}

	[SerializeField]
	private Light _light;

	[SerializeField]
	private AudioSource _audio;

	[SerializeField]
	private LightningEffect[] _lightningEffects;

	private CoroutineHandle _lightningSequence;

	private bool _isDone = true;

	public override void UpdateEffect(float normalizedFade)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (WeatherEvent.IsActive && _isDone)
		{
			LightningEffect lightning = _lightningEffects[Random.Range(0, _lightningEffects.Length)];
			_lightningSequence = Timing.RunCoroutine(Lightning_Sequence(lightning));
		}
	}

	private IEnumerator<float> Lightning_Sequence(LightningEffect lightning)
	{
		_isDone = false;
		((Behaviour)_light).enabled = true;
		_audio.clip = lightning.Clip;
		_audio.Play();
		float timer = 0f;
		while (timer < lightning.Duration)
		{
			timer += Time.deltaTime;
			float num = lightning.LightCurve.Evaluate(timer / lightning.Duration);
			_light.intensity = num * lightning.LightIntensity;
			Shader.SetGlobalFloat("_SheetLightning", num);
			yield return 0f;
		}
		while (timer < lightning.Duration + lightning.Interval || _audio.isPlaying)
		{
			timer += Time.deltaTime;
			yield return 0f;
		}
		Shader.SetGlobalFloat("_SheetLightning", 0f);
		((Behaviour)_light).enabled = false;
		_isDone = true;
	}

	public override void Cease()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		base.Cease();
		Timing.KillCoroutines(_lightningSequence);
	}
}
