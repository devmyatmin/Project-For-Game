using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class SlackMessage
{
	public string text;

	public SlackAttachment[] attachments;

	public SlackMessage(string text)
	{
		this.text = text;
		attachments = new SlackAttachment[0];
	}

	public SlackMessage(string text, string attachmentTitle, string attachmentText, Color attachmentColor)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		this.text = text;
		attachments = new SlackAttachment[1]
		{
			new SlackAttachment(attachmentTitle, attachmentText, attachmentColor)
		};
	}

	public void Post()
	{
		Task.Run(() => SlackWebhook.Post(this)).Wait();
	}
}
