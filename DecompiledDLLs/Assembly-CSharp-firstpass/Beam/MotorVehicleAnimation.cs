using UnityEngine;

namespace Beam;

public class MotorVehicleAnimation : VehicleAnimation<MotorVehicleMovement>
{
	[SerializeField]
	private Transform _rudder;

	[SerializeField]
	private Transform _propeller;

	[SerializeField]
	private ParticleSystem _engineSmoke;

	[SerializeField]
	private ParticleSystem _waterWake;

	private MotorVehicleMovement _motorVehicleMovement;

	protected override void Start()
	{
		_motorVehicleMovement = VehicleMovement;
		if ((Object)(object)_motorVehicleMovement != (Object)null)
		{
			_motorVehicleMovement.FuelRanOut += MotorVehicleMovement_FuelRanOut;
		}
		base.Start();
	}

	protected override void OnDestroy()
	{
		if ((Object)(object)_motorVehicleMovement != (Object)null)
		{
			_motorVehicleMovement.FuelRanOut -= MotorVehicleMovement_FuelRanOut;
		}
		base.OnDestroy();
	}

	protected override void VehicleMovement_Operated(bool active)
	{
		base.VehicleMovement_Operated(active);
		VehicleMovement.Operator.Character.SetInteger("Vehicle_Pose", active ? 4 : 0);
		if (!(_motorVehicleMovement.Fuel <= 0f))
		{
			if (active)
			{
				PlayParticles();
			}
			else
			{
				StopParticles();
			}
		}
	}

	protected override void VehicleMovement_Using()
	{
		if (!((Object)(object)_motorVehicleMovement == (Object)null) && _motorVehicleMovement.EngineIsRunning())
		{
			AnimateEngine();
			AnimatePropeller();
			UpdateParticles();
		}
	}

	protected virtual void MotorVehicleMovement_FuelRanOut()
	{
		StopParticles();
	}

	private void AnimateEngine()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_motorVehicleMovement == (Object)null))
		{
			float num = Mathf.Sin(Time.time * 25f * (2f - VehicleMovement.Throttle));
			Vector3 localEulerAngles = _rudder.localEulerAngles;
			localEulerAngles.y += num;
			_rudder.localEulerAngles = localEulerAngles;
		}
	}

	private void AnimatePropeller()
	{
		if ((Object)(object)_propeller != (Object)null)
		{
			float num = 2000f * VehicleMovement.Throttle * Time.deltaTime;
			((Component)_propeller).transform.Rotate(0f, 0f, num);
		}
	}

	private void PlayParticles()
	{
		ParticleSystem engineSmoke = _engineSmoke;
		if (engineSmoke != null)
		{
			engineSmoke.Play();
		}
		ParticleSystem waterWake = _waterWake;
		if (waterWake != null)
		{
			waterWake.Play();
		}
	}

	private void StopParticles()
	{
		ParticleSystem engineSmoke = _engineSmoke;
		if (engineSmoke != null)
		{
			engineSmoke.Stop();
		}
		ParticleSystem waterWake = _waterWake;
		if (waterWake != null)
		{
			waterWake.Stop();
		}
	}

	private void UpdateParticles()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_engineSmoke != (Object)null)
		{
			EmissionModule emission = _engineSmoke.emission;
			emission.rateOverTime = MinMaxCurve.op_Implicit(Mathf.Clamp(20f * VehicleMovement.Throttle, 5f, 20f));
		}
		if ((Object)(object)_waterWake != (Object)null)
		{
			EmissionModule emission2 = _waterWake.emission;
			emission2.rateOverDistance = MinMaxCurve.op_Implicit(Mathf.Clamp(10f * VehicleMovement.Throttle, 0f, 10f));
		}
	}
}
