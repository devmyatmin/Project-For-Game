using Beam.Serialization.Json;
using Funlabs;

public abstract class SaveableReference : Saveable, ISaveableReference, ISaveable
{
	private MiniGuid _referenceId;

	public MiniGuid ReferenceId
	{
		get
		{
			if (_referenceId.IsDefault())
			{
				_referenceId = MiniGuid.New();
			}
			return _referenceId;
		}
		set
		{
			_referenceId = value;
		}
	}

	public override JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("name", JSerializer.Serialize(base.gameObject.name));
		jObject.AddField("reference", JSerializer.Serialize(ReferenceId.ToString()));
		return jObject;
	}

	public override void Load(JObject data)
	{
	}
}
