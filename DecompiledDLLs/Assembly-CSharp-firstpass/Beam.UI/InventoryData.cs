using Beam.Utilities;
using Beam.Utilities.Reflection;
using UnityEngine;

namespace Beam.UI;

public class InventoryData : Data<IPickupable>
{
	public InventoryData(IPickupable data)
	{
		Value = data;
	}

	public override string GetDisplayName()
	{
		if (Value == null)
		{
			return string.Empty;
		}
		return Value.GetDisplayName();
	}

	public override string GetDescription()
	{
		if (Value == null)
		{
			return string.Empty;
		}
		return Value.GetDescription();
	}

	public Sprite GetIcon()
	{
		if (BeamTeam.IsNullOrDestroyed(Value))
		{
			return null;
		}
		IconKey key = new IconKey
		{
			CraftingType = Value.CraftingType
		};
		if (Value is InteractiveObject_FOOD interactiveObject_FOOD)
		{
			interactiveObject_FOOD.CheckSpoiled();
			if (interactiveObject_FOOD.Spoiled)
			{
				key.CustomType = ECustomType.Spoiled;
			}
			else if (Object.op_Implicit((Object)(object)interactiveObject_FOOD.Smoking) && interactiveObject_FOOD.Smoking.IsSmoked)
			{
				key.CustomType = ECustomType.Smoked;
			}
			else if (Object.op_Implicit((Object)(object)interactiveObject_FOOD.Cooking) && interactiveObject_FOOD.Cooking.IsCooked)
			{
				key.CustomType = ECustomType.Cooked;
			}
		}
		return Icons.GetMediumIcon(key);
	}

	public virtual string GetClassification()
	{
		if (Value == null)
		{
			return string.Empty;
		}
		InteractiveObjectClassification classification = Value.GetClassification();
		if (classification == InteractiveObjectClassification.NONE)
		{
			return string.Empty;
		}
		return Reflection.GetAttribute<DescriptionAttribute>(classification).Text;
	}

	public string GetHealth()
	{
		if (Value == null)
		{
			return string.Empty;
		}
		return Value.GetHealthTerm();
	}

	public float GetHealthNormalized()
	{
		if (Value == null)
		{
			return 1f;
		}
		return Value.GetHealthUnits();
	}

	public float GetHealthValue()
	{
		if (Value == null)
		{
			return 1f;
		}
		return GetHealthNormalized() * Value.GetHealthRange();
	}
}
