using UnityEngine;

namespace Beam;

public class SphereCastProjectileCollisionDetection : ProjectileCollisionDetectionBase
{
	[SerializeField]
	private float _radius;

	public override bool CheckCollisions(Ray ray, out RaycastHit hitInfo, float distance, int layermask)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Physics.SphereCast(ray, _radius, ref hitInfo, distance - _radius, layermask);
	}
}
