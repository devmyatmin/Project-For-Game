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
			((Component)_arrowProxy).gameObject.SetActive(false);
			_reloaded = false;
		}
		if ((Object)(object)_bowString != (Object)null)
		{
			((Behaviour)_bowString).enabled = holding;
		}
	}

	public bool CanReloadWith(IPickupable pickupable)
	{
		return pickupable.CraftingType.InteractiveType == _arrowPrefab.CraftingType.InteractiveType;
	}

	public void Reload(IPickupable pickupable)
	{
		Interacting_ARROW interacting_ARROW;
		if (!BeamTeam.IsNullOrDestroyed(pickupable))
		{
			interacting_ARROW = pickupable as Interacting_ARROW;
			if ((Object)(object)interacting_ARROW != (Object)null && CanReloadWith(interacting_ARROW))
			{
				CreateArrowProxy(interacting_ARROW);
				_reloaded = true;
				UnityEvent onReload = _onReload;
				if (onReload != null)
				{
					onReload.Invoke();
				}
			}
			return;
		}
		interacting_ARROW = Owner.Holder.Storage.Get<Interacting_ARROW>(_arrowPrefab.CraftingType);
		if ((Object)(object)interacting_ARROW != (Object)null)
		{
			CreateArrowProxy(interacting_ARROW);
			_reloaded = true;
			UnityEvent onReload2 = _onReload;
			if (onReload2 != null)
			{
				onReload2.Invoke();
			}
		}
	}

	public bool ValidateSecondary(IBase obj)
	{
		if (!BeamTeam.IsNullOrDestroyed(obj))
		{
			Interacting_ARROW interacting_ARROW = obj as Interacting_ARROW;
			if ((Object)(object)interacting_ARROW != (Object)null && CanReloadWith(interacting_ARROW))
			{
				return true;
			}
		}
		else
		{
			Interacting_ARROW interacting_ARROW = Owner.Holder.Storage.Get<Interacting_ARROW>(_arrowPrefab.CraftingType);
			if ((Object)(object)interacting_ARROW != (Object)null)
			{
				return true;
			}
		}
		return false;
	}

	public void PreSecondary()
	{
		if ((Object)(object)_bowString != (Object)null)
		{
			_bowString.Pullback();
		}
		ActionEventData.Populate(this, null);
		((UnityEvent<IBase, IBaseActionEventData>)_charged).Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
	}

	public void UseSecondary()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		Interacting_ARROW interacting_ARROW = Owner.Holder.Storage.Get<Interacting_ARROW>(_arrowPrefab.CraftingType);
		if ((Object)(object)interacting_ARROW != (Object)null)
		{
			Owner.Holder.Storage.Pop(interacting_ARROW);
			BeamRay ray = Owner.PlayerCamera.GetRay(5f);
			float num = Vector3.Distance(((Component)_arrowProxy).transform.position, ray.OriginPoint);
			interacting_ARROW.transform.position = ray.OriginPoint;
			interacting_ARROW.transform.LookAt(ray.EndPoint);
			Transform val = interacting_ARROW.transform;
			val.position += interacting_ARROW.transform.forward * num;
			Physics.IgnoreCollision(((Component)interacting_ARROW).GetComponent<Collider>(), ((Component)this).GetComponent<Collider>(), true);
			((Behaviour)interacting_ARROW.ProjectileMovement).enabled = true;
			interacting_ARROW.ProjectileMovement.Fire(Owner);
			interacting_ARROW.rigidbody.AddForce(Owner.Movement.CharacterController.velocity + interacting_ARROW.transform.forward * _arrowSpeed * Owner.PlayerSkills.HuntingMultiplier, (ForceMode)1);
			((Component)_arrowProxy).gameObject.SetActive(false);
			_reloaded = false;
			if (Object.op_Implicit((Object)(object)_bowString))
			{
				_bowString.Release();
			}
			ActionEventData.Populate(this, null);
			((UnityEvent<IBase, IBaseActionEventData>)_usedSecondary).Invoke((IBase)this, (IBaseActionEventData)ActionEventData);
		}
	}

	public void PostSecondary()
	{
		if ((Object)(object)_bowString != (Object)null)
		{
			_bowString.Pullback_Release();
		}
	}

	private void CreateArrowProxy(Interacting_ARROW arrow)
	{
		_arrowProxy.MeshFilter.sharedMesh = arrow.Proxy.MeshFilter.sharedMesh;
		((Renderer)_arrowProxy.MeshRenderer).sharedMaterial = ((Renderer)arrow.Proxy.MeshRenderer).sharedMaterial;
		((Component)_arrowProxy).gameObject.SetActive(true);
	}
}
