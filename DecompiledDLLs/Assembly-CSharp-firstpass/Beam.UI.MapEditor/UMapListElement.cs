using Beam.Terrain;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Beam.UI.MapEditor;

public class UMapListElement : ViewAdapterBase, IMapListElement, IViewAdapter, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	[SerializeField]
	private TMPLabelViewAdapter _nameLabel;

	[SerializeField]
	private TMPLabelViewAdapter _descriptionLabel;

	[SerializeField]
	private TMPLabelViewAdapter _authorLabel;

	[SerializeField]
	private TMPLabelViewAdapter _versionLabel;

	[SerializeField]
	private UTextureViewAdapter _previewImage;

	[SerializeField]
	private TMPLabelButtonViewAdapter _editButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _deleteButton;

	[SerializeField]
	private bool _popular;

	[SerializeField]
	private bool _featured;

	public Map Map { get; set; }

	public ILabelViewAdapter NameLabel => _nameLabel;

	public ILabelViewAdapter DescriptionLabel => _descriptionLabel;

	public ILabelViewAdapter AuthorLabel => _authorLabel;

	public ILabelViewAdapter VersionLabel => _versionLabel;

	public ITextureViewAdapter PreviewImage => _previewImage;

	public ILabelButtonViewAdapter EditButton => _editButton;

	public ILabelButtonViewAdapter DeleteButton => _deleteButton;

	public bool Popular
	{
		get
		{
			return _popular;
		}
		set
		{
			_popular = value;
		}
	}

	public bool Featured
	{
		get
		{
			return _featured;
		}
		set
		{
			_featured = value;
		}
	}

	public event MapListElementHandler EditClick;

	public event MapListElementHandler DeleteClick;

	public event MapListElementHandler BeginDrag;

	public event MapListElementHandler Drag;

	public event MapListElementHandler EndDrag;

	protected virtual void OnEditClick(UMapListElement sender, PointerEventData eventData)
	{
		EditClick?.Invoke(sender, eventData);
	}

	protected virtual void OnDeleteClick(UMapListElement sender, PointerEventData eventData)
	{
		DeleteClick?.Invoke(sender, eventData);
	}

	protected virtual void OnBeginDrag(UMapListElement sender, PointerEventData eventData)
	{
		BeginDrag?.Invoke(this, eventData);
	}

	protected virtual void OnDrag(UMapListElement sender, PointerEventData eventData)
	{
		Drag?.Invoke(this, eventData);
	}

	protected virtual void OnEndDrag(UMapListElement sender, PointerEventData eventData)
	{
		EndDrag?.Invoke(this, eventData);
	}

	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
	{
		OnBeginDrag(this, eventData);
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		OnDrag(this, eventData);
	}

	void IEndDragHandler.OnEndDrag(PointerEventData eventData)
	{
		OnEndDrag(this, eventData);
	}

	private void Awake()
	{
		_editButton.Click += EditButton_Click;
		_deleteButton.Click += DeleteButton_Click;
	}

	private void EditButton_Click(object sender)
	{
		OnEditClick(this, null);
	}

	private void DeleteButton_Click(object sender)
	{
		OnDeleteClick(this, null);
	}

	protected void OnDestroy()
	{
		EditClick = null;
		DeleteClick = null;
		BeginDrag = null;
		Drag = null;
		EndDrag = null;
	}
}
