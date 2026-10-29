using Beam;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

public class InteractiveObject_GIANTCRAB : InteractiveObject_ANIMAL
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private GiantCrab _giantCrab;

	[SerializeField]
	private InteractiveObject_RAGDOLL _ragdollPrefab;

	[SerializeField]
	private GameObject _gib;

	private bool _destroyed;

	public override void Damage(float damage, IBase damager)
	{
		IPickupable pickupable = damager as IPickupable;
		if (!pickupable.IsNullOrDestroyed())
		{
			BaseUtilities.DoPlayerDamageExperience_Hunting(pickupable);
			BaseUtilities.DoPlayerDamageMultipliers_Hunting(ref damage, pickupable);
		}
		if (Game.Mode.IsMaster())
		{
			_giantCrab.TakeDamage();
		}
		base.Damage(damage, damager);
	}

	public override void Destroy()
	{
		if (!_destroyed)
		{
			_destroyed = true;
			EventManager.RaiseEvent(new ExperienceEvent(LastPlayerIdToDamageThis, PlayerSkills.Skills.HUNTING, PlayerSkills.ExpSource.SHARK_KILL));
			EventManager.RaiseEvent(new AddStatsEvent(LastPlayerIdToDamageThis, new Stats
			{
				AnimalsKilled = 1
			}));
			_giantCrab.Kill();
			if (Game.Mode.IsMaster())
			{
				CreateRagdoll();
			}
			CreateGibs();
			MultiplayerMng.Destroy(this);
		}
	}

	private void CreateRagdoll()
	{
		MultiplayerMng.Instantiate<InteractiveObject_RAGDOLL>(_ragdollPrefab.PrefabId, base.transform.position + Vector3.up * 0.5f, base.transform.rotation, MiniGuid.New());
	}

	private void CreateGibs()
	{
		Object.Instantiate(_gib, base.transform.position, base.transform.rotation);
	}
}
