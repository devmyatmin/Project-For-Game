using System;
using Beam.Events;
using Beam.Utilities;
using Funlabs;
using Photon.Bolt;
using UdpKit;
using UnityEngine;

namespace Beam;

public class LandAnimal : StateMachine<LandAnimal>
{
	public class ReplicateState : MultiplayerMessage
	{
		[Replicate]
		public LandAnimal stateMachine;

		public LandAnimalState newState;

		public override void OnPeer()
		{
			if (Object.op_Implicit((Object)(object)stateMachine))
			{
				stateMachine.CurrentState = newState;
			}
		}

		public override void Read(UdpPacket packet)
		{
			base.Read(packet);
			newState = (LandAnimalState)packet.ReadByte();
		}

		public override void Write(UdpPacket packet)
		{
			base.Write(packet);
			packet.WriteByte((byte)newState);
		}
	}

	public class ReplicateAnimation : MultiplayerMessage
	{
		[Replicate]
		public byte AnimationId;

		[Replicate]
		public LandAnimal StateMachine;

		public override void OnPeer()
		{
			if (Object.op_Implicit((Object)(object)StateMachine))
			{
				StateMachine.PlayAnimation(AnimationId);
			}
		}
	}

	[SerializeField]
	private LandAnimalState _startingState;

	public Vector3 StartingPosition;

	protected float breakingMultiplier = 1f;

	public float AvoidanceCheckRate;

	private float avoidanceCheckRateTimer;

	private float _lastGroundNormalCheck;

	[SerializeField]
	protected Animator _animator;

	[SerializeField]
	private LandSpecies _species;

	[SerializeField]
	protected float turnSpeed = 1f;

	protected HidingSpot _hidingspot;

	protected HidingSpotController _hidingSpotController;

	protected Vector3 _waypointPosition;

	protected float _stuckWaypointTime;

	[SerializeField]
	protected float minExploreHeight;

	[SerializeField]
	protected float maxExploreHeight = 500f;

	public LayerMask _groundNormalMask;

	public LayerMask _avoidanceMask;

	[SerializeField]
	protected Collider _movementCollider;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	protected Rigidbody _rigidbody;

	[SerializeField]
	private bool IgnoreCollisionWithPlayer = true;

	protected float _controllerVelocity;

	protected float _currentSpeed;

	protected float _adjustedSpeed;

	private Vector3 _lastPosition;

	private float _velocityStuckTimer;

	protected IPlayer closestPlayer;

	protected float distanceToClosestPlayer;

	[SerializeField]
	private float wideRayDistance = 1.9f;

	private float longRayDistance = 1.5f;

	[SerializeField]
	private float _avoidanceRayYOffset = -0.05f;

	public LandSpecies Species => _species;

	public event Action Attacked;

	public event Action Killed;

	protected virtual void OnAttacked()
	{
		Attacked?.Invoke();
	}

	protected virtual void OnKilled()
	{
		Killed?.Invoke();
	}

