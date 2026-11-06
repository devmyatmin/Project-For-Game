using System;
using Beam.Utilities;
using UnityEngine;

namespace Beam.UI;

public class DamageIndicatorElement : IndicatorElementBase
{
	[SerializeField]
	private float _fadeDuration = 4f;

	[SerializeField]
	private LeanTweenType _fadeCurve = (LeanTweenType)1;

	private void Start()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		LeanTween.value(((Component)this).gameObject, 1f, 0f, _fadeDuration).setEase(_fadeCurve).setOnUpdate((Action<float>)Fade_OnUpdate)
			.setOnComplete((Action)Fade_OnComplete);
	}

	private void Fade_OnUpdate(float value)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Color color = View.IndicatorImage.Color;
		color.a = value;
		View.IndicatorImage.Color = color;
	}

	private void Fade_OnComplete()
	{
		BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
	}

	public DamageIndicatorElement()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
	}
}
