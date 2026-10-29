using Beam.Events;

namespace Funlabs;

public class PlayerConnectedEvent : IEvent
{
	public int PlayerId;

	public PlayerConnectedEvent(int playerId)
	{
		PlayerId = playerId;
	}
}
