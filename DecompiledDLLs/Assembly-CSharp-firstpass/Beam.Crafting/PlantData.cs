using System;
using UnityEngine;

namespace Beam.Crafting;

[Serializable]
public class PlantData
{
	[Tooltip("Time to generate a fruit, in game hours.")]
	public float HarvestTime;

	public Plant PlantPrefab;

	public InteractiveObject_FOOD HarvestedPrefab;
}
