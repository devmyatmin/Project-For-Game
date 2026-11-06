using Beam.UI;
using UnityEngine;

namespace Beam;

public class NotificationSequenceNode<TNotification> : SequenceNodeBase where TNotification : NotificationBase
{
	[SerializeField]
	private TNotification _notification;

	private bool _listeningForFinish;

	public TNotification Notification => _notification;

	public override void Reset()
	{
		_notification.Disposed = false;
		_notification.Interrupted = false;
		base.Reset();
	}

	public override void Execute()
	{
		if (!_listeningForFinish)
		{
			_notification.Finished += Notification_Finished;
			_listeningForFinish = true;
		}
		_notification.Raise();
	}

	public override void Complete()
	{
		base.Complete();
		_notification.Dispose();
	}

	public override void Kill()
	{
		if (!_notification.Disposed)
		{
			_notification.Interrupted = true;
		}
		_notification.Dispose();
		base.Kill();
	}

	public void Notification_Finished(NotificationBase sender)
	{
		_notification.Finished -= Notification_Finished;
		_listeningForFinish = false;
		if (!_notification.Interrupted)
		{
			Complete();
		}
		if (Completed)
		{
			Continue();
		}
	}
}
public class NotificationSequenceNode : NotificationSequenceNode<NotificationBase>
{
}
