namespace Beam.UI;

public abstract class CreateSessionMenuViewAdapterBase : MenuViewAdapterBase
{
	public abstract IInputFieldViewAdapter InputField { get; }

	public abstract IOptionButtonViewAdapter VisibilityOptionButton { get; }

	public abstract IOptionButtonViewAdapter RegionOptionButton { get; }

	public abstract ILabelButtonViewAdapter CreateButton { get; }

	public abstract ILabelButtonViewAdapter BackButton { get; }

	public abstract void SetTooltip(string tooltip);

	public abstract void SetAllButtonInteractability(bool state);
}
