using System;
using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.UI;

public class ActiveMissionHudPresenter : ControllerBase
{
	private ActiveMissionHudViewAdapterBase _view;

	[SerializeField]
	private IndicatorPresenter _indicatorPresenter;

	[SerializeField]
	private IndicatorElementBase _indicatorPrefab;

	private IMissionDetailsProvider _missionDetailsProvider;

	private IndicatorElementBase _indicator;

	private CoroutineHandle _indicatorSequence;

	private IEnumerator Start()
	{
		while (LevelLoader.IsLoading())
		{
			yield return null;
		}
		if ((Object)(object)_view == (Object)null)
		{
			_view = NullActiveMissionHudViewAdapter.Instance;
			Debug.LogError((object)"ActiveMissionHudPresenter:: No view component found!");
		}
		IMissionEventListener missionEventListener = MissionEventBroadcasterRelay.GetMissionEventListener();
		((UnityEvent<IMissionDetailsProvider, bool>)missionEventListener.MissionStarted).AddListener((UnityAction<IMissionDetailsProvider, bool>)MissionListener_MissionStarted);
		missionEventListener.MissionEnded.AddListener((UnityAction)MissionListener_MissionEnded);
		missionEventListener.EnteredMissionProximity.AddListener((UnityAction<bool>)MissionListener_EnteredMissionProximity);
		missionEventListener.LeftMissionProximity.AddListener((UnityAction)MissionListener_LeftMissionProximity);
		DisplayMissionNamePresenter uIComponent = GameUI.GetUIComponent<DisplayMissionNamePresenter>();
		if ((Object)(object)uIComponent != (Object)null)
		{
			uIComponent.FadedOut += DisplayMissionNamePresenter_FadedOut;
		}
	}

	private void OnDestroy()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected Obj, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected Obj, but got Unknown
		IMissionEventListener missionEventListener = MissionEventBroadcasterRelay.GetMissionEventListener();
		((UnityEvent<IMissionDetailsProvider, bool>)missionEventListener.MissionStarted).RemoveListener((UnityAction<IMissionDetailsProvider, bool>)MissionListener_MissionStarted);
		missionEventListener.MissionEnded.RemoveListener((UnityAction)MissionListener_MissionEnded);
		missionEventListener.EnteredMissionProximity.RemoveListener((UnityAction<bool>)MissionListener_EnteredMissionProximity);
		missionEventListener.LeftMissionProximity.RemoveListener((UnityAction)MissionListener_LeftMissionProximity);
		DisplayMissionNamePresenter uIComponent = GameUI.GetUIComponent<DisplayMissionNamePresenter>();
		if ((Object)(object)uIComponent != (Object)null)
		{
			uIComponent.FadedOut -= DisplayMissionNamePresenter_FadedOut;
		}
	}

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as ActiveMissionHudViewAdapterBase;
		if ((Object)(object)_view == (Object)null)
		{
			_view = NullActiveMissionHudViewAdapter.Instance;
			Debug.LogError((object)"ActiveMissionHudPresenter:: No view component found!");
		}
	}

	public void MissionListener_MissionStarted(IMissionDetailsProvider missionDetailsProvider, bool alreadyStarted)
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		_missionDetailsProvider = missionDetailsProvider;
		_missionDetailsProvider.MissionGoal.UpdateCompletion += MissionGoal_UpdatedCompletion;
		_view.MissionNameLabel.Text = _missionDetailsProvider.MissionData.Name;
		if (!alreadyStarted)
		{
			_view.CompletionSlider.Value = 1f;
		}
		else
		{
			_view.CompletionSlider.Value = _missionDetailsProvider.MissionGoal.GetCompletionPercent();
		}
		if ((Object)(object)_indicator == (Object)null)
		{
			_indicator = Object.Instantiate<IndicatorElementBase>(_indicatorPrefab);
		}
		_indicator.Initialize(new IndicatorDataBase(_missionDetailsProvider.MissionGoal.Location));
		_indicatorSequence = Timing.RunCoroutine(UpdateIndicator_Sequence());
	}

	public void MissionListener_MissionEnded()
	{
		Finish();
	}

	public void MissionListener_EnteredMissionProximity(bool showHUD)
	{
		if (showHUD)
		{
			Show();
		}
		HideIndicator();
	}

	public void MissionListener_LeftMissionProximity()
	{
		Hide();
		ShowIndicator();
	}

	public void DisplayMissionNamePresenter_FadedOut(IMissionDetailsProvider missionDetailsProvider)
	{
		ShowIndicator();
	}

	private void MissionGoal_UpdatedCompletion()
	{
		_view.CompletionSlider.Value = _missionDetailsProvider.MissionGoal.GetCompletionPercent();
		Flash();
	}

	private void Show()
	{
		_view.Show();
	}

	private void Hide()
	{
		_view.Hide();
	}

	private void ShowIndicator()
	{
		_indicatorPresenter.AddIndicator(_indicator);
		((Component)_indicator).gameObject.SetActive(true);
	}

	private void HideIndicator()
	{
		((Component)_indicator).gameObject.SetActive(false);
	}

	private void Finish()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_indicatorSequence);
		if (_missionDetailsProvider != null)
		{
			_missionDetailsProvider.MissionGoal.UpdateCompletion -= MissionGoal_UpdatedCompletion;
		}
		HideIndicator();
		_indicatorPresenter.DestroyIndicator(_indicator);
		Hide();
	}

	private IEnumerator<float> UpdateIndicator_Sequence()
	{
		while (_missionDetailsProvider.MissionGoal != null && !_missionDetailsProvider.MissionGoal.Equals(null))
		{
			_indicator.Data.Direction = _missionDetailsProvider.MissionGoal.Location;
			yield return 0f;
		}
	}

	private void Flash()
	{
		LeanTween.cancel(((Component)this).gameObject);
		LeanTween.value(((Component)this).gameObject, (Action<float>)Flash_Callback, 0f, 1f, 0.15f).setOnComplete((Action)(() =>
		{
			LeanTween.value(((Component)this).gameObject, (Action<float>)Flash_Callback, 1f, 0f, 0.15f);
		}));
	}

	private void Flash_Callback(float value)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		_view.CompletionSliderFill.Color = Color.Lerp(Color.white, Color.red, value);
	}
}
