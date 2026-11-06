using UnityEngine;

namespace Beam;

public class AmbientAudioHelper : MonoBehaviour
{
	[SerializeField]
	private AudioClip _audioClip;

	[SerializeField]
	private AudioRollOffDistance _distance = AudioRollOffDistance.Near;

	private AudioActorHandle _handle;

	private void Start()
	{
		_handle = AudioManager.GetAudioPlayer().Play3D(_audioClip, ((Component)this).transform, AudioMixerChannels.FX, _distance, AudioPlayMode.Persistent);
	}

	private void OnDestroy()
	{
		Stop();
	}

	public void Stop()
	{
		AudioManager.GetAudioPlayer().Stop(_handle);
		_handle = AudioActorHandle.Empty;
	}
}
