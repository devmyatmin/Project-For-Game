using Beam.Events;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Beam.Crafting;

public class Constructing_VEHICLE : Constructing, IVehicle, IBase, IHasCraftingType, IInteractable
{
	[SerializeField]
	private bool _canInteract;

	[PREFAB_PROCESSOR_GetComponent]
	[FormerlySerializedAs("_vehicle")]
	[SerializeField]
	private VehicleMovementBase _vehicleMovement;

	[SerializeField]
	private Collider _wallCollider;

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

	public VehicleMovementBase VehicleMovement => _vehicleMovement;

	public virtual string GetInteractionDescription(int playerId, IBase obj)
	{
		if (_vehicleMovement.IsOverturned() && !VehicleMovement.Operating)
		{
			return string.Empty;
		}
		IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(playerId);
		if (_vehicleMovement.Operating)
		{
			if (playerFromId == _vehicleMovement.Operator)
			{
				return "ITEM_INTERACTION_DESCRIPTION_STOP_OPERATING";
			}
			return string.Empty;
		}
		return "ITEM_INTERACTION_DESCRIPTION_OPERATE";
	}

	public virtual bool Interact(IPlayer player)
	{
		if (_vehicleMovement.StartOperating(player))
		{
			player.Movement.Vehicle = this;
			return true;
		}
		if (_vehicleMovement.StopOperating(player))
		{
			player.Movement.Vehicle = null;
			return true;
		}
		return false;
	}

	public virtual bool InteractWithObject(IPlayer player, IBase obj)
	{
		return Interact(player);
	}

	public override void Place()
	{
		if (Object.op_Implicit((Object)(object)_wallCollider))
		{
			_wallCollider.enabled = true;
		}
		base.Place();
		if (Object.op_Implicit((Object)(object)_wallCollider))
		{
			_wallCollider.enabled = false;
		}
		VehicleMovement.Rigidbody = Structure.rigidbody;
	}

	public override void Destroy()
	{
		if (BeamTeam.IsValid(_vehicleMovement.Operator))
		{
			Interact(_vehicleMovement.Operator);
		}
		if (Object.op_Implicit((Object)(object)_wallCollider))
		{
			_wallCollider.enabled = true;
		}
		base.Destroy();
	}

	protected override Placement CheckPlacement(Placement placement)
	{
		return placement;
	}

	protected override void Start()
	{
		base.Start();
		EventManager.AddListener<ServerJoinInProgressStarted>(OnServerJoinInProgressStarted);
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<ServerJoinInProgressStarted>(OnServerJoinInProgressStarted);
	}

	private void OnServerJoinInProgressStarted(ServerJoinInProgressStarted @event)
	{
		if (!BeamTeam.IsNullOrDestroyed(PlayerRegistry.LocalPlayer) && PlayerRegistry.LocalPlayer.IsInitialized && !BeamTeam.IsNullOrDestroyed(PlayerRegistry.LocalPlayer.Movement) && !BeamTeam.IsNullOrDestroyed(_vehicleMovement) && _vehicleMovement.Operator == PlayerRegistry.LocalPlayer)
		{
			Interact(PlayerRegistry.LocalPlayer);
		}
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
		{
			VehicleMovement.Rigidbody = Structure.rigidbody;
		});
	}
}
