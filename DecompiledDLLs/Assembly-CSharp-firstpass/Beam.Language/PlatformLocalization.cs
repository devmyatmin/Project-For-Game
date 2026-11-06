namespace Beam.Language;

public static class PlatformLocalization
{
	public static IPlatformLocalizationHandler GetHandler()
	{
		IPlatformLocalizationHandler platformLocalization = ScenePlatformLocalization.Instance.GetPlatformLocalization();
		return platformLocalization ?? NullObject<NullPlatformLocalizationHandler>.Instance;
	}
}
