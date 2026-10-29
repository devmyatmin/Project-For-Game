using UnityEngine;

namespace Beam.UI;

public class TMPPlayerFishingHudViewAdapter : PlayerFishingHudViewAdapterBase
{
	[SerializeField]
	private GameObjectViewAdapter _panel;

	[SerializeField]
	private TMPSliderViewAdapter _reelSlider;

	public override IGameObjectViewAdapter Panel => _panel;

	public override void UpdateReel(float value)
	{
		_reelSlider.NormalizedValue = value;
	}

	public override void UpdateReelShow(bool show)
	{
		if (!base.Visible && show)
		{
			Show();
		}
		else if (base.Visible && !show)
		{
			Hide();
		}
	}
}
