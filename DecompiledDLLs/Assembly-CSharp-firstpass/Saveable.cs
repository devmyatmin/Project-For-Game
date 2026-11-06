using Beam.Serialization.Json;
using Funlabs;

public abstract class Saveable : MultiplayerBehaviour, ISaveable
{
	public abstract JObject Save();

	public abstract void Load(JObject data);
}
