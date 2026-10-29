using Beam;
using UnityEngine;

public class NullAudioPlayer : IAudioPlayer
{
	public static NullAudioPlayer Instance { get; } = new NullAudioPlayer();


	public AudioActorHandle Play2D(AudioClip clip, AudioMixerChannels channel, AudioPlayMode mode = AudioPlayMode.Single)
	{
		return AudioActorHandle.Empty;
	}

	public AudioActorHandle Play3D(AudioClip clip, Vector3 position, AudioMixerChannels channel, AudioRollOffDistance distance = AudioRollOffDistance.Far, AudioPlayMode mode = AudioPlayMode.Single)
	{
		return AudioActorHandle.Empty;
	}

	public AudioActorHandle Play3D(AudioClip[] clips, Vector3 position, AudioMixerChannels channel, AudioRollOffDistance distance = AudioRollOffDistance.Far, AudioPlayMode mode = AudioPlayMode.Single)
	{
		return AudioActorHandle.Empty;
	}

	public AudioActorHandle Play3D(AudioClip clip, Transform target, AudioMixerChannels channel, AudioRollOffDistance distance = AudioRollOffDistance.Far, AudioPlayMode mode = AudioPlayMode.Single)
	{
		return AudioActorHandle.Empty;
	}

	public void Stop(AudioActorHandle handle)
	{
	}

	public IAudioSource GetAudioSource(AudioActorHandle handle)
	{
		return NullAudioSource.Instance;
	}

	public IAudioMixer GetAudioMixer()
	{
		return NullObject<NullAudioMixer>.Instance;
	}
}
