using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.Crafting;

[RequireComponent(typeof(PaddleVehicleMovement))]
public class InteractiveObject_LIFERAFT : InteractiveObject, IVehicle, IBase, IHasCraftingType
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private VehicleMovementBase _vehicleMovement;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Buoyancy _buoyancy;

	[PREFAB_PROCESSOR_GetComponentInChildren]
	[SerializeField]
	private Constructing_HOOK _paddleHook;

	[PREFAB_PROCESSOR_GetComponentInChildren]
	[SerializeField]
	private Interactive_SEA_ANCHOR _seaAnchor;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Interactive_RAFT_STORAGE _raftStorage;

	[SerializeField]
	[HideInInspector]
	private UnityEvent _onOperated;

	[SerializeField]
	[HideInInspector]
	private UnityEvent _onDragged;

	public VehicleMovementBase VehicleMovement => _vehicleMovement;

	public Buoyancy Buoyancy => _buoyancy;

	public Constructing_HOOK PaddleHook => _paddleHook;

	public override string GetInteractionDescription(int playerId, IBase obj)
	{
		IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(playerId);
		if ((Object)(object)playerFromId.Movement.ActivePlatform != (Object)(object)transform)
		{
			return string.Empty;
		}
		if (_vehicleMovement.IsOverturned() && !_vehicleMovement.Operating)
		{
			return string.Empty;
		}
		if (!BeamTeam.IsNullOrDestroyed(obj) && obj.CraftingType.InteractiveType == InteractiveType.TOOLS_PADDLE)
		{
			if (_vehicleMovement.Operating)
			{
				if (playerFromId == VehicleMovement.Operator)
				{
					return "ITEM_INTERACTION_DESCRIPTION_STOP_OPERATING";
				}
				return string.Empty;
			}
			return "ITEM_INTERACTION_DESCRIPTION_OPERATE";
		}
		if (_vehicleMovement.GetPassengerSeat(playerId) != null)
		{
			return "ITEM_INTERACTION_DESCRIPTION_STOP_RIDING";
		}
		if (_vehicleMovement.GetPassengerSeat() != null)
		{
			return "ITEM_INTERACTION_DESCRIPTION_RIDE_ALONG";
		}
		return string.Empty;
	}

	protected override void Awake()
	{
		base.Awake();
		VehicleMovement.Rigidbody = rigidbody;
	}

	protected override void Start()
	{
		base.Start();
		_vehicleMovement.Operated += VehicleMovement_Operated;
		if (!IsOwner)
		{
			PaddleVehicleMovement component = ((Component)this).gameObject.GetComponent<PaddleVehicleMovement>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((Behaviour)component).enabled = false;
			}
			Buoyancy component2 = ((Component)this).gameObject.GetComponent<Buoyancy>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				((Behaviour)component2).enabled = false;
			}
			MovingPlatform component3 = ((Component)this).gameObject.GetComponent<MovingPlatform>();
			if (Object.op_Implicit((Object)(object)component3))
			{
				((Behaviour)component3).enabled = false;
			}
		}
	}

	public override void OnDestroy()
	{
		_vehicleMovement.Operated -= VehicleMovement_Operated;
		base.OnDestroy();
	}

	public override void BeginDrag()
	{
		UnityEvent onDragged = _onDragged;
		if (onDragged != null)
		{
			onDragged.Invoke();
		}
		base.BeginDrag();
	}

	private void VehicleMovement_Operated(bool operated)
	{
		if (operated && _seaAnchor.Deployed)
		{
			_seaAnchor.Retrieve();
		}
		ShowPaddle(_vehicleMovement.Operator, operated);
		UnityEvent onOperated = _onOperated;
		if (onOperated != null)
		{
			onOperated.Invoke();
		}
	}

	public override bool Interact(IPlayer player)
	{
		if (_vehicleMovement.StartRiding(player))
		{
			player.Movement.Vehicle = this;
			return true;
		}
		if (_vehicleMovement.StopRiding(player))
		{
			ShowPaddle(player, show: false);
			player.Movement.Vehicle = null;
			return true;
		}
		return false;
	}

	public override bool InteractWithObject(IPlayer player, IBase obj)
	{
		if ((Object)(object)(obj as Interactive_PADDLE) == (Object)null)
		{
			return Interact(player);
		}
		if (_vehicleMovement.StartOperating(player))
		{
			player.Movement.Vehicle = this;
			return true;
		}
		if (_vehicleMovement.StopOperating(player))
		{
			player.Movement.Vehicle = null;
			ShowPaddle(player, show: false);
			return true;
		}
		if (_vehicleMovement.StartRiding(player))
		{
			player.Movement.Vehicle = this;
			return true;
		}
		if (_vehicleMovement.StopRiding(player))
		{
			player.Movement.Vehicle = null;
			return true;
		}
		return false;
	}

	private void ShowPaddle(IPlayer player, bool show)
	{
		GameObject proxy = player.Holder.Proxies.GetProxy(new CraftingType(AttributeType.None, InteractiveType.TOOLS_PADDLE));
		if ((Object)(object)proxy != (Object)null)
		{
			if (show && !proxy.activeSelf)
			{
				player.Holder.HideCurrent();
				proxy.SetActive(true);
			}
			else if (!show && proxy.activeSelf)
			{
				player.Holder.ShowCurrent();
				proxy.SetActive(false);
			}
		}
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		if (!BeamTeam.IsNullOrDestroyed(_paddleHook.Attached1))
		{
			JObject jObject2 = SavingUtilities.Save(_paddleHook);
			if (jObject2 != null && !jObject2.IsNull())
			{
				jObject.AddField("Paddle", jObject2);
			}
		}
		JObject jObject3 = SavingUtilities.Save(_seaAnchor);
		if (jObject3 != null && !jObject3.IsNull())
		{
			jObject.AddField("Anchor", jObject3);
		}
		JObject jObject4 = _raftStorage.Save();
		if (jObject4 != null && !jObject4.IsNull())
		{
			jObject.AddField("Storage", jObject4);
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JObject field = data.GetField("Paddle");
		if (field != null)
		{
			SavingUtilities.Load(_paddleHook, field);
		}
		JObject field2 = data.GetField("Anchor");
		if (field2 != null)
		{
			SavingUtilities.Load(_seaAnchor, field2);
		}
		JObject field3 = data.GetField("Storage");
		if (field3 != null)
		{
			SavingUtilities.Load(_raftStorage, field3);
		}
	}
}
