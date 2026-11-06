using UnityEngine;

namespace Beam;

public abstract class MissionRewardBase : MonoBehaviour, IMissionReward
{
	public abstract string Name { get; }

	public abstract int Quantity { get; }

	public abstract Sprite GetIcon();

	public abstract void DoReward();
}
