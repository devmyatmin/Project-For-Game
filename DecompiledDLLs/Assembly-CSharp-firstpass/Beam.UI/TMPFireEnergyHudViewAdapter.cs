using UnityEngine;

namespace Beam.UI;

public class TMPFireEnergyHudViewAdapter : FireEnergyHudViewAdapterBase
{
	[SerializeField]
	private GameObjectViewAdapter _panel;

	[SerializeField]
	private TMPSliderViewAdapter _fireEnergySlider;

	public override IGameObjectViewAdapter Panel => _panel;

	public override void UpdateEnergy(float value)
	{
		_fireEnergySlider.NormalizedValue = value;
	}

	public override void UpdateFiring(bool firing)
	{
		if (!Visible & firing)
		{
			Show();
		}
		else if (Visible && !firing)
		{
			Hide();
		}
	}
}
