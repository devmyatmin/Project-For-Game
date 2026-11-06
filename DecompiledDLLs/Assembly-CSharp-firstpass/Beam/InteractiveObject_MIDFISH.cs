using Beam.Events;
using UnityEngine;

namespace Beam;

public class InteractiveObject_MIDFISH : InteractiveObject
{
	[SerializeField]
	private InteractiveObject _ragdollPrefab;

	[SerializeField]
	private Animation _animation;

	public override void Destroy()
	{
		EventManager.RaiseEvent(new ExperienceEvent(LastPlayerIdToDamageThis, PlayerSkills.Skills.HUNTING, PlayerSkills.ExpSource.SMALL_KILL));
		CreateRagdoll();
		base.Destroy();
	}

	protected virtual void CreateRagdoll()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		InteractiveObject interactiveObject = Object.Instantiate<InteractiveObject>(_ragdollPrefab);
		interactiveObject.transform.position = transform.position;
		this.CopyTransformsTo(interactiveObject);
		this.CopyProjectilesTo(interactiveObject);
	}

	public override void Damage(float damage, IBase damager)
	{
		if (Object.op_Implicit((Object)(object)_animation) && TrackedReference.op_Implicit((TrackedReference)(object)_animation["TakeDamage"]))
		{
			_animation.CrossFade("TakeDamage", 0.1f);
		}
		IPickupable damager2 = damager as IPickupable;
		BaseUtilities.DoPlayerDamageExperience_Hunting(damager2);
		BaseUtilities.DoPlayerDamageMultipliers_Hunting(ref damage, damager2);
		base.Damage(damage, damager);
	}
}
