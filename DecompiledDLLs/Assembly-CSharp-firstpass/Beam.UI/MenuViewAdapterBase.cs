using Beam.Utilities;
using Rewired;

namespace Beam.UI;

public abstract class MenuViewAdapterBase : ViewAdapterBase, IMenuPanel
{
	private Player _input;

	public virtual bool Active => Visible;

	public Player Input => _input;

	public virtual void SetInput(Player input)
	{
		_input = input;
	}

	public abstract IButtonViewAdapter GetFirstButton();

	public virtual void SelectFirstButton()
	{
		IButtonViewAdapter firstButton = GetFirstButton();
		if (!BeamTeam.IsNullOrDestroyed(firstButton))
		{
			firstButton.Select();
		}
	}
}
