using UnityEngine;

namespace Beam.UI;

public interface IImageViewAdapter : IViewAdapter
{
	Color Color { get; set; }

	Sprite Sprite { get; set; }
}
