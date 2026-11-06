using System;
using System.Collections;
using Beam;
using Beam.Events;
using Beam.Utilities;
using Ceto;
using Funlabs;
using Photon.Bolt;
using Photon.Bolt.Internal;
using UdpKit;
using UnityEngine;
using UnityEngine.Events;

public class Piscus_Creature : StateMachine<Piscus_Creature>, IPlayerProximityHandler
{
	public enum PiscusBehaviour
	{
		Idle,
		Explore,
		CircleTarget,
		AttackPlayer,
		AttackingPlayer,
		AttackRaft,
		AttackJump,
		Flee,
		Die
	}

	public class ReplicateFinishAttack : MultiplayerMessage
	{
		[Replicate]
		public Piscus_Creature Animal;

		[Replicate]
		public bool CompletedAttack;

		[Replicate]
		public bool Destroyed;

		public override void OnPeer()
		{
			Piscus_Creature animal = Animal;
			if ((Object)(object)animal != (Object)null)
			{
				animal.ReplicatedFinishAttack(CompletedAttack, Destroyed);
			}
		}
	}

	public class ReplicateAttackBite : MultiplayerMessage
	{
		[Replicate]
		public Piscus_Creature Animal;

		[Replicate]
		public float Force;

		public override void OnPeer()
		{
			Piscus_Creature animal = Animal;
			if ((Object)(object)animal != (Object)null)
			{
				animal.ReplicatedAttack_Player_Bite(Force);
			}
		}
	}

	public class ReplicateAttackNudge : MultiplayerMessage
	{
		[Replicate]
		public Piscus_Creature Animal;

		[Replicate]
		public IPlayer CurrentTarget;

		public override void OnPeer()
		{
			Animal.CurrentTarget = CurrentTarget;
			Animal.Attack_Nudge();
		}
	}

	public class ReplicateAnimations : MultiplayerMessage
	{
		[Replicate]
		public byte AnimationId;

		[Replicate]
		public Piscus_Creature StateMachine;

		public override void OnPeer()
		{
			StateMachine.PlayAnimation(AnimationId);
		}
	}

	public class ReplicateState : MultiplayerMessage
	{
		[Replicate]
		public Piscus_Creature StateMachine;

		public PiscusBehaviour NewState;

		[Replicate]
		public IPlayer CurrentTarget;

		[Replicate]
		public bool FinishAttack;

		public override void OnPeer()
		{
			if (Object.op_Implicit((Object)(object)StateMachine))
			{
				StateMachine.ReplicatedChangeState(NewState, CurrentTarget, FinishAttack);
			}
		}

		public override void Read(UdpPacket packet)
		{
			base.Read(packet);
			NewState = (PiscusBehaviour)packet.ReadByte();
		}

		public override void Write(UdpPacket packet)
		{
			base.Write(packet);
			packet.WriteByte((byte)NewState);
		}
	}

	private const byte ANIMATION_BITE = 0;

	private const byte ANIMATION_ATTACKING = 1;

	protected const byte PISCUS_CREATURE_ANIMATION_COUNT = 2;

	[SerializeField]
	private bool _applyDifficulty;

	private bool _debug;

	[SerializeField]
	private PiscusBehaviour _debugCurrentState;

	[SerializeField]
	private string _debugWaypointType;

	private PiscusManager _manager;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private InteractiveObject _interactiveObject;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Rigidbody _rigidBody;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Piscus_Animation _animation;

	private IPlayer _currentTarget;

	[Header("AVOIDANCE")]
	[SerializeField]
	private LayerMask _avoidanceMask;

	[SerializeField]
	private int _avoidanceRayDistance = 6;

	[SerializeField]
	private float _avoidanceRayWidth = 1f;

	private float _timeSinceLastAvoidance;

	[Header("MOVEMENT")]
	[SerializeField]
	private float _minSpeed = 3f;

	[SerializeField]
	private float _maxSpeed = 10f;

	private float _lerpedSpeed;

	private float _currentSpeed;

	[SerializeField]
	private float _minExploreDepth = -30f;

	[SerializeField]
	private float _maxExploreDepth;

	[SerializeField]
	private float _waypointArrivalDist = 2f;

	private Vector3 _biomePosition;

	private float _biomeSize;

	private Vector3 _movementPosition;

	private float _distanceToMovePos;

	private const float _waypointDistance = 12f;

	[Header("BEHAVIOUR")]
	[SerializeField]
	private float _circleRadius = 15f;

	[SerializeField]
	private float _maxCircleRadius = 30f;

	[SerializeField]
	private float _randCircleFreq = 0.5f;

	[SerializeField]
	private float _targetOutOfReachTime = -1f;

	[SerializeField]
	private float _circleTargetTime = -1f;

	private float _currentCircleTime;

	[SerializeField]
	private bool _avoidRepellent = true;

	[Header("ATTACKING")]
	[SerializeField]
	private bool _canAttack;

	[SerializeField]
	private bool _canAttackRafts;

	[SerializeField]
	private bool _canAttackJump;

	[SerializeField]
	private bool _canAttackNudge;

	private IPlayer _currentAttackTarget;

	[SerializeField]
	private float _engagePlayerDistance = 10f;

	[SerializeField]
	private float _attackRate = 10f;

	[SerializeField]
	private float _aggression = 1f;

	private float _attackPoints;

	private float _attackDamage;

	[SerializeField]
	private float _normalAttackDamage = 100f;

	private float _attackCritDamage;

	[SerializeField]
	private float _normalAttackCritDamage = 200f;

	[SerializeField]
	private float _attackEventChance = 0.3f;

	[SerializeField]
	private float _attackRaftForce = 10f;

	[SerializeField]
	private Transform _mouthPosition;

	[SerializeField]
	private Transform _mouthPlayerPosition;

	private float _lastAttack = 100f;

	private float _lastNudgeAttackTime = 100f;

	private float _attackingTimer;

	private float _targetBehindBossTimer;

	private float _wayPointStuckTime;

	private float _underTerrainTime;

	private float _targetOutOfReachTimer;

	private float _fleeTimer;

	[Header("EVENTS")]
	public UnityEvent EnteredTargetProximity;

	public UnityEvent LeftTargetProximity;

	public UnityEvent AttackedPlayer;

	private bool _playerFreezed;

	private bool _jumped;

	private float _jumpTimer;

	private float _outOfBoundsTimer;

	private float _rayRightAngle = 20f;

	private float _rayLeftAngle = 20f;

	private float _cornerTrapTimer;

	private bool hit_Left;

	private bool hit_Right;

	private bool hit_Forward;

	private bool hit_ForwardUp;

	private bool selectTurnLeft;

	private bool selectTurnRight;

	private float hit_Left_Weight;

	private float hit_Right_Weight;

	private float newInputSteer;

