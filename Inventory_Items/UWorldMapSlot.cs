using System.Runtime.CompilerServices;
using Beam.Terrain;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Beam.UI.MapEditor;

public class UWorldMapSlot : ViewAdapterBase, IWorldMapSlot, IViewAdapter, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
	[SerializeField]
	private UTextureViewAdapter _previewImage;

	[SerializeField]
	private GameObject _statusIcon;

	[SerializeField]
	private UButtonViewAdapter _button;

	public Map Map { get; private set; }

	public ITextureViewAdapter PreviewImage => _previewImage;

	public IButtonViewAdapter Button => _button;

	public event MapSlotHandler Click;

	public event MapSlotHandler Enter;

	public event MapSlotHandler Leave;

	public event MapSlotHandler Drop;

	protected virtual void OnClick(IWorldMapSlot sender, PointerEventData eventData)
	{
		this.Click?.Invoke(sender, eventData);
	}

	protected virtual void OnEnter(IWorldMapSlot sender, PointerEventData eventData)
	{
		this.Enter?.Invoke(sender, eventData);
	}

	protected virtual void OnLeave(IWorldMapSlot sender, PointerEventData eventData)
	{
		this.Leave?.Invoke(sender, eventData);
	}

	protected virtual void OnDrop(IWorldMapSlot sender, PointerEventData eventData)
	{
		this.Drop?.Invoke(this, eventData);
	}

	void IDropHandler.OnDrop(PointerEventData eventData)
	{
		OnDrop(this, eventData);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		OnClick(this, eventData);
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		OnEnter(this, eventData);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		OnLeave(this, eventData);
	}

	protected void OnDestroy()
	{
		this.Click = null;
		this.Enter = null;
		this.Leave = null;
		this.Drop = null;
	}

	public void SetMap(Map map)
	{
		Map = map;
		PreviewImage.Texture = map.EditorData.SmallPreviewImage;
		SetState(map.EditorData.State);
	}

	public void SetState(MapEditorData.EMapState state)
	{
		bool active = state != MapEditorData.EMapState.Current;
		_statusIcon.SetActive(active);
	}

	[SpecialName]
	GameObject IWorldMapSlot.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	Transform IViewAdapter.get_transform()
	{
		return base.transform;
	}
}
