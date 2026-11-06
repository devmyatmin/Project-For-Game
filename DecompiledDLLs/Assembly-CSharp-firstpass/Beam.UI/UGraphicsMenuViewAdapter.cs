using UnityEngine;

namespace Beam.UI;

public class UGraphicsMenuViewAdapter : ViewAdapterBase, IGraphicsMenuViewAdapter, IOptionsPanelView, IViewAdapter
{
	[SerializeField]
	private GameObject _graphicsMenuGameObject;

	[SerializeField]
	private Transform _categoriesContainer;

	[SerializeField]
	private UScrollviewViewAdapter _scrollview;

	[SerializeField]
	private TMPOptionButtonViewAdapter _presetButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _resolutionButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _colorModeButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _fullScreenButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _fixedFrameRateButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _verticalSyncButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _drawDistanceButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _textureQualityButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _shadowQualityButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _billboardDensityButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _grassButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _cloudsButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _cloudsOcclusionButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _waterQualityButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _waterReflectionsButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _causticsButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _antialiasingButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _ambientOcclusionButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _bloomButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _lensEffectsButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _motionBlurButton;

	[SerializeField]
	private TMPSliderViewAdapter _fieldOfViewSlider;

	public Transform CategoriesContainer => _categoriesContainer;

	public IScrollviewViewAdapter Scrollview => _scrollview;

	public IOptionButtonViewAdapter PresetButton => _presetButton;

	public IOptionButtonViewAdapter ResolutionButton => _resolutionButton;

	public IOptionButtonViewAdapter ColorModeButton => _colorModeButton;

	public IOptionButtonViewAdapter FullScreenButton => _fullScreenButton;

	public IOptionButtonViewAdapter FixedFrameRateButton => _fixedFrameRateButton;

	public IOptionButtonViewAdapter VerticalSyncButton => _verticalSyncButton;

	public IOptionButtonViewAdapter DrawDistanceButton => _drawDistanceButton;

	public IOptionButtonViewAdapter TextureQualityButton => _textureQualityButton;

	public IOptionButtonViewAdapter ShadowQualityButton => _shadowQualityButton;

	public IOptionButtonViewAdapter BillboardDensityButton => _billboardDensityButton;

	public IOptionButtonViewAdapter GrassButton => _grassButton;

	public IOptionButtonViewAdapter CloudsButton => _cloudsButton;

	public IOptionButtonViewAdapter CloudsOcclusionButton => _cloudsOcclusionButton;

	public IOptionButtonViewAdapter WaterQualityButton => _waterQualityButton;

	public IOptionButtonViewAdapter WaterReflectionsButton => _waterReflectionsButton;

	public IOptionButtonViewAdapter CausticsButton => _causticsButton;

	public IOptionButtonViewAdapter AntialiasingButton => _antialiasingButton;

	public IOptionButtonViewAdapter AmbientOcclusionButton => _ambientOcclusionButton;

	public IOptionButtonViewAdapter BloomButton => _bloomButton;

	public IOptionButtonViewAdapter LensEffectsButton => _lensEffectsButton;

	public IOptionButtonViewAdapter MotionBlurButton => _motionBlurButton;

	public ISliderViewAdapter FieldOfViewSlider => _fieldOfViewSlider;

	private void Awake()
	{
		Object.FindObjectOfType<GraphicsMenuPresenter>().Initialize(this);
		Visible = _graphicsMenuGameObject.activeSelf;
	}

	public override void Show()
	{
		base.Show();
		_graphicsMenuGameObject.SetActive(Visible);
	}

	public override void Hide()
	{
		base.Hide();
		_graphicsMenuGameObject.SetActive(Visible);
	}
}
