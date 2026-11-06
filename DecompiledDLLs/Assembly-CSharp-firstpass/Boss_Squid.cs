using System.Collections;
using System.Collections.Generic;
using Beam;
using Beam.Utilities;
using Funlabs;
using MEC;
using UnityEngine;

public class Boss_Squid : Piscus_Creature
{
	public class ReplicateIntroSetup : MultiplayerMessage
	{
		[Replicate]
		public Piscus_Creature Creature;

		public override void OnPeer()
		{
			((Boss_Squid)Creature).Setup_Intro();
		}
	}

	private bool _introSetup;

	[SerializeField]
	private Boss_Squid_Tentacle _tentacle;

	private Vector3 _goalStartPosition;

	private Quaternion _goalStartRotation;

	public Boss_Squid_Tentacle Tentacle => _tentacle;

	protected override void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		base.Start();
		Timing.RunCoroutine(Start_Sequence());
	}

	private IEnumerator<float> Start_Sequence()
	{
		while (LevelLoader.IsLoading())
		{
			yield return 0f;
		}
		PostAssetLoad();
	}

	private void PostAssetLoad()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		CurrentState = PiscusBehaviour.Idle;
		BiomePosition = ((Component)this).transform.position;
		BiomeSize = 100f;
		_goalStartPosition = ((Component)this).transform.position;
		_goalStartRotation = ((Component)this).transform.rotation;
		SetDifficultyParams();
	}

	protected override void Initialize()
	{
	}

	protected override void SEARCH_FOR_PLAYER()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		IPlayer currentTarget = PlayerUtilities.FindClosestPlayer(((Component)this).transform.position);
		CurrentTarget = currentTarget;
	}

	public override void ApplyDamage(float damageTaken)
	{
		base.ApplyDamage(damageTaken);
		Setup_Intro();
		_tentacle.TakeDamage();
	}

	protected IEnumerator Idle_EnterState()
	{
		CurrentSpeed = 0f;
		Animation.AnimatorController.SetFloat("Speed", 0f);
		((Component)this).transform.LookAt(((Component)this).transform.position + Vector3.up);
		yield break;
	}

	protected void Idle_FixedUpdate()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		SEARCH_FOR_PLAYER();
		if (CurrentTarget == null)
		{
			CurrentState = PiscusBehaviour.Explore;
			return;
		}
		if (!_introSetup)
		{
			if (Vector3.Distance(CurrentTarget.transform.position, ((Component)this).transform.position) < EngagePlayerDistance)
			{
				ReplicatedIntroSetup();
			}
			return;
		}
		UPDATE_MOVEMENT(checkAvoidance: false);
		if (Vector3.Distance(MovementPosition, ((Component)this).transform.position) < 4f)
		{
			ReplicatedChangeState(PiscusBehaviour.CircleTarget, CurrentTarget);
		}
	}

	private void Setup_Intro()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (!_introSetup)
		{
			_introSetup = true;
			MovementPosition = ((Component)this).transform.position + ((Component)this).transform.forward * 20f + ((Component)this).transform.right * 25f;
			MovementPosition = ClampToBounds(MovementPosition);
			CurrentSpeed = MaxSpeed;
			OnAwakened();
		}
	}

	protected override void Damage_Avoid()
	{
	}

	protected override void Attempt_Attacks()
	{
		if (CanAttack)
		{
			if (CurrentTarget.Movement.InWater)
			{
				Attempt_Attack_Player_InWater();
			}
			else
			{
				Attempt_Tentacle_GrabAttack();
			}
		}
	}

	protected override bool IsAttackPositionValid_Player_InWater(Vector3 attackPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (attackPosition.y < ((Component)this).transform.position.y - 1f)
		{
			return false;
		}
		return base.IsAttackPositionValid_Player_InWater(attackPosition);
	}

	private void Attempt_Tentacle_GrabAttack()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		if (!CheckAttackTimer())
		{
			Vector3 val = new Vector3(CurrentTarget.transform.position.x, 0f, CurrentTarget.transform.position.z);
			Vector3 val2 = new Vector3(((Component)this).transform.position.x, 0f, ((Component)this).transform.position.z);
			float num = Vector3.Distance(val, val2);
			if (!(num > 40f) && !(num < 5f) && !(Vector3.Angle(-((Component)this).transform.forward, CurrentTarget.transform.position - ((Component)this).transform.position) > 110f) && !(((Component)this).transform.position.y < -5f))
			{
				_tentacle.ReplicatedAttackTarget(CurrentTarget);
				LastAttack *= 0.5f;
			}
		}
	}

	private void Attempt_Tentacle_RaftAttack()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		if (CurrentTarget == null)
		{
			return;
		}
		Transform activePlatform = CurrentTarget.Movement.ActivePlatform;
		if ((Object)(object)activePlatform == (Object)null || ((Component)activePlatform).transform.position.y > 5f)
		{
			return;
		}
		Vector3 val = ((Component)this).transform.position + ((Component)this).transform.forward * 20f;
		Vector2 val2 = ClampPositionToCircle(new Vector2(((Component)activePlatform).transform.position.x, ((Component)activePlatform).transform.position.z), MaxCircleRadius, new Vector2(val.x, val.z));
		MovementPosition = new Vector3(val2.x, Mathf.Lerp(MovementPosition.y, -0.65f, 8f * FixedTime.TimeStep(this)), val2.y);
		MovementPosition = ClampToBounds(MovementPosition);
		if (!(((Component)this).transform.position.y < activePlatform.position.y - 5f))
		{
			Vector3 val3 = activePlatform.position - ((Component)this).transform.position;
			RaycastHit val4 = default;
			if (!Physics.SphereCast(((Component)this).transform.position, AvoidanceRayWidth, val3, ref val4, val3.magnitude, 1 << Layers.TERRAIN, (QueryTriggerInteraction)1) || !Object.op_Implicit((Object)(object)val4.collider))
			{
				_tentacle.ReplicatedAttackTarget(CurrentTarget);
			}
		}
	}

	public void Reset()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (CurrentState.Equals(PiscusBehaviour.AttackingPlayer))
		{
			ReplicatedFinishAttack();
		}
		_introSetup = false;
		((Component)this).transform.position = _goalStartPosition;
		((Component)this).transform.rotation = _goalStartRotation;
		CurrentTarget = null;
		ReplicatedChangeState(PiscusBehaviour.Idle, CurrentTarget);
		Beam.Utilities.ExtensionMethods.DestroyProjectilesInChildren(((Component)this).transform);
	}

	private void ReplicatedIntroSetup()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode))
		{
			ReplicateIntroSetup replicateIntroSetup = new ReplicateIntroSetup();
			replicateIntroSetup.Creature = this;
			replicateIntroSetup.Post();
		}
		Setup_Intro();
	}

	protected override void EventManager_PlayerDisconnected_Core(PlayerDisconnectedEvent disconnectedEvent)
	{
		if (disconnectedEvent.PlayerId == 1)
		{
			if (CurrentAttackTarget != null && PlayerUtilities.GetPlayerId_Safe(CurrentAttackTarget) == disconnectedEvent.PlayerId)
			{
				FinishAttack(completedAttack: true);
			}
			if (CurrentTarget != null && PlayerUtilities.GetPlayerId_Safe(CurrentTarget) == disconnectedEvent.PlayerId)
			{
				IPlayer playerFromId = PlayerUtilities.GetPlayerFromId(0);
				CurrentTarget = playerFromId;
			}
		}
	}
}
