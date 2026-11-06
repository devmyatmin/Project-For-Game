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
		if (!BeamTeam.IsNullOrDestroyed(pickupable))
		{
			BaseUtilities.DoPlayerDamageExperience_Hunting(pickupable);
			BaseUtilities.DoPlayerDamageMultipliers_Hunting(ref damage, pickupable);
		}
		_boar.TakeDamage();
		base.Damage(damage, damager);
	}

	public override void Destroy()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (!_destroyed)
		{
			_destroyed = true;
			EventManager.RaiseEvent(new ExperienceEvent(LastPlayerIdToDamageThis, PlayerSkills.Skills.HUNTING, PlayerSkills.ExpSource.SMALL_KILL));
			EventManager.RaiseEvent(new AddStatsEvent(LastPlayerIdToDamageThis, new Stats
			{
				AnimalsKilled = 1
			}));
			_boar.Kill();
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
		InteractiveObject_RAGDOLL interactiveObject_RAGDOLL = MultiplayerMng.Instantiate<InteractiveObject_RAGDOLL>(_ragdoll.PrefabId, MiniGuid.New());
		interactiveObject_RAGDOLL.transform.position = transform.position;
		this.ReplicatedCopyTransformsTo(interactiveObject_RAGDOLL);
		this.ReplicatedCopyProjectilesTo(interactiveObject_RAGDOLL);
	}
}
