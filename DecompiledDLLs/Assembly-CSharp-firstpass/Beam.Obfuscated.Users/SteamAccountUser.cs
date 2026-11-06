using Steamworks;

namespace Beam.Obfuscated.Users;

public class SteamAccountUser : AccountUser
{
	public SteamAccountUser(string id)
	{
		UserInfo = new AccountUserInfo
		{
			UserId = id,
			DisplayName = SteamFriends.GetPersonaName()
		};
	}
}
