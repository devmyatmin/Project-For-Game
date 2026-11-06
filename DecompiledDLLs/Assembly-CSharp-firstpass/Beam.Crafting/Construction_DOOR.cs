using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.Crafting;

public class Construction_DOOR : Constructing, IInteractable, IBase, IHasCraftingType, IDoor, IHaveAttribute
{
	[Header("INTERACTING")]
	[SerializeField]
	private bool _canInteract;

	[Header("ATTRIBUTE")]
	[SerializeField]
	private IconType _type = IconType.Construction;

	[SerializeField]
	private string _actionDescription;

	[SerializeField]
	private Door[] _doors;

	[SerializeField]
	private Collider _wallCollider;

	[HideInInspector]
	public BaseActionUnityEvent Opened;

	public bool CanInteract
	{
		get
		{
			return _canInteract;
		}
		set
		{
			_canInteract = value;
		}
	}

	IconType IHaveAttribute.Type => _type;

	public string ActionDescription => _actionDescription;

	public Door[] Doors => _doors;

	public bool FirstOpen { get; set; } = true;

	public bool IsOpened { get; set; }

	public bool IsOpening { get; set; }

	string IInteractable.GetInteractionDescription(int playerId, IBase obj)
	{
		if (!IsOpened)
		{
			return "ITEM_INTERACTION_DESCRIPTION_OPEN";
		}
		return "ITEM_INTERACTION_DESCRIPTION_CLOSE";
	}

	float IHaveAttribute.GetValue()
	{
		return HealthPoints / MaximumHealthPoints;
	}

	public override void Place()
	{
		if (CraftingType.AttributeType == AttributeType.Container)
		{
			_wallCollider.enabled = true;
			base.Place();
			_wallCollider.enabled = false;
		}
		else
		{
			base.Place();
		}
	}

	protected override IList<Connector> GetConnectorsAtPlacement()
	{
		IList<Connector> connectorsAtPlacement = base.GetConnectorsAtPlacement();
		for (int num = connectorsAtPlacement.Count - 1; num >= 0; num--)
		{
			Connector connector = connectorsAtPlacement[num];
			if (connector.Type != ConnectorType.DOOR)
			{
				connectorsAtPlacement.Remove(connector);
			}
		}
		return connectorsAtPlacement;
	}

	public override void Destroy()
	{
		if (CraftingType.AttributeType == AttributeType.Container)
		{
			_wallCollider.enabled = true;
			base.Destroy();
		}
		else
		{
			base.Destroy();
		}
	}

	public bool Interact(IPlayer player)
	{
		this.Open();
		((UnityEvent<IBase, IBaseActionEventData>)Opened)?.Invoke((IBase)this, (IBaseActionEventData)BaseActionEventData.Empty);
		return true;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		return Interact(player);
	}
}
