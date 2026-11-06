using Beam.UI;
using UnityEngine;
using UnityEngine.Audio;

namespace Beam;

public sealed class NullAudioSource : IAudioSource
{
	public static NullAudioSource Instance { get; } = new NullAudioSource();

	public Transform transform => NullTransform.Instance;

	public AudioClip Clip { get; set; }

	public float Volume { get; set; }

	public float Level { get; set; }

	public float Pitch { get; set; }

	public float DopplerLevel { get; set; }

	public float SpatialBlend { get; set; }

	public float Spread { get; set; }

	public float Pan { get; set; }

	public int Priority { get; set; }

	public bool Loop { get; set; }

	public bool IsPlaying { get; }

	public bool IsPaused { get; }

	public AudioRollOff RollOff { get; set; }

	public AudioMixerGroup OutputAudioMixerGroup { get; set; }

	public void Play()
	{
	}

	public void Pause()
	{
	}

	public void Unpause()
	{
	}

	public void Stop()
	{
	}
}
