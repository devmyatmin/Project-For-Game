using System.Collections.Generic;
using Beam.UI;
using Beam.Utilities;
using MEC;
using UnityEngine;

public class UStaticWorldSpaceAttributeView : WorldSpaceAttributeViewBase
{
	[SerializeField]
	private UCanvasGroupViewAdapter _canvasGroup;

	[SerializeField]
	private UFilledImageViewAdapter _radialBar;

	private CoroutineHandle _showSequence;

	private void Start()
	{
		Reset();
	}

	public override void Show(IHaveAttribute attribute)
	{
		if (!attribute.IsNullOrDestroyed())
		{
			Reset();
			_showSequence = Timing.RunCoroutine(Show_Sequence(attribute));
		}
	}

	public override void Hide()
	{
		Timing.KillCoroutines(_showSequence);
		FadeOutAndReset();
	}

	private void FadeOutAndReset()
	{
		LeanTween.value(base.gameObject, FadeCallback, 1f, 0f, 0.2f).setOnComplete(Reset);
	}

	private void Reset()
	{
		LeanTween.cancel(base.gameObject);
		_canvasGroup.Alpha = 1f;
		_canvasGroup.transform.gameObject.SetActive(value: false);
	}

	private IEnumerator<float> Show_Sequence(IHaveAttribute attribute)
	{
		_radialBar.Fill = Mathf.Max(0f, attribute.GetValue());
		_canvasGroup.transform.gameObject.SetActive(value: true);
		while (!attribute.IsNullOrDestroyed())
		{
			_radialBar.SmoothFill(Mathf.Max(0f, attribute.GetValue()), 15f);
			yield return 0f;
		}
	}

	private void FadeCallback(float value)
	{
		_canvasGroup.Alpha = value;
	}
}
