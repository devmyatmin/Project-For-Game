namespace Beam.Obfuscated.Users;

public class AccountUser
{
	public AccountUserInfo UserInfo { get; protected set; }

	public AccountUserStats UserStats { get; } = new AccountUserStats();
}
