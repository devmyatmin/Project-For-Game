namespace Beam.Events;

public class LoadingProgressChangedEvent : IEvent
{
	public readonly string LoadingMessage;

	public LoadingProgressChangedEvent(string message)
	{
		LoadingMessage = message;
	}
}
