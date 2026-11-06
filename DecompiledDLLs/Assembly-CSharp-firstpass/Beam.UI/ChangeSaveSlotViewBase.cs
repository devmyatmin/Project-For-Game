namespace Beam.UI;

public abstract class ChangeSaveSlotViewBase : MenuViewAdapterBase
{
	public abstract IButtonViewAdapter[] SlotButtons { get; }

	public abstract IButtonViewAdapter ApplyButton { get; }

	public abstract IButtonViewAdapter CancelButton { get; }
}
