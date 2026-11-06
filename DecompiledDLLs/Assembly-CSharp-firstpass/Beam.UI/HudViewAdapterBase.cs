namespace Beam.UI;

public abstract class HudViewAdapterBase : ViewAdapterBase
{
	public abstract IImageViewAdapter DefaultCursor { get; }

	public abstract IImageViewAdapter InteractingCursor { get; }

	public abstract ILabelViewAdapter ItemNameLabel { get; }

	public abstract ActionHudElementViewAdapterBase ClimbUI { get; }

	public abstract ActionHudElementViewAdapterBase ActionHudElementPrefab { get; }

	public abstract ITransformViewAdapter ActionHudElementsContainer { get; }

	public abstract ITransformViewAdapter PooledActionHudElementsContainer { get; }

	public abstract ICanvasGroupViewAdapter ItemActionGroup { get; }
}
