using System;
using Beam.Crafting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Beam.UI.Crafting;

public class CombinationMaterialPresenter : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	[SerializeField]
	private CombinationMaterialViewAdapterBase _view;

	private string _name;

	private int _available;

	private int _required;

	private MaterialInfo _materialInfo;

	public string Name => _name;

	public event Action<CombinationMaterialPresenter> Click;

	public event Action<CombinationMaterialPresenter, PointerEventData> MouseEnter;

	public event Action<CombinationMaterialPresenter, PointerEventData> MouseLeave;

	public void Initialize(AttributeType attributeType, InteractiveType interactiveType, int available, int required)
	{
		CraftingType craftingType = new CraftingType(attributeType, interactiveType);
		SetName(craftingType);
		_available = available;
		_required = required;
		_view.SetRequired(_required);
		_view.SetAvailable(_available);
		_view.SetIcon(craftingType);
	}

	public void Initialize(MaterialInfo materialInfo)
	{
		_materialInfo = materialInfo;
		_materialInfo.Available.ValueChanged += MaterialInfoAvailableProperty_ValueChanged;
		Initialize(_materialInfo.Type.AttributeType, _materialInfo.Type.InteractiveType, _materialInfo.Available.Value, _materialInfo.Required);
	}

	public void SetMaterialQtyDisplayMode(EMaterialQtyDisplayMode mode)
	{
		_view.MaterialQtyDisplayMode = mode;
	}

	private void SetName(CraftingType type)
	{
		_name = type.ToString();
	}

	private void OnDestroy()
	{
		if (_materialInfo?.Available != null)
		{
			_materialInfo.Available.ValueChanged -= MaterialInfoAvailableProperty_ValueChanged;
		}
		Click = null;
		MouseEnter = null;
		MouseLeave = null;
	}

	private void UpdateQuantity(int available)
	{
		_available = available;
		_view.SetAvailable(_available);
	}

	private void MaterialInfoAvailableProperty_ValueChanged(int value)
	{
		UpdateQuantity(value);
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		MouseEnter?.Invoke(this, eventData);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		MouseLeave?.Invoke(this, eventData);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		Click?.Invoke(this);
	}
}
