using System;
using System.Collections.Generic;

namespace Beam.Language;

[Serializable]
public class ControllerGlyphSet
{
	public string Name;

	public Guid Guid;

	public List<ElementGlyphMap> ElementGlyphMaps = new List<ElementGlyphMap>();
}
