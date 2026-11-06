using UnityEngine;

namespace Beam;

public struct BeamRay
{
	private Ray _ray;

	private float _length;

	public Ray BaseRay
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _ray;
		}
	}

	public Vector3 OriginPoint
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _ray.origin;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			_ray.origin = value;
		}
	}

	public Vector3 Direction
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _ray.direction;
		}
	}

	public Vector3 EndPoint
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			return _ray.origin + _ray.direction * _length;
		}
	}

	public float Length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
		}
	}

	public BeamRay(Vector3 origin, Vector3 direction, float length)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		_ray = new Ray(origin, direction);
		_length = length;
	}

	public BeamRay(Ray ray, float length)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_ray = ray;
		_length = length;
	}

	public Vector3 GetPoint(float distance)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return _ray.origin + _ray.direction * distance;
	}
}
