namespace Beam.Events;

public class MainMenuOpenedEvent : IEvent
{
	public int PlayerId { get; }

	public bool Opened { get; }

	public MainMenuOpenedEvent(int playerId, bool opened)
	{
		PlayerId = playerId;
		Opened = opened;
	}
}
