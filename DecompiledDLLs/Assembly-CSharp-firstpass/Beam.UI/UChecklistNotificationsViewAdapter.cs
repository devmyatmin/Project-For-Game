using UnityEngine;

namespace Beam.UI;

public class UChecklistNotificationsViewAdapter : NotificationsViewAdapterBase
{
	[SerializeField]
	private TransformViewAdapter _notificationsContainer;

	public override ITransformViewAdapter NotificationsContainer => _notificationsContainer;
}
