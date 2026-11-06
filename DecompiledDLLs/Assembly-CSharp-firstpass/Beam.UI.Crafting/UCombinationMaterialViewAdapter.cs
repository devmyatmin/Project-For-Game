using Beam.Crafting;
using Beam.Utilities.UGUI;
using UnityEngine;

namespace Beam.UI.Crafting;

public class UCombinationMaterialViewAdapter : CombinationMaterialViewAdapterBase
{
	[SerializeField]
	private TMPLabelViewAdapter _quantityLabel;

	[SerializeField]
	private UImageViewAdapter _icon;

	[SerializeField]
	private USelectableViewAdapter _selectable;

	private EMaterialQtyDisplayMode _materialQtyDisplayMode;

	private int _requiredCnt;

	public override EMaterialQtyDisplayMode MaterialQtyDisplayMode
	{
		set
		{
			_materialQtyDisplayMode = value;
		}
	}

	public override void SetIcon(CraftingType type)
	{
		_icon.Sprite = Icons.GetSmallIcon(new IconKey
		{
			CraftingType = type
		});
	}

	public override void SetRequired(int value)
	{
		_requiredCnt = value;
		if (_materialQtyDisplayMode == EMaterialQtyDisplayMode.Required)
		{
			_quantityLabel.Text = $"{_requiredCnt}x";
		}
	}

	public override void SetAvailable(int value)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (_materialQtyDisplayMode == EMaterialQtyDisplayMode.AvailableAndRequired)
		{
			_quantityLabel.Text = $"{value}/{_requiredCnt}";
		}
		bool flag = value >= _requiredCnt;
		_quantityLabel.Color = Color32.op_Implicit(flag ? UGUIColor.TEXT_BRIGHT : UGUIColor.TEXT_RED);
		_selectable.Interactable = flag;
	}
}
