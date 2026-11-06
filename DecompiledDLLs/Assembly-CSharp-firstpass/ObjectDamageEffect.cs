using System;
using Beam;
using UnityEngine;

public class ObjectDamageEffect : MonoBehaviour
{
	public void Damaged(IBase sender, IBaseActionEventData data)
	{
		PlayDamageAnimation(data.Value);
	}

	public void PlayDamageAnimation(float damage)
	{
		float num = damage * 0.01f;
		LeanTween.value(((Component)this).gameObject, (Action<float>)DamageAnimation, 1f, 0f - num, 0.5f).setEase((LeanTweenType)34);
	}

	private void DamageAnimation(float val)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localScale = new Vector3(val, 1f, val);
	}
}
