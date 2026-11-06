using System;

namespace Beam.Rendering;

[Flags]
public enum ImposterScope
{
	None = 0,
	All = 6,
	Manual = 1,
	Local = 2,
	Terrain = 4
}
