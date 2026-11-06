namespace Beam.UI;

public abstract class Data<TData> : IRadialMenuModel
{
	public TData Value;

	public abstract string GetDisplayName();

	public abstract string GetDescription();
}
