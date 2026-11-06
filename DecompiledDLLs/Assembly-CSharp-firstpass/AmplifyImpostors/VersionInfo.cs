using System;

namespace AmplifyImpostors;

[Serializable]
public class VersionInfo
{
	public const byte Major = 0;

	public const byte Minor = 9;

	public const byte Release = 6;

	public static byte Revision = 3;

	public static int FullNumber => 9600 + Revision;

	public static string FullLabel => "Version=" + FullNumber;

	public static string StaticToString()
	{
		return $"{(byte)0}.{(byte)9}.{(byte)6}" + ((Revision > 0) ? ("r" + Revision) : "");
	}
}
