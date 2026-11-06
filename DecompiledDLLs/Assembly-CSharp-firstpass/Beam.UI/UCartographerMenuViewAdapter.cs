using Beam.UI.MapEditor;
using Beam.Utilities;
using Rewired;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Beam.UI;

public class UCartographerMenuViewAdapter : CartographerMenuViewAdapterBase
{
	[SerializeField]
	private GameObject _cartographerPanelGameObject;

	[SerializeField]
	private UCanvasGroupViewAdapter _canvasGroup;

	[SerializeField]
	private UMapHandle _mapHandle;

	[SerializeField]
	private UWorldMapSlot _mapSlotPrefab;

	[SerializeField]
	private GameObject _mapSlotsContainer;

	[SerializeField]
	private UMapListElement _mapListElementPrefab;

	[SerializeField]
	[FormerlySerializedAs("_regularMapListContainer")]
	private GameObject _localMapListContainer;

	[SerializeField]
	private GameObject _remoteMapListContainer;

	[SerializeField]
	private TMPInputFieldViewAdapter _searchInputField;

	[SerializeField]
	private TMPLabelViewAdapter _retrievingLabel;

	[SerializeField]
	private TMPLabelViewAdapter _noMapsLabel;

	[SerializeField]
	private GameObject _mapsSourceGroup;

	[SerializeField]
	private UButtonViewAdapter _localMapsButton;

	[SerializeField]
	private UButtonViewAdapter _remoteMapsButton;

	[SerializeField]
	private ULabelButtonViewAdapter _randomizeButton;

	[SerializeField]
	private ULabelButtonViewAdapter _applyButton;

	[SerializeField]
	private ULabelButtonViewAdapter _createIslandButton;

	[SerializeField]
	private ULabelButtonViewAdapter _cancelButton;

	[SerializeField]
	private TMPLabelViewAdapter _worldSeedLabel;

	[SerializeField]
	private TMPLabelViewAdapter _infoLabel;

	public override ICanvasGroupViewAdapter CanvasGroup => _canvasGroup;

	public override IMapHandle MapHandle => _mapHandle;

	public override UWorldMapSlot MapSlotPrefab => _mapSlotPrefab;

	public override GameObject MapSlotsContainer => _mapSlotsContainer;

	public override GameObject LocalMapListContainer => _localMapListContainer;

	public override GameObject RemoteMapListContainer => _remoteMapListContainer;

	public override IInputFieldViewAdapter SearchInputField => _searchInputField;

	public override ILabelViewAdapter RetrievingLabel => _retrievingLabel;

	public override ILabelViewAdapter NoMapsLabel => _noMapsLabel;

	public override IButtonViewAdapter LocalMapsButton => _localMapsButton;

	public override IButtonViewAdapter RemoteMapsButton => _remoteMapsButton;

	public override ILabelButtonViewAdapter RandomizeButton => _randomizeButton;

	public override ILabelButtonViewAdapter ApplyButton => _applyButton;

	public override ILabelButtonViewAdapter CreateIslandButton => _createIslandButton;

	public override ILabelButtonViewAdapter CancelButton => _cancelButton;

	public override ILabelViewAdapter WorldSeedLabel => _worldSeedLabel;

	public override ILabelViewAdapter InfoLabel => _infoLabel;

	public override void SetInput(Player input)
	{
		base.SetInput(input);
		if (Input != null)
		{
			RewiredUtilities.SetUserInterfaceFromInput(BeamTeam.GetInterfacesInChildren<Selectable>((Component)(object)((Component)this).transform, includeInactive: true), Input);
		}
	}

	public override void Show()
	{
		if (!_cartographerPanelGameObject.activeSelf)
		{
			_cartographerPanelGameObject.SetActive(true);
		}
		base.Show();
	}

	public override void Hide()
	{
		if (_cartographerPanelGameObject.activeSelf)
		{
			_cartographerPanelGameObject.SetActive(false);
		}
		base.Hide();
	}

	public override IMapListElement CreateMapListElement(Transform container)
	{
		UMapListElement uMapListElement = Object.Instantiate<UMapListElement>(_mapListElementPrefab);
		((Component)uMapListElement).transform.SetParent(container, false);
		return uMapListElement;
	}

	public override IButtonViewAdapter GetFirstButton()
	{
		return _cancelButton;
	}
}
