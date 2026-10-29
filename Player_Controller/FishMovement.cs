using System.Collections.Generic;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

[RequireComponent(typeof(FishBase))]
public class FishMovement : MonoBehaviour, IFixedTick, ITick
{
	private const int MAX_WAYPOINT_ATTEMPTS = 12;

	private const float MIN_WAYPOINT_LENGTH = 1f;

	private const float MIN_WAYPOINT_DISTANCE = 0.02f;

	private const float AVOID_SPEED = 0.2f;

	private const float AVOID_STEER = 8f;

	[SerializeField]
	private FishBase _fish;

	[SerializeField]
	private float _minWorldHeight;

	[SerializeField]
	private float _maxWorldHeight;

	[SerializeField]
	private float _minSpeed = 1f;

	[SerializeField]
	private float _maxSpeed = 2f;

	[SerializeField]
	private float _minSteer = 1f;

	[SerializeField]
	private float _maxSteer = 2f;

	[SerializeField]
	private float _waypointDistance = 3f;

	[SerializeField]
	private float _waypointForwardBias = 0.5f;

	[SerializeField]
	private LayerMask _avoidanceMask;

	[SerializeField]
	private float _avoidanceRayLength = 0.7f;

	[SerializeField]
	private float _positionSmoothing = 10f;

	[SerializeField]
	private float _rotationSmoothing = 7f;

	private float _speed;

	private float _steer;

	private float _stuck;

	private float _originalSpeed;

	private float _originalSteer;

	private Vector3 _waypoint;

	private Vector3 _boundsCenter;

	private float _boundsRadius;

	private bool _hooked;

	private bool _validWaypoint;

	private TransformProxy _transformProxy;

	private bool _visible;

	private const int AVOIDANCE_FRAMES_RANGE = 3;

	private static int _avoidanceGroups = 0;

	private int _doAvoidanceFrame = -1;

	private bool _hitForward;

	private bool _hitLeft;

	private bool _hitRight;

	private bool _hitDown;

	private List<Vector3> _waypointsCache;

	private int _tickCount;

	private bool _teleportFlag;

	private static int MAX_WAYPOINTS_CACHE_COUNT = 10;

	private static int MAX_TICKS_TO_TELEPORT = 60;

	private static int MAX_TELEPORT_ATTEMPTS = 4;

	[SerializeField]
	private UnityEvent _waypointFailed;

	FixedTickRate IFixedTick.Rate => FixedTickRate.Quarter;

	public bool Active => base.isActiveAndEnabled;

	private float DeltaTime => FixedTime.TimeStep(this);

	private void Awake()
	{
		_transformProxy = new TransformProxy(base.transform);
		_waypointsCache = new List<Vector3>();
		float value = Random.value;
		_speed = Mathf.Lerp(_minSpeed, _maxSpeed, value);
		_steer = Mathf.Lerp(_minSteer, _maxSteer, value);
		_originalSpeed = _speed;
		_originalSteer = _steer;
		_fish.Subscribed += Fish_Subscribed;
		_fish.LodChanged += Fish_LodChanged;
		_fish.Hooked += Fish_Hooked;
	}

	private void OnEnable()
	{
		_avoidanceGroups++;
		_doAvoidanceFrame = _avoidanceGroups % 3;
		Ticker.Instance?.Add(this);
		FixedTicker.Instance?.Add(this);
	}

	private void OnDisable()
	{
		_avoidanceGroups--;
		_doAvoidanceFrame = -1;
		Ticker.Instance?.Remove(this);
		FixedTicker.Instance?.Remove(this);
	}

	private void OnDestroy()
	{
		_fish.Subscribed -= Fish_Subscribed;
		_fish.LodChanged -= Fish_LodChanged;
		_fish.Hooked -= Fish_Hooked;
	}

	public void Wake()
	{
		_waypointsCache.Clear();
		_tickCount = 0;
		_teleportFlag = false;
		base.enabled = true;
	}

	public void Sleep()
	{
		base.enabled = false;
	}

	void ITick.Tick()
	{
		_visible = _fish.FishRenderer.IsVisible();
	}

	void IFixedTick.FixedTick()
	{
		if (!LevelLoader.IsLoading(base.gameObject) && !_hooked && _visible)
		{
			_tickCount++;
			DoMovement();
			DoAvoidance();
			CheckIfReachedWaypoint();
		}
	}

