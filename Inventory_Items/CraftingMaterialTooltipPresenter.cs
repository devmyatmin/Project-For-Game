using UnityEngine;
using UnityEngine.EventSystems;

namespace Beam.UI;

public class CraftingMaterialTooltipPresenter : ControllerBase
{
	[SerializeField]
	private CraftingMaterialTooltipViewAdapterBase _view;

	private void Start()
	{
		if (_view == null)
		{
			Debug.LogError("CraftingMaterialTooltipPresenter:: No view component assigned!");
			_view = NullCraftingMaterialTooltipViewAdapter.Instance;
		}
	}

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as CraftingMaterialTooltipViewAdapterBase;
		if (_view == null)
		{
			Debug.LogError("CraftingMaterialTooltipPresenter:: No view component assigned!");
			_view = NullCraftingMaterialTooltipViewAdapter.Instance;
		}
	}

	public void Show(string message, CraftingTooltipType tooltipType, PointerEventData eventData)
	{
		Vector2 screenPosition = eventData.pointerCurrentRaycast.screenPosition;
		Show(message, tooltipType, screenPosition);
	}

	public void Show(string message, CraftingTooltipType tooltipType, Vector3 screenPosition)
	{
		_view.SetPosition(screenPosition);
		_view.SetDescription(message);
		_view.Show();
	}

	public void Hide(CraftingTooltipType tooltipType)
	{
		_view.Hide();
	}

	public void Hide()
	{
		_view.Hide();
	}
}
