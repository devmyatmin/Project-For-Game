using UnityEngine;

namespace Beam;

public class ScatterCastProjectileCollisionDetection : ProjectileCollisionDetectionBase
{
	[SerializeField]
	private float _radius;

	[SerializeField]
	private float _numberOfRays;

	public override bool CheckCollisions(Ray ray, out RaycastHit hitInfo, float distance, int layermask)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (Physics.Raycast(ray, ref hitInfo, distance, layermask))
		{
			return true;
		}
		for (int i = 0; (float)i < _numberOfRays; i++)
		{
			Vector3 val = Random.insideUnitSphere * _radius;
			val.z = 0f;
			if (Physics.Raycast(new Ray(ray.origin + val, ray.direction), ref hitInfo, distance, layermask))
			{
				return true;
			}
		}
		hitInfo = default;
		return false;
	}
}
