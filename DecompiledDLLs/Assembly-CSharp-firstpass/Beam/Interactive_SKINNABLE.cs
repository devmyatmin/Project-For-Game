using Beam.Serialization.Json;
using UnityEngine;

namespace Beam;

public class Interactive_SKINNABLE : InteractiveObject, ISkinnable, IDisintegratable, IBase, IHasCraftingType, INeedSkills
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

	public bool IsAlive { get; set; }

	public bool Disintegrating { get; set; }

	public InteracterUnityEvent OnSkinned => _onSkinned;

	public override JObject Save()
	{
		JObject jObject = base.Save();
		JObject data = this.SaveSkinnable();
		jObject.AddField("Skinnable", data);
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
	}
}
