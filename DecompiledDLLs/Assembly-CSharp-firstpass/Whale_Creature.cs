using System.Collections;
using Beam;
using UnityEngine;

public class Whale_Creature : Piscus_Creature
{
	private const byte ANIMATION_DIVE = 2;

	private const byte ANIMATION_ROLLOVER = 3;

	[SerializeField]
	private Animation _anim;

	[SerializeField]
	private Transform _airhole;

	[SerializeField]
	private ParticleSystem _tailWaterSpray;

	[SerializeField]
	private ParticleSystem _spitParticle;

	[SerializeField]
	private Whale_TailForce _whaleTailForce;

	private float _lastSpitTime = 60f;

	private float _nextSpitTime = 60f;

	private float _lastWhaleNoise;

	private float _nextWhaleNoise = 60f;

	private float _curCirclingTime;

	private float _circlingTime;

	protected override void Initialize()
	{
		if ((Object)(object)_anim != (Object)null)
		{
			_anim.wrapMode = (WrapMode)2;
			_anim["Dive"].wrapMode = (WrapMode)1;
			_anim["RollOver"].wrapMode = (WrapMode)1;
			_anim["RollOver"].speed = 0.3f;
			_anim["Dive"].layer = 1;
			_anim["RollOver"].layer = 1;
		}
		base.Initialize();
	}

	protected override void UPDATE_MOVEMENT(bool avoidance = true)
	{
		base.UPDATE_MOVEMENT(avoidance);
		if ((PiscusBehaviour)CurrentState == PiscusBehaviour.CircleTarget)
		{
			Attempt_Spray();
		}
		AttemptWhaleSound();
		CheckWaterSpray();
		CheckWaterSpit();
		if ((Object)(object)_anim != (Object)null)
		{
			_anim["Swim"].speed = Mathf.Clamp(CurrentSpeed / 4f, 0.2f, 1f);
			_anim.CrossFade("Swim", 2f);
		}
	}

	protected override IEnumerator CircleTarget_EnterState()
	{
		if (CurrentTarget == null)
		{
			CurrentState = PiscusBehaviour.Explore;
			yield break;
		}
		Vector3 val = Avoid(new Vector3(CurrentTarget.transform.position.x, ((Component)this).transform.position.y, CurrentTarget.transform.position.z), 100f);
		Vector2 val2 = ClampPositionToCircle(new Vector2(CurrentTarget.transform.position.x, CurrentTarget.transform.position.z), MaxCircleRadius, new Vector2(val.x, val.z));
		Vector3 val3 = new Vector3(val2.x, ((Component)this).transform.position.y, val2.y) - ((Component)this).transform.position;
		MovementPosition = ((Component)this).transform.position + val3.normalized * 6f;
		MovementPosition = ClampToBounds(MovementPosition);
		_curCirclingTime = 0f;
		CurrentSpeed = MinSpeed;
	}

	protected override void CircleTarget_FixedUpdate()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		if (CurrentTarget == null)
		{
			ReplicatedChangeState(PiscusBehaviour.Explore, CurrentTarget);
			return;
		}
		CurrentSpeed = MinSpeed;
		_curCirclingTime += FixedTime.TimeStep(this);
		if (_circlingTime > 0f && _curCirclingTime > _circlingTime)
		{
			_curCirclingTime = 0f;
			ReplicatedChangeState(PiscusBehaviour.Flee, CurrentTarget);
			return;
		}
		Vector3 val = ((Component)this).transform.position - CurrentTarget.transform.position;
		_ = val.magnitude;
		val = ((Component)this).transform.position - MovementPosition;
		if (val.magnitude < 4f)
		{
			float y = ((Component)_airhole).transform.position.y;
			if (y > 0f && Random.value < 0.05f)
			{
				MovementPosition = ((Component)this).transform.position + ((Component)this).transform.forward * 6f + -((Component)this).transform.up * 20f;
				MovementPosition = ClampToBounds(MovementPosition);
				ReplicatedPlayAnimation(2);
			}
			else if (y > 0f && Random.value < 0.05f)
			{
				MovementPosition = ((Component)this).transform.position + ((Component)this).transform.forward * 6f;
				MovementPosition = ClampToBounds(MovementPosition);
				ReplicatedPlayAnimation(3);
			}
			else
			{
				Vector3 forward = ((Component)this).transform.forward;
				if (Random.value < 0.05f)
				{
					float num = ((Random.value < 0.5f) ? 1 : (-1));
					Vector3 val2 = Vector3.Lerp(forward, Random.insideUnitSphere * num, 0.4f);
					MovementPosition = CurrentTarget.transform.position + val2 * CircleRadius;
					MovementPosition = ClampToBounds(MovementPosition);
				}
				else
				{
					Vector3 val3 = ((Component)this).transform.position + forward * 6f;
					Vector2 val4 = ClampPositionToCircle(new Vector2(CurrentTarget.transform.position.x, CurrentTarget.transform.position.z), CircleRadius, new Vector2(val3.x, val3.z));
					float num2 = Mathf.Lerp(MovementPosition.y, CurrentTarget.transform.position.y, 8f * FixedTime.TimeStep(this));
					Vector3 movementPosition = new Vector3(val4.x, num2, val4.y);
					MovementPosition = movementPosition;
					MovementPosition = ClampToBounds(MovementPosition);
				}
			}
		}
		UPDATE_MOVEMENT();
	}

	private void Dive()
	{
		Spray();
		if ((Object)(object)_anim != (Object)null)
		{
			_anim.CrossFade("Dive", 0.5f);
		}
		((MonoBehaviour)this).StartCoroutine(_whaleTailForce.DiveForce());
	}

	private void RollOver()
	{
		Spray();
		if ((Object)(object)_anim != (Object)null)
		{
			_anim.CrossFade("RollOver", 0.5f);
		}
		((MonoBehaviour)this).StartCoroutine(_whaleTailForce.DiveForce());
	}

	private void Attempt_Spray()
	{
		if (_lastSpitTime > _nextSpitTime)
		{
			Spray();
		}
	}

	private void Spray()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (_airhole.position.y >= -0.25f)
		{
			_lastSpitTime = 0f;
			_nextSpitTime = Random.Range(15, 60);
			_spitParticle.Play(true);
			OnAttackedRaft();
		}
	}

	private void AttemptWhaleSound()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		_lastWhaleNoise += FixedTime.TimeStep(this);
		if (!(_lastWhaleNoise < _nextWhaleNoise) && !(Vector3.Distance(CurrentTarget.transform.position, ((Component)this).transform.position) > 75f))
		{
			PlayerWhaleSound();
		}
	}

	private void PlayerWhaleSound()
	{
		_lastWhaleNoise = 0f;
		_nextWhaleNoise = Random.Range(30, 120);
		OnAttacked();
	}

	private void CheckWaterSpray()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)_tailWaterSpray).transform.position.y > 0f)
		{
			if (_tailWaterSpray.isStopped)
			{
				_tailWaterSpray.Play(true);
			}
		}
		else if (_tailWaterSpray.isPlaying)
		{
			_tailWaterSpray.Stop(true);
		}
	}

	private void CheckWaterSpit()
	{
		if (_lastSpitTime < _nextSpitTime + 10f)
		{
			_lastSpitTime += FixedTime.TimeStep(this);
		}
	}

	protected override void PlayAnimation(byte animationId)
	{
		base.PlayAnimation(animationId);
		switch (animationId)
		{
		case 2:
			Dive();
			break;
		case 3:
			RollOver();
			break;
		}
	}
}
