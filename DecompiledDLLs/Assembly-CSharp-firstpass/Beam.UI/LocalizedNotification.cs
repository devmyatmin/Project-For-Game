using Beam.Events;
using Beam.Language;
using UnityEngine;

namespace Beam.UI;

public class LocalizedNotification : NotificationDecoratorBase
{
	[SerializeField]
	private LocalizedText _titleText = new LocalizedText("NOTIFICATION_NEW_NOTIFICATION_TITLE");

	[SerializeField]
	private LocalizedText _messageText = new LocalizedText();

	public LocalizedText TitleText
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

	public LocalizedText MessageText
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

	public override string Title => TitleText.ToString();

	public override string Message => _messageText.ToString();

	public LocalizedNotification(NotificationBase decorated)
		: base(decorated)
	{
	}

	public override void Raise()
	{
		EventManager.RaiseEvent(new NewNotificationEvent<LocalizedNotification>(this));
	}

	public static void Post(IPlayer player, NotificationPriority priority, float duration, string term)
	{
		if (player.IsOwner)
		{
			LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
			localizedNotification.Priority = priority;
			localizedNotification.Duration = duration;
			localizedNotification.PlayerId = player.Id;
			localizedNotification.MessageText.SetTerm(term);
			localizedNotification.Raise();
		}
	}

	public static void Post(IPlayer player, NotificationPriority priority, float duration, LocalizedText message, bool surviveSuspension = false)
	{
		if (player.IsOwner)
		{
			LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
			localizedNotification.Priority = priority;
			localizedNotification.Duration = duration;
			localizedNotification.PlayerId = player.Id;
			localizedNotification.MessageText = message;
			localizedNotification.SurviveSuspension = surviveSuspension;
			localizedNotification.Raise();
		}
	}

	public static void Post(IPlayer player, NotificationPriority priority, float duration, string term, params string[] args)
	{
		if (player.IsOwner)
		{
			LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
			localizedNotification.Priority = priority;
			localizedNotification.Duration = duration;
			localizedNotification.PlayerId = player.Id;
			localizedNotification.MessageText.SetTerm(term);
			localizedNotification.MessageText.SetArgs(args);
			localizedNotification.Raise();
		}
	}
}
