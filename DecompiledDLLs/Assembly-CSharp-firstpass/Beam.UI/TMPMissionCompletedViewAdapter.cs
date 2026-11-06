using Beam.Utilities;
using UnityEngine;

namespace Beam.UI;

public class TMPMissionCompletedViewAdapter : MissionCompletedViewAdapterBase
{
	[SerializeField]
	private GameObjectViewAdapter _panel;

	[SerializeField]
	private Transform _rewardElementContainer;

	[SerializeField]
	private MissionRewardElement _rewardElementPrefab;

	[SerializeField]
	private TMPActionHudElementViewAdapter _actionLabelPrefab;

	[SerializeField]
	private Transform _actionLabelContainer;

	public override IGameObjectViewAdapter Panel => _panel;

	public override IMissionRewardElement CreateRewardElement()
	{
		if ((Object)(object)_rewardElementPrefab != (Object)null)
		{
			MissionRewardElement missionRewardElement = Object.Instantiate<MissionRewardElement>(_rewardElementPrefab);
			((Component)missionRewardElement).transform.SetParent(_rewardElementContainer, false);
			return missionRewardElement;
		}
		return NullObject<NullMissionRewardElement>.Instance;
	}

	public override ActionHudElementViewAdapterBase CreateActionLabel()
	{
		if ((Object)(object)_actionLabelPrefab != (Object)null)
		{
			TMPActionHudElementViewAdapter tMPActionHudElementViewAdapter = Object.Instantiate<TMPActionHudElementViewAdapter>(_actionLabelPrefab);
			((Component)tMPActionHudElementViewAdapter).transform.SetParent(_actionLabelContainer, false);
			return tMPActionHudElementViewAdapter;
		}
		return NullActionHudElementViewAdapter.Instance;
	}

	public override void DestroyRewardElements()
	{
		BeamTeam.DestroyAllChildren(_rewardElementContainer);
	}

	public override void DestroyActionLabels()
	{
		BeamTeam.DestroyAllChildren(_actionLabelContainer);
	}
}
