namespace Beam.UI;

public interface IGraphicsMenuViewAdapter : IOptionsPanelView, IViewAdapter
{
	IScrollviewViewAdapter Scrollview { get; }

	IOptionButtonViewAdapter PresetButton { get; }

	IOptionButtonViewAdapter ResolutionButton { get; }

	IOptionButtonViewAdapter FullScreenButton { get; }

	IOptionButtonViewAdapter FixedFrameRateButton { get; }

	IOptionButtonViewAdapter VerticalSyncButton { get; }

	IOptionButtonViewAdapter ColorModeButton { get; }

	IOptionButtonViewAdapter DrawDistanceButton { get; }

	IOptionButtonViewAdapter TextureQualityButton { get; }

	IOptionButtonViewAdapter ShadowQualityButton { get; }

	IOptionButtonViewAdapter BillboardDensityButton { get; }

	IOptionButtonViewAdapter GrassButton { get; }

	IOptionButtonViewAdapter CloudsButton { get; }

	IOptionButtonViewAdapter WaterQualityButton { get; }

	IOptionButtonViewAdapter WaterReflectionsButton { get; }

	IOptionButtonViewAdapter CausticsButton { get; }

	IOptionButtonViewAdapter AntialiasingButton { get; }

	IOptionButtonViewAdapter AmbientOcclusionButton { get; }

	IOptionButtonViewAdapter BloomButton { get; }

	IOptionButtonViewAdapter LensEffectsButton { get; }

	IOptionButtonViewAdapter MotionBlurButton { get; }

	ISliderViewAdapter FieldOfViewSlider { get; }
}
