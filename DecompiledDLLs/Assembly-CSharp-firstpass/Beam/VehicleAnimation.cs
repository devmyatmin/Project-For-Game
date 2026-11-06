using UnityEngine;

namespace Beam;

public class VehicleAnimation<TVehicle> : MonoBehaviour where TVehicle : VehicleMovementBase
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private TVehicle _vehicleMovement;

	[SerializeField]
	private AnimationClip _idle;

	[SerializeField]
	private AnimationClip _using;

	protected TVehicle VehicleMovement => _vehicleMovement;

	protected virtual void Start()
	{
		if ((Object)(object)_vehicleMovement != (Object)null)
		{
			_vehicleMovement.Operated += VehicleMovement_Operated;
			_vehicleMovement.Using += VehicleMovement_Using;
		}
		Initialise();
	}

	protected virtual void OnDestroy()
	{
		if ((Object)(object)_vehicleMovement != (Object)null)
		{
			_vehicleMovement.Operated -= VehicleMovement_Operated;
			_vehicleMovement.Using -= VehicleMovement_Using;
		}
	}

	protected virtual void Initialise()
	{
	}

	protected virtual void VehicleMovement_Operated(bool active)
	{
		if (active && (Object)(object)_idle != (Object)null)
		{
			_vehicleMovement.Operator.Character.PlayIdleAnimation(((Object)_idle).name);
		}
	}

	protected virtual void VehicleMovement_Using()
	{
		if ((Object)(object)_using != (Object)null)
		{
			_vehicleMovement.Operator.Character.PlayUseAnimation(((Object)_using).name);
		}
	}
}
