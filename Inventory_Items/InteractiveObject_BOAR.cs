using Beam;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

public class InteractiveObject_BOAR : InteractiveObject_ANIMAL
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Boar _boar;

	[SerializeField]
	private InteractiveObject _ragdoll;

	private bool _destroyed;

	public override void Damage(float damage, IBase damager)
	{
		IPickupable pickupable = damager as IPickupable;
		if (!pickupable.IsNullOrDestroyed())
		{
			BaseUtilities.DoPlayerDamageExperience_Hunting(pickupable);
			BaseUtilities.DoPlayerDamageMultipliers_Hunting(ref damage, pickupable);
		}
		_boar.TakeDamage();
		base.Damage(damage, damager);
	}

	public override void Destroy()
	{
		if (!_destroyed)
		{
			_destroyed = true;
			EventManager.RaiseEvent(new ExperienceEvent(LastPlayerIdToDamageThis, PlayerSkills.Skills.HUNTING, PlayerSkills.ExpSource.SMALL_KILL));
			EventManager.RaiseEvent(new AddStatsEvent(LastPlayerIdToDamageThis, new Stats
			{
				AnimalsKilled = 1
			}));
			_boar.Kill();
			if (Game.Mode.IsMaster())
			{
				CreateRagdoll();
			}
			MultiplayerMng.Destroy(this, DestroyDelay.LATE);
		}
	}

	public void CreateRagdoll()
	{
		InteractiveObject_RAGDOLL interactiveObject_RAGDOLL = MultiplayerMng.Instantiate<InteractiveObject_RAGDOLL>(_ragdoll.PrefabId, MiniGuid.New());
		interactiveObject_RAGDOLL.transform.position = base.transform.position;
		this.ReplicatedCopyTransformsTo(interactiveObject_RAGDOLL);
		this.ReplicatedCopyProjectilesTo(interactiveObject_RAGDOLL);
	}
}
