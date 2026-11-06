using System;
using System.Collections;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam;

public class Snake : LandAnimal
{
	public class ReplicatePlayRattle : MultiplayerMessage
	{
		[Replicate]
		public LandAnimal animal;

		public override void OnPeer()
		{
			Snake snake = (Snake)animal;
			if ((Object)(object)snake != (Object)null)
			{
				snake.ReplicatedPlayRattleNoise();
			}
		}
	}

	public class ReplicateAttack_Bite : MultiplayerMessage
	{
		[Replicate]
		public LandAnimal Animal;

		[Replicate]
		public IPlayer Player;

		[Replicate]
		public bool CorrectDistance;

		public override void OnPeer()
		{
			Snake snake = (Snake)Animal;
			if ((Object)(object)snake != (Object)null && Player != null)
			{
				snake.ReplicatedAttack_Bite(Player, CorrectDistance);
			}
		}
	}

	[SerializeField]
	private Transform _head;

	private const float _exploreRange = 20f;

	private const float _distance_Attack = 1.5f;

	private const float _distance_AttackStance = 6.5f;

	private const float _distance_AvoidPlayer = 10f;

	private const float _distance_FleePlayer = 8f;

	private bool _canAttack = true;

	private float _attackDamage;

	[SerializeField]
	private float _normalAttackDamage = 75f;

	[SerializeField]
	private float _hardAttackDamage = 90f;

	private const float walkSpeed = 0.5f;

	private const float fleeSpeed = 2.1f;

	private bool _canMove;

	private float _currentIdleTime;

	private float _idleTime;

	private float _lastAttackTime;

	private float _attackCooldownSeconds = 5f;

	private float _headRaise;

	private float _headRaiseLerped;

	private float _lastRattle = 100f;

	private float _rattleTimer;

	private float _currentHidingTime;

	private float _chaseTimer;

	private float _fleeTime;

	public event Action Rattled;

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

	private void OnDestroy()
	{
		Rattled = null;
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
			if (_lastRattle < _rattleTimer + 1f)
			{
				_lastRattle += FixedTime.TimeStep(this);
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
		_currentIdleTime = 0f;
		_idleTime = Random.Range(30, 120);
		_waypointPosition = ((Component)this).transform.position + ((Component)this).transform.forward;
		MatchGroundNormal();
		yield break;
	}

	private void Idle_Update()
	{
		_currentIdleTime += FixedTime.TimeStep(this);
		if (_currentIdleTime > _idleTime)
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			return;
		}
		if (!BeamTeam.IsNullOrDestroyed(closestPlayer) && distanceToClosestPlayer < 10f)
		{
			ReplicatedChangeState(LandAnimalState.Explore);
		}
		SEARCH_FOR_PLAYER();
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
		_adjustedSpeed = 0.5f;
		if (!BeamTeam.IsNullOrDestroyed(closestPlayer) && distanceToClosestPlayer < 10f)
		{
			_adjustedSpeed = 0.1f;
			if (CheckWaypointDist())
			{
				NewAvoidWaypoint();
			}
			if (distanceToClosestPlayer < 8f)
			{
				ReplicatedChangeState(LandAnimalState.Flee);
				return;
			}
		}
		if (distanceToClosestPlayer > 10f && CheckWaypointDist())
		{
			float value = Random.value;
			if (value < 0.25f)
			{
				NewExploreWaypoint(20f);
				return;
			}
			if (value < 0.45f && FindHidingSpot())
			{
				ReplicatedChangeState(LandAnimalState.Hide);
				return;
			}
			ReplicatedChangeState(LandAnimalState.Idle);
		}
		CheckIfStuck();
		SEARCH_FOR_PLAYER();
	}

	private IEnumerator Hide_EnterState()
	{
		NewHideWaypoint();
		_canMove = true;
		_stuckWaypointTime = 0f;
		_currentHidingTime = 0f;
		_adjustedSpeed = 2.1f;
		yield break;
	}

