using Rewired;

namespace Beam.UI;

public interface IMenuPanel
{
	bool Active { get; }

	Player Input { get; }

	void SetInput(Player input);

	IButtonViewAdapter GetFirstButton();

	void SelectFirstButton();
}
