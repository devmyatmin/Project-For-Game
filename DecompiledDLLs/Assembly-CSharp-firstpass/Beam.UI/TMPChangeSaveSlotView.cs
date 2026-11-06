using UnityEngine;

namespace Beam.UI;

public class TMPChangeSaveSlotView : ChangeSaveSlotViewBase
{
	[SerializeField]
	private GameObject _changeSaveSlotViewGameObject;

	[SerializeField]
	private GameObject _slotButtonsGroup;

	[SerializeField]
	private UButtonViewAdapter _applyButton;

	[SerializeField]
	private UButtonViewAdapter _cancelButton;

	private TMPLabelButtonViewAdapter[] _slotButtons;

	[SerializeField]
	private ButtonMarker _slotButtonMarker;

	public override IButtonViewAdapter ApplyButton => _applyButton;

	public override IButtonViewAdapter CancelButton => _cancelButton;

	public override IButtonViewAdapter[] SlotButtons
	{
		get
		{
			if (_slotButtons == null)
			{
				_slotButtons = _slotButtonsGroup.GetComponentsInChildren<TMPLabelButtonViewAdapter>(true);
			}
			return _slotButtons;
		}
	}

	private void Awake()
	{
		Visible = _changeSaveSlotViewGameObject.activeSelf;
		IButtonViewAdapter[] slotButtons = SlotButtons;
		for (int i = 0; i < slotButtons.Length; i++)
		{
			TMPLabelButtonViewAdapter tMPLabelButtonViewAdapter = (TMPLabelButtonViewAdapter)slotButtons[i];
			tMPLabelButtonViewAdapter.Text = string.Format("{0} {1}", "CHANGE_SAVE_SLOT_MENU_BUTTON_SLOT", ((Component)tMPLabelButtonViewAdapter).transform.GetSiblingIndex() + 1);
			tMPLabelButtonViewAdapter.Click += SlotButton_Click;
		}
	}

	private void SlotButton_Click(object sender)
	{
		TMPLabelButtonViewAdapter button = (TMPLabelButtonViewAdapter)sender;
		MarkButton(button);
	}

	private void MarkButton(IButtonViewAdapter button)
	{
		ButtonMarker slotButtonMarker = _slotButtonMarker;
		if (slotButtonMarker != null)
		{
			Transform transform = button.transform;
			slotButtonMarker.SetPosition((RectTransform)(object)((transform is RectTransform) ? transform : null));
		}
	}

	public override void Show()
	{
		if (!_changeSaveSlotViewGameObject.activeSelf)
		{
			_changeSaveSlotViewGameObject.SetActive(true);
			MarkButton(GetFirstButton());
			Visible = true;
			OnViewShow();
		}
	}

	public override void Hide()
	{
		if (_changeSaveSlotViewGameObject.activeSelf)
		{
			Visible = false;
			OnViewHide();
			_changeSaveSlotViewGameObject.SetActive(false);
		}
	}

	public override IButtonViewAdapter GetFirstButton()
	{
		int lastSaveSlotUsed = Options.GeneralSettings.LastSaveSlotUsed;
		return ((Component)_slotButtonsGroup.transform.GetChild(lastSaveSlotUsed)).GetComponent<TMPLabelButtonViewAdapter>();
	}
}
