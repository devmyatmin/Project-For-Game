using UnityEngine;
using UnityEngine.Audio;

namespace Beam;

public class UAudioSource : IAudioSource
{
	private readonly AudioSource _audioSource;

	private float _volume = 1f;

	private float _level = 1f;

	private bool _isPaused;

	public Transform transform => ((Component)_audioSource).transform;

	public AudioClip Clip
	{
		get
		{
			return _audioSource.clip;
		}
		set
		{
			_audioSource.clip = value;
		}
	}

	public float Volume
	{
		get
		{
			return _volume;
		}
		set
		{
			_volume = value;
			_audioSource.volume = _level * _volume;
		}
	}

	public float Level
	{
		get
		{
			return _level;
		}
		set
		{
			_level = value;
			_audioSource.volume = _level * _volume;
		}
	}

	public float Pitch
	{
		get
		{
			return _audioSource.pitch;
		}
		set
		{
			_audioSource.pitch = value;
		}
	}

	public float DopplerLevel
	{
		get
		{
			return _audioSource.dopplerLevel;
		}
		set
		{
			_audioSource.dopplerLevel = value;
		}
	}

	public float SpatialBlend
	{
		get
		{
			return _audioSource.spatialBlend;
		}
		set
		{
			_audioSource.spatialBlend = value;
		}
	}

	public float Spread
	{
		get
		{
			return _audioSource.spread;
		}
		set
		{
			_audioSource.spread = value;
		}
	}

	public float Pan
	{
		get
		{
			return _audioSource.panStereo;
		}
		set
		{
			_audioSource.panStereo = value;
		}
	}

	public int Priority
	{
		get
		{
			return _audioSource.priority;
		}
		set
		{
			_audioSource.priority = value;
		}
	}

	public bool Loop
	{
		get
		{
			return _audioSource.loop;
		}
		set
		{
			_audioSource.loop = value;
		}
	}

	public bool IsPlaying
	{
		get
		{
			if (!_audioSource.isPlaying)
			{
				return _isPaused;
			}
			return true;
		}
	}

	public bool IsPaused => _isPaused;

	public AudioRollOff RollOff { get; set; }

	public AudioMixerGroup OutputAudioMixerGroup
	{
		get
		{
			return _audioSource.outputAudioMixerGroup;
		}
		set
		{
			_audioSource.outputAudioMixerGroup = value;
		}
	}

	public UAudioSource(AudioSource audioSource)
	{
		_audioSource = audioSource;
	}

	public void Play()
	{
		_audioSource.Play();
		_isPaused = false;
	}

	public void Pause()
	{
		_audioSource.Pause();
		_isPaused = true;
	}

	public void Unpause()
	{
		_audioSource.UnPause();
		_isPaused = false;
	}

	public void Stop()
	{
		_audioSource.Stop();
		_isPaused = false;
	}
}
