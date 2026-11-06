using System;
using System.Collections;
using System.Collections.Generic;
using Beam.Events;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.Crafting;

public class Construction_CAMPFIRE : Constructing, IInteractable, IBase, IHasCraftingType, IIgnitable, IFlammable, IHaveAttribute
{
	[SerializeField]
	private bool _isOnUsing;

	private const float MAXIMUM_FUEL_HOURS = 12f;

	private const float REFUEL_THRESHOLD = 10.799999f;

	[Header("INTERACTING")]
	[SerializeField]
	private bool _canInteract;

	private PlayerSkills.ExpSource _skillExpSource = PlayerSkills.ExpSource.CAMPFIRE_COOKING;

	private float _proximityCheckTimeleft;

	[Header("ATTRIBUTE")]
	[SerializeField]
	private IconType _type;

	[SerializeField]
	private Transform _attachedPosition;

	[SerializeField]
	private GameObject _attachmentModel;

	private bool _isBurning;

	private bool _isCooking;

	[SerializeField]
	private Transform[] _sticksTransforms;

	[SerializeField]
	private ParticleSystem[] _particles;

	[SerializeField]
	private GameObject _light;

	[SerializeField]
	private float _fuelHours = 12f;

	private float _efficiency = 1f;

	private float _burningPerHour = 1f;

	private float _emptyFuelStickPosition = -0.24f;

	private List<Cooking> _cookables = new List<Cooking>();

	private bool _ignoreCookableCollisions;

	[HideInInspector]
	public BaseActionUnityEvent Ignited;

	[HideInInspector]
	public InteracterUnityEvent _ignitedTrophy;

	[HideInInspector]
	public BaseActionUnityEvent Extinguished;

	[HideInInspector]
	public BaseActionUnityEvent OnAttach;

	[HideInInspector]
	public BaseActionUnityEvent Refuelled;

	[HideInInspector]
	public BaseActionUnityEvent StartedCooking;

	[HideInInspector]
	public BaseActionUnityEvent StoppedCooking;

	[SerializeField]
	public bool IsOnUsing
	{
		get
		{
			return _isOnUsing;
		}
		set
		{
			_isOnUsing = value;
		}
	}

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

	public InteractiveObject_FOOD Food { get; set; }

	public bool IsBurning => _isBurning;

	public float FuelHours => _fuelHours;

	public bool IgnoreCookableCollisionsWhileLoading
	{
		get
		{
			return _ignoreCookableCollisions;
		}
		set
		{
			_ignoreCookableCollisions = value;
		}
	}

	public event FoodEventHandler AttachedEvent;

	public event EmptyEventHandler DetachedEvent;

	public event EmptyEventHandler FireStarted;

	public event EmptyEventHandler FireStopped;

	public event FloatEventHandler FireBurned;

	string IInteractable.GetInteractionDescription(int playerId, IBase obj)
	{
		if (BeamTeam.IsNullOrDestroyed(obj))
		{
			return GetInteractionDescription();
		}
		if (_fuelHours < 10.799999f)
		{
			InteractiveObject io = obj as InteractiveObject;
			if (IsRefillable(io))
			{
				return "ITEM_INTERACTION_DESCRIPTION_REFILL";
			}
		}
		if (_isBurning)
		{
			if (!BeamTeam.IsNullOrDestroyed(obj as IIgnitable))
			{
				return "ITEM_INTERACTION_DESCRIPTION_IGNITE";
			}
		}
		else
		{
			IFlammable flammable = obj as IFlammable;
			if (!BeamTeam.IsNullOrDestroyed(flammable) && flammable.IsBurning)
			{
				return "ITEM_INTERACTION_DESCRIPTION_LIGHT";
			}
		}
		InteractiveObject_FOOD interactiveObject_FOOD = obj as InteractiveObject_FOOD;
		Constructing_FUEL_STILL_BOILER componentInChildren = ((Component)transform.parent).GetComponentInChildren<Constructing_FUEL_STILL_BOILER>();
		if ((Object)(object)interactiveObject_FOOD != (Object)null && interactiveObject_FOOD.CanAttach && (Object)(object)componentInChildren == (Object)null && (Object)(object)Food == (Object)null)
		{
			if (interactiveObject_FOOD.Spoiled || (Object)(object)interactiveObject_FOOD.Cooking == (Object)null || ((Object)(object)interactiveObject_FOOD.Cooking != (Object)null && interactiveObject_FOOD.Cooking.IsCooked) || ((Object)(object)interactiveObject_FOOD.Smoking != (Object)null && interactiveObject_FOOD.Smoking.IsSmoked))
			{
				return GetInteractionDescription();
			}
			return "ITEM_INTERACTION_DESCRIPTION_ATTACH";
		}
		return GetInteractionDescription();
	}

