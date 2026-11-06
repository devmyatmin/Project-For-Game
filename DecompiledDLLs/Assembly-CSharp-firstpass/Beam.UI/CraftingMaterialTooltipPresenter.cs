using UnityEngine;
using UnityEngine.EventSystems;

namespace Beam.UI;

public class CraftingMaterialTooltipPresenter : ControllerBase
{
	[SerializeField]
	private CraftingMaterialTooltipViewAdapterBase _view;

	private void Start()
	{
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"CraftingMaterialTooltipPresenter:: No view component assigned!");
			_view = NullCraftingMaterialTooltipViewAdapter.Instance;
		}
	}

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as CraftingMaterialTooltipViewAdapterBase;
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"CraftingMaterialTooltipPresenter:: No view component assigned!");
			_view = NullCraftingMaterialTooltipViewAdapter.Instance;
		}
	}

	public void Show(string message, CraftingTooltipType tooltipType, PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector2 screenPosition = eventData.pointerCurrentRaycast.screenPosition;
		Show(message, tooltipType, Vector2.op_Implicit(screenPosition));
	}

	public void Show(string message, CraftingTooltipType tooltipType, Vector3 screenPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_view.SetPosition(Vector2.op_Implicit(screenPosition));
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
