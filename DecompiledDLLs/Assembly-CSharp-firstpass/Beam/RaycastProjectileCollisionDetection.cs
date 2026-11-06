using UnityEngine;

namespace Beam;

public class RaycastProjectileCollisionDetection : ProjectileCollisionDetectionBase
{
	public override bool CheckCollisions(Ray ray, out RaycastHit hitInfo, float distance, int layermask)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Physics.Raycast(ray, ref hitInfo, distance, layermask);
	}
}
