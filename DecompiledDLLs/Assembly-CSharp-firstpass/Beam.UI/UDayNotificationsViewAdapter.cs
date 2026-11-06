using UnityEngine;

namespace Beam.UI;

public class UDayNotificationsViewAdapter : NotificationsViewAdapterBase
{
	[SerializeField]
	private TransformViewAdapter _notificationsContainer;

	public override ITransformViewAdapter NotificationsContainer => _notificationsContainer;
}
