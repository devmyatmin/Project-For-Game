namespace Beam.UI;

public class NullIndicatorViewBase : IndicatorViewBase
{
	private static NullIndicatorViewBase _Instance = new NullIndicatorViewBase();

	public static NullIndicatorViewBase Instance => _Instance;

	public override DamageIndicatorElement IndicatorPrefab { get; }

	public override ITransformViewAdapter IndicatorContainer => NullObject<NullTransformViewAdapter>.Instance;
}
