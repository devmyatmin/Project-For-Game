using UnityEngine;

namespace Beam;

public class PlotAudio : MonoBehaviour
{
	[SerializeField]
	private AudioClip[] _plantedAudioClips;

	[SerializeField]
	private AudioClip _wateredAudioClip;

	[SerializeField]
	private AudioClip _clearedAudioClip;

	private IAudioPlayer _audioPlayer;

	private void Awake()
	{
		_audioPlayer = AudioManager.GetAudioPlayer();
	}

	public void Plot_Planted()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_audioPlayer.Play3D(_plantedAudioClips, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
	}

	public void Plot_Watered()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_audioPlayer.Play3D(_wateredAudioClip, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
	}

	public void Plot_Cleared()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_audioPlayer.Play3D(_clearedAudioClip, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
	}
}
