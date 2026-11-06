using System;
using System.Collections.Generic;
using Beam.Crafting;
using UnityEngine;

namespace Beam.UI;

public static class Icons
{
	private static Dictionary<IconKey, Sprite> _smallIcons;

	private static Dictionary<IconKey, Sprite> _mediumIcons;

	static Icons()
	{
		_smallIcons = new Dictionary<IconKey, Sprite>();
		_mediumIcons = new Dictionary<IconKey, Sprite>();
		Load();
	}

	private static void Load()
	{
		LoadIcons(Resources.LoadAll<Sprite>("Icons/Small"), _smallIcons);
		LoadIcons(Resources.LoadAll<Sprite>("Icons/Medium"), _mediumIcons);
	}

	private static void LoadIcons(IEnumerable<Sprite> sprites, IDictionary<IconKey, Sprite> collection)
	{
		collection.Clear();
		foreach (Sprite sprite in sprites)
		{
			string[] array = ((Object)sprite).name.Split('-');
			AttributeType attributeType = AttributeType.None;
			InteractiveType interactiveType = InteractiveType.INVALID;
			ECustomType customType = ECustomType.None;
			try
			{
				string value = ((array.Length > 1) ? array[1] : string.Empty);
				if (array.Length > 1)
				{
					attributeType = (AttributeType)Enum.Parse(typeof(AttributeType), value, ignoreCase: true);
				}
			}
			catch (Exception)
			{
				if (array.Length <= 1)
				{
					throw;
				}
				Debug.LogError((object)("Icons::LoadIcons:: Could not parse AttributeType for sprite asset with name [" + ((Object)sprite).name + "]"));
			}
			try
			{
				string value2 = array[0];
				interactiveType = (InteractiveType)Enum.Parse(typeof(InteractiveType), value2, ignoreCase: true);
			}
			catch (Exception)
			{
				Debug.LogError((object)("Icons::LoadIcons:: Could not parse InteractiveType for sprite asset with name [" + ((Object)sprite).name + "]"));
				continue;
			}
			try
			{
				string value3 = ((array.Length > 2) ? array[2] : string.Empty);
				if (!string.IsNullOrWhiteSpace(value3))
				{
					customType = (ECustomType)Enum.Parse(typeof(ECustomType), value3, ignoreCase: true);
				}
			}
			catch (Exception)
			{
				if (array.Length <= 2)
				{
					throw;
				}
				Debug.LogError((object)("Icons::LoadIcons:: Could not parse custom type for sprite asset with name [" + ((Object)sprite).name + "]"));
			}
			IconKey key = new IconKey
			{
				CraftingType = new CraftingType(attributeType, interactiveType),
				CustomType = customType
			};
			if (!collection.ContainsKey(key))
			{
				collection.Add(key, sprite);
			}
		}
	}

	public static Sprite GetSmallIcon(IconKey key)
	{
		if (!_smallIcons.TryGetValue(key, out var value))
		{
			_smallIcons.TryGetValue(new IconKey
			{
				CraftingType = key.CraftingType
			}, out value);
		}
		return value;
	}

	public static Sprite GetMediumIcon(IconKey key)
	{
		if (!_mediumIcons.TryGetValue(key, out var value))
		{
			_mediumIcons.TryGetValue(new IconKey
			{
				CraftingType = key.CraftingType
			}, out value);
		}
		return value;
	}
}
