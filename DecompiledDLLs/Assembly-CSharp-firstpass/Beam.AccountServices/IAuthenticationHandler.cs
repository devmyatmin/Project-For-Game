using System;

namespace Beam.AccountServices;

public interface IAuthenticationHandler
{
	bool LoggedIn { get; }

	void Login(Action<AuthCallbackInfo> callback);
}
