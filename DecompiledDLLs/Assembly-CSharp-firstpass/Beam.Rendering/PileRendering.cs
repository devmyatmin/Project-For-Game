using Beam.Crafting;
using UnityEngine;

namespace Beam.Rendering;

public class PileRendering : MonoBehaviour
{
	[SerializeField]
	private ConstructionObject_ITEM_PILE _pile;

	[SerializeField]
	private InstancingData _instancingData;

	[SerializeField]
	private InstancedMeshRenderer _renderer;

	public InstancedMeshRenderer Renderer => _renderer;

	public void Initialize(CraftingType craftingType)
	{
		IInstancedMeshData instancingData = _instancingData.GetInstancingData(craftingType);
		_renderer.SetData(instancingData);
	}

	private void Awake()
	{
		_pile.SlotStorage.Pushed += Storage_Pushed;
		_pile.SlotStorage.Popped += Storage_Popped;
	}

	private void OnDestroy()
	{
		_pile.SlotStorage.Pushed -= Storage_Pushed;
		_pile.SlotStorage.Popped -= Storage_Popped;
	}

	private void Storage_Pushed(IPickupable obj, bool success)
	{
		if (success)
		{
			int storedCount = _pile.SlotStorage.GetStoredCount();
			if (storedCount == 1)
			{
				Initialize(obj.CraftingType);
			}
			_renderer.DrawCount = storedCount;
		}
	}

	private void Storage_Popped(IPickupable obj)
	{
		_renderer.DrawCount = _pile.SlotStorage.GetStoredCount();
	}
}
