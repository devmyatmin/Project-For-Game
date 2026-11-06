using UnityEngine;

namespace Beam;

public interface IPhysical : IBase, IHasCraftingType
{
	Rigidbody rigidbody { get; }

	bool IsKinematic { get; }

	CollisionDetectionMode CollisionDetectionMode { get; set; }

	void EnablePhysics();

	void DisablePhysics();
}
