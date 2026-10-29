using Beam.Events;
using UnityEngine;

namespace Beam.UI;

public class LoadingScreenPresenter : MonoBehaviour
{
	private readonly string[] _hints = new string[22]
	{
		"TEXT_SCREEN_LABEL_LOADING_TIP_1", "TEXT_SCREEN_LABEL_LOADING_TIP_2", "TEXT_SCREEN_LABEL_LOADING_TIP_3", "TEXT_SCREEN_LABEL_LOADING_TIP_4", "TEXT_SCREEN_LABEL_LOADING_TIP_5", "TEXT_SCREEN_LABEL_LOADING_TIP_6", "TEXT_SCREEN_LABEL_LOADING_TIP_7", "TEXT_SCREEN_LABEL_LOADING_TIP_8", "TEXT_SCREEN_LABEL_LOADING_TIP_9", "TEXT_SCREEN_LABEL_LOADING_TIP_10",
		"TEXT_SCREEN_LABEL_LOADING_TIP_11", "TEXT_SCREEN_LABEL_LOADING_TIP_12", "TEXT_SCREEN_LABEL_LOADING_TIP_13", "TEXT_SCREEN_LABEL_LOADING_TIP_14", "TEXT_SCREEN_LABEL_LOADING_TIP_15", "TEXT_SCREEN_LABEL_LOADING_TIP_16", "TEXT_SCREEN_LABEL_LOADING_TIP_17", "TEXT_SCREEN_LABEL_LOADING_TIP_18", "TEXT_SCREEN_LABEL_LOADING_TIP_19", "TEXT_SCREEN_LABEL_LOADING_TIP_20",
		"TEXT_SCREEN_LABEL_LOADING_TIP_21", "TEXT_SCREEN_LABEL_LOADING_TIP_22"
	};

	[SerializeField]
	private TextScreenViewAdapterBase _view;

	[SerializeField]
	private LoadingProgressViewBase _ticker;

	private void Start()
	{
		EventManager.AddListener<LoadingProgressChangedEvent>(EventManager_LoadingProgressChanged);
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<LoadingProgressChangedEvent>(EventManager_LoadingProgressChanged);
	}

	private void EventManager_LoadingProgressChanged(LoadingProgressChangedEvent e)
	{
		_view.SetSubheaderText(e.LoadingMessage);
	}

	public void Show(string header = "TEXT_SCREEN_LABEL_LOADING")
	{
		string tooltipText = _hints[Random.Range(0, _hints.Length)];
		_view.SetHeaderText(header);
		_view.SetText(string.Empty);
		_view.SetTooltipText(tooltipText);
		_view.Show();
		_ticker.Show();
	}

	public void Hide()
	{
		_view.Hide();
		_ticker.Hide();
	}
}
