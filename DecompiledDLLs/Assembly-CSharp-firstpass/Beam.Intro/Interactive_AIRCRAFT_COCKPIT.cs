using System;
using Beam.Crafting;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.Intro;

public class Interactive_AIRCRAFT_COCKPIT : ReplicableMonoBehaviour, IInteractable, IBase, IHasCraftingType, ISelectable
{
	private MiniGuid _referenceId;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Selecting _selecting;

	private int _currentStage = 1;

	[Header("EVENTS")]
	public InteractionSequenceEvent Interacted;

	public override MiniGuid ReferenceId
	{
		get
		{
			if (_referenceId.IsDefault())
			{
				_referenceId = MiniGuid.INTERACTIVE_AIRCRAFT_COCKPIT;
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

	public string DisplayName { get; protected set; } = "";

	public CraftingType CraftingType { get; } = CraftingType.Empty;

	public RaycastMode RaycastMode { get; }

	public bool CanParent { get; set; }

	public bool IsDamageable { get; }

	public bool CanInteract { get; set; } = true;

	public string GetInteractionDescription(int playerId, IBase obj)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsClient(Game.Mode))
		{
			return string.Empty;
		}
		string result = "";
		if (_currentStage == 0)
		{
			result = "AIRCRAFT_COCKPIT_INTERACTION_DESCRIPTION_CHECK_SYSTEMS";
		}
		if (_currentStage == 1)
		{
			result = "AIRCRAFT_COCKPIT_INTERACTION_DESCRIPTION_START_ENGINE";
		}
		if (_currentStage == 2)
		{
			result = "AIRCRAFT_COCKPIT_INTERACTION_DESCRIPTION_CLOSE_COCKPIT";
		}
		if (_currentStage == 3)
		{
			result = "AIRCRAFT_COCKPIT_INTERACTION_DESCRIPTION_ENGAGE_RAIL";
		}
		if (_currentStage == 4)
		{
			result = "AIRCRAFT_COCKPIT_INTERACTION_DESCRIPTION_THROTTLE_ENGINE";
		}
		if (_currentStage == 5)
		{
			result = "AIRCRAFT_COCKPIT_INTERACTION_DESCRIPTION_LAUNCH";
		}
		return result;
	}

	private void Start()
	{
		DisplayName = string.Empty;
		CanInteract = true;
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

	public void Activate(bool toggle)
	{
		CanInteract = toggle;
	}

	public bool Interact(IPlayer player)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if ((MPExtensions.IsClient(Game.Mode) && player.IsOwner) || (MPExtensions.IsServer(Game.Mode) && !player.IsOwner))
		{
			return false;
		}
		((UnityEvent<InteractionSequence>)Interacted)?.Invoke((InteractionSequence)_currentStage);
		_currentStage++;
		if (MPExtensions.IsMaster(Game.Mode))
		{
			((Component)this).gameObject.SetActive(false);
		}
		return false;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		return Interact(player);
	}
}
