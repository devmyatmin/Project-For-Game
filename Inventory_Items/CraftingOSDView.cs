using UnityEngine;

namespace Beam.UI.Crafting;

public class CraftingOSDView : ViewAdapterBase
{
	[SerializeField]
	private GameObject _panel;

	[SerializeField]
	private GameObject _rotateLabelGroup;

	public void SetCanRotate(bool canRotate)
	{
		_rotateLabelGroup.SetActive(canRotate);
	}

	public override void Show()
	{
		_panel.SetActive(value: true);
		base.Show();
	}

	public override void Hide()
	{
		_panel.SetActive(value: false);
		base.Hide();
	}
}
