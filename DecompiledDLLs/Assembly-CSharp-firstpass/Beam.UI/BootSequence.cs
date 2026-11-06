using System;
using System.Collections.Generic;
using Beam.Utilities;
using MEC;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Beam.UI;

public class BootSequence : SerializedMonoBehaviour
{
	[SerializeField]
	private IClipsPlayer m_player;

	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private TextScreenViewAdapterBase m_textScreen;

	private void Start()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		QualitySettings.vSyncCount = 0;
		Time.timeScale = 1f;
		Timing.RunCoroutine(Play());
	}

	private IEnumerator<float> Play()
	{
		RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, false, 0);
		RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, true, 8);
		AudioListener.volume = 0f;
		CoroutineHandle sequence = Timing.RunCoroutine(m_player.Play_Sequence());
		while (sequence.IsRunning)
		{
			if (CheckInputPressed())
			{
				m_player.Skip();
				yield return 0f;
			}
			yield return 0f;
		}
		Timing.KillCoroutines(sequence);
		yield return 0f;
		m_container.SetActive(true);
		LTDescr fadeAudio = LeanTween.value(((Component)this).gameObject, 0f, 1f, 2.7f).setOnUpdate((Action<float>)FadeAudioCallback);
		Singleton<Fader>.Instance.SetFadeColor(Color.black);
		Singleton<Fader>.Instance.FadeIn(2.7f);
		while (Singleton<Fader>.Instance.IsFading && !CheckInputPressed())
		{
			yield return 0f;
		}
		while (!CheckInputPressed())
		{
			yield return 0f;
		}
		yield return 0f;
		LeanTween.cancel(fadeAudio.id);
		fadeAudio = LeanTween.value(((Component)this).gameObject, AudioListener.volume, 0f, 2.7f).setOnUpdate((Action<float>)FadeAudioCallback);
		yield return Singleton<Fader>.Instance.FadeOut_Async(2.7f, Singleton<Fader>.Instance.CurrentValue);
		LeanTween.cancel(fadeAudio.id);
		AudioListener.volume = 0f;
		Singleton<Fader>.Instance.ResetFade(FadeMode.Solid, FadeDirection.In);
		LoadMenu();
	}

	private bool CheckInputPressed()
	{
		return Beam.Utilities.ExtensionMethods.Any_NonAlloc(ReInput.players.Players, (Player p) => p.GetButtonDown(30) || p.GetButtonDown(31) || p.GetButtonDown(39));
	}

	private void FadeAudioCallback(float value)
	{
		AudioListener.volume = Mathf.Clamp01(value);
	}

	private void LoadMenu()
	{
		m_textScreen.SetHeaderText("TEXT_LOADING");
		m_textScreen.Show();
		SceneManager.LoadScene(1);
	}
}
