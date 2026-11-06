namespace Beam.UI;

public abstract class IndicatorViewBase : ViewAdapterBase
{
	public abstract DamageIndicatorElement IndicatorPrefab { get; }

	public abstract ITransformViewAdapter IndicatorContainer { get; }
}
