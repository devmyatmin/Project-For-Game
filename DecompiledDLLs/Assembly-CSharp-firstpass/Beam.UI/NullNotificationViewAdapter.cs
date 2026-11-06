using UnityEngine;

namespace Beam.UI;

public class NullNotificationViewAdapter : NotificationViewAdapterBase
{
	private static NullNotificationViewAdapter _Instance = new NullNotificationViewAdapter();

	public static NullNotificationViewAdapter Instance => _Instance;

	public override ILabelViewAdapter TitleLabel
	{
		get
		{
			Debug.LogError((object)"NullNotificationViewAdapter::TitleLabel:: Not implemented!");
			return NullViewAdapter<NullLabelViewAdapter>.Instance;
		}
	}

	public override ILabelViewAdapter MessageLabel
	{
		get
		{
			Debug.LogError((object)"NullNotificationViewAdapter::MessageLabel:: Not implemented!");
			return NullViewAdapter<NullLabelViewAdapter>.Instance;
		}
	}

	public override float Hide_Coroutine()
	{
		return 0f;
	}
}
