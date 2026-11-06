using UnityEngine;

namespace Beam.Language;

public class SimpleText : TextBase
{
	[SerializeField]
	private string _text = "";

	public string Text => _text;

	public SimpleText(string text)
	{
		_text = text;
	}

	public override string ToString()
	{
		return _text;
	}
}
