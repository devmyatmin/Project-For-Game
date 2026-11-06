namespace Beam.AccountServices;

public static class GAccountService
{
	private static IAccountService m_service = SteamManager.Instance;

	public static IAccountService Get()
	{
		return m_service;
	}

	public static IAccountService Set(IAccountService service)
	{
		return m_service = service;
	}
}
