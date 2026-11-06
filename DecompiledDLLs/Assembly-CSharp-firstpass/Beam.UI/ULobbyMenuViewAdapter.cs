using System;
using System.Collections.Generic;
using Beam.Utilities;
using Funlabs;
using UdpKit;
using UnityEngine;

namespace Beam.UI;

public class ULobbyMenuViewAdapter : LobbyMenuViewAdapterBase
{
	[SerializeField]
	private GameObject _panel;

	[SerializeField]
	private ULobbyElement _lobbyElementButtonPrefab;

	[SerializeField]
	private ULobbyEntryElement _lobbyEntryElementPrefab;

	[SerializeField]
	private Transform _lobbyButtonContainer;

	[SerializeField]
	private UScrollviewViewAdapter _scrollView;

	[SerializeField]
	private Transform _lobbyEntryElementContainer;

	[SerializeField]
	private TMPLabelViewAdapter _retrievingLabel;

	[SerializeField]
	private TMPLabelViewAdapter _noLobbyEntriesLabel;

	[SerializeField]
	private UButtonViewAdapter _enterCodeButton;

	[SerializeField]
	private UButtonViewAdapter _backButton;

	[SerializeField]
	private UButtonViewAdapter _applyButton;

	[SerializeField]
	private UScrollviewViewAdapter _crollview;

	[SerializeField]
	private TMPOptionButtonViewAdapter _regionOptionButton;

	[SerializeField]
	private TMPLabelViewAdapter _regionLabel;

	private HashSet<ILobbyElement> _lobbyButtons = new HashSet<ILobbyElement>();

	public override IButtonViewAdapter EnterCodeButton => _enterCodeButton;

	public override IButtonViewAdapter BackButton => _backButton;

	public override IButtonViewAdapter ApplyButton => _applyButton;

	public override IScrollviewViewAdapter Scrollview => _crollview;

	public override IOptionButtonViewAdapter RegionOptionButton => _regionOptionButton;

	public override ILabelViewAdapter RegionLabel => _regionLabel;

	public override IEnumerable<ILobbyElement> LobyButtons => _lobbyButtons;

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
			BeamTeam.DestroyAllChildren(_lobbyButtonContainer);
			BeamTeam.DestroyAllChildren(_lobbyEntryElementContainer);
			_lobbyButtons.Clear();
			_panel.SetActive(false);
		}
		base.Hide();
	}

	public override ILobbyElement CreateLobbyButton(Session_Filter sessionFilter)
	{
		ULobbyElement uLobbyElement = Object.Instantiate<ULobbyElement>(_lobbyElementButtonPrefab);
		((Component)uLobbyElement).transform.SetParent(_lobbyButtonContainer, false);
		uLobbyElement.SetLobbyFilter(sessionFilter);
		uLobbyElement.Selected += SessionFilterButton_Selected;
		_lobbyButtons.Add(uLobbyElement);
		return uLobbyElement;
	}

	private ULobbyEntryElement CreateLobbyEntryElement()
	{
		ULobbyEntryElement uLobbyEntryElement = Object.Instantiate<ULobbyEntryElement>(_lobbyEntryElementPrefab);
		((Component)uLobbyEntryElement).transform.SetParent(_lobbyEntryElementContainer, false);
		return uLobbyEntryElement;
	}

	public override void UpdateLobbydEntries(Map<Guid, UdpSession> sessionList)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		UdpSession session = default;
		foreach (Transform item in _lobbyEntryElementContainer)
		{
			ULobbyEntryElement component = ((Component)item).GetComponent<ULobbyEntryElement>();
			if (!BeamTeam.IsNullOrDestroyed(component) && sessionList.TryFind(component.GetGuid(), ref session) && !BeamTeam.IsNullOrDestroyed(MPExtensions.GetToken(session)))
			{
				component.SetSession(component.GetGuid(), session, component.GetSessionFilter());
			}
		}
	}

	public override ULobbyEntryElement CreateLobbydEntries(List<UdpSession> sessionList, Session_Filter selectedFilter, Guid selectedGuid, Action<object> onEntryButtonClick, Action<ISelectableViewAdapter> onEntryButtonSelect)
	{
		CleanButtons();
		int num = 1;
		ULobbyEntryElement uLobbyEntryElement = null;
		foreach (UdpSession session in sessionList)
		{
			MultiplayerTokens.Session token = MPExtensions.GetToken(session);
			if (token != null && token.IsShowable())
			{
				ULobbyEntryElement uLobbyEntryElement2 = CreateLobbyEntryElement();
				uLobbyEntryElement2.SetSession(session.Id, session, selectedFilter);
				uLobbyEntryElement2.SetRank(num);
				num++;
				if ((Object)(object)uLobbyEntryElement == (Object)null)
				{
					uLobbyEntryElement = uLobbyEntryElement2;
				}
				if (uLobbyEntryElement2.GetGuid() == selectedGuid)
				{
					uLobbyEntryElement = uLobbyEntryElement2;
				}
				TMPLabelButtonViewAdapter component = ((Component)uLobbyEntryElement2).gameObject.GetComponent<TMPLabelButtonViewAdapter>();
				component.Click += onEntryButtonClick;
				component.Selected += onEntryButtonSelect;
			}
		}
		Canvas.ForceUpdateCanvases();
		return uLobbyEntryElement;
	}

	public override IButtonViewAdapter GetFirstButton()
	{
		IButtonViewAdapter buttonViewAdapter = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(_lobbyButtons);
		if (BeamTeam.IsNullOrDestroyed(buttonViewAdapter))
		{
			return _backButton;
		}
		return buttonViewAdapter;
	}

	public override void CleanButtons()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		foreach (Transform item in _lobbyEntryElementContainer)
		{
			Object.Destroy((Object)(object)((Component)item).gameObject);
		}
	}

	private void SessionFilterButton_Selected(ISelectableViewAdapter widget)
	{
		_scrollView.FocusElement(widget.transform);
	}

	public void ToggleNoEntriesFoundLabel(bool value)
	{
		((Component)((Component)_noLobbyEntriesLabel).transform).gameObject.SetActive(value);
	}
}
