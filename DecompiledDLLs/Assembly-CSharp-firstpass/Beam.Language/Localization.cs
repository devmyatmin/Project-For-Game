namespace Beam.Language;

public static class Localization
{
	public static ILocalizationHandler GetLanguageHandler()
	{
		ILocalizationHandler localization = SceneLocalization.Instance.GetLocalization();
		return localization ?? NullObject<NullLocalizationHandler>.Instance;
	}
}
