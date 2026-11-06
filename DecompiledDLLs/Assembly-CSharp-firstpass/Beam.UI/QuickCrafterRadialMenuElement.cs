using System.Collections.Generic;
using UnityEngine;

namespace Beam.UI;

public class QuickCrafterRadialMenuElement : RadialMenuElementBase<CraftingData, TMPQuickCrafterRadialMenuElementViewAdapter>
{
	private QuickCrafterRadialElementType _elementType;

	public QuickCrafterRadialElementType ElementType
	{
		get
		{
			return _elementType;
		}
		set
		{
			_elementType = value;
			View.SetElementType(_elementType);
		}
	}

	public override void Initialize(IList<CraftingData> data)
	{
		base.Initialize(data);
		Format();
	}

	protected virtual void Format()
	{
		if (Data != null)
		{
			View.Image.Sprite = SelectedValue.GetIcon();
			View.Image.Enabled = Object.op_Implicit((Object)(object)View.Image.Sprite);
		}
	}
}
