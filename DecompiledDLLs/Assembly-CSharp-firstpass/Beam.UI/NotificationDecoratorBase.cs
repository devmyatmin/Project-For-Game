using System;
using UnityEngine;

namespace Beam.UI;

public abstract class NotificationDecoratorBase : NotificationBase
{
	[SerializeField]
	private NotificationBase _notification;

	public override NotificationPriority Priority
	{
		get
		{
			return _notification.Priority;
		}
		set
		{
			_notification.Priority = value;
		}
	}

	public override float Duration
	{
		get
		{
			return _notification.Duration;
		}
		set
		{
			_notification.Duration = value;
		}
	}

	public override int PlayerId
	{
		get
		{
			return _notification.PlayerId;
		}
		set
		{
			_notification.PlayerId = value;
		}
	}

	public override string Title => _notification.Title;

	public override string Message => _notification.Message;

	public override bool SurviveSuspension
	{
		get
		{
			return _notification.SurviveSuspension;
		}
		set
		{
			_notification.SurviveSuspension = value;
		}
	}

	public override bool Interrupted
	{
		get
		{
			return _notification.Interrupted;
		}
		set
		{
			_notification.Interrupted = value;
		}
	}

	public override bool Disposed
	{
		get
		{
			return _notification.Disposed;
		}
		set
		{
			_notification.Disposed = value;
		}
	}

	public override event Action<NotificationBase> Finished;

	protected NotificationDecoratorBase(NotificationBase notification)
	{
		_notification = notification;
	}

	public override void Raise()
	{
		_notification.Raise();
	}

	public override void Finish()
	{
		_notification.Finish();
		Finished?.Invoke(this);
		Finished = null;
	}
}
