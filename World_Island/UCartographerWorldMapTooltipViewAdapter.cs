using System.Runtime.CompilerServices;
using Beam.Terrain;
using Beam.Utilities.Reflection;
using Beam.Utilities.UGUI;
using UnityEngine;

namespace Beam.UI;

public class UCartographerWorldMapTooltipViewAdapter : ViewAdapterBase, ICartographerWorldMapTooltipViewAdapter, IViewAdapter
{
	[SerializeField]
	private GameObject _tooltipGameObject;

	[SerializeField]
	private UCanvasGroupViewAdapter _canvasGroup;

	[SerializeField]
	private TMPLabelViewAdapter _nameLabel;

	[SerializeField]
	private TMPLabelViewAdapter _authorLabel;

	[SerializeField]
	private TMPLabelViewAdapter _descriptionLabel;

	[SerializeField]
	private TMPLabelViewAdapter _versionNumberLabel;

	[SerializeField]
	private TMPLabelViewAdapter _statusLabel;

	[SerializeField]
	private TMPLabelViewAdapter _sourceLabel;

	public GameObject TooltipGameObject => _tooltipGameObject;

	public ICanvasGroupViewAdapter CanvasGroup => _canvasGroup;

	public ILabelViewAdapter NameLabel => _nameLabel;

	public ILabelViewAdapter AuthorLabel => _authorLabel;

	public ILabelViewAdapter DescriptionLabel => _descriptionLabel;

	public ILabelViewAdapter VersionNumberLabel => _versionNumberLabel;

	public ILabelViewAdapter StatusLabel => _statusLabel;

	private void Awake()
	{
		Object.FindObjectOfType<CartographerWorldMapTooltipPresenter>().Initialize(this);
	}

	public override void Show()
	{
		if (!_tooltipGameObject.activeSelf)
		{
			_tooltipGameObject.SetActive(value: true);
			base.Show();
		}
	}

	public override void Hide()
	{
		if (_tooltipGameObject.activeSelf)
		{
			_tooltipGameObject.SetActive(value: false);
			base.Hide();
		}
	}

	public void SetPosition(Vector2 screenPosition)
	{
		RectTransform obj = (RectTransform)base.transform.parent;
		float num = obj.sizeDelta.x / (float)Screen.width;
		float num2 = obj.sizeDelta.y / (float)Screen.height;
		Vector2 anchoredPosition = new Vector2(screenPosition.x * num, screenPosition.y * num2);
		RectTransform rectTransform = (RectTransform)_canvasGroup.transform;
		if (anchoredPosition.x + rectTransform.rect.width > (float)Screen.width)
		{
			anchoredPosition.x -= rectTransform.rect.width;
		}
		if (anchoredPosition.y - rectTransform.rect.height < 0f)
		{
			anchoredPosition.y += rectTransform.rect.height;
		}
		((RectTransform)base.transform).anchoredPosition = anchoredPosition;
	}

	public void SetSource(MapEditorData.EMapSource source)
	{
		Color color = ((source == MapEditorData.EMapSource.Local || source != MapEditorData.EMapSource.Community) ? ((Color)UGUIColor.TEXT_BRIGHT) : ((Color)UGUIColor.TEXT_CYAN));
		_sourceLabel.Text = source.GetAttribute<DescriptionAttribute>().Text;
		_sourceLabel.Color = color;
	}

	public void SetState(MapEditorData.EMapState state)
	{
		Color color = state switch
		{
			MapEditorData.EMapState.Older => UGUIColor.TEXT_YELLOW, 
			MapEditorData.EMapState.Unavailable => UGUIColor.TEXT_RED, 
			_ => UGUIColor.TEXT_GREEN, 
		};
		_statusLabel.Text = state.GetAttribute<DescriptionAttribute>().Text;
		_statusLabel.Color = color;
	}

	[SpecialName]
	GameObject ICartographerWorldMapTooltipViewAdapter.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	Transform IViewAdapter.get_transform()
	{
		return base.transform;
	}
}
