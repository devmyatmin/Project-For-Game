using System;
using UnityEngine;

namespace Beam;

[Serializable]
public struct TransformAttracterRepeller : IAttracterRepeller
{
	[SerializeField]
	private TransformPosition _transformPosition;

	[SerializeField]
	private float _radius;

	private int _id;

	public Vector3 Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _transformPosition.Position;
		}
	}

	public float Radius => _radius;

	public int Id => _id;

	public TransformAttracterRepeller(TransformPosition transformPosition, float radius, int id = -1)
	{
		_transformPosition = transformPosition;
		_radius = radius;
		_id = id;
	}
}
