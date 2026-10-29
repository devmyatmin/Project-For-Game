using Beam.Crafting;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Beam;

public class SlotStorageAudio : SerializedMonoBehaviour
{
	[SerializeField]
	private ISlotStorageProvider<IPickupable> _slotStorageProvider;

	[SerializeField]
	private AudioClip _openedClip;

	[SerializeField]
	private AudioClip _closedClip;

	[SerializeField]
	private AudioClip _storedClip;

	[SerializeField]
	private AudioClip _poppedClip;

	[SerializeField]
	private AudioClip _fullClip;

	private ISlotStorage<IPickupable> _slotStorage;

	private void Awake()
	{
		_slotStorage = _slotStorageProvider.GetSlotStorage();
		if (_slotStorage != null)
		{
			_slotStorage.Opened += SlotStorage_Opened;
			_slotStorage.Closed += SlotStorage_Closed;
			_slotStorage.Pushed += SlotStorage_Pushed;
			_slotStorage.Popped += SlotStorage_Popped;
		}
	}

	private void OnDestroy()
	{
		if (_slotStorage != null)
		{
			_slotStorage.Opened -= SlotStorage_Opened;
			_slotStorage.Closed -= SlotStorage_Closed;
			_slotStorage.Pushed -= SlotStorage_Pushed;
			_slotStorage.Popped -= SlotStorage_Popped;
		}
	}

	private void SlotStorage_Opened()
	{
		if (!LevelLoader.IsLoading())
		{
			AudioManager.GetAudioPlayer().Play3D(_openedClip, base.transform.position, AudioMixerChannels.FX, AudioRollOffDistance.VeryNear);
		}
	}

	private void SlotStorage_Closed()
	{
		if (!LevelLoader.IsLoading())
		{
			AudioManager.GetAudioPlayer().Play3D(_closedClip, base.transform.position, AudioMixerChannels.FX, AudioRollOffDistance.VeryNear);
		}
	}

	private void SlotStorage_Pushed(IPickupable pickupable, bool success)
	{
		if (!LevelLoader.IsLoading())
		{
			AudioManager.GetAudioPlayer().Play3D(success ? _storedClip : _fullClip, base.transform.position, AudioMixerChannels.FX, AudioRollOffDistance.VeryNear);
		}
	}

	private void SlotStorage_Popped(IPickupable pickupable)
	{
		if (!LevelLoader.IsLoading())
		{
			AudioManager.GetAudioPlayer().Play3D(_poppedClip, base.transform.position, AudioMixerChannels.FX, AudioRollOffDistance.VeryNear);
		}
	}
}
