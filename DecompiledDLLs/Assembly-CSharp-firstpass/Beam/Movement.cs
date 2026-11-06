using System;
using Beam.Crafting;
using Beam.Events;
using Beam.Serialization.Json;
using Beam.Utilities;
using Ceto;
using Funlabs;
using Photon.Bolt;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

[RequireComponent(typeof(CharacterController))]
public class Movement : MultiplayerBehaviour, INeedPlayerComponent, ISaveable, IIdleUntilInitialized, IMultiplayerState<IPlayerState>
{
	public delegate void VehicleChangedHandler(object sender, EventArgs e);

	private class ReplicateEject : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		public override void OnPeer()
		{
			Player.Movement.Vehicle?.VehicleMovement.Eject(Player);
		}
	}

	public class ReplicateBreakBones : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		public override void OnPeer()
		{
			Player.Movement.BreakBones();
		}
	}

	public class ReplicateAddVelocity : MultiplayerMessage
	{
		[Replicate]
		public IPlayer player;

		[Replicate]
		public Vector3 velocityDir;

		[Replicate]
		public float velocityForce;

		public override void OnPeer()
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			MultiplayerMng.Log("Add velocity piscus event");
			if (player != null && (Object)(object)player.Movement != (Object)null)
			{
				player.Movement.AddVelocity(velocityDir, velocityForce);
			}
		}
	}

	public class ReplicateIsBusy : MultiplayerMessage
	{
		[Replicate]
		public IPlayer player;

		[Replicate]
		public bool isBusy;

		public override void OnPeer()
		{
			MultiplayerMng.Log("Is busy piscus event");
			if (player != null && (Object)(object)player.Movement != (Object)null)
			{
				if (isBusy)
				{
					++player.Movement.IsBusy;
				}
				else
				{
					--player.Movement.IsBusy;
				}
			}
		}
	}

	private const float DOUBLE_TAP_TIME = 0.2f;

	[BitMask(typeof(UsingConstraint))]
	[SerializeField]
	private UsingConstraint _usingContraints;

	[SerializeField]
	private float _climbOffsetTreeEnd = 0.45f;

	private CharacterController _characterController;

	private bool _debug;

	public Condition IsBusy;

	public bool IsInCutscene;

	public bool _isInEndGameScene;

	private IDragger _dragger;

	private IHolder _holder;

	[SerializeField]
	private LayerMask clipMask;

	private PlayerSkills.ExpSource _skillExpSource = PlayerSkills.ExpSource.SWIMMING;

	private float physicalSkillCheckTimer = 1f;

	public bool canJump = true;

	private float _playerSpeed;

	private Vector3 _targetVelocity = Vector3.zero;

	private float walkSpeed = 3f;

	private float walkBackwardSpeed = 2f;

	private float runSpeed = 5.5f;

	private float crouchSpeed = 1.5f;

	private float swimSpeed = 2.5f;

	private float swimBackwardSpeed = 1.5f;

	private float diveSpeed = 2.5f;

	private float slideSpeed = 5f;

	private float ladderClimbSpeed = 0.8f;

	private float swimFastSpeed = 4f;

	private float diveFastSpeed = 4f;

	private float _playerControllerHeight = 1.8f;

	private float _crouchControllerHeight = 1.2f;

	private float _swimHeight = 1.2f;

	private float _jumpHeight = 8f;

	private float _jumpInterval = 0.5f;

	private float _gravity = -18f;

	private float _antiBumpFactor = 0.75f;

	private float _playerFloatOffset;

	private float _treeSnapOffset = 0.45f;

	private float _ladderSnapOffset = 0.3f;

	private bool _inWater;

	public bool isFloating;

	public bool grounded;

	public bool groundedAnimal;

	public bool crouched;

	public bool sprinting;

	public bool sliding;

	public bool falling;

	public bool touchingWater;

	private float _oceanHeight;

	private Vector3 groundNormal;

	private Quaternion _controllerRotation;

	private float _fallStartLevel;

	private float _ccRayDistance;

	private float _jumpCooldownTimer;

	private float _inAirTime;

	private Vector3 calculatedVelocity;

	private Vector3 _previousPosition;

	private Vector3 _extraVelocity;

	private Vector3 _extraJumpVelocity;

	public bool terrainGrounded;

	[SerializeField]
	private Transform _activePlatform;

	private bool _activePlatformFollow;

	private MovingPlatform _previousPlatform;

	private Vector3 _activeLocalPlatformPoint;

	private IBaseSync<Vector3> _platformPositionSync = new ZenoSyncPos();

	private IBaseSync<Quaternion> _platformRotationSync = new ZenoSyncRot();

	private Vector3 _lastPlatformSpacePosition = Vector3.zero;

	public Ladders activeLadder;

	private bool _climbingLadder;

	private bool _canClimbLadder;

	private Quaternion _ladderRot;

	private bool _climbingLedge;

	private bool _canClimbLedge;

	public Player_Ledge activeLedge;

	private Player_Ledge _ledgeClimbed;

	private IVehicle _vehicle;

	public bool GodMode;

	private float _inputX;

	private float _inputY;

	private float _inputModifyFactor;

	private bool _inputJumpPress;

	private bool _inputJumpPressUp;

	private bool _inputCrouch;

	private bool _inputCrouchPress;

	private bool _inputSprint;

	private bool _inputSprintPress;

	private bool _inputClimb;

	private float _inputClimbDownTime;

	private float _lastTapTime;

	private bool _autoWalk = true;

	private bool _sprintButtonDown;

	private bool _sprintToggle;

	private bool _doubleTap;

	private bool _crouchButtonDown;

	private bool _crouchToggle;

	private bool _climbAvailable;

	private IPlayer _player;

	[SerializeField]
	private UnityEvent _onReachedShore;

	private bool _reachedShoreCheck;

	private bool _initialized;

	public IPlayerState MultiplayerState => base.GetMultiplayerState<IPlayerState>();

	public UsingConstraint UsingContraints
	{
		get
		{
			if (InWater)
			{
				_usingContraints |= UsingConstraint.Underwater;
			}
			else
			{
				_usingContraints &= ~UsingConstraint.Underwater;
			}
			if (isFloating)
			{
				_usingContraints |= UsingConstraint.Floating;
			}
			else
			{
				_usingContraints &= ~UsingConstraint.Floating;
			}
			return _usingContraints;
		}
	}

	public CharacterController CharacterController
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_characterController))
			{
				_characterController = ((Component)this).GetComponent<CharacterController>();
			}
			return _characterController;
		}
		set
		{
			_characterController = value;
		}
	}

	public bool IsInEndGameScene
	{
		get
		{
			return _isInEndGameScene;
		}
		set
		{
			_isInEndGameScene = value;
		}
	}

	private float DeltaTime
	{
		get
		{
			if (!Time.inFixedTimeStep)
			{
				return Time.deltaTime;
			}
			return Time.fixedDeltaTime;
		}
	}

	public bool CanSprint { get; set; }

	public bool InWater
	{
		get
		{
			return _inWater;
		}
		set
		{
			if (_inWater != value)
			{
				OnInWaterChanged(value);
			}
			_inWater = value;
		}
	}

	public bool IsUnderwater
	{
		get
		{
			if (InWater)
			{
				return !isFloating;
			}
			return false;
		}
	}

	protected Vector3 PlayerPosition
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.position + _characterController.center;
		}
	}

	public Transform ActivePlatform
	{
		get
		{
			return _activePlatform;
		}
		set
		{
			if ((Object)(object)value != (Object)(object)_activePlatform)
			{
				_activePlatform = value;
				OnActivePlatformChanged();
			}
		}
	}

	private bool ActivePlatformFollow
	{
		get
		{
			return _activePlatformFollow;
		}
		set
		{
			if (value != _activePlatformFollow)
			{
				_activePlatformFollow = value;
				OnActivePlatformFollowChanged();
			}
		}
	}

	public bool ClimbingLadder
	{
		get
		{
			return _climbingLadder;
		}
		set
		{
			_climbingLadder = value;
		}
	}

	public bool ClimbingLedge => _climbingLedge;

	public IVehicle Vehicle
	{
		get
		{
			return _vehicle;
		}
		set
		{
			if (_vehicle != value)
			{
				_vehicle = value;
				OnVehicleChanged();
			}
		}
	}

	public bool CrouchToggle
	{
		get
		{
			return _crouchToggle;
		}
		set
		{
			_crouchToggle = value;
		}
	}

	public bool ClimbAvailable
	{
		get
		{
			return _climbAvailable;
		}
		set
		{
			if (_climbAvailable != value)
			{
				_climbAvailable = value;
				OnClimbChanged(_climbAvailable);
			}
		}
	}

	IPlayer INeedPlayerComponent.Player => _player;

	public bool IsInitialized => _initialized;

	public event VehicleChangedHandler VehicleChanged;

	public event Action ClimbedLedge;

	public event Action Jumped;

	public event Action<LandType> Landed;

	public event Action BeginClimbing;

	public event Action EndClimbing;

	public event Action<bool> ClimbChanged;

	protected virtual void OnVehicleChanged()
	{
		VehicleChanged?.Invoke(this, EventArgs.Empty);
	}

	protected virtual void OnClimbedLedge()
	{
		ClimbedLedge?.Invoke();
	}

	protected virtual void OnJumped()
	{
		Jumped?.Invoke();
	}

	protected virtual void OnLanded(LandType land)
	{
		Landed?.Invoke(land);
	}

	protected virtual void OnBeginClimbing()
	{
		BeginClimbing?.Invoke();
	}

	protected virtual void OnEndClimbing()
	{
		EndClimbing?.Invoke();
	}

	protected virtual void OnClimbChanged(bool value)
	{
		ClimbChanged?.Invoke(value);
	}

	private void Awake()
	{
		_characterController = ((Component)this).GetComponent<CharacterController>();
		_characterController.radius = 0.25f;
		_characterController.slopeLimit = 60f;
		_characterController.stepOffset = 0.2f;
		_characterController.skinWidth = 0.03f;
		_characterController.detectCollisions = false;
		_ccRayDistance = _characterController.height * 0.5f + _characterController.skinWidth;
		_playerSpeed = walkSpeed;
		_jumpCooldownTimer = _jumpInterval;
	}

	public void Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
			EventManager.AddListener<OptionsAppliedEvent>(EventManager_OptionsAppliedEvent);
			EventManager.AddListener<QuickCrafterMenuOpenedEvent>(EventManager_QuickCrafterMenuOpenedEvent);
			EventManager.AddListener<CrafterMenuOpenedEvent>(EventManager_CrafterMenuOpenedEvent);
			EventManager.AddListener<ServerJoinInProgressStarted>(EventManager_ServerJoinInProgressStarted);
			LoadOptions();
		}
	}

	public override void Attached()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected Obj, but got Unknown
		base.Attached();
		((IState)MultiplayerState).SetTransforms(MultiplayerState.Transform, ((Component)this).transform);
		((IState)MultiplayerState).AddCallback("Platform", (PropertyCallbackSimple)OnPlatformChanged);
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_dragger = _player.GameInput.Dragger;
		_player.Interacter.InteractingFinished += Interacter_InteractingFinished;
	}

	private void OnDestroy()
	{
		BeginClimbing = null;
		EndClimbing = null;
		ClimbedLedge = null;
		Jumped = null;
		Landed = null;
		ClimbChanged = null;
		VehicleChanged = null;
		if (!BeamTeam.IsNullOrDestroyed(_player) && Object.op_Implicit((Object)(object)_player.Interacter))
		{
			_player.Interacter.InteractingFinished -= Interacter_InteractingFinished;
		}
		EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_OptionsAppliedEvent);
		EventManager.RemoveListener<QuickCrafterMenuOpenedEvent>(EventManager_QuickCrafterMenuOpenedEvent);
		EventManager.RemoveListener<CrafterMenuOpenedEvent>(EventManager_CrafterMenuOpenedEvent);
		EventManager.RemoveListener<ServerJoinInProgressStarted>(EventManager_ServerJoinInProgressStarted);
	}

	public void LoadOptions()
	{
		_crouchToggle = Options.InputSettings.CrouchToggle == Quality_OFF_ON.ON;
		_sprintToggle = Options.InputSettings.SprintToggle == Quality_OFF_ON.ON;
		_autoWalk = Options.InputSettings.AutoWalk == Quality_OFF_ON.ON;
		if (!_autoWalk)
		{
			_doubleTap = false;
		}
		Debug.Log((object)"Movement:: Loaded settings");
	}

	private void EventManager_OptionsAppliedEvent(OptionsAppliedEvent e)
	{
		LoadOptions();
	}

	private void EventManager_CrafterMenuOpenedEvent(CrafterMenuOpenedEvent e)
	{
		StopAutoWalk();
	}

	private void EventManager_ServerJoinInProgressStarted(ServerJoinInProgressStarted @event)
	{
		StopAutoWalk();
	}

	private void EventManager_QuickCrafterMenuOpenedEvent(QuickCrafterMenuOpenedEvent e)
	{
		StopAutoWalk();
	}

	private void Interacter_InteractingFinished()
	{
		StopAutoWalk();
	}

	public float GetHorizontalInput()
	{
		if ((bool)IsBusy)
		{
			return 0f;
		}
		return GetNormalizedInputAxis(_player.Input.GetAxis(0));
	}

	public float GetVerticalInput()
	{
		if ((bool)IsBusy)
		{
			return 0f;
		}
		if (!_doubleTap)
		{
			return GetNormalizedInputAxis(_player.Input.GetAxis(1));
		}
		return 1f;
	}

	private float GetNormalizedInputAxis(float rawInput, float deadzone = 0.25f)
	{
		return (rawInput > deadzone) ? 1 : ((rawInput < 0f - deadzone) ? (-1) : 0);
	}

	private Quaternion GetRotation()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (CheckVehicle())
		{
			Constructing_RUDDER obj = Vehicle as Constructing_RUDDER;
			Constructing_BOAT_MOTOR constructing_BOAT_MOTOR = Vehicle as Constructing_BOAT_MOTOR;
			if ((Object)(object)obj == (Object)null && (Object)(object)constructing_BOAT_MOTOR == (Object)null)
			{
				return Vehicle.VehicleMovement.GetSeatOrientation(_player).Rotation;
			}
		}
		return Quaternion.Euler(new Vector3(0f, ((Component)_player.PlayerCamera).transform.eulerAngles.y, 0f));
	}

	private bool CheckDevMode()
	{
		return GodMode;
	}

	public bool CheckVehicle()
	{
		return !BeamTeam.IsNullOrDestroyed(Vehicle);
	}

	private void DoDevMode()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.rotation = GetRotation();
		_fallStartLevel = 0f;
	}

	private void DoVehicle()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		falling = false;
		grounded = true;
		InWater = false;
		ActivePlatformFollow = false;
		_climbingLadder = false;
		activeLadder = null;
		_climbingLedge = false;
		VehicleSeatOrientation seatOrientation = Vehicle.VehicleMovement.GetSeatOrientation(_player);
		((Component)this).transform.position = seatOrientation.Position;
		((Component)this).transform.rotation = GetRotation();
		_fallStartLevel = ((Component)this).transform.position.y;
		if (Vehicle.VehicleMovement.IsOverturned())
		{
			Vehicle.VehicleMovement.Eject(_player);
			if (_player.IsOwner)
			{
				ReplicateEject replicateEject = new ReplicateEject();
				replicateEject.Player = _player;
				replicateEject.Post();
			}
		}
	}

	private void DoAutoMove()
	{
		if (_autoWalk)
		{
			if (_player.Input.GetNegativeButtonDown(1))
			{
				_doubleTap = false;
			}
			else if (_player.Input.GetButtonDown(1))
			{
				_doubleTap = Time.time < _lastTapTime + 0.2f;
				_lastTapTime = Time.time;
			}
		}
	}

	public void StopAutoWalk()
	{
		_doubleTap = false;
	}

	private void DoInput()
	{
		if ((bool)IsBusy)
		{
			ClearInput();
			return;
		}
		_inputX = GetHorizontalInput();
		_inputY = GetVerticalInput();
		_inputJumpPress = _player.Input.GetButtonDown(4);
		_inputJumpPressUp = _player.Input.GetButtonUp(4);
		_inputClimb = _player.Input.GetButton(4);
		_inputCrouchPress = _player.Input.GetButtonDown(5);
		_inputCrouch = _player.Input.GetButton(5);
		_inputSprintPress = _player.Input.GetButtonDown(6);
		_inputSprint = _player.Input.GetButton(6);
		_inputModifyFactor = 1f;
		if (Mathf.Abs(_inputX) == 1f && Mathf.Abs(_inputY) == 1f)
		{
			_inputModifyFactor = ((!Mathf.Approximately(_inputX, 0f) && !Mathf.Approximately(_inputY, 0f)) ? 0.7071f : 1f);
		}
	}

	private void ClearInput()
	{
		_inputX = 0f;
		_inputY = 0f;
		_inputJumpPress = false;
		_inputJumpPressUp = false;
		_inputClimb = false;
		_inputCrouch = false;
		_inputCrouchPress = false;
		_inputSprint = false;
		_inputSprintPress = false;
		_inputModifyFactor = 0f;
	}

	private void CalculateVelocity()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (Time.timeScale > 0f)
		{
			calculatedVelocity = (((Component)this).transform.position - _previousPosition) / DeltaTime;
			_previousPosition = ((Component)this).transform.position;
		}
		else
		{
			calculatedVelocity = Vector3.zero;
		}
	}

	private void Update()
	{
		if (_initialized && !LevelLoader.IsLoadingScene() && !CheckDevMode() && Physics.autoSimulation && _player.IsOwner && !_player.Statistics.IsKnockedOut)
		{
			DoAutoMove();
			DoInput();
			CheckInputCrouch();
			CheckInputSprint();
			CheckInputClimb();
			CheckControllerHeight();
			FollowActivePlatform();
		}
	}

	protected void LateUpdate()
	{
		if (IsInitialized)
		{
			if (_player.IsOwner)
			{
				UpdatePlatformFromTransform();
				UpdateInWaterOffsetFromTransform(InWater);
			}
			else
			{
				UpdateTransformFromPlatform();
				UpdateTransformFromInWaterOffset(InWater);
			}
		}
	}

	private void FixedUpdate()
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Invalid comparison between Unknown and I4
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		if (!_initialized || LevelLoader.IsLoadingScene() || _player == null || _player.Statistics.IsKnockedOut)
		{
			return;
		}
		if (_player != null && !_player.IsOwner)
		{
			UpdateReplicantState();
			CheckOceanHeight();
			return;
		}
		UpdateOwnerState();
		CalculateVelocity();
		if (CheckDevMode())
		{
			DoDevMode();
		}
		else if (CheckVehicle())
		{
			DoVehicle();
		}
		else
		{
			if (!Physics.autoSimulation)
			{
				return;
			}
			ClimbAvailable = (_canClimbLedge && !_climbingLedge) || (_canClimbLadder && !_climbingLadder);
			terrainGrounded = false;
			sliding = false;
			_controllerRotation = GetRotation();
			CheckOceanHeight();
			CheckInWater();
			CheckFalling();
			CheckLedge();
			DoClimbLedge();
			CheckLadder();
			if (_canClimbLadder && (_inputClimbDownTime > 0.3f || _climbingLadder))
			{
				DoClimbLadder();
			}
			else
			{
				_player.Character.CurrentCharacterPosition = _player.Character.PlayerCharacterPosition;
				if (InWater)
				{
					DoSwim();
				}
				else if (grounded)
				{
					DoMove();
				}
				else
				{
					DoFall();
				}
			}
			if (!_climbingLedge)
			{
				if (!InWater && !_climbingLadder && !_climbingLedge)
				{
					_targetVelocity.y += _gravity * DeltaTime;
				}
				FollowActivePlatform();
				grounded = (CharacterControllerMove((_targetVelocity + _extraVelocity + _extraJumpVelocity) * DeltaTime) & 4) > 0;
				_extraVelocity = Vector3.Lerp(_extraVelocity, Vector3.zero, DeltaTime * 2f);
				CheckMovingPlatform();
				if (!IsBusy)
				{
					((Component)this).transform.rotation = _controllerRotation;
				}
				_player.Character.Moving(_inputX, _inputY);
			}
		}
	}

	private CollisionFlags CharacterControllerMove(Vector3 intendedDelta)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		CollisionFlags result = _characterController.Move(intendedDelta);
		UpdateFollowActivePlatformInternalData();
		return result;
	}

	private void FollowActivePlatform()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (!BeamTeam.IsNullOrDestroyed(ActivePlatform) && ActivePlatformFollow)
		{
			((Component)this).transform.position = ActivePlatform.TransformPoint(_activeLocalPlatformPoint);
		}
	}

	private void UpdateFollowActivePlatformInternalData()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (!BeamTeam.IsNullOrDestroyed(ActivePlatform) && ActivePlatformFollow)
		{
			_activeLocalPlatformPoint = ActivePlatform.InverseTransformPoint(((Component)this).transform.position);
		}
	}

	private void DoMove()
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		_inAirTime = 0f;
		if (falling)
		{
			falling = false;
			DoLand();
		}
		CheckSliding();
		if (sliding)
		{
			Vector3 targetVelocity = new Vector3(groundNormal.x, 0f - groundNormal.y, groundNormal.z);
			Vector3.OrthoNormalize(ref groundNormal, ref targetVelocity);
			_targetVelocity = targetVelocity;
			_targetVelocity *= slideSpeed;
		}
		else
		{
			CheckSpeed();
			_targetVelocity = new Vector3(_inputX * _inputModifyFactor, 0f - _antiBumpFactor, _inputY * _inputModifyFactor);
			_targetVelocity = ((Component)this).transform.TransformDirection(_targetVelocity);
			_targetVelocity *= _playerSpeed;
			if (groundNormal.y != 0f)
			{
				_targetVelocity *= Mathf.Clamp01(groundNormal.y);
			}
		}
		if (!IsBusy)
		{
			DoCrouch();
			DoSprint();
			CheckJump();
		}
	}

	private void DoSwim()
	{
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		if (!InWater)
		{
			return;
		}
		if (crouched && !CheckCollisionUp())
		{
			crouched = false;
		}
		if (_sprintButtonDown && CanSprint && !Mathf.Approximately(_inputY, 0f) && !_player.Statistics.OutOfBreath && !CheckBrokenBones())
		{
			sprinting = true;
			ref float playerSpeed = ref _playerSpeed;
			float num;
			if (_inputY < 0f)
			{
				num = swimBackwardSpeed;
			}
			else
			{
				num = (isFloating ? swimFastSpeed : diveFastSpeed);
			}
			playerSpeed = num;
		}
		else
		{
			sprinting = (_sprintButtonDown = false);
			ref float playerSpeed2 = ref _playerSpeed;
			float num2;
			if (_inputY < 0f)
			{
				num2 = swimBackwardSpeed;
			}
			else
			{
				num2 = (isFloating ? swimSpeed : diveSpeed);
			}
			playerSpeed2 = num2;
		}
		if (CheckBrokenBones())
		{
			float playerSpeed3 = _playerSpeed * 0.5f;
			_playerSpeed = playerSpeed3;
		}
		if (isFloating)
		{
			Vector3 val = new Vector3(_inputX * _inputModifyFactor, 0f, _inputY * _inputModifyFactor);
			val = ((Component)this).transform.TransformDirection(val);
			val *= _playerSpeed;
			CheckAndIncreasePhysicalSkills(val);
			if ((_player.PlayerCamera.MouseLook.RotationY > 25f && (double)_inputY > 0.0) || _inAirTime > 1f || _jumpCooldownTimer < _jumpInterval)
			{
				val = new Vector3(_inputX * _inputModifyFactor, 0f, _inputY * _inputModifyFactor);
				val = ((Component)_player.PlayerCamera.Camera).transform.TransformDirection(val);
				val *= _playerSpeed;
			}
			else
			{
				_inAirTime = 0f;
				int num3 = 20;
				float num4 = _oceanHeight - ((Component)this).transform.position.y;
				val.y = num4 * (float)num3;
				_targetVelocity.y = num4 * (float)num3;
			}
			_targetVelocity = Vector3.Lerp(_targetVelocity, val, DeltaTime * 1.2f);
		}
		if (!isFloating)
		{
			_inAirTime = 0f;
			Vector3 val2 = new Vector3(_inputX * _inputModifyFactor, 0f, _inputY * _inputModifyFactor);
			val2 = ((Component)_player.PlayerCamera.Camera).transform.TransformDirection(val2);
			if (_inputClimb)
			{
				val2.y += 0.5f;
			}
			else if (_inputCrouch)
			{
				val2.y -= 0.5f;
			}
			val2 *= _playerSpeed;
			Vector3 velocity = _characterController.velocity;
			float num5 = Mathf.Clamp(Mathf.Abs(velocity.magnitude) * 1.5f, 1f, 20f);
			_targetVelocity = Vector3.Lerp(_targetVelocity, val2, DeltaTime * num5);
			CheckAndIncreasePhysicalSkills(val2);
		}
		if (!IsBusy)
		{
			CheckJump();
		}
		if (falling)
		{
			falling = false;
			DoLand();
		}
	}

	private void DoFall()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		_inAirTime += DeltaTime;
		if (!falling && _inAirTime > 0.3f)
		{
			falling = true;
		}
		if (!sliding && !IsBusy)
		{
			float num = _playerSpeed * 0.65f;
			_targetVelocity.x = _inputX * num * _inputModifyFactor;
			_targetVelocity.z = _inputY * num * _inputModifyFactor;
			_targetVelocity = ((Component)this).transform.TransformDirection(_targetVelocity);
		}
		if (CheckCollisionUp() && _targetVelocity.y > 0f)
		{
			_targetVelocity.y = _gravity;
		}
	}

	private void DoJump()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		_player.Character.Jump();
		crouched = false;
		if (CheckBrokenBones())
		{
			_player.Statistics.ReplicatedApplyDamage(10f, ((Component)this).transform.position + Vector3.down, "Fracture");
		}
		_extraJumpVelocity = Vector3.zero;
		if ((Object)(object)ActivePlatform != (Object)null && (Object)(object)((Component)ActivePlatform).GetComponent<Rigidbody>() != (Object)null)
		{
			_extraJumpVelocity = ((Component)ActivePlatform).GetComponent<Rigidbody>().velocity;
		}
		if (_climbingLadder)
		{
			if (!CheckCollisionUp())
			{
				_targetVelocity.y = _jumpHeight;
			}
		}
		else if (InWater)
		{
			_targetVelocity.y = _jumpHeight * 0.75f;
		}
		else
		{
			_targetVelocity.y = _jumpHeight;
		}
		OnJumped();
	}

	private void DoLand()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		_player.SetVibration(0, 0.08f, 0.1f);
		_extraJumpVelocity = Vector3.zero;
		if (_fallStartLevel < ((Component)this).transform.position.y)
		{
			return;
		}
		_player.Character.Fall_Land();
		if (InWater)
		{
			OnLanded(LandType.Water);
		}
		else
		{
			if (_climbingLedge || _climbingLadder)
			{
				return;
			}
			float num = 4f;
			float num2 = _fallStartLevel - ((Component)this).transform.position.y;
			float damage = num2 * 25f;
			if (num2 > num)
			{
				if (num2 > 6f)
				{
					ReplicatedBreakBones();
				}
				_player.Statistics.ReplicatedApplyDamage(damage, "Falling");
			}
			else
			{
				_ = 1f;
			}
			OnLanded(LandType.Ground);
		}
	}

	private void BreakBones()
	{
		_player.Statistics.ApplyStatusEffect(StatusEffect.BROKEN_BONES);
	}

	private void DoSprint()
	{
		sprinting = false;
		if (CheckBrokenBones())
		{
			return;
		}
		if (_sprintButtonDown && CanSprint && _inputY > 0f && !_player.Statistics.OutOfBreath)
		{
			_playerSpeed = runSpeed;
			sprinting = true;
			if (crouched && !CheckCollisionUp())
			{
				crouched = false;
			}
		}
		else if (!crouched)
		{
			_playerSpeed = ((_inputY < 0f) ? walkBackwardSpeed : walkSpeed);
		}
	}

	private void DoCrouch()
	{
		crouched = false;
		if (_crouchButtonDown)
		{
			_playerSpeed = crouchSpeed;
			crouched = true;
		}
	}

	private void DoClimbLadder()
	{
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		//IL_045e: Unknown result type (might be due to invalid IL or missing references)
		//IL_046e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_0490: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_050e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_054d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0552: Unknown result type (might be due to invalid IL or missing references)
		//IL_0562: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Unknown result type (might be due to invalid IL or missing references)
		//IL_0571: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		if (_climbingLadder)
		{
			if ((grounded || InWater) && _inputY < 0f)
			{
				activeLadder = null;
			}
			grounded = true;
		}
		if ((Object)(object)activeLadder == (Object)null)
		{
			ExitLadder();
			return;
		}
		if (!_climbingLadder)
		{
			OnBeginClimbing();
		}
		_climbingLadder = true;
		crouched = false;
		_inAirTime = 0f;
		sprinting = false;
		BoxCollider ladderCollider = activeLadder.LadderCollider;
		if (!_player.PlayerCamera.MouseLook.ClampLadder)
		{
			if ((Object)(object)activeLadder.LandPos != (Object)null && ((Component)this).transform.position.y > activeLadder.LandPos.position.y && !_climbingLedge)
			{
				((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, activeLadder.LandPos.position.y - 0.5f, ((Component)this).transform.position.z);
			}
			float num = Vector3.Distance(((Component)this).transform.position, ((Component)activeLadder).transform.position);
			Vector3 val = ((Component)activeLadder).transform.position + ((Component)activeLadder).transform.up * num;
			Vector3 val2 = val + new Vector3(ladderCollider.center.x, 0f, ladderCollider.center.z);
			Vector3 val3 = new Vector3(val2.x, ((Component)this).transform.position.y, val2.z) - ((Component)this).transform.position;
			val3.Normalize();
			if (!activeLadder.isTree)
			{
				val3 = ((Component)activeLadder).transform.forward;
			}
			_ladderRot = Quaternion.FromToRotation(((Component)this).transform.forward, val3) * ((Component)this).transform.rotation;
			_controllerRotation = _ladderRot;
			((Component)this).transform.rotation = _controllerRotation;
			if (activeLadder.isTree)
			{
				float num2 = Vector3.Distance(new Vector3(val.x, ((Component)this).transform.position.y, val.z), ((Component)this).transform.position);
				if (num2 > _treeSnapOffset)
				{
					Transform transform = ((Component)this).transform;
					transform.position += ((Component)this).transform.forward * (num2 - _treeSnapOffset);
				}
				else
				{
					Transform transform2 = ((Component)this).transform;
					transform2.position += -((Component)this).transform.forward * (_treeSnapOffset - num2);
				}
			}
			else
			{
				((Component)this).transform.position = new Vector3(((Component)activeLadder).transform.position.x, ((Component)this).transform.position.y, ((Component)activeLadder).transform.position.z) + -val3 * _ladderSnapOffset;
			}
			_player.Character.CurrentCharacterPosition = _player.Character.LadderHelperPosition;
			_player.Character.CurrentCharacterPosition.up = ((Component)activeLadder).transform.up;
			_player.Character.CurrentCharacterPosition.rotation = _player.Character.CurrentCharacterPosition.rotation * _ladderRot;
			_player.PlayerCamera.MouseLook.ClampLadder = true;
			_player.PlayerCamera.MouseLook.ClampViewToPos(val3);
		}
		_controllerRotation = _ladderRot;
		_playerSpeed = ladderClimbSpeed;
		_targetVelocity = _player.Character.CurrentCharacterPosition.up * (_inputY * _inputModifyFactor * _playerSpeed);
		if (activeLadder.isTree)
		{
			Vector3 val4 = ((Component)activeLadder).transform.position + ((Component)activeLadder).transform.up * ladderCollider.size.y;
			if (((Component)this).transform.position.y >= val4.y - _climbOffsetTreeEnd && _inputY > 0f)
			{
				_targetVelocity = Vector3.zero;
			}
		}
		else if ((Object)(object)activeLadder.LandPos != (Object)null && _inputY > 0f && Mathf.Abs(((Component)this).transform.position.y - activeLadder.LandPos.position.y) < 0.8f && !_climbingLedge)
		{
			ClimbLedge(activeLadder.LandPos.position + new Vector3(0f, 0.9f, 0f) + -((Component)activeLadder).transform.forward * 0.45f);
			return;
		}
		if (!InWater && !IsBusy && CheckJump())
		{
			Transform transform3 = ((Component)this).transform;
			transform3.position += -_player.Character.CurrentCharacterPosition.forward * (_treeSnapOffset * 2f);
			_playerSpeed = 4f;
			ExitLadder();
		}
		if (falling)
		{
			falling = false;
			DoLand();
		}
	}

	private void DoClimbLedge()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (_climbingLedge)
		{
			_inAirTime = 0f;
			_targetVelocity = Vector3.zero;
		}
		else if (_canClimbLedge && _inputClimbDownTime > 0.3f)
		{
			ClimbLedge(activeLedge.LandPos.position + new Vector3(0f, 0.9f, 0f) + ((Component)activeLedge).transform.forward * 0.45f, activeLedge);
		}
	}

	private void ClimbLedge(Vector3 climbPosition, Player_Ledge ledge = null)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		if (!_climbingLedge)
		{
			_ledgeClimbed = ledge;
			sprinting = false;
			Vector3 rotationAngle = Vector3.zero;
			if ((Object)(object)_ledgeClimbed != (Object)null && _ledgeClimbed.IsRaft)
			{
				((Component)_player.PlayerCamera).transform.LookAt(((Component)_ledgeClimbed).transform.position);
				((Component)_player.PlayerCamera.MouseLook).transform.LookAt(((Component)_ledgeClimbed).transform.position);
				Vector3 val = new Vector3(((Component)this).transform.position.x, ((Component)_ledgeClimbed).transform.position.y, ((Component)this).transform.position.z);
				rotationAngle = ((Component)_ledgeClimbed).transform.position - val;
				float num = 0.1f + ((Component)_ledgeClimbed).GetComponent<BoxCollider>().size.y / 2f;
				Vector3 val2 = Vector3.MoveTowards(val, ((Component)_ledgeClimbed).transform.position, 0.45f) + Vector3.up * num;
				Vector3 val3 = new Vector3(0f, 0.7f, 0f) + ((Component)_ledgeClimbed).transform.forward * 0.45f;
				climbPosition = val2 + val3;
			}
			else if ((Object)(object)_ledgeClimbed != (Object)null)
			{
				((Component)_player.PlayerCamera).transform.forward = -((Component)_ledgeClimbed).transform.forward;
				((Component)_player.PlayerCamera.MouseLook).transform.forward = -((Component)_ledgeClimbed).transform.forward;
				rotationAngle = -((Component)_ledgeClimbed).transform.forward;
			}
			else if ((Object)(object)activeLadder != (Object)null)
			{
				((Component)_player.PlayerCamera).transform.forward = ((Component)activeLadder).transform.forward;
				((Component)_player.PlayerCamera.MouseLook).transform.forward = ((Component)activeLadder).transform.forward;
				rotationAngle = ((Component)activeLadder).transform.forward;
			}
			rotationAngle.Normalize();
			_player.PlayerCamera.MouseLook.ClampLedge = true;
			_player.PlayerCamera.MouseLook.ClampViewToPos(rotationAngle);
			((Collider)_characterController).enabled = false;
			_targetVelocity = Vector3.zero;
			++IsBusy;
			_climbingLedge = true;
			crouched = false;
			InWater = false;
			isFloating = false;
			((MonoBehaviour)_player.Character).StartCoroutine(_player.Character.ClimbAnimation());
			((Component)this).transform.position = climbPosition;
			OnClimbedLedge();
		}
	}

	public void FinishLedgeClimb()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		_ = Vector3.zero;
		Vector3 val = Vector3.up * (_characterController.height / 2f);
		if ((Object)(object)activeLadder != (Object)null && _climbingLadder)
		{
			if ((Object)(object)activeLadder.LandPos != (Object)null)
			{
				((Component)this).transform.position = activeLadder.LandPos.position + val;
			}
			else
			{
				Debug.LogError((object)"Movement :: Unassigned Ladder Ledge 'Landing Position'.");
			}
			ExitLadder();
		}
		else if ((Object)(object)_ledgeClimbed != (Object)null)
		{
			if ((Object)(object)_ledgeClimbed.LandPos != (Object)null)
			{
				((Component)this).transform.position = _ledgeClimbed.LandPos.position + val;
			}
			else
			{
				Debug.LogError((object)"Movement :: Unassigned Ledge 'Landing Position'.");
			}
			_ledgeClimbed = null;
		}
		_player.PlayerCamera.MouseLook.ClampLedge = false;
		((Collider)_characterController).enabled = true;
		_targetVelocity = Vector3.zero;
		--IsBusy;
		_climbingLedge = false;
		activeLedge = null;
	}

	private void CheckSpeed()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (touchingWater && !crouched && ((Component)this).transform.position.y - 0.8f < _oceanHeight)
		{
			_playerSpeed /= 1.3f;
		}
		if (CheckBrokenBones())
		{
			float playerSpeed = walkSpeed * 0.5f;
			_playerSpeed = playerSpeed;
		}
	}

	private void CheckInputSprint()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (_sprintToggle)
		{
			if (!_sprintButtonDown && _inputSprintPress)
			{
				_sprintButtonDown = true;
			}
			else
			{
				if (!_sprintButtonDown)
				{
					return;
				}
				if (!_inputSprintPress)
				{
					Vector3 velocity = CharacterController.velocity;
					if (!(velocity.sqrMagnitude < BeamMath.Sqr(0.2f)))
					{
						return;
					}
				}
				_sprintButtonDown = false;
			}
		}
		else
		{
			_sprintButtonDown = _inputSprint;
		}
	}

	private void CheckInputCrouch()
	{
		if (_crouchToggle)
		{
			if (!_crouchButtonDown && _inputCrouchPress)
			{
				_crouchButtonDown = true;
			}
			else if (_crouchButtonDown && _inputCrouchPress)
			{
				_crouchButtonDown = false;
			}
		}
		else
		{
			_crouchButtonDown = _inputCrouch;
		}
	}

	private void CheckInputClimb()
	{
		if (_inputClimb)
		{
			_inputClimbDownTime += DeltaTime;
		}
		else
		{
			_inputClimbDownTime = 0f;
		}
	}

	private void CheckControllerHeight()
	{
		if (_climbingLedge)
		{
			if (_characterController.height != _playerControllerHeight)
			{
				SetControllerHeight(_playerControllerHeight);
			}
		}
		else if (crouched || Vehicle != null)
		{
			if (_characterController.height != _crouchControllerHeight)
			{
				SetControllerHeight(_crouchControllerHeight);
			}
		}
		else if (InWater)
		{
			if (_characterController.height != _swimHeight)
			{
				SetControllerHeight(_swimHeight);
			}
		}
		else if (!_climbingLadder && _characterController.height != _playerControllerHeight)
		{
			SetControllerHeight(_playerControllerHeight);
		}
	}

	private bool CheckJump()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (!canJump)
		{
			return false;
		}
		if (_climbingLedge)
		{
			return false;
		}
		if (_jumpCooldownTimer >= _jumpInterval)
		{
			bool flag = _inputJumpPress;
			if (_canClimbLedge || (_canClimbLadder && !_climbingLadder))
			{
				flag = _inputJumpPressUp;
			}
			if (flag && (!CheckCollisionUp() || _climbingLadder))
			{
				if (!InWater || !isFloating)
				{
					_jumpCooldownTimer = 0f;
					DoJump();
					return true;
				}
				if (Physics.SphereCast(new Ray(((Component)_player.PlayerCamera).transform.position + Vector3.up * 1.5f, Vector3.down), _characterController.radius * 2f, _playerControllerHeight + 1.5f, clipMask.value, (QueryTriggerInteraction)1))
				{
					_jumpCooldownTimer = 0f;
					DoJump();
					return true;
				}
			}
		}
		else
		{
			_jumpCooldownTimer += DeltaTime;
		}
		return false;
	}

	private void CheckSliding()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = new Vector3(groundNormal.x, 0f - groundNormal.y, groundNormal.z);
		Vector3.OrthoNormalize(ref groundNormal, ref val);
		if (TooSteep() && groundNormal.y != 0f && calculatedVelocity.magnitude > 0.2f && !Physics.Raycast(((Component)this).transform.position, val, _characterController.height * 0.5f, clipMask.value, (QueryTriggerInteraction)1))
		{
			sliding = true;
		}
		if (_debug && groundNormal.y != 0f)
		{
			Debug.DrawRay(((Component)this).transform.position, _targetVelocity, Color.red);
		}
	}

	private void CheckFalling()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (!falling && (grounded || _climbingLadder || _climbingLedge || InWater))
		{
			_fallStartLevel = ((Component)this).transform.position.y;
		}
	}

	private void CheckLedge()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		_canClimbLedge = false;
		if (!((Object)(object)activeLedge == (Object)null) && !_climbingLedge && !IsUnderwater && (activeLedge.LandPos.position + activeLedge.LandPos.up).y > ((Component)this).transform.position.y)
		{
			Vector3 forward = ((Component)this).transform.forward;
			forward.y = 0f;
			forward.Normalize();
			Vector3 val = ((Component)this).transform.position - activeLedge.LandPos.position;
			val.y = 0f;
			val.Normalize();
			if (Vector3.Dot(forward, val) < -0.2f && Vector3.Dot(((Component)activeLedge).transform.up, Vector3.up) > 0.9f)
			{
				_canClimbLedge = true;
			}
		}
	}

	private void CheckLadder()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)activeLadder == (Object)null && !_climbingLadder && canJump)
		{
			_player.PlayerCamera.MouseLook.ClampLadder = false;
		}
		_canClimbLadder = false;
		if (Object.op_Implicit((Object)(object)activeLadder) && !crouched)
		{
			if (_climbingLadder)
			{
				_canClimbLadder = true;
			}
			else
			{
				float num = Vector3.Distance(((Component)this).transform.position, ((Component)activeLadder).transform.position);
				Vector3 val = ((Component)activeLadder).transform.position + ((Component)activeLadder).transform.up * num;
				Vector3 forward = ((Component)this).transform.forward;
				forward.y = 0f;
				forward.Normalize();
				Vector3 val2 = ((Component)this).transform.position - val;
				val2.y = 0f;
				val2.Normalize();
				if (((Component)activeLadder).transform.up.y > 0.7f && Vector3.Dot(forward, val2) < -0.75f)
				{
					_canClimbLadder = true;
				}
			}
		}
		if (!_canClimbLadder)
		{
			_climbingLadder = false;
		}
	}

	private void CheckMovingPlatform()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)ActivePlatform) && (Object)(object)_previousPlatform != (Object)(object)ActivePlatform)
		{
			_previousPlatform = ((Component)ActivePlatform).GetComponent<MovingPlatform>();
		}
		bool activePlatformFollow = false;
		Transform activePlatform = null;
		_dragger.CanDrag = true;
		RaycastHit val = default;
		if (grounded && Physics.SphereCast(PlayerPosition, _characterController.radius, -((Component)this).transform.up, ref val, 2f, clipMask.value, (QueryTriggerInteraction)1) && !_climbingLedge && !_climbingLadder)
		{
			if (((Component)val.collider).gameObject.CompareTag("Terrain") && !InWater)
			{
				terrainGrounded = true;
				if (!_reachedShoreCheck)
				{
					_reachedShoreCheck = true;
					UnityEvent onReachedShore = _onReachedShore;
					if (onReachedShore != null)
					{
						onReachedShore.Invoke();
					}
					_onReachedShore = null;
				}
			}
			else if (Object.op_Implicit((Object)(object)val.rigidbody))
			{
				if (((Component)val.rigidbody).CompareTag("Raft"))
				{
					Transform transform = ((Component)val.rigidbody).transform;
					MovingPlatform component = ((Component)transform).GetComponent<MovingPlatform>();
					activePlatform = transform;
					activePlatformFollow = true;
					if (!component.Calibrated)
					{
						component.OnEnter();
					}
				}
				if (_dragger.Draggable != null && (Object)(object)_dragger.Draggable.rigidbody == (Object)(object)val.rigidbody)
				{
					_dragger.CanDrag = false;
				}
			}
		}
		ActivePlatform = activePlatform;
		ActivePlatformFollow = activePlatformFollow;
		if ((Object)(object)ActivePlatform == (Object)null && Object.op_Implicit((Object)(object)_previousPlatform) && (Object)(object)_previousPlatform != (Object)(object)ActivePlatform)
		{
			_previousPlatform.OnExit();
			_previousPlatform = null;
		}
	}

	private void CheckInWater()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		float num = ((Component)this).transform.position.y + _characterController.center.y;
		bool flag = ((Component)_player.PlayerCamera).transform.position.y <= _oceanHeight;
		bool flag2 = grounded;
		if (CheckCollisionDown())
		{
			flag2 = true;
		}
		bool inWater = false;
		if (num <= _oceanHeight)
		{
			inWater = true;
		}
		else if (flag2 && !flag)
		{
			inWater = false;
		}
		InWater = inWater;
		isFloating = false;
		if (InWater)
		{
			if (!flag)
			{
				isFloating = true;
			}
			if (_player.PlayerCamera.MouseLook.RotationY > 25f && (double)_inputY > 0.0)
			{
				isFloating = false;
			}
		}
		touchingWater = ((Component)this).transform.position.y - _playerControllerHeight * 0.5f < _oceanHeight;
	}

	private void CheckOceanHeight()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		_oceanHeight = Ceto.Ocean.Instance.QueryWaves(((Component)this).transform.position.x, ((Component)this).transform.position.z) + _playerFloatOffset;
		_oceanHeight += Mathf.Cos(Time.time * 2f) * 0.1f;
	}

	private bool CheckCollisionUp()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return Physics.SphereCast(new Ray(PlayerPosition, ((Component)this).transform.up), _characterController.radius, _ccRayDistance - _characterController.radius, clipMask.value, (QueryTriggerInteraction)1);
	}

	private bool CheckCollisionDown()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		groundedAnimal = false;
		RaycastHit val = default;
		if (Physics.Raycast(new Ray(PlayerPosition, -((Component)this).transform.up), ref val, 1f, 1 << Layers.TERRAIN_OBJECTS) && ((Component)val.collider).gameObject.CompareTag("Animal"))
		{
			groundedAnimal = true;
		}
		groundedAnimal = grounded && groundedAnimal;
		return Physics.SphereCast(new Ray(PlayerPosition, -((Component)this).transform.up), _characterController.radius, _ccRayDistance - _characterController.radius, clipMask.value, (QueryTriggerInteraction)1);
	}

	private bool CheckBrokenBones()
	{
		return _player.Statistics.HasStatusEffect(StatusEffect.BROKEN_BONES);
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		groundNormal = new Vector3(hit.normal.x, Mathf.Abs(hit.normal.y), hit.normal.z);
	}

	private void CheckAndIncreasePhysicalSkills(Vector3 velocity)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		physicalSkillCheckTimer -= DeltaTime;
		if (!(physicalSkillCheckTimer > 0f) && !(velocity == Vector3.zero))
		{
			if (physicalSkillCheckTimer <= 0f)
			{
				physicalSkillCheckTimer = 1f;
			}
			_player.PlayerSkills.ReplicatedAddPhysical(_skillExpSource);
		}
	}

	public Vector3 GetVelocity()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return calculatedVelocity * DeltaTime;
	}

	public void AddVelocity(Vector3 dir, float force)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (Vehicle == null && !_climbingLadder && !IsBusy && !_climbingLedge)
		{
			_extraVelocity += dir * force;
		}
	}

	public float GetGroundPosition(Vector3 pos, bool controllerOffset = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = pos;
		RaycastHit val2 = default;
		if (Physics.SphereCast(pos, _characterController.radius, -((Component)this).transform.up, ref val2, 2f, clipMask.value, (QueryTriggerInteraction)1))
		{
			val = val2.point;
		}
		return val.y + (controllerOffset ? (_characterController.height * 0.5f) : 0f);
	}

	private void SetControllerHeight(float height)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		transform.position += ((Component)this).transform.up * 0.01f;
		_characterController.height = height;
		_characterController.center = new Vector3(0f, (0f - (_playerControllerHeight - height)) / 2f, 0f);
	}

	private bool TooSteep()
	{
		double num = Mathf.Cos(_characterController.slopeLimit * ((float)Math.PI / 180f)) - 0.05f;
		if ((double)groundNormal.y < num)
		{
			return true;
		}
		return false;
	}

	public void ExitLadder()
	{
		_climbingLadder = false;
		activeLadder = null;
		OnEndClimbing();
	}

	public void ResetChecks()
	{
		ActivePlatform = null;
		activeLedge = null;
		activeLadder = null;
		_climbingLadder = false;
		_climbingLedge = false;
		((Collider)_characterController).enabled = true;
		IsBusy = default;
	}

	public float GetCharacterHeight()
	{
		if (_player != null && _player.IsOwner)
		{
			return CharacterController.height;
		}
		return MultiplayerState.CharacterControllerHeight;
	}

	public void IntroSceneSetup()
	{
		walkSpeed = 1f;
		walkBackwardSpeed = 0.5f;
		runSpeed = 1f;
		swimSpeed = 1f;
		diveSpeed = 1f;
		canJump = false;
	}

	private void OnPlatformChanged()
	{
		((IState)MultiplayerState).SetTransforms(MultiplayerState.Transform, BeamTeam.IsNullOrDestroyed(MultiplayerState.Platform) ? ((Component)this).transform : null);
		if (BeamTeam.IsValid(_player) && !_player.IsOwner)
		{
			UpdateTransformStartFromPlatform();
		}
	}

	private void UpdateTransformStartFromPlatform()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (!BeamTeam.IsNullOrDestroyed(_player) && !BeamTeam.IsNullOrDestroyed(MultiplayerState.Platform))
		{
			Transform transform = _player.transform;
			_platformPositionSync.SetStart(transform.position);
			_platformRotationSync.SetStart(transform.rotation);
		}
	}

	private void UpdateTransformFromPlatform()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if (BeamTeam.IsNullOrDestroyed(MultiplayerState.Platform) || BeamTeam.IsNullOrDestroyed(_player))
		{
			return;
		}
		Vector3 platformSpacePosition = MultiplayerState.PlatformSpacePosition;
		Quaternion platformSpaceRotation = MultiplayerState.PlatformSpaceRotation;
		if (!(platformSpacePosition == Vector3.zero))
		{
			Transform transform = ((Component)MultiplayerState.Platform).transform;
			Transform transform2 = _player.transform;
			Vector3 val = transform.TransformPoint(platformSpacePosition);
			Quaternion target = transform.rotation * platformSpaceRotation;
			if (!MultiplayerState.PlatformSpaceMoving)
			{
				_platformPositionSync.SetStart(val);
			}
			_platformPositionSync.Set(val);
			_platformRotationSync.Set(target);
			transform2.position = _platformPositionSync.Get();
			transform2.rotation = _platformRotationSync.Get();
		}
	}

	private void UpdateTransformFromInWaterOffset(bool inWater)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (inWater && !BeamTeam.IsNullOrDestroyed(_player))
		{
			Vector3 position = ((Component)this).transform.position;
			position.y = _oceanHeight + MultiplayerState.InWaterOffset;
			((Component)this).transform.position = position;
		}
	}

	private void UpdateReplicantState()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		InWater = MultiplayerState.InWater;
		ClimbingLadder = MultiplayerState.ClimbingLadder;
		terrainGrounded = MultiplayerState.IsTerrainGrounded;
		isFloating = MultiplayerState.IsFloating;
		grounded = MultiplayerState.IsGrounded;
		crouched = MultiplayerState.IsCrouched;
		_controllerRotation = GetRotation();
		CalculateVelocity();
		CheckMovingPlatform();
	}

	private void OnActivePlatformChanged()
	{
		if (!BeamTeam.IsNullOrDestroyed(_player) && _player.IsOwner)
		{
			if (Object.op_Implicit((Object)(object)ActivePlatform) && ActivePlatformFollow)
			{
				UpdateFollowActivePlatformInternalData();
			}
			IPlayerState multiplayerState = MultiplayerState;
			Transform activePlatform = ActivePlatform;
			multiplayerState.Platform = ((activePlatform != null) ? BeamTeam.GetInterface<BoltEntity>((Component)(object)activePlatform) : null);
			UpdatePlatformFromTransform();
		}
	}

	private void OnActivePlatformFollowChanged()
	{
		if (!BeamTeam.IsNullOrDestroyed(_player) && _player.IsOwner)
		{
			if (ActivePlatformFollow && !BeamTeam.IsNullOrDestroyed(ActivePlatform))
			{
				UpdateFollowActivePlatformInternalData();
			}
			UpdatePlatformFromTransform();
		}
	}

	private void UpdatePlatformFromTransform()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		if (!BeamTeam.IsNullOrDestroyed(_player))
		{
			if (BeamTeam.IsNullOrDestroyed(MultiplayerState.Platform))
			{
				MultiplayerState.PlatformSpacePosition = Vector3.zero;
				MultiplayerState.PlatformSpaceRotation = Quaternion.identity;
				MultiplayerState.PlatformSpaceMoving = false;
				return;
			}
			Transform transform = ((Component)MultiplayerState.Platform).transform;
			Transform transform2 = _player.transform;
			Vector3 val = ((!BeamTeam.IsNullOrDestroyed(_player.Movement.Vehicle)) ? _player.Character.CurrentCharacterPosition.position : transform2.position);
			MultiplayerState.PlatformSpacePosition = transform.InverseTransformPoint(val);
			MultiplayerState.PlatformSpaceRotation = Quaternion.Inverse(transform.rotation) * transform2.rotation;
			IPlayerState multiplayerState = MultiplayerState;
			Vector3 val2 = MultiplayerState.PlatformSpacePosition - _lastPlatformSpacePosition;
			multiplayerState.PlatformSpaceMoving = val2.sqrMagnitude > 0.0001f;
			_lastPlatformSpacePosition = MultiplayerState.PlatformSpacePosition;
		}
	}

	private void OnInWaterChanged(bool value)
	{
		if (!BeamTeam.IsNullOrDestroyed(_player) && _player.IsOwner)
		{
			UpdateInWaterOffsetFromTransform(value);
		}
	}

	private void UpdateInWaterOffsetFromTransform(bool inWater)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!BeamTeam.IsNullOrDestroyed(_player) && inWater)
		{
			MultiplayerState.InWaterOffset = ((Component)this).transform.position.y - _oceanHeight;
		}
	}

	private void UpdateOwnerState()
	{
		MultiplayerState.InWater = InWater;
		MultiplayerState.ClimbingLadder = ClimbingLadder;
		MultiplayerState.IsTerrainGrounded = terrainGrounded;
		MultiplayerState.IsFloating = isFloating;
		MultiplayerState.IsGrounded = grounded;
		MultiplayerState.IsCrouched = crouched;
		MultiplayerState.CharacterControllerHeight = CharacterController.height;
	}

	private void ReplicatedBreakBones()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateBreakBones replicateBreakBones = new ReplicateBreakBones();
			replicateBreakBones.Player = _player;
			replicateBreakBones.Post();
		}
		BreakBones();
	}

	public void ReplicatedAddVelocity(Vector3 velocityDir, float velocityForce)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (_player != null && !_player.IsOwner)
		{
			ReplicateAddVelocity replicateAddVelocity = new ReplicateAddVelocity();
			replicateAddVelocity.player = _player;
			replicateAddVelocity.velocityDir = velocityDir;
			replicateAddVelocity.velocityForce = velocityForce;
			replicateAddVelocity.Post();
		}
		_player.Movement.AddVelocity(velocityDir, velocityForce);
	}

	public void ReplicatedIsBusy(bool isBusy)
	{
		if (_player != null && !_player.IsOwner)
		{
			ReplicateIsBusy replicateIsBusy = new ReplicateIsBusy();
			replicateIsBusy.player = _player;
			replicateIsBusy.isBusy = isBusy;
			replicateIsBusy.Post();
		}
		else if (isBusy)
		{
			++IsBusy;
		}
		else
		{
			--IsBusy;
		}
	}

	public JObject Save()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		JObject jObject = new JObject();
		JObject jObject2 = JSerializer.Serialize((Component)(object)((Component)this).transform);
		if (MPExtensions.IsMaster(Game.Mode) && _player.Statistics.IsKnockedOut)
		{
			Vector3 localPosition = ((Component)this).transform.localPosition;
			localPosition += new Vector3(0f, _characterController.height / 2f, 0f);
			jObject2.SetField("localPosition", JSerializer.Serialize((object)localPosition));
		}
		jObject.AddField("Transform", jObject2);
		jObject.AddField("crouched", JSerializer.Serialize((object)crouched));
		return jObject;
	}

	public void Load(JObject data)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		JSerializer.DeserializeInto(data.GetField("Transform"), (Component)(object)((Component)this).transform);
		((Component)this).transform.localScale = Vector3.one;
		bool value = data.GetField("crouched").GetValue<bool>();
		crouched = value;
		_fallStartLevel = 0f;
		Debug.Log((object)"PlayerMovement:: Loaded.");
	}

	public Movement()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
	}
}
