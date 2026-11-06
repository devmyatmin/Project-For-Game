namespace Beam.UI;

public interface ILobbyElement : IButtonViewAdapter, ISelectableViewAdapter, IViewAdapter
{
	Session_Filter SessionFilter { get; }

	void SetLobbyFilter(Session_Filter leaderboardData);
}
