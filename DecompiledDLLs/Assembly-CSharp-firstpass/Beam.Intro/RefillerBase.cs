using UnityEngine;

namespace Beam.Intro;

public abstract class RefillerBase : ScriptableObject
{
	public abstract string GetInteractionDescription(IHaveFuel fuelable, IBase obj);

	public abstract bool DoRefill(RefillArgs args);
}
