using System;
using System.Collections.Generic;
using Beam.Crafting;
using Beam.Utilities;
using MEC;
using UnityEngine;

namespace Beam.UI;

public class RadialBarPresenter : ControllerBase, INeedPlayerComponent, IRefresh
{
	[SerializeField]
	private RadialBarViewAdapterBase _view;

	private IPlayer _player;

	private Raycaster _raycaster;

	private User _user;

	private IHaveAttribute _displayable;

	private CoroutineHandle _showSequence;

	private bool _suspended;

	IPlayer INeedPlayerComponent.Player => _player;

	private void Start()
	{
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"RadialBarPresenter:: No view component found!");
			_view = NullRadialBarViewAdapter.Instance;
		}
	}

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as RadialBarViewAdapterBase;
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"RadialBarPresenter:: No view component found!");
			_view = NullRadialBarViewAdapter.Instance;
		}
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_user = _player.GameInput.User;
		_user.UsedOnObject += User_UsedOnObject;
		_raycaster = _player.GameInput.Raycaster;
		_raycaster.EnterObject += Raycaster_EnterObject;
		_raycaster.LeaveObject += Raycaster_LeaveObject;
	}

	private void OnDestroy()
	{
		if ((Object)(object)_user != (Object)null)
		{
			_user.UsedOnObject -= User_UsedOnObject;
		}
		if ((Object)(object)_raycaster != (Object)null)
		{
			_raycaster.EnterObject -= Raycaster_EnterObject;
			_raycaster.LeaveObject -= Raycaster_LeaveObject;
		}
	}

	private void User_UsedOnObject(IBase obj)
	{
		if (!_showSequence.IsRunning)
		{
			Refresh(RefreshUIMode.Refresh);
		}
	}

	private void Raycaster_EnterObject(IBase obj)
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		WorldSpaceAttributeController worldSpaceAttributeController = default;
		if (!_suspended && !BeamTeam.IsNullOrDestroyed(obj) && !((Component)obj.transform).TryGetComponent<WorldSpaceAttributeController>(ref worldSpaceAttributeController))
		{
			_displayable = obj as IHaveAttribute;
			if (BeamTeam.IsNullOrDestroyed(_displayable))
			{
				((Component)_view.CanvasGroup.transform).gameObject.SetActive(false);
				return;
			}
			if (!BeamTeam.IsNullOrDestroyed(obj as IConstructable) && _displayable.Type == IconType.Construction && _displayable.GetValue() == 1f)
			{
				((Component)_view.CanvasGroup.transform).gameObject.SetActive(false);
				return;
			}
			Reset();
			_showSequence = Timing.RunCoroutine(Show_Sequence());
		}
	}

	private void Raycaster_LeaveObject(IBase obj)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_showSequence);
		FadeOutAndReset();
	}

	private IEnumerator<float> Show_Sequence()
	{
		_view.RadialBar.Fill = Mathf.Max(0f, _displayable.GetValue());
		_view.SetIcon(_displayable.Type);
		((Component)_view.CanvasGroup.transform).gameObject.SetActive(true);
		while (!BeamTeam.IsNullOrDestroyed(_displayable))
		{
			_view.RadialBar.SmoothFill(Mathf.Max(0f, _displayable.GetValue()), 15f);
			yield return 0f;
		}
	}

	private void FadeOutAndReset()
	{
		LeanTween.value(((Component)this).gameObject, (Action<float>)FadeCallback, 1f, 0f, 0.2f).setOnComplete((Action)Reset);
	}

	private void Reset()
	{
		LeanTween.cancel(((Component)this).gameObject);
		_view.CanvasGroup.Alpha = 1f;
		((Component)_view.CanvasGroup.transform).gameObject.SetActive(false);
	}

	public void Suspend(bool suspend)
	{
		_suspended = suspend;
		Refresh((!_suspended) ? RefreshUIMode.Refresh : RefreshUIMode.Clear);
	}

	public void Refresh(RefreshUIMode mode)
	{
		switch (mode)
		{
		case RefreshUIMode.Clear:
			Reset();
			break;
		case RefreshUIMode.Refresh:
			Raycaster_EnterObject(_raycaster.CurrentObject);
			break;
		}
	}

	private void FadeCallback(float value)
	{
		_view.CanvasGroup.Alpha = value;
	}
}
