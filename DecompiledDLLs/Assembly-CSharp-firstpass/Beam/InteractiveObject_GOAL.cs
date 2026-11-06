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

	public IPiscus_CreatureState MultiplayerState => ((MultiplayerBehaviour)this).GetMultiplayerState<IPiscus_CreatureState>();

	public override bool IsValidPrefab => true;

	public override bool IsMultiplayerEntity => true;

	public override void Attached()
	{
		base.Attached();
		if (Object.op_Implicit((Object)(object)_piscusCreature) && Object.op_Implicit((Object)(object)_piscusCreature.Animation) && Object.op_Implicit((Object)(object)_piscusCreature.Animation.AnimatorController))
		{
			((IState)MultiplayerState).SetAnimator(_piscusCreature.Animation.AnimatorController);
		}
	}

	public void Reset()
	{
		HealthPoints = OriginalHealthPoints;
		UnityEvent resetted = Resetted;
		if (resetted != null)
		{
			resetted.Invoke();
		}
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
		UnityEvent destroyed = Destroyed;
		if (destroyed != null)
		{
			destroyed.Invoke();
		}
		base.Destroy();
		Beam.Utilities.ExtensionMethods.DestroyProjectilesInChildren(transform);
	}

	protected override void SetTransformSyncInternal(bool enabled)
	{
		base.SetTransformSyncInternal(enabled: true);
	}
}
