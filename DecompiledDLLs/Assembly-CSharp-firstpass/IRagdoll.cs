using Beam;

public interface IRagdoll : IBase, IHasCraftingType, ISaveablePrefab, ISaveableReference, ISaveable
{
	RagdollChild[] RagdollChildren { get; }
}
