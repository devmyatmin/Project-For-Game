using Rewired;

namespace Beam.UI;

public class ToggleRadialMenuInput : RadialMenuInput
{
	private readonly IRadialMenuController _menu;

	public ToggleRadialMenuInput(IRadialMenuController menu, Player input)
		: base(input)
	{
		_menu = menu;
	}

	public override void CheckInput()
	{
		if (!Input.GetButtonUp(12))
		{
			return;
		}
		OpenState open = _menu.Open;
		if (open != OpenState.Reserved)
		{
			if (!open.IsOpen())
			{
				_menu.Show();
				return;
			}
			OnOpenKeyUp();
			_menu.Hide();
		}
	}
}
