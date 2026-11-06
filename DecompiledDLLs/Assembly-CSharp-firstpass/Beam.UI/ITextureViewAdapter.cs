using UnityEngine;

namespace Beam.UI;

public interface ITextureViewAdapter : IViewAdapter
{
	Texture Texture { get; set; }

	Color Color { get; set; }
}
