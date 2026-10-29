using Beam;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using Photon.Bolt;
using UnityEngine;

public class InteractiveObject_PISCUS : InteractiveObject, IMultiplayerState<IPiscus_CreatureState>
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Piscus_Creature _piscusCreature;

	[SerializeField]
	private InteractiveObject_RAGDOLL _piscusCarcass;

	private bool _destroyed;

	public IPiscus_CreatureState MultiplayerState => GetMultiplayerState<IPiscus_CreatureState>();

	public override void Attached()
	{
		base.Attached();
		MultiplayerState.SetTransforms(MultiplayerState.Transform, base.transform);
		if ((bool)_piscusCreature && (bool)_piscusCreature.Animation && (bool)_piscusCreature.Animation.AnimatorController)
		{
			MultiplayerState.SetAnimator(_piscusCreature.Animation.AnimatorController);
		}
	}

	public override void Damage(float damage, IBase damager)
	{
		IPickupable pickupable = damager as IPickupable;
		if (!pickupable.IsNullOrDestroyed())
		{
			BaseUtilities.DoPlayerDamageExperience_Hunting(pickupable);
			BaseUtilities.DoPlayerDamageMultipliers_Hunting(ref damage, pickupable);
		}
		base.Damage(damage, damager);
		_piscusCreature.ApplyDamage(damage);
	}

	public override void OnCustomMapLoaded()
	{
		base.OnCustomMapLoaded();
		_piscusCreature.Setup_Position();
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		_piscusCreature.Setup_Position();
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
			if (!Game.Mode.IsClient())
			{
				CreateRagdoll();
				MultiplayerMng.Destroy(this, DestroyDelay.LATE);
			}
		}
	}

	private void CreateRagdoll()
	{
		InteractiveObject_RAGDOLL interactiveObject_RAGDOLL = MultiplayerMng.Instantiate<InteractiveObject_RAGDOLL>(_piscusCarcass.GetComponent<SaveablePrefab>().PrefabId, MiniGuid.New());
		interactiveObject_RAGDOLL.transform.position = base.transform.position;
		this.ReplicatedCopyTransformsTo(interactiveObject_RAGDOLL);
		this.ReplicatedCopyProjectilesTo(interactiveObject_RAGDOLL);
	}
}
