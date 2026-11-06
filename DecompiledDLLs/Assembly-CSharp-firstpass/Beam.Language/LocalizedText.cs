using UnityEngine;

namespace Beam.Language;

public class LocalizedText : TextBase
{
	[SerializeField]
	[LocalizationTermDropdown]
	[LocalizationTermPreview]
	private string _term = "NONE";

	private string[] _args;

	public string Term => _term;

	public string[] Args => _args;

	public void SetTerm(string term)
	{
		_term = term;
	}

	public void SetArgs(params string[] args)
	{
		_args = args;
	}

	public LocalizedText()
	{
	}

	public LocalizedText(string term, params string[] args)
	{
		_term = term;
		_args = args;
	}

	public virtual string GetLocalizedText()
	{
		return Localization.GetLanguageHandler().Localize(_term, _args);
	}

	public override string ToString()
	{
		return GetLocalizedText();
	}
}
