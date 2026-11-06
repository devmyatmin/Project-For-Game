using UnityEngine;

namespace Beam.UI;

public class NullPlayerFishingHudViewAdapter : PlayerFishingHudViewAdapterBase
{
	private static NullPlayerFishingHudViewAdapter _Instance = new NullPlayerFishingHudViewAdapter();

	public static NullPlayerFishingHudViewAdapter Instance => _Instance;

	public override IGameObjectViewAdapter Panel
	{
		get
		{
			Debug.LogError((object)"NullPlayerFishingHudViewAdapter::Panel:: Not implemented!");
			return NullObject<NullGameObjectViewAdapter>.Instance;
		}
	}

	public override void UpdateReel(float value)
	{
		Debug.LogError((object)"NullPlayerFishingHudViewAdapter::UpdateReel:: Not implemented!");
	}

	public override void UpdateReelShow(bool show)
	{
		Debug.LogError((object)"NullPlayerFishingHudViewAdapter::UpdateReelShow:: Not implemented!");
	}
}
