using Beam.Obfuscated.Users;

namespace Beam.AccountServices;

public class NoAchievementsHandler : NullObject<NoAchievementsHandler>, IAchievementsHandler
{
	public bool IsAchievementUnlocked(EAchievement achId)
	{
		return false;
	}

	public void SubmitStats(AccountUserStats stats, bool immediate = false)
	{
	}

	public void UnlockAchievement(EAchievement ach)
	{
	}
}
