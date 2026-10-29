using Beam;
using Beam.Events;
using Beam.Utilities;
using UnityEngine;

namespace Funlabs;

public class MultiplayerPickupableLocalCorrection : MonoBehaviour
{
	private IPickupable _pickupable;

	private void Start()
	{
		EventManager.AddListener<PlayerDisconnectedEvent>(EventManager_PlayerDisconnected);
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<PlayerDisconnectedEvent>(EventManager_PlayerDisconnected);
	}

	public void Set(IPickupable pickuable)
	{
		_pickupable = pickuable;
	}

	private void CorrectBoltWorldSpaceMovement()
	{
		if (!_pickupable.IsNullOrDestroyed())
		{
			_pickupable.transform.localPosition = _pickupable.LocalHoldingPosition;
			_pickupable.transform.localEulerAngles = _pickupable.LocalHoldingRotation;
		}
	}

	private void OnPreCull()
	{
		CorrectBoltWorldSpaceMovement();
	}

	public void EventManager_PlayerDisconnected(PlayerDisconnectedEvent disconnectedEvent)
	{
		if (disconnectedEvent.PlayerId == 1)
		{
			Set(null);
		}
	}
}
