using System.Collections.Generic;

namespace Beam.UI;

public class ChecklistController : NotificationPresenterBase<ChecklistNotification, ChecklistView>
{
	private readonly string CHECKLIST_TITLE_DEFAULT = "New Objectives!";

	public override void Initialize(ChecklistNotification notification)
	{
		base.Initialize(notification);
		View.TitleLabel.Text = (string.IsNullOrEmpty(notification.Title) ? CHECKLIST_TITLE_DEFAULT : notification.Title);
		CreateChecklistElements(notification.Objectives);
	}

	private void CreateChecklistElements(IEnumerable<ObjectiveBase> objectives)
	{
		foreach (ObjectiveBase objective in objectives)
		{
			View.CreateChecklistElement().Initialize(objective);
		}
	}
}
