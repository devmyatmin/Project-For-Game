using Beam;
using Beam.Crafting;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

public class InteractiveObject_SNAKE : InteractiveObject_ANIMAL
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Snake _snake;

	[SerializeField]
	private InteractiveObject_RAGDOLL _ragdollPrefab;

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
			_snake.TakeDamage();
		}
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
			if (Game.Mode.IsMaster())
			{
				CreateRagdoll();
			}
			MultiplayerMng.Destroy(this, DestroyDelay.LATE);
		}
	}

	private void CreateRagdoll()
	{
		InteractiveObject_RAGDOLL interactiveObject_RAGDOLL = MultiplayerMng.Instantiate<InteractiveObject_RAGDOLL>(_ragdollPrefab.PrefabId, MiniGuid.New());
		interactiveObject_RAGDOLL.transform.position = base.transform.position;
		this.ReplicatedCopyTransformsTo(interactiveObject_RAGDOLL, broad: true);
		this.ReplicatedCopyProjectilesTo(interactiveObject_RAGDOLL);
	}

	private void CopyTransformsRecurse(Transform source, Transform destination)
	{
		destination.position = source.position;
		destination.rotation = source.rotation;
		foreach (Transform item in destination)
		{
			Transform transform2 = base.transform.FindInChildren(item.name);
			if ((bool)transform2)
			{
				CopyTransformsRecurse(transform2, item);
			}
		}
	}

	public void CopyProjectiles(Transform source, Transform destination, BaseObject owner)
	{
		ProjectileObject[] componentsInChildren = base.transform.GetComponentsInChildren<ProjectileObject>(includeInactive: true);
		foreach (ProjectileObject projectileObject in componentsInChildren)
		{
			Transform transform = destination.FindInChildren(projectileObject.transform.parent.name);
			if (transform != null)
			{
				projectileObject.transform.parent = null;
				projectileObject.TransformParent = transform;
				projectileObject.Parent();
				projectileObject.ProjectileMovement.AttachedBase = owner;
			}
		}
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		if (base.HealthPoints > 40f)
		{
			base.HealthPoints = 40f;
			base.OriginalHealthPoints = 40f;
		}
	}
}
