using System;
using Rewired;

namespace Beam.UI;

public abstract class RadialMenuInput
{
	protected readonly Player Input;

	public event Action OpenKeyUp;

	protected virtual void OnOpenKeyUp()
	{
		OpenKeyUp?.Invoke();
	}

	protected RadialMenuInput(Player input)
	{
		Input = input;
	}

	public abstract void CheckInput();
}
