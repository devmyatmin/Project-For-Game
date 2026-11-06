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

	public IPiscus_CreatureState MultiplayerState => ((MultiplayerBehaviour)this).GetMultiplayerState<IPiscus_CreatureState>();

	public override void Attached()
	{
		base.Attached();
		((IState)MultiplayerState).SetTransforms(((ISaveablePrefabState)MultiplayerState).Transform, transform);
		if (Object.op_Implicit((Object)(object)_piscusCreature) && Object.op_Implicit((Object)(object)_piscusCreature.Animation) && Object.op_Implicit((Object)(object)_piscusCreature.Animation.AnimatorController))
		{
			((IState)MultiplayerState).SetAnimator(_piscusCreature.Animation.AnimatorController);
		}
	}

	public override void Damage(float damage, IBase damager)
	{
		IPickupable pickupable = damager as IPickupable;
		if (!BeamTeam.IsNullOrDestroyed(pickupable))
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
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (!_destroyed)
		{
			_destroyed = true;
			EventManager.RaiseEvent(new ExperienceEvent(LastPlayerIdToDamageThis, PlayerSkills.Skills.HUNTING, PlayerSkills.ExpSource.SHARK_KILL));
			EventManager.RaiseEvent(new AddStatsEvent(LastPlayerIdToDamageThis, new Stats
			{
				AnimalsKilled = 1
			}));
			if (!MPExtensions.IsClient(Game.Mode))
			{
				CreateRagdoll();
				MultiplayerMng.Destroy(this, DestroyDelay.LATE);
			}
		}
	}

	private void CreateRagdoll()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		InteractiveObject_RAGDOLL interactiveObject_RAGDOLL = MultiplayerMng.Instantiate<InteractiveObject_RAGDOLL>(((Component)_piscusCarcass).GetComponent<SaveablePrefab>().PrefabId, MiniGuid.New());
		interactiveObject_RAGDOLL.transform.position = transform.position;
		this.ReplicatedCopyTransformsTo(interactiveObject_RAGDOLL);
		this.ReplicatedCopyProjectilesTo(interactiveObject_RAGDOLL);
	}
}
