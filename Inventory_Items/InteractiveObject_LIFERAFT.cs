using System.Runtime.CompilerServices;
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
		if (playerFromId.Movement.ActivePlatform != base.transform)
		{
			return string.Empty;
		}
		if (_vehicleMovement.IsOverturned() && !_vehicleMovement.Operating)
		{
			return string.Empty;
		}
		if (!obj.IsNullOrDestroyed() && obj.CraftingType.InteractiveType == InteractiveType.TOOLS_PADDLE)
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
		VehicleMovement.Rigidbody = base.rigidbody;
	}

	protected override void Start()
	{
		base.Start();
		_vehicleMovement.Operated += VehicleMovement_Operated;
		if (!base.IsOwner)
		{
			PaddleVehicleMovement component = base.gameObject.GetComponent<PaddleVehicleMovement>();
			if ((bool)component)
			{
				component.enabled = false;
			}
			Buoyancy component2 = base.gameObject.GetComponent<Buoyancy>();
			if ((bool)component2)
			{
				component2.enabled = false;
			}
			MovingPlatform component3 = base.gameObject.GetComponent<MovingPlatform>();
			if ((bool)component3)
			{
				component3.enabled = false;
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
		_onDragged?.Invoke();
		base.BeginDrag();
	}

	private void VehicleMovement_Operated(bool operated)
	{
		if (operated && _seaAnchor.Deployed)
		{
			_seaAnchor.Retrieve();
		}
		ShowPaddle(_vehicleMovement.Operator, operated);
		_onOperated?.Invoke();
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
		if (obj as Interactive_PADDLE == null)
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
		if (proxy != null)
		{
			if (show && !proxy.activeSelf)
			{
				player.Holder.HideCurrent();
				proxy.SetActive(value: true);
			}
			else if (!show && proxy.activeSelf)
			{
				player.Holder.ShowCurrent();
				proxy.SetActive(value: false);
			}
		}
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		if (!_paddleHook.Attached1.IsNullOrDestroyed())
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

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}
}
