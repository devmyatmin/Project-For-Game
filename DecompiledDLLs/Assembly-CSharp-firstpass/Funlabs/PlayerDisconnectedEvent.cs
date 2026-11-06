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
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (!MPExtensions.IsMultiplayer(Game.Mode))
			{
				return true;
			}
			if (!MPExtensions.IsServer(Game.Mode))
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
