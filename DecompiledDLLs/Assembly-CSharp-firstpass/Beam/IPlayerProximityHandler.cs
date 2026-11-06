using UnityEngine;

namespace Beam;

public interface IPlayerProximityHandler
{
	Vector3 Position { get; }

	float ProximityDistance { get; }

	bool InProximity { get; set; }

	void OnEnteredPlayerProximity(PlayerData playerData);

	void OnLeftPlayerProximity(PlayerData playerData);
}
