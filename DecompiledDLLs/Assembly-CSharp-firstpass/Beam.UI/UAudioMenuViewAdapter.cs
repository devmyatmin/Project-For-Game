using UnityEngine;

namespace Beam.UI;

public class UAudioMenuViewAdapter : ViewAdapterBase, IAudioMenuViewAdapter, IOptionsPanelView, IViewAdapter
{
	[SerializeField]
	private GameObject _audioMenuGameObject;

	[SerializeField]
	private Transform _categoriesContainer;

	[SerializeField]
	private TMPSliderViewAdapter _masterVolumeSlider;

	[SerializeField]
	private TMPSliderViewAdapter _musicVolumeSlider;

	[SerializeField]
	private TMPSliderViewAdapter _environmentVolumeSlider;

	[SerializeField]
	private TMPSliderViewAdapter _effectsVolumeSlider;

	[SerializeField]
	private TMPSliderViewAdapter _voiceVolumeSlider;

	[SerializeField]
	private TMPSliderViewAdapter _uiVolumeSlider;

	public Transform CategoriesContainer => _categoriesContainer;

	public ISliderViewAdapter MasterVolumeSlider => _masterVolumeSlider;

	public ISliderViewAdapter MusicVolumeSlider => _musicVolumeSlider;

	public ISliderViewAdapter EnvironmentVolumeSlider => _environmentVolumeSlider;

	public ISliderViewAdapter EffectsVolumeSlider => _effectsVolumeSlider;

	public ISliderViewAdapter VoiceVolumeSlider => _voiceVolumeSlider;

	public ISliderViewAdapter UiVolumeSlider => _uiVolumeSlider;

	private void Awake()
	{
		Object.FindObjectOfType<AudioMenuPresenter>().Initialize(this);
		Visible = _audioMenuGameObject.activeSelf;
	}

	public override void Show()
	{
		base.Show();
		_audioMenuGameObject.SetActive(Visible);
	}

	public override void Hide()
	{
		base.Hide();
		_audioMenuGameObject.SetActive(Visible);
	}
}
