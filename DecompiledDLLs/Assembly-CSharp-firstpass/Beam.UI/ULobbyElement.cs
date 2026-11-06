using Beam.Language;

namespace Beam.UI;

public class ULobbyElement : TMPLabelButtonViewAdapter, ILobbyElement, IButtonViewAdapter, ISelectableViewAdapter, IViewAdapter
{
	private Session_Filter _sessionFilter;

	public Session_Filter SessionFilter => _sessionFilter;

	public void SetLobbyFilter(Session_Filter sessionFilter)
	{
		_sessionFilter = sessionFilter;
		Text = LocalizationTerms.GetLeaderboardButtonTerm(SessionFilter.ToString());
	}
}
