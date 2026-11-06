using System.Collections.Generic;
using Beam.Serialization;
using Beam.Serialization.Json;

namespace Beam.Crafting;

public class CraftingCategory : ISaveable
{
	public string Name = "Category Name Here";

	public string Term = "";

	public CraftingType ToolRequired = new CraftingType(AttributeType.None, InteractiveType.None);

	public List<CraftingSubcategory> Subcategories = new List<CraftingSubcategory>();

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("Name", Name.Trim());
		jObject.AddField("Term", Term.Trim());
		JObject jObject2 = SavingUtilities.Save(ToolRequired);
		if (jObject2 != null)
		{
			jObject.AddField("ToolRequired", jObject2);
		}
		JObject jObject3 = new JObject();
		foreach (CraftingSubcategory subcategory in Subcategories)
		{
			JObject jObject4 = SavingUtilities.Save(subcategory);
			if (jObject4 != null)
			{
				jObject3.Add(jObject4);
			}
		}
		jObject.AddField("Subcategories", jObject3);
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
		JObject field2 = data.GetField("ToolRequired");
		ToolRequired = new CraftingType(AttributeType.INVALID, InteractiveType.INVALID);
		SavingUtilities.Load(ToolRequired, field2);
		JObject field3 = data.GetField("Subcategories");
		if (field3 == null || field3.IsNull())
		{
			return;
		}
		foreach (JObject child in field3.Children)
		{
			CraftingSubcategory craftingSubcategory = new CraftingSubcategory();
			if (SavingUtilities.Load(craftingSubcategory, child))
			{
				Subcategories.Add(craftingSubcategory);
			}
		}
	}
}
