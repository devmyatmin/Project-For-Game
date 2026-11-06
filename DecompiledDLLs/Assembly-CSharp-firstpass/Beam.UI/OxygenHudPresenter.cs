using UnityEngine;

namespace Beam.UI;

public class OxygenHudPresenter : ControllerBase, INeedPlayerComponent
{
	[SerializeField]
	private PlayerOxygenHudViewAdapterBase _view;

	private IPlayer _player;

	IPlayer INeedPlayerComponent.Player => _player;

	private void Start()
	{
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"OxygenHudPresenter:: No view component assigned!");
			_view = NullPlayerOxygenHudViewAdapter.Instance;
		}
	}

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as PlayerOxygenHudViewAdapterBase;
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"OxygenHudPresenter:: No view component assigned!");
			_view = NullPlayerOxygenHudViewAdapter.Instance;
		}
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		if (_player != null)
		{
			_player.PlayerCamera.ToggleUnderwater += PlayerCamera_ToggleUnderwater;
			_player.Statistics.OxygenUpdated += Statistics_OxygenUpdated;
		}
	}

	private void OnDestroy()
	{
		if (_player != null)
		{
			_player.PlayerCamera.ToggleUnderwater -= PlayerCamera_ToggleUnderwater;
			_player.Statistics.OxygenUpdated -= Statistics_OxygenUpdated;
		}
	}

	private void PlayerCamera_ToggleUnderwater(bool underwater)
	{
		if (_player == null || !_player.Statistics.Invincible)
		{
			_view.UpdateUnderwater(underwater);
		}
	}

	private void Statistics_OxygenUpdated(float oxygen)
	{
		if (_player == null || !_player.Statistics.Invincible)
		{
			_view.UpdateOxygen(oxygen);
		}
	}
}
