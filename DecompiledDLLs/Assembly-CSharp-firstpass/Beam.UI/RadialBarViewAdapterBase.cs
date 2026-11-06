namespace Beam.UI;

public abstract class RadialBarViewAdapterBase : ViewAdapterBase
{
	public abstract ICanvasGroupViewAdapter CanvasGroup { get; }

	public abstract IFilledImageViewAdapter RadialBar { get; }

	public abstract void SetIcon(IconType iconType);
}
