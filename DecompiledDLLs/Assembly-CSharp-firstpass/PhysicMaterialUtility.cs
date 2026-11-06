using Beam.Utilities;
using UnityEngine;

public static class PhysicMaterialUtility
{
	public const string WOOD = "Wood";

	public const string ROCK = "Rock";

	public const string METAL = "Metal";

	public const string FLESH = "Flesh";

	public const string FLESH_BREAK = "Flesh_Break";

	public const string LEAVES = "Leaves";

	public const string FEATHERS = "Feathers";

	public static PhysicMaterial GetPhysicMaterialType(Collider collider)
	{
		if (!BeamTeam.IsNullOrDestroyed(collider))
		{
			PhysicMaterial sharedMaterial = collider.sharedMaterial;
			return GetPhysicMaterialType((sharedMaterial != null) ? ((Object)sharedMaterial).name : null);
		}
		return PhysicMaterial.ROCK;
	}

	public static PhysicMaterial GetPhysicMaterialType(string name)
	{
		return name switch
		{
			"Leaves" => PhysicMaterial.LEAVES, 
			"Wood" => PhysicMaterial.WOOD, 
			"Rock" => PhysicMaterial.ROCK, 
			"Metal" => PhysicMaterial.METAL, 
			"Flesh" => PhysicMaterial.FLESH, 
			"Flesh_Break" => PhysicMaterial.FLESH_BREAK, 
			"Feathers" => PhysicMaterial.FEATHERS, 
			_ => PhysicMaterial.NONE, 
		};
	}

	public static PhysicMaterialPenetration GetPenetration(string name)
	{
		return GetPenetration(GetPhysicMaterialType(name));
	}

	public static PhysicMaterialPenetration GetPenetration(PhysicMaterial material)
	{
		return material switch
		{
			PhysicMaterial.LEAVES => PhysicMaterialPenetration.THROUGH, 
			PhysicMaterial.WOOD => PhysicMaterialPenetration.STICK, 
			PhysicMaterial.ROCK => PhysicMaterialPenetration.BREAK, 
			PhysicMaterial.METAL => PhysicMaterialPenetration.BREAK, 
			PhysicMaterial.FLESH => PhysicMaterialPenetration.STICK, 
			PhysicMaterial.FEATHERS => PhysicMaterialPenetration.STICK, 
			PhysicMaterial.FLESH_BREAK => PhysicMaterialPenetration.BREAK_DAMAGE, 
			_ => PhysicMaterialPenetration.STICK, 
		};
	}
}
