using Beam.Rendering;
using UnityEngine;

namespace Beam.Crafting;

[CreateAssetMenu(menuName = "Beam Team/Pile Data Asset")]
public class PileData : ScriptableObject
{
	[SerializeField]
	private InstancingData _pileInstancingData;

	public int GetPileCapacity(CraftingType craftingType)
	{
		return _pileInstancingData.GetInstancingData(craftingType).GetMatrices().Length;
	}
}
