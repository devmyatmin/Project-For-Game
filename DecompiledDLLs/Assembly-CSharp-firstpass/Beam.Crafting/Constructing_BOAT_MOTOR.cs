using Beam.Serialization.Json;
using Beam.Utilities;
using UnityEngine;

namespace Beam.Crafting;

public class Constructing_BOAT_MOTOR : Constructing_VEHICLE, IHaveAttribute
{
	private IHaveFuel _movementFuel;

	public IconType Type => IconType.Fuel;

	public float GetValue()
	{
		return _movementFuel.Fuel / _movementFuel.FuelCapacity;
	}

	public override string GetInteractionDescription(int playerId, IBase obj)
	{
		if (string.IsNullOrEmpty(base.GetInteractionDescription(playerId, obj)))
		{
			return string.Empty;
		}
		if (VehicleMovement.IsOverturned() && !VehicleMovement.Operating)
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
		if (VehicleMovement.Operating)
		{
			if (playerFromId == VehicleMovement.Operator)
			{
				if (!(_movementFuel.Fuel > 0f))
				{
					return "ITEM_INTERACTION_DESCRIPTION_STOP_OPERATING";
				}
				return "ITEM_INTERACTION_DESCRIPTION_STOP_ENGINE";
			}
			return string.Empty;
		}
		if (!(_movementFuel.Fuel > 0f))
		{
			return "ITEM_INTERACTION_DESCRIPTION_OPERATE";
		}
		return "ITEM_INTERACTION_DESCRIPTION_START_ENGINE";
	}

	protected override void Awake()
	{
		_movementFuel = (IHaveFuel)VehicleMovement;
		base.Awake();
	}

	public override bool InteractWithObject(IPlayer player, IBase obj)
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
		return base.InteractWithObject(player, obj);
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("Fuel", _movementFuel.Fuel);
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JObject field = data.GetField("Fuel");
		if (field != null)
		{
			float value = field.GetValue<float>();
			_movementFuel.Refill(value);
		}
	}
}
