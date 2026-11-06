using System;
using System.Collections.Generic;
using Beam.Utilities;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

public class UOptionsMenuViewAdapter : OptionsMenuViewAdapterBase
{
	[SerializeField]
	private GameObject _optionsMenuPanel;

	[SerializeField]
	private UScrollbarViewAdapter _scrollBar;

	[SerializeField]
	private UButtonViewAdapter _generalButton;

	[SerializeField]
	private UButtonViewAdapter _graphicsButton;

	[SerializeField]
	private UButtonViewAdapter _audioButton;

	[SerializeField]
	private UButtonViewAdapter _inputButton;

	[SerializeField]
	private UButtonViewAdapter _aboutButton;

	[SerializeField]
	private UButtonViewAdapter _backButton;

	[SerializeField]
	private UButtonViewAdapter _applyButton;

	[SerializeField]
	private TMPOptionCategoryHeader _optionCategoryHeaderPrefab;

	[SerializeField]
	private UCanvasGroupViewAdapter _tooltipCanvasGroup;

	[SerializeField]
	private TMPLabelViewAdapter _tooltipLabel;

	private List<IOptionCategoryHeader> _optionCategoryHeaders = new List<IOptionCategoryHeader>();

	private string _tooltip;

	public override IScrollbarViewAdapter Scrollbar => _scrollBar;

	public override IButtonViewAdapter GeneralButton => _generalButton;

	public override IButtonViewAdapter GraphicsButton => _graphicsButton;

	public override IButtonViewAdapter AudioButton => _audioButton;

	public override IButtonViewAdapter InputButton => _inputButton;

	public override IButtonViewAdapter AboutButton => _aboutButton;

	public override IButtonViewAdapter BackButton => _backButton;

	public override IButtonViewAdapter ApplyButton => _applyButton;

	public override void SetInput(Player input)
	{
		base.SetInput(input);
		if (Input != null)
		{
			RewiredUtilities.SetUserInterfaceFromInput(((Component)((Component)this).transform).GetComponentsInChildren<Selectable>(true), Input);
		}
	}

	public override IButtonViewAdapter GetFirstButton()
	{
		if (BeamTeam.IsNullOrDestroyed(CurrentPanelButton))
		{
			return _generalButton;
		}
		return CurrentPanelButton;
	}

	private void Awake()
	{
		Visible = _optionsMenuPanel.activeSelf;
		ClearTooltip();
	}

	public override void Show()
	{
		ClearTooltip();
		_optionsMenuPanel.gameObject.SetActive(true);
		base.Show();
	}

	public override void Hide()
	{
		_optionsMenuPanel.gameObject.SetActive(false);
		base.Hide();
	}

	public override IOptionCategoryHeader CreateOptionCategoryHeader(IOptionPanel panel)
	{
		TMPOptionCategoryHeader tMPOptionCategoryHeader = Object.Instantiate<TMPOptionCategoryHeader>(_optionCategoryHeaderPrefab);
		((Component)tMPOptionCategoryHeader).transform.SetParent(panel.GetCategoriesContainer(), false);
		return tMPOptionCategoryHeader;
	}

	public override void DestroyOptionCategoryHeaders()
	{
		for (int i = 0; i < _optionCategoryHeaders.Count; i++)
		{
			BeamTeam.Destroy<GameObject>(((Component)_optionCategoryHeaders[i].transform).gameObject);
		}
		_optionCategoryHeaders.Clear();
	}

	private void ClearTooltip()
	{
		_tooltipLabel.Text = string.Empty;
		_tooltipCanvasGroup.Alpha = 0f;
	}

	public override void SetTooltip(string tooltip)
	{
		_tooltip = tooltip;
		LeanTween.cancel(((Component)this).gameObject);
		FadeTooltipOut();
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
}
