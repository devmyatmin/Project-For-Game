using System.Collections;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam;

public class LandCrab : LandAnimal
{
	public class ReplicateAttack : MultiplayerMessage
	{
		[Replicate]
		public LandAnimal animal;

		[Replicate]
		public IPlayer player;

		public override void OnPeer()
		{
			LandCrab landCrab = (LandCrab)animal;
			if ((Object)(object)landCrab != (Object)null && player != null)
			{
				landCrab.Attack(player);
			}
		}
	}

	[SerializeField]
	public Animation anim;

	private const float _exploreRange = 5f;

	private const float _distance_AvoidPlayer = 5f;

	private const float _distance_FleePlayer = 3.5f;

	private bool _canAttack = true;

	private float _attackDamage;

	[SerializeField]
	private float _normalAttackDamage = 5f;

	[SerializeField]
	private float _hardAttackDamage = 6f;

	private const float walkSpeed = 0.6f;

	private const float avoidSpeed = 0.6f;

	private const float fleeSpeed = 1.1f;

	public bool _canMove;

	private float _currentIdleTime;

	private float _idleTime;

	private float _lastAttack;

	private bool _dead;

	private void Awake()
	{
		Setup();
		Animation_Setup();
		SetDifficultyParams();
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
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
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
				val.Normalize();
				Vector3 val2 = val * _currentSpeed;
				val2.y = 0f;
				Vector3 val3 = val2 - _rigidbody.velocity;
				val3.y = 0f;
				_rigidbody.AddForce(val3, (ForceMode)2);
			}
			_rigidbody.AddForce(Physics.gravity, (ForceMode)5);
			_controllerVelocity = CalculateTrueVelocity();
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
		_idleTime = Random.Range(5, 25);
		MatchGroundNormal();
		_waypointPosition = ((Component)this).transform.position;
		yield break;
	}

	private void Idle_Update()
	{
		_currentIdleTime += Time.deltaTime;
		if (_currentIdleTime > _idleTime)
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			return;
		}
		if (!BeamTeam.IsNullOrDestroyed(closestPlayer) && distanceToClosestPlayer < 5f)
		{
			ReplicatedChangeState(LandAnimalState.Explore);
		}
		SEARCH_FOR_PLAYER();
	}

	private IEnumerator Explore_EnterState()
	{
		NewExploreWaypoint(5f);
		_canMove = true;
		_adjustedSpeed = 0.6f;
		_stuckWaypointTime = 0f;
		yield break;
	}

	private void Explore_Update()
	{
		_adjustedSpeed = 0.6f;
		if (!BeamTeam.IsNullOrDestroyed(closestPlayer) && distanceToClosestPlayer < 5f)
		{
			_adjustedSpeed = 0.6f;
			if (CheckWaypointDist())
			{
				NewAvoidWaypoint(2f, 1f);
			}
			if (distanceToClosestPlayer < 3.5f)
			{
				ReplicatedChangeState(LandAnimalState.Flee);
				return;
			}
		}
		if (distanceToClosestPlayer > 5f && CheckWaypointDist())
		{
			if (Random.value < 0.25f)
			{
				NewExploreWaypoint(5f);
				return;
			}
			ReplicatedChangeState(LandAnimalState.Idle);
		}
		CheckIfStuck();
		SEARCH_FOR_PLAYER();
	}

	private IEnumerator Flee_EnterState()
	{
		NewAvoidWaypoint(2f, 1f);
		_canMove = true;
		_lastAttack = 100f;
		_adjustedSpeed = 1.1f;
		_stuckWaypointTime = 0f;
		yield break;
	}

	private void Flee_Update()
	{
		_lastAttack += Time.deltaTime;
		if (CheckWaypointDist())
		{
			NewAvoidWaypoint(2f, 1f);
		}
		if (BeamTeam.IsNullOrDestroyed(closestPlayer))
		{
			ReplicatedChangeState(LandAnimalState.Explore);
			return;
		}
		if (distanceToClosestPlayer < 1f && _lastAttack > 0.5f && _canAttack)
		{
			ReplicatedAttack(closestPlayer);
		}
		if (distanceToClosestPlayer > 3.5f)
		{
			ReplicatedChangeState(LandAnimalState.Explore);
		}
		CheckIfStuck();
		SEARCH_FOR_PLAYER();
	}

	private void Attack(IPlayer player)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		_lastAttack = 0f;
		player.Statistics.ApplyDamage(_attackDamage, ((Component)this).transform.position, "Crab");
		Animation_Attack();
		OnAttacked();
	}

	private void ReplicatedAttack(IPlayer player)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode))
		{
			ReplicateAttack replicateAttack = new ReplicateAttack();
			replicateAttack.player = player;
			replicateAttack.animal = this;
			replicateAttack.Post();
		}
		Attack(player);
	}

	protected override void OnKilled()
	{
		base.OnKilled();
		OnKilledDisable();
	}

	public void OnKilledDisable()
	{
		_rigidbody.mass = 1f;
		_rigidbody.constraints = (RigidbodyConstraints)0;
		Animation_Die();
		((Behaviour)this).enabled = false;
		_dead = true;
	}

	private IEnumerator Dying_EnterState()
	{
		OnKilledDisable();
		yield break;
	}

	public void TakeDamage()
	{
		Animation_TakeDamage();
	}

	private void Animation_Setup()
	{
		if (!((Object)(object)anim == (Object)null))
		{
			anim.wrapMode = (WrapMode)2;
			anim["Death"].wrapMode = (WrapMode)1;
			anim["Death"].normalizedTime = 1f;
			anim.Play("Death");
			anim.Sample();
			anim["Attack"].wrapMode = (WrapMode)1;
			anim["Attack"].layer = 2;
		}
	}

	private void Animation_TakeDamage()
	{
		if (!((Object)(object)anim == (Object)null))
		{
			anim.CrossFade("Death");
		}
	}

	private void Animation_Die()
	{
		if (!((Object)(object)anim == (Object)null))
		{
			anim.CrossFade("Death");
		}
	}

	private void Animation_Attack()
	{
		if (!((Object)(object)anim == (Object)null))
		{
			anim.CrossFade("Attack", 0.01f);
		}
	}

	private void Animation_Move()
	{
		if (!((Object)(object)anim == (Object)null))
		{
			if (_controllerVelocity >= 0.01f)
			{
				anim["Walk"].speed = 5f * _controllerVelocity;
				anim.CrossFade("Walk");
			}
			else
			{
				anim.CrossFade("Idle");
			}
		}
	}
}
