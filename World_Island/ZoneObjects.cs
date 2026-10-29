using Beam.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

public class ZoneObjects : SerializedMonoBehaviour
{
	[Space]
	[Header("GENERATION TYPE")]
	public ZoneGenerationType generationType;

	public bool spawnOnStartingIsland = true;

	[Space]
	[Header("OBJECTS")]
	[SerializeField]
	private ZoneObject[] _objects;

	[Space]
	[Header("TOOLS")]
	public bool occupyPoint;

	public int maxObjectCount;

	[Range(0f, 100f)]
	public int spawnChance;

	[Name("Density")]
	[Range(0f, 64f)]
	public int detailAmountFactor;

	[Space]
	[Header("SOILMAP / ELEVATION")]
	[Name("Soilmap Min Strength")]
	public float minStrength;

	[Name("Soilmap Max Strength")]
	public float maxStrength = 1f;

	[Space]
	[Header("HEIGHTS")]
	[Name("World Min Height")]
	[Range(-200f, 15f)]
	public float minHeight;

	[Name("World Max Height")]
	[Range(-200f, 15f)]
	public float maxHeight;

	[Space]
	[Header("SLOPE")]
	[Range(1f, 90f)]
	public float maxSteepness;

	[Range(0f, 90f)]
	public float minSteepness;

	[Space]
	[Header("PLACEMENT")]
	[Name("Random Rotation")]
	public bool randomFullRotation;

	[Name("Random Y Rotation")]
	public bool randomRotation;

	[Name("Snap to Terrain")]
	public bool terrainSnap;

	[Range(0f, 1f)]
	public float TerrainSnapBias = 1f;

	public Vector3 manualPosition;

	public Vector3 offsetPosition;

	public Vector3 minPositionOffset;

	public Vector3 maxPositionOffset;

	public ZoneObject[] Objects => _objects;

	private bool CheckGenerationIsProcedural()
	{
		return generationType == ZoneGenerationType.Procedural;
	}

	private bool CheckGenerationIsManual()
	{
		return generationType == ZoneGenerationType.Position;
	}
}