	private string GetInteractionDescription()
	{
		if (!_isBurning)
		{
			return string.Empty;
		}
		return "ITEM_INTERACTION_DESCRIPTION_EXTINGUISH";
	}

	float IHaveAttribute.GetValue()
	{
		return _fuelHours / 12f;
	}

	protected virtual void OnAttached(InteractiveObject_FOOD food)
	{
		AttachedEvent?.Invoke(food);
	}

	protected virtual void OnDetached()
	{
		DetachedEvent?.Invoke();
	}

	protected virtual void OnFireStarted(IPlayer player)
	{
		FireStarted?.Invoke();
		ActionEventData.Populate(this, null);
		((UnityEvent<IBase, IBaseActionEventData>)Ignited)?.Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
		if (BeamTeam.IsValid(player))
		{
			((UnityEvent<IPlayer>)_ignitedTrophy)?.Invoke(player);
		}
	}

	protected virtual void OnFireStopped()
	{
		FireStopped?.Invoke();
		ActionEventData.Populate(this, null);
		((UnityEvent<IBase, IBaseActionEventData>)Extinguished)?.Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
	}

	protected virtual void OnFireBurned(float hours)
	{
		FireBurned?.Invoke(hours);
	}

	protected override void Start()
	{
		base.Start();
		_attachmentModel.SetActive(false);
		if (!LevelLoader.IsLoading())
		{
			Initialize();
		}
	}

	private void Selecting_Selected(IPlayer player)
	{
		if (_fuelHours > 0f)
		{
			DisplayNamePrefixes.Remove("ITEM_DISPLAY_NAME_PREFIX_EMPTY");
		}
		else
		{
			DisplayNamePrefixes.AddOrIgnore("ITEM_DISPLAY_NAME_PREFIX_EMPTY", -1);
		}
	}

