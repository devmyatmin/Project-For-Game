using System.Runtime.CompilerServices;
using Beam;
using Beam.Crafting;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.Events;

public class Interacting_BOW : ConstructionObject, IChargeable, IUseable, IAnimatablePrimary, IBase, IHasCraftingType, IReloadable, IAnimatableSecondary
{
	[SerializeField]
	private bool _canUseSecondary;

	private bool _reloaded;

	[Header("SECONDARY ANIMATIONS")]
	[SerializeField]
	private AnimationType _secondaryIdle;

	[SerializeField]
	private AnimationType _secondary;

	[SerializeField]
	private AnimationType _reloadAnimation;

	[SerializeField]
	private Interacting_ARROW _arrowPrefab;

	[SerializeField]
	private float _arrowSpeed;

	[SerializeField]
	private BaseObjectProxy _arrowProxy;

	[SerializeField]
	private BowString _bowString;

	[SerializeField]
	[HideInInspector]
	private BaseActionUnityEvent _charged;

	[SerializeField]
	[HideInInspector]
	private BaseActionUnityEvent _usedSecondary;

	[SerializeField]
	[HideInInspector]
	private UnityEvent _onReload;

	public bool CanUseSecondary
	{
		get
		{
			return _canUseSecondary;
		}
		set
		{
			_canUseSecondary = value;
		}
	}

	public bool Reloaded => _reloaded;

	public AnimationType SecondaryIdle => _secondaryIdle;

	public AnimationType Secondary => _secondary;

	public AnimationType ReloadAnimation => _reloadAnimation;

	BaseActionUnityEvent IChargeable.Charged => _charged;

	BaseActionUnityEvent IChargeable.UsedSecondary => _usedSecondary;

	public override void Hold(bool holding)
	{
		base.Hold(holding);
		if (!holding)
		{
			_arrowProxy.gameObject.SetActive(value: false);
			_reloaded = false;
		}
		if (_bowString != null)
		{
			_bowString.enabled = holding;
		}
	}

	public bool CanReloadWith(IPickupable pickupable)
	{
		return pickupable.CraftingType.InteractiveType == _arrowPrefab.CraftingType.InteractiveType;
	}

	public void Reload(IPickupable pickupable)
	{
		if (!pickupable.IsNullOrDestroyed())
		{
			Interacting_ARROW interacting_ARROW = pickupable as Interacting_ARROW;
			if (interacting_ARROW != null && CanReloadWith(interacting_ARROW))
			{
				CreateArrowProxy(interacting_ARROW);
				_reloaded = true;
				_onReload?.Invoke();
			}
		}
		else
		{
			Interacting_ARROW interacting_ARROW = base.Owner.Holder.Storage.Get<Interacting_ARROW>(_arrowPrefab.CraftingType);
			if (interacting_ARROW != null)
			{
				CreateArrowProxy(interacting_ARROW);
				_reloaded = true;
				_onReload?.Invoke();
			}
		}
	}

	public bool ValidateSecondary(IBase obj)
	{
		if (!obj.IsNullOrDestroyed())
		{
			Interacting_ARROW interacting_ARROW = obj as Interacting_ARROW;
			if (interacting_ARROW != null && CanReloadWith(interacting_ARROW))
			{
				return true;
			}
		}
		else
		{
			Interacting_ARROW interacting_ARROW = base.Owner.Holder.Storage.Get<Interacting_ARROW>(_arrowPrefab.CraftingType);
			if (interacting_ARROW != null)
			{
				return true;
			}
		}
		return false;
	}

	public void PreSecondary()
	{
		if (_bowString != null)
		{
			_bowString.Pullback();
		}
		base.ActionEventData.Populate(this, null);
		_charged.Invoke(this, base.ActionEventData);
	}

	public void UseSecondary()
	{
		Interacting_ARROW interacting_ARROW = base.Owner.Holder.Storage.Get<Interacting_ARROW>(_arrowPrefab.CraftingType);
		if (interacting_ARROW != null)
		{
			base.Owner.Holder.Storage.Pop(interacting_ARROW);
			BeamRay ray = base.Owner.PlayerCamera.GetRay(5f);
			float num = Vector3.Distance(_arrowProxy.transform.position, ray.OriginPoint);
			interacting_ARROW.transform.position = ray.OriginPoint;
			interacting_ARROW.transform.LookAt(ray.EndPoint);
			interacting_ARROW.transform.position += interacting_ARROW.transform.forward * num;
			Physics.IgnoreCollision(interacting_ARROW.GetComponent<Collider>(), GetComponent<Collider>(), ignore: true);
			interacting_ARROW.ProjectileMovement.enabled = true;
			interacting_ARROW.ProjectileMovement.Fire(base.Owner);
			interacting_ARROW.rigidbody.AddForce(base.Owner.Movement.CharacterController.velocity + interacting_ARROW.transform.forward * _arrowSpeed * base.Owner.PlayerSkills.HuntingMultiplier, ForceMode.Impulse);
			_arrowProxy.gameObject.SetActive(value: false);
			_reloaded = false;
			if ((bool)_bowString)
			{
				_bowString.Release();
			}
			base.ActionEventData.Populate(this, null);
			_usedSecondary.Invoke(this, base.ActionEventData);
		}
	}

	public void PostSecondary()
	{
		if (_bowString != null)
		{
			_bowString.Pullback_Release();
		}
	}

	private void CreateArrowProxy(Interacting_ARROW arrow)
	{
		_arrowProxy.MeshFilter.sharedMesh = arrow.Proxy.MeshFilter.sharedMesh;
		_arrowProxy.MeshRenderer.sharedMaterial = arrow.Proxy.MeshRenderer.sharedMaterial;
		_arrowProxy.gameObject.SetActive(value: true);
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}
}
