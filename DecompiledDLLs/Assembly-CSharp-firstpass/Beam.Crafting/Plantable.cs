using UnityEngine;

namespace Beam.Crafting;

public class Plantable : MonoBehaviour
{
	[SerializeField]
	private PlantType _plantType;

	public PlantType PlantType => _plantType;
}
