using System.Runtime.CompilerServices;
using UnityEngine;

namespace Beam.Crafting;

public class InteractiveObject_LANTERN : InteractiveObject, IAttachable, IPickupable, IAnimatablePrimary, ICraftingDisplayableStorable, IDisplayableStorable, IStorable, IBase, IHasCraftingType, ISaveablePrefab, ISaveableReference, ISaveable, ICraftingMaterial, IPhysical
{
	[SerializeField]
	private Light _light;

	[SerializeField]
	private GameObject _particles;

	private MaterialPropertyBlock _propertyBlock;

	[Header("ATTACHING")]
	[SerializeField]
	private bool _canAttach;

	[SerializeField]
	private StaticAttachingAnchor _attachingAnchor;

	public bool CanAttach
	{
		get
		{
			return _canAttach;
		}
		set
		{
			_canAttach = value;
		}
	}

	public bool IsAttached { get; protected set; }

	public IAttachingAnchor AttachingAnchor => _attachingAnchor;

	public event AttachableEventHandler<IAttachable> DetachedEvent;

	public override string GetInteractionDescription(int playerId, IBase obj)
	{
		if (!_light.enabled)
		{
			return "ITEM_INTERACTION_DESCRIPTION_TURN_ON";
		}
		return "ITEM_INTERACTION_DESCRIPTION_TURN_OFF";
	}

	protected virtual void OnDetached(IAttachable sender)
	{
		this.DetachedEvent?.Invoke(sender);
		this.DetachedEvent = null;
	}

	protected override void Start()
	{
		base.Start();
		_light.enabled = !Singleton<Atmosphere>.Instance.DayTime;
		_particles.SetActive(!Singleton<Atmosphere>.Instance.DayTime);
		SetEmission();
		Singleton<Atmosphere>.Instance.SunsetEvent += TriggerLight;
	}

	public override void OnDestroy()
	{
		if ((bool)Singleton<Atmosphere>.Instance)
		{
			Singleton<Atmosphere>.Instance.SunsetEvent -= TriggerLight;
		}
		this.DetachedEvent = null;
		base.OnDestroy();
	}

	public override bool Store()
	{
		Detach();
		return base.Store();
	}

	public void Attach(IAttachingAnchor anchor)
	{
		if (!IsAttached)
		{
			IsAttached = true;
			DisablePhysics();
		}
	}

	public void Detach()
	{
		if (IsAttached)
		{
			IsAttached = false;
			OnDetached(this);
		}
	}

	public override bool Interact(IPlayer player)
	{
		TriggerLight(manual: true);
		return base.Interact(player);
	}

	public override void Use()
	{
		base.Use();
		TriggerLight(manual: true);
	}

	private void TriggerLight()
	{
		TriggerLight(manual: false);
	}

	private void TriggerLight(bool manual)
	{
		if (manual)
		{
			_light.enabled = !_light.enabled;
			_particles.SetActive(!Singleton<Atmosphere>.Instance.DayTime && !_particles.activeSelf);
		}
		else
		{
			_light.enabled = !Singleton<Atmosphere>.Instance.DayTime;
			_particles.SetActive(!Singleton<Atmosphere>.Instance.DayTime);
		}
		SetEmission();
	}

	private void SetEmission()
	{
		if (_propertyBlock == null)
		{
			_propertyBlock = new MaterialPropertyBlock();
		}
		for (int i = 0; i < base.renderers.Length; i++)
		{
			base.renderers[i].GetPropertyBlock(_propertyBlock);
			_propertyBlock.SetFloat("_Multiplier", _light.enabled ? 20f : 0f);
			base.renderers[i].SetPropertyBlock(_propertyBlock);
		}
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}
}
