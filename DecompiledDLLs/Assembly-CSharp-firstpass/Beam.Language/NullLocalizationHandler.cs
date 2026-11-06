using System.Collections.Generic;
using UnityEngine;

namespace Beam.Language;

public class NullLocalizationHandler : NullObject<NullLocalizationHandler>, ILocalizationHandler
{
	public IEnumerable<Language> GetLanguages()
	{
		return new Language[0];
	}

	public IEnumerable<string> GetTerms()
	{
		Debug.LogError((object)"NullLocalizationHandler:: No localization.");
		return null;
	}

	public string Localize(string term, params string[] args)
	{
		Debug.LogError((object)"NullLocalizationHandler:: No localization.");
		return term;
	}
}
