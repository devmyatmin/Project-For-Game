using Beam.Utilities;
using UnityEngine;

namespace Beam.UI;

public class DamageIndicatorElement : IndicatorElementBase
{
	[SerializeField]
	private float _fadeDuration = 4f;

	[SerializeField]
	private LeanTweenType _fadeCurve = LeanTweenType.linear;

	private void Start()
	{
		LeanTween.value(base.gameObject, 1f, 0f, _fadeDuration).setEase(_fadeCurve).setOnUpdate(Fade_OnUpdate)
			.setOnComplete(Fade_OnComplete);
	}

	private void Fade_OnUpdate(float value)
	{
		Color color = base.View.IndicatorImage.Color;
		color.a = value;
		base.View.IndicatorImage.Color = color;
	}

	private void Fade_OnComplete()
	{
		BeamTeam.Destroy(base.gameObject);
	}
}
