using UnityEngine;

namespace Beam.UI;

public class TMPTextScreenViewAdapter : TextScreenViewAdapterBase
{
	[SerializeField]
	private GameObject _textScreenGameObject;

	[SerializeField]
	private GameObject _headerGroup;

	[SerializeField]
	private GameObject _subheaderGroup;

	[SerializeField]
	private GameObject _textGroup;

	[SerializeField]
	private GameObject _tooltipGroup;

	[SerializeField]
	private TMPLabelViewAdapter _headerLabel;

	[SerializeField]
	private TMPLabelViewAdapter _subheaderLabel;

	[SerializeField]
	private TMPLabelViewAdapter _textLabel;

	[SerializeField]
	private TMPLabelViewAdapter _tooltipLabel;

	public override void SetHeaderText(string text)
	{
		_headerLabel.Text = text;
		_headerGroup.SetActive(!string.IsNullOrEmpty(_headerLabel.Text));
	}

	public override void SetSubheaderText(string text)
	{
		_subheaderLabel.Text = text;
		_subheaderGroup.SetActive(!string.IsNullOrEmpty(_subheaderLabel.Text));
	}

	public override void SetText(string text)
	{
		_textLabel.Text = text;
		_textGroup.SetActive(!string.IsNullOrEmpty(_textLabel.Text));
	}

	public override void SetTooltipText(string text)
	{
		_tooltipLabel.Text = text;
		_tooltipGroup.SetActive(!string.IsNullOrEmpty(_tooltipLabel.Text));
	}

	public override void Show()
	{
		_headerGroup.SetActive(!string.IsNullOrEmpty(_headerLabel.Text));
		_subheaderGroup.SetActive(!string.IsNullOrEmpty(_subheaderLabel.Text));
		_textGroup.SetActive(!string.IsNullOrEmpty(_textLabel.Text));
		_tooltipGroup.SetActive(!string.IsNullOrEmpty(_tooltipLabel.Text));
		if (!_textScreenGameObject.activeSelf)
		{
			_textScreenGameObject.SetActive(true);
		}
		base.Show();
	}

	public override void Hide()
	{
		if (_textScreenGameObject.activeSelf)
		{
			_textScreenGameObject.SetActive(false);
		}
		base.Hide();
	}
}
