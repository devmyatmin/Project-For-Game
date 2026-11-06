using System.Collections.Generic;
using Beam.Events;
using Beam.Utilities;
using MEC;
using UnityEngine;

namespace Beam.UI;

public abstract class NotificationsPresenterBase<TNotification, TNotificationEvent, TNotificationPresenter> : ControllerBase, INeedPlayerComponent where TNotification : NotificationBase where TNotificationEvent : NewNotificationEvent<TNotification> where TNotificationPresenter : MonoBehaviour, INotificationPresenter<TNotification>
{
	[SerializeField]
	private NotificationsViewAdapterBase _view;

	[SerializeField]
	private TNotificationPresenter _notificationPrefab;

	private TNotificationPresenter _notificationPresenter;

	private Stack<TNotification> _notifications = new Stack<TNotification>();

	private CoroutineHandle _notifyingSequence;

	private TNotification _currentNotification;

	private bool _notifying;

	private bool _suspended;

	private IPlayer _player;

	public IPlayer Player => _player;

	public void SetPlayer(IPlayer player)
	{
		_player = player;
	}

	private void Start()
	{
		SetView(_view);
		CreateNotificationPresenter();
		EventManager.AddListener<TNotificationEvent>(EventManager_NewNotificationEvent);
		EventManager.AddListener<FreezeNotificationsEvent>(EventManager_FreezeNotifications);
	}

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as NotificationsViewAdapterBase;
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)$"{((object)this).GetType()}:: No view component assigned!");
			_view = NullNotificationsViewAdapter.Instance;
		}
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<TNotificationEvent>(EventManager_NewNotificationEvent);
		EventManager.RemoveListener<FreezeNotificationsEvent>(EventManager_FreezeNotifications);
	}

	private void CreateNotificationPresenter()
	{
		if (!((Object)(object)_notificationPresenter != (Object)null))
		{
			_notificationPresenter = Object.Instantiate<TNotificationPresenter>(_notificationPrefab);
			((Component)(object)_notificationPresenter).transform.SetParent(_view.NotificationsContainer.Transform, false);
		}
	}

	private void EventManager_NewNotificationEvent(TNotificationEvent e)
	{
		int num = ((!BeamTeam.IsNullOrDestroyed(_player)) ? _player.Id : 0);
		if (e.Notification.PlayerId == num)
		{
			NewNotification(e.Notification);
		}
	}

	private void EventManager_FreezeNotifications(FreezeNotificationsEvent e)
	{
		if (e.Freeze)
		{
			SuspendNotifications();
		}
		else
		{
			ResumeNotifications();
		}
	}

	public void NewNotification(TNotification notification)
	{
		if ((_suspended && !notification.SurviveSuspension) || ((_suspended || _currentNotification != null) && notification.Priority == NotificationPriority.Low))
		{
			return;
		}
		if (_currentNotification != null)
		{
			if (_currentNotification.Priority > notification.Priority)
			{
				_notifications.Push(notification);
			}
			else
			{
				if (_currentNotification.Priority > NotificationPriority.Normal)
				{
					_currentNotification.Interrupted = true;
					_notifications.Push(_currentNotification);
				}
				else
				{
					_currentNotification.Dispose();
				}
				_notifications.Push(notification);
			}
		}
		else
		{
			_notifications.Push(notification);
		}
		Notify();
	}

	private void SuspendNotifications()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		_suspended = true;
		Timing.KillCoroutines(_notifyingSequence);
		if (_currentNotification != null)
		{
			_currentNotification = null;
			_notificationPresenter.ForceHide();
		}
		_notifying = false;
	}

	public void ResumeNotifications()
	{
		_suspended = false;
		Notify();
	}

	private void Notify()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (_notifications.Count > 0 && !_notifying && !_suspended)
		{
			_notifyingSequence = Timing.RunCoroutine(Notify_Sequence());
		}
	}

	private IEnumerator<float> Notify_Sequence()
	{
		_notifying = true;
		while (_notifications.Count > 0)
		{
			_currentNotification = _notifications.Pop();
			_currentNotification.Interrupted = false;
			if (!_currentNotification.Disposed)
			{
				_notificationPresenter.Initialize(_currentNotification);
				yield return _notificationPresenter.Show_Yieldable();
				yield return _notificationPresenter.Hide_Yieldable();
			}
			if (!_currentNotification.Interrupted)
			{
				_currentNotification.Finish();
			}
		}
		_currentNotification = null;
		_notifying = false;
	}
}
