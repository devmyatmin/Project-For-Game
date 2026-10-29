using Beam.Serialization.Json;

namespace Beam;

public class InteractiveObject_WHALE : InteractiveObject
{
	private Whale_Creature _whaleCreature;

	protected override void Start()
	{
		base.Start();
		if (!_whaleCreature)
		{
			_whaleCreature = GetComponent<Whale_Creature>();
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
