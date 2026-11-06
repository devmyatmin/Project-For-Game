using System;
using System.Collections.Generic;
using System.Linq;
using Beam.AccountServices;
using Beam.Language;
using Beam.Utilities;
using Beam.Utilities.Reflection;
using Rewired;
using UnityEngine;

namespace Beam.UI;

public class GeneralMenuPresenter : MonoBehaviour, IOptionPanel
{
	[SerializeField]
	private OptionsMenuPresenter _optionsMenuPresenter;

	private IGeneralMenuViewAdapter _view;

	private IEnumerable<IOptionElementData> _languages;

	private bool _loadedUGC;

	public Transform GetCategoriesContainer()
	{
		return _view.CategoriesContainer;
	}

	private void Awake()
	{
		GUserContentProvider.Get().LoadedUserContent += UserContentProvider_LoadedUserContent;
	}

	public void Initialize(IGeneralMenuViewAdapter view)
	{
		_view = view;
		PopulateControls();
		SetControlsSelections();
		SetControlEvents();
	}

	private void OnDestroy()
	{
		GUserContentProvider.Get().LoadedUserContent -= UserContentProvider_LoadedUserContent;
	}

	private void UserContentProvider_LoadedUserContent(LoadedContentArgs args)
	{
		if (!_loadedUGC)
		{
			PopulateLanguageControl();
			_loadedUGC = true;
		}
	}

	public void Show()
	{
		if (!_view.Visible)
		{
			_view.Show();
			_optionsMenuPresenter.SetApplyButtonEvent(GeneralMenuApplyButton_Click);
			SetControlsSelections();
		}
	}

	public void Hide()
	{
		if (_view.Visible)
		{
			_view.Hide();
		}
	}

	public void Apply()
	{
		GeneralMenuApplyButton_Click(null);
	}

	private void SetControlEvents()
	{
		WireButtonEvents(_view.HudInformationButton);
		WireButtonEvents(_view.TemperatureUnitsButton);
		WireButtonEvents(_view.CrosshairButton);
		WireButtonEvents(_view.HighlightingButton);
		WireButtonEvents(_view.TutorialButton);
		WireButtonEvents(_view.CameraViewModeButton);
		WireSliderEvents(_view.OceanRotationSlider);
		_view.LanguageButton.SelectionChanged += (IOptionButtonViewAdapter sender) =>
		{
			_optionsMenuPresenter.SetDirtySettings();
		};
	}

	private void OptionWidget_MouseEnter(ISelectableViewAdapter sender)
	{
		IOptionWidgetViewAdapter optionWidgetViewAdapter = sender as IOptionWidgetViewAdapter;
		if (!BeamTeam.IsNullOrDestroyed(optionWidgetViewAdapter))
		{
			_optionsMenuPresenter.SetTooltip(optionWidgetViewAdapter.GetTooltip());
		}
	}

	private void OptionWidget_MouseLeave(ISelectableViewAdapter sender)
	{
		_optionsMenuPresenter.SetTooltip(string.Empty);
	}

	private void OptionButton_SelectionChanged(IOptionButtonViewAdapter button)
	{
		_optionsMenuPresenter.SetTooltip(button.GetTooltip());
		_optionsMenuPresenter.SetDirtySettings();
	}

	private void OptionSlider_ValueChanged(float value)
	{
		_optionsMenuPresenter.SetDirtySettings();
	}

	private void PopulateControls()
	{
		_view.HudInformationButton.Initialize(Reflection.GetAttributes<DescriptionAttribute>(typeof(Quality_HUDInformation)));
		_view.TemperatureUnitsButton.Initialize(Reflection.GetAttributes<DescriptionAttribute>(typeof(Temperature)));
		_view.CrosshairButton.Initialize(Reflection.GetAttributes<DescriptionAttribute>(typeof(Quality_OFF_ON)));
		_view.HighlightingButton.Initialize(Reflection.GetAttributes<DescriptionAttribute>(typeof(Quality_OFF_ON)));
		_view.CameraViewModeButton.Initialize(Reflection.GetAttributes<DescriptionAttribute>(typeof(CameraViewMode)));
		_view.TutorialButton.Initialize(Reflection.GetAttributes<DescriptionAttribute>(typeof(Quality_OFF_ON)));
		PopulateLanguageControl();
	}

