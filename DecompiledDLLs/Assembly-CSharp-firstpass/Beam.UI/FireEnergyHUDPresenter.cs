using UnityEngine;

namespace Beam.UI;

public class FireEnergyHUDPresenter : ControllerBase, INeedPlayerComponent
{
	[SerializeField]
	private FireEnergyHudViewAdapterBase _view;

	private IPlayer _player;

	IPlayer INeedPlayerComponent.Player => _player;

	private void Start()
	{
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"FireEnergyHUDPresenter:: No view component assigned!");
			_view = NullFireEnergyHudViewAdapter.Instance;
		}
	}

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as FireEnergyHudViewAdapterBase;
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"FireEnergyHUDPresenter:: No view component assigned!");
			_view = NullFireEnergyHudViewAdapter.Instance;
		}
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_player.GameInput.FireLighter.ToggleFiring += FireLighter_ToggleFiring;
		_player.GameInput.FireLighter.EnergyPercentUpdated += FireLighter_EnergyUpdated;
	}

	private void FireLighter_EnergyUpdated(float energy)
	{
		_view.UpdateEnergy(energy);
	}

	private void FireLighter_ToggleFiring(bool firing)
	{
		_view.UpdateFiring(firing);
	}

	private void OnDestroy()
	{
		if (_player != null)
		{
			_player.GameInput.FireLighter.ToggleFiring -= FireLighter_ToggleFiring;
			_player.GameInput.FireLighter.EnergyPercentUpdated -= FireLighter_EnergyUpdated;
		}
	}
}
