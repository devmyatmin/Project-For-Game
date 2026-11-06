namespace Beam.Crafting;

public interface IGameInputController
{
	Raycaster Raycaster { get; }

	Interacter Interacter { get; }

	IGameInputHandler CurrentHandler { get; set; }
}
