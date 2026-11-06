using UnityEngine;

namespace Beam.UI;

public class FishingRodHud : ControllerBase
{
	[SerializeField]
	private GameObjectViewAdapter _panel;

	[SerializeField]
	private TMPSliderViewAdapter _reelSlider;

	[SerializeField]
	private TMPSliderViewAdapter _enduranceSlider;

	public IGameObjectViewAdapter Panel => _panel;

	public override void SetView(ViewAdapterBase view)
	{
	}

	public void UpdateReelValue(float value)
	{
		_reelSlider.NormalizedValue = value;
	}

	public void UpdateEnduranceValue(float value)
	{
		_enduranceSlider.NormalizedValue = value;
	}

	public void UpdateReelShow(bool show)
	{
		if (show)
		{
			Show();
		}
		else if (!show)
		{
			Hide();
		}
	}

	private void Show()
	{
		if (!_panel.Active)
		{
			_panel.Active = true;
		}
	}

	private void Hide()
	{
		if (_panel.Active)
		{
			_panel.Active = false;
		}
	}
}
