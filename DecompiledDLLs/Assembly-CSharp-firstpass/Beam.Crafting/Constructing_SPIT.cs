using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam.Crafting;

public class Constructing_SPIT : Constructing, IInteractable, IBase, IHasCraftingType
{
	[Header("INTERACTING")]
	[SerializeField]
	private bool _canInteract;

	[SerializeField]
	private Transform _attachedPosition;

	private bool _isCooking;

	private Construction_CAMPFIRE _fire;

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

	public InteractiveObject_FOOD Food { get; set; }

	public bool IsCooking => _isCooking;

	public event FoodEventHandler AttachedEvent;

	public event EmptyEventHandler DetachedEvent;

	public event FoodEventHandler StartedCooking;

	public event EmptyEventHandler StoppedCooking;

	public event FloatEventHandler Burned;

	string IInteractable.GetInteractionDescription(int playerId, IBase obj)
	{
		if (BeamTeam.IsNullOrDestroyed(obj))
		{
			return string.Empty;
		}
		InteractiveObject_FOOD interactiveObject_FOOD = obj as InteractiveObject_FOOD;
		if ((Object)(object)interactiveObject_FOOD != (Object)null && interactiveObject_FOOD.CanAttach)
		{
			if (interactiveObject_FOOD.Spoiled || (Object)(object)interactiveObject_FOOD.Cooking == (Object)null || ((Object)(object)interactiveObject_FOOD.Cooking != (Object)null && interactiveObject_FOOD.Cooking.IsCooked) || ((Object)(object)interactiveObject_FOOD.Smoking != (Object)null && interactiveObject_FOOD.Smoking.IsSmoked))
			{
				return GetInteractionDescription();
			}
			if (!((Object)(object)Food != (Object)null))
			{
				return "ITEM_INTERACTION_DESCRIPTION_ATTACH";
			}
			return "ITEM_INTERACTION_DESCRIPTION_DETACH";
		}
		return GetInteractionDescription();
	}

	private string GetInteractionDescription()
	{
		if (!((Object)(object)Food != (Object)null))
		{
			return string.Empty;
		}
		return "ITEM_INTERACTION_DESCRIPTION_DETACH";
	}

	protected virtual void OnAttached(InteractiveObject_FOOD food)
	{
		AttachedEvent?.Invoke(food);
	}

	protected virtual void OnDetached()
	{
		DetachedEvent?.Invoke();
	}

	protected virtual void OnStartedCooking(InteractiveObject_FOOD food)
	{
		StartedCooking?.Invoke(food);
	}

	protected virtual void OnStoppedCooking()
	{
		StoppedCooking?.Invoke();
	}

	protected virtual void OnBurnt(float hours)
	{
		Burned?.Invoke(hours);
	}

	public override void Place()
	{
		base.Place();
		Construction_CAMPFIRE construction = Structure.GetConstruction<Construction_CAMPFIRE>();
		Initialize(construction);
	}

	private void Initialize(Construction_CAMPFIRE fire)
	{
		_fire = fire;
		_fire.FireStarted += Fire_FireStarted;
		_fire.FireStopped += Fire_FireStopped;
		_fire.FireBurned += Fire_FireBurned;
		if (_fire.IsBurning)
		{
			Fire_FireStarted();
		}
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)_fire))
		{
			_fire.FireStarted -= Fire_FireStarted;
			_fire.FireStopped -= Fire_FireStopped;
			_fire.FireBurned -= Fire_FireBurned;
		}
		AttachedEvent = null;
		DetachedEvent = null;
		StartedCooking = null;
		StoppedCooking = null;
		Burned = null;
	}

	private void Fire_FireStarted()
	{
		_isCooking = true;
		OnStartedCooking(Food);
	}

	private void Fire_FireStopped()
	{
		_isCooking = false;
		OnStoppedCooking();
	}

	private void Fire_FireBurned(float hours)
	{
		OnBurnt(hours);
	}

	public bool Interact(IPlayer player)
	{
		return false;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		if ((Object)(object)Food != (Object)null)
		{
			return false;
		}
		InteractiveObject_FOOD interactiveObject_FOOD = obj as InteractiveObject_FOOD;
		if ((Object)(object)interactiveObject_FOOD != (Object)null && interactiveObject_FOOD.CanAttach)
		{
			if (interactiveObject_FOOD.Spoiled || (Object)(object)interactiveObject_FOOD.Cooking == (Object)null || ((Object)(object)interactiveObject_FOOD.Cooking != (Object)null && interactiveObject_FOOD.Cooking.IsCooked) || ((Object)(object)interactiveObject_FOOD.Smoking != (Object)null && interactiveObject_FOOD.Smoking.IsSmoked))
			{
				return false;
			}
			player.Holder.DropCurrent();
			Attach(interactiveObject_FOOD);
			return true;
		}
		return false;
	}

	private void Attach(InteractiveObject_FOOD food)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		Food = food;
		Food.transform.position = _attachedPosition.position - Food.AttachingAnchor.GetPosition();
		Food.transform.rotation = _attachedPosition.rotation * Food.AttachingAnchor.GetRotation();
		Food.Attach(new StaticAttachingAnchor(_attachedPosition));
		Food.DetachedEvent += Food_Detached;
		if (Object.op_Implicit((Object)(object)Food.Cooking))
		{
			if (_isCooking)
			{
				Food.Cooking.IsBeingCooked = true;
			}
			if ((Object)(object)_fire != (Object)null)
			{
				_fire.AddCookable(Food.Cooking, Food.PreviousOwnerId);
			}
		}
		CanInteract = false;
		OnAttached(Food);
	}

	private void Detach()
	{
		if (!((Object)(object)Food != (Object)null))
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)Food.Cooking))
		{
			Food.Cooking.IsBeingCooked = false;
			if ((Object)(object)_fire != (Object)null)
			{
				_fire.RemoveCookable(Food.Cooking);
			}
		}
		Food.DetachedEvent -= Food_Detached;
		Food = null;
		CanInteract = true;
	}

	private void Food_Detached(IAttachable sender)
	{
		Detach();
	}

	public override void Damage(float damage, IBase damager)
	{
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
			if (!MyConnectors[i].IsValid)
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
		jObject.AddField("Fire", _fire.ReferenceId.ToString());
		if ((Object)(object)Food != (Object)null)
		{
			jObject.AddField("Food", Food.ReferenceId.ToString());
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		string fireReferenceId = data.GetField("Fire").GetValue<string>();
		LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
		{
			_fire = Structure.transform.FindObject<Construction_CAMPFIRE>(Beam.Utilities.ExtensionMethods.ToMiniGuid(fireReferenceId));
			if ((Object)(object)_fire != (Object)null)
			{
				Initialize(_fire);
			}
			else
			{
				Debug.LogError((object)"Constructing_SPIT::Load:: Error finding fire.");
			}
		});
		JObject foodReferenceData = data.GetField("Food");
		if (!foodReferenceData)
		{
			return;
		}
		LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
		{
			string value = foodReferenceData.GetValue<string>();
			Food = MultiplayerMng.FindObject<InteractiveObject_FOOD>(Beam.Utilities.ExtensionMethods.ToMiniGuid(value));
			if ((Object)(object)Food != (Object)null)
			{
				Attach(Food);
			}
			else
			{
				Debug.LogError((object)"Constructing_SPIT::Load:: Error finding food.");
			}
		});
	}
}
