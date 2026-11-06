namespace Beam.AccountServices;

public static class GUserContentProvider
{
	private static IUserContentProvider m_provider = SteamWorkshop.Instance;

	public static IUserContentProvider Get()
	{
		return m_provider;
	}
}
