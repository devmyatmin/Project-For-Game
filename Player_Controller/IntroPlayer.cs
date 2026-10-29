using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace Beam.UI;

public class IntroPlayer : IClipsPlayer, IDisposable
{
	public class IntroSequence : IDisposable
	{
		[SerializeField]
		private GameObject m_container;

		[SerializeField]
		private float m_displayTime;

		public IEnumerator<float> Play_Sequence()
		{
			m_container.SetActive(value: true);
			Singleton<Fader>.Instance.SetFadeColor(Color.black);
			yield return Singleton<Fader>.Instance.FadeIn_Async(1f);
			yield return Timing.WaitForSeconds(m_displayTime);
			yield return Singleton<Fader>.Instance.FadeOut_Async(1f);
			Dispose();
		}

		public void Dispose()
		{
			m_container.SetActive(value: false);
		}
	}

	[SerializeField]
	private IntroSequence[] m_sequences;

	private IntroSequence m_sequence;

	private CoroutineHandle m_handle;

	private bool m_skip;

	public IEnumerator<float> Play_Sequence()
	{
		IntroSequence[] sequences = m_sequences;
		foreach (IntroSequence sequence in sequences)
		{
			m_sequence = sequence;
			m_handle = Timing.RunCoroutine(m_sequence.Play_Sequence());
			while (m_handle.IsRunning)
			{
				if (m_skip)
				{
					Dispose();
					m_skip = false;
					break;
				}
				yield return 0f;
			}
		}
	}

	public void Skip()
	{
		m_skip = true;
	}

	public void Dispose()
	{
		Timing.KillCoroutines(m_handle);
		m_sequence?.Dispose();
		m_sequence = null;
	}
}
