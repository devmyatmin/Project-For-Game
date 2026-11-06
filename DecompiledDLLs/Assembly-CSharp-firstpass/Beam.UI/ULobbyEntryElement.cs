using System;
using Funlabs;
using UdpKit;
using UnityEngine;

namespace Beam.UI;

public class ULobbyEntryElement : LobbyEntryElement
{
	[SerializeField]
	private TMPLabelViewAdapter _roomnameLabel;

	[SerializeField]
	private TMPLabelViewAdapter _rankLabel;

	[SerializeField]
	private TMPLabelViewAdapter _valueLabel;

	private ELeaderboardDisplayType _displayType;

	private Guid _guid;

	private UdpSession _session;

	private MultiplayerTokens.Session _tsession;

	private Session_Filter _session_Filter;

	public override void SetRoomname(string username)
	{
		_roomnameLabel.Text = username;
	}

	public override void SetRank(int rank)
	{
		string text = ((rank == -1) ? "-" : rank.ToString());
		_rankLabel.Text = text;
	}

	public override void SetValue(int value, ELeaderboardDisplayType displayType)
	{
		string text = ((value == -1) ? "---" : Format(value, displayType));
		_valueLabel.Text = text;
	}

	public override void SetDisplayType(ELeaderboardDisplayType displayType)
	{
		_displayType = displayType;
	}

	public override void SetSession(Guid guid, UdpSession session, Session_Filter session_Filter)
	{
		_guid = guid;
		_session = session;
		UdpSession session2 = _session;
		_tsession = ((session2 != null) ? MPExtensions.GetToken(session2) : null);
		SetRoomname((_tsession != null) ? _tsession.GetCosmeticName() : guid.ToString());
		_session_Filter = session_Filter;
		if (_tsession != null)
		{
			switch (_session_Filter)
			{
			case Session_Filter.MOST_TIME_PLAYED:
				SetValue((int)_tsession.TimePlayed, ELeaderboardDisplayType.Seconds);
				break;
			case Session_Filter.MOST_DAYS_SURVIVED:
				SetValue(_tsession.DaysSurvived, ELeaderboardDisplayType.Numeric);
				break;
			case Session_Filter.QUICKEST_ESCAPE:
				SetValue((int)_tsession.QuickestScape, ELeaderboardDisplayType.Seconds);
				break;
			case Session_Filter.MOST_ISLANDS_HOPPED:
				SetValue(_tsession.IslandsHopped, ELeaderboardDisplayType.Numeric);
				break;
			case Session_Filter.MOST_ISLANDS_EXPLORED:
				SetValue(_tsession.IslandsExplored, ELeaderboardDisplayType.Numeric);
				break;
			case Session_Filter.MOST_FISH_FISHED:
				SetValue(_tsession.FishFished, ELeaderboardDisplayType.Numeric);
				break;
			case Session_Filter.MOST_ANIMALS_KILLED:
				SetValue(_tsession.EnemiesKilled, ELeaderboardDisplayType.Numeric);
				break;
			case Session_Filter.MOST_ANIMALS_TRAPPED:
				SetValue(_tsession.AnimalsTrapped, ELeaderboardDisplayType.Numeric);
				break;
			case Session_Filter.MOST_ITEMS_CRAFTED:
				SetValue(_tsession.ItemsCrafted, ELeaderboardDisplayType.Numeric);
				break;
			}
		}
	}

	public UdpSession getSession()
	{
		return _session;
	}

	public Guid GetGuid()
	{
		return _guid;
	}

	public Session_Filter GetSessionFilter()
	{
		return _session_Filter;
	}

	private string Format(int value, ELeaderboardDisplayType displayType)
	{
		if (displayType != ELeaderboardDisplayType.Numeric && displayType == ELeaderboardDisplayType.Seconds)
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds((double)value);
			return $"{timeSpan.Days:00}d {timeSpan.Hours:00}h {timeSpan.Minutes:00}m";
		}
		return value.ToString();
	}
}
