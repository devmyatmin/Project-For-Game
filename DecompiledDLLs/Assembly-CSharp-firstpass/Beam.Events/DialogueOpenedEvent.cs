namespace Beam.Events;

public class DialogueOpenedEvent : IEvent
{
	public int PlayerId { get; }

	public bool Opened { get; }

	public DialogueOpenedEvent(int playerId, bool opened)
	{
		PlayerId = playerId;
		Opened = opened;
	}
}
