using Beam.Rendering;
using UnityEngine;

namespace Beam.Crafting;

[RequireComponent(typeof(BoxCollider))]
public class PileCollider : MonoBehaviour
{
	[SerializeField]
	private ConstructionObject_ITEM_PILE _pile;

	[SerializeField]
	private BoxCollider _collider;

	[SerializeField]
	private PileRendering _rendering;

	private void Awake()
	{
		_pile.SlotStorage.Pushed += SlotStorage_Pushed;
		_pile.SlotStorage.Popped += SlotStorage_Popped;
		_pile.SlotStorage.Closed += SlotStorage_Closed;
	}

	private void OnDestroy()
	{
		if (!((Object)(object)_pile == (Object)null))
		{
			_pile.SlotStorage.Pushed -= SlotStorage_Pushed;
			_pile.SlotStorage.Popped -= SlotStorage_Popped;
			_pile.SlotStorage.Closed -= SlotStorage_Closed;
		}
	}

	private void SlotStorage_Pushed(IPickupable obj, bool success)
	{
		if (success && !_pile.SlotStorage.IsOpened)
		{
			RecalculateColliderBounds();
		}
	}

	private void SlotStorage_Popped(IPickupable obj)
	{
		if (!_pile.SlotStorage.IsOpened)
		{
			RecalculateColliderBounds();
		}
	}

	private void SlotStorage_Closed()
	{
		RecalculateColliderBounds();
	}

	private void RecalculateColliderBounds()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = _rendering.Renderer.GetBounds(_pile.SlotStorage.GetStoredCount());
		_collider.size = bounds.size;
		_collider.center = bounds.center;
	}
}
