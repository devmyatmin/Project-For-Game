using UnityEngine;

namespace Beam.UI;

public class AboutMenuPresenter : MonoBehaviour, IOptionPanel
{
	[SerializeField]
	private AboutMenuView _view;

	[SerializeField]
	private CreditsAsset _creditsAsset;

	private void Awake()
	{
		_view.SetCreditsText(_creditsAsset.CreditsText);
		_view.SetCopyrightText(_creditsAsset.CopyrightText);
	}

	public Transform GetCategoriesContainer()
	{
		return _view.CategoriesContainer;
	}

	public void Show()
	{
		_view.Show();
	}

	public void Hide()
	{
		if (_view.Visible)
		{
			_view.Hide();
		}
	}

	public void Apply()
	{
	}
}
