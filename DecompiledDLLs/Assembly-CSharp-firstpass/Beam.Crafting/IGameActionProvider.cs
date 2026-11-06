using System.Collections.Generic;

namespace Beam.Crafting;

public interface IGameActionProvider
{
	IEnumerable<IGameAction> GetGameActions();
}
