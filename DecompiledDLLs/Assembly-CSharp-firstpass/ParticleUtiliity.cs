using UnityEngine;

public class ParticleUtiliity : MonoBehaviour
{
	private static ParticleUtiliity _Instance;

	public GameObject LeafParticles;

	public GameObject WoodParticles;

	public GameObject RockParticles;

	public GameObject MetalParticles;

	public GameObject FleshParticles;

	public GameObject FeatherParticles;

	public static ParticleUtiliity Instance
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_Instance))
			{
				_Instance = Object.FindObjectOfType<ParticleUtiliity>();
			}
			return _Instance;
		}
	}

	public static GameObject GetParticleFromMaterial(string name)
	{
		return PhysicMaterialUtility.GetPhysicMaterialType(name) switch
		{
			PhysicMaterial.LEAVES => Instance.LeafParticles, 
			PhysicMaterial.WOOD => Instance.WoodParticles, 
			PhysicMaterial.ROCK => Instance.RockParticles, 
			PhysicMaterial.METAL => Instance.MetalParticles, 
			PhysicMaterial.FLESH => Instance.FleshParticles, 
			PhysicMaterial.FEATHERS => Instance.FeatherParticles, 
			_ => (GameObject)null, 
		};
	}
}
