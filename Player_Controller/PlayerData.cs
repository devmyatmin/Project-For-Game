using UnityEngine;

namespace Beam;

public class PlayerData : IHavePosition
{
	public Vector3 Position { get; set; }

	public Quaternion Rotation { get; set; }
}
