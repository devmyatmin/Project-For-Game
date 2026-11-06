using System.Collections.Generic;
using Beam.Events;
using Beam.Language;
using MEC;
using Rewired;
using UnityEngine;

namespace Beam.UI;

public class MissionCompletedPresenter : MonoBehaviour
{
	[SerializeField]
	private MissionCompletedViewAdapterBase _view;

	private void Awake()
	{
		if ((Object)(object)_view == (Object)null)
		{
			_view = NullMissionCompletedViewAdapter.Instance;
			Debug.LogError((object)"MissionCompletedPresenter::Awake:: No view component!");
		}
	}

	private IEnumerator<float> Show_Sequence(IMissionDetailsProvider missionDetailsProvider)
	{
		float timer = 0f;
		while (timer < 0f)
		{
			timer += Time.deltaTime;
			yield return 0f;
		}
		Show(missionDetailsProvider);
	}

	private void Show(IMissionDetailsProvider missionDetailsProvider)
	{
		Time.timeScale = 0f;
		RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, false, 0);
		RewiredUtilities.SetAndCacheMapsEnabled(PlayerRegistry.LocalPlayer.Input, state: true, 16);
		PlayerRegistry.LocalPlayer.PlayerUI.Refresh(RefreshUIMode.Clear);
		CreateActionLabels();
		CreateRewardElements(missionDetailsProvider.MissionData.RewardProvider);
		_view.Show();
	}

	public void Hide()
	{
		Time.timeScale = 1f;
		RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, true, 0);
		RewiredUtilities.SetAndCacheMapsEnabled(PlayerRegistry.LocalPlayer.Input, state: false, 16);
		PlayerRegistry.LocalPlayer.PlayerUI.Refresh(RefreshUIMode.Refresh);
		EventManager.RaiseEvent(new FreezeNotificationsEvent(freeze: false));
		EventManager.RaiseEvent(new FreezePlayerEffectsEvent(freeze: false));
		_view.Hide();
	}

	private void CreateActionLabels()
	{
		_view.DestroyActionLabels();
		CreateActionLabel();
	}

	private ActionHudElementViewAdapterBase CreateActionLabel()
	{
		string text = Localization.GetLanguageHandler().Localize("PLAYER_ACTION_SENTENCE", "PLAYER_ACTION_PRESS", "PLAYER_ACTION_PREPOSITION", "PLAYER_ACTION_CONTINUE");
		text = PlatformLocalization.GetHandler().SubstituteControllerGlyphs(text, 0, 30);
		ActionHudElementViewAdapterBase actionHudElementViewAdapterBase = _view.CreateActionLabel();
		actionHudElementViewAdapterBase.PressLabel.Text = text;
		return actionHudElementViewAdapterBase;
	}

	private void CreateRewardElements(IMissionRewardProvider missionRewardProvider)
	{
		_view.DestroyRewardElements();
		IMissionReward[] rewards = missionRewardProvider.GetRewards();
		foreach (IMissionReward reward in rewards)
		{
			_view.CreateRewardElement().Initialize(reward);
		}
	}

	private void Update()
	{
		if (_view.Visible && PlayerRegistry.LocalPlayer.Input.GetAnyButtonDown())
		{
			AcceptButton_Click(null);
		}
	}

	public void MissionEventBroadcaster_MissionCompleted(IMissionDetailsProvider missionDetailsProvider)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Timing.RunCoroutine(Show_Sequence(missionDetailsProvider));
	}

	private void AcceptButton_Click(object sender)
	{
		Hide();
	}
}