	private void Fish_Subscribed(ICell cell)
	{
		_transformProxy.position = base.transform.position;
		_transformProxy.rotation = base.transform.rotation;
		_speed = _originalSpeed;
		_steer = _originalSteer;
		_boundsCenter = cell.Position;
		_boundsRadius = cell.Size.x / 2f;
		Wake();
		GetValidWaypoint();
	}

	private void Fish_LodChanged(int lod)
	{
	}

	private void Fish_Hooked(bool hooked)
	{
		_hooked = hooked;
	}

	private void DoMovement()
	{
		Vector3 normalized = (_waypoint - _transformProxy.position).normalized;
		if (normalized != Vector3.zero)
		{
			Quaternion b = Quaternion.LookRotation(normalized);
			_transformProxy.rotation = Quaternion.Lerp(_transformProxy.rotation, b, DeltaTime * _steer);
		}
		_transformProxy.position += _transformProxy.forward * _speed * DeltaTime;
		base.transform.position = Vector3.Lerp(base.transform.position, _transformProxy.position, DeltaTime * _positionSmoothing);
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, _transformProxy.rotation, DeltaTime * _rotationSmoothing);
	}

	private void DoAvoidance()
	{
		if (_hitForward || _hitLeft || _hitRight || _hitDown || Time.frameCount % 3 == _doAvoidanceFrame)
		{
			bool flag = _transformProxy.position.y < -0.4f;
			Vector3 eulerAngles = _transformProxy.rotation.eulerAngles;
			Quaternion rotation = _transformProxy.rotation;
			float avoidanceRayLength = _avoidanceRayLength;
			_hitLeft = Physics.Raycast(_transformProxy.position, (_transformProxy.forward - _transformProxy.right).normalized, avoidanceRayLength, _avoidanceMask);
			_hitRight = Physics.Raycast(_transformProxy.position, (_transformProxy.forward + _transformProxy.right).normalized, avoidanceRayLength, _avoidanceMask);
			_hitForward = _hitLeft && _hitRight && Physics.Raycast(_transformProxy.position, _transformProxy.forward, avoidanceRayLength, _avoidanceMask);
			_hitDown = flag && Physics.Raycast(_transformProxy.position, (_transformProxy.forward - _transformProxy.up * _fish.GetBounds().size.y).normalized, avoidanceRayLength, _avoidanceMask);
			if (_hitForward)
			{
				eulerAngles.y -= 2800f * FixedTime.TimeStep(this) * _steer;
				rotation.eulerAngles = eulerAngles;
				_transformProxy.rotation = rotation;
			}
			else if (_hitRight)
			{
				eulerAngles.y -= 400f * FixedTime.TimeStep(this) * _steer;
				rotation.eulerAngles = eulerAngles;
				_transformProxy.rotation = rotation;
			}
			else if (_hitLeft)
			{
				eulerAngles.y += 400f * FixedTime.TimeStep(this) * _steer;
				rotation.eulerAngles = eulerAngles;
				_transformProxy.rotation = rotation;
			}
			if (_hitDown && flag)
			{
				eulerAngles.x -= 100f * FixedTime.TimeStep(this) * _steer;
				rotation.eulerAngles = eulerAngles;
				_transformProxy.rotation = rotation;
				Vector3 position = _transformProxy.position;
				position.y += 100f * FixedTime.TimeStep(this) * 0.01f;
				_transformProxy.position = position;
			}
		}
	}

	private void CheckIfReachedWaypoint()
	{
		if ((_transformProxy.position - _waypoint).sqrMagnitude < BeamMath.Sqr(0.02f + _stuck))
		{
			_stuck = 0f;
			GetValidWaypoint();
		}
		else
		{
			_stuck += DeltaTime;
		}
	}

	private void AdjustSpeedAndTurningSpeed()
	{
		float t = Mathf.Clamp01((_transformProxy.position - _waypoint).sqrMagnitude / BeamMath.Sqr(_waypointDistance));
		_speed = Mathf.Lerp(0.2f, _originalSpeed, t);
		_steer = Mathf.Lerp(8f, _originalSteer, t);
	}

	public void HandleInitialStuck()
	{
		if (_teleportFlag && _waypointsCache.Count <= 0)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			if (_waypointsCache.Count > 0)
			{
				return;
			}
			Vector3 vector = base.transform.position;
			for (int i = 0; i < MAX_TELEPORT_ATTEMPTS; i++)
			{
				vector = new Vector3(Random.Range(_boundsCenter.x - _boundsRadius, _boundsCenter.x + _boundsRadius), Random.Range(_minWorldHeight + _fish.GetBounds().extents.y, _maxWorldHeight - _fish.GetBounds().extents.y), Random.Range(_boundsCenter.z - _boundsRadius, _boundsCenter.z + _boundsRadius));
				if (Physics.Linecast(new Vector3(vector.x, 10f, vector.z), vector, out var hitInfo, _avoidanceMask))
				{
					vector.y = hitInfo.point.y + _fish.GetBounds().extents.y;
					if (vector.y > _minWorldHeight && vector.y < _maxWorldHeight)
					{
						_teleportFlag = true;
						break;
					}
					continue;
				}
				_teleportFlag = true;
				break;
			}
			if (_teleportFlag)
			{
				base.transform.position = vector;
				_transformProxy.position = vector;
				GenerateWaypoint(11);
				AdjustSpeedAndTurningSpeed();
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	public void DoBacktracking()
	{
		if (_waypointsCache.Count <= 0)
		{
			GenerateWaypoint(11);
			AdjustSpeedAndTurningSpeed();
		}
		else
		{
			_waypoint = _waypointsCache[_waypointsCache.Count - 1];
			_waypointsCache.RemoveAt(_waypointsCache.Count - 1);
			_validWaypoint = true;
		}
	}

	private void GetValidWaypoint()
	{
		for (int i = 0; i < 12; i++)
		{
			GenerateWaypoint(i);
			_validWaypoint = CheckWaypoint(ref _waypoint);
			if (_validWaypoint)
			{
				AdjustSpeedAndTurningSpeed();
				if (_waypointsCache.Count > MAX_WAYPOINTS_CACHE_COUNT)
				{
					_waypointsCache.RemoveAt(0);
				}
				_waypointsCache.Add(new Vector3(_waypoint.x, _waypoint.y, _waypoint.z));
				return;
			}
		}
		if (_tickCount < MAX_TICKS_TO_TELEPORT)
		{
			HandleInitialStuck();
		}
		else
		{
			DoBacktracking();
		}
	}

	private void GenerateWaypoint(int attempts = 0)
	{
		float num = Mathf.Clamp01((float)(attempts * 2) / 12f);
		Vector3 vector = _boundsCenter - _transformProxy.position;
		vector.y = 0f;
		float num2 = vector.sqrMagnitude / BeamMath.Sqr(_boundsRadius);
		num2 *= num2;
		num2 *= 1f - num;
		float num3 = Mathf.Lerp(1f, -1f, num);
		Vector3 a = _transformProxy.forward * num3;
		Vector3 onUnitSphere = Random.onUnitSphere;
		onUnitSphere.z = Mathf.Abs(onUnitSphere.z) * num3;
		onUnitSphere.Normalize();
		Vector3 vector2 = Vector3.Lerp(t: Mathf.Lerp(_waypointForwardBias, 1f, num), a: a, b: _transformProxy.rotation * onUnitSphere);
		vector2 += vector.normalized * num2;
		vector2.Normalize();
		float num4 = Mathf.Lerp(_waypointDistance, 1f, num);
		vector2 *= num4;
		_waypoint = _transformProxy.position + vector2;
		ClampHeightToRange(ref _waypoint);
	}

	private void ClampHeightToRange(ref Vector3 point)
	{
		point.y = Mathf.Clamp(point.y, _minWorldHeight, _maxWorldHeight);
	}

	private bool CheckWaypoint(ref Vector3 position)
	{
		if (Physics.Linecast(new Vector3(position.x, 10f, position.z), position, out var hitInfo, _avoidanceMask))
		{
			position.y = hitInfo.point.y + _fish.GetBounds().extents.y;
			if (position.y > _minWorldHeight)
			{
				return position.y < _maxWorldHeight;
			}
			return false;
		}
		return true;
	}
}
