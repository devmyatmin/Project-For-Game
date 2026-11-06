using UnityEngine;

namespace Beam.Crafting;

public struct ProjectileHit(Transform transform, IBase obj = null)
{
	public Transform Transform = transform;

	public IBase Object = obj;
}
