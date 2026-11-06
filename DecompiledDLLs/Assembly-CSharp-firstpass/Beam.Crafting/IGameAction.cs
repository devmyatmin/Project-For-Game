namespace Beam.Crafting;

public interface IGameAction
{
	string PlayerActionRequiredDescription { get; }

	int ElementRequiredId { get; }

	string ResultingActionDescription { get; }

	void DoAction(IBase obj);
}
