namespace Beam.UI;

public class DayNotificationsPresenter : NotificationsPresenterBase<DayNotification, NewNotificationEvent<DayNotification>, DayNotificationPresenter>
{
	public void NewDayNotification(int days)
	{
		Notification notification = new Notification();
		notification.Priority = NotificationPriority.Immediate;
		notification.Duration = 8f;
		notification.MessageText = days.ToString();
		new DayNotification(notification).Raise();
	}
}
