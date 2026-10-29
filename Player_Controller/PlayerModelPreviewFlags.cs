using System;

namespace Beam.UI;

[Flags]
public enum PlayerModelPreviewFlags
{
	None = 0,
	Male = 1,
	Female = 2,
	Connected = 4,
	Disconnected = 8
}
