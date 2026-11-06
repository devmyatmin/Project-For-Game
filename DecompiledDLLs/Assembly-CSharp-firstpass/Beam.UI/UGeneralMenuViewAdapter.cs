using UnityEngine;

namespace Beam.UI;

public class UGeneralMenuViewAdapter : ViewAdapterBase, IGeneralMenuViewAdapter, IOptionsPanelView, IViewAdapter
{
	[SerializeField]
	private GameObject _generalMenuPanel;

	[SerializeField]
	private Transform _categoriesContainer;

	[SerializeField]
	private TMPOptionButtonViewAdapter _hudInformationButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _crosshairButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _highlightingButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _temperatureUnitsButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _tutorialButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _cameraViewModeButton;

	[SerializeField]
	private TMPOptionSliderViewAdapter _oceanRotationSlider;

	[SerializeField]
	private TMPOptionButtonViewAdapter _languageButton;

	public Transform CategoriesContainer => _categoriesContainer;

	public IOptionButtonViewAdapter HudInformationButton => _hudInformationButton;

	public IOptionButtonViewAdapter CrosshairButton => _crosshairButton;

	public IOptionButtonViewAdapter HighlightingButton => _highlightingButton;

	public IOptionButtonViewAdapter TemperatureUnitsButton => _temperatureUnitsButton;

	public IOptionButtonViewAdapter TutorialButton => _tutorialButton;

	public IOptionButtonViewAdapter CameraViewModeButton => _cameraViewModeButton;

	public IOptionSliderViewAdapter OceanRotationSlider => _oceanRotationSlider;

	public IOptionButtonViewAdapter LanguageButton => _languageButton;

	private void Awake()
	{
		Object.FindObjectOfType<GeneralMenuPresenter>().Initialize(this);
		Visible = _generalMenuPanel.activeSelf;
	}

	public override void Show()
	{
		base.Show();
		_generalMenuPanel.SetActive(Visible);
	}

	public override void Hide()
	{
		base.Hide();
		_generalMenuPanel.SetActive(Visible);
	}
}
