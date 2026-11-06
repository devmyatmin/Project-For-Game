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

	public bool Active => ((Behaviour)this).isActiveAndEnabled;

	private float DeltaTime => FixedTime.TimeStep(this);

	private void Awake()
	{
		_transformProxy = new TransformProxy(((Component)this).transform);
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
		((Behaviour)this).enabled = true;
	}

	public void Sleep()
	{
		((Behaviour)this).enabled = false;
	}

	void ITick.Tick()
	{
		_visible = _fish.FishRenderer.IsVisible();
	}

	void IFixedTick.FixedTick()
	{
		if (!LevelLoader.IsLoading(((Component)this).gameObject) && !_hooked && _visible)
		{
			_tickCount++;
			DoMovement();
			DoAvoidance();
			CheckIfReachedWaypoint();
		}
	}

	private void Fish_Subscribed(ICell cell)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		_transformProxy.position = ((Component)this).transform.position;
		_transformProxy.rotation = ((Component)this).transform.rotation;
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
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = _waypoint - _transformProxy.position;
		Vector3 normalized = val.normalized;
		if (normalized != Vector3.zero)
		{
			Quaternion val2 = Quaternion.LookRotation(normalized);
			_transformProxy.rotation = Quaternion.Lerp(_transformProxy.rotation, val2, DeltaTime * _steer);
		}
		TransformProxy transformProxy = _transformProxy;
		transformProxy.position += _transformProxy.forward * _speed * DeltaTime;
		((Component)this).transform.position = Vector3.Lerp(((Component)this).transform.position, _transformProxy.position, DeltaTime * _positionSmoothing);
		((Component)this).transform.rotation = Quaternion.Lerp(((Component)this).transform.rotation, _transformProxy.rotation, DeltaTime * _rotationSmoothing);
	}

	private void DoAvoidance()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		if (_hitForward || _hitLeft || _hitRight || _hitDown || Time.frameCount % 3 == _doAvoidanceFrame)
		{
			bool flag = _transformProxy.position.y < -0.4f;
			Vector3 eulerAngles = _transformProxy.rotation.eulerAngles;
			Quaternion rotation = _transformProxy.rotation;
			float avoidanceRayLength = _avoidanceRayLength;
			Vector3 position = _transformProxy.position;
			Vector3 val = _transformProxy.forward - _transformProxy.right;
			_hitLeft = Physics.Raycast(position, val.normalized, avoidanceRayLength, LayerMask.op_Implicit(_avoidanceMask));
			Vector3 position2 = _transformProxy.position;
			val = _transformProxy.forward + _transformProxy.right;
			_hitRight = Physics.Raycast(position2, val.normalized, avoidanceRayLength, LayerMask.op_Implicit(_avoidanceMask));
			_hitForward = _hitLeft && _hitRight && Physics.Raycast(_transformProxy.position, _transformProxy.forward, avoidanceRayLength, LayerMask.op_Implicit(_avoidanceMask));
			int hitDown;
			if (flag)
			{
				Vector3 position3 = _transformProxy.position;
				Vector3 forward = _transformProxy.forward;
				Vector3 up = _transformProxy.up;
				Bounds bounds = _fish.GetBounds();
				val = forward - up * bounds.size.y;
				hitDown = (Physics.Raycast(position3, val.normalized, avoidanceRayLength, LayerMask.op_Implicit(_avoidanceMask)) ? 1 : 0);
			}
			else
			{
				hitDown = 0;
			}
			_hitDown = (byte)hitDown != 0;
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
			if (_hitDown & flag)
			{
				eulerAngles.x -= 100f * FixedTime.TimeStep(this) * _steer;
				rotation.eulerAngles = eulerAngles;
				_transformProxy.rotation = rotation;
				Vector3 position4 = _transformProxy.position;
				position4.y += 100f * FixedTime.TimeStep(this) * 0.01f;
				_transformProxy.position = position4;
			}
		}
	}

	private void CheckIfReachedWaypoint()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = _transformProxy.position - _waypoint;
		if (val.sqrMagnitude < BeamMath.Sqr(0.02f + _stuck))
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
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = _transformProxy.position - _waypoint;
		float num = Mathf.Clamp01(val.sqrMagnitude / BeamMath.Sqr(_waypointDistance));
		_speed = Mathf.Lerp(0.2f, _originalSpeed, num);
		_steer = Mathf.Lerp(8f, _originalSteer, num);
	}

	public void HandleInitialStuck()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		if (_teleportFlag && _waypointsCache.Count <= 0)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		else
		{
			if (_waypointsCache.Count > 0)
			{
				return;
			}
			Vector3 val = ((Component)this).transform.position;
			RaycastHit val2 = default;
			for (int i = 0; i < MAX_TELEPORT_ATTEMPTS; i++)
			{
				float num = Random.Range(_boundsCenter.x - _boundsRadius, _boundsCenter.x + _boundsRadius);
				float minWorldHeight = _minWorldHeight;
				Bounds bounds = _fish.GetBounds();
				float num2 = minWorldHeight + bounds.extents.y;
				float maxWorldHeight = _maxWorldHeight;
				bounds = _fish.GetBounds();
				val = new Vector3(num, Random.Range(num2, maxWorldHeight - bounds.extents.y), Random.Range(_boundsCenter.z - _boundsRadius, _boundsCenter.z + _boundsRadius));
				if (Physics.Linecast(new Vector3(val.x, 10f, val.z), val, ref val2, LayerMask.op_Implicit(_avoidanceMask)))
				{
					float y = val2.point.y;
					bounds = _fish.GetBounds();
					val.y = y + bounds.extents.y;
					if (val.y > _minWorldHeight && val.y < _maxWorldHeight)
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
				((Component)this).transform.position = val;
				_transformProxy.position = val;
				GenerateWaypoint(11);
				AdjustSpeedAndTurningSpeed();
			}
			else
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
		}
	}

	public void DoBacktracking()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp01((float)(attempts * 2) / 12f);
		Vector3 val = _boundsCenter - _transformProxy.position;
		val.y = 0f;
		float num2 = val.sqrMagnitude / BeamMath.Sqr(_boundsRadius);
		num2 *= num2;
		num2 *= 1f - num;
		float num3 = Mathf.Lerp(1f, -1f, num);
		Vector3 val2 = _transformProxy.forward * num3;
		Vector3 onUnitSphere = Random.onUnitSphere;
		onUnitSphere.z = Mathf.Abs(onUnitSphere.z) * num3;
		onUnitSphere.Normalize();
		float num4 = Mathf.Lerp(_waypointForwardBias, 1f, num);
		Vector3 val3 = Vector3.Lerp(val2, _transformProxy.rotation * onUnitSphere, num4);
		val3 += val.normalized * num2;
		val3.Normalize();
		float num5 = Mathf.Lerp(_waypointDistance, 1f, num);
		val3 *= num5;
		_waypoint = _transformProxy.position + val3;
		ClampHeightToRange(ref _waypoint);
	}

	private void ClampHeightToRange(ref Vector3 point)
	{
		point.y = Mathf.Clamp(point.y, _minWorldHeight, _maxWorldHeight);
	}

	private bool CheckWaypoint(ref Vector3 position)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default;
		if (Physics.Linecast(new Vector3(position.x, 10f, position.z), position, ref val, LayerMask.op_Implicit(_avoidanceMask)))
		{
			float y = val.point.y;
			Bounds bounds = _fish.GetBounds();
			position.y = y + bounds.extents.y;
			if (position.y > _minWorldHeight)
			{
				return position.y < _maxWorldHeight;
			}
			return false;
		}
		return true;
	}
}
