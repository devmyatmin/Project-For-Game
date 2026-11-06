using System.Collections.Generic;
using Beam.Events;
using UnityEngine;

namespace Beam.UI;

public class ChecklistNotification : NotificationDecoratorBase
{
	[SerializeField]
	private ObjectiveBase[] _objectives;

	public IEnumerable<ObjectiveBase> Objectives => _objectives;

	public ChecklistNotification(NotificationBase decorated)
		: base(decorated)
	{
	}

	public void SetObjectives(params ObjectiveBase[] objectives)
	{
		_objectives = objectives;
	}

	public override void Raise()
	{
		EventManager.RaiseEvent(new NewNotificationEvent<ChecklistNotification>(this));
	}
}
