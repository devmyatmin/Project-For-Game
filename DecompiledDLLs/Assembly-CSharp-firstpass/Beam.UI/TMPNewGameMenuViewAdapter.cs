using System;
using UnityEngine;

namespace Beam.UI;

public class TMPNewGameMenuViewAdapter : NewGameMenuViewAdapterBase
{
	[SerializeField]
	private GameObject _newGameViewGameObject;

	[SerializeField]
	private TMPOptionButtonViewAdapter _modeOptionButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _difficultyOptionButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _worldOptionButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _genderOptionButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _gender2OptionButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _permadeathOptionButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _wildlifeOptionButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _startingCrateOptionButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _kickButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _startButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _backButton;

	[SerializeField]
	private UCanvasGroupViewAdapter _tooltipCanvasGroup;

	[SerializeField]
	private TMPLabelViewAdapter _tooltipLabel;

	[SerializeField]
	private TMPLabelViewAdapter _titleLabel;

	[SerializeField]
	private GameObject _lobbyDetails;

	[SerializeField]
	private UButtonViewAdapter _copyCodeButton;

	[SerializeField]
	private TMPLabelViewAdapter _lobbyNameLabel;

	[SerializeField]
	private TMPLabelViewAdapter _regionLabel;

	[SerializeField]
	private TMPLabelViewAdapter _pingLabel;

	[SerializeField]
	private TMPLabelViewAdapter _codeLabel;

	private string _tooltip;

	public override IOptionButtonViewAdapter ModeOptionButton => _modeOptionButton;

	public override IOptionButtonViewAdapter DifficultyOptionButton => _difficultyOptionButton;

	public override IOptionButtonViewAdapter WorldOptionButton => _worldOptionButton;

	public override IOptionButtonViewAdapter GenderOptionButton => _genderOptionButton;

	public override IOptionButtonViewAdapter Gender2OptionButton => _gender2OptionButton;

	public override IOptionButtonViewAdapter PermadeathOptionButton => _permadeathOptionButton;

	public override IOptionButtonViewAdapter WildlifeOptionButton => _wildlifeOptionButton;

	public override IOptionButtonViewAdapter StartingCrateOptionButton => _startingCrateOptionButton;

	public override ILabelButtonViewAdapter KickButton => _kickButton;

	public override ILabelButtonViewAdapter StartButton => _startButton;

	public override ILabelButtonViewAdapter BackButton => _backButton;

	public override ILabelViewAdapter TitleLabel => _titleLabel;

	public override GameObject LobbyDetails => _lobbyDetails;

	public override IButtonViewAdapter CopyCodeButton => _copyCodeButton;

	public override ILabelViewAdapter LobbyNameLabel => _lobbyNameLabel;

	public override ILabelViewAdapter RegionLabel => _regionLabel;

	public override ILabelViewAdapter PingLabel => _pingLabel;

	public override ILabelViewAdapter CodeLabel => _codeLabel;

	public override IButtonViewAdapter GetFirstButton()
	{
		return _backButton;
	}

	private void Awake()
	{
		Visible = _newGameViewGameObject.activeSelf;
		ClearTooltip();
	}

	public override void Show()
	{
		if (!_newGameViewGameObject.activeSelf)
		{
			ClearTooltip();
			_newGameViewGameObject.SetActive(true);
			Visible = true;
			OnViewShow();
		}
	}

	public override void Hide()
	{
		if (_newGameViewGameObject.activeSelf)
		{
			Visible = false;
			OnViewHide();
			_newGameViewGameObject.SetActive(false);
		}
	}

	private void ClearTooltip()
	{
		_tooltipLabel.Text = string.Empty;
		_tooltipCanvasGroup.Alpha = 0f;
	}

	public override void SetTooltip(string tooltip)
	{
		if (string.IsNullOrEmpty(_tooltip) || !(_tooltip == tooltip))
		{
			_tooltip = tooltip;
			LeanTween.cancel(((Component)this).gameObject);
			FadeTooltipOut();
		}
	}

	private void FadeTooltipOut()
	{
		float alpha = _tooltipCanvasGroup.Alpha;
		LTDescr val = LeanTween.value(((Component)this).gameObject, (Action<float>)FadeCallback, alpha, 0f, alpha * 0.15f).setIgnoreTimeScale(true);
		if (!string.IsNullOrEmpty(_tooltip))
		{
			val.setOnComplete((Action)FadeTooltipIn);
		}
	}

	private void FadeTooltipIn()
	{
		_tooltipLabel.Text = _tooltip;
		LeanTween.value(((Component)this).gameObject, (Action<float>)FadeCallback, 0f, 1f, 0.15f).setIgnoreTimeScale(true);
	}

	private void FadeCallback(float fade)
	{
		_tooltipCanvasGroup.Alpha = fade;
	}

	public void ToggleHostButtons(bool state)
	{
		ModeOptionButton.Interactable = state;
		DifficultyOptionButton.Interactable = state;
		WorldOptionButton.Interactable = state;
		GenderOptionButton.Interactable = state;
		PermadeathOptionButton.Interactable = state;
		WildlifeOptionButton.Interactable = state;
		StartingCrateOptionButton.Interactable = state;
		CopyCodeButton.Interactable = state;
		KickButton.Interactable = state;
		StartButton.Interactable = state;
		BackButton.Interactable = state;
	}
}
