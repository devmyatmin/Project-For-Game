using Beam.Serialization.Json;
using UnityEngine;

namespace Beam;

public abstract class SaveableBehaviour : MonoBehaviour, ISaveable
{
	public abstract JObject Save();

	public abstract void Load(JObject data);
}
