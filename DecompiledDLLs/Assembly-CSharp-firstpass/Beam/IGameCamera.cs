using UnityEngine;

namespace Beam;

public interface IGameCamera
{
	int Id { get; set; }

	GameObject gameObject { get; }

	Camera Camera { get; }
}
