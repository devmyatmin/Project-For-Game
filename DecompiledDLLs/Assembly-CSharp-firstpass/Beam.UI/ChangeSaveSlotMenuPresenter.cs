using System;
using Beam.Utilities;
using Rewired;
using UnityEngine;

namespace Beam.UI;

public class ChangeSaveSlotMenuPresenter : MonoBehaviour
{
	[SerializeField]
	private ChangeSaveSlotViewBase _view;

	private IViewAdapter _parent;

	private int _slotIndex;

	public event Action ShowView;

	public event Action HideView;

	public event Action SlotChanged;

	private void Awake()
	{
		_view.ShowView += View_ShowView;
		_view.HideView += View_HideView;
		_view.ApplyButton.Click += ApplyButton_Click;
		_view.CancelButton.Click += CancelButton_Click;
		IButtonViewAdapter[] slotButtons = _view.SlotButtons;
		for (int i = 0; i < slotButtons.Length; i++)
		{
			slotButtons[i].Click += SlotButton_Click;
		}
	}

	public void Initialize(IViewAdapter parent)
	{
		_parent = parent;
		if (!BeamTeam.IsNullOrDestroyed(_parent))
		{
			_parent.HideView += Hide;
		}
	}

	private void OnDestroy()
	{
		if (!BeamTeam.IsNullOrDestroyed(_parent))
		{
			_parent.HideView -= Hide;
		}
		_view.ShowView -= View_ShowView;
		_view.HideView -= View_HideView;
		_view.ApplyButton.Click -= ApplyButton_Click;
		_view.CancelButton.Click -= CancelButton_Click;
		IButtonViewAdapter[] slotButtons = _view.SlotButtons;
		for (int i = 0; i < slotButtons.Length; i++)
		{
			slotButtons[i].Click -= SlotButton_Click;
		}
		ShowView = null;
		HideView = null;
		SlotChanged = null;
	}

	public void Show()
	{
		ControllerFocus.Instance?.Subscribe(_view);
		_slotIndex = Options.GeneralSettings.LastSaveSlotUsed;
		_view.Show();
		_view.SelectFirstButton();
		_view.ApplyButton.Enabled = false;
	}

	public void Hide()
	{
		ControllerFocus.Instance?.Unsubscribe(_view);
		_view.Hide();
	}

	private void Update()
	{
		Player player = ReInput.players.GetPlayer(0);
		if ((player.GetButtonDown(39) || player.GetButtonDown(31)) && _view.Visible)
		{
			CancelButton_Click(this);
		}
	}

	private void View_ShowView()
	{
		ShowView?.Invoke();
	}

	private void View_HideView()
	{
		HideView?.Invoke();
	}

	private void ApplyButton_Click(object sender)
	{
		if (SaveManager.ChangeCurrentSlot(_slotIndex))
		{
			Options.Save();
			SlotChanged?.Invoke();
		}
		Hide();
	}

	private void CancelButton_Click(object sender)
	{
		Hide();
	}

	private void SlotButton_Click(object sender)
	{
		_view.ApplyButton.Enabled = true;
		IButtonViewAdapter buttonViewAdapter = (IButtonViewAdapter)sender;
		_slotIndex = buttonViewAdapter.transform.GetSiblingIndex();
	}
}
