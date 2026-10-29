using System.Collections.Generic;

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
			base.View.SetElementType(_elementType);
		}
	}

	public override void Initialize(IList<CraftingData> data)
	{
		base.Initialize(data);
		Format();
	}

	protected virtual void Format()
	{
		if (base.Data != null)
		{
			base.View.Image.Sprite = base.SelectedValue.GetIcon();
			base.View.Image.Enabled = base.View.Image.Sprite;
		}
	}
}
