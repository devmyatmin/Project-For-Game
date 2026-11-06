using Sirenix.OdinInspector;

namespace Beam;

public abstract class PlacementModifier : SerializedScriptableObject
{
	public abstract void ModifyPlacement(ref Placement placement, IPlacementSettings settings);
}
