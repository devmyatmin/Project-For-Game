namespace Beam.UI;

public interface INotificationPresenter<in TNotification> where TNotification : NotificationBase
{
	void Initialize(TNotification notification);

	float Show_Yieldable();

	float Hide_Yieldable();

	void ForceHide();
}
