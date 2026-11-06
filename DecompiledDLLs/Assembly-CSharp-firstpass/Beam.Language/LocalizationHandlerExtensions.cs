using System.Collections.Generic;
using System.Text;

namespace Beam.Language;

public static class LocalizationHandlerExtensions
{
	private static readonly StringBuilder _builder = new StringBuilder();

	public static string LocalizeMany(this ILocalizationHandler handler, string terms, char delimeter)
	{
		if (string.IsNullOrWhiteSpace(terms))
		{
			return string.Empty;
		}
		return handler.LocalizeMany(terms.Split(delimeter));
	}

	public static string LocalizeMany(this ILocalizationHandler handler, IEnumerable<string> terms, string seperator = " ")
	{
		if (terms == null)
		{
			return string.Empty;
		}
		using IEnumerator<string> enumerator = terms.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return string.Empty;
		}
		_builder.Clear();
		_builder.Append(handler.Localize(enumerator.Current));
		while (enumerator.MoveNext())
		{
			_builder.Append(seperator);
			_builder.Append(handler.Localize(enumerator.Current));
		}
		return _builder.ToString();
	}
}
