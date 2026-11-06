using Beam.UI;
using UnityEngine;

namespace Beam.Developer.UI;

public class TMPDeveloperConsoleViewAdapter : ViewAdapterBase, IDeveloperConsoleViewAdapter, IViewAdapter
{
	[SerializeField]
	private GameObject _consoleCanvasGameObject;

	[SerializeField]
	private TMPLabelViewAdapter _outputLabel;

	[SerializeField]
	private UScrollbarViewAdapter _scrollbar;

	[SerializeField]
	private TMPInputFieldViewAdapter _inputField;

	public ILabelViewAdapter OutputLabel => _outputLabel;

	public IScrollbarViewAdapter Scrollbar => _scrollbar;

	public IInputFieldViewAdapter InputField => _inputField;

	private void Awake()
	{
		Object.FindObjectOfType<DeveloperConsolePresenter>().Initialize(this);
	}

	public override void Show()
	{
		if (!_consoleCanvasGameObject.activeSelf)
		{
			_consoleCanvasGameObject.SetActive(true);
			base.Show();
		}
	}

	public override void Hide()
	{
		if (_consoleCanvasGameObject.activeSelf)
		{
			_consoleCanvasGameObject.SetActive(false);
			base.Hide();
		}
	}
}