	private void Initialize()
	{
		if (Object.op_Implicit((Object)(object)Singleton<GameTime>.Instance))
		{
			Singleton<GameTime>.Instance.HourChanged += GameTime_HourChanged;
		}
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)Singleton<GameTime>.Instance))
		{
			Singleton<GameTime>.Instance.HourChanged -= GameTime_HourChanged;
		}
		FireStarted = null;
		FireStopped = null;
		FireBurned = null;
	}

	public override void Place()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		base.Place();
		EventManager.RaiseEvent(new CampFirePlacedEvent(transform.position));
	}

	public bool Interact(IPlayer player)
	{
		Debug.Log((object)"Interacting");
		if (_isBurning)
		{
			Extinguish();
			return true;
		}
		return false;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		if (BeamTeam.IsNullOrDestroyed(obj))
		{
			return Interact(player);
		}
		if ((Object)(object)Food == (Object)null)
		{
			InteractiveObject_FOOD interactiveObject_FOOD = obj as InteractiveObject_FOOD;
			if ((Object)(object)interactiveObject_FOOD != (Object)null && interactiveObject_FOOD.CanAttach && Object.op_Implicit((Object)(object)interactiveObject_FOOD.Cooking))
			{
				if (interactiveObject_FOOD.Spoiled || interactiveObject_FOOD.Cooking.IsCooked || ((Object)(object)interactiveObject_FOOD.Smoking != (Object)null && interactiveObject_FOOD.Smoking.IsSmoked))
				{
					return Interact(player);
				}
				player.Holder.DropCurrent();
				Attach(interactiveObject_FOOD, player.Id);
				return true;
			}
		}
		if (_fuelHours < 10.799999f)
		{
			InteractiveObject io = obj as InteractiveObject;
			if (TryConsumeObjectAsFuel(io, player))
			{
				return true;
			}
		}
		if (_isBurning)
		{
			IIgnitable ignitable = obj as IIgnitable;
			if (!BeamTeam.IsNullOrDestroyed(ignitable))
			{
				ignitable.Ignite();
				return true;
			}
		}
		else
		{
			IFlammable flammable = obj as IFlammable;
			if (!BeamTeam.IsNullOrDestroyed(flammable) && flammable.IsBurning)
			{
				Ignite(player);
				return true;
			}
		}
		return Interact(player);
	}

	private void Attach(InteractiveObject_FOOD food, int playerID)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		Food = food;
		if (!((Object)(object)Food.Cooking == (Object)null))
		{
			if (_isBurning && !Food.Cooking.IsCooked)
			{
				Food.Cooking.IsBeingCooked = true;
			}
			Food.transform.position = _attachedPosition.position - Food.AttachingAnchor.GetPosition();
			Food.transform.rotation = _attachedPosition.rotation * Food.AttachingAnchor.GetRotation();
			Food.Attach(new StaticAttachingAnchor(_attachedPosition));
			Food.DetachedEvent += Food_Detached;
			AddCookable(Food.Cooking, playerID);
			ActionEventData.Populate(this, null);
			((UnityEvent<IBase, IBaseActionEventData>)OnAttach)?.Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
			_attachmentModel.SetActive(true);
			OnAttached(Food);
		}
	}

	private void Detach()
	{
		if ((Object)(object)Food != (Object)null)
		{
			if (Object.op_Implicit((Object)(object)Food.Cooking))
			{
				Food.Cooking.IsBeingCooked = false;
				RemoveCookable(Food.Cooking);
			}
			_attachmentModel.SetActive(false);
			Food.DetachedEvent -= Food_Detached;
			Food = null;
		}
	}

	private void Food_Detached(IAttachable sender)
	{
		Detach();
	}

	public void Ignite(IPlayer player)
	{
		if (!_isBurning && _fuelHours > 0f)
		{
			((MonoBehaviour)this).StartCoroutine(Burn(player));
		}
	}

	public void Ignite()
	{
		Ignite(null);
	}

	private IEnumerator Burn(IPlayer player = null)
	{
		_isBurning = true;
		OnFireStarted(player);
		_light.SetActive(true);
		for (int i = 0; i < _particles.Length; i++)
		{
			_particles[i].Play();
		}
		while (_isBurning)
		{
			if (_cookables.Count > 0)
			{
				if (!_isCooking)
				{
					ActionEventData.Populate(this, null);
					((UnityEvent<IBase, IBaseActionEventData>)StartedCooking)?.Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
					_isCooking = true;
				}
				for (int j = 0; j < _cookables.Count; j++)
				{
					if (_cookables[j].Food.IsPickedUp)
					{
						_cookables[j].IsBeingCooked = false;
						_cookables.RemoveAt(j);
						j--;
					}
				}
				for (int k = 0; k < _cookables.Count; k++)
				{
					if (!_cookables[k].IsCooked)
					{
						_cookables[k].IsBeingCooked = true;
					}
					_cookables[k].Burn(_burningPerHour);
				}
				_proximityCheckTimeleft -= Time.deltaTime;
				if (_proximityCheckTimeleft <= 0f)
				{
					_proximityCheckTimeleft = 1f;
					CheckPlayersInProximity();
				}
			}
			else if (_isCooking)
			{
				ActionEventData.Populate(this, null);
				((UnityEvent<IBase, IBaseActionEventData>)StoppedCooking)?.Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
				_isCooking = false;
			}
			RemoveFuel();
			yield return null;
		}
	}

	private void CheckPlayersInProximity()
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if (!PlayerInProximity(allPlayer))
			{
				continue;
			}
			for (int i = 0; i < _cookables.Count; i++)
			{
				if (!_cookables[i].IsCooked)
				{
					allPlayer.PlayerSkills.ReplicatedAddCooking(_skillExpSource);
				}
			}
		}
	}

	private bool PlayerInProximity(IPlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.SqrMagnitude(player.transform.position - transform.position) <= BeamMath.Sqr(8f);
	}

	private void BurnManual(float hours)
	{
		float hours2 = Mathf.Clamp(_fuelHours, 0f, hours);
		for (int i = 0; i < _cookables.Count; i++)
		{
			_cookables[i].BurnManual(hours2);
		}
		OnFireBurned(hours2);
		_fuelHours -= hours;
		_fuelHours = Mathf.Clamp(_fuelHours, 0f, 12f);
	}

	private bool TryConsumeObjectAsFuel(InteractiveObject io, IPlayer player)
	{
		if ((Object)(object)io == (Object)null)
		{
			return false;
		}
		if (!IsRefillable(io))
		{
			return false;
		}
		_fuelHours += io.Burning.FuelHours;
		_fuelHours = Mathf.Clamp(_fuelHours, 0f, 12f);
		ActionEventData.Populate(this, io);
		((UnityEvent<IBase, IBaseActionEventData>)Refuelled)?.Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
		RepositionSticks();
		player.Holder.ReleaseAndDestroy(io);
		return true;
	}

	private void RemoveFuel()
	{
		_fuelHours -= _efficiency * GameTime.DeltaTimeHour;
		if (_fuelHours <= 0f)
		{
			Extinguish();
		}
		RepositionSticks();
	}

	public bool AddCookable(Cooking cookable, int cookerID = -1)
	{
		if (Beam.Utilities.ExtensionMethods.DoesntContain(_cookables, cookable))
		{
			_cookables.Add(cookable);
			cookable.PlayerChefID = cookerID;
			if ((Object)(object)cookable.Food.Smoking != (Object)null)
			{
				cookable.Food.Smoking.PlayerChefID = cookerID;
			}
			return true;
		}
		return false;
	}

	public bool RemoveCookable(Cooking cookable)
	{
		cookable.PlayerChefID = -1;
		return _cookables.Remove(cookable);
	}

	private void GameTime_HourChanged(float hours)
	{
		if (_isBurning)
		{
			BurnManual(hours);
		}
	}

	private void Extinguish()
	{
		if (_isBurning)
		{
			for (int i = 0; i < _cookables.Count; i++)
			{
				_cookables[i].IsBeingCooked = false;
			}
			_isBurning = false;
			OnFireStopped();
			_light.SetActive(false);
			for (int j = 0; j < _particles.Length; j++)
			{
				_particles[j].Stop();
			}
		}
	}

	private void RepositionSticks()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		float num = _emptyFuelStickPosition + Mathf.Abs(_emptyFuelStickPosition) / (12f / _fuelHours);
		num = Mathf.Clamp(num, _emptyFuelStickPosition, 0f);
		Transform[] sticksTransforms = _sticksTransforms;
		foreach (Transform obj in sticksTransforms)
		{
			Vector3 localPosition = obj.localPosition;
			localPosition.y = num;
			obj.localPosition = localPosition;
		}
	}

	private bool IsRefillable(InteractiveObject io)
	{
		if ((Object)(object)io != (Object)null && (Object)(object)io.Burning != (Object)null && io.Burning.FuelHours > 0f)
		{
			return !(io is InteractiveObject_FOOD);
		}
		return false;
	}

	public override void Damage(float damage, IBase damager)
	{
		if (_isOnUsing)
		{
			return;
		}
		IPickupable pickupable = damager as IPickupable;
		if (BeamTeam.IsNullOrDestroyed(pickupable))
		{
			return;
		}
		IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(pickupable.OwnerId);
		if (BeamTeam.IsNullOrDestroyed(playerFromId))
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)Food))
		{
			LocalizedNotification.Post(playerFromId, NotificationPriority.Low, 4f, "CONSTRUCTING_NTF_CANT_DESTROY_WITH_ATTACHED_DESC");
			return;
		}
		for (int i = 0; i < MyConnectors.Length; i++)
		{
			if (!MyConnectors[i].IsValid && MyConnectors[i].Constructions.Count > 0 && MyConnectors[i].Constructions[0].CraftingType.InteractiveType != InteractiveType.CRAFTING_FIRE)
			{
				LocalizedNotification.Post(playerFromId, NotificationPriority.Low, 4f, "CONSTRUCTING_NTF_CANT_DESTROY_ANOTHER_DEPENDS_DESC");
				return;
			}
		}
		base.Damage(damage, damager);
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("FuelHours", JSerializer.Serialize((object)_fuelHours));
		jObject.AddField("IsBurning", JSerializer.Serialize((object)_isBurning));
		jObject.AddField("TimeLeft", JSerializer.Serialize((object)GameTime.Now));
		if (_cookables.Count > 0)
		{
			JObject jObject2 = new JObject();
			for (int i = 0; i < _cookables.Count; i++)
			{
				jObject2.Add(_cookables[i].Food.ReferenceId.ToString());
			}
			jObject.AddField("Cookables", jObject2);
		}
		if ((Object)(object)Food != (Object)null)
		{
			jObject.AddField("PlayerChefID", Food.Cooking.PlayerChefID);
			jObject.AddField("Food", Food.ReferenceId.ToString());
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JObject field = data.GetField("FuelHours");
		_fuelHours = field.GetValue<float>();
		JObject field2 = data.GetField("IsBurning");
		_isBurning = field2.GetValue<bool>();
		if (_isBurning)
		{
			DateTime a = JSerializer.Deserialize<DateTime>(data.GetField("TimeLeft"));
			float elapsed = GameTime.GetHoursSince(a);
			LevelLoader.CurrentLoader.PostLoading.Add(() =>
			{
				BurnManual(elapsed);
				if (_fuelHours <= 0f)
				{
					Extinguish();
					RepositionSticks();
				}
				else if (_fuelHours > 0f)
				{
					((MonoBehaviour)this).StartCoroutine(Burn());
				}
			});
		}
		JObject foodReferenceData = data.GetField("Food");
		if (foodReferenceData != null)
		{
			JObject field3 = data.GetField("PlayerChefID");
			int cookerID = ((field3 != null) ? field3.GetValue<int>() : (-1));
			LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
			{
				MiniGuid referenceId = Beam.Utilities.ExtensionMethods.ToMiniGuid(foodReferenceData.GetValue<string>());
				Food = MultiplayerMng.FindObject<InteractiveObject_FOOD>(referenceId);
				if ((Object)(object)Food != (Object)null)
				{
					Attach(Food, cookerID);
				}
				else
				{
					Debug.LogError((object)"Constructing_Campfire::Load:: Error finding food.");
				}
				IgnoreCookableCollisionsWhileLoading = true;
			});
		}
		Initialize();
		RepositionSticks();
	}
}