	protected void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		CurrentState = _startingState;
		if (StartingPosition == Vector3.zero)
		{
			StartingPosition = ((Component)this).transform.position;
		}
		EventManager.AddListener<PlayerDisconnectedEvent>(EventManager_PlayerDisconnected);
	}

	private void OnDestroy()
	{
		Attacked = null;
		Killed = null;
		EventManager.RemoveListener<PlayerDisconnectedEvent>(EventManager_PlayerDisconnected);
	}

	private void EventManager_PlayerDisconnected(PlayerDisconnectedEvent disconnectedEvent)
	{
		if (!BeamTeam.IsNullOrDestroyed(closestPlayer) && closestPlayer.Id == disconnectedEvent.PlayerId)
		{
			closestPlayer = null;
			distanceToClosestPlayer = 500f;
		}
	}

	protected void Setup()
	{
		Beam.Utilities.ExtensionMethods.ForEach_NonAlloc(PlayerRegistry.AllPlayers, IgnorePlayerCollider);
		_hidingSpotController = Singleton<HidingSpotController>.Instance;
		longRayDistance = wideRayDistance - 0.4f;
		if (Object.op_Implicit((Object)(object)_animator))
		{
			_animator.applyRootMotion = false;
		}
	}

	private void IgnorePlayerCollider(IPlayer player)
	{
		if ((Object)(object)_movementCollider != (Object)null && IgnoreCollisionWithPlayer)
		{
			Physics.IgnoreCollision(_movementCollider, (Collider)(object)player.Movement.CharacterController);
		}
	}

	public void Kill()
	{
		CurrentState = LandAnimalState.Dying;
		OnKilled();
	}

	protected bool CheckWaypointDist(float dist = 0.75f)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = new Vector2(_waypointPosition.x, _waypointPosition.z) - new Vector2(((Component)this).transform.position.x, ((Component)this).transform.position.z);
		return val.magnitude < dist;
	}

	protected void MatchGroundNormal()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.identity;
		_lastGroundNormalCheck += FixedTime.TimeStep(this);
		if (_lastGroundNormalCheck > AvoidanceCheckRate)
		{
			RaycastHit val2 = default;
			if (Physics.Raycast(((Component)this).transform.position, Vector3.down, ref val2, 10f, LayerMask.op_Implicit(_groundNormalMask), (QueryTriggerInteraction)1) && val2.normal != Vector3.zero)
			{
				val = Quaternion.FromToRotation(Vector3.up, val2.normal);
			}
			_lastGroundNormalCheck = 0f;
		}
		Vector3 val3 = new Vector3(_waypointPosition.x, ((Component)this).transform.position.y, _waypointPosition.z) - ((Component)this).transform.position;
		if (val3 != Vector3.zero)
		{
			((Component)this).transform.rotation = Quaternion.Lerp(((Component)this).transform.rotation, val * Quaternion.LookRotation(val3), turnSpeed * FixedTime.TimeStep(this));
		}
	}

	protected bool FindHidingSpot(float dist = 40f)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		_hidingspot = _hidingSpotController.FindHidingSpot(_species, _rigidbody.position, dist, ValidateLandingSpot);
		if ((Object)(object)_hidingspot != (Object)null)
		{
			_hidingspot.HideChild = this;
			_hidingspot.Occupied = true;
			return true;
		}
		return false;
	}

	protected float CalculateTrueVelocity()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = (((Component)this).transform.position - _lastPosition) / FixedTime.TimeStep(this);
		_lastPosition = ((Component)this).transform.position;
		return val.magnitude;
	}

	protected void NewAvoidWaypoint(float dist = 4f, float zigzag = 0f)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		_stuckWaypointTime = 0f;
		IPlayer player = PlayerUtilities.FindClosestPlayer(((Component)this).transform.position);
		Vector3 val = ((Component)this).transform.position - player.transform.position;
		Vector3 val2 = Vector3.Lerp(((Component)this).transform.forward, val.normalized, 0.8f);
		_waypointPosition = ((Component)this).transform.position + val2.normalized * dist + ((Component)this).transform.right * Random.Range(0f - zigzag, zigzag);
	}

	protected void NewExploreWaypoint(float dist)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		_stuckWaypointTime = 0f;
		float num = ((Random.value < 0.5f) ? 1 : (-1));
		Vector3 val = Vector3.Lerp(((Component)this).transform.forward, Random.insideUnitSphere * num, 0.4f);
		_waypointPosition = ((Component)this).transform.position + val * dist;
	}

	protected void NewHideWaypoint()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)_hidingspot))
		{
			_stuckWaypointTime = 0f;
			_waypointPosition = ((Component)_hidingspot).transform.position;
		}
	}

	protected void NewChaseWaypoint(Vector3 targetPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_waypointPosition = targetPosition;
		_stuckWaypointTime = 0f;
	}

	protected void NewChaseWaypoint(Vector3 targetPosition, float waypointDistance, float closeness = 1f)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		targetPosition.y = ((Component)this).transform.position.y;
		Vector3 val = targetPosition - ((Component)this).transform.position;
		float num = Mathf.Min(Mathf.Clamp(val.magnitude - closeness, 0.01f, float.PositiveInfinity), waypointDistance);
		_waypointPosition = ((Component)this).transform.position + val.normalized * num;
		_stuckWaypointTime = 0f;
	}

	protected bool ValidateLandingSpot(HidingSpot hidingSpot)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return BeamTeam.IsNullOrDestroyed(PlayerUtilities.FindClosestPlayer(((Component)this).transform.position, 20f));
	}

	protected void CheckBounds()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)this).transform.position.y < minExploreHeight || ((Component)this).transform.position.y > maxExploreHeight)
		{
			_waypointPosition = StartingPosition;
		}
	}

	protected void CheckIfStuck()
	{
		_stuckWaypointTime += FixedTime.TimeStep(this);
		if (_controllerVelocity < 0.03f)
		{
			_velocityStuckTimer += FixedTime.TimeStep(this);
		}
		else
		{
			_velocityStuckTimer = 0f;
		}
		if (_velocityStuckTimer > 3f)
		{
			_velocityStuckTimer = 0f;
			((Component)this).transform.Rotate(0f, 180f, 0f);
			NewExploreWaypoint(5f);
		}
		if (_stuckWaypointTime > 15f)
		{
			NewExploreWaypoint(5f);
		}
	}

	protected void SEARCH_FOR_PLAYER()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		closestPlayer = PlayerUtilities.FindClosestPlayer(((Component)this).transform.position);
		distanceToClosestPlayer = float.PositiveInfinity;
		if (!BeamTeam.IsNullOrDestroyed(closestPlayer))
		{
			Vector3 val = ((Component)this).transform.position - closestPlayer.transform.position;
			distanceToClosestPlayer = val.magnitude;
		}
	}

	protected bool CAN_SEE_PLAYER(IPlayer player)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		float num = 100f;
		Vector3 val = ((Component)this).transform.position + ((Component)this).transform.up * _avoidanceRayYOffset;
		RaycastHit val2 = default;
		if (Vector3.Angle(player.transform.position - val, ((Component)this).transform.forward) <= num && !Physics.Linecast(val, player.transform.position, ref val2, LayerMask.op_Implicit(_avoidanceMask), (QueryTriggerInteraction)1))
		{
			return true;
		}
		return false;
	}

	protected void SteerAvoidance()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		if (avoidanceCheckRateTimer != 0f && avoidanceCheckRateTimer < AvoidanceCheckRate)
		{
			avoidanceCheckRateTimer += FixedTime.TimeStep(this);
			return;
		}
		avoidanceCheckRateTimer = 0.01f;
		Vector3 forward = ((Component)this).transform.forward;
		Vector3 val = ((Component)this).transform.position + ((Component)this).transform.up * _avoidanceRayYOffset;
		Vector3 val2 = Quaternion.AngleAxis(0f, ((Component)this).transform.up) * forward;
		Vector3 zero = Vector3.zero;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		float num = 0f;
		float num2 = 0f;
		float num3 = 25f;
		float num4 = 0.25f;
		val2 = Quaternion.AngleAxis(num3, ((Component)this).transform.up) * forward;
		zero = val + val2 * wideRayDistance;
		RaycastHit val3 = default;
		if (Physics.Raycast(val, val2, ref val3, wideRayDistance, LayerMask.op_Implicit(_avoidanceMask), (QueryTriggerInteraction)1))
		{
			num = Mathf.Lerp(0f - num4, 0f, val3.distance / wideRayDistance);
			flag = true;
		}
		if ((zero.y < minExploreHeight && ((Component)this).transform.position.y > minExploreHeight) || (zero.y > maxExploreHeight && ((Component)this).transform.position.y < maxExploreHeight))
		{
			num = 0f - num4;
			flag = true;
		}
		val2 = Quaternion.AngleAxis(0f - num3, ((Component)this).transform.up) * forward;
		zero = val + val2 * wideRayDistance;
		if (Physics.Raycast(val, val2, ref val3, wideRayDistance, LayerMask.op_Implicit(_avoidanceMask), (QueryTriggerInteraction)1))
		{
			num2 = Mathf.Lerp(num4, 0f, val3.distance / wideRayDistance);
			flag2 = true;
		}
		if ((zero.y < minExploreHeight && ((Component)this).transform.position.y > minExploreHeight) || (zero.y > maxExploreHeight && ((Component)this).transform.position.y < maxExploreHeight))
		{
			num2 = num4;
			flag2 = true;
		}
		val2 = Quaternion.AngleAxis(0f, ((Component)this).transform.up) * forward;
		zero = val + val2 * longRayDistance;
		if (Physics.Raycast(val, val2, ref val3, longRayDistance, LayerMask.op_Implicit(_avoidanceMask), (QueryTriggerInteraction)1) || (zero.y > maxExploreHeight && ((Component)this).transform.position.y < maxExploreHeight))
		{
			if (num2 == 0f && num == 0f)
			{
				num = Mathf.Lerp(-1f, 0f, val3.distance / longRayDistance);
			}
			flag3 = true;
		}
		Vector3 val4 = val + ((Component)this).transform.forward * wideRayDistance;
		val2 = -((Component)this).transform.up;
		float num5 = 3f;
		bool flag4 = true;
		if (Physics.Raycast(val4, val2, ref val3, num5, LayerMask.op_Implicit(_groundNormalMask), (QueryTriggerInteraction)1) && val3.point.y > minExploreHeight)
		{
			flag4 = false;
		}
		if (flag | flag2 | flag3 | flag4)
		{
			if (flag & flag2)
			{
				if (((Component)this).transform.InverseTransformPoint(_waypointPosition).x > 0f)
				{
					num = 0f;
					num2 = 1f;
				}
				else
				{
					num = 1f;
					num2 = 0f;
				}
			}
			if (flag4)
			{
				num = 1f;
				num2 = 0f;
			}
			float num6 = num + num2;
			int num7 = 2;
			Vector3 val5 = forward * (float)num7;
			Vector3 val6 = ((Component)this).transform.right * (float)num7 * num6;
			_waypointPosition = ((Component)this).transform.position + val5 + val6;
		}
		float num8 = (Mathf.Abs(num2) + num2) * 100f;
		float num9 = 1f - Mathf.Clamp(num8, 0f, 0.5f);
		breakingMultiplier = Mathf.Lerp(breakingMultiplier, num9, FixedTime.TimeStep(this) * (1f + _currentSpeed));
	}

	public void ReplicatedChangeState(LandAnimalState NewState)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode))
		{
			ReplicateState replicateState = new ReplicateState();
			replicateState.newState = NewState;
			replicateState.stateMachine = this;
			replicateState.Post();
		}
		CurrentState = NewState;
	}

	public void ReplicatedPlayAnimation(byte animationId)
	{
		if (((EntityBehaviour)this).entity.IsOwner)
		{
			ReplicateAnimation replicateAnimation = new ReplicateAnimation();
			replicateAnimation.AnimationId = animationId;
			replicateAnimation.StateMachine = this;
			replicateAnimation.Post();
		}
		PlayAnimation(animationId);
	}
}
