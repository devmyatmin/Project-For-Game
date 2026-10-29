using System.Collections.Generic;
using Beam.Language;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam.Crafting;

public class ConstructionObject_ITEM_PILE : ConstructionObject, ISlotStorageProvider<IPickupable>, ICraftingMaterialProvider
{
	[SerializeField]
	private PileSlotStorage _slotStorage;

	public PileSlotStorage SlotStorage => _slotStorage;

	CraftingType ICraftingMaterialProvider.MaterialsType
	{
		get
		{
			return _slotStorage.StorageSlot.CraftingType;
		}
		set
		{
			_slotStorage.StorageSlot.CraftingType = value;
		}
	}

	public ISlotStorage<IPickupable> GetSlotStorage()
	{
		return SlotStorage;
	}

	IEnumerable<IPickupable> ICraftingMaterialProvider.GetCraftingMaterials()
	{
		return _slotStorage.GetStored();
	}

	protected override void Awake()
	{
		base.Awake();
		_slotStorage.Name = base.DisplayName;
		_slotStorage.Closed += SlotStorage_Closed;
		_slotStorage.Pushed += SlotStorage_Pushed;
		_slotStorage.Popped += SlotStorage_Popped;
		StorageRegistry.Link(_slotStorage);
	}

	protected override void Start()
	{
		base.Start();
		_slotStorage.ReferenceId = MiniGuid.NewFrom(base.ReferenceId);
	}

	public override void OnDestroy()
	{
		_slotStorage.Closed -= SlotStorage_Closed;
		_slotStorage.Pushed -= SlotStorage_Pushed;
		_slotStorage.Popped -= SlotStorage_Popped;
		_slotStorage.Dispose();
		StorageRegistry.Unlink(_slotStorage);
		base.OnDestroy();
	}

	public override void OnBeginCrafting()
	{
		base.OnBeginCrafting();
		IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(base.CreatorId);
		((ICraftingMaterialProvider)this).MaterialsType = playerFromId.Crafter.LastCraftingInfo.CurrentType;
	}

	public override void OnCrafted(IPlayer player)
	{
		base.OnCrafted(player);
		IPickupable template = DummyPickupable.GetTemplate(player.Crafter.LastCraftingInfo.CurrentPrefabId);
		_slotStorage.Push(template);
		_slotStorage.Push(template);
	}

	protected override void OnDisplayNameChanged()
	{
		base.OnDisplayNameChanged();
		_slotStorage.Name = base.DisplayName;
	}

	private void SlotStorage_Pushed(IPickupable pickupable, bool success)
	{
		if (success && _slotStorage.GetStoredCount() <= 1)
		{
			string displayName = pickupable.DisplayName;
			string str = Localization.GetLanguageHandler().Localize("ITEM_PILE_TYPE_SENTENCE");
			base.DisplayName = string_safemethods.Format(str, displayName, "INTERACTIVE_TYPE_CRAFTING_ITEM_PILE");
		}
	}

	private void SlotStorage_Popped(IPickupable pickupable)
	{
		if (_slotStorage.GetStoredCount() == 0)
		{
			string str = Localization.GetLanguageHandler().Localize("ITEM_PILE_TYPE_SENTENCE");
			base.DisplayName = string_safemethods.Format(str, "ITEM_DISPLAY_NAME_PREFIX_EMPTY", "INTERACTIVE_TYPE_CRAFTING_ITEM_PILE");
			if (!_slotStorage.IsOpened)
			{
				MultiplayerMng.Destroy(this);
			}
		}
	}

	private void SlotStorage_Closed()
	{
		if (_slotStorage.GetStoredCount() == 0)
		{
			MultiplayerMng.Destroy(this);
		}
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		JObject jObject2 = SavingUtilities.Save(_slotStorage);
		if (jObject2 != null)
		{
			jObject.AddField("Storage", jObject2);
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JObject field = data.GetField("Storage");
		SavingUtilities.Load(_slotStorage, field);
		if (_slotStorage.GetStoredCount() == 0)
		{
			Debug.LogError("Empty pile has been saved!", base.gameObject);
		}
	}
}
