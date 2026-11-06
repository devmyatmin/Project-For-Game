namespace Beam.UI;

public abstract class TextScreenViewAdapterBase : ViewAdapterBase
{
	public abstract void SetHeaderText(string text);

	public abstract void SetSubheaderText(string text);

	public abstract void SetText(string text);

	public abstract void SetTooltipText(string text);

	protected virtual void Awake()
	{
		SetHeaderText("");
		SetSubheaderText("");
		SetText("");
		SetTooltipText("");
	}
}
