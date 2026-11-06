using UnityEngine;

namespace Beam;

public class DamageParticleHelper : InteractiveParticleHelper
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private BaseObject _base;
}
