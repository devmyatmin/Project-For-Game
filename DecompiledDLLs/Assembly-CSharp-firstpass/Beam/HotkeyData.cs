using Beam.Crafting;
using Beam.UI;
using Funlabs;

namespace Beam;

public class HotkeyData
{
	public int Number;

	public MiniGuid ReferenceId;

	public Property<bool> Locked = new Property<bool>(value: true);

	public Property<CraftingType> CraftingType = new Property<CraftingType>(Beam.Crafting.CraftingType.Empty);
}
