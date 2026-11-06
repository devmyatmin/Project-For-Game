using System;
using Beam.Crafting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Beam.UI.Crafting;

public class TMPCraftingSubcategoryButtonViewAdapter : UButtonViewAdapter, ISelectHandler, IEventSystemHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
	public UImageViewAdapter Image;

	private CraftingSubcategory _subcategory;

	private string _name;

	private Sprite _imageSpriteResource;

	public CraftingSubcategory Subcategory => _subcategory;

	public string Name => _name;

	public new event Action<TMPCraftingSubcategoryButtonViewAdapter, PointerEventData> MouseEnter;

	public new event Action<TMPCraftingSubcategoryButtonViewAdapter, PointerEventData> MouseLeave;

	public event Action<TMPCraftingSubcategoryButtonViewAdapter> Deselected;

	protected virtual void OnMouseEnter(TMPCraftingSubcategoryButtonViewAdapter sender, PointerEventData eventData)
	{
		MouseEnter?.Invoke(sender, eventData);
	}

	protected virtual void OnMouseLeave(TMPCraftingSubcategoryButtonViewAdapter sender, PointerEventData eventData)
	{
		MouseLeave?.Invoke(sender, eventData);
	}

	protected virtual void OnDeselected(TMPCraftingSubcategoryButtonViewAdapter sender)
	{
		Deselected?.Invoke(this);
	}

	public void Initialize(CraftingSubcategory subcategory)
	{
		_subcategory = subcategory;
		_name = _subcategory.Name;
		_imageSpriteResource = SpriteDictionary.GetSprite(_subcategory.Name);
		Image.Sprite = _imageSpriteResource;
	}

	protected override void OnDestroy()
	{
		SpriteDictionary.ReturnSprite(_imageSpriteResource);
		MouseEnter = null;
		MouseLeave = null;
		Deselected = null;
		base.OnDestroy();
	}

	public override void PerformClick()
	{
		OnClick(Subcategory);
	}

	void ISelectHandler.OnSelect(BaseEventData eventData)
	{
		OnSelected(this);
	}

	void IDeselectHandler.OnDeselect(BaseEventData eventData)
	{
		OnDeselected(this);
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		OnMouseEnter(this, eventData);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		OnMouseLeave(this, eventData);
	}
}
