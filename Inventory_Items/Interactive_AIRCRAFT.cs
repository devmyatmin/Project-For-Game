using System;
using System.Runtime.CompilerServices;
using Beam.Crafting;
using Beam.Events;
using Beam.Language;
using Beam.UI;
using Funlabs;
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
		Activated?.Invoke(activate);
	}

	public bool Interact(IPlayer player)
	{
		if (Game.Mode.IsClient() || !player.IsOwner)
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
		WeatherEvent weatherEvent = Singleton<AtmosphereStorm>.Instance?.CurrentWeatherEvent;
		if (weatherEvent != null && weatherEvent.HasStarted && Mathf.Approximately(weatherEvent.Humidity, 100f))
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
		if ((Game.Mode.IsMultiplayer() || Game.Mode.IsLocalCoop()) && !CheckAllPlayersInRange(player))
		{
			LocalizedNotification.Post(player, NotificationPriority.Immediate, 4f, "AIRCRAFT_NTF_CANT_LEAVE_DESC");
			return false;
		}
		return true;
	}

	private bool CheckAllPlayersInRange(IPlayer player)
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if ((player.transform.position - allPlayer.transform.position).magnitude > CHARACTER_MIN_DISTANCE)
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
		dialogueBoxViewAdapter.Accept += delegate
		{
			HideDialogue(player);
			if (ValidateRequirements(player))
			{
				Interacted?.Invoke();
			}
		};
		dialogueBoxViewAdapter.Decline += delegate
		{
			SaveGame(player);
			HideDialogue(player);
		};
		dialogueBoxViewAdapter.Cancel += delegate
		{
			HideDialogue(player);
		};
	}

	private void ShowDialogue(IPlayer player)
	{
		RewiredUtilities.Players.SetAndCacheMapsEnabled(state: false, 0);
		player.Input.SetAndCacheMapsEnabled(state: true, 16);
		player.Input.SetCursorEnabled(enabled: true);
	}

	private void HideDialogue(IPlayer player)
	{
		RewiredUtilities.Players.SetAndCacheMapsEnabled(state: true, 0);
		player.Input.SetAndCacheMapsEnabled(state: false, 16);
		player.Input.SetCursorEnabled(enabled: false);
	}

	private void SaveGame(IPlayer player)
	{
		HideDialogue(player);
		SaveManager.SaveGame(player);
	}

	private void EventManager_PlayerDisconnected(PlayerDisconnectedEvent disconnectedEvent)
	{
		RewiredUtilities.Players.SetAndCacheMapsEnabled(state: false, 10);
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	Transform IBase.get_transform()
	{
		return base.transform;
	}
}
