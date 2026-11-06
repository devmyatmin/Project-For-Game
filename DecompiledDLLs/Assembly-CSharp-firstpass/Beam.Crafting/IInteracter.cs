using System;

namespace Beam.Crafting;

public interface IInteracter
{
	event Action<IBase> GetInteractionActions;
}
