using System;

namespace Beam.AccountServices;

public interface IUserContentProvider
{
	event Action<LoadedContentArgs> LoadedUserContent;

	void RequestUserContent();
}
