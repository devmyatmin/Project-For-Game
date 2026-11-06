using System.Collections.Generic;
using Beam.Crafting;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Beam.Rendering;

[CreateAssetMenu(menuName = "Beam Team/Instancing Data Asset")]
public class InstancingData : SerializedScriptableObject
{
	[SerializeField]
	private Dictionary<CraftingType, SerializedInstancedMeshData> _data;

	public IInstancedMeshData GetInstancingData(CraftingType craftingType)
	{
		if (!_data.TryGetValue(craftingType, out var value))
		{
			return null;
		}
		return value;
	}
}
