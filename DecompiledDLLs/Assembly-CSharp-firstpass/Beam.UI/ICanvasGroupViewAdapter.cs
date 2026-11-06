using UnityEngine;

namespace Beam.UI;

public interface ICanvasGroupViewAdapter
{
	Transform transform { get; }

	bool Interactable { get; set; }

	float Alpha { get; set; }
}
