using System;
using System.Collections.Generic;
using Beam.Crafting;
using Beam.Events;
using Beam.Language;
using Beam.UI;
using Funlabs;
using Rewired;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.Intro;

public class Interactive_AIRCRAFT : ReplicableMonoBehaviour, IInteractable, IBase, IHasCraftingType, ISelectable
{
	[SerializeField]
	[LocalizationTermDropdown]
	[LocalizationTermPreview]
	private string _displayName;

	private MiniGuid _referenceId;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Selecting _selecting;

	public UnityEvent Interacted;

	public BoolUnityEvent Activated;

	private float CHARACTER_MIN_DISTANCE = 3f;

	public string DisplayName => _displayName;

	public override MiniGuid ReferenceId
	{
		get
		{
			if (_referenceId.IsDefault())
			{
				_referenceId = MiniGuid.INTERACTIVE_AIRCRAFT;
			}
			return _referenceId;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public Selecting Selecting => _selecting;

	public Transform TransformParent { get; set; }

	public CraftingType CraftingType { get; } = CraftingType.Empty;

	public RaycastMode RaycastMode { get; }

	public bool CanParent { get; set; }

	public bool IsDamageable { get; }

	public bool CanInteract { get; set; } = true;

	public string GetInteractionDescription(int playerId, IBase obj)
	{
		return "AIRCRAFT_INTERACTION_DESCRIPTION_GO_HOME";
	}

	private void Start()
	{
		EventManager.AddListener<PlayerDisconnectedEvent>(EventManager_PlayerDisconnected);
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<PlayerDisconnectedEvent>(EventManager_PlayerDisconnected);
	}

	public void Damage(float damage, IBase damager)
	{
	}

	public void Destroy()
	{
	}

	public void Parent()
	{
	}

	public void Activate(bool activate)
	{
		((UnityEvent<bool>)Activated)?.Invoke(activate);
	}

	public bool Interact(IPlayer player)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsClient(Game.Mode) || !player.IsOwner)
		{
			LocalizedNotification.Post(player, NotificationPriority.Immediate, 4f, "AIRCRAFT_NTF_CLIENT_INTERACT");
			return false;
		}
		DisplayDialogue(player);
		return false;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		return Interact(player);
	}

	private bool ValidateRequirements(IPlayer player)
	{
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		WeatherEvent weatherEvent = Singleton<AtmosphereStorm>.Instance?.CurrentWeatherEvent;
		if ((Object)(object)weatherEvent != (Object)null && weatherEvent.HasStarted && Mathf.Approximately(weatherEvent.Humidity, 100f))
		{
			LocalizedNotification.Post(player, NotificationPriority.Immediate, 4f, "AIRCRAFT_NTF_WAIT_WEATHER_DESC");
			return false;
		}
		if (player.Statistics.HasStatusEffect(StatusEffect.STARVATION))
		{
			LocalizedNotification.Post(player, NotificationPriority.Immediate, 4f, "AIRCRAFT_NTF_EAT_BEFORE_LEAVE_DESC");
			return false;
		}
		if (player.Statistics.HasStatusEffect(StatusEffect.DEHYDRATION))
		{
			LocalizedNotification.Post(player, NotificationPriority.Immediate, 4f, "AIRCRAFT_NTF_DRINK_BEFORE_LEAVE_DESC");
			return false;
		}
		if (player.Statistics.HasStatusEffect(StatusEffect.BROKEN_BONES))
		{
			LocalizedNotification.Post(player, NotificationPriority.Immediate, 4f, "AIRCRAFT_NTF_FIX_BROKEN_BONES_DESC");
			return false;
		}
		if (player.Statistics.HasStatusEffect(StatusEffect.BLEEDING))
		{
			LocalizedNotification.Post(player, NotificationPriority.Immediate, 4f, "AIRCRAFT_NTF_FIX_BLEEDING_DESC");
			return false;
		}
		if (player.Statistics.HasStatusEffect(StatusEffect.POISON))
		{
			LocalizedNotification.Post(player, NotificationPriority.Immediate, 4f, "AIRCRAFT_NTF_CURE_POISON_DESC");
			return false;
		}
		if ((MPExtensions.IsMultiplayer(Game.Mode) || MPExtensions.IsLocalCoop(Game.Mode)) && !CheckAllPlayersInRange(player))
		{
			LocalizedNotification.Post(player, NotificationPriority.Immediate, 4f, "AIRCRAFT_NTF_CANT_LEAVE_DESC");
			return false;
		}
		return true;
	}

	private bool CheckAllPlayersInRange(IPlayer player)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			Vector3 val = player.transform.position - allPlayer.transform.position;
			if (val.magnitude > CHARACTER_MIN_DISTANCE)
			{
				return false;
			}
		}
		return true;
	}

	private void DisplayDialogue(IPlayer player)
	{
		ShowDialogue(player);
		DialogueBoxViewAdapter dialogueBoxViewAdapter = DialogueBox.Show(player.Input, "AIRCRAFT_DB_GO_HOME_TITLE", "AIRCRAFT_DB_GO_HOME_DESC", "AIRCRAFT_DB_BUTTON_YES", "AIRCRAFT_DB_BUTTON_SAVE", "AIRCRAFT_DB_BUTTON_NO_SCARED");
		dialogueBoxViewAdapter.Accept += () =>
		{
			HideDialogue(player);
			if (ValidateRequirements(player))
			{
				UnityEvent interacted = Interacted;
				if (interacted != null)
				{
					interacted.Invoke();
				}
			}
		};
		dialogueBoxViewAdapter.Decline += () =>
		{
			SaveGame(player);
			HideDialogue(player);
		};
		dialogueBoxViewAdapter.Cancel += () =>
		{
			HideDialogue(player);
		};
	}

	private void ShowDialogue(IPlayer player)
	{
		RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, false, 0);
		RewiredUtilities.SetAndCacheMapsEnabled(player.Input, state: true, 16);
		RewiredUtilities.SetCursorEnabled(player.Input, enabled: true);
	}

	private void HideDialogue(IPlayer player)
	{
		RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, true, 0);
		RewiredUtilities.SetAndCacheMapsEnabled(player.Input, state: false, 16);
		RewiredUtilities.SetCursorEnabled(player.Input, enabled: false);
	}

	private void SaveGame(IPlayer player)
	{
		HideDialogue(player);
		SaveManager.SaveGame(player);
	}

	private void EventManager_PlayerDisconnected(PlayerDisconnectedEvent disconnectedEvent)
	{
		RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, false, 10);
	}
}
