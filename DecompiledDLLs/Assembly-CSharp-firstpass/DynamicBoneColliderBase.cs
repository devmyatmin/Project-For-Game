using UnityEngine;

public class DynamicBoneColliderBase : MonoBehaviour
{
	public enum Direction
	{
		X,
		Y,
		Z
	}

	public enum Bound
	{
		Outside,
		Inside
	}

	public Direction m_Direction = Direction.Y;

	public Vector3 m_Center = Vector3.zero;

	public Bound m_Bound;

	public virtual void Collide(ref Vector3 particlePosition, float particleRadius)
	{
	}

	public DynamicBoneColliderBase()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
	}
}
