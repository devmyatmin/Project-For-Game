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
		if (Data != null && Data.Count != 0 && SelectedValue != null)
		{
			return BeamTeam.IsNullOrDestroyed(SelectedValue.Value);
		}
		return true;
	}

	protected virtual void Format()
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (IsEmptySlot())
		{
			View.Image.Enabled = false;
			View.DepletedImage.Enabled = false;
			View.QuantityGroup.SetActive(false);
			return;
		}
		View.QuantityLabel.Text = $"{SelectedIndex + 1}/{Data.Count}";
		GameObject quantityGroup = View.QuantityGroup;
		IList<InventoryData> data = Data;
		quantityGroup.SetActive(data != null && data.Count > 1);
		View.Image.Color = Color.white;
		View.Image.Sprite = SelectedValue.GetIcon();
		View.Image.Enabled = Object.op_Implicit((Object)(object)View.Image.Sprite);
		View.Image.Fill = SelectedValue.GetHealthNormalized();
		View.DepletedImage.Sprite = View.Image.Sprite;
		View.DepletedImage.Enabled = Object.op_Implicit((Object)(object)View.DepletedImage.Sprite);
	}

	public override bool NavigateLeft()
	{
		bool flag = base.NavigateLeft();
		if (flag)
		{
			Format();
		}
		return flag;
	}

	public override bool NavigateRight()
	{
		bool flag = base.NavigateRight();
		if (flag)
		{
			Format();
		}
		return flag;
	}
}