	private void PopulateLanguageControl()
	{
		_languages = from lang in Localization.GetLanguageHandler().GetLanguages()
			select new OptionElementData(lang.Name, lang.Path);
		_view.LanguageButton.Initialize(_languages);
	}

	private void SetControlsSelections()
	{
		_view.HudInformationButton.SelectedIndex = Reflection.GetIndex(Options.GeneralSettings.HudInformation);
		_view.TemperatureUnitsButton.SelectedIndex = Reflection.GetIndex(Options.GeneralSettings.Temperature);
		_view.CrosshairButton.SelectedIndex = Reflection.GetIndex(Options.GeneralSettings.ShowCrosshair);
		_view.HighlightingButton.SelectedIndex = Reflection.GetIndex(Options.GeneralSettings.ShowHighlighting);
		_view.TutorialButton.SelectedIndex = Reflection.GetIndex(Options.GeneralSettings.Tutorial);
		_view.CameraViewModeButton.SelectedIndex = Reflection.GetIndex(Options.GeneralSettings.CameraViewMode);
		_view.OceanRotationSlider.Value = Options.GeneralSettings.OceanRotationIntensity;
		_view.OceanRotationSlider.Refresh();
		SetLanguageControlSelection();
	}

	private void SetLanguageControlSelection()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Invalid comparison between Unknown and I4
		int num = Beam.Utilities.ExtensionMethods.FindIndex(_view.LanguageButton.Elements, (IOptionElementData r) => r.Tooltip.Equals(Options.GeneralSettings.LangPath, StringComparison.InvariantCultureIgnoreCase));
		if (num != -1)
		{
			_view.LanguageButton.SelectedIndex = num;
		}
		_view.LanguageButton.Enabled = (int)Game.State == 0;
	}

	private void GeneralMenuApplyButton_Click(object sender)
	{
		Options.GeneralSettings.HudInformation = (Quality_HUDInformation)_view.HudInformationButton.SelectedIndex;
		Options.GeneralSettings.Temperature = (Temperature)_view.TemperatureUnitsButton.SelectedIndex;
		Options.GeneralSettings.ShowCrosshair = (Quality_OFF_ON)_view.CrosshairButton.SelectedIndex;
		Options.GeneralSettings.ShowHighlighting = (Quality_OFF_ON)_view.HighlightingButton.SelectedIndex;
		Options.GeneralSettings.Tutorial = (Quality_OFF_ON)_view.TutorialButton.SelectedIndex;
		Options.GeneralSettings.CameraViewMode = (CameraViewMode)_view.CameraViewModeButton.SelectedIndex;
		Options.GeneralSettings.OceanRotationIntensity = _view.OceanRotationSlider.Value;
		Beam.Language.Language language = Localization.GetLanguageHandler().GetLanguages().ElementAt(_view.LanguageButton.SelectedIndex);
		string langPath = Options.GeneralSettings.LangPath;
		string path = language.Path;
		Options.GeneralSettings.LangPath = language.Path;
		_optionsMenuPresenter.Save();
		if (!string.IsNullOrWhiteSpace(langPath) && langPath != path)
		{
			DialogueBox.Show(ReInput.players.GetPlayer(0), "OPTIONS_GENERAL_MENU_DB_LANGUAGE_TITLE", "OPTIONS_GENERAL_MENU_DB_LANGUAGE_DESC", DialogueButton.Ok);
		}
	}

	private void WireWidgetEvents(IOptionWidgetViewAdapter widget)
	{
		widget.MouseEnter += OptionWidget_MouseEnter;
		widget.MouseLeave += OptionWidget_MouseLeave;
	}

	private void WireButtonEvents(IOptionButtonViewAdapter button)
	{
		button.SelectionChanged += OptionButton_SelectionChanged;
		WireWidgetEvents(button);
	}

	private void WireSliderEvents(IOptionSliderViewAdapter slider)
	{
		slider.ValueChanged += OptionSlider_ValueChanged;
		WireWidgetEvents(slider);
	}
}
