using System.Collections.Generic;
using Beam.Serialization;
using Beam.Serialization.Json;

namespace Beam.Crafting;

public class CraftingSubcategory : ISaveable
{
	public string Name = "Subcategory Name Here";

	public string Term = "";

	public List<CraftingCombination> Combinations = new List<CraftingCombination>();

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("Name", Name.Trim());
		jObject.AddField("Term", Term.Trim());
		JObject jObject2 = new JObject();
		foreach (CraftingCombination combination in Combinations)
		{
			JObject jObject3 = SavingUtilities.Save(combination);
			if (jObject3 != null)
			{
				jObject2.Add(jObject3);
			}
		}
		jObject.AddField("Combinations", jObject2);
		return jObject;
	}

	public void Load(JObject data)
	{
		Name = data.GetField("Name").GetValue<string>();
		JObject field = data.GetField("Term");
		if (field != null)
		{
			Term = field.GetValue<string>();
		}
		JObject field2 = data.GetField("Combinations");
		if (field2 == null || field2.IsNull())
		{
			return;
		}
		foreach (JObject child in field2.Children)
		{
			CraftingCombination craftingCombination = new CraftingCombination();
			if (SavingUtilities.Load(craftingCombination, child))
			{
				Combinations.Add(craftingCombination);
			}
		}
	}
}
