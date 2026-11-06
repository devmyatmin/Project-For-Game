using System.Linq;
using Beam.UI;

namespace Beam;

public class ChecklistNotificationSequenceNode : NotificationSequenceNode<ChecklistNotification>
{
	public override void Initialize()
	{
		base.Initialize();
		foreach (ObjectiveBase objective in Notification.Objectives)
		{
			objective.Completed += Objective_Completed;
		}
	}

	private void Objective_Completed(ObjectiveBase objective)
	{
		if (Notification.Objectives.All((ObjectiveBase o) => o.IsCompleted))
		{
			Complete();
		}
	}

	public override void Complete()
	{
		foreach (ObjectiveBase objective in Notification.Objectives)
		{
			objective.Completed -= Objective_Completed;
		}
		base.Complete();
	}
}
