using System.Collections.Generic;
using Beam;
using MEC;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FlockChildAudio : MonoBehaviour
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private AudioSource _audioSource;

	[SerializeField]
	private AudioClip[] _clips;

	private CoroutineHandle _playSequence;

	public void Play()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (!_audioSource.isPlaying)
		{
			_playSequence = Timing.RunCoroutine(Play_Sequence());
		}
	}

	private IEnumerator<float> Play_Sequence()
	{
		while (true)
		{
			_audioSource.clip = _clips[Random.Range(0, _clips.Length)];
			_audioSource.Play();
			yield return Timing.WaitForSeconds(_audioSource.clip.length + Random.Range(0.5f, 2f));
		}
	}

	public void Stop()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_playSequence);
		_audioSource.Stop();
	}
}
