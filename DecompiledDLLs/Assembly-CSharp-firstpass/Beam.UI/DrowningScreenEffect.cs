using UnityEngine;

namespace Beam.UI;

public class DrowningScreenEffect : ControllerBase, INeedPlayerComponent
{
	[SerializeField]
	private UTextureViewAdapter _view;

	private IPlayer _player;

	public IPlayer Player => _player;

	private void Start()
	{
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"DrowningScreenEffect:: No view component assigned!");
			_view = NullRawTextureViewAdapter.Instance;
		}
		_view.Enabled = false;
	}

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as UTextureViewAdapter;
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"DrowningScreenEffect:: No view component assigned!");
			_view = NullRawTextureViewAdapter.Instance;
		}
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_player.Statistics.DrowningUpdated += PlayerStatistics_DrowningUpdated;
		_player.Statistics.DrowningRestored += PlayerStatistics_DrowningRestored;
	}

	private void OnDestroy()
	{
		if (_player != null)
		{
			_player.Statistics.DrowningUpdated -= PlayerStatistics_DrowningUpdated;
			_player.Statistics.DrowningRestored -= PlayerStatistics_DrowningRestored;
		}
	}

	private void PlayerStatistics_DrowningUpdated(float value)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		_view.Enabled = true;
		_view.Color = new Color(0f, 0f, 0f, value);
	}

	private void PlayerStatistics_DrowningRestored()
	{
		_view.Enabled = false;
	}
}
