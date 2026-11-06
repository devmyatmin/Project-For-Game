namespace Beam;

public static class ISelectableMixins
{
	public static void Select(this ISelectable selectable, IPlayer player)
	{
		selectable.Selecting?.Select(player);
	}

	public static void Deselect(this ISelectable selectable)
	{
		selectable.Selecting?.Deselect();
	}
}
