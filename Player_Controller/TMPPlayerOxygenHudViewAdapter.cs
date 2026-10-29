using UnityEngine;

namespace Beam.UI;

public class TMPPlayerOxygenHudViewAdapter : PlayerOxygenHudViewAdapterBase
{
	[SerializeField]
	private GameObjectViewAdapter _panel;

	[SerializeField]
	private TMPSliderViewAdapter _oxygenSlider;

	public override IGameObjectViewAdapter Panel => _panel;

	public override void UpdateOxygen(float value)
	{
		_oxygenSlider.NormalizedValue = value;
	}

	public override void UpdateUnderwater(bool underwater)
	{
		if (!base.Visible && underwater)
		{
			Show();
		}
		else if (base.Visible && !underwater)
		{
			Hide();
		}
	}
}
