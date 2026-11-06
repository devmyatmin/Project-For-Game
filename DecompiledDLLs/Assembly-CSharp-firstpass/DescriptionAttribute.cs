using System;

[AttributeUsage(AttributeTargets.Field)]
public class DescriptionAttribute : Attribute, IOptionElementData
{
	private readonly string _text;

	private readonly string _tooltip;

	public string Text => _text;

	public string Tooltip => _tooltip;

	public DescriptionAttribute(string text, string tooltip = "")
	{
		_text = text;
		_tooltip = tooltip;
	}
}
