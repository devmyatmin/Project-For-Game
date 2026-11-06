using UnityEngine;

namespace Beam.UI;

public class TMPDisplayMissionNameViewAdapter : DisplayMissionNameViewAdapterBase
{
	[SerializeField]
	private GameObjectViewAdapter _panel;

	[SerializeField]
	private TMPLabelViewAdapter _missionNameLabel;

	[SerializeField]
	private UCanvasGroupViewAdapter _missionNameGroup;

	public override IGameObjectViewAdapter Panel => _panel;

	public override ILabelViewAdapter MissionNameLabel => _missionNameLabel;

	public override ICanvasGroupViewAdapter MissionNameGroup => _missionNameGroup;
}
