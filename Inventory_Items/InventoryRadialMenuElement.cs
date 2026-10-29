using System.Collections.Generic;
using Beam.Utilities;
using UnityEngine;

namespace Beam.UI;

public class InventoryRadialMenuElement : RadialMenuElementBase<InventoryData, UInventoryRadialMenuElementViewAdapter>
{
	public override void Initialize(IList<InventoryData> data)
	{
		base.Initialize(data);
		Format();
	}

	protected bool IsEmptySlot()
	{
		if (base.Data != null && base.Data.Count != 0 && base.SelectedValue != null)
		{
			return base.SelectedValue.Value.IsNullOrDestroyed();
		}
		return true;
	}

	protected virtual void Format()
	{
		if (IsEmptySlot())
		{
			base.View.Image.Enabled = false;
			base.View.DepletedImage.Enabled = false;
			base.View.QuantityGroup.SetActive(value: false);
			return;
		}
		base.View.QuantityLabel.Text = $"{base.SelectedIndex + 1}/{base.Data.Count}";
		GameObject quantityGroup = base.View.QuantityGroup;
		IList<InventoryData> data = base.Data;
		quantityGroup.SetActive(data != null && data.Count > 1);
		base.View.Image.Color = Color.white;
		base.View.Image.Sprite = base.SelectedValue.GetIcon();
		base.View.Image.Enabled = base.View.Image.Sprite;
		base.View.Image.Fill = base.SelectedValue.GetHealthNormalized();
		base.View.DepletedImage.Sprite = base.View.Image.Sprite;
		base.View.DepletedImage.Enabled = base.View.DepletedImage.Sprite;
	}

	public override bool NavigateLeft()
	{
		bool num = base.NavigateLeft();
		if (num)
		{
			Format();
		}
		return num;
	}

	public override bool NavigateRight()
	{
		bool num = base.NavigateRight();
		if (num)
		{
			Format();
		}
		return num;
	}
}
