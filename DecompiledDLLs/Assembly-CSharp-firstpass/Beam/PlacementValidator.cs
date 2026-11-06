using Sirenix.OdinInspector;

namespace Beam;

public abstract class PlacementValidator : SerializedScriptableObject
{
	public abstract bool ValidatePlacement(Placement placement, IPlacementSettings settings);
}
