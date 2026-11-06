using UnityEngine;

namespace Beam;

public interface IFishable
{
	FishingData FishingData { get; }

	bool CheckPositionInBounds(Vector3 position);

	bool CheckPositionInInterestRadius(Vector3 position);

	void Hook(bool hooked);

	void Catch();
}
