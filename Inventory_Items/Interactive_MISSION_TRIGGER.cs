using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Beam;

public class Interactive_MISSION_TRIGGER : BaseObject, IInteractable, IBase, IHasCraftingType, ISelectable
{
	[SerializeField]
	private Selecting _selecting;

	public Selecting Selecting => _selecting;

	public bool CanInteract { get; set; } = true;


	public event Action Interacted;

	private void OnDestroy()
	{
		this.Interacted = null;
	}

	public string GetInteractionDescription(int playerId, IBase obj)
	{
		return "VIEW";
	}

	public bool Interact(IPlayer player)
	{
		this.Interacted?.Invoke();
		return true;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		return Interact(player);
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}
}
