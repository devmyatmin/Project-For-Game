namespace Beam.Crafting;

public class NullDragger : IDragger
{
	public bool CanDrag { get; set; }

	public IDraggable Draggable => null;
}
