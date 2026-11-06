using UnityEngine;

namespace Beam.UI;

public class UAboutView : AboutMenuView
{
	[SerializeField]
	private GameObject _panel;

	[SerializeField]
	private Transform _categoriesContainer;

	[SerializeField]
	private TMPLabelViewAdapter _copyrightText;

	[SerializeField]
	private TMPLabelViewAdapter _creditsText;

	public override Transform CategoriesContainer => _categoriesContainer;

	private void Awake()
	{
		Visible = _panel.activeSelf;
	}

	public override void SetCreditsText(string text)
	{
		_creditsText.Text = text;
	}

	public override void SetCopyrightText(string text)
	{
		_copyrightText.Text = text;
	}

	public override void Show()
	{
		base.Show();
		_panel.SetActive(Visible);
	}

	public override void Hide()
	{
		base.Hide();
		_panel.SetActive(Visible);
	}
}
