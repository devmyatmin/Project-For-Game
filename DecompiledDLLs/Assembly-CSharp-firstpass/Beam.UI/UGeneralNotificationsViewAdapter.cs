using UnityEngine;

namespace Beam.UI;

public class UGeneralNotificationsViewAdapter : NotificationsViewAdapterBase
{
	[SerializeField]
	private TransformViewAdapter _notificationsContainer;

	public override ITransformViewAdapter NotificationsContainer => _notificationsContainer;
}
