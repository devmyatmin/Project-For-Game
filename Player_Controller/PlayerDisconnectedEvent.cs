using Beam;
using Beam.Events;

namespace Funlabs;

public class PlayerDisconnectedEvent : IEvent
{
	public int PlayerId;

	public DisconnectPayLoad DisconnectPayload;

	public bool AmIDisconnected
	{
		get
		{
			if (!Game.Mode.IsMultiplayer())
			{
				return true;
			}
			if (!Game.Mode.IsServer())
			{
				return true;
			}
			return PlayerId == 0;
		}
	}

	public PlayerDisconnectedEvent(int playerId, DisconnectPayLoad disconnectPayLoad)
	{
		DisconnectPayload = disconnectPayLoad;
		PlayerId = playerId;
	}

	public bool TryGetDisconnectDialog(out string title, out string content)
	{
		title = (content = string.Empty);
		if (!AmIDisconnected)
		{
			return false;
		}
		return DisconnectPayload.TryGetDisconnectDialog(out title, out content);
	}
}
