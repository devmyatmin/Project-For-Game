using UnityEngine;

namespace Beam;

public interface IAttracterRepeller
{
	Vector3 Position { get; }

	float Radius { get; }

	int Id { get; }
}
