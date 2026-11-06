using System.Collections;
using Beam.Utilities;

namespace Beam;

public static class IInteractableMixins
{
	public static void Interact_(this IInteractable interactable, IPlayer player, IBase interactee = null)
	{
		if (!BeamTeam.IsNullOrDestroyed(interactable))
		{
			CoroutineHelper.StartCoroutine(Interact_Sequence(interactable, player, interactee));
		}
	}

	private static IEnumerator Interact_Sequence(IInteractable interactable, IPlayer player, IBase interactee = null)
	{
		if (BeamTeam.IsNullOrDestroyed(interactee))
		{
			interactable.Interact(player);
		}
		else
		{
			interactable.InteractWithObject(player, interactee);
		}
		yield break;
	}
}
