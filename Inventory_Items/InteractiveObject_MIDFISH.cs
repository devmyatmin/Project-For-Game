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
		InteractiveObject interactiveObject = Object.Instantiate(_ragdollPrefab);
		interactiveObject.transform.position = base.transform.position;
		this.CopyTransformsTo(interactiveObject);
		this.CopyProjectilesTo(interactiveObject);
	}

	public override void Damage(float damage, IBase damager)
	{
		if ((bool)_animation && (bool)_animation["TakeDamage"])
		{
			_animation.CrossFade("TakeDamage", 0.1f);
		}
		IPickupable damager2 = damager as IPickupable;
		BaseUtilities.DoPlayerDamageExperience_Hunting(damager2);
		BaseUtilities.DoPlayerDamageMultipliers_Hunting(ref damage, damager2);
		base.Damage(damage, damager);
	}
}
