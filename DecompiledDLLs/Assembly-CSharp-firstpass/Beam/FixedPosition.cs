using System;
using UnityEngine;

namespace Beam;

[Serializable]
public struct FixedPosition : IHavePosition
{
	[SerializeField]
	private Vector3 _position;

	public Vector3 Position
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _position;
		}
	}

	public FixedPosition(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_position = position;
	}
}
