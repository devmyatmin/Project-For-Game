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
			MultiplayerMng.Log("Add velocity piscus event");
			if (player != null && player.Movement != null)
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
			if (player != null && player.Movement != null)
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

	public IPlayerState MultiplayerState => GetMultiplayerState<IPlayerState>();

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
			if (!_characterController)
			{
				_characterController = GetComponent<CharacterController>();
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

	protected Vector3 PlayerPosition => base.transform.position + _characterController.center;

	public Transform ActivePlatform
	{
		get
		{
			return _activePlatform;
		}
		set
		{
			if (value != _activePlatform)
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
		this.VehicleChanged?.Invoke(this, EventArgs.Empty);
	}

	protected virtual void OnClimbedLedge()
	{
		this.ClimbedLedge?.Invoke();
	}

	protected virtual void OnJumped()
	{
		this.Jumped?.Invoke();
	}

	protected virtual void OnLanded(LandType land)
	{
		this.Landed?.Invoke(land);
	}

	protected virtual void OnBeginClimbing()
	{
		this.BeginClimbing?.Invoke();
	}

	protected virtual void OnEndClimbing()
	{
		this.EndClimbing?.Invoke();
	}

	protected virtual void OnClimbChanged(bool value)
	{
		this.ClimbChanged?.Invoke(value);
	}

	private void Awake()
	{
		_characterController = GetComponent<CharacterController>();
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
		base.Attached();
		MultiplayerState.SetTransforms(MultiplayerState.Transform, base.transform);
		MultiplayerState.AddCallback("Platform", OnPlatformChanged);
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_dragger = _player.GameInput.Dragger;
		_player.Interacter.InteractingFinished += Interacter_InteractingFinished;
	}

	private void OnDestroy()
	{
		this.BeginClimbing = null;
		this.EndClimbing = null;
		this.ClimbedLedge = null;
		this.Jumped = null;
		this.Landed = null;
		this.ClimbChanged = null;
		this.VehicleChanged = null;
		if (!_player.IsNullOrDestroyed() && (bool)_player.Interacter)
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
		Debug.Log("Movement:: Loaded settings");
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
		if (CheckVehicle())
		{
			Constructing_RUDDER obj = Vehicle as Constructing_RUDDER;
			Constructing_BOAT_MOTOR constructing_BOAT_MOTOR = Vehicle as Constructing_BOAT_MOTOR;
			if (obj == null && constructing_BOAT_MOTOR == null)
			{
				return Vehicle.VehicleMovement.GetSeatOrientation(_player).Rotation;
			}
		}
		return Quaternion.Euler(new Vector3(0f, _player.PlayerCamera.transform.eulerAngles.y, 0f));
	}

	private bool CheckDevMode()
	{
		return GodMode;
	}

	public bool CheckVehicle()
	{
		return !Vehicle.IsNullOrDestroyed();
	}

	private void DoDevMode()
	{
		base.transform.rotation = GetRotation();
		_fallStartLevel = 0f;
	}

	private void DoVehicle()
	{
		falling = false;
		grounded = true;
		InWater = false;
		ActivePlatformFollow = false;
		_climbingLadder = false;
		activeLadder = null;
		_climbingLedge = false;
		VehicleSeatOrientation seatOrientation = Vehicle.VehicleMovement.GetSeatOrientation(_player);
		base.transform.position = seatOrientation.Position;
		base.transform.rotation = GetRotation();
		_fallStartLevel = base.transform.position.y;
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
		if (Time.timeScale > 0f)
		{
			calculatedVelocity = (base.transform.position - _previousPosition) / DeltaTime;
			_previousPosition = base.transform.position;
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
				grounded = (CharacterControllerMove((_targetVelocity + _extraVelocity + _extraJumpVelocity) * DeltaTime) & CollisionFlags.Below) != 0;
				_extraVelocity = Vector3.Lerp(_extraVelocity, Vector3.zero, DeltaTime * 2f);
				CheckMovingPlatform();
				if (!IsBusy)
				{
					base.transform.rotation = _controllerRotation;
				}
				_player.Character.Moving(_inputX, _inputY);
			}
		}
	}

	private CollisionFlags CharacterControllerMove(Vector3 intendedDelta)
	{
		CollisionFlags result = _characterController.Move(intendedDelta);
		UpdateFollowActivePlatformInternalData();
		return result;
	}

	private void FollowActivePlatform()
	{
		if (!ActivePlatform.IsNullOrDestroyed() && ActivePlatformFollow)
		{
			base.transform.position = ActivePlatform.TransformPoint(_activeLocalPlatformPoint);
		}
	}

	private void UpdateFollowActivePlatformInternalData()
	{
		if (!ActivePlatform.IsNullOrDestroyed() && ActivePlatformFollow)
		{
			_activeLocalPlatformPoint = ActivePlatform.InverseTransformPoint(base.transform.position);
		}
	}

	private void DoMove()
	{
		_inAirTime = 0f;
		if (falling)
		{
			falling = false;
			DoLand();
		}
		CheckSliding();
		if (sliding)
		{
			Vector3 tangent = new Vector3(groundNormal.x, 0f - groundNormal.y, groundNormal.z);
			Vector3.OrthoNormalize(ref groundNormal, ref tangent);
			_targetVelocity = tangent;
			_targetVelocity *= slideSpeed;
		}
		else
		{
			CheckSpeed();
			_targetVelocity = new Vector3(_inputX * _inputModifyFactor, 0f - _antiBumpFactor, _inputY * _inputModifyFactor);
			_targetVelocity = base.transform.TransformDirection(_targetVelocity);
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
			_playerSpeed = ((_inputY < 0f) ? swimBackwardSpeed : (isFloating ? swimFastSpeed : diveFastSpeed));
		}
		else
		{
			sprinting = (_sprintButtonDown = false);
			_playerSpeed = ((_inputY < 0f) ? swimBackwardSpeed : (isFloating ? swimSpeed : diveSpeed));
		}
		if (CheckBrokenBones())
		{
			float playerSpeed = _playerSpeed * 0.5f;
			_playerSpeed = playerSpeed;
		}
		if (isFloating)
		{
			Vector3 direction = new Vector3(_inputX * _inputModifyFactor, 0f, _inputY * _inputModifyFactor);
			direction = base.transform.TransformDirection(direction);
			direction *= _playerSpeed;
			CheckAndIncreasePhysicalSkills(direction);
			if ((_player.PlayerCamera.MouseLook.RotationY > 25f && (double)_inputY > 0.0) || _inAirTime > 1f || _jumpCooldownTimer < _jumpInterval)
			{
				direction = new Vector3(_inputX * _inputModifyFactor, 0f, _inputY * _inputModifyFactor);
				direction = _player.PlayerCamera.Camera.transform.TransformDirection(direction);
				direction *= _playerSpeed;
			}
			else
			{
				_inAirTime = 0f;
				int num = 20;
				float num2 = _oceanHeight - base.transform.position.y;
				direction.y = num2 * (float)num;
				_targetVelocity.y = num2 * (float)num;
			}
			_targetVelocity = Vector3.Lerp(_targetVelocity, direction, DeltaTime * 1.2f);
		}
		if (!isFloating)
		{
			_inAirTime = 0f;
			Vector3 direction2 = new Vector3(_inputX * _inputModifyFactor, 0f, _inputY * _inputModifyFactor);
			direction2 = _player.PlayerCamera.Camera.transform.TransformDirection(direction2);
			if (_inputClimb)
			{
				direction2.y += 0.5f;
			}
			else if (_inputCrouch)
			{
				direction2.y -= 0.5f;
			}
			direction2 *= _playerSpeed;
			float num3 = Mathf.Clamp(Mathf.Abs(_characterController.velocity.magnitude) * 1.5f, 1f, 20f);
			_targetVelocity = Vector3.Lerp(_targetVelocity, direction2, DeltaTime * num3);
			CheckAndIncreasePhysicalSkills(direction2);
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
			_targetVelocity = base.transform.TransformDirection(_targetVelocity);
		}
		if (CheckCollisionUp() && _targetVelocity.y > 0f)
		{
			_targetVelocity.y = _gravity;
		}
	}

	private void DoJump()
	{
		_player.Character.Jump();
		crouched = false;
		if (CheckBrokenBones())
		{
			_player.Statistics.ReplicatedApplyDamage(10f, base.transform.position + Vector3.down, "Fracture");
		}
		_extraJumpVelocity = Vector3.zero;
		if (ActivePlatform != null && ActivePlatform.GetComponent<Rigidbody>() != null)
		{
			_extraJumpVelocity = ActivePlatform.GetComponent<Rigidbody>().velocity;
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
		_player.SetVibration(0, 0.08f, 0.1f);
		_extraJumpVelocity = Vector3.zero;
		if (_fallStartLevel < base.transform.position.y)
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
			float num2 = _fallStartLevel - base.transform.position.y;
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
		if (_climbingLadder)
		{
			if ((grounded || InWater) && _inputY < 0f)
			{
				activeLadder = null;
			}
			grounded = true;
		}
		if (activeLadder == null)
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
			if (activeLadder.LandPos != null && base.transform.position.y > activeLadder.LandPos.position.y && !_climbingLedge)
			{
				base.transform.position = new Vector3(base.transform.position.x, activeLadder.LandPos.position.y - 0.5f, base.transform.position.z);
			}
			float num = Vector3.Distance(base.transform.position, activeLadder.transform.position);
			Vector3 vector = activeLadder.transform.position + activeLadder.transform.up * num;
			Vector3 vector2 = vector + new Vector3(ladderCollider.center.x, 0f, ladderCollider.center.z);
			Vector3 vector3 = new Vector3(vector2.x, base.transform.position.y, vector2.z) - base.transform.position;
			vector3.Normalize();
			if (!activeLadder.isTree)
			{
				vector3 = activeLadder.transform.forward;
			}
			_ladderRot = Quaternion.FromToRotation(base.transform.forward, vector3) * base.transform.rotation;
			_controllerRotation = _ladderRot;
			base.transform.rotation = _controllerRotation;
			if (activeLadder.isTree)
			{
				float num2 = Vector3.Distance(new Vector3(vector.x, base.transform.position.y, vector.z), base.transform.position);
				if (num2 > _treeSnapOffset)
				{
					base.transform.position += base.transform.forward * (num2 - _treeSnapOffset);
				}
				else
				{
					base.transform.position += -base.transform.forward * (_treeSnapOffset - num2);
				}
			}
			else
			{
				base.transform.position = new Vector3(activeLadder.transform.position.x, base.transform.position.y, activeLadder.transform.position.z) + -vector3 * _ladderSnapOffset;
			}
			_player.Character.CurrentCharacterPosition = _player.Character.LadderHelperPosition;
			_player.Character.CurrentCharacterPosition.up = activeLadder.transform.up;
			_player.Character.CurrentCharacterPosition.rotation = _player.Character.CurrentCharacterPosition.rotation * _ladderRot;
			_player.PlayerCamera.MouseLook.ClampLadder = true;
			_player.PlayerCamera.MouseLook.ClampViewToPos(vector3);
		}
		_controllerRotation = _ladderRot;
		_playerSpeed = ladderClimbSpeed;
		_targetVelocity = _player.Character.CurrentCharacterPosition.up * (_inputY * _inputModifyFactor * _playerSpeed);
		if (activeLadder.isTree)
		{
			Vector3 vector4 = activeLadder.transform.position + activeLadder.transform.up * ladderCollider.size.y;
			if (base.transform.position.y >= vector4.y - _climbOffsetTreeEnd && _inputY > 0f)
			{
				_targetVelocity = Vector3.zero;
			}
		}
		else if (activeLadder.LandPos != null && _inputY > 0f && Mathf.Abs(base.transform.position.y - activeLadder.LandPos.position.y) < 0.8f && !_climbingLedge)
		{
			ClimbLedge(activeLadder.LandPos.position + new Vector3(0f, 0.9f, 0f) + -activeLadder.transform.forward * 0.45f);
			return;
		}
		if (!InWater && !IsBusy && CheckJump())
		{
			base.transform.position += -_player.Character.CurrentCharacterPosition.forward * (_treeSnapOffset * 2f);
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
		if (_climbingLedge)
		{
			_inAirTime = 0f;
			_targetVelocity = Vector3.zero;
		}
		else if (_canClimbLedge && _inputClimbDownTime > 0.3f)
		{
			ClimbLedge(activeLedge.LandPos.position + new Vector3(0f, 0.9f, 0f) + activeLedge.transform.forward * 0.45f, activeLedge);
		}
	}

	private void ClimbLedge(Vector3 climbPosition, Player_Ledge ledge = null)
	{
		if (!_climbingLedge)
		{
			_ledgeClimbed = ledge;
			sprinting = false;
			Vector3 rotationAngle = Vector3.zero;
			if (_ledgeClimbed != null && _ledgeClimbed.IsRaft)
			{
				_player.PlayerCamera.transform.LookAt(_ledgeClimbed.transform.position);
				_player.PlayerCamera.MouseLook.transform.LookAt(_ledgeClimbed.transform.position);
				Vector3 vector = new Vector3(base.transform.position.x, _ledgeClimbed.transform.position.y, base.transform.position.z);
				rotationAngle = _ledgeClimbed.transform.position - vector;
				float num = 0.1f + _ledgeClimbed.GetComponent<BoxCollider>().size.y / 2f;
				Vector3 vector2 = Vector3.MoveTowards(vector, _ledgeClimbed.transform.position, 0.45f) + Vector3.up * num;
				Vector3 vector3 = new Vector3(0f, 0.7f, 0f) + _ledgeClimbed.transform.forward * 0.45f;
				climbPosition = vector2 + vector3;
			}
			else if (_ledgeClimbed != null)
			{
				_player.PlayerCamera.transform.forward = -_ledgeClimbed.transform.forward;
				_player.PlayerCamera.MouseLook.transform.forward = -_ledgeClimbed.transform.forward;
				rotationAngle = -_ledgeClimbed.transform.forward;
			}
			else if (activeLadder != null)
			{
				_player.PlayerCamera.transform.forward = activeLadder.transform.forward;
				_player.PlayerCamera.MouseLook.transform.forward = activeLadder.transform.forward;
				rotationAngle = activeLadder.transform.forward;
			}
			rotationAngle.Normalize();
			_player.PlayerCamera.MouseLook.ClampLedge = true;
			_player.PlayerCamera.MouseLook.ClampViewToPos(rotationAngle);
			_characterController.enabled = false;
			_targetVelocity = Vector3.zero;
			++IsBusy;
			_climbingLedge = true;
			crouched = false;
			InWater = false;
			isFloating = false;
			_player.Character.StartCoroutine(_player.Character.ClimbAnimation());
			base.transform.position = climbPosition;
			OnClimbedLedge();
		}
	}

	public void FinishLedgeClimb()
	{
		_ = Vector3.zero;
		Vector3 vector = Vector3.up * (_characterController.height / 2f);
		if (activeLadder != null && _climbingLadder)
		{
			if (activeLadder.LandPos != null)
			{
				base.transform.position = activeLadder.LandPos.position + vector;
			}
			else
			{
				Debug.LogError("Movement :: Unassigned Ladder Ledge 'Landing Position'.");
			}
			ExitLadder();
		}
		else if (_ledgeClimbed != null)
		{
			if (_ledgeClimbed.LandPos != null)
			{
				base.transform.position = _ledgeClimbed.LandPos.position + vector;
			}
			else
			{
				Debug.LogError("Movement :: Unassigned Ledge 'Landing Position'.");
			}
			_ledgeClimbed = null;
		}
		_player.PlayerCamera.MouseLook.ClampLedge = false;
		_characterController.enabled = true;
		_targetVelocity = Vector3.zero;
		--IsBusy;
		_climbingLedge = false;
		activeLedge = null;
	}

	private void CheckSpeed()
	{
		if (touchingWater && !crouched && base.transform.position.y - 0.8f < _oceanHeight)
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
		if (_sprintToggle)
		{
			if (!_sprintButtonDown && _inputSprintPress)
			{
				_sprintButtonDown = true;
			}
			else if (_sprintButtonDown && (_inputSprintPress || CharacterController.velocity.sqrMagnitude < BeamMath.Sqr(0.2f)))
			{
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
				if (Physics.SphereCast(new Ray(_player.PlayerCamera.transform.position + Vector3.up * 1.5f, Vector3.down), _characterController.radius * 2f, _playerControllerHeight + 1.5f, clipMask.value, QueryTriggerInteraction.Ignore))
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
		Vector3 tangent = new Vector3(groundNormal.x, 0f - groundNormal.y, groundNormal.z);
		Vector3.OrthoNormalize(ref groundNormal, ref tangent);
		if (TooSteep() && groundNormal.y != 0f && calculatedVelocity.magnitude > 0.2f && !Physics.Raycast(base.transform.position, tangent, _characterController.height * 0.5f, clipMask.value, QueryTriggerInteraction.Ignore))
		{
			sliding = true;
		}
		if (_debug && groundNormal.y != 0f)
		{
			Debug.DrawRay(base.transform.position, _targetVelocity, Color.red);
		}
	}

	private void CheckFalling()
	{
		if (!falling && (grounded || _climbingLadder || _climbingLedge || InWater))
		{
			_fallStartLevel = base.transform.position.y;
		}
	}

	private void CheckLedge()
	{
		_canClimbLedge = false;
		if (!(activeLedge == null) && !_climbingLedge && !IsUnderwater && (activeLedge.LandPos.position + activeLedge.LandPos.up).y > base.transform.position.y)
		{
			Vector3 forward = base.transform.forward;
			forward.y = 0f;
			forward.Normalize();
			Vector3 rhs = base.transform.position - activeLedge.LandPos.position;
			rhs.y = 0f;
			rhs.Normalize();
			if (Vector3.Dot(forward, rhs) < -0.2f && Vector3.Dot(activeLedge.transform.up, Vector3.up) > 0.9f)
			{
				_canClimbLedge = true;
			}
		}
	}

	private void CheckLadder()
	{
		if (activeLadder == null && !_climbingLadder && canJump)
		{
			_player.PlayerCamera.MouseLook.ClampLadder = false;
		}
		_canClimbLadder = false;
		if ((bool)activeLadder && !crouched)
		{
			if (_climbingLadder)
			{
				_canClimbLadder = true;
			}
			else
			{
				float num = Vector3.Distance(base.transform.position, activeLadder.transform.position);
				Vector3 vector = activeLadder.transform.position + activeLadder.transform.up * num;
				Vector3 forward = base.transform.forward;
				forward.y = 0f;
				forward.Normalize();
				Vector3 rhs = base.transform.position - vector;
				rhs.y = 0f;
				rhs.Normalize();
				if (activeLadder.transform.up.y > 0.7f && Vector3.Dot(forward, rhs) < -0.75f)
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
		if ((bool)ActivePlatform && _previousPlatform != ActivePlatform)
		{
			_previousPlatform = ActivePlatform.GetComponent<MovingPlatform>();
		}
		bool activePlatformFollow = false;
		Transform activePlatform = null;
		_dragger.CanDrag = true;
		if (grounded && Physics.SphereCast(PlayerPosition, _characterController.radius, -base.transform.up, out var hitInfo, 2f, clipMask.value, QueryTriggerInteraction.Ignore) && !_climbingLedge && !_climbingLadder)
		{
			if (hitInfo.collider.gameObject.CompareTag("Terrain") && !InWater)
			{
				terrainGrounded = true;
				if (!_reachedShoreCheck)
				{
					_reachedShoreCheck = true;
					_onReachedShore?.Invoke();
					_onReachedShore = null;
				}
			}
			else if ((bool)hitInfo.rigidbody)
			{
				if (hitInfo.rigidbody.CompareTag("Raft"))
				{
					Transform obj = hitInfo.rigidbody.transform;
					MovingPlatform component = obj.GetComponent<MovingPlatform>();
					activePlatform = obj;
					activePlatformFollow = true;
					if (!component.Calibrated)
					{
						component.OnEnter();
					}
				}
				if (_dragger.Draggable != null && _dragger.Draggable.rigidbody == hitInfo.rigidbody)
				{
					_dragger.CanDrag = false;
				}
			}
		}
		ActivePlatform = activePlatform;
		ActivePlatformFollow = activePlatformFollow;
		if (ActivePlatform == null && (bool)_previousPlatform && _previousPlatform != ActivePlatform)
		{
			_previousPlatform.OnExit();
			_previousPlatform = null;
		}
	}

	private void CheckInWater()
	{
		float num = base.transform.position.y + _characterController.center.y;
		bool flag = _player.PlayerCamera.transform.position.y <= _oceanHeight;
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
		touchingWater = base.transform.position.y - _playerControllerHeight * 0.5f < _oceanHeight;
	}

	private void CheckOceanHeight()
	{
		_oceanHeight = Ceto.Ocean.Instance.QueryWaves(base.transform.position.x, base.transform.position.z) + _playerFloatOffset;
		_oceanHeight += Mathf.Cos(Time.time * 2f) * 0.1f;
	}

	private bool CheckCollisionUp()
	{
		return Physics.SphereCast(new Ray(PlayerPosition, base.transform.up), _characterController.radius, _ccRayDistance - _characterController.radius, clipMask.value, QueryTriggerInteraction.Ignore);
	}

	private bool CheckCollisionDown()
	{
		groundedAnimal = false;
		if (Physics.Raycast(new Ray(PlayerPosition, -base.transform.up), out var hitInfo, 1f, 1 << Layers.TERRAIN_OBJECTS) && hitInfo.collider.gameObject.CompareTag("Animal"))
		{
			groundedAnimal = true;
		}
		groundedAnimal = grounded && groundedAnimal;
		return Physics.SphereCast(new Ray(PlayerPosition, -base.transform.up), _characterController.radius, _ccRayDistance - _characterController.radius, clipMask.value, QueryTriggerInteraction.Ignore);
	}

	private bool CheckBrokenBones()
	{
		return _player.Statistics.HasStatusEffect(StatusEffect.BROKEN_BONES);
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		groundNormal = new Vector3(hit.normal.x, Mathf.Abs(hit.normal.y), hit.normal.z);
	}

	private void CheckAndIncreasePhysicalSkills(Vector3 velocity)
	{
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
		return calculatedVelocity * DeltaTime;
	}

	public void AddVelocity(Vector3 dir, float force)
	{
		if (Vehicle == null && !_climbingLadder && !IsBusy && !_climbingLedge)
		{
			_extraVelocity += dir * force;
		}
	}

	public float GetGroundPosition(Vector3 pos, bool controllerOffset = false)
	{
		Vector3 vector = pos;
		if (Physics.SphereCast(pos, _characterController.radius, -base.transform.up, out var hitInfo, 2f, clipMask.value, QueryTriggerInteraction.Ignore))
		{
			vector = hitInfo.point;
		}
		return vector.y + (controllerOffset ? (_characterController.height * 0.5f) : 0f);
	}

	private void SetControllerHeight(float height)
	{
		base.transform.position += base.transform.up * 0.01f;
		_characterController.height = height;
		_characterController.center = new Vector3(0f, (0f - (_playerControllerHeight - height)) / 2f, 0f);
	}

	private bool TooSteep()
	{
		double num = Mathf.Cos(_characterController.slopeLimit * (MathF.PI / 180f)) - 0.05f;
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
		_characterController.enabled = true;
		IsBusy = default(Condition);
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
		MultiplayerState.SetTransforms(MultiplayerState.Transform, MultiplayerState.Platform.IsNullOrDestroyed() ? base.transform : null);
		if (_player.IsValid() && !_player.IsOwner)
		{
			UpdateTransformStartFromPlatform();
		}
	}

	private void UpdateTransformStartFromPlatform()
	{
		if (!_player.IsNullOrDestroyed() && !MultiplayerState.Platform.IsNullOrDestroyed())
		{
			Transform transform = _player.transform;
			_platformPositionSync.SetStart(transform.position);
			_platformRotationSync.SetStart(transform.rotation);
		}
	}

	private void UpdateTransformFromPlatform()
	{
		if (MultiplayerState.Platform.IsNullOrDestroyed() || _player.IsNullOrDestroyed())
		{
			return;
		}
		Vector3 platformSpacePosition = MultiplayerState.PlatformSpacePosition;
		Quaternion platformSpaceRotation = MultiplayerState.PlatformSpaceRotation;
		if (!(platformSpacePosition == Vector3.zero))
		{
			Transform obj = MultiplayerState.Platform.transform;
			Transform transform = _player.transform;
			Vector3 vector = obj.TransformPoint(platformSpacePosition);
			Quaternion target = obj.rotation * platformSpaceRotation;
			if (!MultiplayerState.PlatformSpaceMoving)
			{
				_platformPositionSync.SetStart(vector);
			}
			_platformPositionSync.Set(vector);
			_platformRotationSync.Set(target);
			transform.position = _platformPositionSync.Get();
			transform.rotation = _platformRotationSync.Get();
		}
	}

	private void UpdateTransformFromInWaterOffset(bool inWater)
	{
		if (inWater && !_player.IsNullOrDestroyed())
		{
			Vector3 position = base.transform.position;
			position.y = _oceanHeight + MultiplayerState.InWaterOffset;
			base.transform.position = position;
		}
	}

	private void UpdateReplicantState()
	{
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
		if (!_player.IsNullOrDestroyed() && _player.IsOwner)
		{
			if ((bool)ActivePlatform && ActivePlatformFollow)
			{
				UpdateFollowActivePlatformInternalData();
			}
			MultiplayerState.Platform = ActivePlatform?.GetInterface<BoltEntity>();
			UpdatePlatformFromTransform();
		}
	}

	private void OnActivePlatformFollowChanged()
	{
		if (!_player.IsNullOrDestroyed() && _player.IsOwner)
		{
			if (ActivePlatformFollow && !ActivePlatform.IsNullOrDestroyed())
			{
				UpdateFollowActivePlatformInternalData();
			}
			UpdatePlatformFromTransform();
		}
	}

	private void UpdatePlatformFromTransform()
	{
		if (!_player.IsNullOrDestroyed())
		{
			if (MultiplayerState.Platform.IsNullOrDestroyed())
			{
				MultiplayerState.PlatformSpacePosition = Vector3.zero;
				MultiplayerState.PlatformSpaceRotation = Quaternion.identity;
				MultiplayerState.PlatformSpaceMoving = false;
				return;
			}
			Transform transform = MultiplayerState.Platform.transform;
			Transform transform2 = _player.transform;
			Vector3 position = ((!_player.Movement.Vehicle.IsNullOrDestroyed()) ? _player.Character.CurrentCharacterPosition.position : transform2.position);
			MultiplayerState.PlatformSpacePosition = transform.InverseTransformPoint(position);
			MultiplayerState.PlatformSpaceRotation = Quaternion.Inverse(transform.rotation) * transform2.rotation;
			MultiplayerState.PlatformSpaceMoving = (MultiplayerState.PlatformSpacePosition - _lastPlatformSpacePosition).sqrMagnitude > 0.0001f;
			_lastPlatformSpacePosition = MultiplayerState.PlatformSpacePosition;
		}
	}

	private void OnInWaterChanged(bool value)
	{
		if (!_player.IsNullOrDestroyed() && _player.IsOwner)
		{
			UpdateInWaterOffsetFromTransform(value);
		}
	}

	private void UpdateInWaterOffsetFromTransform(bool inWater)
	{
		if (!_player.IsNullOrDestroyed() && inWater)
		{
			MultiplayerState.InWaterOffset = base.transform.position.y - _oceanHeight;
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
		if (Game.Mode.IsMultiplayer())
		{
			ReplicateBreakBones replicateBreakBones = new ReplicateBreakBones();
			replicateBreakBones.Player = _player;
			replicateBreakBones.Post();
		}
		BreakBones();
	}

	public void ReplicatedAddVelocity(Vector3 velocityDir, float velocityForce)
	{
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
		JObject jObject = new JObject();
		JObject jObject2 = JSerializer.Serialize(base.transform);
		if (Game.Mode.IsMaster() && _player.Statistics.IsKnockedOut)
		{
			Vector3 localPosition = base.transform.localPosition;
			localPosition += new Vector3(0f, _characterController.height / 2f, 0f);
			jObject2.SetField("localPosition", JSerializer.Serialize(localPosition));
		}
		jObject.AddField("Transform", jObject2);
		jObject.AddField("crouched", JSerializer.Serialize(crouched));
		return jObject;
	}

	public void Load(JObject data)
	{
		JSerializer.DeserializeInto(data.GetField("Transform"), base.transform);
		base.transform.localScale = Vector3.one;
		bool value = data.GetField("crouched").GetValue<bool>();
		crouched = value;
		_fallStartLevel = 0f;
		Debug.Log("PlayerMovement:: Loaded.");
	}
}
