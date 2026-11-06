using System;
using Funlabs;
using UnityEngine;

namespace Beam.UI;

public class DisplayMissionNamePresenter : MonoBehaviour
{
	[SerializeField]
	private bool alreadyShowed;

	[SerializeField]
	private DisplayMissionNameViewAdapterBase _view;

	private const float START_DISTANCE = 100f;

	public event Action<IMissionDetailsProvider> FadedOut;

	protected virtual void OnFadedOut(IMissionDetailsProvider missionDetailsProvider)
	{
		FadedOut?.Invoke(missionDetailsProvider);
	}

	private void Awake()
	{
		if ((Object)(object)_view == (Object)null)
		{
			_view = NullDisplayMissionNameViewAdapter.Instance;
			Debug.LogError((object)"DisplayMissionNamePresenter::Awake:: No view component found!");
		}
	}

	private void OnDestroy()
	{
		FadedOut = null;
	}

	public void MissionListener_MissionStarted(IMissionDetailsProvider missionDetailsProvider, bool alreadyStarted)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val;
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			val = missionDetailsProvider.MissionGoal.Location - PlayerRegistry.LocalPlayer.transform.position;
			if (val.magnitude > 100f)
			{
				return;
			}
		}
		_view.MissionNameLabel.Text = missionDetailsProvider.MissionData.Name;
		_view.MissionNameGroup.Alpha = 0f;
		if (!alreadyShowed)
		{
			val = missionDetailsProvider.MissionGoal.Location - PlayerRegistry.LocalPlayer.transform.position;
			if (val.magnitude < 100f)
			{
				alreadyShowed = true;
				FadeIn(missionDetailsProvider);
			}
		}
	}

	public void MissionListener_MissionEnded()
	{
		alreadyShowed = false;
		LeanTween.cancel(((Component)this).gameObject, false);
		_view.Hide();
	}

	private void FadeIn(IMissionDetailsProvider missionDetailsProvider)
	{
		_view.Show();
		LeanTween.value(((Component)this).gameObject, (Action<float>)Fade_Update, 0f, 1f, 2f).setOnComplete((Action)(() =>
		{
			FadeOut(missionDetailsProvider);
		}));
	}

	private void FadeOut(IMissionDetailsProvider missionDetailsProvider)
	{
		LeanTween.value(((Component)this).gameObject, (Action<float>)Fade_Update, 1f, 0f, 2f).setDelay(7f).setOnComplete((Action)(() =>
		{
			OnFadedOut(missionDetailsProvider);
			_view.Hide();
		}));
	}

	private void Fade_Update(float value)
	{
		_view.MissionNameGroup.Alpha = value;
	}
}
