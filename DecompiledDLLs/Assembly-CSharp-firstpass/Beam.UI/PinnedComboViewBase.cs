namespace Beam.UI;

public abstract class PinnedComboViewBase : ViewAdapterBase
{
	public abstract void SetCombo(CombinationInfo combination);

	public abstract void SetCurrentCraftsmanshipLevel(int level);

	public abstract void SetRequiredCraftsmanshipLevel(int level);
}
