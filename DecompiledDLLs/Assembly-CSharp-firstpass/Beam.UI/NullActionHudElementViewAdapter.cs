namespace Beam.UI;

public class NullActionHudElementViewAdapter : ActionHudElementViewAdapterBase
{
	public static NullActionHudElementViewAdapter Instance { get; } = new NullActionHudElementViewAdapter();

	public override ILabelViewAdapter PressLabel => NullViewAdapter<NullLabelViewAdapter>.Instance;

	public override ILabelViewAdapter KeyLabel => NullViewAdapter<NullLabelViewAdapter>.Instance;

	public override IFilledImageViewAdapter KeyFill => NullViewAdapter<NullFilledImageViewAdapter>.Instance;

	public override ILabelViewAdapter ActionLabel => NullViewAdapter<NullLabelViewAdapter>.Instance;
}
