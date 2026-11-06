using Beam.Events;
using Beam.Utilities;
using UnityEngine;

namespace Beam.Crafting;

public class ConstructionObject_HAMMER : ConstructionObject
{
	[SerializeField]
	private float HealthPointsToAdd;

	public override void Hold(bool holding)
	{
		Owner.Crafter.State = (holding ? CrafterState.BUILDING : CrafterState.CRAFTING);
		base.Hold(holding);
	}

	public override void UseOnObject(IBase obj)
	{
		base.UseOnObject(obj);
		IConstructable constructable = obj as IConstructable;
		if (!BeamTeam.IsNullOrDestroyed(constructable))
		{
			if (!constructable.HasMaxHealth() && !BeamTeam.IsNullOrDestroyed(Owner))
			{
				EventManager.RaiseEvent(new ExperienceEvent(Owner.Id, PlayerSkills.Skills.PHYSICAL, PlayerSkills.ExpSource.HAMMER_HIT));
			}
			constructable.HealthPoints += HealthPointsToAdd;
			constructable.CheckHealth();
			DurabilityPoints--;
			if (DurabilityPoints <= 0f)
			{
				Destroy();
			}
		}
	}
}
