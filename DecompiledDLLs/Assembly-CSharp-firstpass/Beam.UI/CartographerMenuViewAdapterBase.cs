using Beam.UI.MapEditor;
using UnityEngine;

namespace Beam.UI;

public abstract class CartographerMenuViewAdapterBase : MenuViewAdapterBase
{
	public abstract ICanvasGroupViewAdapter CanvasGroup { get; }

	public abstract IMapHandle MapHandle { get; }

	public abstract UWorldMapSlot MapSlotPrefab { get; }

	public abstract GameObject MapSlotsContainer { get; }

	public abstract GameObject LocalMapListContainer { get; }

	public abstract GameObject RemoteMapListContainer { get; }

	public abstract IInputFieldViewAdapter SearchInputField { get; }

	public abstract ILabelViewAdapter RetrievingLabel { get; }

	public abstract ILabelViewAdapter NoMapsLabel { get; }

	public abstract IButtonViewAdapter LocalMapsButton { get; }

	public abstract IButtonViewAdapter RemoteMapsButton { get; }

	public abstract ILabelButtonViewAdapter RandomizeButton { get; }

	public abstract ILabelButtonViewAdapter ApplyButton { get; }

	public abstract ILabelButtonViewAdapter CreateIslandButton { get; }

	public abstract ILabelButtonViewAdapter CancelButton { get; }

	public abstract ILabelViewAdapter WorldSeedLabel { get; }

	public abstract ILabelViewAdapter InfoLabel { get; }

	public abstract IMapListElement CreateMapListElement(Transform container);
}
