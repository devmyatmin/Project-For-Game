namespace Beam.UI;

public abstract class ActionHudElementViewAdapterBase : ViewAdapterBase
{
	public abstract ILabelViewAdapter PressLabel { get; }

	public abstract ILabelViewAdapter KeyLabel { get; }

	public abstract IFilledImageViewAdapter KeyFill { get; }

	public abstract ILabelViewAdapter ActionLabel { get; }
}
