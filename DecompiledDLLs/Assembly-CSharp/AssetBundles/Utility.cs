using UnityEngine;

namespace AssetBundles;

public class Utility
{
	public const string AssetBundlesOutputPath = "Assets/Data/Bundles";

	public static string GetPlatformName()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return GetPlatformForAssetBundles(Application.platform);
	}

	private static string GetPlatformForAssetBundles(RuntimePlatform platform)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		if ((int)platform <= 2)
		{
			if ((int)platform == 1)
			{
				return "OSX";
			}
			if ((int)platform == 2)
			{
				return "Windows";
			}
		}
		else
		{
			if ((int)platform == 8)
			{
				return "iOS";
			}
			if ((int)platform == 11)
			{
				return "Android";
			}
			if ((int)platform == 17)
			{
				return "WebGL";
			}
		}
		return null;
	}
}
