using System.Collections;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam;

public class GiantCrab : LandAnimal
{
	public class ReplicateAttackPlayer : MultiplayerMessage
	{
		[Replicate]
		public LandAnimal Animal;

		[Replicate]
		public IPlayer Player;

		[Replicate]
		public bool CorrectDistance;

		public override void OnPeer()
		{
			GiantCrab giantCrab = (GiantCrab)Animal;
			if ((Object)(object)giantCrab != (Object)null && Player != null)
			{
				giantCrab.ReplicatedAttack_Player(Player, CorrectDistance);
			}
		}
	}

	private const byte ANIMATION_TAKEDAMAGE = 0;

	private const float _exploreRange = 20f;

	private const float _distance_Attack = 2.25f;

	private const float _distance_Chase = 20f;

	private bool _canAttack = true;

	private float _attackDamage;

	[SerializeField]
	private float _normalAttackDamage = 75f;

	[SerializeField]
	private float _hardAttackDamage = 90f;

	private const float walkSpeed = 0.5f;

	private const float fleeSpeed = 2.1f;

	private const float chaseSpeed = 2.5f;

	private bool _canMove;

	private float _idleTimeMax;

	private float _idleTime;

	private float _fleeTime;

	private float _chaseTime;

	private float _outOfReachTime;

	private float _cantSeePlayerTime;

	private float _lastAttackTime;

	private float _attackCooldownSeconds = 1.5f;

	private bool _stunned;

	private int attackAnimationStyle;

	private void Awake()
	{
		Setup();
		SetDifficultyParams();
		_rigidbody.mass = 1f;
	}

	private void SetDifficultyParams()
	{
		_attackDamage = _normalAttackDamage;
		if (PlayerUtilities.GetServerGameDifficulty() == EGameDifficultyMode.Hard)
		{
			_attackDamage = _hardAttackDamage;
		}
		if (PlayerUtilities.GetServerWildlifeMode() == EGameWildlifeMode.Passive)
		{
			_canAttack = false;
		}
	}

