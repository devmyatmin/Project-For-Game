using Beam.Serialization.Json;

public class GeneralSettings : ISaveable
{
	public Quality_HUDInformation HudInformation;

	public Quality_OFF_ON ShowCrosshair;

	public Quality_OFF_ON ShowHighlighting;

	public Temperature Temperature;

	public string LangPath = "";

	public Quality_OFF_ON Tutorial;

	public CameraViewMode CameraViewMode;

	public float OceanRotationIntensity;

	public bool AlwaysShowHotkeyBar;

	public int LastSaveSlotUsed;

	public GeneralSettings()
	{
		HudInformation = Quality_HUDInformation.ITEM_NAMES;
		ShowCrosshair = Quality_OFF_ON.ON;
		ShowHighlighting = Quality_OFF_ON.ON;
		Temperature = Temperature.FAHRENHEIT;
		Tutorial = Quality_OFF_ON.ON;
		CameraViewMode = CameraViewMode.FIRST_PERSON;
		OceanRotationIntensity = 0.25f;
		LastSaveSlotUsed = 0;
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("HudInformation", JSerializer.Serialize((object)HudInformation));
		jObject.AddField("ShowCrosshair", JSerializer.Serialize((object)ShowCrosshair));
		jObject.AddField("LangPath", JSerializer.Serialize((object)LangPath));
		jObject.AddField("ShowHighlighting", JSerializer.Serialize((object)ShowHighlighting));
		jObject.AddField("Temperature", JSerializer.Serialize((object)Temperature));
		jObject.AddField("Tutorial", JSerializer.Serialize((object)Tutorial));
		jObject.AddField("CameraViewMode", JSerializer.Serialize((object)CameraViewMode));
		jObject.AddField("OceanRotationIntensity", JSerializer.Serialize((object)OceanRotationIntensity));
		jObject.AddField("LastSaveSlotUsed", JSerializer.Serialize((object)LastSaveSlotUsed));
		return jObject;
	}

	public void Load(JObject data)
	{
		HudInformation = JSerializer.Deserialize<Quality_HUDInformation>(data.GetField("HudInformation"));
		ShowCrosshair = JSerializer.Deserialize<Quality_OFF_ON>(data.GetField("ShowCrosshair"));
		Temperature = JSerializer.Deserialize<Temperature>(data.GetField("Temperature"));
		ShowHighlighting = JSerializer.Deserialize<Quality_OFF_ON>(data.GetField("ShowHighlighting"));
		OceanRotationIntensity = data.GetField("OceanRotationIntensity").GetValue<float>();
		JObject field = data.GetField("LastSaveSlotUsed");
		if (field != null)
		{
			LastSaveSlotUsed = field.GetValue<int>();
		}
		JObject field2 = data.GetField("Tutorial");
		if (field2 != null)
		{
			Tutorial = JSerializer.Deserialize<Quality_OFF_ON>(field2);
		}
		JObject field3 = data.GetField("CameraViewMode");
		if (field3 != null)
		{
			CameraViewMode = JSerializer.Deserialize<CameraViewMode>(field3);
		}
		JObject field4 = data.GetField("LangPath");
		if (field4 != null)
		{
			LangPath = field4.GetValue<string>();
		}
	}
}
