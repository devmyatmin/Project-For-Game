using Beam.Crafting;
using UnityEngine;

namespace Beam;

public class InteractiveObject_PICK : ConstructionObject
{
	private float _pickMiningDamage = 20f;

	public override void UseOnObject(IBase obj)
	{
		InteractiveObject_MININGROCK interactiveObject_MININGROCK = obj as InteractiveObject_MININGROCK;
		if ((Object)(object)interactiveObject_MININGROCK != (Object)null && interactiveObject_MININGROCK.RequirePick)
		{
			interactiveObject_MININGROCK.PickMine(_pickMiningDamage, this);
			DamageObject(obj);
		}
		else
		{
			base.UseOnObject(obj);
		}
	}
}
