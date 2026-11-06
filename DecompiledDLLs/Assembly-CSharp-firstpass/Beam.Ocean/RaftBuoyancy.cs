using UnityEngine;

namespace Beam.Ocean;

public class RaftBuoyancy : MonoBehaviour
{
	[SerializeField]
	private BoxCollider _collider;

	[SerializeField]
	private float _density;

	[SerializeField]
	private Vector2 _voxelPerAxisIncrease;

	public BoxCollider Collider => _collider;

	public float Density => _density;

	public Vector2 VoxelPerAxisIncrease
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _voxelPerAxisIncrease;
		}
	}
}
