using System.Collections.Generic;

namespace Beam.Language;

public interface ILocalizationHandler
{
	IEnumerable<Language> GetLanguages();

	IEnumerable<string> GetTerms();

	string Localize(string term, params string[] args);
}
