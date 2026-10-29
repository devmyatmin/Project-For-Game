using System.Runtime.CompilerServices;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Serialization.Json;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Beam;

public class InteractiveObject_CRAB : InteractiveObject_ANIMAL, ISpearable, ISpearedInstance, IKillable, ISkinnable, IDisintegratable, IBase, IHasCraftingType, INeedSkills
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private LandCrab _crab;

	[SerializeField]
	private Collider _interactiveCollider;

	[Header("SKINNING")]
	[SerializeField]
	private SkinningRewardType _meatType;

	[SerializeField]
	private SkillType _skillRequired;

	private bool _isAlive = true;

	[SerializeField]
	private int _quantity;

	[SerializeField]
	private int _leatherQuantity;

	private int _lastPlayerIdToDamageThis = -1;

	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("_onKilled")]
	private UnityEvent _killed;

	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("_onSkinned")]
	private InteracterUnityEvent _skinned;

	public SkinningRewardType MeatType => _meatType;

	public SkillType SkillRequired => _skillRequired;

	bool IKillable.IsAlive => _isAlive;

	InteractiveType ISpearable.Type => base.CraftingType.InteractiveType;

	bool ISpearable.CanSpear => true;

	bool ISpearable.IsLocalFauna => false;

	uint ISpearable.LocalFaunaSpearedInstancePrefabId => 0u;

	IPhysical ISpearedInstance.PhysicsObject => this;

	IPickupable ISpearedInstance.PickupableObject => this;

	public bool CanSkin
	{
		get
		{
			return !_isAlive;
		}
		set
		{
		}
	}

	public int Quantity => _quantity;

	public int LeatherQuantity
	{
		get
		{
			return _leatherQuantity;
		}
		set
		{
			_leatherQuantity = value;
		}
	}

	public int Skinned { get; set; }

	public int FailedAttempts { get; set; }

	public bool Disintegrating { get; set; }

	public InteracterUnityEvent OnSkinned => _skinned;

	bool ISpearable.LocalFaunaCheckHit(Vector3 postion)
	{
		return false;
	}

	void ISpearable.LocalFaunaRelease()
	{
	}

	ISpearedInstance ISpearable.GetSpearedInstance(Vector3 position)
	{
		return this;
	}

	public override void Damage(float damage, IBase damager)
	{
		if (_crab != null)
		{
			IPickupable pickupable = damager as IPickupable;
			if (!pickupable.IsNullOrDestroyed())
			{
				BaseUtilities.DoPlayerDamageExperience_Hunting(pickupable);
				BaseUtilities.DoPlayerDamageMultipliers_Hunting(ref damage, pickupable);
			}
			base.Damage(damage, damager);
		}
	}

	public override void Destroy()
	{
		if (_isAlive)
		{
			Kill(LastPlayerIdToDamageThis);
		}
		else
		{
			base.Destroy();
		}
	}

	public void Kill(int killerId = -1)
	{
		if (_isAlive)
		{
			_isAlive = false;
			if (killerId != -1)
			{
				EventManager.RaiseEvent(new ExperienceEvent(killerId, PlayerSkills.Skills.HUNTING, PlayerSkills.ExpSource.SMALL_KILL));
				EventManager.RaiseEvent(new AddStatsEvent(killerId, new Stats
				{
					AnimalsKilled = 1
				}));
			}
			_crab?.Kill();
			_crab = null;
			base.IsDamageable = false;
			base.IsKinematic = false;
			base.IsDraggable = true;
			CanPickUp = true;
			_interactiveCollider.enabled = true;
			base.transform.parent = null;
			base.TransformParent = null;
			Parent();
			_killed?.Invoke();
		}
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("IsAlive", _isAlive);
		JObject data = this.SaveSkinnable();
		jObject.AddField("Skinnable", data);
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JObject field = data.GetField("IsAlive");
		if ((bool)field)
		{
			_isAlive = field.GetValue<bool>();
		}
		if (!_isAlive)
		{
			_crab?.Kill();
			_crab = null;
			base.IsDamageable = false;
			base.IsKinematic = false;
			base.IsDraggable = true;
			CanPickUp = true;
			_interactiveCollider.enabled = true;
		}
		JObject field2 = data.GetField("Skinnable");
		if (field2 != null)
		{
			this.LoadSkinnable(field2);
		}
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}
}
