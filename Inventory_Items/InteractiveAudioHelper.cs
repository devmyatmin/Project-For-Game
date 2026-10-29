using UnityEngine;
using UnityEngine.Serialization;

namespace Beam;

public class InteractiveAudioHelper : MonoBehaviour
{
	[Space]
	[SerializeField]
	private ClipSelectionMode _clipSelectionMode;

	[Header("DEFAULT AUDIO CLIP RANGE")]
	[FormerlySerializedAs("_emptyClip")]
	[SerializeField]
	private AudioClip[] _audioClips;

	private int _i;

	protected ClipSelectionMode ClipSelectionMode => _clipSelectionMode;

	protected AudioClip[] AudioClips => _audioClips;

	public void PlaySingle2D(AudioClip clip)
	{
		AudioManager.GetAudioPlayer().Play2D(clip, AudioMixerChannels.FX);
	}

	public void PlaySingle3D(AudioClip clip)
	{
		AudioManager.GetAudioPlayer().Play3D(clip, base.transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
	}

	public virtual void PlayRange(IBase sender, IBaseActionEventData data)
	{
		if (_audioClips == null || _audioClips.Length == 0)
		{
			return;
		}
		AudioClip clip;
		if (_clipSelectionMode == ClipSelectionMode.InOrder)
		{
			clip = _audioClips[_i++];
			if (_i > _audioClips.Length)
			{
				_i = 0;
			}
		}
		else
		{
			clip = _audioClips[Random.Range(0, _audioClips.Length)];
		}
		AudioManager.GetAudioPlayer().Play3D(clip, data.Position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
	}
}
