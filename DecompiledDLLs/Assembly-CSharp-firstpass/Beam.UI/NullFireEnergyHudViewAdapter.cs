using UnityEngine;

namespace Beam.UI;

public class NullFireEnergyHudViewAdapter : FireEnergyHudViewAdapterBase
{
	private static NullFireEnergyHudViewAdapter _Instance = new NullFireEnergyHudViewAdapter();

	public static NullFireEnergyHudViewAdapter Instance => _Instance;

	public override IGameObjectViewAdapter Panel
	{
		get
		{
			Debug.LogError((object)"NullFireEnergyHudViewAdapter::Panel:: Not implemented!");
			return NullObject<NullGameObjectViewAdapter>.Instance;
		}
	}

	public override void UpdateEnergy(float value)
	{
		Debug.LogError((object)"NullFireEnergyHudViewAdapter::UpdateEnergy:: Not implemented!");
	}

	public override void UpdateFiring(bool firing)
	{
		Debug.LogError((object)"NullFireEnergyHudViewAdapter::UpdateFiring:: Not implemented!");
	}
}
