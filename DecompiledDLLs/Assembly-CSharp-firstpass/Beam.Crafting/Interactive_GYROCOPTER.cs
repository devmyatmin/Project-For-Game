using System;
using System.Runtime.CompilerServices;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam.Crafting;

public class Interactive_GYROCOPTER : BaseObject, IInteractable, IBase, IHasCraftingType, ISelectable, IDraggable, IPhysical, IVehicle, IHaveAttribute
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Selecting _selecting;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private VehicleMovementBase _vehicleMovement;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Rigidbody _rigidbody;

	private IHaveFuel _movementFuel;

	private bool _operating;

	public Selecting Selecting => _selecting;

	public IconType Type => IconType.Fuel;

	public bool CanInteract { get; set; } = true;

	public bool IsKinematic { get; }

	CollisionDetectionMode IPhysical.CollisionDetectionMode
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public bool CanDrag { get; set; } = true;

	public bool IsDragging { get; set; }

	public float CurrentDrag { get; set; }

	public float CurrentAngularDrag { get; set; }

	public VehicleMovementBase VehicleMovement => _vehicleMovement;

	public Rigidbody rigidbody => _rigidbody;

	public IPlayer Operator { get; set; }

	public event Action<IDraggable> BeginDragged;

	public event Action<IDraggable> EndDragged;

	public float GetValue()
	{
		return _movementFuel.Fuel / _movementFuel.FuelCapacity;
	}

	public string GetInteractionDescription(int playerId, IBase obj)
	{
		if (_vehicleMovement.IsOverturned() && !_operating)
		{
			return string.Empty;
		}
		if (!BeamTeam.IsNullOrDestroyed(obj) && obj.CraftingType.InteractiveType == InteractiveType.TOOLS_JERRYCAN)
		{
			if (_movementFuel.CanRefill())
			{
				InteractiveObject_FUELCAN interactiveObject_FUELCAN = obj as InteractiveObject_FUELCAN;
				if ((Object)(object)interactiveObject_FUELCAN != (Object)null && interactiveObject_FUELCAN.Fuel > 0f)
				{
					return "ITEM_INTERACTION_DESCRIPTION_REFUEL";
				}
			}
			return string.Empty;
		}
		IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(playerId);
		if (_vehicleMovement.Operating)
		{
			if (playerFromId == _vehicleMovement.Operator)
			{
				return "ITEM_INTERACTION_DESCRIPTION_STOP_ENGINE";
			}
			return string.Empty;
		}
		return "ITEM_INTERACTION_DESCRIPTION_START_ENGINE";
	}

	public void EnablePhysics()
	{
	}

	public void DisablePhysics()
	{
	}

	public virtual void BeginDrag()
	{
		BeginDragged?.Invoke(this);
	}

	public virtual void EndDrag()
	{
		EndDragged?.Invoke(this);
	}

	protected override void Awake()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		_vehicleMovement.Rigidbody = _rigidbody;
		_movementFuel = (IHaveFuel)VehicleMovement;
		this.InitializeKinematic(IsKinematic, MPExtensions.IsMaster(Game.Mode));
	}

	public override void Attached()
	{
		base.Attached();
		this.InitializeKinematic(IsKinematic, IsOwner);
	}

	public override void OnCustomMapLoaded()
	{
		base.OnCustomMapLoaded();
		this.SetKinematic(IsKinematic, IsOwner);
	}

	protected override void Start()
	{
		base.Start();
		_vehicleMovement.Operated += VehicleMovement_Operated;
	}

	private void OnDestroy()
	{
		_vehicleMovement.Operated -= VehicleMovement_Operated;
	}

	private void VehicleMovement_Operated(bool value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode) && !PlayerUtilities.AllPeersPresent())
		{
			MultiplayerMng.UpdateSessionToken_IsBusy(value);
		}
		_operating = value;
	}

	public bool Interact(IPlayer player)
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

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		if (!BeamTeam.IsNullOrDestroyed(obj) && obj.CraftingType.InteractiveType == InteractiveType.TOOLS_JERRYCAN)
		{
			if (_movementFuel.CanRefill())
			{
				InteractiveObject_FUELCAN interactiveObject_FUELCAN = obj as InteractiveObject_FUELCAN;
				if ((Object)(object)interactiveObject_FUELCAN != (Object)null && interactiveObject_FUELCAN.Fuel > 0f)
				{
					interactiveObject_FUELCAN.Fill(_movementFuel);
					return true;
				}
			}
			return false;
		}
		return Interact(player);
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		JObject data = JSerializer.Serialize((Component)(object)_rigidbody);
		jObject.AddField("Rigidbody", data);
		jObject.AddField("Fuel", _movementFuel.Fuel);
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		LevelLoader.CurrentLoader.PhysicalLoading.Add(() =>
		{
			this.LoadRigidbody(data);
		});
		JObject field = data.GetField("Fuel");
		if (field != null)
		{
			float value = field.GetValue<float>();
			_movementFuel.Refill(value);
		}
	}
}
