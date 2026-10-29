using System.Runtime.CompilerServices;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Utilities;
using Funlabs;
using Photon.Bolt;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class InteractiveObject_GOAL : InteractiveObject, IDisintegratable, IBase, IHasCraftingType, IMultiplayerState<IPiscus_CreatureState>
{
	[HideInInspector]
	public UnityEvent Destroyed;

	[HideInInspector]
	public UnityEvent Resetted;

	[SerializeField]
	private Piscus_Creature _piscusCreature;

	public bool Disintegrating { get; set; }

	public IPiscus_CreatureState MultiplayerState => GetMultiplayerState<IPiscus_CreatureState>();

	public override bool IsValidPrefab => true;

	public override bool IsMultiplayerEntity => true;

	public override void Attached()
	{
		base.Attached();
		if ((bool)_piscusCreature && (bool)_piscusCreature.Animation && (bool)_piscusCreature.Animation.AnimatorController)
		{
			MultiplayerState.SetAnimator(_piscusCreature.Animation.AnimatorController);
		}
	}

	public void Reset()
	{
		base.HealthPoints = base.OriginalHealthPoints;
		Resetted?.Invoke();
	}

	public override void Damage(float damage, IBase damager)
	{
		IPickupable damager2 = damager as IPickupable;
		BaseUtilities.DoPlayerDamageExperience_Hunting(damager2);
		BaseUtilities.DoPlayerDamageMultipliers_Hunting(ref damage, damager2);
		OnDamaged(damage, damager);
	}

	public override void Destroy()
	{
		EventManager.RaiseEvent(new ExperienceEvent(LastPlayerIdToDamageThis, PlayerSkills.Skills.HUNTING, PlayerSkills.ExpSource.MONSTER_KILL));
		EventManager.RaiseEvent(new AddStatsEvent(LastPlayerIdToDamageThis, new Stats
		{
			AnimalsKilled = 1
		}));
		Destroyed?.Invoke();
		base.Destroy();
		base.transform.DestroyProjectilesInChildren();
	}

	protected override void SetTransformSyncInternal(bool enabled)
	{
		base.SetTransformSyncInternal(enabled: true);
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}
}
