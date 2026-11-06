using Beam.Events;

namespace Funlabs;

public class ServerJoinInProgressEnded : IEvent
{
	public bool Success;

	public ServerJoinInProgressEnded(bool success)
	{
		Success = success;
	}
}
