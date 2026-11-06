using System.Collections;
using System.Runtime.CompilerServices;
using Beam.Utilities;
using Funlabs;
using UdpKit;
using UnityEngine;

namespace Beam;

public class FlockChild : StateMachine<FlockChild>
{
	public class ReplicateState : MultiplayerMessage
	{
		[Replicate]
		public FlockChild StateMachine;

		public FlockChildState NewState;

		[Replicate]
		public Vector3 LandingPosition;

		public override void OnPeer()
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)StateMachine))
			{
				StateMachine.ReplicatedChangeState(NewState, LandingPosition);
			}
		}

		public override void Read(UdpPacket packet)
		{
			base.Read(packet);
			NewState = (FlockChildState)packet.ReadByte();
		}

		public override void Write(UdpPacket packet)
		{
			base.Write(packet);
			packet.WriteByte((byte)NewState);
		}
	}

	private const float LANDING_DAMPING = 5f;

	private const float LANDING_SPEED_MODIFIER = 0.5f;

	private FlockController _controller;

	private LandingSpotController _landingSpotController;

	[SerializeField]
	private FlockSpecies _species;

	[SerializeField]
	private float _localWaypointSpreadRadius = 3f;

	[SerializeField]
	private float _minSpeed = 1f;

	[SerializeField]
	private float _maxSpeed = 1f;

	[SerializeField]
	private float _minRotationDamper = 1f;

	[SerializeField]
	private float _maxRotationDamper = 2f;

	[SerializeField]
	private float _minAnimationSpeed = 0.5f;

	[SerializeField]
	private float _maxAnimationSpeed = 2f;

	[SerializeField]
	private float _minIdleTime = 1f;

	[SerializeField]
	private float _maxIdleTime = 1f;

	[SerializeField]
	private float _minFlyTime = 1f;

	[SerializeField]
	private float _maxFlyTime = 1f;

	[SerializeField]
	private bool _scaredOfPlayer;

	[SerializeField]
	private float _scaringRadius;

	[Header("Animation Clips")]
	[SerializeField]
	private string _idleAnimation;

	[SerializeField]
	private string _soarAnimation;

	[SerializeField]
	private string _flapAnimation;

	[SerializeField]
	private string _landAnimation;

	[SerializeField]
	private string _takeOffAnimation;

	[SerializeField]
	private string _dieAnimation;

	private LandingSpot _landingSpot;

	private Vector3 _localWaypoint;

	private float _speed;

	[SerializeField]
	private float _targetSpeed;

	private float _damping;

	private float _idleTime;

	private float _soarTime = 5f;

	private float _landingTime;

	private float _idleTimer;

	private float _soarTimer;

	private float _flapTime = 1f;

	private float _flapTimer;

	private float _landingTimer;

	private bool _avoid = true;

	private bool _avoidUp;

	private bool _avoidDown;

	private float _flyTime;

	private float _avoidance = 0.3f;

	private float _stuck;

	private bool _takenOff;

	private bool _soaring;

	private bool _migrating;

	private bool _scared;

	private Vector3 _scaredPosition;

	private Vector3 _landingPosition;

	private static int AVOIDANCE_FRAMES_RANGE = 3;

	private static int _avoidanceGroups = 0;

	private int _doAvoidanceFrame = -1;

	private bool _hitLeft;

	private bool _hitRight;

	private bool _hitDown;

	private bool _hitUp;

	[PREFAB_PROCESSOR_GetComponentInChildren]
	[SerializeField]
	private Animation _animation;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Rigidbody _rigidbody;

	public FlockSpecies Species => _species;

	public float LocalWaypointSpreadRadius => _localWaypointSpreadRadius;

	public string DieAnimation => _dieAnimation;

	public LandingSpot LandingSpot
	{
		get
		{
			return _landingSpot;
		}
		set
		{
			_landingSpot = value;
		}
	}

	public Vector3 SpeciesWaypoint
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	private void Awake()
	{
		Reset();
		RandomizeAnimationSpeed();
		RandomizeAnimationStartingFrame();
		CurrentState = FlockChildState.Flying;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_avoidanceGroups++;
		_doAvoidanceFrame = _avoidanceGroups % AVOIDANCE_FRAMES_RANGE;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_avoidanceGroups--;
		_doAvoidanceFrame = -1;
	}

	public void Initialize(FlockController controller, LandingSpotController landingSpots)
	{
		_controller = controller;
		_landingSpotController = landingSpots;
	}

	public void Reset()
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		_targetSpeed = Random.Range(_minSpeed, _maxSpeed);
		_damping = Mathf.Lerp(_minRotationDamper, _maxRotationDamper, BeamMath.Normalize(_targetSpeed, _minSpeed, _maxSpeed));
		_idleTimer = 0f;
		_soarTimer = 0f;
		_flapTimer = 0f;
		_idleTime = Random.Range(_minIdleTime, _maxIdleTime);
		_landingTime = Random.Range(_minFlyTime, _maxFlyTime);
		_landingTimer = Random.Range(0f, _landingTime);
		_avoid = true;
		_avoidUp = true;
		_avoidDown = true;
		_takenOff = false;
		_migrating = false;
		_scared = false;
		_scaredPosition = Vector3.zero;
	}

	private void RandomizeAnimationStartingFrame()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected Obj, but got Unknown
		foreach (AnimationState item in _animation)
		{
			AnimationState val = item;
			val.time = Random.value * val.length;
		}
		_animation[_landAnimation].time = 0f;
		_animation[_takeOffAnimation].time = 0f;
		_animation[_dieAnimation].time = 0f;
	}

	private void RandomizeAnimationSpeed()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		float num = BeamMath.Normalize(_targetSpeed, _minSpeed, _maxSpeed);
		float speed = Mathf.Lerp(_minAnimationSpeed, _minAnimationSpeed, num);
		foreach (AnimationState item in _animation)
		{
			item.speed = speed;
		}
		_animation[_flapAnimation].speed = ((_species == FlockSpecies.Seagull) ? 3f : 1f);
		_animation[_landAnimation].speed = 1f;
		_animation[_takeOffAnimation].speed = 1f;
		_animation[_dieAnimation].speed = 1f;
	}

	private void RandomizeSpeed()
	{
		_targetSpeed = Random.Range(_minSpeed, _maxSpeed);
		_damping = Mathf.Lerp(_minRotationDamper, _maxRotationDamper, BeamMath.Normalize(_targetSpeed, _minSpeed, _maxSpeed));
	}

	private void Idle_Update()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		IdleAnimation();
		_idleTimer += Time.deltaTime;
		if (_idleTimer > _idleTime)
		{
			_idleTimer = 0f;
			_flapTimer = 0f;
			ReplicatedChangeState(FlockChildState.TakingOff, Vector3.zero);
		}
		if (_scaredOfPlayer)
		{
			IPlayer player = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc<IPlayer, Vector3>(PlayerRegistry.AllPlayers, CheckPlayerProximity, ((Component)this).transform.position);
			if (!BeamTeam.IsNullOrDestroyed(player))
			{
				Scare(player.transform.position);
			}
		}
	}

	private void IdleAnimation()
	{
		if ((Object)(object)_animation != (Object)null)
		{
			_animation.CrossFade(_idleAnimation, 0.7f);
		}
	}

	private IEnumerator Idle_EnterState()
	{
		_speed = 0f;
		_targetSpeed = 0f;
		if (Object.op_Implicit((Object)(object)_landingSpot))
		{
			_landingSpot.Land();
		}
		yield break;
	}

	private IEnumerator Idle_ExitState()
	{
		_idleTimer = 0f;
		yield break;
	}

	private void Flying_Update()
	{
		FlyingAnimation();
		CheckIfReachedWaypoint();
		CheckIfNeedToLand();
	}

	private void Flying_FixedUpdate()
	{
		DoMovement();
		DoAvoidance();
	}

	private void FlyingAnimation()
	{
		_flapTimer += Time.deltaTime;
		if (_flapTimer > _flapTime)
		{
			_animation.CrossFade(_soarAnimation, 0.5f);
			_soarTimer += Time.deltaTime;
			if (_soarTimer > _soarTime)
			{
				_soarTimer = 0f;
				_soarTime = Random.Range(4, 8);
				_flapTimer = 0f;
				_flapTime = Random.Range(2, 4);
			}
		}
		else
		{
			_animation.CrossFade(_flapAnimation, 0.5f);
		}
	}

	private IEnumerator Migrated_EnterState()
	{
		if (Object.op_Implicit((Object)(object)_controller))
		{
			_controller.DeactivateFlockChild(this);
		}
		yield break;
	}

	private void DoMovement()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		_speed = Mathf.Lerp(_speed, _targetSpeed, FixedTime.TimeStep(this));
		Vector3 val = _localWaypoint - _rigidbody.position;
		if (val != Vector3.zero)
		{
			Quaternion val2 = Quaternion.LookRotation(val);
			_rigidbody.rotation = Quaternion.Slerp(_rigidbody.rotation, val2, FixedTime.TimeStep(this) * _damping);
		}
		Rigidbody rigidbody = _rigidbody;
		rigidbody.position += ((Component)this).transform.forward * _speed * FixedTime.TimeStep(this);
	}

	private void DoAvoidance()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		if (_avoid && (_hitUp || _hitLeft || _hitRight || _hitDown || Time.frameCount % AVOIDANCE_FRAMES_RANGE == _doAvoidanceFrame))
		{
			Quaternion rotation = _rigidbody.rotation;
			Quaternion rotation2 = _rigidbody.rotation;
			Vector3 eulerAngles = rotation2.eulerAngles;
			Vector3 position = _rigidbody.position;
			Vector3 forward = ((Component)this).transform.forward;
			_hitRight = Physics.Raycast(position, forward + ((Component)this).transform.right * _avoidance, 5f, LayerMask.op_Implicit(_controller.AvoidanceMask));
			_hitLeft = !_hitRight && Physics.Raycast(position, forward + ((Component)this).transform.right * (0f - _avoidance), 5f, LayerMask.op_Implicit(_controller.AvoidanceMask));
			_hitDown = _avoidDown && Physics.Raycast(position, Vector3.down, 5f, LayerMask.op_Implicit(_controller.AvoidanceMask));
			_hitUp = !_hitDown && _avoidUp && Physics.Raycast(position, Vector3.up, 5f, LayerMask.op_Implicit(_controller.AvoidanceMask));
			if (_hitRight)
			{
				eulerAngles.y -= 600f * FixedTime.TimeStep(this) * _damping;
				rotation.eulerAngles = eulerAngles;
				_rigidbody.rotation = rotation;
			}
			else if (_hitLeft)
			{
				eulerAngles.y += 600f * FixedTime.TimeStep(this) * _damping;
				rotation.eulerAngles = eulerAngles;
				_rigidbody.rotation = rotation;
			}
			if (_hitDown)
			{
				eulerAngles.x -= 300f * FixedTime.TimeStep(this) * _damping;
				rotation.eulerAngles = eulerAngles;
				_rigidbody.rotation = rotation;
				position.y += 300f * FixedTime.TimeStep(this) * 0.01f;
				_rigidbody.position = position;
			}
			else if (_hitUp)
			{
				eulerAngles.x += 300f * FixedTime.TimeStep(this) * _damping;
				rotation.eulerAngles = eulerAngles;
				_rigidbody.rotation = rotation;
				position.y -= 300f * FixedTime.TimeStep(this) * 0.01f;
				_rigidbody.position = position;
			}
		}
	}

	private void CheckIfReachedWaypoint()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = _rigidbody.position - _localWaypoint;
		if (val.sqrMagnitude < BeamMath.Sqr(1f + _stuck))
		{
			if (_migrating)
			{
				ReplicatedChangeState(FlockChildState.Migrated, Vector3.zero);
				return;
			}
			SetLocalWaypoint();
			RandomizeSpeed();
			if (_takenOff)
			{
				_takenOff = false;
				_avoidUp = true;
				_avoidDown = true;
			}
			_stuck = 0f;
		}
		else
		{
			_stuck += Time.deltaTime;
		}
		val = _rigidbody.position - SpeciesWaypoint;
		if (val.sqrMagnitude < BeamMath.Sqr(2f))
		{
			_controller.SetNewWaypointForSpecies(_species, ((Component)this).transform.forward);
		}
	}

	private void CheckIfNeedToLand()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (_migrating)
		{
			return;
		}
		_landingTimer += Time.deltaTime;
		if (_landingTimer > _landingTime)
		{
			_landingSpot = _landingSpotController.FindLandingSpot(_species, _rigidbody.position, _controller.MaxLandingSpotDistance, ValidateLandingSpot);
			if (Object.op_Implicit((Object)(object)_landingSpot))
			{
				_landingSpot.FlockChild = this;
				_landingSpot.Occupied = true;
				_landingTimer = 0f;
				ReplicatedChangeState(FlockChildState.Landing, ((Component)_landingSpot).transform.position);
			}
			else
			{
				_landingTimer = 0f;
			}
		}
	}

	private bool ValidateLandingSpot(LandingSpot landingSpot)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (_scaredOfPlayer && !BeamTeam.IsNullOrDestroyed(Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc<IPlayer, Vector3>(PlayerRegistry.AllPlayers, CheckPlayerProximity, ((Component)this).transform.position)))
		{
			return false;
		}
		if (Physics.Linecast(_rigidbody.position, ((Component)landingSpot).transform.position, LayerMask.op_Implicit(_landingSpotController.AvoidanceMask)))
		{
			return false;
		}
		return true;
	}

	private void Landing_FixedUpdate()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		_avoidUp = false;
		_avoidDown = false;
		_localWaypoint = ((Component)_landingSpot).transform.position;
		Vector3 val = _rigidbody.position - ((Component)_landingSpot).transform.position;
		float magnitude = val.magnitude;
		_targetSpeed = Mathf.Max(0.1f, _maxSpeed * Mathf.Clamp01(BeamMath.Normalize(magnitude, 0f, 5f)));
		if (magnitude < 5f && magnitude > 0.3f)
		{
			_damping = 5f;
			_avoid = false;
			LandingAnimation(magnitude);
			DoMovement();
			DoAvoidance();
		}
		else if (magnitude <= 0.3f)
		{
			_damping = 5f;
			if (magnitude > 0.01f)
			{
				_targetSpeed = _minSpeed * 0.5f;
				Rigidbody rigidbody = _rigidbody;
				rigidbody.position -= (_rigidbody.position - ((Component)_landingSpot).transform.position) * _speed * FixedTime.TimeStep(this);
			}
			if (magnitude < 0.1f)
			{
				ReplicatedChangeState(FlockChildState.Idle, Vector3.zero);
			}
			Quaternion val2 = Quaternion.Lerp(_rigidbody.rotation, ((Component)_landingSpot).transform.rotation, 1f - BeamMath.Normalize(magnitude, 0f, 0.3f));
			Rigidbody rigidbody2 = _rigidbody;
			float x = val2.eulerAngles.x;
			Quaternion rotation = _rigidbody.rotation;
			rigidbody2.rotation = Quaternion.Euler(x, rotation.eulerAngles.y, val2.eulerAngles.z);
		}
		else
		{
			_damping = 1f;
			DoMovement();
			DoAvoidance();
		}
	}

	private void LandingAnimation(float distance)
	{
		if (distance > 3f)
		{
			_animation.CrossFade(_soarAnimation, 0.5f);
		}
		else
		{
			_animation.CrossFade(_landAnimation, 0.2f);
		}
	}

	private IEnumerator TakingOff_EnterState()
	{
		_takenOff = true;
		_avoid = true;
		if (Object.op_Implicit((Object)(object)_landingSpot))
		{
			_landingSpot.FlockChild = null;
			_landingSpot.Occupied = false;
		}
		if (_scared)
		{
			_speed = _maxSpeed;
			_targetSpeed = _maxSpeed;
			_damping = _maxRotationDamper;
			Vector3 val = ((Component)this).transform.position - _scaredPosition;
			val.y = 0f;
			val.Normalize();
			_rigidbody.rotation = Quaternion.LookRotation(val);
			_localWaypoint = ((Component)this).transform.position + val * 10f;
			_localWaypoint.y = 10f;
			_scared = false;
		}
		else
		{
			_speed = 0.1f;
			_targetSpeed = _maxSpeed;
			_damping = _maxRotationDamper;
			_localWaypoint = ((Component)this).transform.position + ((Component)this).transform.forward * 10f;
			_localWaypoint.y = 10f;
		}
		_animation.CrossFade(_takeOffAnimation, 0.1f);
		yield return (object)new WaitForSeconds(_animation[_takeOffAnimation].length);
		CurrentState = FlockChildState.Flying;
	}

	private void TakingOff_FixedUpdate()
	{
		DoMovement();
		DoAvoidance();
	}

	private IEnumerator Dying_EnterState()
	{
		_animation.CrossFade(_dieAnimation, 0.2f);
		_rigidbody.AddForce(((Component)this).transform.forward * _speed, (ForceMode)2);
		_rigidbody.AddTorque(((Component)this).transform.forward * -600f * _speed, (ForceMode)2);
		((Behaviour)this).enabled = false;
		yield break;
	}

	public void Kill()
	{
		if ((Object)(object)_controller != (Object)null)
		{
			_controller.RemoveChildFromFlock(this);
		}
		CurrentState = FlockChildState.Dying;
	}

	public void Scare(Vector3 position)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (CurrentState.Equals(FlockChildState.Idle))
		{
			_scared = true;
			_scaredPosition = position;
			_flapTimer = 0f;
			ReplicatedChangeState(FlockChildState.TakingOff, Vector3.zero);
		}
	}

	public void Migrate()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (CurrentState.Equals(FlockChildState.Idle))
		{
			_scared = true;
			_scaredPosition = ((Component)this).transform.position - ((Component)this).transform.forward;
			ReplicatedChangeState(FlockChildState.TakingOff, Vector3.zero);
		}
		else
		{
			Reset();
			_targetSpeed = _maxSpeed;
			ReplicatedChangeState(FlockChildState.Flying, Vector3.zero);
		}
		_migrating = true;
	}

	public void SetLocalWaypoint()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.Range(0f - _localWaypointSpreadRadius, _localWaypointSpreadRadius);
		float num2 = Random.Range(0f - _localWaypointSpreadRadius, _localWaypointSpreadRadius);
		float num3 = Random.Range(0f - _localWaypointSpreadRadius, _localWaypointSpreadRadius);
		_localWaypoint = SpeciesWaypoint + new Vector3(num, num2, num3);
	}

	protected override void ReplicantUpdate()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (CurrentState == null)
		{
			return;
		}
		if (CurrentState.Equals(FlockChildState.Idle))
		{
			IdleAnimation();
		}
		if (CurrentState.Equals(FlockChildState.Flying))
		{
			FlyingAnimation();
		}
		if (CurrentState.Equals(FlockChildState.Landing))
		{
			Vector3 val = _rigidbody.position - _landingPosition;
			float magnitude = val.magnitude;
			if (magnitude < 5f && magnitude > 0.3f)
			{
				LandingAnimation(magnitude);
			}
		}
	}

	public void ReplicatedChangeState(FlockChildState NewState, Vector3 LandingPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode))
		{
			ReplicateState replicateState = new ReplicateState();
			replicateState.StateMachine = this;
			replicateState.NewState = NewState;
			replicateState.LandingPosition = LandingPosition;
			replicateState.Post();
		}
		if (MPExtensions.IsClient(Game.Mode))
		{
			_landingPosition = LandingPosition;
		}
		CurrentState = NewState;
	}

	private bool CheckPlayerProximity(IPlayer player, Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = player.transform.position - position;
		return val.sqrMagnitude < BeamMath.Sqr(_scaringRadius);
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsClient(Game.Mode))
		{
			Gizmos.color = new Color(0.3f, 0f, 1f);
			Gizmos.DrawLine(_rigidbody.position, _localWaypoint);
			Gizmos.DrawWireSphere(_localWaypoint, 0.3f);
		}
	}
}
