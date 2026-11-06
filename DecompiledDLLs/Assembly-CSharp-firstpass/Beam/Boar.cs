using System.Collections;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam;

public class Boar : LandAnimal
{
	public class ReplicateAttack_Bite : MultiplayerMessage
	{
		[Replicate]
		public LandAnimal animal;

		[Replicate]
		public IPlayer player;

		public override void OnPeer()
		{
			Boar boar = (Boar)animal;
			if ((Object)(object)boar != (Object)null && player != null)
			{
				boar.Attack_Bite(player);
			}
		}
	}

	private const float _exploreRange = 15f;

	private const float _distance_Attack = 2.5f;

	private const float _distance_AttackChase = 4f;

	private const float _distance_AvoidPlayer = 12f;

	private const float _distance_FleePlayer = 6f;

	private float _attackDamage;

	[SerializeField]
	private float _normalAttackDamage = 75f;

	[SerializeField]
	private float _hardAttackDamage = 90f;

	private const float walkSpeed = 0.5f;

	private const float avoidSpeed = 2f;

	private const float fleeSpeed = 5f;

	public bool _canMove;

	private float _idleTime;

	private float _idleTimeToWait;

	private float _lastAttackTime;

	private float _attackCooldownSeconds = 5f;

	private float _fleeTime;

	private float _chaseTimer;

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
	}

	public override void FixedTick()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		if (_rigidbody.useGravity)
		{
			_rigidbody.constraints = (RigidbodyConstraints)112;
			_rigidbody.useGravity = false;
		}
		if (!MPExtensions.IsClient(Game.Mode))
		{
			_currentSpeed = Mathf.Lerp(_currentSpeed, _adjustedSpeed * breakingMultiplier, FixedTime.TimeStep(this) * 2f);
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
			_controllerVelocity = CalculateTrueVelocity();
			_rigidbody.AddForce(Physics.gravity, (ForceMode)0);
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
		_idleTimeToWait = Random.Range(5, 25);
		_waypointPosition = ((Component)this).transform.position + ((Component)this).transform.forward;
		Animation_Idle();
		MatchGroundNormal();
		yield break;
	}

	private void Idle_Update()
	{
		_idleTime += FixedTime.TimeStep(this);
		if (_idleTime > _idleTimeToWait)
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			return;
		}
		if (!BeamTeam.IsNullOrDestroyed(closestPlayer) && distanceToClosestPlayer < 12f)
		{
			ReplicatedChangeState(LandAnimalState.Explore);
		}
		SEARCH_FOR_PLAYER();
	}

	private IEnumerator Explore_EnterState()
	{
		NewExploreWaypoint(15f);
		_canMove = true;
		_adjustedSpeed = 0.5f;
		_stuckWaypointTime = 0f;
		yield break;
	}

	private void Explore_Update()
	{
		_adjustedSpeed = 0.5f;
		if (!BeamTeam.IsNullOrDestroyed(closestPlayer) && distanceToClosestPlayer < 12f)
		{
			_adjustedSpeed = 2f;
			if (CheckWaypointDist())
			{
				NewAvoidWaypoint(4f, 4f);
			}
			if (distanceToClosestPlayer < 6f)
			{
				ReplicatedChangeState(LandAnimalState.Flee);
				return;
			}
		}
		if (distanceToClosestPlayer > 12f && CheckWaypointDist())
		{
			if (Random.value < 0.25f)
			{
				NewExploreWaypoint(15f);
				return;
			}
			ReplicatedChangeState(LandAnimalState.Idle);
		}
		CheckIfStuck();
		SEARCH_FOR_PLAYER();
	}

	private IEnumerator Chase_EnterState()
	{
		if (BeamTeam.IsNullOrDestroyed(closestPlayer))
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			yield break;
		}
		NewChaseWaypoint(closestPlayer.transform.position);
		_canMove = true;
		_adjustedSpeed = 5f;
		_stuckWaypointTime = 0f;
		_chaseTimer = 0f;
	}

	private void Chase_Update()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		_chaseTimer += FixedTime.TimeStep(this);
		if (_chaseTimer > 30f)
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			return;
		}
		if (BeamTeam.IsNullOrDestroyed(closestPlayer))
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			return;
		}
		if (distanceToClosestPlayer < 2.5f)
		{
			ReplicatedAttack_Bite(closestPlayer);
			ReplicatedChangeState(LandAnimalState.Flee);
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
		NewAvoidWaypoint(4f, 4f);
		_canMove = true;
		_fleeTime = 0f;
		_adjustedSpeed = 5f;
		_stuckWaypointTime = 0f;
		yield break;
	}

	private void Flee_Update()
	{
		if (CheckWaypointDist())
		{
			NewAvoidWaypoint(4f, 4f);
		}
		_fleeTime += FixedTime.TimeStep(this);
		if (BeamTeam.IsNullOrDestroyed(closestPlayer))
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			return;
		}
		if ((distanceToClosestPlayer > 20f && _fleeTime > 10f) || _fleeTime > 30f)
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			return;
		}
		CheckIfStuck();
		SEARCH_FOR_PLAYER();
	}

	private void Attack_Bite(IPlayer player)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.LookAt(player.transform.position);
		((Component)this).transform.rotation = Quaternion.LookRotation(player.transform.position - ((Component)this).transform.position);
		OnAttacked();
		Animation_Attack();
		player.Statistics.ApplyStatusEffect(StatusEffect.BLEEDING);
		player.Statistics.ApplyDamage(_attackDamage, ((Component)this).transform.position, "Boar");
	}

	public void ReplicatedAttack_Bite(IPlayer player)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMaster(Game.Mode))
		{
			ReplicateAttack_Bite replicateAttack_Bite = new ReplicateAttack_Bite();
			replicateAttack_Bite.player = player;
			replicateAttack_Bite.animal = this;
			replicateAttack_Bite.Post();
		}
		Attack_Bite(player);
	}

	public void TakeDamage()
	{
		CurrentState = LandAnimalState.Flee;
		Animation_TakeDamage();
	}

	private void Animation_TakeDamage()
	{
		if (!((Object)(object)_animator == (Object)null))
		{
			_animator.SetTrigger("TakeDamage");
		}
	}

	private void Animation_Idle()
	{
		if (!((Object)(object)_animator == (Object)null))
		{
			_animator.SetFloat("IdleState", Mathf.Round((float)Random.Range(0, 2)));
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
		}
	}
}
