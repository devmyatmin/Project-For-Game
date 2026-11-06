using UnityEngine;

namespace Beam;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceHelper : MonoBehaviour
{
	private AudioSource _audio;

	protected AudioSource Audio
	{
		get
		{
			if ((Object)(object)_audio == (Object)null)
			{
				_audio = ((Component)this).GetComponent<AudioSource>();
			}
			return _audio;
		}
	}

	public void PlaySound(AudioClip clip)
	{
		if (!((Object)(object)clip == (Object)null))
		{
			StopAudio();
			PlayClip(clip);
		}
	}

	public void PlaySoundRange(AudioClip[] clips)
	{
		if (clips != null && clips.Length != 0)
		{
			StopAudio();
			PlayClip(clips[Random.Range(0, clips.Length)]);
		}
	}

	private void PlayClip(AudioClip clip)
	{
		Audio.clip = clip;
		Audio.Play();
	}

	public void StopAudio()
	{
		Audio.Stop();
	}
}
