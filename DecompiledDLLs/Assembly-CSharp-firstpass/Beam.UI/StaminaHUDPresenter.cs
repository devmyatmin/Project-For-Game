using UnityEngine;

namespace Beam.UI;

public class StaminaHUDPresenter : ControllerBase, INeedPlayerComponent
{
	[SerializeField]
	private PlayerStaminaHudViewAdapterBase _view;

	private IPlayer _player;

	IPlayer INeedPlayerComponent.Player => _player;

	private void Start()
	{
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"StaminaHudPresenter:: No view component assigned!");
			_view = NullPlayerStaminaHudViewAdapter.Instance;
		}
	}

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as PlayerStaminaHudViewAdapterBase;
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"StaminaHudPresenter:: No view component assigned!");
			_view = NullPlayerStaminaHudViewAdapter.Instance;
		}
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		if (_player != null)
		{
			_player.PlayerCamera.ToggleRunning += PlayerCamera_ToggleRunning;
			_player.PlayerCamera.OutOfBreath += PlayerCamera_OutOfBreath;
			_player.Statistics.StaminaUpdated += Statistics_StaminaUpdated;
		}
	}

	private void OnDestroy()
	{
		if (_player != null)
		{
			_player.PlayerCamera.ToggleRunning -= PlayerCamera_ToggleRunning;
			_player.PlayerCamera.OutOfBreath -= PlayerCamera_OutOfBreath;
			_player.Statistics.StaminaUpdated -= Statistics_StaminaUpdated;
		}
	}

	private void PlayerCamera_ToggleRunning(bool running)
	{
		if (_player == null || !_player.Statistics.Invincible)
		{
			_view.UpdateRunning(running);
		}
	}

	private void PlayerCamera_OutOfBreath(bool outOfBreath)
	{
		if (_player == null || !_player.Statistics.Invincible)
		{
			_view.OutOfBreath(outOfBreath);
		}
	}

	private void Statistics_StaminaUpdated(float stamina)
	{
		if (_player == null || !_player.Statistics.Invincible)
		{
			_view.UpdateStamina(stamina);
		}
	}
}
