using System;
using UnityEngine;

namespace Beam.UI;

public class ULeaderboardEntryElement : LeaderboardEntryElement
{
	[SerializeField]
	private TMPLabelViewAdapter _usernameLabel;

	[SerializeField]
	private TMPLabelViewAdapter _rankLabel;

	[SerializeField]
	private TMPLabelViewAdapter _valueLabel;

	private ELeaderboardDisplayType _displayType;

	public override void SetUsername(string username)
	{
		_usernameLabel.Text = username;
	}

	public override void SetRank(int rank)
	{
		string text = ((rank == -1) ? "-" : rank.ToString());
		_rankLabel.Text = text;
	}

	public override void SetValue(int value)
	{
		string text = ((value == -1) ? "---" : Format(value));
		_valueLabel.Text = text;
	}

	public override void SetDisplayType(ELeaderboardDisplayType displayType)
	{
		_displayType = displayType;
	}

	private string Format(int value)
	{
		ELeaderboardDisplayType displayType = _displayType;
		if (displayType != ELeaderboardDisplayType.Numeric && displayType == ELeaderboardDisplayType.Seconds)
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds((double)value);
			return $"{timeSpan.Days:00}d {timeSpan.Hours:00}h {timeSpan.Minutes:00}m";
		}
		return value.ToString();
	}
}
