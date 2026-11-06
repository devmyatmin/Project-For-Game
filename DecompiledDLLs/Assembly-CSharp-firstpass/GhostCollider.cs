using System.Collections.Generic;
using UnityEngine;

public class GhostCollider : MonoBehaviour
{
	public enum ColliderType
	{
		NONE,
		FOUNDATION,
		FLOOR,
		WEDGE_FOUNDATION,
		WEDGE_FLOOR,
		WALL,
		WALL_HALF,
		WALL_SLOPED,
		STEPS,
		DOOR,
		ROOF_MIDDLE,
		RAFT_BASE,
		RAFT_FLOOR,
		VEHICLE_SAIL,
		VEHICLE_MOTOR,
		FARMING_PATCH,
		HOOK,
		SHELF,
		ROOF_CORNER,
		ROOF_CAP,
		ROOF_WEDGE,
		TANNING_RACK,
		LOOM,
		FURNACE,
		BRICK_STATION,
		CONTAINER_SHELF,
		FUEL_STILL,
		LIGHT_HOOK
	}

	[SerializeField]
	private ColliderType _type;

	[SerializeField]
	private Collider[] _colliders;

	[SerializeField]
	private GhostCollisionModifierBase[] _collisionModifiers;

	private HashSet<Collider> _colliding = new HashSet<Collider>();

	private HashSet<Collider> _ignore = new HashSet<Collider>();

	public ColliderType Type => _type;

	public Collider[] Colliders => _colliders;

	public bool Colliding => _colliding.Count > 0;

	private void OnDisable()
	{
		_ignore.Clear();
		_colliding.Clear();
	}

	public void IgnoreCollider(Collider collider, bool ignore)
	{
		if (ignore)
		{
			if (_ignore.Add(collider))
			{
				_colliding.Remove(collider);
			}
		}
		else
		{
			_ignore.Remove(collider);
		}
	}

	private void FixedUpdate()
	{
		_colliding.Clear();
	}

	private void OnTriggerStay(Collider other)
	{
		if (_ignore.Contains(other) || _colliding.Contains(other) || (((((Component)this).gameObject.layer != Layers.CONSTRUCTIONS && ((Component)this).gameObject.layer != Layers.CONSTRUCTIONS_SMALL && ((Component)this).gameObject.layer != Layers.CONSTRUCTIONS_RAFTS) || !((Component)other).CompareTag("VerticalBuildingCollision")) && ((((Component)other).CompareTag("NoBuildingCollision") && ((Component)other).gameObject.layer != Layers.CONSTRUCTIONS_SMALL) || (((Component)this).CompareTag("NoBuildingCollision") && ((Component)other).gameObject.layer == Layers.CONSTRUCTIONS))))
		{
			return;
		}
		for (int i = 0; i < _collisionModifiers.Length; i++)
		{
			if (!_collisionModifiers[i].ValidateCollision(other) && _type == ColliderType.RAFT_FLOOR)
			{
				return;
			}
		}
		_colliding.Add(other);
	}
}
