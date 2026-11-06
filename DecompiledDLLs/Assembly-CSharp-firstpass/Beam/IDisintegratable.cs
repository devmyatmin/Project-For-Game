using UnityEngine;

namespace Beam;

public interface IDisintegratable : IBase, IHasCraftingType
{
	Renderer[] renderers { get; }

	bool Disintegrating { get; set; }
}
