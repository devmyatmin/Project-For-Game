using System.Collections.Generic;
using UnityEngine;

namespace Beam.UI;

public class PlatformHotkeyViewProvider : HotkeyView, INeedPlayerComponent
{
	[SerializeField]
	private HotkeyView _kbViewPrefab;

	[SerializeField]
	private HotkeyView _dpViewPrefab;

	private HotkeyView _kbView;

	private HotkeyView _dpView;

	private HotkeyView _view;

	private IPlayer _player;

	public IPlayer Player => _player;

	private HotkeyView GetView()
	{
		if (!RewiredUtilities.HasJoystick(Player.Input))
		{
			return _kbView;
		}
		return _dpView;
	}

	public override void OnAssignmentCreated()
	{
		GetView().OnAssignmentCreated();
	}

	public override void OnAssignmentRemoved()
	{
		GetView().OnAssignmentRemoved();
	}

	public override void OnAssignmentFailed()
	{
		GetView().OnAssignmentFailed();
	}

	public void SetPlayer(IPlayer player)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		_player = player;
		_kbView = Object.Instantiate<HotkeyView>(_kbViewPrefab);
		((Component)_kbView).transform.SetParent(((Component)this).transform, false);
		((Component)_kbView).transform.localPosition = Vector3.zero;
		((Component)_kbView).transform.localRotation = Quaternion.identity;
		_dpView = Object.Instantiate<HotkeyView>(_dpViewPrefab);
		((Component)_dpView).transform.SetParent(((Component)this).transform, false);
		((Component)_dpView).transform.localPosition = Vector3.zero;
		((Component)_dpView).transform.localRotation = Quaternion.identity;
	}

	public override void Initialize(IEnumerable<HotkeyData> hotkeys)
	{
		_kbView.Initialize(hotkeys);
		_dpView.Initialize(hotkeys);
	}

	public override void Show()
	{
		_view = GetView();
		_view.Show();
	}

	public override void Hide()
	{
		if (!((Object)(object)_view == (Object)null))
		{
			_view.Hide();
			_view = null;
		}
	}
}
