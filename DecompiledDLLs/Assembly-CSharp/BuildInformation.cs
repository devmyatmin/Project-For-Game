using System;
using Photon.Bolt;
using UnityEngine;

public static class BuildInformation
{
	public static ushort Major = 1;

	public static ushort Minor = 0;

	public static ushort Build = 38;

	public static string Region = "";

	private static ushort _multiplayerVersion = 29;

	public static readonly Version BuildVersion = new Version(Major, Minor, Build);

	public static readonly Version MinimumVersion = new Version(0, 18, 1);

	public static ushort MultiplayerVersion => _multiplayerVersion;

	public static string GetPrettyString()
	{
		string text = $"{BuildVersion}.0.{MultiplayerVersion}";
		if (Application.isPlaying && BoltNetwork.IsDebugMode)
		{
			text += "d";
		}
		return text;
	}
}
