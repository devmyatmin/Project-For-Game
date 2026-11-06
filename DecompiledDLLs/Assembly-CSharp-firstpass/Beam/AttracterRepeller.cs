using UnityEngine;

namespace Beam;

public struct AttracterRepeller : IAttracterRepeller
{
	private readonly IHavePosition _position;

	private readonly float _radius;

	private readonly int _id;

	public Vector3 Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _position.Position;
		}
	}

	public float Radius => _radius;

	public int Id => _id;

	public AttracterRepeller(IHavePosition position, float radius, int id = -1)
	{
		_position = position;
		_radius = radius;
		_id = id;
	}
}
