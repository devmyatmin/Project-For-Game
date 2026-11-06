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
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
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
				EmissionModule emission = particles.ParticleSystem.emission;
				MinMaxCurve rateOverTime = emission.rateOverTime;
				particles.OriginalEmission = rateOverTime.constantMax;
				particles.Initialized = true;
			}
			EmissionModule emission2 = particles.ParticleSystem.emission;
			emission2.rateOverTime = MinMaxCurve.op_Implicit(0f);
			((Component)particles.ParticleSystem).gameObject.SetActive(true);
			particles.ParticleSystem.Play();
		}
	}

	public override void UpdateEffect(float normalizedFade)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if (_particles.Length == 0)
		{
			return;
		}
		bool flag = !BeamTeam.IsNullOrDestroyed(PlayerRegistry.LocalPlayer) && Object.op_Implicit((Object)(object)PlayerRegistry.LocalPlayer.Movement);
		for (int i = 0; i < _particles.Length; i++)
		{
			Particles particles = _particles[i];
			float num = normalizedFade * particles.OriginalEmission;
			bool flag2 = flag && PlayerRegistry.LocalPlayer.Movement.IsUnderwater;
			((Component)particles.ParticleSystem).gameObject.SetActive(!flag2);
			EmissionModule emission = particles.ParticleSystem.emission;
			emission.rateOverTime = MinMaxCurve.op_Implicit(num);
			if (flag)
			{
				((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, PlayerRegistry.LocalPlayer.transform.position.y + 1.4f, ((Component)this).transform.position.z);
			}
			ParticleSystemRenderer component = ((Component)particles.ParticleSystem).gameObject.GetComponent<ParticleSystemRenderer>();
			bool flag3 = flag && PlayerRegistry.LocalPlayer.Movement.InWater;
			bool flag4 = flag && PlayerRegistry.LocalPlayer.Movement.isFloating;
			if (flag3 && !flag4)
			{
				((Renderer)component).enabled = false;
			}
			else
			{
				((Renderer)component).enabled = true;
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
				((Component)_particles[i].ParticleSystem).gameObject.SetActive(false);
			}
		}
	}
}
