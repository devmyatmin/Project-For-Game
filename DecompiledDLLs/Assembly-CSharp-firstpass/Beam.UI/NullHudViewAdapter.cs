namespace Beam.UI;

public class NullHudViewAdapter : HudViewAdapterBase
{
	public static NullHudViewAdapter Instance { get; } = new NullHudViewAdapter();

	public override IImageViewAdapter DefaultCursor => NullViewAdapter<NullImageViewAdapter>.Instance;

	public override IImageViewAdapter InteractingCursor => NullViewAdapter<NullImageViewAdapter>.Instance;

	public override ILabelViewAdapter ItemNameLabel => NullViewAdapter<NullLabelViewAdapter>.Instance;

	public override ActionHudElementViewAdapterBase ClimbUI => NullActionHudElementViewAdapter.Instance;

	public override ActionHudElementViewAdapterBase ActionHudElementPrefab => NullActionHudElementViewAdapter.Instance;

	public override ITransformViewAdapter ActionHudElementsContainer => NullObject<NullTransformViewAdapter>.Instance;

	public override ITransformViewAdapter PooledActionHudElementsContainer => NullObject<NullTransformViewAdapter>.Instance;

	public override ICanvasGroupViewAdapter ItemActionGroup => NullViewAdapter<NullCanvasGroupViewAdapter>.Instance;
}
