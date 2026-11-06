namespace Beam.UI;

public abstract class OptionsMenuViewAdapterBase : MenuViewAdapterBase
{
	public abstract IScrollbarViewAdapter Scrollbar { get; }

	public abstract IButtonViewAdapter GeneralButton { get; }

	public abstract IButtonViewAdapter GraphicsButton { get; }

	public abstract IButtonViewAdapter AudioButton { get; }

	public abstract IButtonViewAdapter InputButton { get; }

	public abstract IButtonViewAdapter AboutButton { get; }

	public abstract IButtonViewAdapter BackButton { get; }

	public abstract IButtonViewAdapter ApplyButton { get; }

	public IButtonViewAdapter CurrentPanelButton { get; set; }

	public abstract IOptionCategoryHeader CreateOptionCategoryHeader(IOptionPanel panel);

	public abstract void DestroyOptionCategoryHeaders();

	public abstract void SetTooltip(string tooltip);
}
