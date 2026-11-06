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
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		IPickupable pickupable = damager as IPickupable;
		if (!BeamTeam.IsNullOrDestroyed(pickupable))
		{
			BaseUtilities.DoPlayerDamageExperience_Hunting(pickupable);
			BaseUtilities.DoPlayerDamageMultipliers_Hunting(ref damage, pickupable);
		}
		if (MPExtensions.IsMaster(Game.Mode))
		{
			_giantCrab.TakeDamage();
		}
		base.Damage(damage, damager);
	}

	public override void Destroy()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (!_destroyed)
		{
			_destroyed = true;
			EventManager.RaiseEvent(new ExperienceEvent(LastPlayerIdToDamageThis, PlayerSkills.Skills.HUNTING, PlayerSkills.ExpSource.SHARK_KILL));
			EventManager.RaiseEvent(new AddStatsEvent(LastPlayerIdToDamageThis, new Stats
			{
				AnimalsKilled = 1
			}));
			_giantCrab.Kill();
			if (MPExtensions.IsMaster(Game.Mode))
			{
				CreateRagdoll();
			}
			CreateGibs();
			MultiplayerMng.Destroy(this);
		}
	}

	private void CreateRagdoll()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		MultiplayerMng.Instantiate<InteractiveObject_RAGDOLL>(_ragdollPrefab.PrefabId, transform.position + Vector3.up * 0.5f, transform.rotation, MiniGuid.New());
	}

	private void CreateGibs()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Object.Instantiate<GameObject>(_gib, transform.position, transform.rotation);
	}
}
