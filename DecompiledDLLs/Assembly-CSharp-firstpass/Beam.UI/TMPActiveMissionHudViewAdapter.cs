using UnityEngine;

namespace Beam.UI;

public class TMPActiveMissionHudViewAdapter : ActiveMissionHudViewAdapterBase
{
	[SerializeField]
	private GameObjectViewAdapter _panel;

	[SerializeField]
	private TMPLabelViewAdapter _missionNameLabel;

	[SerializeField]
	private TMPSliderViewAdapter _healthSlider;

	[SerializeField]
	private UImageViewAdapter _healthSliderFill;

	public override IGameObjectViewAdapter Panel => _panel;

	public override ILabelViewAdapter MissionNameLabel => _missionNameLabel;

	public override ISliderViewAdapter CompletionSlider => _healthSlider;

	public override IImageViewAdapter CompletionSliderFill => _healthSliderFill;
}
