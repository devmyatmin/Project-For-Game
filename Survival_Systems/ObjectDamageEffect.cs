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
		LeanTween.value(base.gameObject, DamageAnimation, 1f, 0f - num, 0.5f).setEase(LeanTweenType.punch);
	}

	private void DamageAnimation(float val)
	{
		base.transform.localScale = new Vector3(val, 1f, val);
	}
}
