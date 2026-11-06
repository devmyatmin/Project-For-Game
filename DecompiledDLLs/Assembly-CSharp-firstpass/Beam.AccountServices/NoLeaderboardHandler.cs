using System;
using System.Collections.Generic;
using Beam.Obfuscated.Users;

namespace Beam.AccountServices;

public class NoLeaderboardHandler : NullObject<NoLeaderboardHandler>, ILeaderboardHandler
{
	public bool Connected => false;

	public event Action<LeaderboardData> LeaderboardDownloaded
	{
		add
		{
		}
		remove
		{
		}
	}

	public event Action<IEnumerable<LeaderboardEntryData>> LeaderboardEntriesDownloaded
	{
		add
		{
		}
		remove
		{
		}
	}

	public event Action<LeaderboardEntryData> LeaderboardEntryDownloadedForUser
	{
		add
		{
		}
		remove
		{
		}
	}

	public void DownloadLeaderboardEntries(string leaderboardName)
	{
	}

	public void DownloadLeaderboardEntryForUser(string leaderboardName)
	{
	}

	public void DownloadLeaderboards()
	{
	}

	public void SubmitScore(string leaderboardName, int score)
	{
	}

	public void SubmitScores(AccountUserStats stats)
	{
	}
}
