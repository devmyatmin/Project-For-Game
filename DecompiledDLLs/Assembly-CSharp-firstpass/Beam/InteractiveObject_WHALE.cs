using Beam.Serialization.Json;
using UnityEngine;

namespace Beam;

public class InteractiveObject_WHALE : InteractiveObject
{
	private Whale_Creature _whaleCreature;

	protected override void Start()
	{
		base.Start();
		if (!Object.op_Implicit((Object)(object)_whaleCreature))
		{
			_whaleCreature = ((Component)this).GetComponent<Whale_Creature>();
		}
	}

	public override JObject Save()
	{
		return base.Save();
	}

	public override void Load(JObject data)
	{
		base.Load(data);
	}
}
