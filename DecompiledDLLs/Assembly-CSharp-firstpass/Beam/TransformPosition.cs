using System;
using UnityEngine;

namespace Beam;

[Serializable]
public struct TransformPosition : IHavePosition
{
	[SerializeField]
	private Transform _transform;

	public Vector3 Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _transform.position;
		}
	}

	public TransformPosition(Transform transform)
	{
		_transform = transform;
	}
}
