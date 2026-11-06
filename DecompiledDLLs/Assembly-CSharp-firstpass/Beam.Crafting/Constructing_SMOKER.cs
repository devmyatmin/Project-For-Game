using System;
using System.Collections.Generic;
using System.Linq;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using MEC;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.Crafting;

public class Constructing_SMOKER : Constructing, IInteractable, IBase, IHasCraftingType
{
	[Header("INTERACTING")]
	[SerializeField]
	private bool _canInteract;

	[SerializeField]
	private List<SmokableAttachingPosition> _attachingPositions;

	private Construction_CAMPFIRE _fire;

	private CoroutineHandle _smokingSequence;

	private bool _isFireOn;

	[SerializeField]
	private ParticleSystem _smokeParticle;

	[HideInInspector]
	public BaseActionUnityEvent OnAttach;

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

	string IInteractable.GetInteractionDescription(int playerId, IBase obj)
	{
		if (BeamTeam.IsNullOrDestroyed(obj))
		{
			return string.Empty;
		}
		ISmokable smokable = obj as ISmokable;
		if (BeamTeam.IsNullOrDestroyed(smokable) || (Object)(object)smokable.Smoking == (Object)null || smokable.Smoking.IsSmoked)
		{
			return string.Empty;
		}
		InteractiveObject_FOOD interactiveObject_FOOD = obj as InteractiveObject_FOOD;
		if (!BeamTeam.IsNullOrDestroyed(interactiveObject_FOOD) && interactiveObject_FOOD.Spoiled)
		{
			return string.Empty;
		}
		if (GetAvailableAttachingPosition() == null)
		{
			return string.Empty;
		}
		return "ITEM_INTERACTION_DESCRIPTION_ATTACH";
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
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_smokingSequence);
		if (Object.op_Implicit((Object)(object)_fire))
		{
			_fire.FireStarted -= Fire_FireStarted;
			_fire.FireStopped -= Fire_FireStopped;
			_fire.FireBurned -= Fire_FireBurned;
		}
	}

	public override void Damage(float damage, IBase damager)
	{
		IPickupable pickupable = damager as IPickupable;
		if (BeamTeam.IsNullOrDestroyed(pickupable))
		{
			return;
		}
		IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(pickupable.OwnerId);
		if (!BeamTeam.IsNullOrDestroyed(playerFromId))
		{
			if (Enumerable.Count(_attachingPositions, (SmokableAttachingPosition ap) => (Object)(object)ap.Smokable != (Object)null) > 0)
			{
				LocalizedNotification.Post(playerFromId, NotificationPriority.Low, 4f, "CONSTRUCTING_NTF_CANT_DESTROY_WITH_ATTACHED_DESC");
			}
			else
			{
				base.Damage(damage, damager);
			}
		}
	}

	public bool Interact(IPlayer player)
	{
		return false;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		SmokableAttachingPosition availableAttachingPosition = GetAvailableAttachingPosition();
		if (availableAttachingPosition == null)
		{
			return false;
		}
		InteractiveObject_FOOD interactiveObject_FOOD = obj as InteractiveObject_FOOD;
		if ((Object)(object)interactiveObject_FOOD != (Object)null && (Object)(object)interactiveObject_FOOD.Smoking != (Object)null && (Object)(object)interactiveObject_FOOD.Cooking != (Object)null)
		{
			if (interactiveObject_FOOD.Spoiled || interactiveObject_FOOD.Smoking.IsSmoked)
			{
				return false;
			}
			player.Holder.DropCurrent();
			Attach(interactiveObject_FOOD, availableAttachingPosition);
			return true;
		}
		return false;
	}

	private void Attach(InteractiveObject_FOOD food, SmokableAttachingPosition attachedPosition)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		Cooking cooking = food.Cooking;
		if (!((Object)(object)cooking == (Object)null))
		{
			if (_isFireOn && !food.Smoking.IsSmoked)
			{
				food.Smoking.IsBeingSmoked = true;
			}
			attachedPosition.Smokable = food.Smoking;
			food.transform.position = attachedPosition.AttachingPosition.position - food.AttachingAnchor.GetPosition();
			food.transform.rotation = attachedPosition.AttachingPosition.rotation * food.AttachingAnchor.GetRotation();
			food.Attach(new StaticAttachingAnchor(attachedPosition.AttachingPosition));
			food.DetachedEvent += Food_Detached;
			if ((Object)(object)_fire != (Object)null)
			{
				_fire.AddCookable(cooking, food.PreviousOwnerId);
			}
			ActionEventData.Populate(this, null);
			((UnityEvent<IBase, IBaseActionEventData>)OnAttach)?.Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
			CanInteract = Enumerable.Count(_attachingPositions, (SmokableAttachingPosition ap) => (Object)(object)ap.Smokable != (Object)null) != _attachingPositions.Count;
			Check_Smoking();
		}
	}

	private void Detach(IAttachable attachable)
	{
		InteractiveObject_FOOD food = attachable as InteractiveObject_FOOD;
		if ((Object)(object)food == (Object)null)
		{
			return;
		}
		food.Smoking.IsBeingSmoked = false;
		food.Cooking.IsBeingCooked = false;
		SmokableAttachingPosition smokableAttachingPosition = _attachingPositions.FirstOrDefault((SmokableAttachingPosition ap) => (Object)(object)ap.Smokable == (Object)(object)food.Smoking);
		if (smokableAttachingPosition != null)
		{
			smokableAttachingPosition.Smokable.IsBeingSmoked = false;
			Cooking cooking = smokableAttachingPosition.Smokable.Food.Cooking;
			if ((Object)(object)_fire != (Object)null && (Object)(object)cooking != (Object)null)
			{
				_fire.RemoveCookable(cooking);
			}
			smokableAttachingPosition.Smokable.Food.DetachedEvent -= Food_Detached;
			smokableAttachingPosition.Smokable = null;
			CanInteract = true;
			Check_Smoking();
		}
	}

	private void Food_Detached(IAttachable attachable)
	{
		Detach(attachable);
	}

	private void Fire_FireStarted()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!_isFireOn)
		{
			_smokingSequence = Timing.RunCoroutine(Smoke_Sequence());
			_isFireOn = true;
			Check_Smoking();
		}
	}

	private void Fire_FireStopped()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_smokingSequence);
		_isFireOn = false;
		Check_Smoking();
	}

	private void Fire_FireBurned(float hours)
	{
		for (int i = 0; i < _attachingPositions.Count; i++)
		{
			SmokableAttachingPosition smokableAttachingPosition = _attachingPositions[i];
			if (!((Object)(object)smokableAttachingPosition.Smokable == (Object)null))
			{
				smokableAttachingPosition.Smokable.SmokeManual(hours);
			}
		}
	}

	private IEnumerator<float> Smoke_Sequence()
	{
		while (true)
		{
			for (int i = 0; i < _attachingPositions.Count; i++)
			{
				SmokableAttachingPosition smokableAttachingPosition = _attachingPositions[i];
				if ((Object)(object)smokableAttachingPosition.Smokable == (Object)null || smokableAttachingPosition.Smokable.Food.IsPickedUp)
				{
					smokableAttachingPosition.Smokable = null;
				}
				else
				{
					smokableAttachingPosition.Smokable.Smoke();
				}
			}
			yield return 0f;
		}
	}

	private SmokableAttachingPosition GetAvailableAttachingPosition()
	{
		return _attachingPositions.FirstOrDefault((SmokableAttachingPosition ap) => (Object)(object)ap.Smokable == (Object)null);
	}

	private void Check_Smoking()
	{
		if (_isFireOn)
		{
			bool flag = false;
			for (int i = 0; i < _attachingPositions.Count; i++)
			{
				SmokableAttachingPosition smokableAttachingPosition = _attachingPositions[i];
				if (!((Object)(object)smokableAttachingPosition.Smokable == (Object)null))
				{
					smokableAttachingPosition.Smokable.IsBeingSmoked = true;
					if (smokableAttachingPosition.Smokable.Food.IsPickedUp)
					{
						smokableAttachingPosition.Smokable.IsBeingSmoked = false;
						smokableAttachingPosition.Smokable.Food.Cooking.IsBeingCooked = false;
					}
					else
					{
						flag = true;
					}
				}
			}
			if (flag && (Object)(object)_smokeParticle == (Object)null)
			{
				_smokeParticle.Play();
			}
			else if ((Object)(object)_smokeParticle != (Object)null && !flag)
			{
				_smokeParticle.Stop();
				flag = false;
			}
			return;
		}
		for (int j = 0; j < _attachingPositions.Count; j++)
		{
			if (Object.op_Implicit((Object)(object)_attachingPositions[j].Smokable))
			{
				_attachingPositions[j].Smokable.IsBeingSmoked = false;
				_attachingPositions[j].Smokable.Food.Cooking.IsBeingCooked = false;
			}
		}
		if ((Object)(object)_smokeParticle != (Object)null)
		{
			_smokeParticle.Stop();
		}
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		JObject jObject2 = new JObject();
		for (int i = 0; i < _attachingPositions.Count; i++)
		{
			SmokableAttachingPosition smokableAttachingPosition = _attachingPositions[i];
			jObject2.Add(((Object)(object)smokableAttachingPosition.Smokable != (Object)null) ? smokableAttachingPosition.Smokable.Food.ReferenceId.ToString() : string.Empty);
		}
		jObject.AddField("AttachingPositions", jObject2);
		jObject.AddField("Fire", _fire.ReferenceId.ToString());
		return jObject;
	}

	public override void Load(JObject data)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		base.Load(data);
		if (SaveManager.CompareSaveFileVersion(Game.Mode, new Version(0, 81, 0)) == VersionStatus.Older)
		{
			LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
			{
				Construction_PIT construction = Structure.GetConstruction<Construction_PIT>();
				if ((Object)(object)construction != (Object)null)
				{
					Connector[] myConnectors = construction.MyConnectors;
					for (int i = 0; i < myConnectors.Length; i++)
					{
						myConnectors[i].Refresh();
					}
				}
			});
		}
		MiniGuid fireReferenceId = Beam.Utilities.ExtensionMethods.ToMiniGuid(data.GetField("Fire").GetValue<string>());
		LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
		{
			try
			{
				_fire = Structure.transform.FindObject<Construction_CAMPFIRE>(fireReferenceId);
				if ((Object)(object)_fire != (Object)null)
				{
					Initialize(_fire);
				}
				else
				{
					Debug.LogError((object)"Constructing_SMOKER::Load:: Error finding fire.");
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("Constructing_SMOKER::Load:: Error loading fire:\n" + ex.Message));
			}
		});
		JObject attachingPositionsData = data.GetField("AttachingPositions");
		if (attachingPositionsData == null || attachingPositionsData.IsNull())
		{
			return;
		}
		LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
		{
			try
			{
				for (int i = 0; i < attachingPositionsData.Children.Count; i++)
				{
					JObject jObject = attachingPositionsData.Children[i];
					if (jObject == null)
					{
						Debug.LogError((object)"Constructing_SMOKER::Load:: Error loading smokable. Attaching position data is null or empty.");
					}
					else
					{
						string value = jObject.GetValue<string>();
						if (!string.IsNullOrEmpty(value))
						{
							InteractiveObject_FOOD interactiveObject_FOOD = MultiplayerMng.FindObject<InteractiveObject_FOOD>(Beam.Utilities.ExtensionMethods.ToMiniGuid(value));
							if ((Object)(object)interactiveObject_FOOD == (Object)null)
							{
								Debug.LogError((object)("Constructing_SMOKER::Load:: Error finding smokable [" + value + "]"));
							}
							else
							{
								SmokableAttachingPosition attachedPosition = _attachingPositions[i];
								Attach(interactiveObject_FOOD, attachedPosition);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("Constructing_SMOKER::Load:: Error loading smokables:\n" + ex));
			}
		});
	}
}
