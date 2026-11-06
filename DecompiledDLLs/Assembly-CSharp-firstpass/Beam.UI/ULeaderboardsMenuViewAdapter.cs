using System.Collections.Generic;
using Beam.Utilities;
using UnityEngine;

namespace Beam.UI;

public class ULeaderboardsMenuViewAdapter : LeaderboardsMenuViewAdapterBase
{
	[SerializeField]
	private GameObject _panel;

	[SerializeField]
	private ULeaderboardElement _leaderboardButtonPrefab;

	[SerializeField]
	private LeaderboardEntryElement _leaderboardEntryElementPrefab;

	[SerializeField]
	private Transform _leaderboardButtonContainer;

	[SerializeField]
	private UScrollviewViewAdapter _scrollView;

	[SerializeField]
	private Transform _leaderboardEntryElementContainer;

	[SerializeField]
	private LeaderboardEntryElement _userLeaderboardEntryElement;

	[SerializeField]
	private TMPLabelViewAdapter _retrievingLabel;

	[SerializeField]
	private TMPLabelViewAdapter _noLeaderboardEntriesLabel;

	[SerializeField]
	private UButtonViewAdapter _backButton;

	private HashSet<ILeaderboardElement> _leaderboardButtons = new HashSet<ILeaderboardElement>();

	private ELeaderboardDisplayType _leaderboardDisplayType;

	public override IButtonViewAdapter BackButton => _backButton;

	public override IEnumerable<ILeaderboardElement> LeaderboardButtons => _leaderboardButtons;

	private void Awake()
	{
		Visible = _panel.activeSelf;
	}

	public override void Show()
	{
		if (!_panel.activeSelf)
		{
			_panel.SetActive(true);
		}
		base.Show();
	}

	public override void Hide()
	{
		if (_panel.activeSelf)
		{
			BeamTeam.DestroyAllChildren(_leaderboardButtonContainer);
			BeamTeam.DestroyAllChildren(_leaderboardEntryElementContainer);
			_leaderboardButtons.Clear();
			_panel.SetActive(false);
		}
		base.Hide();
	}

	public override ILeaderboardElement CreateLeaderboardButton(LeaderboardData leaderboardData)
	{
		ULeaderboardElement uLeaderboardElement = Object.Instantiate<ULeaderboardElement>(_leaderboardButtonPrefab);
		((Component)uLeaderboardElement).transform.SetParent(_leaderboardButtonContainer, false);
		uLeaderboardElement.SetLeaderboardData(leaderboardData);
		uLeaderboardElement.Selected += LeaderboardButton_Selected;
		_leaderboardButtons.Add(uLeaderboardElement);
		return uLeaderboardElement;
	}

	private LeaderboardEntryElement CreateLeaderboardEntryElement()
	{
		LeaderboardEntryElement leaderboardEntryElement = Object.Instantiate<LeaderboardEntryElement>(_leaderboardEntryElementPrefab);
		((Component)leaderboardEntryElement).transform.SetParent(_leaderboardEntryElementContainer, false);
		return leaderboardEntryElement;
	}

	public override void CreateLeaderboardEntries(IEnumerable<LeaderboardEntryData> leaderboardEntries)
	{
		BeamTeam.DestroyAllChildrenImmediate(_leaderboardEntryElementContainer);
		int num = 1;
		foreach (LeaderboardEntryData leaderboardEntry in leaderboardEntries)
		{
			LeaderboardEntryElement leaderboardEntryElement = CreateLeaderboardEntryElement();
			PopulateLeaderboardEntryElement(leaderboardEntryElement, leaderboardEntry);
			leaderboardEntryElement.SetRank(num++);
		}
		bool flag = _leaderboardEntryElementContainer.childCount > 0;
		((Component)_leaderboardEntryElementContainer).gameObject.SetActive(flag);
		((Component)((Component)_noLeaderboardEntriesLabel).transform).gameObject.SetActive(!flag);
		((Component)((Component)_retrievingLabel).transform).gameObject.SetActive(false);
	}

	public override void SetLeaderboard(LeaderboardData leaderboardData)
	{
		_leaderboardDisplayType = leaderboardData.DisplayType;
		((Component)_leaderboardEntryElementContainer).gameObject.SetActive(false);
		((Component)((Component)_noLeaderboardEntriesLabel).transform).gameObject.SetActive(false);
		((Component)((Component)_retrievingLabel).transform).gameObject.SetActive(true);
	}

	public override void SetUserLeaderboardEntry(LeaderboardEntryData leaderboardEntryData)
	{
		PopulateLeaderboardEntryElement(_userLeaderboardEntryElement, leaderboardEntryData);
	}

	private void PopulateLeaderboardEntryElement(LeaderboardEntryElement leaderboardEntryElement, LeaderboardEntryData leaderboardEntryData)
	{
		leaderboardEntryElement.SetDisplayType(_leaderboardDisplayType);
		leaderboardEntryElement.SetUsername(leaderboardEntryData.Username);
		leaderboardEntryElement.SetRank(leaderboardEntryData.Rank);
		leaderboardEntryElement.SetValue(leaderboardEntryData.Value);
	}

	public override IButtonViewAdapter GetFirstButton()
	{
		IButtonViewAdapter buttonViewAdapter = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(_leaderboardButtons);
		if (BeamTeam.IsNullOrDestroyed(buttonViewAdapter))
		{
			return _backButton;
		}
		return buttonViewAdapter;
	}

	private void LeaderboardButton_Selected(ISelectableViewAdapter widget)
	{
		_scrollView.FocusElement(widget.transform);
	}
}
