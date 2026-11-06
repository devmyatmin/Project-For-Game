using UnityEngine;

[AddComponentMenu("Dynamic Bone/Dynamic Bone Plane Collider")]
public class DynamicBonePlaneCollider : DynamicBoneColliderBase
{
	private void OnValidate()
	{
	}

	public override void Collide(ref Vector3 particlePosition, float particleRadius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.up;
		switch (m_Direction)
		{
		case Direction.X:
			val = ((Component)this).transform.right;
			break;
		case Direction.Y:
			val = ((Component)this).transform.up;
			break;
		case Direction.Z:
			val = ((Component)this).transform.forward;
			break;
		}
		Vector3 val2 = ((Component)this).transform.TransformPoint(m_Center);
		Plane val3 = new Plane(val, val2);
		float distanceToPoint = val3.GetDistanceToPoint(particlePosition);
		if (m_Bound == Bound.Outside)
		{
			if (distanceToPoint < 0f)
			{
				particlePosition -= val * distanceToPoint;
			}
		}
		else if (distanceToPoint > 0f)
		{
			particlePosition -= val * distanceToPoint;
		}
	}

	private void OnDrawGizmosSelected()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled)
		{
			if (m_Bound == Bound.Outside)
			{
				Gizmos.color = Color.yellow;
			}
			else
			{
				Gizmos.color = Color.magenta;
			}
			Vector3 val = Vector3.up;
			switch (m_Direction)
			{
			case Direction.X:
				val = ((Component)this).transform.right;
				break;
			case Direction.Y:
				val = ((Component)this).transform.up;
				break;
			case Direction.Z:
				val = ((Component)this).transform.forward;
				break;
			}
			Vector3 val2 = ((Component)this).transform.TransformPoint(m_Center);
			Gizmos.DrawLine(val2, val2 + val);
		}
	}
}
