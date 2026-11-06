using UnityEngine;

namespace Beam;

public abstract class ProjectileCollisionDetectionBase : ScriptableObject
{
	public abstract bool CheckCollisions(Ray ray, out RaycastHit hitInfo, float distance, int layermask);
}
