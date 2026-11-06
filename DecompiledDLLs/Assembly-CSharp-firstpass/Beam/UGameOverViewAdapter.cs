using Beam.UI;
using UnityEngine;

namespace Beam;

public class UGameOverViewAdapter : ViewAdapterBase, IGameOverView, IViewAdapter
{
	[SerializeField]
	private GameOverPresenter _presenter;

	[SerializeField]
	private GameObject _gameOverPanel;

	[SerializeField]
	private TMPLabelViewAdapter _causeOfDeathLabel;

	[SerializeField]
	private TMPLabelViewAdapter _daysSurvivedLabel;

	[SerializeField]
	private TMPLabelViewAdapter _permadeathLabel;

	public ILabelViewAdapter CauseOfDeathLabel => _causeOfDeathLabel;

	public ILabelViewAdapter DaysSurvivedLabel => _daysSurvivedLabel;

	public ILabelViewAdapter PermadeathLabel => _permadeathLabel;

	private void Awake()
	{
		_presenter?.Initialize(this);
	}

	public override void Show()
	{
		if (!_gameOverPanel.activeSelf)
		{
			_gameOverPanel.SetActive(true);
			base.Show();
		}
	}

	public override void Hide()
	{
		if (_gameOverPanel.activeSelf)
		{
			_gameOverPanel.SetActive(false);
			base.Hide();
		}
	}
}
