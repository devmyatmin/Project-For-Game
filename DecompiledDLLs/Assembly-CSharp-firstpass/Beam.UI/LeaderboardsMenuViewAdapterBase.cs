using System.Collections.Generic;

namespace Beam.UI;

public abstract class LeaderboardsMenuViewAdapterBase : MenuViewAdapterBase
{
	public abstract IEnumerable<ILeaderboardElement> LeaderboardButtons { get; }

	public abstract IButtonViewAdapter BackButton { get; }

	public abstract ILeaderboardElement CreateLeaderboardButton(LeaderboardData leaderboardData);

	public abstract void CreateLeaderboardEntries(IEnumerable<LeaderboardEntryData> leaderboardEntries);

	public abstract void SetLeaderboard(LeaderboardData leaderboardData);

	public abstract void SetUserLeaderboardEntry(LeaderboardEntryData leaderboardEntryData);
}