	Vector3 IPlayerProximityHandler.Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.position;
		}
	}

	float IPlayerProximityHandler.ProximityDistance => _engagePlayerDistance;

	bool IPlayerProximityHandler.InProximity { get; set; }

	public new object CurrentState
	{
		get
		{
			return base.CurrentState;
		}
		set
		{
			if (CurrentState == null || (PiscusBehaviour)CurrentState != PiscusBehaviour.Die)
			{
				base.CurrentState = value;
			}
		}
	}

	public PiscusManager Manager
	{
		get
		{
			return _manager;
		}
		set
		{
			_manager = value;
		}
	}

	public Piscus_Animation Animation => _animation;

	public IPlayer CurrentTarget
	{
		get
		{
			return _currentTarget;
		}
		set
		{
			_currentTarget = value;
		}
	}

	public float AvoidanceRayWidth => _avoidanceRayWidth;

	public float MinSpeed => _maxSpeed;

	public float MaxSpeed => _maxSpeed;

	public float CurrentSpeed
	{
		get
		{
			return _currentSpeed;
		}
		set
		{
			_currentSpeed = value;
		}
	}

	public Vector3 BiomePosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _biomePosition;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_biomePosition = value;
		}
	}

	public float BiomeSize
	{
		get
		{
			return _biomeSize;
		}
		set
		{
			_biomeSize = value;
		}
	}

	public Vector3 MovementPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _movementPosition;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_movementPosition = value;
		}
	}

	public float CircleRadius => _circleRadius;

	public float MaxCircleRadius => _maxCircleRadius;

	public bool CanAttack => _canAttack;

	public bool CanAttackRafts => _canAttackRafts;

	public bool CanAttackJump => _canAttackJump;

	public bool CanAttackNudge => _canAttackNudge;

	public IPlayer CurrentAttackTarget
	{
		get
		{
			return _currentAttackTarget;
		}
		set
		{
			_currentAttackTarget = value;
		}
	}

	public float EngagePlayerDistance
	{
		get
		{
			return _engagePlayerDistance;
		}
		set
		{
			_engagePlayerDistance = value;
		}
	}

	public float LastAttack
	{
		get
		{
			return _lastAttack;
		}
		set
		{
			_lastAttack = value;
		}
	}

	public event Action Awakened;

	public event Action Attacked;

	public event Action AttackedRaft;

	public event Action Jumped;

	public event Action Landed;

	void IPlayerProximityHandler.OnEnteredPlayerProximity(PlayerData playerData)
	{
		UnityEvent enteredTargetProximity = EnteredTargetProximity;
		if (enteredTargetProximity != null)
		{
			enteredTargetProximity.Invoke();
		}
	}

	void IPlayerProximityHandler.OnLeftPlayerProximity(PlayerData playerData)
	{
		UnityEvent leftTargetProximity = LeftTargetProximity;
		if (leftTargetProximity != null)
		{
			leftTargetProximity.Invoke();
		}
	}

	protected virtual void OnAwakened()
	{
		Awakened?.Invoke();
	}

	protected virtual void OnAttacked()
	{
		Attacked?.Invoke();
	}

	protected virtual void OnAttackedRaft()
	{
		AttackedRaft?.Invoke();
	}

	protected virtual void OnJumped()
	{
		Jumped?.Invoke();
	}

	protected virtual void OnLanded()
	{
		Landed?.Invoke();
	}

	private void Awake()
	{
		_rigidBody.isKinematic = true;
	}

	protected virtual void Start()
	{
		Initialize();
		EventManager.AddListener<PlayerDisconnectedEvent>(EventManager_PlayerDisconnected);
	}

	protected virtual void Initialize()
	{
		_rigidBody.isKinematic = true;
		Singleton<PiscusInspectionManager>.Instance?.piscuses.Add(this);
		PlayerProximityManager.Instance?.Subcribe(this);
		Setup_Position();
		SetDifficultyParams();
		CurrentState = PiscusBehaviour.Explore;
	}

	public void SetDifficultyParams()
	{
		_attackDamage = _normalAttackDamage;
		_attackCritDamage = _normalAttackCritDamage;
		if (PlayerUtilities.GetServerGameDifficulty() == EGameDifficultyMode.Hard)
		{
			_attackDamage = _normalAttackDamage * 1.2f;
			_attackCritDamage = _normalAttackCritDamage * 1.2f;
		}
		if (_applyDifficulty && PlayerUtilities.GetServerWildlifeMode() == EGameWildlifeMode.Passive)
		{
			_canAttack = false;
			_canAttackJump = false;
			_canAttackRafts = false;
			if (_circleTargetTime == -1f)
			{
				_circleTargetTime = 240f;
			}
		}
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<PlayerDisconnectedEvent>(EventManager_PlayerDisconnected);
		FinishAttack(completedAttack: false, destroyed: true);
		Singleton<PiscusInspectionManager>.Instance?.piscuses.Remove(this);
		PlayerProximityManager.Instance?.Unsubscribe(this);
		Awakened = null;
		Attacked = null;
		AttackedRaft = null;
		Jumped = null;
		Landed = null;
		((UnityEventBase)AttackedPlayer).RemoveAllListeners();
		AttackedPlayer = null;
	}

	public void Setup_Position()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_manager != (Object)null)
		{
			_biomePosition = new Vector3(((Component)_manager).transform.position.x, 0f, ((Component)_manager).transform.position.z);
			((Component)this).transform.position = ((Component)_manager).transform.position;
		}
		else
		{
			_biomePosition = new Vector3(((Component)this).transform.position.x, 0f, ((Component)this).transform.position.z);
			BiomeSize = 50f;
		}
	}

	protected virtual void UPDATE_MOVEMENT(bool checkAvoidance = true)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		float num = FixedTime.TimeStep(this);
		float num2 = 0f;
		if (!MPExtensions.IsClient(Game.Mode))
		{
			if (CheckAttackTimer())
			{
				_lastAttack += num;
			}
			if (checkAvoidance)
			{
				SteerAvoidance();
			}
			Vector3 val = ((Component)this).transform.position - _movementPosition;
			_distanceToMovePos = val.magnitude;
			CheckWaypointBounds();
			CheckWaypointStuck();
			float num3 = Mathf.Lerp(_currentSpeed, 0.2f, Mathf.Clamp01(_cornerTrapTimer));
			num2 = ((_cornerTrapTimer > 0f) ? num3 : _currentSpeed);
			((IPiscus_CreatureState)((EntityEventListenerBase<IState>)(object)this).state).CurrentSpeed = num2;
			((IPiscus_CreatureState)((EntityEventListenerBase<IState>)(object)this).state).MovementPosition = _movementPosition;
		}
		else
		{
			num2 = ((IPiscus_CreatureState)((EntityEventListenerBase<IState>)(object)this).state).CurrentSpeed;
			_movementPosition = ((IPiscus_CreatureState)((EntityEventListenerBase<IState>)(object)this).state).MovementPosition;
		}
		_lerpedSpeed = Mathf.Lerp(_lerpedSpeed, num2, 2f * num);
		float num4 = 1f + Mathf.Clamp(_lerpedSpeed, _minSpeed, _maxSpeed);
		Quaternion rotation = Quaternion.Lerp(((Component)this).transform.localRotation, Quaternion.LookRotation(_movementPosition - ((Component)this).transform.position), num4 * num);
		Vector3 velocity = ((Component)this).transform.forward * _lerpedSpeed * num;
		Apply_Movement(rotation, velocity);
		_animation.UpdateAnimation(_movementPosition, _lerpedSpeed, _minSpeed, _maxSpeed);
	}

	protected void Apply_Movement(Quaternion rotation, Vector3 velocity)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsClient(Game.Mode))
		{
			_rigidBody.MoveRotation(rotation);
			_rigidBody.MovePosition(((Component)this).transform.position + velocity);
		}
	}

	protected override void ReplicantFixedUpdate()
	{
		if (!LevelLoader.IsLoading())
		{
			if (_currentTarget != null && (PiscusBehaviour)CurrentState == PiscusBehaviour.AttackingPlayer)
			{
				BiteSnapUpdate();
			}
			UPDATE_MOVEMENT();
		}
	}

	private void BiteSnapUpdate()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		_currentTarget.transform.position = _mouthPlayerPosition.position + _mouthPlayerPosition.up * (_currentTarget.Movement.GetCharacterHeight() / 2f);
		_currentTarget.transform.rotation = ((Component)_mouthPlayerPosition).transform.rotation;
	}

	protected IEnumerator Explore_EnterState()
	{
		_currentSpeed = _minSpeed;
		_currentTarget = null;
		NewExploreWaypoint();
		yield break;
	}

	protected void Explore_FixedUpdate()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (!LevelLoader.IsLoading(((Component)this).gameObject))
		{
			Vector3 val = ((Component)this).transform.position - _movementPosition;
			_distanceToMovePos = val.magnitude;
			if (_distanceToMovePos < _waypointArrivalDist)
			{
				NewExploreWaypoint();
			}
			SEARCH_FOR_PLAYER();
			UPDATE_MOVEMENT();
		}
	}

	protected virtual void SEARCH_FOR_PLAYER()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		IPlayer player = PlayerUtilities.FindClosestPlayer(((Component)this).transform.position, out var _, _engagePlayerDistance);
		if (BeamTeam.IsNullOrDestroyed(player))
		{
			ReplicatedChangeState(PiscusBehaviour.Explore, _currentTarget);
			return;
		}
		Vector3 val = player.transform.position - _biomePosition;
		if (val.sqrMagnitude < BeamMath.Sqr(BiomeSize))
		{
			_currentTarget = player;
			if (_circleTargetTime != 0f)
			{
				ReplicatedChangeState(CheckTargetRepellent() ? PiscusBehaviour.Flee : PiscusBehaviour.CircleTarget, _currentTarget);
			}
		}
	}

	protected virtual IEnumerator CircleTarget_EnterState()
	{
		if (_currentTarget == null)
		{
			CurrentState = PiscusBehaviour.Explore;
			yield break;
		}
		Vector3 val = Avoid(new Vector3(_currentTarget.transform.position.x, ((Component)this).transform.position.y, _currentTarget.transform.position.z), 100f);
		Vector2 val2 = ClampPositionToCircle(new Vector2(_currentTarget.transform.position.x, _currentTarget.transform.position.z), _maxCircleRadius, new Vector2(val.x, val.z));
		Vector3 val3 = new Vector3(val2.x, ((Component)this).transform.position.y, val2.y) - ((Component)this).transform.position;
		_movementPosition = ((Component)this).transform.position + val3.normalized * 6f;
		_movementPosition = ClampToBounds(_movementPosition);
		if (_debug)
		{
			_debugWaypointType = "CircleEnter";
		}
		_currentCircleTime = 0f;
		_currentSpeed = _minSpeed;
	}

	protected virtual void CircleTarget_FixedUpdate()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		if (_currentTarget == null)
		{
			ReplicatedChangeState(PiscusBehaviour.Explore, _currentTarget);
			return;
		}
		if (CirclingTimerExpiry())
		{
			_currentSpeed = _minSpeed;
			ReplicatedChangeState(PiscusBehaviour.Flee, _currentTarget);
			return;
		}
		if (TargetOutOfReach())
		{
			_currentSpeed = _minSpeed;
			ReplicatedChangeState(PiscusBehaviour.Flee, _currentTarget);
			return;
		}
		Vector3 val = ((Component)this).transform.position - _currentTarget.transform.position;
		float magnitude = val.magnitude;
		val = ((Component)this).transform.position - _movementPosition;
		_distanceToMovePos = val.magnitude;
		if (_distanceToMovePos < _waypointArrivalDist)
		{
			SEARCH_FOR_PLAYER();
			if (_currentTarget == null)
			{
				return;
			}
			if (CheckTargetRepellent())
			{
				ReplicatedChangeState(PiscusBehaviour.Flee, _currentTarget);
				return;
			}
			float num = Mathf.Clamp01((_maxSpeed - _minSpeed) * _attackPoints);
			float num2 = _minSpeed + num;
			_currentSpeed = Mathf.Clamp(num2, _minSpeed, _maxSpeed);
			if (_timeSinceLastAvoidance > 1f)
			{
				float num3 = _circleRadius + (_maxCircleRadius - _circleRadius) * _attackPoints;
				Vector3 val2 = ((Component)this).transform.forward;
				if (Random.value < _randCircleFreq && magnitude > _circleRadius)
				{
					float num4 = ((Random.value < 0.5f) ? 1 : (-1));
					Vector3 val3 = Vector3.Lerp(val2, Random.insideUnitSphere * num4, 0.4f);
					Vector3 val4 = _currentTarget.transform.position + val3 * num3;
					_movementPosition = Vector3.Lerp(((Component)this).transform.position + ((Component)this).transform.forward, val4, 0.5f);
					_movementPosition = ClampToBounds(_movementPosition);
					if (_debug)
					{
						_debugWaypointType = "CircleRandom";
					}
				}
				else
				{
					if (_targetBehindBossTimer > 5f)
					{
						val2 = Quaternion.AngleAxis((float)((Random.value < 0.5f) ? 30 : (-30)), Vector3.up) * -((Component)this).transform.forward;
						_targetBehindBossTimer = 0f;
					}
					Vector2 center = new Vector2(_currentTarget.transform.position.x, _currentTarget.transform.position.z);
					Vector3 val5 = ((Component)this).transform.position + val2 * 6f;
					Vector2 position = new Vector2(val5.x, val5.z);
					Vector2 val6 = ClampPositionToCircle(center, num3, position);
					float num5 = Mathf.Lerp(_movementPosition.y, _currentTarget.transform.position.y, 8f * FixedTime.TimeStep(this));
					Vector3 val7 = new Vector3(val6.x, num5, val6.y);
					_movementPosition = Vector3.Lerp(((Component)this).transform.position + ((Component)this).transform.forward, val7, 0.5f);
					_movementPosition = ClampToBounds(_movementPosition);
					if (_debug)
					{
						_debugWaypointType = "Circle";
					}
				}
			}
			else
			{
				NewExploreWaypoint(6f);
			}
		}
		CalculateAttack();
		UPDATE_MOVEMENT();
	}

	private bool CirclingTimerExpiry()
	{
		_currentCircleTime += FixedTime.TimeStep(this);
		if (_circleTargetTime > 0f && _currentCircleTime > _circleTargetTime)
		{
			_currentCircleTime = 0f;
			return true;
		}
		return false;
	}

	private void CalculateAttack()
	{
		if (_canAttack)
		{
			_attackPoints += _aggression * 0.01f * FixedTime.TimeStep(this);
			_attackPoints = Mathf.Clamp01(_attackPoints);
			if (_attackPoints >= 1f)
			{
				Attempt_Attacks();
			}
			else if (_canAttackNudge)
			{
				Attempt_Player_Attack_Nudge();
			}
		}
	}

	protected virtual void Attempt_Attacks()
	{
		if (_currentTarget != null)
		{
			if (_currentTarget.Movement.InWater)
			{
				Attempt_Attack_Player_InWater();
			}
			else if (_canAttackRafts && (Object)(object)_currentTarget.Movement.ActivePlatform != (Object)null)
			{
				Attempt_Attack_Raft();
			}
			else if (_canAttackJump && Validate_Jump_Attack())
			{
				Attempt_Attack_Jump();
			}
		}
	}

	private void Attempt_Player_Attack_Nudge()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if (_currentTarget == null || !_currentTarget.Movement.InWater)
		{
			return;
		}
		if (_lastNudgeAttackTime < 30f)
		{
			_lastNudgeAttackTime += FixedTime.TimeStep(this);
		}
		if ((Object)(object)_mouthPosition != (Object)null && _lastNudgeAttackTime > 10f && _lastAttack > 5f)
		{
			Vector3 val = _mouthPosition.position - _currentTarget.transform.position;
			if (val.magnitude < 2.5f && Vector3.Angle(((Component)this).transform.forward, _currentTarget.transform.position - ((Component)this).transform.position) < 120f)
			{
				_targetBehindBossTimer = 0f;
				ReplicatedAttack_Nudge();
			}
			else
			{
				_targetBehindBossTimer += FixedTime.TimeStep(this);
			}
		}
	}

	public void Attack_Nudge()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		_animation.Animation_Damage(_currentTarget.transform.position);
		Vector3 dir = _currentTarget.transform.position - _mouthPosition.position;
		_currentTarget.Movement.AddVelocity(dir, 6f);
		_currentTarget.Statistics.ApplyDamage(_attackDamage * 0.2f, ((Component)this).transform.position, "Shark Nudge");
		_lastNudgeAttackTime = 0f;
	}

	public void Attempt_Attack_Player_InWater(bool retaliation = false)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		if (_currentTarget == null || CheckAttackTimer())
		{
			return;
		}
		Vector3 velocity = _currentTarget.Movement.GetVelocity();
		Vector3 val = _currentTarget.transform.position + velocity;
		if (!IsAttackPositionValid_Player_InWater(val))
		{
			return;
		}
		if (Vector3.Angle(((Component)this).transform.forward, _currentTarget.transform.position - ((Component)this).transform.position) > 100f)
		{
			_targetBehindBossTimer += FixedTime.TimeStep(this);
			return;
		}
		_targetBehindBossTimer = 0f;
		bool flag = true;
		Vector3 val2 = val - ((Component)this).transform.position;
		RaycastHit val3 = default;
		if (Physics.SphereCast(((Component)this).transform.position, _avoidanceRayWidth, val2, ref val3, val2.magnitude, LayerMask.op_Implicit(_avoidanceMask), (QueryTriggerInteraction)1) && (Object)(object)((Component)val3.collider).gameObject.transform != (Object)(object)_currentTarget.transform && (Object)(object)((Component)val3.collider).gameObject.transform != (Object)(object)((Component)this).transform)
		{
			flag = false;
		}
		if (flag)
		{
			_currentAttackTarget = _currentTarget;
			ReplicatedChangeState(PiscusBehaviour.AttackPlayer, _currentTarget);
		}
	}

	protected virtual bool IsAttackPositionValid_Player_InWater(Vector3 attackPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return attackPosition.y < 0f;
	}

	protected IEnumerator AttackPlayer_EnterState()
	{
		if (_currentTarget == null)
		{
			FinishAttack();
			yield break;
		}
		UnityEvent attackedPlayer = AttackedPlayer;
		if (attackedPlayer != null)
		{
			attackedPlayer.Invoke();
		}
		Vector3 velocity = _currentTarget.Movement.GetVelocity();
		_movementPosition = _currentTarget.transform.position + velocity;
		_movementPosition = ClampToBounds(_movementPosition, minClamp: false);
		if (_debug)
		{
			_debugWaypointType = "AttackPlayerEnter";
		}
	}

	protected void AttackPlayer_FixedUpdate()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		if (_currentTarget == null)
		{
			ReplicatedFinishAttack();
			return;
		}
		Vector3 val = _mouthPosition.position - _currentTarget.transform.position;
		if (val.magnitude < 4.5f)
		{
			ReplicatedPlayAnimation(0);
		}
		val = _currentTarget.Movement.GetVelocity();
		float magnitude = val.magnitude;
		_currentSpeed = _maxSpeed + magnitude;
		_currentSpeed = Mathf.Clamp(_currentSpeed, _minSpeed, _maxSpeed);
		val = ((Component)this).transform.position - _movementPosition;
		_distanceToMovePos = val.magnitude;
		if (_distanceToMovePos < _waypointArrivalDist)
		{
			val = _mouthPosition.position - _currentTarget.transform.position;
			if (val.magnitude < 3f && Vector3.Angle(((Component)this).transform.forward, _currentTarget.transform.position - ((Component)this).transform.position) < 90f)
			{
				if (Random.value < _attackEventChance)
				{
					ReplicatedChangeState(PiscusBehaviour.AttackingPlayer, _currentTarget);
				}
				else if (_currentTarget == null)
				{
					ReplicatedFinishAttack();
				}
				else
				{
					ReplicatedAttack_Player_Bite();
				}
				return;
			}
			FinishAttack();
		}
		UPDATE_MOVEMENT();
	}

	protected IEnumerator AttackPlayer_ExitState()
	{
		if (!CurrentState.Equals(PiscusBehaviour.AttackingPlayer))
		{
			ReplicatedFinishAttack();
		}
		yield break;
	}

	public void Attack_Player_Bite(float force = 6f)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		_currentTarget.Statistics.ApplyDamage(_attackDamage, ((Component)this).transform.position, "Shark Bite");
		Vector3 dir = _currentTarget.transform.position - ((Component)this).transform.position;
		_currentTarget.Movement.AddVelocity(dir, force);
		OnAttacked();
		FinishAttack(completedAttack: true);
	}

	protected IEnumerator AttackingPlayer_EnterState()
	{
		if (_currentTarget == null)
		{
			FinishAttack();
			yield break;
		}
		UnityEvent attackedPlayer = AttackedPlayer;
		if (attackedPlayer != null)
		{
			attackedPlayer.Invoke();
		}
		FreezePlayerControls(freeze: true);
		_animation.Animation_Attacking();
		_currentTarget.Statistics.ApplyStatusEffect(StatusEffect.BLEEDING);
		_currentTarget.Statistics.ApplyDamage(_attackCritDamage, ((Component)this).transform.position, "Shark Bite");
		NewExploreWaypoint();
		OnAttacked();
	}

	protected void AttackingPlayer_FixedUpdate()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		if (_currentTarget == null)
		{
			ReplicatedFinishAttack();
			return;
		}
		BiteSnapUpdate();
		_currentSpeed = _minSpeed;
		Vector3 val = ((Component)this).transform.position - _movementPosition;
		_distanceToMovePos = val.magnitude;
		if (_distanceToMovePos < _waypointArrivalDist)
		{
			NewExploreWaypoint();
		}
		_attackingTimer += FixedTime.TimeStep(this);
		if (_attackingTimer > 5f)
		{
			_attackingTimer = 0f;
			Vector3 velocityDir = _currentTarget.transform.position - ((Component)this).transform.position;
			_currentTarget.Movement.ReplicatedAddVelocity(velocityDir, 6f);
			ReplicatedFinishAttack(completedAttack: true);
		}
		UPDATE_MOVEMENT();
	}

	protected IEnumerator AttackingPlayer_ExitState()
	{
		ReplicatedFinishAttack();
		yield break;
	}

	private void FreezePlayerControls(bool freeze)
	{
		if (_currentTarget == null)
		{
			return;
		}
		if (freeze)
		{
			_playerFreezed = true;
			if (Object.op_Implicit((Object)(object)_currentTarget.Movement))
			{
				++_currentTarget.Movement.IsBusy;
				((Behaviour)_currentTarget.PlayerCamera.MouseLook).enabled = false;
			}
		}
		else if (_playerFreezed)
		{
			_playerFreezed = false;
			if (Object.op_Implicit((Object)(object)_currentTarget.Movement))
			{
				--_currentTarget.Movement.IsBusy;
				((Behaviour)_currentTarget.PlayerCamera.MouseLook).enabled = _currentTarget.IsOwner;
			}
		}
	}

	protected IEnumerator AttackRaft_EnterState()
	{
		if (_currentTarget == null)
		{
			FinishAttack();
			yield break;
		}
		Transform activePlatform = _currentTarget.Movement.ActivePlatform;
		if ((Object)(object)activePlatform == (Object)null)
		{
			FinishAttack();
			yield break;
		}
		UnityEvent attackedPlayer = AttackedPlayer;
		if (attackedPlayer != null)
		{
			attackedPlayer.Invoke();
		}
		Vector3 val = Vector3.zero;
		if (Object.op_Implicit((Object)(object)((Component)activePlatform).GetComponent<Rigidbody>()))
		{
			val = ((Component)activePlatform).GetComponent<Rigidbody>().velocity;
		}
		else
		{
			Debug.LogWarning((object)"Piscus_Creature :: No Rigidbody found on Raft target");
		}
		_movementPosition = ((Component)activePlatform).transform.position + val;
		_movementPosition = ClampToBounds(_movementPosition);
		if (_debug)
		{
			_debugWaypointType = "AttackRaftEnter";
		}
	}

	protected void AttackRaft_FixedUpdate()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		if (_currentTarget == null)
		{
			ReplicatedFinishAttack();
			return;
		}
		Transform activePlatform = _currentTarget.Movement.ActivePlatform;
		if ((Object)(object)activePlatform == (Object)null)
		{
			ReplicatedFinishAttack();
			return;
		}
		_currentSpeed = _maxSpeed;
		Vector3 val = _mouthPosition.position - ((Component)activePlatform).transform.position;
		if (val.magnitude < 5f)
		{
			ReplicatedPlayAnimation(0);
		}
		val = ((Component)this).transform.position - _movementPosition;
		_distanceToMovePos = val.magnitude;
		if (_distanceToMovePos < _waypointArrivalDist)
		{
			val = _mouthPosition.position - ((Component)activePlatform).transform.position;
			if (val.magnitude < 3f)
			{
				if (Object.op_Implicit((Object)(object)((Component)activePlatform).GetComponent<Rigidbody>()))
				{
					((Component)activePlatform).GetComponent<Rigidbody>().AddForceAtPosition(((Component)this).transform.forward * _attackRaftForce + Vector3.up * 2f, _mouthPosition.position, (ForceMode)1);
				}
				OnAttackedRaft();
				_lastAttack = 0f;
			}
			ReplicatedFinishAttack();
		}
		UPDATE_MOVEMENT();
	}

	public void Attempt_Attack_Raft()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		if (CheckAttackTimer())
		{
			return;
		}
		Transform activePlatform = _currentTarget.Movement.ActivePlatform;
		if ((Object)(object)activePlatform == (Object)null || ((Component)activePlatform).transform.position.y > 5f)
		{
			return;
		}
		bool flag = ((Component)this).transform.position.y < ((Component)activePlatform).transform.position.y - 3f;
		Vector3 val = activePlatform.position - ((Component)this).transform.position;
		RaycastHit val2 = default;
		if (Physics.SphereCast(((Component)this).transform.position, _avoidanceRayWidth, val, ref val2, val.magnitude, 1 << Layers.TERRAIN, (QueryTriggerInteraction)1) && Object.op_Implicit((Object)(object)val2.collider))
		{
			flag = false;
		}
		if (Physics.SphereCast(((Component)this).transform.position, _avoidanceRayWidth, -((Component)this).transform.up, ref val2, 3f, LayerMask.op_Implicit(_avoidanceMask), (QueryTriggerInteraction)1))
		{
			return;
		}
		if (flag)
		{
			ReplicatedChangeState(PiscusBehaviour.AttackRaft, _currentTarget);
			return;
		}
		Vector3 val3 = ((Component)this).transform.position - _movementPosition;
		_distanceToMovePos = val3.magnitude;
		if (_distanceToMovePos < _waypointArrivalDist)
		{
			Vector3 val4 = ((Component)this).transform.position + ((Component)this).transform.forward * 12f;
			Vector2 val5 = ClampPositionToCircle(new Vector2(((Component)activePlatform).transform.position.x, ((Component)activePlatform).transform.position.z), _maxCircleRadius, new Vector2(val4.x, val4.z));
			_movementPosition = new Vector3(val5.x, ((Component)activePlatform).transform.position.y - 5f, val5.y);
			_movementPosition = ClampToBounds(_movementPosition);
			if (_debug)
			{
				_debugWaypointType = "AttackRaftPrep";
			}
		}
	}

	private bool Validate_Jump_Attack()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (CheckAttackTimer())
		{
			return false;
		}
		if (_currentTarget == null)
		{
			return false;
		}
		float num = 4f;
		if (_currentTarget.transform.position.y > num)
		{
			return false;
		}
		RaycastHit val = default;
		if (Physics.Raycast(new Vector3(_currentTarget.transform.position.x, 0f, _currentTarget.transform.position.z), Vector3.down, ref val, 8f, 1 << Layers.TERRAIN, (QueryTriggerInteraction)1) && Object.op_Implicit((Object)(object)val.collider) && ((Component)val.collider).gameObject.tag == "Terrain")
		{
			return false;
		}
		return true;
	}

	public void Attempt_Attack_Jump()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		if (CheckAttackTimer() || _currentTarget == null)
		{
			return;
		}
		Vector3 val = ((Component)this).transform.position + ((Component)this).transform.forward * 12f;
		Vector2 val2 = ClampPositionToCircle(new Vector2(_currentTarget.transform.position.x, _currentTarget.transform.position.z), _maxCircleRadius, new Vector2(val.x, val.z));
		_movementPosition = new Vector3(val2.x, Mathf.Lerp(_movementPosition.y, -6.5f, 8f * FixedTime.TimeStep(this)), val2.y);
		_movementPosition = ClampToBounds(_movementPosition);
		if (_debug)
		{
			_debugWaypointType = "JumpAttackPrep";
		}
		Vector3 val3 = new Vector3(_currentTarget.transform.position.x, 0f, _currentTarget.transform.position.z);
		Vector3 val4 = new Vector3(((Component)this).transform.position.x, 0f, ((Component)this).transform.position.z);
		float num = Vector3.Distance(val3, val4);
		if ((!(num < _circleRadius * 0.5f) || !(num > _circleRadius * 0.2f)) && !(((Component)this).transform.position.y < -8f) && !(((Component)this).transform.position.y > -1f))
		{
			Vector3 val5 = NewJumpWaypoint() - ((Component)this).transform.position;
			RaycastHit val6 = default;
			if (!Physics.SphereCast(((Component)this).transform.position, _avoidanceRayWidth, val5, ref val6, val5.magnitude, 1 << Layers.TERRAIN, (QueryTriggerInteraction)1) || !Object.op_Implicit((Object)(object)val6.collider))
			{
				_currentAttackTarget = _currentTarget;
				ReplicatedChangeState(PiscusBehaviour.AttackJump, _currentTarget);
			}
		}
	}

	protected IEnumerator AttackJump_EnterState()
	{
		_jumped = false;
		_movementPosition = NewJumpWaypoint();
		if (_debug)
		{
			_debugWaypointType = "JumpAttack";
		}
		UnityEvent attackedPlayer = AttackedPlayer;
		if (attackedPlayer != null)
		{
			attackedPlayer.Invoke();
		}
		yield break;
	}

	protected void AttackJump_Update()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		_jumpTimer += FixedTime.TimeStep(this);
		if (_jumpTimer > 10f)
		{
			ReplicatedFinishAttack();
			return;
		}
		_currentSpeed = _maxSpeed * 2f;
		UPDATE_MOVEMENT(checkAvoidance: false);
		Vector3 val = _currentTarget.transform.position - ((Component)_mouthPosition).transform.position;
		float magnitude = val.magnitude;
		if (magnitude < 5.5f)
		{
			ReplicatedPlayAnimation(1);
		}
		if (!_jumped)
		{
			if (_distanceToMovePos < _waypointArrivalDist)
			{
				_jumped = true;
				OnJumped();
				_movementPosition = _currentTarget.transform.position + Vector3.up * 1f;
				if (_debug)
				{
					_debugWaypointType = "JumpAttackPlayer";
				}
			}
			return;
		}
		if (_distanceToMovePos < _waypointArrivalDist)
		{
			_movementPosition = NewJumpLandWaypoint();
			if (_debug)
			{
				_debugWaypointType = "JumpAttackLand";
			}
			if (magnitude < 2f)
			{
				ReplicatedPlayAnimation(1);
				if (_currentTarget == null)
				{
					ReplicatedFinishAttack();
				}
				else
				{
					ReplicatedAttack_Player_Bite(10f);
				}
				return;
			}
		}
		if (((Component)this).transform.position.y < 0f)
		{
			_lastAttack = 0f;
			ReplicatedFinishAttack();
			OnLanded();
		}
	}

	private Vector3 NewJumpLandWaypoint()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vector3 result = ((Component)this).transform.position + ((Component)this).transform.forward * 10f;
		result.y = -10f;
		return result;
	}

	private Vector3 NewJumpWaypoint()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = new Vector3(_currentTarget.transform.position.x, 0f, _currentTarget.transform.position.z);
		Vector3 val2 = new Vector3(((Component)this).transform.position.x, 0f, ((Component)this).transform.position.z);
		return Vector3.Lerp(val, val2, 0.5f) + Vector3.up * 2f;
	}

	protected void FinishAttack(bool completedAttack = false, bool destroyed = false)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		if (completedAttack)
		{
			_lastAttack = 0f;
			_attackPoints = 0f;
		}
		FreezePlayerControls(freeze: false);
		_animation.Animation_Attack_Complete();
		_currentSpeed = _maxSpeed;
		if (!destroyed)
		{
			CurrentState = ((_currentTarget == null) ? PiscusBehaviour.Explore : PiscusBehaviour.CircleTarget);
		}
		if ((_currentTarget != null) & completedAttack)
		{
			_movementPosition = ((Component)this).transform.position + ((Component)this).transform.forward * 6f;
			_movementPosition = ClampToBounds(_movementPosition);
			if (_debug)
			{
				_debugWaypointType = "FinishAttack";
			}
		}
		_currentAttackTarget = null;
		_currentTarget = null;
		if (!destroyed && MPExtensions.IsMaster(Game.Mode))
		{
			SEARCH_FOR_PLAYER();
		}
	}

	protected IEnumerator Flee_EnterState()
	{
		if (_currentTarget == null)
		{
			CurrentState = PiscusBehaviour.Explore;
			yield break;
		}
		_movementPosition = Avoid(_currentTarget.transform.position, 6f, Random.Range(1, 6));
		_movementPosition = ClampToBounds(_movementPosition);
		_fleeTimer = 0f;
		if (_debug)
		{
			_debugWaypointType = "EnterFleeAvoid";
		}
	}

	protected void Flee_FixedUpdate()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		_fleeTimer += FixedTime.TimeStep(this);
		if (_fleeTimer > 30f)
		{
			_fleeTimer = 0f;
			ReplicatedChangeState(PiscusBehaviour.Explore, _currentTarget);
			return;
		}
		if (_currentTarget == null)
		{
			ReplicatedChangeState(PiscusBehaviour.Explore, _currentTarget);
			return;
		}
		Vector3 val = ((Component)this).transform.position - _movementPosition;
		_distanceToMovePos = val.magnitude;
		if (_distanceToMovePos < _waypointArrivalDist)
		{
			_movementPosition = Avoid(_currentTarget.transform.position, 6f, Random.Range(1, 6));
			_movementPosition = ClampToBounds(_movementPosition);
			if (_debug)
			{
				_debugWaypointType = "FleeAvoid";
			}
		}
		val = ((Component)this).transform.position - _currentTarget.transform.position;
		if (val.magnitude > _engagePlayerDistance)
		{
			ReplicatedChangeState(PiscusBehaviour.Explore, _currentTarget);
		}
		UPDATE_MOVEMENT();
	}

	protected virtual void Die()
	{
	}

	public IEnumerator Die_EnterState()
	{
		Animation.AnimatorController.SetTrigger("Dead");
		Die();
		yield return (object)new WaitForSeconds(4f);
		IDisintegratable disintegratable = _interactiveObject as IDisintegratable;
		if (!BeamTeam.IsNullOrDestroyed(disintegratable))
		{
			disintegratable.Disintegrate();
		}
		else
		{
			_interactiveObject.Destroy();
		}
	}

	public void Damaged(IBase sender, IBaseActionEventData data)
	{
		_interactiveObject.HealthPoints -= data.Value;
		ApplyDamage(data.Value);
	}

	public virtual void ApplyDamage(float damageTaken)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (_currentTarget == null)
		{
			return;
		}
		_animation.Animation_Damage(_currentTarget.transform.position);
		if (_interactiveObject.HealthPoints <= 0f)
		{
			if (CurrentState.Equals(PiscusBehaviour.AttackingPlayer))
			{
				ReplicatedChangeState(PiscusBehaviour.Die, _currentTarget, finishAttack: true);
			}
			else
			{
				ReplicatedChangeState(PiscusBehaviour.Die, _currentTarget);
			}
		}
		else
		{
			if (_currentAttackTarget != null)
			{
				return;
			}
			Damage_Avoid();
			if (_canAttack)
			{
				if (Random.value < 0.5f)
				{
					_attackPoints = 1f;
					Attempt_Attack_Player_InWater(retaliation: true);
				}
				_lastAttack = _attackRate;
				_attackPoints += _aggression * 0.05f;
				_attackPoints = Mathf.Clamp01(_attackPoints);
			}
			else
			{
				CurrentState = PiscusBehaviour.Flee;
			}
		}
	}

	protected virtual void Damage_Avoid()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		_currentSpeed = _maxSpeed;
		_movementPosition = Avoid(_currentTarget.transform.position + _currentTarget.Movement.GetVelocity(), 10f);
		_movementPosition = ClampToBounds(_movementPosition);
		if (_debug)
		{
			_debugWaypointType = "DamageAvoid";
		}
	}

	private bool CheckTargetRepellent()
	{
		if (_avoidRepellent && _currentTarget != null)
		{
			return _currentTarget.Statistics.HasStatusEffect(StatusEffect.SHARK_REPELLENT);
		}
		return false;
	}

	private void CheckWaypointBounds()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.position - _biomePosition;
		if (val.magnitude > BiomeSize)
		{
			_outOfBoundsTimer += FixedTime.TimeStep(this);
			if (_distanceToMovePos < _waypointArrivalDist)
			{
				val = _biomePosition - ((Component)this).transform.position;
				Vector3 normalized = val.normalized;
				_movementPosition = ((Component)this).transform.position + normalized * 12f;
				_movementPosition = ClampToBounds(_movementPosition);
				if (_debug)
				{
					_debugWaypointType = "BiomeCenterDirection";
				}
			}
		}
		else
		{
			_outOfBoundsTimer = 0f;
		}
		if (_outOfBoundsTimer > 10f)
		{
			ResetPosition();
		}
	}

	private void ResetPosition()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = _biomePosition;
		NewExploreWaypoint();
	}

	private void CheckWaypointStuck()
	{
		if (_distanceToMovePos < 5f)
		{
			_wayPointStuckTime += FixedTime.TimeStep(this);
		}
		else
		{
			_wayPointStuckTime = 0f;
		}
		if (_wayPointStuckTime > 10f)
		{
			_wayPointStuckTime = 0f;
			_distanceToMovePos = 0f;
		}
	}

	public bool CheckAttackTimer()
	{
		if (_lastAttack < _attackRate)
		{
			return true;
		}
		return false;
	}

	private void SteerAvoidance()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		//IL_0496: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_0428: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_0517: Unknown result type (might be due to invalid IL or missing references)
		//IL_051c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_0522: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0533: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_057a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0587: Unknown result type (might be due to invalid IL or missing references)
		//IL_058c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0591: Unknown result type (might be due to invalid IL or missing references)
		//IL_0592: Unknown result type (might be due to invalid IL or missing references)
		//IL_059e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Unknown result type (might be due to invalid IL or missing references)
		//IL_0552: Unknown result type (might be due to invalid IL or missing references)
		//IL_0557: Unknown result type (might be due to invalid IL or missing references)
		//IL_055c: Unknown result type (might be due to invalid IL or missing references)
		//IL_055d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0569: Unknown result type (might be due to invalid IL or missing references)
		//IL_056e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05da: Unknown result type (might be due to invalid IL or missing references)
		//IL_05df: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05be: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = ((Component)this).transform.forward;
		Vector3 val = ((Component)this).transform.position + -((Component)this).transform.up * 0.2f;
		int num = (int)((float)_avoidanceRayDistance * 1.5f);
		RaycastHit val2 = default;
		if (Physics.SphereCast(val, _avoidanceRayWidth, Quaternion.AngleAxis(0f - _rayLeftAngle, ((Component)this).transform.up) * forward, ref val2, (float)_avoidanceRayDistance, LayerMask.op_Implicit(_avoidanceMask), (QueryTriggerInteraction)1))
		{
			hit_Left_Weight = Mathf.Lerp(1f, 0.05f, val2.distance / (float)_avoidanceRayDistance);
			hit_Left = true;
		}
		else
		{
			hit_Left_Weight = 0f;
			hit_Left = false;
		}
		if (Physics.SphereCast(val, _avoidanceRayWidth, Quaternion.AngleAxis(_rayRightAngle, ((Component)this).transform.up) * forward, ref val2, (float)_avoidanceRayDistance, LayerMask.op_Implicit(_avoidanceMask), (QueryTriggerInteraction)1))
		{
			hit_Right_Weight = Mathf.Lerp(1f, 0.05f, val2.distance / (float)_avoidanceRayDistance);
			hit_Right = true;
		}
		else
		{
			hit_Right_Weight = 0f;
			hit_Right = false;
		}
		if (Physics.SphereCast(val, _avoidanceRayWidth, forward, ref val2, (float)num, LayerMask.op_Implicit(_avoidanceMask), (QueryTriggerInteraction)1))
		{
			hit_Forward = true;
		}
		else
		{
			hit_Forward = false;
		}
		if (_timeSinceLastAvoidance < 30f)
		{
			_timeSinceLastAvoidance += FixedTime.TimeStep(this);
		}
		if (Physics.Raycast(val + Vector3.up * 20f, -Vector3.up, ref val2, 20f, 1 << Layers.TERRAIN, (QueryTriggerInteraction)1))
		{
			_underTerrainTime += FixedTime.TimeStep(this);
		}
		else if (!hit_Left && !hit_Right && !hit_Forward)
		{
			_underTerrainTime = 0f;
		}
		if (_underTerrainTime > 5f)
		{
			_cornerTrapTimer = 0f;
			if (_underTerrainTime > 60f)
			{
				_underTerrainTime = 0f;
				ResetPosition();
			}
			return;
		}
		if (hit_Left && hit_Right)
		{
			_cornerTrapTimer += FixedTime.TimeStep(this);
			if (_cornerTrapTimer > 0.5f && !selectTurnLeft && !selectTurnRight)
			{
				if (hit_Left_Weight > hit_Right_Weight)
				{
					selectTurnLeft = true;
				}
				else
				{
					selectTurnRight = true;
				}
			}
		}
		else if (_cornerTrapTimer > 0f)
		{
			_cornerTrapTimer -= FixedTime.TimeStep(this);
		}
		if ((!hit_Left && !hit_Right) || _cornerTrapTimer <= 0.1f)
		{
			selectTurnLeft = false;
			selectTurnRight = false;
		}
		if (hit_Left || hit_Right || hit_Forward)
		{
			_timeSinceLastAvoidance = 0f;
			newInputSteer = hit_Left_Weight + (0f - hit_Right_Weight);
			if (selectTurnRight)
			{
				newInputSteer = hit_Left_Weight + hit_Right_Weight;
			}
			else if (selectTurnLeft)
			{
				newInputSteer = 0f - (hit_Left_Weight + hit_Right_Weight);
			}
			newInputSteer = Mathf.Clamp(newInputSteer, -1f, 1f);
			float num2 = _waypointArrivalDist * 2f;
			Vector3 val3 = forward * num2;
			Vector3 val4 = ((Component)this).transform.right * num2 * newInputSteer;
			_movementPosition = ((Component)this).transform.position + val3 + val4;
			_movementPosition.y = ((Component)this).transform.position.y;
			if (hit_Forward)
			{
				_movementPosition += ((Component)this).transform.up;
				if (_debug)
				{
					Debug.DrawRay(val, forward * (float)num + ((Component)this).transform.up, Color.yellow);
				}
			}
			_movementPosition = ClampToBounds(_movementPosition, minClamp: true, boundsClamp: false);
			if (_debug)
			{
				_debugWaypointType = "Avoid";
			}
		}
		if (_debug)
		{
			_debugCurrentState = (PiscusBehaviour)CurrentState;
			Debug.DrawRay(((Component)this).transform.position, _movementPosition - ((Component)this).transform.position, Color.green);
			Debug.DrawRay(((Component)this).transform.position, _biomePosition - ((Component)this).transform.position, Color.red);
			if (hit_Left)
			{
				Debug.DrawRay(val, Quaternion.AngleAxis(0f - _rayLeftAngle, ((Component)this).transform.up) * forward * (float)_avoidanceRayDistance, Color.red);
			}
			else
			{
				Debug.DrawRay(val, Quaternion.AngleAxis(0f - _rayLeftAngle, ((Component)this).transform.up) * forward * (float)_avoidanceRayDistance, Color.white);
			}
			if (hit_Right)
			{
				Debug.DrawRay(val, Quaternion.AngleAxis(_rayRightAngle, ((Component)this).transform.up) * forward * (float)_avoidanceRayDistance, Color.red);
			}
			else
			{
				Debug.DrawRay(val, Quaternion.AngleAxis(_rayRightAngle, ((Component)this).transform.up) * forward * (float)_avoidanceRayDistance, Color.white);
			}
			if (hit_Forward)
			{
				Debug.DrawRay(val, forward * (float)num, Color.red);
			}
			else
			{
				Debug.DrawRay(val, Quaternion.AngleAxis(0f, ((Component)this).transform.up) * forward * (float)num, Color.green);
			}
		}
	}

	public Vector3 Avoid(Vector3 targetPos, float dist = 6f, float zigzag = 1f)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.position - targetPos;
		return ((Component)this).transform.position + val.normalized * dist + ((Component)this).transform.right * Random.Range(0f - zigzag, zigzag);
	}

	private void NewExploreWaypoint(float dist = 12f)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		float num = ((Random.value < 0.5f) ? 1 : (-1));
		Vector3 val = Vector3.Lerp(((Component)this).transform.forward, Random.insideUnitSphere * num, 0.4f);
		Vector3 val2 = ((Component)this).transform.position + val * dist;
		Vector2 val3 = ClampPositionToCircle(new Vector2(_biomePosition.x, _biomePosition.z), BiomeSize, new Vector2(val2.x, val2.z));
		_movementPosition = new Vector3(val3.x, val2.y, val3.y);
		_movementPosition = ClampToBounds(_movementPosition);
		if (_debug)
		{
			_debugWaypointType = "Explore";
		}
	}

	public Vector3 ClampToBounds(Vector3 point, bool minClamp = true, bool boundsClamp = true)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = (Vector2)(boundsClamp ? ClampPositionToCircle(new Vector2(_biomePosition.x, _biomePosition.z), BiomeSize, new Vector2(point.x, point.z)) : new Vector2(point.x, point.z));
		Vector3 val2 = point;
		if ((Object)(object)Ocean.Instance != (Object)null)
		{
			float num = Ocean.Instance.FindMaxDisplacement(inculdeOverlays: false);
			float num2 = Ocean.Instance.level - num;
			float num3 = Mathf.Min(_maxExploreDepth, num2 - _maxExploreDepth);
			val2.y = Mathf.Clamp(val2.y, _minExploreDepth, num3);
		}
		else
		{
			val2.y = Mathf.Clamp(val2.y, _minExploreDepth, _maxExploreDepth);
		}
		return new Vector3(val.x, val2.y, val.y);
	}

	public Vector2 ClampPositionToCircle(Vector2 center, float radius, Vector2 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = position - center;
		float magnitude = val.magnitude;
		if (radius < magnitude)
		{
			Vector2 val2 = val / magnitude;
			position = center + val2 * radius;
		}
		return position;
	}

	private bool TargetOutOfReach()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (_currentTarget == null)
		{
			return true;
		}
		if (_targetOutOfReachTime <= 0f)
		{
			return false;
		}
		bool flag = !_currentTarget.Movement.InWater && (Object)(object)_currentTarget.Movement.ActivePlatform == (Object)null;
		if (Vector3.Distance(_currentTarget.transform.position, _biomePosition) > BiomeSize)
		{
			flag = true;
		}
		if (flag)
		{
			_targetOutOfReachTimer += FixedTime.TimeStep(this);
			if (_targetOutOfReachTimer >= _targetOutOfReachTime)
			{
				_targetOutOfReachTimer = 0f;
				return true;
			}
		}
		else
		{
			_targetOutOfReachTimer = 0f;
		}
		return false;
	}

	protected override void PlayAnimation(byte animationId)
	{
		switch (animationId)
		{
		case 0:
			_animation.Animation_Bite();
			break;
		case 1:
			_animation.Animation_Attacking();
			break;
		}
	}

	public void PlayDamageAnimation(IPlayer Player)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (Player != null)
		{
			_animation.Animation_Damage(Player.transform.position);
		}
	}

	public void ReplicatedFinishAttack(bool completedAttack = false, bool destroyed = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMaster(Game.Mode))
		{
			ReplicateFinishAttack replicateFinishAttack = new ReplicateFinishAttack();
			replicateFinishAttack.Animal = this;
			replicateFinishAttack.CompletedAttack = completedAttack;
			replicateFinishAttack.Destroyed = destroyed;
			replicateFinishAttack.Post();
		}
		FinishAttack(completedAttack, destroyed);
	}

	public void ReplicatedAttack_Player_Bite(float force = 6f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMaster(Game.Mode))
		{
			ReplicateAttackBite replicateAttackBite = new ReplicateAttackBite();
			replicateAttackBite.Animal = this;
			replicateAttackBite.Force = force;
			replicateAttackBite.Post();
		}
		Attack_Player_Bite(force);
	}

	public void ReplicatedAttack_Nudge()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMaster(Game.Mode))
		{
			ReplicateAttackNudge replicateAttackNudge = new ReplicateAttackNudge();
			replicateAttackNudge.Animal = this;
			replicateAttackNudge.CurrentTarget = _currentTarget;
			replicateAttackNudge.Post();
		}
		Attack_Nudge();
	}

	public void ReplicatedPlayAnimation(byte animationId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode))
		{
			ReplicateAnimations replicateAnimations = new ReplicateAnimations();
			replicateAnimations.AnimationId = animationId;
			replicateAnimations.StateMachine = this;
			replicateAnimations.Post();
		}
		PlayAnimation(animationId);
	}

	public void ReplicatedChangeState(PiscusBehaviour NewState, IPlayer currentTarget, bool finishAttack = false)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (currentTarget != _currentTarget || NewState != (PiscusBehaviour)CurrentState || finishAttack)
		{
			if (MPExtensions.IsServer(Game.Mode) || NewState == PiscusBehaviour.Die)
			{
				ReplicateState replicateState = new ReplicateState();
				replicateState.NewState = NewState;
				replicateState.StateMachine = this;
				replicateState.CurrentTarget = currentTarget;
				replicateState.FinishAttack = finishAttack;
				replicateState.Post();
			}
			if (finishAttack)
			{
				FinishAttack();
			}
			if (MPExtensions.IsClient(Game.Mode))
			{
				_currentTarget = currentTarget;
			}
			CurrentState = NewState;
		}
	}

	public void EventManager_PlayerDisconnected(PlayerDisconnectedEvent disconnectedEvent)
	{
		EventManager_PlayerDisconnected_Core(disconnectedEvent);
	}

	protected virtual void EventManager_PlayerDisconnected_Core(PlayerDisconnectedEvent disconnectedEvent)
	{
		if (_currentAttackTarget != null && PlayerUtilities.GetPlayerId_Safe(_currentAttackTarget) == disconnectedEvent.PlayerId)
		{
			FinishAttack();
		}
		if (_currentTarget != null && PlayerUtilities.GetPlayerId_Safe(_currentTarget) == disconnectedEvent.PlayerId)
		{
			_currentTarget = null;
			if (((Behaviour)this).isActiveAndEnabled)
			{
				CurrentState = PiscusBehaviour.Explore;
			}
		}
	}
}
