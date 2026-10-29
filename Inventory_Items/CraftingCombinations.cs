using System.Collections.Generic;
using System.Linq;
using Beam.Serialization;
using Beam.Serialization.Json;
using Funlabs;
using UnityEngine;

namespace Beam.Crafting;

public class CraftingCombinations : ISaveable
{
	private static CraftingCombinations s_combinations;

	private static bool _initialized;

	private List<CraftingCategory> _categories = new List<CraftingCategory>();

	private List<CraftingCombination> _combinations = new List<CraftingCombination>();

	public IList<CraftingCategory> Categories => _categories;

	public IList<CraftingCombination> Combinations => _combinations;

	public static CraftingCombinations Create()
	{
		CraftingCombinations craftingCombinations = new CraftingCombinations();
		craftingCombinations.Load();
		return craftingCombinations;
	}

	private static IList<CraftingCombination> GetCombinations()
	{
		if (!_initialized)
		{
			s_combinations = Create();
			_initialized = true;
		}
		return s_combinations.Combinations;
	}

	public static Sprite GetCombinationIcon(string name)
	{
		return GetCombinations().FirstOrDefault((CraftingCombination combo) => combo.Name == name)?.Sprite;
	}

	public static string GetCombinationTerm(string name)
	{
		CraftingCombination craftingCombination = GetCombinations().FirstOrDefault((CraftingCombination combo) => combo.Name == name);
		if (craftingCombination != null)
		{
			return craftingCombination.Term;
		}
		return name;
	}

	protected CraftingCombinations()
	{
	}

	public void Load()
	{
		_categories.Clear();
		TextAsset textAsset = Resources.Load<TextAsset>("CraftingCombinations");
		if (textAsset != null)
		{
			JObject jObject = new JObject(textAsset.ToString());
			if (!jObject.IsNull())
			{
				foreach (JObject child in jObject.Children)
				{
					CraftingCategory craftingCategory = new CraftingCategory();
					if (SavingUtilities.Load(craftingCategory, child))
					{
						_categories.Add(craftingCategory);
					}
				}
				_combinations = _categories.SelectMany((CraftingCategory cat) => cat.Subcategories).SelectMany((CraftingSubcategory subcat) => subcat.Combinations).ToList();
			}
			else
			{
				Debug.LogError("CraftingCombinations:: Category data was empty");
			}
			Resources.UnloadAsset(textAsset);
		}
		else
		{
			Debug.LogError("CraftingCombinations:: Error loading combinations file.");
		}
		MultiplayerMng.Log($"Crafting Combinations {Combinations.Count}");
		for (int i = 0; i < Combinations.Count; i++)
		{
			Combinations[i].SelfIndex = (short)i;
		}
	}

	public CraftingCombination GetCombination(string name)
	{
		return _combinations.FirstOrDefault((CraftingCombination combo) => combo.Name == name);
	}

	public void UnlockCombination(string name)
	{
		CraftingCombination combination = GetCombination(name);
		if (combination != null)
		{
			combination.Unlocked = true;
		}
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		foreach (CraftingCombination combination in Combinations)
		{
			JObject jObject2 = new JObject();
			jObject2.AddField("Name", combination.Name);
			jObject2.AddField("Unlocked", combination.Unlocked);
			jObject2.AddField("CraftedCount", combination.CraftedCount);
			jObject.Add(jObject2);
		}
		return jObject;
	}

	public void Load(JObject data)
	{
		if (data == null || data.IsNull())
		{
			Debug.LogError("CraftingCombinations:: Data was null or empty.");
			return;
		}
		foreach (JObject child in data.Children)
		{
			string name = child.GetField("Name").GetValue<string>();
			CraftingCombination craftingCombination = Combinations.FirstOrDefault((CraftingCombination combo) => combo.Name == name);
			if (craftingCombination == null)
			{
				Debug.LogWarning("CraftingCombinations::Load:: Could not find combination named [" + name + "]");
				continue;
			}
			bool value = child.GetField("Unlocked").GetValue<bool>();
			craftingCombination.Unlocked = value;
			int value2 = child.GetField("CraftedCount").GetValue<int>();
			craftingCombination.CraftedCount = value2;
		}
		LegacyUtilities.LoadCompletedMissionGyroPieces(this);
	}
}
