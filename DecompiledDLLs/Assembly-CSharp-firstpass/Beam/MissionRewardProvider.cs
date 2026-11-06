using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class MissionRewardProvider : MonoBehaviour, IMissionRewardProvider
{
	[SerializeField]
	private MissionRewardBase[] _rewards;

	[SerializeField]
	private InteracterUnityEvent _reward;

	public IMissionReward[] GetRewards()
	{
		return _rewards;
	}

	public void DoRewards(IPlayer player)
	{
		MissionRewardBase[] rewards = _rewards;
		for (int i = 0; i < rewards.Length; i++)
		{
			rewards[i].DoReward();
		}
		((UnityEvent<IPlayer>)_reward)?.Invoke(player);
	}
}
