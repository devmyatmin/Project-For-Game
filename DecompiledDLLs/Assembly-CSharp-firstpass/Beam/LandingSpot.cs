using UnityEngine;

namespace Beam;

public class LandingSpot : MonoBehaviour
{
	private LandingSpotController _landingSpotController;

	[SerializeField]
	private FlockSpecies _species;

	public FlockSpecies Species => _species;

	public FlockChild FlockChild { get; set; }

	public bool Occupied { get; set; }

	public event EmptyEventHandler Landed;

	protected virtual void OnLanded()
	{
		Landed?.Invoke();
	}

	public void Land()
	{
		OnLanded();
	}

	private void Start()
	{
		_landingSpotController = Singleton<LandingSpotController>.Instance;
		if ((Object)(object)_landingSpotController != (Object)null)
		{
			_landingSpotController.AddLandingSpot(this);
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)FlockChild != (Object)null)
		{
			FlockChild.CurrentState = FlockChildState.TakingOff;
			FlockChild = null;
		}
		if ((Object)(object)_landingSpotController != (Object)null)
		{
			_landingSpotController.RemoveLandingSpot(this);
		}
		Landed = null;
	}

	private void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(((Component)this).transform.position, 0.3f);
	}
}
