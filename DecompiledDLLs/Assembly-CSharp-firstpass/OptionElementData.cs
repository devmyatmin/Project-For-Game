public class OptionElementData : IOptionElementData
{
	private readonly string _text;

	private readonly string _tooltip;

	public string Text => _text;

	public string Tooltip => _tooltip;

	public OptionElementData(string text, string tooltip = "")
	{
		_text = text;
		_tooltip = tooltip;
	}
}
