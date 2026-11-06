namespace Beam;

public interface IMissionRewardProvider
{
	IMissionReward[] GetRewards();

	void DoRewards(IPlayer player);
}
