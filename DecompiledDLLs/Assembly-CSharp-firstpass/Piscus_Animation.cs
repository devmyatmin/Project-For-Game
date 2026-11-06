using UnityEngine;

public class Piscus_Animation : MonoBehaviour
{
	[SerializeField]
	private Animation _anim;

	[SerializeField]
	private Animator _animatorController;

	private float _normalDirection;

	public Animator AnimatorController => _animatorController;

	public void Animation_Setup()
	{
		if ((Object)(object)_anim != (Object)null)
		{
			_anim.wrapMode = (WrapMode)2;
			if (TrackedReference.op_Implicit((TrackedReference)(object)_anim["Attack"]))
			{
				_anim["Attack"].layer = 2;
				_anim["Attack"].wrapMode = (WrapMode)1;
			}
			if (TrackedReference.op_Implicit((TrackedReference)(object)_anim["Damage_Left"]))
			{
				_anim["Damage_Left"].layer = 1;
				_anim["Damage_Left"].wrapMode = (WrapMode)1;
			}
			if (TrackedReference.op_Implicit((TrackedReference)(object)_anim["Damage_Right"]))
			{
				_anim["Damage_Right"].layer = 1;
				_anim["Damage_Right"].wrapMode = (WrapMode)1;
			}
			if (TrackedReference.op_Implicit((TrackedReference)(object)_anim["Attacking"]))
			{
				_anim["Attacking"].wrapMode = (WrapMode)1;
				_anim["Attacking"].layer = 2;
			}
			if (TrackedReference.op_Implicit((TrackedReference)(object)_anim["Swimming_Left"]))
			{
				_anim["Swimming_Left"].wrapMode = (WrapMode)2;
			}
			if (TrackedReference.op_Implicit((TrackedReference)(object)_anim["Swimming_Right"]))
			{
				_anim["Swimming_Right"].wrapMode = (WrapMode)2;
			}
		}
	}

	public void UpdateAnimation(Vector3 movementPosition, float currentSpeed, float minSpeed, float maxSpeed)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		Vector3 val = ((Component)this).transform.InverseTransformPoint(movementPosition);
		num = Mathf.Lerp(num, Mathf.Atan2(val.x, val.z), 4f * Time.deltaTime);
		_normalDirection = Mathf.Lerp(_normalDirection, num, Time.deltaTime * 2f);
		if ((Object)(object)_anim != (Object)null)
		{
			if (_normalDirection < -0.005f && TrackedReference.op_Implicit((TrackedReference)(object)_anim["Swimming_Left"]))
			{
				_anim["Swimming_Left"].speed = Mathf.Clamp(currentSpeed / 2f, 0.5f, 2f);
				_anim.CrossFade("Swimming_Left", 0.4f);
			}
			else if (_normalDirection > 0.005f && TrackedReference.op_Implicit((TrackedReference)(object)_anim["Swimming_Right"]))
			{
				_anim["Swimming_Right"].speed = Mathf.Clamp(currentSpeed / 2f, 0.5f, 2f);
				_anim.CrossFade("Swimming_Right", 0.4f);
			}
			else
			{
				_anim["Swimming"].speed = Mathf.Clamp(currentSpeed / 2f, 0.5f, 2f);
				_anim.CrossFade("Swimming", 0.4f);
			}
		}
		if (Object.op_Implicit((Object)(object)_animatorController))
		{
			_animatorController.SetFloat("Direction", _normalDirection);
			float num2 = (currentSpeed - minSpeed) / (maxSpeed - minSpeed);
			_animatorController.SetFloat("Speed", num2);
		}
	}

	public void Animation_Damage(Vector3 targetPosition)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_anim != (Object)null)
		{
			float num = 0f;
			Vector3 val = ((Component)this).transform.InverseTransformPoint(targetPosition);
			num = Mathf.Atan2(val.x, val.z);
			if (num < -0.05f && TrackedReference.op_Implicit((TrackedReference)(object)_anim["Damage_Left"]))
			{
				_anim.CrossFade("Damage_Left", 0.2f);
			}
			else if (num > 0.05f && TrackedReference.op_Implicit((TrackedReference)(object)_anim["Damage_Right"]))
			{
				_anim.CrossFade("Damage_Right", 0.2f);
			}
			else if (TrackedReference.op_Implicit((TrackedReference)(object)_anim["Damage_Left"]))
			{
				_anim.CrossFade("Damage_Left", 0.2f);
			}
		}
		if (Object.op_Implicit((Object)(object)_animatorController))
		{
			_animatorController.SetTrigger("TakeDamage");
		}
	}

	public void Animation_Bite()
	{
		if ((Object)(object)_anim != (Object)null && TrackedReference.op_Implicit((TrackedReference)(object)_anim["Attack"]))
		{
			_anim.CrossFade("Attack", 0.05f);
		}
		if (Object.op_Implicit((Object)(object)_animatorController))
		{
			_animatorController.SetTrigger("Attack");
		}
	}

	public void Animation_Attacking()
	{
		if ((Object)(object)_anim != (Object)null && TrackedReference.op_Implicit((TrackedReference)(object)_anim["Attacking"]))
		{
			_anim.CrossFade("Attacking");
		}
		if (Object.op_Implicit((Object)(object)_animatorController))
		{
			_animatorController.SetTrigger("Attack");
		}
	}

	public void Animation_Attack_Complete()
	{
		if ((Object)(object)_anim != (Object)null && TrackedReference.op_Implicit((TrackedReference)(object)_anim["Attacking"]) && TrackedReference.op_Implicit((TrackedReference)(object)_anim["Attack"]))
		{
			_anim["Attack"].enabled = false;
			_anim["Attacking"].enabled = false;
		}
	}
}
