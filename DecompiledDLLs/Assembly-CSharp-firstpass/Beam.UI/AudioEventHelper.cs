using UnityEngine;

namespace Beam.UI;

public class AudioEventHelper : MonoBehaviour
{
	[SerializeField]
	private AudioSource _audio;

	public void Play(AudioClip clip)
	{
		_audio.Stop();
		_audio.clip = clip;
		_audio.Play();
	}
}
