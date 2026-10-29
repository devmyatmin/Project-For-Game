using System.Collections.Generic;
using System.Linq;
using Beam.Language;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using UnityEngine;

namespace Beam.Crafting;

public class CraftingCombination : ISaveable
{
	public string Name = "Combination Name Here";

	public string Term = "";

	public short SelfIndex = -1;

	public string Description = "";

	public bool IsDynamic;

	public bool IsExtension;

	public bool Lockable;

	public bool Unlocked = true;

	public int Limit = -1;

	public int CraftedCount;

	public float ExpRewardOnCraft;

	public int CraftsmanshipLevelRequired;

	public List<CraftingType> Materials = new List<CraftingType>();

	public Sprite Sprite;

	private CombinationInfo _info;

	public string PrefabAssetPath { get; set; }

	public string SpriteAssetPath { get; set; }

	public CraftingType CraftingType { get; set; } = new CraftingType(AttributeType.None, InteractiveType.None);


	public bool IsLimited
	{
		get
		{
			if (Limit != -1)
			{
				return CraftedCount >= Limit;
			}
			return false;
		}
	}

	public bool IsLocked
	{
		get
		{
			if (Lockable)
			{
				return !Unlocked;
			}
			return false;
		}
	}

	public CombinationInfo GetInfo()
	{
		return _info ?? (_info = new CombinationInfo(this));
	}

	public bool IsValid(CraftingInfo craftingInfo)
	{
		if (IsDynamic)
		{
			if (craftingInfo.CurrentPrefabId == 0)
			{
				return false;
			}
			if (!craftingInfo.CurrentType.IsPileType())
			{
				return false;
			}
			int num = 0;
			foreach (CraftingType currentMaterial in craftingInfo.CurrentMaterials)
			{
				if (currentMaterial == craftingInfo.CurrentType)
				{
					num++;
				}
			}
			if (num > 1)
			{
				CraftingType = craftingInfo.CurrentType;
				string str = Localization.GetLanguageHandler().Localize("ITEM_PILE_TYPE_SENTENCE");
				Term = string_safemethods.Format(str, craftingInfo.CurrentType.ToString(), "INTERACTIVE_TYPE_CRAFTING_ITEM_PILE");
				Materials = new List<CraftingType> { craftingInfo.CurrentType, craftingInfo.CurrentType };
				return true;
			}
			return false;
		}
		if (Materials.Count > craftingInfo.CurrentMaterials.Count())
		{
			return false;
		}
		if (CraftsmanshipLevelRequired > craftingInfo.CurrentCraftsmanshipLevel)
		{
			return false;
		}
		bool flag = true;
		foreach (IGrouping<CraftingType, CraftingType> uniqueType2 in Materials.Skip(IsExtension ? 1 : 0).ToLookup((CraftingType uniqueType) => uniqueType))
		{
			if (uniqueType2.Count() > craftingInfo.CurrentMaterials.Where((CraftingType material) => material.Equals(uniqueType2.Key.AttributeType, uniqueType2.Key.InteractiveType)).Count())
			{
				flag = false;
			}
		}
		if (IsExtension)
		{
			if (flag)
			{
				return craftingInfo.CurrentMaterials.Any_NonAlloc((CraftingType c, List<CraftingType> materials) => c.InteractiveType == materials.FirstOrDefault_NonAlloc()?.InteractiveType, Materials);
			}
			return false;
		}
		return flag;
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("Name", JSerializer.Serialize(Name.Trim()));
		jObject.AddField("Term", JSerializer.Serialize(Term.Trim()));
		jObject.AddField("Description", JSerializer.Serialize(Description.Trim()));
		jObject.AddField("PrefabAssetPath", JSerializer.Serialize(PrefabAssetPath.Trim()));
		jObject.AddField("SpriteAssetPath", JSerializer.Serialize(SpriteAssetPath.Trim()));
		jObject.AddField("Unlocked", JSerializer.Serialize(Unlocked));
		jObject.AddField("Lockable", JSerializer.Serialize(Lockable));
		jObject.AddField("Limit", JSerializer.Serialize(Limit));
		jObject.AddField("IsDynamic", JSerializer.Serialize(IsDynamic));
		jObject.AddField("IsExtension", JSerializer.Serialize(IsExtension));
		jObject.AddField("ExpRewardOnCraft", JSerializer.Serialize(ExpRewardOnCraft));
		jObject.AddField("CraftsmanshipLevelRequired", JSerializer.Serialize(CraftsmanshipLevelRequired));
		JObject data = SavingUtilities.Save(CraftingType);
		jObject.AddField("CraftingType", data);
		JObject jObject2 = new JObject();
		int num = 0;
		foreach (CraftingType material in Materials)
		{
			jObject2.AddField("[" + num++ + "]Material", material.Save());
		}
		jObject.AddField("Materials", jObject2);
		return jObject;
	}

	public void Load(JObject data)
	{
		string value = data.GetField("Name").GetValue<string>();
		Name = value;
		JObject field = data.GetField("Term");
		if (field != null)
		{
			Term = field.GetValue<string>();
		}
		string value2 = data.GetField("Description").GetValue<string>();
		Description = value2;
		string value3 = data.GetField("PrefabAssetPath").GetValue<string>();
		PrefabAssetPath = value3;
		string value4 = data.GetField("SpriteAssetPath").GetValue<string>();
		SpriteAssetPath = value4;
		if (!string.IsNullOrEmpty(SpriteAssetPath))
		{
			Sprite = Resources.Load<Sprite>(SpriteAssetPath);
		}
		bool value5 = data.GetField("IsExtension").GetValue<bool>();
		IsExtension = value5;
		JObject field2 = data.GetField("IsDynamic");
		if (field2 != null)
		{
			IsDynamic = field2.GetValue<bool>();
		}
		JObject field3 = data.GetField("Lockable");
		if (field3 != null)
		{
			Lockable = field3.GetValue<bool>();
		}
		JObject field4 = data.GetField("Unlocked");
		if (field4 != null)
		{
			Unlocked = field4.GetValue<bool>();
		}
		JObject field5 = data.GetField("Limit");
		if (field5 != null)
		{
			Limit = field5.GetValue<int>();
		}
		JObject field6 = data.GetField("ExpRewardOnCraft");
		if (field6 != null)
		{
			ExpRewardOnCraft = field6.GetValue<float>();
		}
		JObject field7 = data.GetField("CraftsmanshipLevelRequired");
		if (field7 != null)
		{
			CraftsmanshipLevelRequired = field7.GetValue<int>();
		}
		JObject field8 = data.GetField("CraftingType");
		if (field8 != null)
		{
			SavingUtilities.Load(CraftingType, field8);
		}
		JObject field9 = data.GetField("Materials");
		if (field9 == null || field9.IsNull())
		{
			return;
		}
		List<CraftingType> list = new List<CraftingType>();
		foreach (JObject child in field9.Children)
		{
			CraftingType craftingType = new CraftingType(AttributeType.INVALID, InteractiveType.INVALID);
			SavingUtilities.Load(craftingType, child);
			list.Add(craftingType);
		}
		Materials = list;
	}
}
