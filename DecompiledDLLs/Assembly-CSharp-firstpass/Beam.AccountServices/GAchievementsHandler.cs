namespace Beam.AccountServices;

public static class GAchievementsHandler
{
	private static IAchievementsHandler m_handler = SteamAchievements.Instance;

	public static IAchievementsHandler Get()
	{
		return m_handler;
	}
}
