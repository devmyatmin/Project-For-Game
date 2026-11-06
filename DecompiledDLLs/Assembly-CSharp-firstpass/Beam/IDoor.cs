namespace Beam;

public interface IDoor : IBase, IHasCraftingType
{
	Door[] Doors { get; }

	bool IsOpened { get; set; }

	bool IsOpening { get; set; }
}
