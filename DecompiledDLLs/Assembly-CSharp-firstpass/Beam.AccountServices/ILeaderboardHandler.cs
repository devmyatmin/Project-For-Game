using System;
using System.Collections.Generic;
using Beam.Obfuscated.Users;

namespace Beam.AccountServices;

public interface ILeaderboardHandler
{
	bool Connected { get; }

	event Action<LeaderboardData> LeaderboardDownloaded;

	event Action<IEnumerable<LeaderboardEntryData>> LeaderboardEntriesDownloaded;

	event Action<LeaderboardEntryData> LeaderboardEntryDownloadedForUser;

	void DownloadLeaderboards();

	void DownloadLeaderboardEntries(string leaderboardName);

	void DownloadLeaderboardEntryForUser(string leaderboardName);

	void SubmitScore(string leaderboardName, int score);

	void SubmitScores(AccountUserStats stats);
}
