using UnityEngine;

namespace Beam;

public interface IMissionReward
{
	string Name { get; }

	int Quantity { get; }

	Sprite GetIcon();

	void DoReward();
}
