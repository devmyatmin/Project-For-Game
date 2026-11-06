using Beam;
using UnityEngine;
using UnityEngine.Audio;

public static class AudioTools
{
	public static void FixMixerGroups(AudioSource audioSource)
	{
		if (!((Object)(object)audioSource == (Object)null) && !((Object)(object)audioSource.outputAudioMixerGroup == (Object)null) && AudioManager.GetAudioPlayer().GetAudioMixer() is UAudioMixer uAudioMixer)
		{
			AudioMixerGroup[] array = uAudioMixer.FindMatchingGroups(((Object)audioSource.outputAudioMixerGroup).name);
			if (array != null && array.Length != 0)
			{
				audioSource.outputAudioMixerGroup = array[0];
			}
		}
	}
}
