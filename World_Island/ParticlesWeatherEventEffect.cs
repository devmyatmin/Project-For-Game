using System;
using Beam.Utilities;
using UnityEngine;

namespace Beam;

public class ParticlesWeatherEventEffect : WeatherEventEffectBase
{
	[Serializable]
	public class Particles
	{
		[SerializeField]
		private ParticleSystem _particleSystem;

		private float _originalEmission;

		private bool _initialized;

		public ParticleSystem ParticleSystem => _particleSystem;

		public float OriginalEmission
		{
			get
			{
				return _originalEmission;
			}
			set
			{
				_originalEmission = value;
			}
		}

		public bool Initialized
		{
			get
			{
				return _initialized;
			}
			set
			{
				_initialized = value;
			}
		}
	}

	[SerializeField]
	private Particles[] _particles;

	public override void InitializeEffect(WeatherEvent weather)
	{
		base.InitializeEffect(weather);
		if (_particles.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _particles.Length; i++)
		{
			Particles particles = _particles[i];
			if (!particles.Initialized)
			{
				particles.OriginalEmission = particles.ParticleSystem.emission.rateOverTime.constantMax;
				particles.Initialized = true;
			}
			ParticleSystem.EmissionModule emission = particles.ParticleSystem.emission;
			emission.rateOverTime = 0f;
			particles.ParticleSystem.gameObject.SetActive(value: true);
			particles.ParticleSystem.Play();
		}
	}

	public override void UpdateEffect(float normalizedFade)
	{
		if (_particles.Length == 0)
		{
			return;
		}
		bool flag = !PlayerRegistry.LocalPlayer.IsNullOrDestroyed() && (bool)PlayerRegistry.LocalPlayer.Movement;
		for (int i = 0; i < _particles.Length; i++)
		{
			Particles particles = _particles[i];
			float num = normalizedFade * particles.OriginalEmission;
			bool flag2 = flag && PlayerRegistry.LocalPlayer.Movement.IsUnderwater;
			particles.ParticleSystem.gameObject.SetActive(!flag2);
			ParticleSystem.EmissionModule emission = particles.ParticleSystem.emission;
			emission.rateOverTime = num;
			if (flag)
			{
				base.transform.position = new Vector3(base.transform.position.x, PlayerRegistry.LocalPlayer.transform.position.y + 1.4f, base.transform.position.z);
			}
			ParticleSystemRenderer component = particles.ParticleSystem.gameObject.GetComponent<ParticleSystemRenderer>();
			bool flag3 = flag && PlayerRegistry.LocalPlayer.Movement.InWater;
			bool flag4 = flag && PlayerRegistry.LocalPlayer.Movement.isFloating;
			if (flag3 && !flag4)
			{
				component.enabled = false;
			}
			else
			{
				component.enabled = true;
			}
		}
	}

	public override void Cease()
	{
		base.Cease();
		if (_particles.Length != 0)
		{
			for (int i = 0; i < _particles.Length; i++)
			{
				_particles[i].ParticleSystem.gameObject.SetActive(value: false);
			}
		}
	}
}
