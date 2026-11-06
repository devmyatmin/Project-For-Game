using UnityEngine;

namespace Beam.UI;

public class UIndicatorView : IndicatorViewBase
{
	[SerializeField]
	private DamageIndicatorElement _indicatorPrefab;

	[SerializeField]
	private TransformViewAdapter _indicatorContainer;

	public override DamageIndicatorElement IndicatorPrefab => _indicatorPrefab;

	public override ITransformViewAdapter IndicatorContainer => _indicatorContainer;
}