	public override void FixedTick()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		if (_rigidbody.useGravity)
		{
			_rigidbody.constraints = (RigidbodyConstraints)112;
			_rigidbody.useGravity = false;
		}
		if (!MPExtensions.IsClient(Game.Mode))
		{
			_currentSpeed = Mathf.Lerp(_currentSpeed, _adjustedSpeed, FixedTime.TimeStep(this) * 2f);
			if (_canMove)
			{
				_waypointPosition = new Vector3(_waypointPosition.x, ((Component)this).transform.position.y, _waypointPosition.z);
				CheckBounds();
				SteerAvoidance();
				MatchGroundNormal();
				Vector3 val = _waypointPosition - ((Component)this).transform.position;
				_ = val.normalized;
				Vector3 val2 = _waypointPosition - ((Component)this).transform.position;
				val2.Normalize();
				float num = Mathf.PingPong(Time.time * (_currentSpeed * 0.5f), 2f) - 1f;
				Vector3 val3 = ((Component)this).transform.right * num * 1.5f * FixedTime.TimeStep(this);
				Vector3 val4 = val2 * _currentSpeed + val3;
				val4.y = 0f;
				Vector3 val5 = val4 - _rigidbody.velocity;
				val5.y = 0f;
				_rigidbody.AddForce(val5, (ForceMode)2);
			}
			_rigidbody.AddForce(Physics.gravity, (ForceMode)5);
			_controllerVelocity = CalculateTrueVelocity();
			if (_lastAttackTime < _attackCooldownSeconds + 1f)
			{
				_lastAttackTime += FixedTime.TimeStep(this);
			}
			Animation_Move();
		}
	}

	protected override void ReplicantUpdate()
	{
		if (!LevelLoader.IsLoading())
		{
			_controllerVelocity = CalculateTrueVelocity();
			Animation_Move();
		}
	}

	private IEnumerator Idle_EnterState()
	{
		_canMove = false;
		_idleTime = 0f;
		_idleTimeMax = Random.Range(30, 120);
		_waypointPosition = ((Component)this).transform.position + ((Component)this).transform.forward;
		MatchGroundNormal();
		yield break;
	}

	private void Idle_Update()
	{
		_idleTime += FixedTime.TimeStep(this);
		if (_idleTime > _idleTimeMax)
		{
			ReplicatedChangeState(LandAnimalState.Explore);
		}
		else if (Check_Chase_Player() && CAN_SEE_PLAYER(closestPlayer))
		{
			ReplicatedChangeState(LandAnimalState.Chase);
		}
		else
		{
			SEARCH_FOR_PLAYER();
		}
	}

	private IEnumerator Explore_EnterState()
	{
		NewExploreWaypoint(20f);
		_canMove = true;
		_adjustedSpeed = 0.5f;
		_stuckWaypointTime = 0f;
		yield break;
	}

	private void Explore_Update()
	{
		if (Check_Chase_Player() && CAN_SEE_PLAYER(closestPlayer))
		{
			ReplicatedChangeState(LandAnimalState.Chase);
			return;
		}
		if (CheckWaypointDist())
		{
			if (Random.value < 0.25f)
			{
				NewExploreWaypoint(20f);
				return;
			}
			ReplicatedChangeState(LandAnimalState.Idle);
		}
		CheckIfStuck();
		SEARCH_FOR_PLAYER();
	}

	private bool Check_Chase_Player()
	{
		if (!_canAttack)
		{
			return false;
		}
		if (!BeamTeam.IsNullOrDestroyed(closestPlayer) && distanceToClosestPlayer < 20f)
		{
			CheckPlayerOutOfReach(closestPlayer);
			if (_outOfReachTime == 0f)
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator Chase_EnterState()
	{
		if (BeamTeam.IsNullOrDestroyed(closestPlayer))
		{
			ReplicatedChangeState(LandAnimalState.Idle);
			yield break;
		}
		_canMove = true;
		_adjustedSpeed = 2.5f;
		_stuckWaypointTime = 0f;
		_chaseTime = 0f;
		NewChaseWaypoint(closestPlayer.transform.position, 4f, 1.6875f);
	}

	private void Chase_Update()
	{
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		_chaseTime += FixedTime.TimeStep(this);
		if (BeamTeam.IsNullOrDestroyed(closestPlayer) || _chaseTime > 120f)
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			return;
		}
		if (distanceToClosestPlayer > 40f)
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			return;
		}
		CheckPlayerOutOfReach(closestPlayer);
		if (_outOfReachTime > 5f)
		{
			if (distanceToClosestPlayer > 20f)
			{
				ReplicatedChangeState(LandAnimalState.Explore);
			}
			else
			{
				ReplicatedChangeState(LandAnimalState.Flee);
			}
			return;
		}
		if (!CAN_SEE_PLAYER(closestPlayer))
		{
			_cantSeePlayerTime += FixedTime.TimeStep(this);
			if (_cantSeePlayerTime > 5f)
			{
				ReplicatedChangeState(LandAnimalState.Explore);
				_cantSeePlayerTime = 0f;
				return;
			}
		}
		else
		{
			_cantSeePlayerTime = 0f;
		}
		if (distanceToClosestPlayer < 2.25f)
		{
			_canMove = false;
			MatchGroundNormal();
			new Vector3(closestPlayer.transform.position.x, ((Component)this).transform.position.y, closestPlayer.transform.position.z);
			_waypointPosition = closestPlayer.transform.position;
		}
		else
		{
			_canMove = true;
			if (CheckWaypointDist())
			{
				NewChaseWaypoint(closestPlayer.transform.position, 4f, 1.6875f);
			}
			CheckIfStuck();
		}
		SEARCH_FOR_PLAYER();
		Attempt_Attack();
	}

	private IEnumerator Flee_EnterState()
	{
		NewAvoidWaypoint();
		_canMove = true;
		_fleeTime = 0f;
		_adjustedSpeed = 2.1f;
		_stuckWaypointTime = 0f;
		yield break;
	}

	private void Flee_Update()
	{
		if (CheckWaypointDist())
		{
			NewAvoidWaypoint();
		}
		_fleeTime += FixedTime.TimeStep(this);
		if (BeamTeam.IsNullOrDestroyed(closestPlayer))
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			return;
		}
		if ((distanceToClosestPlayer > 20f && _fleeTime > 10f) || _fleeTime > 25f)
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			return;
		}
		if (Check_Chase_Player() && CAN_SEE_PLAYER(closestPlayer))
		{
			ReplicatedChangeState(LandAnimalState.Chase);
			return;
		}
		CheckIfStuck();
		SEARCH_FOR_PLAYER();
		Attempt_Attack();
	}

	private void Attempt_Attack()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (!_canAttack)
		{
			return;
		}
		SEARCH_FOR_PLAYER();
		if (!BeamTeam.IsNullOrDestroyed(closestPlayer) && distanceToClosestPlayer < 2.25f && _lastAttackTime > _attackCooldownSeconds && CAN_SEE_PLAYER(closestPlayer))
		{
			Vector3 val = ((Component)closestPlayer.PlayerCamera).transform.position + -Vector3.up * 0.17f - ((Component)this).transform.position;
			RaycastHit val2 = default;
			if (!Physics.Raycast(((Component)this).transform.position, val, ref val2, val.magnitude, LayerMask.op_Implicit(_avoidanceMask), (QueryTriggerInteraction)1))
			{
				ReplicatedAttack_Player(closestPlayer);
				_lastAttackTime = 0f;
			}
		}
	}

	public void ReplicatedAttack_Player(IPlayer player, bool correctDistance = false)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		OnAttacked();
		Animation_Attack();
		if (MPExtensions.IsMaster(Game.Mode))
		{
			Vector3 val = player.transform.position - ((Component)this).transform.position;
			correctDistance = val.sqrMagnitude < BeamMath.Sqr(3f);
			ReplicateAttackPlayer replicateAttackPlayer = new ReplicateAttackPlayer();
			replicateAttackPlayer.Player = player;
			replicateAttackPlayer.Animal = this;
			replicateAttackPlayer.CorrectDistance = correctDistance;
			replicateAttackPlayer.Post();
		}
		if (correctDistance)
		{
			player.Statistics.ApplyDamage(_attackDamage, ((Component)this).transform.position, "Giant Crab Nip");
		}
	}

	private IEnumerator Dying_EnterState()
	{
		_movementCollider.enabled = false;
		((Behaviour)this).enabled = false;
		yield break;
	}

	public void TakeDamage()
	{
		if (!_stunned)
		{
			ReplicatedPlayAnimation(0);
			((MonoBehaviour)this).StartCoroutine(TakeDamageStun());
		}
		SEARCH_FOR_PLAYER();
		if (_outOfReachTime > 1f)
		{
			ReplicatedChangeState(LandAnimalState.Flee);
		}
		else if (BeamTeam.IsNullOrDestroyed(closestPlayer))
		{
			ReplicatedChangeState(LandAnimalState.Explore);
		}
		else if (!CAN_SEE_PLAYER(closestPlayer))
		{
			ReplicatedChangeState(LandAnimalState.Flee);
		}
		else if (Check_Chase_Player())
		{
			ReplicatedChangeState(LandAnimalState.Chase);
		}
		else
		{
			ReplicatedChangeState(LandAnimalState.Flee);
		}
	}

	private IEnumerator TakeDamageStun()
	{
		_stunned = true;
		float originalCurrentSpeed = _currentSpeed;
		float originalAdjustedSpeed = _adjustedSpeed;
		_currentSpeed = 0f;
		_adjustedSpeed = 0f;
		yield return (object)new WaitForSeconds(1f);
		if (originalCurrentSpeed > _currentSpeed)
		{
			_currentSpeed = originalCurrentSpeed;
		}
		if (originalAdjustedSpeed > _adjustedSpeed)
		{
			_adjustedSpeed = originalAdjustedSpeed;
		}
		yield return (object)new WaitForSeconds(2f);
		_stunned = false;
	}

	private void CheckPlayerOutOfReach(IPlayer player)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (player.Movement.ClimbingLadder)
		{
			flag = true;
		}
		if (player.transform.position.y - ((Component)this).transform.position.y > 4f)
		{
			flag = true;
		}
		if (player.transform.position.y < minExploreHeight)
		{
			flag = true;
		}
		if (player.Movement.CheckVehicle())
		{
			flag = true;
		}
		if (player.Movement.InWater)
		{
			flag = true;
		}
		if (distanceToClosestPlayer < 4.5f && player.transform.position.y - ((Component)this).transform.position.y > 1.5f)
		{
			flag = true;
		}
		if (flag)
		{
			_outOfReachTime += FixedTime.TimeStep(this);
		}
		else
		{
			_outOfReachTime = 0f;
		}
	}

	private void Animation_TakeDamage()
	{
		if (!((Object)(object)_animator == (Object)null))
		{
			_animator.SetTrigger("TakeDamage");
		}
	}

	private void Animation_Die()
	{
		if (!((Object)(object)_animator == (Object)null))
		{
			_animator.CrossFade("Die", 0.2f);
		}
	}

	private void Animation_Attack()
	{
		if (!((Object)(object)_animator == (Object)null))
		{
			if (attackAnimationStyle > 2)
			{
				attackAnimationStyle = 0;
			}
			_animator.SetFloat("AttackStyle", (float)attackAnimationStyle);
			attackAnimationStyle++;
			_animator.Play("Attack");
		}
	}

	private void Animation_Move()
	{
		if (!((Object)(object)_animator == (Object)null))
		{
			_animator.SetFloat("Speed", _controllerVelocity);
		}
	}

	protected override void PlayAnimation(byte animationId)
	{
		if (animationId == 0)
		{
			Animation_TakeDamage();
		}
	}
}
