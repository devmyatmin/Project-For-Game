using System;

[Flags]
public enum EFailMode : byte
{
	None = 0,
	Save = 1,
	Load = 2,
	Delete = 4,
	All = byte.MaxValue
}
