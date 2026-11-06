using Beam.Events;

namespace Beam.UI;

public class SkillNotification : NotificationDecoratorBase
{
	public SkillNotification(NotificationBase notification)
		: base(notification)
	{
	}

	public override void Raise()
	{
		EventManager.RaiseEvent(new NewNotificationEvent<SkillNotification>(this));
	}
}
