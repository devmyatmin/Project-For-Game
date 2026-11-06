using UnityEngine;

namespace Beam.UI;

public class UIndicatorElementView : IndicatorElementViewBase
{
	[SerializeField]
	private UImageViewAdapter _indicatorImage;

	public override IImageViewAdapter IndicatorImage => _indicatorImage;
}
