using UnityEngine;

namespace Beam;

public class ArmEffect : ArmEffectBase
{
	[SerializeField]
	private string _effectName;

	protected override bool CheckPlayerEffect(PlayerEffect effect)
	{
		return effect.Name == _effectName;
	}
}
