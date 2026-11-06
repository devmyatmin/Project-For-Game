using System;
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
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (!BeamTeam.IsNullOrDestroyed(attribute))
		{
			Reset();
			_showSequence = Timing.RunCoroutine(Show_Sequence(attribute));
		}
	}

	public override void Hide()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_showSequence);
		FadeOutAndReset();
	}

	private void FadeOutAndReset()
	{
		LeanTween.value(((Component)this).gameObject, (Action<float>)FadeCallback, 1f, 0f, 0.2f).setOnComplete((Action)Reset);
	}

	private void Reset()
	{
		LeanTween.cancel(((Component)this).gameObject);
		_canvasGroup.Alpha = 1f;
		((Component)((Component)_canvasGroup).transform).gameObject.SetActive(false);
	}

	private IEnumerator<float> Show_Sequence(IHaveAttribute attribute)
	{
		_radialBar.Fill = Mathf.Max(0f, attribute.GetValue());
		((Component)((Component)_canvasGroup).transform).gameObject.SetActive(true);
		while (!BeamTeam.IsNullOrDestroyed(attribute))
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
