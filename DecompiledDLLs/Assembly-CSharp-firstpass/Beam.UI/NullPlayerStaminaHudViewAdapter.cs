using UnityEngine;

namespace Beam.UI;

public class NullPlayerStaminaHudViewAdapter : PlayerStaminaHudViewAdapterBase
{
	private static NullPlayerStaminaHudViewAdapter _Instance = new NullPlayerStaminaHudViewAdapter();

	public static NullPlayerStaminaHudViewAdapter Instance => _Instance;

	public override IGameObjectViewAdapter Panel
	{
		get
		{
			Debug.LogError((object)"NullPlayerStaminaHudViewAdapter::Panel:: Not implemented!");
			return NullObject<NullGameObjectViewAdapter>.Instance;
		}
	}

	public override void UpdateStamina(float value)
	{
		Debug.LogError((object)"NullPlayerStaminaHudViewAdapter::UpdateStamina:: Not implemented!");
	}

	public override void UpdateRunning(bool running)
	{
		Debug.LogError((object)"NullPlayerStaminaHudViewAdapter::UpdateRunning:: Not implemented!");
	}

	public override void OutOfBreath(bool outOfBreath)
	{
		Debug.LogError((object)"NullPlayerStaminaHudViewAdapter::OutOfBreath:: Not implemented!");
	}
}
