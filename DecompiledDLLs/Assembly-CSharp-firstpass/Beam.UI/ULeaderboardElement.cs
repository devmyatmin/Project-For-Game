using Beam.Language;

namespace Beam.UI;

public class ULeaderboardElement : TMPLabelButtonViewAdapter, ILeaderboardElement, IButtonViewAdapter, ISelectableViewAdapter, IViewAdapter
{
	private LeaderboardData _leaderboardData;

	public LeaderboardData LeaderboardData => _leaderboardData;

	public void SetLeaderboardData(LeaderboardData leaderboardData)
	{
		_leaderboardData = leaderboardData;
		Text = LocalizationTerms.GetLeaderboardButtonTerm(_leaderboardData.Name);
	}
}
