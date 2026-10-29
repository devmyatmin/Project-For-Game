using System;
using System.Collections;
using System.Collections.Generic;
using Beam.Language;
using Beam.UI;
using UnityEngine;

namespace Beam;

public class PlayerSpeech : ControllerBase, IPlayerSpeech, IIdleUntilInitialized
{
	[Serializable]
	private class SpeechElement
	{
		public AudioClip Clip;

		public AudioClip ClipFemale;

		[LocalizationTermDropdown]
		[LocalizationTermPreview]
		public string Subtitle;
	}

	private PlayerSpeechViewAdapterBase _view;

	[SerializeField]
	private List<SpeechElement> _speech;

	private List<SpeechElement> _clips = new List<SpeechElement>();

	private bool _speaking;

	private IPlayer _player;

	private bool _initialized;

	public PlayerSpeechViewAdapterBase View => _view;

	public bool IsInitialized => _initialized;

	public void Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
			if (_view == null)
			{
				Debug.LogError("PlayerSpeech:: No view component found!");
				_view = NullPlayerSpeechViewAdapter.Instance;
			}
		}
	}

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as PlayerSpeechViewAdapterBase;
		if (_view == null)
		{
			Debug.LogError("PlayerSpeech:: No view component found!");
			_view = NullPlayerSpeechViewAdapter.Instance;
		}
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
	}

	public void Play(string name, int gender, bool underwater)
	{
		SpeechElement speechElement = _speech.Find((SpeechElement s) => s.Clip != null && s.Clip.name == name);
		if (speechElement != null)
		{
			_clips.Add(speechElement);
			if (!_speaking)
			{
				StartCoroutine(Speech_Sequence(gender, underwater));
			}
		}
	}

	private IEnumerator Speech_Sequence(int gender, bool underwater)
	{
		_speaking = true;
		SpeechElement speech = _clips[0];
		if (!string.IsNullOrEmpty(speech.Subtitle))
		{
			_view.SubtitlesLabel.Text = speech.Subtitle;
			_view.Show();
		}
		yield return new WaitForSeconds(0.1f);
		if ((bool)speech.Clip && (bool)speech.ClipFemale && !underwater)
		{
			AudioManager.GetAudioPlayer().Play3D((gender == 0) ? speech.Clip : speech.ClipFemale, _player.transform, AudioMixerChannels.Voice);
		}
		yield return new WaitForSeconds(7f);
		_view.Hide();
		yield return new WaitForSeconds(0.1f);
		FinishSpeaking(gender, underwater);
	}

	private void FinishSpeaking(int gender, bool underwater)
	{
		if (_clips.Count > 1)
		{
			_clips.RemoveAt(0);
			StartCoroutine(Speech_Sequence(gender, underwater));
		}
		else
		{
			_clips.Clear();
			_speaking = false;
		}
	}
}
