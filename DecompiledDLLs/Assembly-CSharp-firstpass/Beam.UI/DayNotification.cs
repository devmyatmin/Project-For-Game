using Beam.Events;

namespace Beam.UI;

public class DayNotification : NotificationDecoratorBase
{
	public DayNotification(NotificationBase notification)
		: base(notification)
	{
	}

	public override void Raise()
	{
		EventManager.RaiseEvent(new NewNotificationEvent<DayNotification>(this));
	}
}
