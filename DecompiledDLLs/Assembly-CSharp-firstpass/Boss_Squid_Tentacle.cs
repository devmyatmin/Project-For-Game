using Beam;
using Beam.Utilities;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

public class Boss_Squid_Tentacle : MonoBehaviour
{
	public class ReplicateAttackTarget : MultiplayerMessage
	{
		[Replicate]
		public Piscus_Creature Creature;

		[Replicate]
		public IPlayer Player;

		public override void OnPeer()
		{
			((Boss_Squid)Creature).Tentacle.AttackTarget(Player);
		}
	}

	public class ReplicatePushAndDamagePlayer : MultiplayerMessage
	{
		[Replicate]
		public Piscus_Creature Creature;

		[Replicate]
		public IPlayer Player;

		[Replicate]
		public Vector3 Direction;

		[Replicate]
		public float Damage;

		public override void OnPeer()
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			((Boss_Squid)Creature).Tentacle.PushAndDamagePlayer(Player, Direction, Damage);
		}
	}

	[SerializeField]
	private Boss_Squid _bossSquid;

	[SerializeField]
	private Transform _Grab;

	[SerializeField]
	private SkinnedMeshRenderer _renderer;

	private Animation _animation;

	private Vector3 _grabTargetPosition;

	private float _grabSpeed = 0.75f;

	private float _reachDist;

	private bool _targetGrabbed;

	private bool _tentacleAttacking;

	private AnimationState _animReach;

	private AnimationState _animMiss;

	private AnimationState _animGrab;

	public UnityEvent Attacked;

	public UnityEvent AttackedGrabbed;

	public UnityEvent AttackedPushed;

	private void Start()
	{
		_animation = ((Component)this).GetComponent<Animation>();
		_animReach = _animation["Reach_1"];
		_animMiss = _animation["Miss"];
		_animGrab = _animation["Grab"];
		_animation.wrapMode = (WrapMode)8;
		_animGrab.wrapMode = (WrapMode)1;
		_animGrab.layer = 2;
	}

	private IPlayer FindClosestPlayer(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		IPlayer player = PlayerUtilities.FindClosestPlayer(position, out var _);
		if (!BeamTeam.IsNullOrDestroyed(player))
		{
			return player;
		}
		return null;
	}

	private void Update()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (((Renderer)_renderer).enabled != _tentacleAttacking)
		{
			((Renderer)_renderer).enabled = _tentacleAttacking;
		}
		if (!_tentacleAttacking)
		{
			return;
		}
		((Component)this).transform.LookAt(_grabTargetPosition);
		IPlayer player = FindClosestPlayer(_Grab.position);
		if (player != null)
		{
			if (Vector3.Distance(_Grab.position, player.transform.position) < 1f && !_targetGrabbed)
			{
				_targetGrabbed = true;
				_reachDist = _animReach.time;
				HitTarget(player);
			}
			float num = _animReach.length * _animReach.normalizedTime;
			if (num >= _animReach.length)
			{
				MissTarget();
			}
			else if (_animReach.speed < 0f && num <= 0f)
			{
				_tentacleAttacking = false;
			}
			else if (_animMiss.length * _animMiss.normalizedTime >= _animMiss.length)
			{
				_tentacleAttacking = false;
			}
		}
	}

	public void TakeDamage()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (_tentacleAttacking)
		{
			IPlayer player = FindClosestPlayer(_Grab.position);
			if (player == null)
			{
				_animation.Stop();
			}
			else if (Vector3.Distance(_Grab.position, player.transform.position) < 12f)
			{
				_reachDist = _animReach.time;
				_animation.Stop();
				_animation.CrossFade("Grab", 0.03f);
				_animReach.time = _reachDist;
				_animReach.speed = 0f - _grabSpeed;
				_animation.Play("Reach_1");
			}
		}
	}

	public void AttackTarget(IPlayer player)
	{
		_tentacleAttacking = true;
		ReachForTarget(player);
	}

	private void ReachForTarget(IPlayer player)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		_targetGrabbed = false;
		_grabTargetPosition = player.transform.position;
		_animation.Stop();
		_animGrab.time = 0f;
		_animReach.time = 0f;
		_animReach.speed = _grabSpeed;
		_animation.Play("Reach_1");
		UnityEvent attacked = Attacked;
		if (attacked != null)
		{
			attacked.Invoke();
		}
	}

	private void HitTarget(IPlayer player)
	{
		_animation.Stop();
		_animation.CrossFade("Grab", 0.03f);
		_animReach.time = _reachDist;
		_animReach.speed = 0f - _grabSpeed;
		_animation.Play("Reach_1");
		if (player.IsOwner)
		{
			if (Random.value < 0.75f)
			{
				Grab_Player(player);
			}
			else
			{
				Push_Player(player);
			}
		}
	}

	private void MissTarget()
	{
		_animation.Play("Miss");
	}

	private void Grab_Player(IPlayer player)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		UnityEvent attackedGrabbed = AttackedGrabbed;
		if (attackedGrabbed != null)
		{
			attackedGrabbed.Invoke();
		}
		Vector3 val = new Vector3(((Component)this).transform.position.x, player.transform.position.y, ((Component)this).transform.position.z);
		Vector3 position = player.transform.position;
		Vector3 direction = val - position;
		ReplicatedPushAndDamagePlayer(player, direction, 25f);
	}

	private void Push_Player(IPlayer player)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		UnityEvent attackedPushed = AttackedPushed;
		if (attackedPushed != null)
		{
			attackedPushed.Invoke();
		}
		Vector3 position = ((Component)this).transform.position;
		Vector3 direction = player.transform.position - position;
		ReplicatedPushAndDamagePlayer(player, direction, 25f);
	}

	private void PushAndDamagePlayer(IPlayer player, Vector3 direction, float damage)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		player.Movement.AddVelocity(direction, 3f);
		player.Statistics.ApplyDamage(damage, ((Component)this).transform.position, "Tentacle");
	}

	public void ReplicatedAttackTarget(IPlayer player)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMaster(Game.Mode))
		{
			ReplicateAttackTarget replicateAttackTarget = new ReplicateAttackTarget();
			replicateAttackTarget.Creature = ((Component)_bossSquid).GetComponent<Piscus_Creature>();
			replicateAttackTarget.Player = player;
			replicateAttackTarget.Post();
		}
		AttackTarget(player);
	}

	private void ReplicatedPushAndDamagePlayer(IPlayer player, Vector3 direction, float damage)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode))
		{
			ReplicatePushAndDamagePlayer replicatePushAndDamagePlayer = new ReplicatePushAndDamagePlayer();
			replicatePushAndDamagePlayer.Creature = ((Component)_bossSquid).GetComponent<Piscus_Creature>();
			replicatePushAndDamagePlayer.Player = player;
			replicatePushAndDamagePlayer.Direction = direction;
			replicatePushAndDamagePlayer.Damage = damage;
			replicatePushAndDamagePlayer.Post();
		}
		PushAndDamagePlayer(player, direction, damage);
	}
}
