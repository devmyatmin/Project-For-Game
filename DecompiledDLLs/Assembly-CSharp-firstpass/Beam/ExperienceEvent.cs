using Beam.Events;

namespace Beam;

public class ExperienceEvent : IEvent
{
	private readonly int _Id;

	private readonly PlayerSkills.ExpSource _expSource;

	private readonly float _customExp;

	private readonly PlayerSkills.Skills _skill;

	public int Id => _Id;

	public PlayerSkills.ExpSource ExpSource => _expSource;

	public float CustomExp => _customExp;

	public PlayerSkills.Skills Skill => _skill;

	public ExperienceEvent(int Id, PlayerSkills.Skills skill, PlayerSkills.ExpSource expSource)
	{
		_Id = Id;
		_skill = skill;
		_expSource = expSource;
	}

	public ExperienceEvent(int Id, PlayerSkills.Skills skill, float experience)
	{
		_Id = Id;
		_skill = skill;
		_customExp = experience;
	}
}
