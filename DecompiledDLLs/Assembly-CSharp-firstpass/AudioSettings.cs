using Beam.Serialization.Json;

public class AudioSettings : ISaveable
{
	public float MasterVolume = 1f;

	public float MusicVolume = 1f;

	public float EnvironmentVolume = 1f;

	public float EffectsVolume = 1f;

	public float VoiceVolume = 1f;

	public float UIVolume = 1f;

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("MasterVolume", JSerializer.Serialize((object)MasterVolume));
		jObject.AddField("MusicVolume", JSerializer.Serialize((object)MusicVolume));
		jObject.AddField("EnvironmentVolume", JSerializer.Serialize((object)EnvironmentVolume));
		jObject.AddField("EffectsVolume", JSerializer.Serialize((object)EffectsVolume));
		jObject.AddField("VoiceVolume", JSerializer.Serialize((object)VoiceVolume));
		jObject.AddField("UIVolume", JSerializer.Serialize((object)UIVolume));
		return jObject;
	}

	public void Load(JObject data)
	{
		JObject field = data.GetField("MasterVolume");
		if ((bool)field)
		{
			MasterVolume = field.GetValue<float>();
		}
		JObject field2 = data.GetField("MusicVolume");
		if ((bool)field2)
		{
			MusicVolume = field2.GetValue<float>();
		}
		JObject field3 = data.GetField("EnvironmentVolume");
		if ((bool)field3)
		{
			EnvironmentVolume = field3.GetValue<float>();
		}
		JObject field4 = data.GetField("EffectsVolume");
		if ((bool)field4)
		{
			EffectsVolume = field4.GetValue<float>();
		}
		JObject field5 = data.GetField("VoiceVolume");
		if ((bool)field5)
		{
			VoiceVolume = field5.GetValue<float>();
		}
		JObject field6 = data.GetField("UIVolume");
		if ((bool)field6)
		{
			UIVolume = field6.GetValue<float>();
		}
	}
}
