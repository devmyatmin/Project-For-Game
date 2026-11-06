using System.Collections.Generic;
using Beam.Language;
using Beam.PostEffects;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.UI;

public class InventoryRadialMenuPresenter : RadialMenuPresenterBase<InventoryData, InventoryRadialMenuElement, InventoryRadialMenuViewAdapterBase>, INeedPlayerComponent, IHotkeySource
{
	private IPlayer _player;

	private IMenuEffectsController _menuEffects;

	private InventoryRadialMenuElement _reservedElement;

	private InventoryRadialMenuElement _storageElement;

	private InventoryRadialMenuElement _emptyHandElement;

	[SerializeField]
	public InteracterUnityEvent _openTrophy;

	public InventoryMenuPresenter Inventory { get; set; }

	IPlayer INeedPlayerComponent.Player => _player;

	IPickupable IHotkeySource.CurrentObject => CurrentElement?.SelectedValue?.Value;

	protected override void Start()
	{
		_menuEffects = _menuEffects ?? NullObject<NullMenuEffectsController>.Instance;
		base.Start();
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		Input = _player.Input;
		IMenuEffectsController component = ((Component)_player.PlayerCamera.Camera).GetComponent<MainMenuEffectsController>();
		_menuEffects = component ?? NullObject<NullMenuEffectsController>.Instance;
	}

	protected override void Layout()
	{
		if (!BeamTeam.IsNullOrDestroyed(Inventory.DestinationStorage))
		{
			if ((Object)(object)_storageElement == (Object)null)
			{
				_storageElement = Object.Instantiate<InventoryRadialMenuElement>(View.StorageElementPrefab);
			}
			_storageElement.Initialize(new List<InventoryData>
			{
				new StorageInventoryData<IPickupable>(Inventory.DestinationStorage)
			});
			_reservedElement = _storageElement;
		}
		else
		{
			if ((Object)(object)_emptyHandElement == (Object)null)
			{
				_emptyHandElement = Object.Instantiate<InventoryRadialMenuElement>(View.EmptyHandElementPrefab);
				_emptyHandElement.Initialize(new List<InventoryData>
				{
					new EmptyHandInventoryData()
				});
			}
			_reservedElement = _emptyHandElement;
		}
		InsertElement(_reservedElement, 0);
		InventoryRadialMenuElement inventoryRadialMenuElement = PopElement();
		inventoryRadialMenuElement.Initialize(new List<InventoryData>
		{
			new HoldingItemInventoryData(_player.Holder.CurrentObject)
		});
		InsertElement(inventoryRadialMenuElement, 1);
		base.Layout();
	}

	public override void Show()
	{
		_menuEffects.SetMaskTexture(View.BlurMaskTexture);
		_menuEffects.Show();
		((UnityEvent<IPlayer>)_openTrophy)?.Invoke(_player);
		base.Show();
	}

	public override void Hide()
	{
		_menuEffects?.Hide();
		base.Hide();
	}

	protected override void Clear()
	{
		base.Clear();
		View.ClassificationLabel.Text = string.Empty;
		View.HealthLabel.Text = string.Empty;
	}

	protected override void Element_Enter(InventoryRadialMenuElement element)
	{
		base.Element_Enter(element);
		if (element.SelectedValue != null)
		{
			View.ClassificationLabel.Text = element.SelectedValue.GetClassification();
			View.HealthLabel.Text = string_safemethods.Format(Localization.GetLanguageHandler().Localize(element.SelectedValue.GetHealth()), element.SelectedValue.GetHealthValue());
		}
	}
}
