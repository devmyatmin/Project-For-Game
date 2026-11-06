using Beam.Serialization.Json;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.Audio;

namespace Beam.UI;

public class AudioMenuPresenter : MonoBehaviour, IOptionPanel
{
	[SerializeField]
	private OptionsMenuPresenter _optionsMenuPresenter;

	[SerializeField]
	private AudioMixer _audioMixer;

	private IAudioMenuViewAdapter _view;

	private bool _initialized;

	private void Awake()
	{
		Options.Loaded += OnOptionsOnLoaded;
	}

	private void OnOptionsOnLoaded(JObject obj)
	{
		LoadMixerValues();
	}

	private void Start()
	{
		LoadMixerValues();
	}

	public void Initialize(IAudioMenuViewAdapter view)
	{
		_view = view;
		SetControlsSelections();
		SetControlEvents();
		_initialized = true;
	}

	private void OnDestroy()
	{
		if (_initialized)
		{
			ClearControlEvents();
			Options.Loaded -= OnOptionsOnLoaded;
		}
	}

	public Transform GetCategoriesContainer()
	{
		return _view.CategoriesContainer;
	}

	public void Show()
	{
		_view.Show();
		_optionsMenuPresenter.SetApplyButtonEvent(AudioMenuApplyButton_Click);
		SetControlsSelections();
	}

	public void Hide()
	{
		if (_view.Visible)
		{
			LoadMixerValues();
			_view.Hide();
		}
	}

	public void Apply()
	{
		AudioMenuApplyButton_Click(null);
	}

	private void SetControlEvents()
	{
		_view.MasterVolumeSlider.ValueChanged += MasterVolume_Change;
		_view.MusicVolumeSlider.ValueChanged += MusicVolume_Change;
		_view.EnvironmentVolumeSlider.ValueChanged += EnvironmentVolume_Change;
		_view.EffectsVolumeSlider.ValueChanged += EffectsVolume_Change;
		_view.VoiceVolumeSlider.ValueChanged += VoiceVolume_Change;
		_view.UiVolumeSlider.ValueChanged += UIVolume_Change;
	}

	private void SetControlsSelections()
	{
		_view.MasterVolumeSlider.Value = Options.AudioSettings.MasterVolume;
		_view.MusicVolumeSlider.Value = Options.AudioSettings.MusicVolume;
		_view.EnvironmentVolumeSlider.Value = Options.AudioSettings.EnvironmentVolume;
		_view.EffectsVolumeSlider.Value = Options.AudioSettings.EffectsVolume;
		_view.VoiceVolumeSlider.Value = Options.AudioSettings.VoiceVolume;
		_view.UiVolumeSlider.Value = Options.AudioSettings.UIVolume;
		CheckVolumeSlider(_view.MasterVolumeSlider, _view.MasterVolumeSlider.Value);
		CheckVolumeSlider(_view.MusicVolumeSlider, _view.MusicVolumeSlider.Value);
		CheckVolumeSlider(_view.EnvironmentVolumeSlider, _view.EnvironmentVolumeSlider.Value);
		CheckVolumeSlider(_view.EffectsVolumeSlider, _view.EffectsVolumeSlider.Value);
		CheckVolumeSlider(_view.VoiceVolumeSlider, _view.VoiceVolumeSlider.Value);
		CheckVolumeSlider(_view.UiVolumeSlider, _view.UiVolumeSlider.Value);
		LoadMixerValues();
	}

	public void LoadMixerValues()
	{
		SetVolume(AudioMixerChannels.Master, Options.AudioSettings.MasterVolume);
		SetVolume(AudioMixerChannels.Music, Options.AudioSettings.MusicVolume);
		SetVolume(AudioMixerChannels.Environment, Options.AudioSettings.EnvironmentVolume);
		SetVolume(AudioMixerChannels.FX, Options.AudioSettings.EffectsVolume);
		SetVolume(AudioMixerChannels.Voice, Options.AudioSettings.VoiceVolume);
		SetVolume(AudioMixerChannels.UI, Options.AudioSettings.UIVolume);
	}

	public void SetVolume(AudioMixerChannels channel, float normalizedVolume)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Game.State == 0)
		{
			string text = channel.ToString() + "Volume";
			_audioMixer.SetFloat(text, BeamTeam.LinearToDecibel(normalizedVolume));
		}
		else
		{
			AudioManager.GetAudioPlayer().GetAudioMixer().GetChannel(channel)
				.Volume = normalizedVolume;
		}
	}

	private void ClearControlEvents()
	{
		_view.MasterVolumeSlider.ValueChanged -= MasterVolume_Change;
		_view.MusicVolumeSlider.ValueChanged -= MusicVolume_Change;
		_view.EnvironmentVolumeSlider.ValueChanged -= EnvironmentVolume_Change;
		_view.EffectsVolumeSlider.ValueChanged -= EffectsVolume_Change;
		_view.VoiceVolumeSlider.ValueChanged -= VoiceVolume_Change;
		_view.UiVolumeSlider.ValueChanged -= UIVolume_Change;
	}

	private void AudioMenuApplyButton_Click(object sender)
	{
		Options.AudioSettings.MasterVolume = _view.MasterVolumeSlider.Value;
		Options.AudioSettings.MusicVolume = _view.MusicVolumeSlider.Value;
		Options.AudioSettings.EnvironmentVolume = _view.EnvironmentVolumeSlider.Value;
		Options.AudioSettings.EffectsVolume = _view.EffectsVolumeSlider.Value;
		Options.AudioSettings.VoiceVolume = _view.VoiceVolumeSlider.Value;
		Options.AudioSettings.UIVolume = _view.UiVolumeSlider.Value;
		_optionsMenuPresenter.Save();
	}

	private void CheckVolumeSlider(ISliderViewAdapter slider, float value)
	{
		float num = value * 100f;
		slider.ValueText = $"{num:f0}%";
	}

	private void MasterVolume_Change(float value)
	{
		CheckVolumeSlider(_view.MasterVolumeSlider, value);
		SetVolume(AudioMixerChannels.Master, value);
		_optionsMenuPresenter.SetDirtySettings();
	}

	private void MusicVolume_Change(float value)
	{
		CheckVolumeSlider(_view.MusicVolumeSlider, value);
		SetVolume(AudioMixerChannels.Music, value);
		_optionsMenuPresenter.SetDirtySettings();
	}

	private void EnvironmentVolume_Change(float value)
	{
		CheckVolumeSlider(_view.EnvironmentVolumeSlider, value);
		SetVolume(AudioMixerChannels.Environment, value);
		_optionsMenuPresenter.SetDirtySettings();
	}

	private void EffectsVolume_Change(float value)
	{
		CheckVolumeSlider(_view.EffectsVolumeSlider, value);
		SetVolume(AudioMixerChannels.FX, value);
		_optionsMenuPresenter.SetDirtySettings();
	}

	private void VoiceVolume_Change(float value)
	{
		CheckVolumeSlider(_view.VoiceVolumeSlider, value);
		SetVolume(AudioMixerChannels.Voice, value);
		_optionsMenuPresenter.SetDirtySettings();
	}

	private void UIVolume_Change(float value)
	{
		CheckVolumeSlider(_view.UiVolumeSlider, value);
		SetVolume(AudioMixerChannels.UI, value);
		_optionsMenuPresenter.SetDirtySettings();
	}
}
