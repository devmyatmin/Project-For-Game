using Beam.Obfuscated.Users;

namespace Beam.AccountServices;

public interface IAccountService
{
	string Name { get; }

	bool Initialized { get; }

	AccountUser LocalUser { get; }

	IAuthenticationHandler GetAuthenticationHandler();

	IAchievementsHandler GetAchievementsHandler();

	ILeaderboardHandler GetLeaderboardsHandler();
}