	private void Hide_Update()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_hidingspot == (Object)null)
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			return;
		}
		_currentHidingTime += FixedTime.TimeStep(this);
		if (_currentHidingTime > 30f)
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			return;
		}
		Vector2 val = new Vector2(((Component)_hidingspot).transform.position.x, ((Component)_hidingspot).transform.position.z) - new Vector2(((Component)this).transform.position.x, ((Component)this).transform.position.z);
		if (val.magnitude < 0.75f)
		{
			ReplicatedChangeState(LandAnimalState.Idle);
		}
		else if (CheckWaypointDist())
		{
			NewHideWaypoint();
		}
	}

	private IEnumerator Hide_ExitState()
	{
		if ((Object)(object)_hidingspot != (Object)null)
		{
			_hidingspot.HideChild = null;
			_hidingspot.Occupied = false;
			_hidingspot = null;
		}
		yield break;
	}

	private IEnumerator Chase_EnterState()
	{
		if (BeamTeam.IsNullOrDestroyed(closestPlayer))
		{
			ReplicatedChangeState(LandAnimalState.Idle);
			yield break;
		}
		_canMove = true;
		_adjustedSpeed = 2.1f;
		_stuckWaypointTime = 0f;
		_chaseTimer = 0f;
	}

	private void Chase_Update()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		_chaseTimer += FixedTime.TimeStep(this);
		if (BeamTeam.IsNullOrDestroyed(closestPlayer) || _chaseTimer > 20f)
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			return;
		}
		if (distanceToClosestPlayer > 20f)
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			return;
		}
		if (distanceToClosestPlayer < 1.5f)
		{
			ReplicatedChangeState(LandAnimalState.AttackStance);
			return;
		}
		if (CheckWaypointDist())
		{
			NewChaseWaypoint(closestPlayer.transform.position);
		}
		CheckIfStuck();
		SEARCH_FOR_PLAYER();
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
		if (_canAttack && distanceToClosestPlayer < 6.5f && _lastAttackTime > _attackCooldownSeconds)
		{
			ReplicatedChangeState(LandAnimalState.AttackStance);
		}
		if ((distanceToClosestPlayer > 20f && _fleeTime > 10f) || _fleeTime > 30f)
		{
			ReplicatedChangeState((!FindHidingSpot()) ? LandAnimalState.Explore : LandAnimalState.Hide);
		}
		CheckIfStuck();
		SEARCH_FOR_PLAYER();
	}

	private IEnumerator AttackStance_EnterState()
	{
		_canMove = false;
		_headRaise = 1f;
		_lastAttackTime = _attackCooldownSeconds;
		MatchGroundNormal();
		yield break;
	}

	private void ReplicatedPlayRattleNoise()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode))
		{
			ReplicatePlayRattle replicatePlayRattle = new ReplicatePlayRattle();
			replicatePlayRattle.animal = this;
			replicatePlayRattle.Post();
		}
		Rattled?.Invoke();
	}

	private void AttackStance_Update()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		if (BeamTeam.IsNullOrDestroyed(closestPlayer))
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			return;
		}
		if (distanceToClosestPlayer > 10f)
		{
			ReplicatedChangeState(LandAnimalState.Idle);
			return;
		}
		if (distanceToClosestPlayer < 1.5f && _lastAttackTime > _attackCooldownSeconds)
		{
			Vector3 val = ((Component)closestPlayer.PlayerCamera).transform.position + -Vector3.up * 0.17f - _head.position;
			RaycastHit val2 = default;
			if (!Physics.Raycast(((Component)this).transform.position, val, ref val2, val.magnitude, LayerMask.op_Implicit(_avoidanceMask), (QueryTriggerInteraction)1))
			{
				ReplicatedAttack_Bite(closestPlayer);
				((MonoBehaviour)this).StartCoroutine(AttackFleeDelay());
				_lastAttackTime = 0f;
				return;
			}
		}
		if (distanceToClosestPlayer < 6.5f && _lastRattle > _rattleTimer)
		{
			_lastRattle = 0f;
			_rattleTimer = Random.Range(3, 9);
			ReplicatedPlayRattleNoise();
		}
		_headRaise = ((distanceToClosestPlayer < 10f && distanceToClosestPlayer > 2f) ? 1 : 0);
		SEARCH_FOR_PLAYER();
	}

	private IEnumerator AttackFleeDelay()
	{
		yield return (object)new WaitForSeconds(0.5f);
		ReplicatedChangeState(LandAnimalState.Flee);
	}

	private IEnumerator AttackStance_ExitState()
	{
		_headRaise = 0f;
		yield break;
	}

	private void ReplicatedAttack_Bite(IPlayer player, bool correctDistance = false)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.LookAt(player.transform.position);
		((Component)this).transform.rotation = Quaternion.LookRotation(player.transform.position - ((Component)this).transform.position);
		OnAttacked();
		Animation_Attack();
		if (MPExtensions.IsMaster(Game.Mode))
		{
			Vector3 val = player.transform.position - _head.position;
			correctDistance = val.sqrMagnitude < BeamMath.Sqr(3f);
			ReplicateAttack_Bite replicateAttack_Bite = new ReplicateAttack_Bite();
			replicateAttack_Bite.Player = player;
			replicateAttack_Bite.Animal = this;
			replicateAttack_Bite.CorrectDistance = correctDistance;
			replicateAttack_Bite.Post();
		}
		if (correctDistance)
		{
			player.Statistics.ApplyDamage(_attackDamage, ((Component)this).transform.position, "Snake Bite");
			player.Statistics.ApplyStatusEffect(StatusEffect.POISON);
		}
	}

	private IEnumerator Dying_EnterState()
	{
		_rigidbody.mass = 1f;
		_rigidbody.constraints = (RigidbodyConstraints)0;
		Animation_Die();
		BeamTeam.Destroy<Snake>(this);
		yield break;
	}

	public void TakeDamage()
	{
		SEARCH_FOR_PLAYER();
		if (BeamTeam.IsNullOrDestroyed(closestPlayer))
		{
			ReplicatedChangeState(LandAnimalState.Explore);
		}
		else if (distanceToClosestPlayer < 10f)
		{
			if (_canAttack && _lastAttackTime > _attackCooldownSeconds)
			{
				ReplicatedChangeState(LandAnimalState.Chase);
			}
			else
			{
				ReplicatedChangeState(LandAnimalState.Flee);
			}
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
			_animator.Play("Attack");
		}
	}

	private void Animation_Move()
	{
		if (!((Object)(object)_animator == (Object)null))
		{
			_animator.SetFloat("Speed", _controllerVelocity);
			_headRaiseLerped = Mathf.Lerp(_headRaiseLerped, _headRaise, 3f * FixedTime.TimeStep(this));
			_animator.SetFloat("AngerValue", _headRaiseLerped);
		}
	}
}
