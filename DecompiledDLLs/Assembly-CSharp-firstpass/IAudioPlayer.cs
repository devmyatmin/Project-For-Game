using Beam;
using UnityEngine;

public interface IAudioPlayer
{
	AudioActorHandle Play2D(AudioClip clip, AudioMixerChannels channel, AudioPlayMode mode = AudioPlayMode.Single);

	AudioActorHandle Play3D(AudioClip clip, Vector3 position, AudioMixerChannels channel, AudioRollOffDistance distance = AudioRollOffDistance.Far, AudioPlayMode mode = AudioPlayMode.Single);

	AudioActorHandle Play3D(AudioClip[] clips, Vector3 position, AudioMixerChannels channel, AudioRollOffDistance distance = AudioRollOffDistance.Far, AudioPlayMode mode = AudioPlayMode.Single);

	AudioActorHandle Play3D(AudioClip clip, Transform target, AudioMixerChannels channel, AudioRollOffDistance distance = AudioRollOffDistance.Far, AudioPlayMode mode = AudioPlayMode.Single);

	void Stop(AudioActorHandle handle);

	IAudioSource GetAudioSource(AudioActorHandle handle);

	IAudioMixer GetAudioMixer();
}
