namespace Beam.Crafting;

public interface IGameInputHandler
{
	void Initialize(IGameInputController controller);

	void CheckInput();
}
