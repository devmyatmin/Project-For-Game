using System;
using System.Collections.Generic;
using UdpKit;

namespace Beam.UI;

public abstract class LobbyMenuViewAdapterBase : MenuViewAdapterBase
{
	public abstract IEnumerable<ILobbyElement> LobyButtons { get; }

	public abstract IButtonViewAdapter EnterCodeButton { get; }

	public abstract IButtonViewAdapter BackButton { get; }

	public abstract IOptionButtonViewAdapter RegionOptionButton { get; }

	public abstract IButtonViewAdapter ApplyButton { get; }

	public abstract IScrollviewViewAdapter Scrollview { get; }

	public abstract ILabelViewAdapter RegionLabel { get; }

	public abstract ILobbyElement CreateLobbyButton(Session_Filter sessionFilter);

	public abstract ULobbyEntryElement CreateLobbydEntries(List<UdpSession> sessionList, Session_Filter selectedFilter, Guid selectedGuid, Action<object> onEntryButtonClick, Action<ISelectableViewAdapter> onEntrySelect);

	public abstract void UpdateLobbydEntries(Map<Guid, UdpSession> sessionList);

	public abstract void CleanButtons();
}
