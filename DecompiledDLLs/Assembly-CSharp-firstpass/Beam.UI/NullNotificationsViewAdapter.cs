using UnityEngine;

namespace Beam.UI;

public class NullNotificationsViewAdapter : NotificationsViewAdapterBase
{
	private static NullNotificationsViewAdapter _Instance = new NullNotificationsViewAdapter();

	public static NullNotificationsViewAdapter Instance => _Instance;

	public override ITransformViewAdapter NotificationsContainer
	{
		get
		{
			Debug.LogError((object)"NullNotificationsViewAdapter::NotificationsContainer:: Not implemented.");
			return NullObject<NullTransformViewAdapter>.Instance;
		}
	}
}
