using System.Runtime.CompilerServices;
using UnityEngine;

namespace Beam.UI.MapEditor;

[RequireComponent(typeof(MapEditorSaveMenuPresenter))]
public class UMapEditorSaveMenuViewAdapter : ViewAdapterBase, IMapEditorSaveMenuViewAdapter, IViewAdapter
{
	[SerializeField]
	private GameObject _savePanel;

	public Texture2D DefaultPreviewImage;

	public GameObject PreviewImageSlotPrefab;

	public GameObject PreviewImageSlotContainer;

	[SerializeField]
	private UCanvasGroupViewAdapter _canvasGroup;

	[SerializeField]
	private TMPLabelButtonViewAdapter _saveButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _backButton;

	[SerializeField]
	private TMPInputFieldViewAdapter _nameInputField;

	[SerializeField]
	private TMPInputFieldViewAdapter _descriptionInputField;

	[SerializeField]
	private UImageViewAdapter _previewImage;

	public ICanvasGroupViewAdapter CanvasGroup => _canvasGroup;

	public ILabelButtonViewAdapter SaveButton => _saveButton;

	public ILabelButtonViewAdapter BackButton => _backButton;

	public IInputFieldViewAdapter NameInputField => _nameInputField;

	public IInputFieldViewAdapter DescriptionInputField => _descriptionInputField;

	public IImageViewAdapter PreviewImage => _previewImage;

	private void Awake()
	{
		Object.FindObjectOfType<MapEditorSaveMenuPresenter>().Initialize(this);
	}

	public override void Show()
	{
		if (!_savePanel.activeSelf)
		{
			_savePanel.SetActive(value: true);
			base.Show();
		}
	}

	public override void Hide()
	{
		if (_savePanel.activeSelf)
		{
			_savePanel.SetActive(value: false);
			base.Hide();
		}
	}

	[SpecialName]
	Transform IViewAdapter.get_transform()
	{
		return base.transform;
	}
}
