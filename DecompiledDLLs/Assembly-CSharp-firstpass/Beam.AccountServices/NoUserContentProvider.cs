using System;

namespace Beam.AccountServices;

public class NoUserContentProvider : NullObject<NoUserContentProvider>, IUserContentProvider
{
	event Action<LoadedContentArgs> IUserContentProvider.LoadedUserContent
	{
		add
		{
		}
		remove
		{
		}
	}

	public void RequestUserContent()
	{
	}
}
