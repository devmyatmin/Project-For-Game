namespace Beam.Crafting;

public interface IDragger
{
	bool CanDrag { get; set; }

	IDraggable Draggable { get; }
}
