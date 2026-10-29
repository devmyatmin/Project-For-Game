namespace Beam;

public interface IInteractable : IBase, IHasCraftingType
{
	bool CanInteract { get; set; }

	bool Interact(IPlayer player);

	bool InteractWithObject(IPlayer player, IBase obj);

	string GetInteractionDescription(int playerId, IBase obj);
}
