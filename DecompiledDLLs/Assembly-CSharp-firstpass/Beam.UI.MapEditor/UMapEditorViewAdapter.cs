using UnityEngine;

namespace Beam.UI.MapEditor;

public class UMapEditorViewAdapter : ViewAdapterBase, IMapEditorMenuViewAdapter, IViewAdapter
{
	[SerializeField]
	private GameObject _mapEditorCanvas;

	[SerializeField]
	private UCanvasGroupViewAdapter _canvasGroup;

	[SerializeField]
	private TMPLabelButtonViewAdapter _resumeButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _saveButton;

	[SerializeField]
	private TMPLabelButtonViewAdapter _quitButton;

	[SerializeField]
	public TMPLabelViewAdapter _mapTitleLabel;

	ICanvasGroupViewAdapter IMapEditorMenuViewAdapter.CanvasGroup => _canvasGroup;

	public ILabelButtonViewAdapter ResumeButton => _resumeButton;

	public ILabelButtonViewAdapter SaveButton => _saveButton;

	public ILabelButtonViewAdapter QuitButton => _quitButton;

	public ILabelViewAdapter MapTitleLabel => _mapTitleLabel;

	private void Awake()
	{
		Object.FindObjectOfType<MapEditorMenuPresenter>().Initialize(this);
	}

	public override void Show()
	{
		if (!_mapEditorCanvas.activeSelf)
		{
			_mapEditorCanvas.SetActive(true);
			base.Show();
		}
	}

	public override void Hide()
	{
		if (_mapEditorCanvas.activeSelf)
		{
			_mapEditorCanvas.SetActive(false);
			base.Hide();
		}
	}
}
