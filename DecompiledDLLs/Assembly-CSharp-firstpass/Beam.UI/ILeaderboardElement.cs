namespace Beam.UI;

public interface ILeaderboardElement : IButtonViewAdapter, ISelectableViewAdapter, IViewAdapter
{
	LeaderboardData LeaderboardData { get; }

	void SetLeaderboardData(LeaderboardData leaderboardData);
}
