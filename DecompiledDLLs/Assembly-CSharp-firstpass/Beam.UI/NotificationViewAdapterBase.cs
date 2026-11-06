using UnityEngine;

namespace Beam.UI;

public abstract class NotificationViewAdapterBase : ViewAdapterBase
{
	[SerializeField]
	private GameObject _notificationGameObject;

	public abstract ILabelViewAdapter TitleLabel { get; }

	public abstract ILabelViewAdapter MessageLabel { get; }

	public override void Show()
	{
		if (!_notificationGameObject.activeSelf)
		{
			_notificationGameObject.SetActive(true);
		}
		base.Show();
	}

	public override void Hide()
	{
		if (_notificationGameObject.activeSelf)
		{
			_notificationGameObject.SetActive(false);
		}
		base.Hide();
	}

	public abstract float Hide_Coroutine();
}
