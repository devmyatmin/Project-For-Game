using UnityEngine;

namespace Beam.UI;

public class NullPlayerOxygenHudViewAdapter : PlayerOxygenHudViewAdapterBase
{
	private static NullPlayerOxygenHudViewAdapter _Instance = new NullPlayerOxygenHudViewAdapter();

	public static NullPlayerOxygenHudViewAdapter Instance => _Instance;

	public override IGameObjectViewAdapter Panel
	{
		get
		{
			Debug.LogError((object)"NullPlayerOxygenHudViewAdapter::Panel:: Not implemented!");
			return NullObject<NullGameObjectViewAdapter>.Instance;
		}
	}

	public override void UpdateOxygen(float value)
	{
		Debug.LogError((object)"NullPlayerOxygenHudViewAdapter::UpdateOxygen:: Not implemented!");
	}

	public override void UpdateUnderwater(bool underwater)
	{
		Debug.LogError((object)"NullPlayerOxygenHudViewAdapter::UpdateUnderwater:: Not implemented!");
	}
}
