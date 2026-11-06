using System;
using UnityEngine;

namespace Beam.UI;

public class TMPCreateSessionMenuViewAdapter : CreateSessionMenuViewAdapterBase
{
	[SerializeField]
	private GameObject _createSessionViewGameObject;

	[SerializeField]
	private TMPInputFieldViewAdapter _inputField;

	[SerializeField]
	private TMPOptionButtonViewAdapter _visibilityOptionButton;

	[SerializeField]
	private TMPOptionButtonViewAdapter _regionOptionButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _createButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _backButton;

	[SerializeField]
	private UCanvasGroupViewAdapter _tooltipCanvasGroup;

	[SerializeField]
	private TMPLabelViewAdapter _tooltipLabel;

	private string _tooltip;

	public override IInputFieldViewAdapter InputField => _inputField;

	public override IOptionButtonViewAdapter VisibilityOptionButton => _visibilityOptionButton;

	public override IOptionButtonViewAdapter RegionOptionButton => _regionOptionButton;

	public override ILabelButtonViewAdapter CreateButton => _createButton;

	public override ILabelButtonViewAdapter BackButton => _backButton;

	public override IButtonViewAdapter GetFirstButton()
	{
		return _visibilityOptionButton;
	}

	private void Awake()
	{
		Visible = _createSessionViewGameObject.activeSelf;
		ClearTooltip();
	}

	public override void Show()
	{
		if (!_createSessionViewGameObject.activeSelf)
		{
			SetAllButtonInteractability(state: true);
			ClearTooltip();
			_createSessionViewGameObject.SetActive(true);
			Visible = true;
			OnViewShow();
		}
	}

	public override void Hide()
	{
		if (_createSessionViewGameObject.activeSelf)
		{
			Visible = false;
			OnViewHide();
			_createSessionViewGameObject.SetActive(false);
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

	public override void SetAllButtonInteractability(bool state)
	{
		VisibilityOptionButton.Interactable = state;
		RegionOptionButton.Interactable = state;
		CreateButton.Interactable = state;
		BackButton.Interactable = state;
	}
}
