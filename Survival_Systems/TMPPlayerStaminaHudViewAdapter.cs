using UnityEngine;

namespace Beam.UI;

public class TMPPlayerStaminaHudViewAdapter : PlayerStaminaHudViewAdapterBase
{
	[SerializeField]
	private GameObjectViewAdapter _panel;

	[SerializeField]
	private UImageViewAdapter _fillColor;

	[SerializeField]
	private TMPSliderViewAdapter _staminaSlider;

	public override IGameObjectViewAdapter Panel => _panel;

	public override void UpdateStamina(float value)
	{
		_staminaSlider.NormalizedValue = value;
	}

	public override void UpdateRunning(bool running)
	{
		if (!base.Visible && running)
		{
			Show();
			_fillColor.Color = Color.yellow;
		}
		else if (base.Visible && !running)
		{
			Hide();
		}
	}

	public override void OutOfBreath(bool outOfBreath)
	{
		if (outOfBreath)
		{
			Show();
			_fillColor.Color = new Color(255f, 0f, 0f);
		}
	}
}
