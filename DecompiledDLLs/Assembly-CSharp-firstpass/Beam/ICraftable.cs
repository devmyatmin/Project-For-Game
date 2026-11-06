using UnityEngine;

namespace Beam;

public interface ICraftable : IBase, IHasCraftingType, IPlaceable
{
	int CreatorId { get; set; }

	GhostCollider.ColliderType ColliderType { get; }

	bool CraftImmediate { get; }

	Mesh GhostMesh { get; }

	Material[] GhostMaterials { get; }

	void OnBeginCrafting();

	bool CheckProximity(Transform ghost);

	void OnCrafted(IPlayer player);
}
