using System;
using UnityEngine;

namespace Beam;

public class AudioWeatherEventEffect : WeatherEventEffectBase
{
	[Serializable]
	public class Audio
	{
		[SerializeField]
		private AudioSource _audioSource;

		private float _originalVolume;

		private bool _initialized;

		public AudioSource AudioSource => _audioSource;

		public float OriginalVolume
		{
			get
			{
				return _originalVolume;
			}
			set
			{
				_originalVolume = value;
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
	private Audio[] _audio;

	public override void InitializeEffect(WeatherEvent weather)
	{
		base.InitializeEffect(weather);
		if (_audio.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _audio.Length; i++)
		{
			((Component)_audio[i].AudioSource).gameObject.SetActive(true);
			_audio[i].AudioSource.Play();
			if (!_audio[i].Initialized)
			{
				_audio[i].OriginalVolume = _audio[i].AudioSource.volume;
			}
			_audio[i].AudioSource.volume = 0f;
			_audio[i].Initialized = true;
		}
	}

	public override void UpdateEffect(float normalizedFade)
	{
		if (_audio.Length != 0)
		{
			for (int i = 0; i < _audio.Length; i++)
			{
				float num = normalizedFade * _audio[i].OriginalVolume;
				_audio[i].AudioSource.volume = Easing.EaseIn(num, EasingType.Cubic);
			}
		}
	}

	public override void Cease()
	{
		base.Cease();
		if (_audio.Length != 0)
		{
			for (int i = 0; i < _audio.Length; i++)
			{
				((Component)_audio[i].AudioSource).gameObject.SetActive(false);
			}
		}
	}
}
