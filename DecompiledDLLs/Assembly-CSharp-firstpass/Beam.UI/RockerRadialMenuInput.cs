using Rewired;

namespace Beam.UI;

public class RockerRadialMenuInput : RadialMenuInput
{
	private readonly IRadialMenuController _menu;

	public RockerRadialMenuInput(IRadialMenuController menu, Player input)
		: base(input)
	{
		_menu = menu;
	}

	public override void CheckInput()
	{
		OpenState open = _menu.Open;
		if (open == OpenState.Reserved)
		{
			return;
		}
		bool flag = open.IsOpen();
		if (Input.GetButtonDown(12))
		{
			if (!flag)
			{
				_menu.Show();
			}
		}
		else if (!Input.GetButton(12) && flag)
		{
			OnOpenKeyUp();
			_menu.Hide();
		}
	}
}
