using System.Collections;
using System.Collections.Generic;
using Beam;
using Beam.Utilities;
using Funlabs;
using MEC;
using UnityEngine;

public class Boss_Shark : Piscus_Creature
{
	public class ReplicateIntroSetup : MultiplayerMessage
	{
		[Replicate]
		public Piscus_Creature Creature;

		public override void OnPeer()
		{
			((Boss_Shark)Creature).IntroSetup();
		}
	}

	private bool _introSetup;

	[SerializeField]
	private ParticleSystem _IntroBloodParticle;

	private Vector3 _goalStartPosition;

	private Quaternion _goalStartRotation;

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
		if (!_introSetup)
		{
			_introSetup = true;
		}
	}

	protected IEnumerator Idle_EnterState()
	{
		_introSetup = false;
		CurrentSpeed = 0f;
		Animation.AnimatorController.SetFloat("Speed", 0f);
		if ((Object)(object)_IntroBloodParticle != (Object)null)
		{
			_IntroBloodParticle.Play(true);
		}
		yield break;
	}

	protected void Idle_FixedUpdate()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		SEARCH_FOR_PLAYER();
		if (CurrentTarget == null)
		{
			CurrentState = PiscusBehaviour.Explore;
			return;
		}
		float num = Vector3.Distance(CurrentTarget.transform.position, ((Component)this).transform.position);
		if (!_introSetup && num < EngagePlayerDistance)
		{
			ReplicatedIntroSetup();
		}
		if (_introSetup)
		{
			MovementPosition = ((Component)this).transform.position + ((Component)this).transform.forward * 20f + Vector3.down * 10f;
			MovementPosition = ClampToBounds(MovementPosition);
			CurrentSpeed = MinSpeed;
			UPDATE_MOVEMENT(checkAvoidance: false);
			if (((Component)this).transform.position.y < -2f)
			{
				ReplicatedChangeState(PiscusBehaviour.CircleTarget, CurrentTarget);
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

	private void IntroSetup()
	{
		_introSetup = true;
		OnAwakened();
		if ((Object)(object)_IntroBloodParticle != (Object)null)
		{
			_IntroBloodParticle.Stop(true);
		}
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
		IntroSetup();
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
