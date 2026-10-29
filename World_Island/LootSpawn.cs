using System;
using UnityEngine;

namespace Beam.Crafting;

public static class LootSpawn
{
	[Serializable]
	public class LootItem
	{
		public float Weight;

		public GameObject Item;
	}

	public static LootItem ChooseItem(LootItem[] Items, float percentage)
	{
		float num = 0f;
		foreach (LootItem lootItem in Items)
		{
			num += lootItem.Weight;
		}
		float num2 = percentage * num;
		for (int j = 0; j < Items.Length; j++)
		{
			if (num2 < Items[j].Weight)
			{
				return Items[j];
			}
			num2 -= Items[j].Weight;
		}
		return Items[0];
	}
}
