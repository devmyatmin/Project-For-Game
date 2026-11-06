using Beam.Events;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam.Crafting;

public class Constructing_SAIL : Constructing, IInteractable, IBase, IHasCraftingType, IVehicle
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private VehicleMovementBase _vehicleMovement;

	[SerializeField]
	private SailVehicleAnimation _sailVehicleAnimation;

	public bool CanInteract { get; set; } = true;

	public VehicleMovementBase VehicleMovement => _vehicleMovement;

	string IInteractable.GetInteractionDescription(int playerId, IBase obj)
	{
		if (_vehicleMovement.IsOverturned())
		{
			return string.Empty;
		}
		if (!_vehicleMovement.Operating)
		{
			return "ITEM_INTERACTION_DESCRIPTION_LOWER";
		}
		return "ITEM_INTERACTION_DESCRIPTION_RAISE";
	}

	public override void Place()
	{
		base.Place();
		_vehicleMovement.Rigidbody = Structure.rigidbody;
	}

	public virtual bool Interact(IPlayer player)
	{
		if (!_vehicleMovement.Operating)
		{
			_vehicleMovement.Operate(player, operate: true);
			return true;
		}
		_vehicleMovement.Operate(player, operate: false);
		return true;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		return Interact(player);
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
		if (_vehicleMovement.Operating && !BeamTeam.IsNullOrDestroyed(PlayerRegistry.LocalPlayer) && PlayerRegistry.LocalPlayer.IsInitialized && !BeamTeam.IsNullOrDestroyed(PlayerRegistry.LocalPlayer.Movement) && !BeamTeam.IsNullOrDestroyed(Structure) && !((Object)(object)Structure.transform != (Object)(object)PlayerRegistry.LocalPlayer.Movement.ActivePlatform))
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
			_sailVehicleAnimation.PlaySailAnimation(raised: false);
		});
	}
}
