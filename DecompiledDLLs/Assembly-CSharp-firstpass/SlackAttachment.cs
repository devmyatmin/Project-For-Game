using System;
using UnityEngine;

[Serializable]
public class SlackAttachment
{
	public string title;

	public string text;

	public string color;

	public SlackAttachment(string title, string text, Color color)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		this.title = title;
		this.text = text;
		this.color = GetHex(color);
	}

	private string GetHex(Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)(color.r * 255f);
		int num2 = (int)(color.g * 255f);
		int num3 = (int)(color.b * 255f);
		return string.Format("#{0}{1}{2}", num.ToString("X2"), num2.ToString("X2"), num3.ToString("X2"));
	}
}
