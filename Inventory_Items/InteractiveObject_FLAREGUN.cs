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
		if (!base.Owner.IsNullOrDestroyed())
		{
			if (base.DurabilityPoints > 0f)
			{
				StartCoroutine(ShootProjectile());
				base.Use();
			}
			else
			{
				_onUseEmpty?.Invoke();
			}
		}
	}

	public override void Hold(bool holding)
	{
		base.Hold(holding);
		if (holding && (bool)_barrel)
		{
			SetBarrel();
		}
	}

	private void SetBarrel()
	{
		LeanTween.cancel(base.gameObject);
		if (base.DurabilityPoints > 0f)
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
		base.DurabilityPoints--;
		if (base.DurabilityPoints <= 0f)
		{
			base.CanUse = false;
			base.CanUseOnObjects = false;
		}
		Camera camera = base.Owner.PlayerCamera.Camera;
		GameObject gameObject = Object.Instantiate(_flareProjectile, camera.transform.position, Quaternion.identity);
		gameObject.transform.position = camera.transform.position;
		gameObject.transform.rotation = Quaternion.LookRotation(camera.transform.forward);
		gameObject.GetComponent<Rigidbody>().velocity = gameObject.transform.forward * 50f;
		LeanTween.value(base.gameObject, AnimateBarrel, -30f, 40f, 0.5f).setEase(LeanTweenType.easeInOutQuad);
		yield return new WaitForSeconds(1f);
		if (base.DurabilityPoints > 0f)
		{
			LeanTween.value(base.gameObject, AnimateBarrel, 40f, -30f, 0.5f).setEase(LeanTweenType.easeInOutQuad);
		}
	}

	private void AnimateBarrel(float rotation)
	{
		_barrel.localRotation = Quaternion.Euler(rotation * new Vector3(1f, 0f, 0f));
	}
}
