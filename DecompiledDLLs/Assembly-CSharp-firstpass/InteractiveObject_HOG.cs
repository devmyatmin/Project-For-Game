using Beam;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

public class InteractiveObject_HOG : InteractiveObject_ANIMAL
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Hog _hog;

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
			_hog.TakeDamage();
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
			_hog.Kill();
			if (MPExtensions.IsMaster(Game.Mode))
			{
				CreateRagdoll();
			}
			MultiplayerMng.Destroy(this, DestroyDelay.LATE);
		}
	}

	public void CreateRagdoll()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		InteractiveObject_RAGDOLL interactiveObject_RAGDOLL = MultiplayerMng.Instantiate<InteractiveObject_RAGDOLL>(_ragdollPrefab.PrefabId, MiniGuid.New());
		interactiveObject_RAGDOLL.transform.position = transform.position;
		this.ReplicatedCopyTransformsTo(interactiveObject_RAGDOLL);
		this.ReplicatedCopyProjectilesTo(interactiveObject_RAGDOLL);
	}
}
