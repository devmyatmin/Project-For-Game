using System;
using UnityEngine;

namespace Beam.UI;

public class TooltipController : MonoBehaviour
{
	[SerializeField]
	private TooltipViewBase _view;

	private string _tooltip;

	public void ClearTooltip()
	{
		_view.Label.Text = string.Empty;
		_view.CanvasGroup.Alpha = 0f;
	}

	public void SetTooltip(string tooltip)
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
		float alpha = _view.CanvasGroup.Alpha;
		LTDescr val = LeanTween.value(((Component)this).gameObject, (Action<float>)FadeCallback, alpha, 0f, alpha * 0.15f).setIgnoreTimeScale(true);
		if (!string.IsNullOrEmpty(_tooltip))
		{
			val.setOnComplete((Action)FadeTooltipIn);
		}
	}

	private void FadeTooltipIn()
	{
		_view.Label.Text = _tooltip;
		LeanTween.value(((Component)this).gameObject, (Action<float>)FadeCallback, 0f, 1f, 0.15f).setIgnoreTimeScale(true);
	}

	private void FadeCallback(float fade)
	{
		_view.CanvasGroup.Alpha = fade;
	}
}
