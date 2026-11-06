using System;
using Beam.Events;
using UnityEngine;

namespace Beam.UI;

public class Notification : NotificationBase
{
	private const string DEFAULT_TITLE_TEXT = "New Notification!";

	[SerializeField]
	private NotificationPriority _priority = NotificationPriority.Normal;

	[SerializeField]
	private float _duration = -1f;

	[SerializeField]
	private bool _surviveSuspension;

	[SerializeField]
	private string _titleText = "";

	[SerializeField]
	private string _messageText = "";

	public override NotificationPriority Priority
	{
		get
		{
			return _priority;
		}
		set
		{
			_priority = value;
		}
	}

	public override float Duration
	{
		get
		{
			return _duration;
		}
		set
		{
			_duration = value;
		}
	}

	public string TitleText
	{
		get
		{
			return _titleText;
		}
		set
		{
			_titleText = value;
		}
	}

	public string MessageText
	{
		get
		{
			return _messageText;
		}
		set
		{
			_messageText = value;
		}
	}

	public override int PlayerId { get; set; }

	public override string Title
	{
		get
		{
			if (string.IsNullOrWhiteSpace(_titleText))
			{
				return "New Notification!";
			}
			return TitleText;
		}
	}

	public override string Message => MessageText;

	public override bool SurviveSuspension
	{
		get
		{
			return _surviveSuspension;
		}
		set
		{
			_surviveSuspension = value;
		}
	}

	public override bool Interrupted { get; set; }

	public override bool Disposed { get; set; }

	public override event Action<NotificationBase> Finished;

	public override void Raise()
	{
		EventManager.RaiseEvent(new NewNotificationEvent<Notification>(this));
	}

	public override void Finish()
	{
		Finished?.Invoke(this);
		Finished = null;
	}
}
