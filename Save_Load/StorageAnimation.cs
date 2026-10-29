using Beam.Crafting;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Beam;

public class StorageAnimation : SerializedMonoBehaviour
{
	[SerializeField]
	private ISlotStorageProvider<IPickupable> _slotStorageProvider;

	[SerializeField]
	private Animator _animator;

	private ISlotStorage<IPickupable> _slotStorage;

	private void Awake()
	{
		_slotStorage = _slotStorageProvider.GetSlotStorage();
		if (_slotStorage != null)
		{
			_slotStorage.Opened += SlotStorage_Opened;
			_slotStorage.Closed += SlotStorage_Closed;
		}
	}

	private void OnDestroy()
	{
		if (_slotStorage != null)
		{
			_slotStorage.Opened -= SlotStorage_Opened;
			_slotStorage.Closed -= SlotStorage_Closed;
		}
	}

	private void SlotStorage_Opened()
	{
		TriggerAnimation("Open");
	}

	private void SlotStorage_Closed()
	{
		TriggerAnimation("Close");
	}

	private void TriggerAnimation(string triggername)
	{
		if (!string.IsNullOrEmpty(triggername))
		{
			_animator.ResetTrigger("Open");
			_animator.ResetTrigger("Close");
			_animator.SetTrigger(triggername);
		}
	}
}
