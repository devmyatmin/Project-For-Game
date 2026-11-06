using Beam.Events;

namespace Beam.UI;

public class FreezeNotificationsEvent : IEvent
{
	public readonly bool Freeze;

	public FreezeNotificationsEvent(bool freeze)
	{
		Freeze = freeze;
	}
}
