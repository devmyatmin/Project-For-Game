using System.Collections;
using Beam.UI;
using UnityEngine;

namespace Beam.Intro;

public class PrivateJetTutorial : Singleton<PrivateJetTutorial>
{
	public bool FindLifeRaft;

	private IEnumerator Start()
	{
		while (!FindLifeRaft)
		{
			yield return null;
		}
		yield return (object)new WaitForSeconds(4f);
		LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
		localizedNotification.Priority = NotificationPriority.Immediate;
		localizedNotification.Duration = -1f;
		localizedNotification.PlayerId = PlayerRegistry.LocalPlayer.Id;
		localizedNotification.TitleText.SetTerm("NOTIFICATION_OBJECTIVE_NEW_OBJECTIVES_TITLE");
		LocalizedObjective localizedObjective = new LocalizedObjective(new Objective(1));
		localizedObjective.DescriptionText.SetTerm("INTRO_OBJECTIVE_GET_TO_LIFERAFT_DESC");
		ChecklistNotification checklistNotification = new ChecklistNotification(localizedNotification);
		checklistNotification.SetObjectives(localizedObjective);
		checklistNotification.Raise();
	}
}
