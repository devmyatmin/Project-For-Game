using UnityEngine;

namespace Beam;

public class Bundle
{
	public AssetBundle AssetBundle { get; set; }

	public int ReferenceCount { get; set; }

	public Bundle()
	{
		ReferenceCount = 1;
	}

	public void Unload()
	{
		if ((Object)(object)AssetBundle != (Object)null)
		{
			_ = ((Object)AssetBundle).name;
			AssetBundle.Unload(true);
		}
	}
}
