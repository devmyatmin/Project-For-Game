using Beam.Obfuscated.Users;

namespace Beam.AccountServices;

public interface IAchievementsHandler
{
	void UnlockAchievement(EAchievement ach);

	void SubmitStats(AccountUserStats stats, bool immediate = false);

	bool IsAchievementUnlocked(EAchievement achId);
}
