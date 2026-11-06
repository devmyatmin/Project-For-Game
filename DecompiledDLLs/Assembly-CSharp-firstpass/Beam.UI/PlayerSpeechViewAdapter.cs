using System;
using UnityEngine;

namespace Beam.UI;

public class PlayerSpeechViewAdapter : PlayerSpeechViewAdapterBase
{
	[SerializeField]
	private UCanvasGroupViewAdapter _canvasGroup;

	[SerializeField]
	public TMPLabelViewAdapter _subtitlesLabel;

	public override ICanvasGroupViewAdapter CanvasGroup => _canvasGroup;

	public override ILabelViewAdapter SubtitlesLabel => _subtitlesLabel;

	public override void Show()
	{
		if (!((Component)_canvasGroup).gameObject.activeSelf)
		{
			((Component)_canvasGroup).gameObject.SetActive(true);
			LeanTween.value(((Component)this).gameObject, (Action<float>)Fade, 0f, 1f, 0.15f);
			base.Show();
		}
	}

	public override void Hide()
	{
		if (((Component)_canvasGroup).gameObject.activeSelf)
		{
			LeanTween.value(((Component)this).gameObject, (Action<float>)Fade, 1f, 0f, 0.15f).setOnComplete((Action)FadedOut);
		}
	}

	private void FadedOut()
	{
		((Component)_canvasGroup).gameObject.SetActive(false);
		base.Hide();
	}

	private void Fade(float value)
	{
		_canvasGroup.Alpha = value;
	}
}
