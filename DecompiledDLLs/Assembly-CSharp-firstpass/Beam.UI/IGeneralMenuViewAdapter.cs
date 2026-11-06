namespace Beam.UI;

public interface IGeneralMenuViewAdapter : IOptionsPanelView, IViewAdapter
{
	IOptionButtonViewAdapter HudInformationButton { get; }

	IOptionButtonViewAdapter CrosshairButton { get; }

	IOptionButtonViewAdapter HighlightingButton { get; }

	IOptionButtonViewAdapter TemperatureUnitsButton { get; }

	IOptionButtonViewAdapter TutorialButton { get; }

	IOptionButtonViewAdapter CameraViewModeButton { get; }

	IOptionSliderViewAdapter OceanRotationSlider { get; }

	IOptionButtonViewAdapter LanguageButton { get; }
}
