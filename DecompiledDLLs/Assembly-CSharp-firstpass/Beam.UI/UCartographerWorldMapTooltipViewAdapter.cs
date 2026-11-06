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
			_tooltipGameObject.SetActive(true);
			base.Show();
		}
	}

	public override void Hide()
	{
		if (_tooltipGameObject.activeSelf)
		{
			_tooltipGameObject.SetActive(false);
			base.Hide();
		}
	}

	public void SetPosition(Vector2 screenPosition)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected Obj, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = (RectTransform)((Component)this).transform.parent;
		float num = val.sizeDelta.x / (float)Screen.width;
		float num2 = val.sizeDelta.y / (float)Screen.height;
		Vector2 val2 = new Vector2(screenPosition.x * num, screenPosition.y * num2);
		RectTransform val3 = (RectTransform)((Component)_canvasGroup).transform;
		float x = val2.x;
		Rect rect = val3.rect;
		if (x + rect.width > (float)Screen.width)
		{
			ref float x2 = ref val2.x;
			float num3 = x2;
			rect = val3.rect;
			x2 = num3 - rect.width;
		}
		float y = val2.y;
		rect = val3.rect;
		if (y - rect.height < 0f)
		{
			ref float y2 = ref val2.y;
			float num4 = y2;
			rect = val3.rect;
			y2 = num4 + rect.height;
		}
		((RectTransform)((Component)this).transform).anchoredPosition = val2;
	}

	public void SetSource(MapEditorData.EMapSource source)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Color color = ((source == MapEditorData.EMapSource.Local || source != MapEditorData.EMapSource.Community) ? Color32.op_Implicit(UGUIColor.TEXT_BRIGHT) : Color32.op_Implicit(UGUIColor.TEXT_CYAN));
		_sourceLabel.Text = Reflection.GetAttribute<DescriptionAttribute>(source).Text;
		_sourceLabel.Color = color;
	}

	public void SetState(MapEditorData.EMapState state)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Color color = state switch
		{
			MapEditorData.EMapState.Older => Color32.op_Implicit(UGUIColor.TEXT_YELLOW), 
			MapEditorData.EMapState.Unavailable => Color32.op_Implicit(UGUIColor.TEXT_RED), 
			_ => Color32.op_Implicit(UGUIColor.TEXT_GREEN), 
		};
		_statusLabel.Text = Reflection.GetAttribute<DescriptionAttribute>(state).Text;
		_statusLabel.Color = color;
	}
}
