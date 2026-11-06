using Beam;
using Beam.Events;
using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
	private IPlayer _player;

	public void SetPlayer(IPlayer player)
	{
		_player = player;
	}

	private void Start()
	{
		EventManager.AddListener<ExperienceEvent>(EventManager_AddExperience);
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<ExperienceEvent>(EventManager_AddExperience);
	}

	private void EventManager_AddExperience(ExperienceEvent Ee)
	{
		if (_player.Id == Ee.Id)
		{
			if (Ee.Skill == PlayerSkills.Skills.HUNTING)
			{
				_player?.PlayerSkills.ReplicatedAddHunting(Ee.ExpSource);
			}
			if (Ee.Skill == PlayerSkills.Skills.COOKING)
			{
				_player?.PlayerSkills.ReplicatedAddCooking(Ee.ExpSource);
			}
			if (Ee.Skill == PlayerSkills.Skills.PHYSICAL)
			{
				_player?.PlayerSkills.ReplicatedAddPhysical(Ee.ExpSource);
			}
			if (Ee.Skill == PlayerSkills.Skills.HARVESTING)
			{
				_player?.PlayerSkills.ReplicatedAddHarvesting(Ee.CustomExp);
			}
			if (Ee.Skill == PlayerSkills.Skills.CRAFTSMANSHIP)
			{
				_player?.PlayerSkills.ReplicatedAddCraftsmanship(Ee.CustomExp);
			}
		}
	}
}
