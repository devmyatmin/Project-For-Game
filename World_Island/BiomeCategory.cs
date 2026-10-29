using System;
using System.Collections.Generic;

namespace Beam;

[Serializable]
public class BiomeCategory
{
	public Zone.BiomeType Biome;

	public List<ZoneObjects> ProceduralObjects;
}
