using System;
using System.Runtime.CompilerServices;
using Beam.Crafting;
using Beam.Serialization.Json;
using Funlabs;
using UnityEngine;

namespace Beam.Intro;

public class Interactive_AIRCRAFT_DOOR : ReplicableMonoBehaviour, IInteractable, IBase, IHasCraftingType, ISelectable, IDoor
{
	[SerializeField]
	private Selecting _selecting;

	[SerializeField]
	private Door[] _doors;

	[Header("EVENTS")]
	public BaseActionUnityEvent Opened;

	public override MiniGuid ReferenceId
	{
		get
		{
			return MiniGuid.INTERACTIVE_AIRCRAFT_DOOR;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public Selecting Selecting => _selecting;

	public Door[] Doors => _doors;

	public bool IsOpened { get; set; }

	public bool IsOpening { get; set; }

	public string DisplayName => "INTERACTIVE_TYPE_BUILDING_DOOR";

	public Transform TransformParent { get; set; }

	public CraftingType CraftingType { get; } = CraftingType.Empty;


	public RaycastMode RaycastMode { get; }

	public bool CanParent { get; set; }

	public bool IsDamageable { get; }

	public bool CanInteract { get; set; } = true;


	public void Damage(float damage, IBase damager)
	{
	}

	public void Destroy()
	{
	}

	public void Parent()
	{
	}

	public bool Interact(IPlayer player)
	{
		this.Open();
		Opened?.Invoke(this, BaseActionEventData.Empty);
		return true;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		return Interact(player);
	}

	public string GetInteractionDescription(int playerId, IBase obj)
	{
		if (!IsOpened)
		{
			return "ITEM_INTERACTION_DESCRIPTION_OPEN";
		}
		return "ITEM_INTERACTION_DESCRIPTION_CLOSE";
	}

	public override JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("opened", IsOpened);
		return jObject;
	}

	public override void Load(JObject data)
	{
		if (data.GetField("opened").GetValue<bool>())
		{
			this.Open();
		}
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	Transform IBase.get_transform()
	{
		return base.transform;
	}
}
