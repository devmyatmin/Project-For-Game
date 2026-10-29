using System.Runtime.CompilerServices;
using Beam;
using Beam.Serialization.Json;
using UnityEngine;

public class InteractiveObject_RAGDOLL : InteractiveObject, IRagdoll, IBase, IHasCraftingType, ISaveablePrefab, ISaveableReference, ISaveable, ISkinnable, IDisintegratable, INeedSkills
{
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

	[SerializeField]
	[HideInInspector]
	private InteracterUnityEvent _onSkinned;

	[PREFAB_PROCESSOR_GetComponentsInChildren]
	[SerializeField]
	private RagdollChild[] _ragdollChildren;

	public SkinningRewardType MeatType => _meatType;

	public SkillType SkillRequired => _skillRequired;

	public bool CanSkin
	{
		get
		{
			return _canSkin;
		}
		set
		{
			_canSkin = value;
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

	public InteracterUnityEvent OnSkinned => _onSkinned;

	public RagdollChild[] RagdollChildren => _ragdollChildren;

	public override JObject Save()
	{
		JObject jObject = base.Save();
		JObject data = this.SaveRagdoll();
		jObject.AddField("Ragdolls", data);
		JObject data2 = this.SaveSkinnable();
		jObject.AddField("Skinnable", data2);
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JObject field = data.GetField("Skinnable");
		if (field != null)
		{
			this.LoadSkinnable(field);
		}
		JObject ragdollData = data.GetField("Ragdolls");
		if (ragdollData != null)
		{
			LevelLoader.CurrentLoader.PhysicalLoading.Add(delegate
			{
				this.LoadRagdoll(ragdollData);
			});
		}
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}
}
