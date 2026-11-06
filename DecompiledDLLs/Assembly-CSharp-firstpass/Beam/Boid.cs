using System.Collections.Generic;
using UnityEngine;

namespace Beam;

public class Boid
{
	private Vector3 _position;

	private Vector3 _velocity;

	private Vector3 _acceleration;

	private readonly float _maxSpeed;

	private readonly float _maxSteer;

	private readonly float _avoidanceWeight;

	private readonly float _allignmentWeight;

	private readonly float _cohesionWeight;

	private readonly float _boundsWeight;

	private readonly float _sqrAvoidanceDistance;

	private readonly float _sqrAllignmentDistance;

	private readonly float _sqrCohesionDistance;

	private const float _minDistance = 0.02f;

	private const float _sqrMinDistance = 0.0004f;

	private readonly float _sqrBoundsSize;

	private BoundsType _boundsType;

	public Vector3 Vel;

	public Vector3 Position
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _position;
		}
	}

	public Vector3 Velocity
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _velocity;
		}
	}

	public Boid(Vector3 position, Vector3 velocity, float maxSpeed, float maxSteer, float avoidanceDistance, float allignmentDistance, float cohesionDistance, float avoidanceWeight, float allignmentWeight, float cohesionWeight, float boundsWeight, float boundsSize, BoundsType boundsType)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		_position = position;
		_velocity = velocity;
		_maxSpeed = maxSpeed;
		_maxSteer = maxSteer;
		_avoidanceWeight = avoidanceWeight;
		_allignmentWeight = allignmentWeight;
		_cohesionWeight = cohesionWeight;
		_boundsWeight = boundsWeight;
		_sqrAvoidanceDistance = avoidanceDistance * avoidanceDistance;
		_sqrAllignmentDistance = allignmentDistance * allignmentDistance;
		_sqrCohesionDistance = cohesionDistance * cohesionDistance;
		_sqrBoundsSize = boundsSize * boundsSize;
		_boundsType = boundsType;
		if (_boundsType == BoundsType.Circle)
		{
			_position.y = 0f;
			_velocity.y = 0f;
		}
	}

	public Boid(Vector3 position, Vector3 velocity, FlockSettings settings)
		: this(position, velocity, settings.MaxSpeed, settings.MaxSteer, settings.AvoidanceDistance, settings.AllignmentDistance, settings.CohesionDistance, settings.AvoidanceWeight, settings.AllignmentWeight, settings.CohesionWeight, settings.BoundsWeight, settings.BoundsRadius, settings.BoundsType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
	}

	public void Reinitialize(Vector3 position, Vector3 velocity)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		_position = position;
		_velocity = velocity;
		if (_boundsType == BoundsType.Circle)
		{
			_position.y = 0f;
			_velocity.y = 0f;
		}
	}

	public void DoUpdate(float dt, float step = 1f)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (_boundsType == BoundsType.Circle)
		{
			_acceleration.y = 0f;
		}
		Vel = _velocity;
		_velocity += _acceleration * step;
		_velocity = Vector3.ClampMagnitude(_velocity, _maxSpeed);
		_position += _velocity * dt;
	}

	public void DoFlock(IList<Boid> boids)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		_acceleration = Vector3.zero;
		ApplyForce(Interact(boids));
	}

	public void DoBounds(Vector3 target)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (_boundsWeight != 0f)
		{
			Vector3 val = target - Position;
			if (val.sqrMagnitude > _sqrBoundsSize)
			{
				Vector3 val2 = val;
				val2.Normalize();
				val2 *= _maxSpeed;
				val2 -= _velocity;
				val2 = Vector3.ClampMagnitude(val2, _maxSteer) * _boundsWeight;
				ApplyForce(val2);
			}
		}
	}

	public void DoRepel(IList<IAttracterRepeller> repellers)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if (repellers.Count == 0)
		{
			return;
		}
		Vector3 val = Vector3.zero;
		int num = 0;
		for (int i = 0; i < repellers.Count; i++)
		{
			IAttracterRepeller attracterRepeller = repellers[i];
			Vector3 val2 = Position - attracterRepeller.Position;
			float sqrMagnitude = val2.sqrMagnitude;
			if (sqrMagnitude < attracterRepeller.Radius * attracterRepeller.Radius)
			{
				val += val2.normalized / Mathf.Sqrt(sqrMagnitude);
				num++;
			}
		}
		if (num > 0)
		{
			val.Normalize();
			val *= _maxSpeed;
			ApplyForce(val);
		}
	}

	private void ApplyForce(Vector3 force)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		_acceleration += force;
	}

	private Vector3 Interact(IList<Boid> boids)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		Vector3 val2 = Vector3.zero;
		Vector3 val3 = Vector3.zero;
		Vector3 val4 = Vector3.zero;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < boids.Count; i++)
		{
			Boid boid = boids[i];
			Vector3 val5 = Position - boid.Position;
			float sqrMagnitude = val5.sqrMagnitude;
			if (sqrMagnitude > 0.0004f && sqrMagnitude < _sqrCohesionDistance)
			{
				val3 += boid.Position;
				num3++;
				if (sqrMagnitude < _sqrAvoidanceDistance)
				{
					val += val5.normalized / Mathf.Sqrt(sqrMagnitude);
					num++;
				}
				if (sqrMagnitude < _sqrAllignmentDistance)
				{
					val2 += boid.Velocity;
					num2++;
				}
			}
		}
		if (num > 0)
		{
			val.Normalize();
			val *= _maxSpeed;
			val -= _velocity;
			val = Vector3.ClampMagnitude(val, _maxSteer);
			val *= _avoidanceWeight;
		}
		if (num2 > 0)
		{
			val2.Normalize();
			val2 *= _maxSpeed;
			val2 -= _velocity;
			val2 = Vector3.ClampMagnitude(val2, _maxSteer);
			val2 *= _allignmentWeight;
		}
		if (num3 > 0)
		{
			val3 /= (float)num3;
			val4 = Seek(val3);
			val4 *= _cohesionWeight;
		}
		return val + val2 + val4;
	}

	private Vector3 Avoid(IList<Boid> boids)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		int num = 0;
		for (int i = 0; i < boids.Count; i++)
		{
			Boid boid = boids[i];
			Vector3 val2 = Position - boid.Position;
			float sqrMagnitude = val2.sqrMagnitude;
			if (sqrMagnitude > 0.0004f && sqrMagnitude < _sqrAvoidanceDistance)
			{
				val += val2.normalized / Mathf.Sqrt(sqrMagnitude);
				num++;
			}
		}
		if (num > 0)
		{
			val.Normalize();
			val *= _maxSpeed;
			val -= _velocity;
			return Vector3.ClampMagnitude(val, _maxSteer) * _avoidanceWeight;
		}
		return Vector3.zero;
	}

	private Vector3 Allign(IList<Boid> boids)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		int num = 0;
		for (int i = 0; i < boids.Count; i++)
		{
			Boid boid = boids[i];
			Vector3 val2 = Position - boid.Position;
			float sqrMagnitude = val2.sqrMagnitude;
			if (sqrMagnitude > 0.0004f && sqrMagnitude < _sqrAllignmentDistance)
			{
				val += boid._velocity;
				num++;
			}
		}
		if (num > 0)
		{
			val.Normalize();
			val *= _maxSpeed;
			val -= _velocity;
			return Vector3.ClampMagnitude(val, _maxSteer) * _allignmentWeight;
		}
		return Vector3.zero;
	}

	private Vector3 Cohesion(IList<Boid> boids)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		int num = 0;
		for (int i = 0; i < boids.Count; i++)
		{
			Boid boid = boids[i];
			Vector3 val2 = Position - boid.Position;
			float sqrMagnitude = val2.sqrMagnitude;
			if (sqrMagnitude > 0.0004f && sqrMagnitude < _sqrCohesionDistance)
			{
				val += boid.Position;
				num++;
			}
		}
		if (num > 0)
		{
			val /= (float)num;
			return Seek(val) * _cohesionWeight;
		}
		return Vector3.zero;
	}

	private Vector3 Seek(Vector3 target)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = target - Position;
		float sqrMagnitude = val.sqrMagnitude;
		if (sqrMagnitude > 0f)
		{
			val.Normalize();
			val = ((!(sqrMagnitude < _sqrAvoidanceDistance)) ? (val * _maxSpeed) : (val * (_maxSpeed * (sqrMagnitude / _sqrAvoidanceDistance))));
			return Vector3.ClampMagnitude(val - _velocity, _maxSteer);
		}
		return Vector3.zero;
	}
}
