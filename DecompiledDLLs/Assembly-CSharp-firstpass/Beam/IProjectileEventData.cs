using UnityEngine;

namespace Beam;

public interface IProjectileEventData
{
	Vector3 Position { get; }

	PhysicMaterial Material { get; }
}
