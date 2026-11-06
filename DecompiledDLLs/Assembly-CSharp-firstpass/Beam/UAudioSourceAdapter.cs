using UnityEngine;

namespace Beam;

[RequireComponent(typeof(AudioSource))]
public class UAudioSourceAdapter : MonoBehaviour, IAudioSourceAdapter
{
	private AudioSource _audioSource;

	protected AudioSource AudioSource
	{
		get
		{
			if ((Object)(object)_audioSource == (Object)null)
			{
				_audioSource = ((Component)this).GetComponent<AudioSource>();
			}
			return _audioSource;
		}
	}

	public void Play()
	{
		_audioSource.Play();
	}

	public void Stop()
	{
		_audioSource.Stop();
	}
}
