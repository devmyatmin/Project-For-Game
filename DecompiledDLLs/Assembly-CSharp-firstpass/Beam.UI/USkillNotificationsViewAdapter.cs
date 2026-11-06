using UnityEngine;

namespace Beam.UI;

public class USkillNotificationsViewAdapter : NotificationsViewAdapterBase
{
	[SerializeField]
	private TransformViewAdapter _notificationsContainer;

	public override ITransformViewAdapter NotificationsContainer => _notificationsContainer;
}
