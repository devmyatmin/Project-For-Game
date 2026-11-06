using UnityEngine;

namespace Beam;

public class WaterStillAudio : MonoBehaviour
{
	[SerializeField]
	private AudioClip _collectingAudioClip;

	[SerializeField]
	private AudioClip[] _refilledClip;

	[SerializeField]
	private AudioClip[] _collectedClip;

	[SerializeField]
	private AudioClip _drinkAudioClip;

	[SerializeField]
	private AudioClip _waterRemovedAudioClip;

	private IAudioPlayer _audioPlayer;

	private AudioActorHandle _collectingAudioHandle;

	private void Awake()
	{
		_audioPlayer = AudioManager.GetAudioPlayer();
	}

	private void OnDestroy()
	{
		_audioPlayer.Stop(_collectingAudioHandle);
	}

	public void Still_StartedCollecting()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		_collectingAudioHandle = _audioPlayer.Play3D(_collectingAudioClip, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.VeryNear, AudioPlayMode.Persistent);
	}

	public void Still_StoppedCollecting()
	{
		_audioPlayer.Stop(_collectingAudioHandle);
		_collectingAudioHandle = AudioActorHandle.Empty;
	}

	public void Still_Refilled()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_audioPlayer.Play3D(_refilledClip, ((Component)this).transform.position, AudioMixerChannels.FX);
	}

	public void Still_Collected()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_audioPlayer.Play3D(_collectedClip, ((Component)this).transform.position, AudioMixerChannels.FX);
	}

	public void Still_Drank()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_audioPlayer.Play3D(_drinkAudioClip, ((Component)this).transform.position, AudioMixerChannels.FX);
	}

	public void Still_WaterRemoved()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_audioPlayer.Play3D(_waterRemovedAudioClip, ((Component)this).transform.position, AudioMixerChannels.FX);
	}
}
