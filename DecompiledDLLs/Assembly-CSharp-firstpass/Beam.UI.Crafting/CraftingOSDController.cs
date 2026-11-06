using Beam.Crafting;
using UnityEngine;

namespace Beam.UI.Crafting;

public class CraftingOSDController : ControllerBase, INeedPlayerComponent
{
	private CraftingOSDView _view;

	private IPlayer _player;

	private Crafter _crafter;

	public IPlayer Player => _player;

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as CraftingOSDView;
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_crafter = _player.Crafter;
		_crafter.StartedPlacing += Crafter_StartedPlacing;
		_crafter.StoppedPlacing += Crafter_StoppedPlacing;
	}

	private void OnDestroy()
	{
		if ((Object)(object)_crafter != (Object)null)
		{
			_crafter.StartedPlacing -= Crafter_StartedPlacing;
			_crafter.StoppedPlacing -= Crafter_StoppedPlacing;
		}
	}

	private void Crafter_StartedPlacing()
	{
		if (Options.GeneralSettings.HudInformation != Quality_HUDInformation.OFF)
		{
			if (_crafter.PlacingCraftable is Constructing constructing)
			{
				bool canRotate = constructing.RotationMode == Constructing.ERotationMode.Free;
				_view.SetCanRotate(canRotate);
			}
			else
			{
				_view.SetCanRotate(canRotate: true);
			}
			_view.Show();
		}
	}

	private void Crafter_StoppedPlacing()
	{
		_view.Hide();
	}
}
