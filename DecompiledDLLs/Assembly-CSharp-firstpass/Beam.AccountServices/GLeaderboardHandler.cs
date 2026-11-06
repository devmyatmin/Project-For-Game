namespace Beam.AccountServices;

public static class GLeaderboardHandler
{
	private static ILeaderboardHandler m_handler = SteamLeaderboards.Instance;

	public static ILeaderboardHandler Get()
	{
		return m_handler;
	}
}
