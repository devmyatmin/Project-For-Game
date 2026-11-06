using Beam.Crafting;
using UnityEngine;

namespace Beam.UI;

public class HotkeyElementView : ViewAdapterBase
{
	[SerializeField]
	private TMPLabelViewAdapter _numberLabel;

	[SerializeField]
	private UImageViewAdapter _lockedIcon;

	[SerializeField]
	private UImageViewAdapter _icon;

	public void Initialize(HotkeyData hotkeyData)
	{
		_numberLabel.Text = hotkeyData.Number.ToString();
		hotkeyData.Locked.ValueChanged += HotkeyData_LockedValueChanged;
		HotkeyData_LockedValueChanged(hotkeyData.Locked.Value);
		hotkeyData.CraftingType.ValueChanged += HotkeyData_CraftingTypeValueChanged;
		HotkeyData_CraftingTypeValueChanged(hotkeyData.CraftingType.Value);
	}

	private void HotkeyData_LockedValueChanged(bool locked)
	{
		_lockedIcon.Enabled = locked;
	}

	private void HotkeyData_CraftingTypeValueChanged(CraftingType craftingType)
	{
		_icon.Enabled = craftingType != CraftingType.Empty;
		if (_icon.Enabled)
		{
			_icon.Sprite = Icons.GetSmallIcon(new IconKey
			{
				CraftingType = craftingType
			});
		}
	}
}
