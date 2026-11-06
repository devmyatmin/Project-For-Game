using Beam.Events;

namespace Beam.UI;

public class NewNotificationEvent<TNotification> : IEvent where TNotification : NotificationBase
{
	private readonly TNotification _notification;

	public TNotification Notification => _notification;

	public NewNotificationEvent(TNotification notification)
	{
		_notification = notification;
	}
}
