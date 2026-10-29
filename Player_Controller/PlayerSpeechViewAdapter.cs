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
		if (!_canvasGroup.gameObject.activeSelf)
		{
			_canvasGroup.gameObject.SetActive(value: true);
			LeanTween.value(base.gameObject, Fade, 0f, 1f, 0.15f);
			base.Show();
		}
	}

	public override void Hide()
	{
		if (_canvasGroup.gameObject.activeSelf)
		{
			LeanTween.value(base.gameObject, Fade, 1f, 0f, 0.15f).setOnComplete(FadedOut);
		}
	}

	private void FadedOut()
	{
		_canvasGroup.gameObject.SetActive(value: false);
		base.Hide();
	}

	private void Fade(float value)
	{
		_canvasGroup.Alpha = value;
	}
}
