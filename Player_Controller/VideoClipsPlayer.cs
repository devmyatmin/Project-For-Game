using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Beam.UI;

public class VideoClipsPlayer : IClipsPlayer, IDisposable
{
	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private VideoPlayer m_player;

	[SerializeField]
	public VideoClip[] m_clips;

	[SerializeField]
	private AudioSource m_audioSource;

	private bool m_skip;

	public IEnumerator<float> Play_Sequence()
	{
		m_container.SetActive(value: true);
		VideoClip[] clips = m_clips;
		foreach (VideoClip clip in clips)
		{
			m_player.clip = clip;
			m_player.audioOutputMode = VideoAudioOutputMode.AudioSource;
			m_player.EnableAudioTrack(0, enabled: true);
			m_player.SetTargetAudioSource(0, m_audioSource);
			m_player.Prepare();
			while (!m_player.isPrepared)
			{
				yield return 0f;
			}
			m_player.Play();
			while (m_player.isPlaying)
			{
				if (m_skip)
				{
					m_player.Stop();
					m_skip = false;
					break;
				}
				yield return 0f;
			}
		}
		Dispose();
	}

	public void Skip()
	{
		m_skip = true;
	}

	public void Dispose()
	{
		m_container.SetActive(value: false);
		m_player.Stop();
		m_player.clip = null;
	}
}
