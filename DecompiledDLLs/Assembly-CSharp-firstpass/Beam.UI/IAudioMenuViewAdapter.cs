namespace Beam.UI;

public interface IAudioMenuViewAdapter : IOptionsPanelView, IViewAdapter
{
	ISliderViewAdapter MasterVolumeSlider { get; }

	ISliderViewAdapter MusicVolumeSlider { get; }

	ISliderViewAdapter EnvironmentVolumeSlider { get; }

	ISliderViewAdapter EffectsVolumeSlider { get; }

	ISliderViewAdapter VoiceVolumeSlider { get; }

	ISliderViewAdapter UiVolumeSlider { get; }
}
