using UnityEngine;

namespace Beam.UI;

public interface ILabelViewAdapter : IViewAdapter
{
	string Text { get; set; }

	Color Color { get; set; }
}
