using System.Runtime.CompilerServices;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Serialization.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Beam;

public class Interactive_BIRD : InteractiveObject, ISkinnable, IDisintegratable, IBase, IHasCraftingType, INeedSkills, IKillable
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private FlockChild _flockChild;

	[SerializeField]
	private Collider _interactiveCollider;

	[PREFAB_PROCESSOR_GetComponentInChildren]
	[SerializeField]
	private Animation _animation;

	[Header("SKINNING")]
	[SerializeField]
	private SkinningRewardType _meatType;

	[SerializeField]
	private SkillType _skillRequired;

	[SerializeField]
	private bool _canSkin;

	[SerializeField]
	private int _quantity;

	[SerializeField]
	private int _leatherQuantity;

	private bool _isAlive = true;

	[FormerlySerializedAs("_onSkinned")]
	[SerializeField]
	private InteracterUnityEvent _skinned;

	public SkinningRewardType MeatType => _meatType;

	public SkillType SkillRequired => _skillRequired;

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

	public bool IsAlive => _isAlive;

	public InteracterUnityEvent OnSkinned => _skinned;

	public override void Damage(float damage, IBase damager)
	{
		if (_flockChild != null)
		{
			IPickupable damager2 = damager as IPickupable;
			BaseUtilities.DoPlayerDamageExperience_Hunting(damager2);
			BaseUtilities.DoPlayerDamageMultipliers_Hunting(ref damage, damager2);
			base.Damage(damage, damager);
		}
	}

	public override void Destroy()
	{
		if (IsAlive)
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
			_flockChild?.Kill();
			_flockChild = null;
			base.IsDamageable = false;
			base.IsKinematic = false;
			base.IsDraggable = true;
			CanPickUp = true;
			_interactiveCollider.enabled = true;
			base.transform.parent = null;
			base.TransformParent = Singleton<StrandedWorld>.Instance.NmlZone.SaveContainer;
			Parent();
		}
	}

	protected override void SetTransformSyncInternal(bool enabled)
	{
		base.SetTransformSyncInternal(enabled: true);
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
			string dieAnimation = _flockChild.DieAnimation;
			_flockChild.enabled = false;
			_flockChild = null;
			_animation.playAutomatically = false;
			_animation[dieAnimation].normalizedTime = 1f;
			_animation.Play(dieAnimation);
			_animation.Sample();
			_animation.Stop();
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
