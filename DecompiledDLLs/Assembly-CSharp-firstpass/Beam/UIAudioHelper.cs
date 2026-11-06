using UnityEngine;

namespace Beam;

public class UIAudioHelper : MonoBehaviour
{
	public void PlaySingle2D(AudioClip clip)
	{
		AudioManager.GetAudioPlayer().Play2D(clip, AudioMixerChannels.UI);
	}
}
