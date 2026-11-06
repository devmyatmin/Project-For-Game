using System;

namespace Beam.Crafting;

public class GameAction : IGameAction
{
	public string PlayerActionRequiredDescription { get; set; }

	public int ElementRequiredId { get; set; }

	public string ResultingActionDescription { get; set; }

	public Action<IBase> Action { get; set; }

	public void DoAction(IBase obj)
	{
		Action?.Invoke(obj);
	}
}
