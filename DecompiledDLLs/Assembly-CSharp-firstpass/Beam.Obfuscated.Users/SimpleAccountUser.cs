namespace Beam.Obfuscated.Users;

public class SimpleAccountUser : AccountUser
{
	public SimpleAccountUser(string id)
	{
		UserInfo = new AccountUserInfo
		{
			UserId = id,
			DisplayName = id
		};
	}
}
