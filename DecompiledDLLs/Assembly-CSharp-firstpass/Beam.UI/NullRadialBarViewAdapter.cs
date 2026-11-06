using UnityEngine;

namespace Beam.UI;

public class NullRadialBarViewAdapter : RadialBarViewAdapterBase
{
	public static NullRadialBarViewAdapter Instance { get; } = new NullRadialBarViewAdapter();

	public override ICanvasGroupViewAdapter CanvasGroup => NullViewAdapter<NullCanvasGroupViewAdapter>.Instance;

	public override IFilledImageViewAdapter RadialBar => NullViewAdapter<NullFilledImageViewAdapter>.Instance;

	public override void SetIcon(IconType iconType)
	{
		Debug.LogError((object)"NullRadialBarViewAdapter::SetIcon:: Not implemented!");
	}
}
