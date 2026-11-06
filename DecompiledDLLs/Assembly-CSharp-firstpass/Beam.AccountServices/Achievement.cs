namespace Beam.AccountServices;

public class Achievement
{
	public EAchievement Id;

	public bool Achieved;

	public Achievement(EAchievement achievementID)
	{
		Id = achievementID;
		Achieved = false;
	}
}
