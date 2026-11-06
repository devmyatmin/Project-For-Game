using UnityEngine;
using UnityEngine.Audio;

namespace Beam;

public interface IAudioSource
{
	Transform transform { get; }

	AudioClip Clip { get; set; }

	float Volume { get; set; }

	float Level { get; set; }

	float Pitch { get; set; }

	float DopplerLevel { get; set; }

	float SpatialBlend { get; set; }

	float Spread { get; set; }

	float Pan { get; set; }

	int Priority { get; set; }

	bool Loop { get; set; }

	bool IsPlaying { get; }

	bool IsPaused { get; }

	AudioRollOff RollOff { get; set; }

	AudioMixerGroup OutputAudioMixerGroup { get; set; }

	void Play();

	void Pause();

	void Unpause();

	void Stop();
}
