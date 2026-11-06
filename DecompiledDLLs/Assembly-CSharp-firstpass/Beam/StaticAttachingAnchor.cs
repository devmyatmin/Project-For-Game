using System;
using UnityEngine;

namespace Beam;

[Serializable]
public struct StaticAttachingAnchor : IAttachingAnchor
{
	[SerializeField]
	private Vector3 _position;

	[SerializeField]
	[EulerAngles]
	private Quaternion _rotation;

	public StaticAttachingAnchor(Vector3 position, Quaternion rotation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		_position = position;
		_rotation = rotation;
	}

	public StaticAttachingAnchor(Transform transform)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		_position = transform.position;
		_rotation = transform.rotation;
	}

	public Vector3 GetPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _position;
	}

	public Quaternion GetRotation()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _rotation;
	}

	public bool IsKinematic()
	{
		return true;
	}
}
