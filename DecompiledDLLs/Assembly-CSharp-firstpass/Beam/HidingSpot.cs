using UnityEngine;

namespace Beam;

public class HidingSpot : MonoBehaviour
{
	private HidingSpotController _hidingSpotController;

	[SerializeField]
	private LandSpecies _species;

	public LandSpecies Species => _species;

	public LandAnimal HideChild { get; set; }

	public bool Occupied { get; set; }

	public event EmptyEventHandler Hid;

	protected virtual void OnHid()
	{
		Hid?.Invoke();
	}

	private void Start()
	{
		_hidingSpotController = Singleton<HidingSpotController>.Instance;
		if ((Object)(object)_hidingSpotController != (Object)null)
		{
			_hidingSpotController.HidingSpots.Add(this);
		}
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)HideChild))
		{
			HideChild.CurrentState = LandAnimalState.Idle;
		}
		if ((Object)(object)_hidingSpotController != (Object)null)
		{
			_hidingSpotController.HidingSpots.Remove(this);
		}
		Hid = null;
	}

	private void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(((Component)this).transform.position, 0.3f);
	}
}
