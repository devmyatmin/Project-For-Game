using System;
using Beam.Obfuscated.Users;

namespace Beam.AccountServices;

public class NoOnlineServices : NullObject<NoOnlineServices>, IAccountService, IAuthenticationHandler
{
	public string Name => "Local User";

	public bool Initialized => true;

	public bool LoggedIn => true;

	public AccountUser LocalUser { get; } = new SimpleAccountUser("Player 1");

	public void Login(Action<AuthCallbackInfo> callback)
	{
		callback?.Invoke(new AuthCallbackInfo
		{
			Result = 0
		});
	}

	public IAuthenticationHandler GetAuthenticationHandler()
	{
		return this;
	}

	public IAchievementsHandler GetAchievementsHandler()
	{
		return NullObject<NoAchievementsHandler>.Instance;
	}

	public ILeaderboardHandler GetLeaderboardsHandler()
	{
		return NullObject<NoLeaderboardHandler>.Instance;
	}
}
