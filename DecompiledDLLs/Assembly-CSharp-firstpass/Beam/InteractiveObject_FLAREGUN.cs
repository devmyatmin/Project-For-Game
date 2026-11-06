using System;
using System.Collections;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class InteractiveObject_FLAREGUN : InteractiveObject
{
	[SerializeField]
	private Transform _barrel;

	[SerializeField]
	private GameObject _flareProjectile;

	[SerializeField]
	[HideInInspector]
	private UnityEvent _onUseEmpty;

	public override void Use()
	{
		if (BeamTeam.IsNullOrDestroyed(Owner))
		{
			return;
		}
		if (DurabilityPoints > 0f)
		{
			((MonoBehaviour)this).StartCoroutine(ShootProjectile());
			base.Use();
			return;
		}
		UnityEvent onUseEmpty = _onUseEmpty;
		if (onUseEmpty != null)
		{
			onUseEmpty.Invoke();
		}
	}

	public override void Hold(bool holding)
	{
		base.Hold(holding);
		if (holding && Object.op_Implicit((Object)(object)_barrel))
		{
			SetBarrel();
		}
	}

	private void SetBarrel()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		LeanTween.cancel(((Component)this).gameObject);
		if (DurabilityPoints > 0f)
		{
			_barrel.localRotation = Quaternion.Euler(new Vector3(-30f, 0f, 0f));
		}
		else
		{
			_barrel.localRotation = Quaternion.Euler(new Vector3(40f, 0f, 0f));
		}
	}

	private IEnumerator ShootProjectile()
	{
		DurabilityPoints--;
		if (DurabilityPoints <= 0f)
		{
			CanUse = false;
			CanUseOnObjects = false;
		}
		Camera camera = Owner.PlayerCamera.Camera;
		GameObject val = Object.Instantiate<GameObject>(_flareProjectile, ((Component)camera).transform.position, Quaternion.identity);
		val.transform.position = ((Component)camera).transform.position;
		val.transform.rotation = Quaternion.LookRotation(((Component)camera).transform.forward);
		val.GetComponent<Rigidbody>().velocity = val.transform.forward * 50f;
		LeanTween.value(((Component)this).gameObject, (Action<float>)AnimateBarrel, -30f, 40f, 0.5f).setEase((LeanTweenType)4);
		yield return (object)new WaitForSeconds(1f);
		if (DurabilityPoints > 0f)
		{
			LeanTween.value(((Component)this).gameObject, (Action<float>)AnimateBarrel, 40f, -30f, 0.5f).setEase((LeanTweenType)4);
		}
	}

	private void AnimateBarrel(float rotation)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		_barrel.localRotation = Quaternion.Euler(rotation * new Vector3(1f, 0f, 0f));
	}
}
