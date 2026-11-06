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
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		IPickupable pickupable = damager as IPickupable;
		if (!BeamTeam.IsNullOrDestroyed(pickupable))
		{
			BaseUtilities.DoPlayerDamageExperience_Hunting(pickupable);
			BaseUtilities.DoPlayerDamageMultipliers_Hunting(ref damage, pickupable);
		}
		if (MPExtensions.IsMaster(Game.Mode))
		{
			_snake.TakeDamage();
		}
		base.Damage(damage, damager);
	}

	public override void Destroy()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (!_destroyed)
		{
			_destroyed = true;
			EventManager.RaiseEvent(new ExperienceEvent(LastPlayerIdToDamageThis, PlayerSkills.Skills.HUNTING, PlayerSkills.ExpSource.SMALL_KILL));
			EventManager.RaiseEvent(new AddStatsEvent(LastPlayerIdToDamageThis, new Stats
			{
				AnimalsKilled = 1
			}));
			if (MPExtensions.IsMaster(Game.Mode))
			{
				CreateRagdoll();
			}
			MultiplayerMng.Destroy(this, DestroyDelay.LATE);
		}
	}

	private void CreateRagdoll()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		InteractiveObject_RAGDOLL interactiveObject_RAGDOLL = MultiplayerMng.Instantiate<InteractiveObject_RAGDOLL>(_ragdollPrefab.PrefabId, MiniGuid.New());
		interactiveObject_RAGDOLL.transform.position = transform.position;
		this.ReplicatedCopyTransformsTo(interactiveObject_RAGDOLL, broad: true);
		this.ReplicatedCopyProjectilesTo(interactiveObject_RAGDOLL);
	}

	private void CopyTransformsRecurse(Transform source, Transform destination)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected Obj, but got Unknown
		destination.position = source.position;
		destination.rotation = source.rotation;
		foreach (Transform item in destination)
		{
			Transform val = item;
			Transform val2 = ExtensionMethods.FindInChildren(transform, ((Object)val).name);
			if (Object.op_Implicit((Object)(object)val2))
			{
				CopyTransformsRecurse(val2, val);
			}
		}
	}

	public void CopyProjectiles(Transform source, Transform destination, BaseObject owner)
	{
		ProjectileObject[] componentsInChildren = ((Component)transform).GetComponentsInChildren<ProjectileObject>(true);
		foreach (ProjectileObject projectileObject in componentsInChildren)
		{
			Transform val = ExtensionMethods.FindInChildren(destination, ((Object)projectileObject.transform.parent).name);
			if ((Object)(object)val != (Object)null)
			{
				projectileObject.transform.parent = null;
				projectileObject.TransformParent = val;
				projectileObject.Parent();
				projectileObject.ProjectileMovement.AttachedBase = owner;
			}
		}
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		if (HealthPoints > 40f)
		{
			HealthPoints = 40f;
			OriginalHealthPoints = 40f;
		}
	}
}
