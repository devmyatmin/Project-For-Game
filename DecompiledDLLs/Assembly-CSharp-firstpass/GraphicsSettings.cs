using Beam.Serialization.Json;

public class GraphicsSettings : ISaveable
{
	public Quality_Preset Preset;

	public string Resolution;

	public Quality_FixedFrameRate FixedFrameRate;

	public Quality_VerticalSync VerticalSync;

	public Quality_OFF_ON FullScreen;

	public float FieldOfView;

	public Quality_ColorMode ColorMode;

	public Quality_DrawDistance DrawDistance;

	public Quality_Textures TextureQuality;

	public Quality_Shadows ShadowQuality;

	public Quality_BillboardDensity BillboardDensity = Quality_BillboardDensity.HIGH;

	public Quality_LOW_ULTRA ShaderQuality;

	public Quality_Grass Grass;

	public Quality_VolumetricClouds Clouds;

	public Quality_Antialiasing Antialiasing;

	public Quality_AmbientOcclusion AmbientOcclusion;

	public Quality_MotionBlur MotionBlur;

	public Quality_Bloom Bloom;

	public Quality_OFF_ON LensEffects;

	public Quality_LAPTOP_ULTRA WaterQuality = Quality_LAPTOP_ULTRA.MEDIUM;

	public Quality_OFF_HIGH WaterReflections;

	public Quality_Caustics Caustics;

	public GraphicsSettings ShallowCopy()
	{
		return (GraphicsSettings)MemberwiseClone();
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("preset", JSerializer.Serialize((object)Preset));
		if (!string.IsNullOrEmpty(Resolution))
		{
			jObject.AddField("resolution", JSerializer.Serialize((object)Resolution.Trim()));
		}
		jObject.AddField("fixedFrameRate", JSerializer.Serialize((object)FixedFrameRate));
		jObject.AddField("verticalSync", JSerializer.Serialize((object)VerticalSync));
		jObject.AddField("fullScreen", JSerializer.Serialize((object)FullScreen));
		jObject.AddField("fieldOfView", JSerializer.Serialize((object)FieldOfView));
		jObject.AddField("colorMode", JSerializer.Serialize((object)ColorMode));
		jObject.AddField("drawDistance", JSerializer.Serialize((object)DrawDistance));
		jObject.AddField("textureQuality", JSerializer.Serialize((object)TextureQuality));
		jObject.AddField("shadowQuality", JSerializer.Serialize((object)ShadowQuality));
		jObject.AddField("shaderQuality", JSerializer.Serialize((object)ShaderQuality));
		jObject.AddField("billboardDensity", JSerializer.Serialize((object)BillboardDensity));
		jObject.AddField("Grass", JSerializer.Serialize((object)Grass));
		jObject.AddField("Clouds", JSerializer.Serialize((object)Clouds));
		jObject.AddField("antialiasing", JSerializer.Serialize((object)Antialiasing));
		jObject.AddField("ambientOcclusion", JSerializer.Serialize((object)AmbientOcclusion));
		jObject.AddField("motionBlur", JSerializer.Serialize((object)MotionBlur));
		jObject.AddField("bloom", JSerializer.Serialize((object)Bloom));
		jObject.AddField("lensEffects", JSerializer.Serialize((object)LensEffects));
		jObject.AddField("waterQuality", JSerializer.Serialize((object)WaterQuality));
		jObject.AddField("waterReflections", JSerializer.Serialize((object)WaterReflections));
		jObject.AddField("caustics", JSerializer.Serialize((object)Caustics));
		return jObject;
	}

	public void Load(JObject data)
	{
		Preset = JSerializer.Deserialize<Quality_Preset>(data.GetField("preset"));
		FixedFrameRate = JSerializer.Deserialize<Quality_FixedFrameRate>(data.GetField("fixedFrameRate"));
		VerticalSync = JSerializer.Deserialize<Quality_VerticalSync>(data.GetField("verticalSync"));
		FullScreen = JSerializer.Deserialize<Quality_OFF_ON>(data.GetField("fullScreen"));
		FieldOfView = data.GetField("fieldOfView").GetValue<float>();
		JObject field = data.GetField("drawDistance");
		if (field.GetValue<string>() == "VERY_NEAR")
		{
			DrawDistance = Quality_DrawDistance.NEAR;
		}
		else
		{
			DrawDistance = JSerializer.Deserialize<Quality_DrawDistance>(field);
		}
		TextureQuality = JSerializer.Deserialize<Quality_Textures>(data.GetField("textureQuality"));
		ShadowQuality = JSerializer.Deserialize<Quality_Shadows>(data.GetField("shadowQuality"));
		ShaderQuality = JSerializer.Deserialize<Quality_LOW_ULTRA>(data.GetField("shaderQuality"));
		Antialiasing = JSerializer.Deserialize<Quality_Antialiasing>(data.GetField("antialiasing"));
		AmbientOcclusion = JSerializer.Deserialize<Quality_AmbientOcclusion>(data.GetField("ambientOcclusion"));
		Bloom = JSerializer.Deserialize<Quality_Bloom>(data.GetField("bloom"));
		LensEffects = JSerializer.Deserialize<Quality_OFF_ON>(data.GetField("lensEffects"));
		JObject field2 = data.GetField("resolution");
		if (field2 != null)
		{
			Resolution = field2.GetValue<string>();
		}
		BillboardDensity = JSerializer.Deserialize<Quality_BillboardDensity>(data.GetField("billboardDensity"));
		MotionBlur = JSerializer.Deserialize<Quality_MotionBlur>(data.GetField("motionBlur"));
		Grass = JSerializer.Deserialize<Quality_Grass>(data.GetField("Grass"));
		Clouds = JSerializer.Deserialize<Quality_VolumetricClouds>(data.GetField("Clouds"));
		WaterQuality = JSerializer.Deserialize<Quality_LAPTOP_ULTRA>(data.GetField("waterQuality"));
		WaterReflections = JSerializer.Deserialize<Quality_OFF_HIGH>(data.GetField("waterReflections"));
		Caustics = JSerializer.Deserialize<Quality_Caustics>(data.GetField("caustics"));
		JObject field3 = data.GetField("colorMode");
		if (field3 != null)
		{
			ColorMode = JSerializer.Deserialize<Quality_ColorMode>(field3);
		}
		if (Preset == Quality_Preset.ULTRA)
		{
			AmbientOcclusion = Quality_AmbientOcclusion.HIGH;
		}
		else if (Preset != Quality_Preset.CUSTOM)
		{
			AmbientOcclusion = Quality_AmbientOcclusion.OFF;
		}
	}
}
