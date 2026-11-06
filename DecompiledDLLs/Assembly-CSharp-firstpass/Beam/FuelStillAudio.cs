using UnityEngine;

namespace Beam;

public class FuelStillAudio : MonoBehaviour
{
	[SerializeField]
	private AudioClip _boilingAudioClip;

	[SerializeField]
	private AudioClip _collectingAudioClip;

	[SerializeField]
	private AudioClip _refilledClip;

	private IAudioPlayer _audioPlayer;

	private AudioActorHandle _boilingAudioHandle;

	private AudioActorHandle _collectingAudioHandle;

	private void Awake()
	{
		_audioPlayer = AudioManager.GetAudioPlayer();
	}

	private void OnDestroy()
	{
		_audioPlayer.Stop(_boilingAudioHandle);
		_audioPlayer.Stop(_collectingAudioHandle);
	}

	public void Still_StartedBoiling()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		_boilingAudioHandle = _audioPlayer.Play3D(_boilingAudioClip, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near, AudioPlayMode.Persistent);
		_collectingAudioHandle = _audioPlayer.Play3D(_collectingAudioClip, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.VeryNear, AudioPlayMode.Persistent);
	}

	public void Still_StoppedBoiling()
	{
		_audioPlayer.Stop(_boilingAudioHandle);
		_boilingAudioHandle = AudioActorHandle.Empty;
		_audioPlayer.Stop(_collectingAudioHandle);
		_collectingAudioHandle = AudioActorHandle.Empty;
	}

	public void Still_Refilled()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_audioPlayer.Play3D(_refilledClip, ((Component)this).transform.position, AudioMixerChannels.FX);
	}
}
