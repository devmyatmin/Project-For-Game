using Beam.Language;
using Beam.Utilities;
using UnityEngine;

namespace Beam.UI;

public class MissionRewardElement : MonoBehaviour, IMissionRewardElement
{
	[SerializeField]
	private MissionRewardElementViewAdapterBase _view;

	[SerializeField]
	private TransformViewAdapter _transform;

	public ITransformViewAdapter Transform => _transform;

	private void Awake()
	{
		if ((Object)(object)_view == (Object)null)
		{
			_view = NullMissionRewardElementViewAdapter.Instance;
			Debug.LogError((object)"MissionRewardElement::Awake:: No view component!");
		}
	}

	public void Initialize(IMissionReward missionReward)
	{
		_view.RewardQuantityLabel.Text = string_safemethods.Format(Localization.GetLanguageHandler().Localize("MISSIONS_MISSION_REWARD_QUANTITY_LABEL"), missionReward.Quantity);
		_view.RewardLabel.Text = missionReward.Name;
		_view.RewardImage.Sprite = missionReward.GetIcon();
	}
}
