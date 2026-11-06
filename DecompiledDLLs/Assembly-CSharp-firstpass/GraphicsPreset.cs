public static class GraphicsPreset
{
	public static GraphicsSettings LAPTOP => new GraphicsSettings
	{
		Preset = Quality_Preset.LAPTOP,
		FixedFrameRate = Quality_FixedFrameRate.OFF,
		VerticalSync = Quality_VerticalSync.OFF,
		FieldOfView = 0f,
		DrawDistance = Quality_DrawDistance.NEAR,
		TextureQuality = Quality_Textures.LOW,
		ShadowQuality = Quality_Shadows.OFF,
		ShaderQuality = Quality_LOW_ULTRA.LOW,
		BillboardDensity = Quality_BillboardDensity.LOW,
		Grass = Quality_Grass.OFF,
		Clouds = Quality_VolumetricClouds.OFF,
		Antialiasing = Quality_Antialiasing.OFF,
		AmbientOcclusion = Quality_AmbientOcclusion.OFF,
		MotionBlur = Quality_MotionBlur.OFF,
		Bloom = Quality_Bloom.OFF,
		LensEffects = Quality_OFF_ON.OFF,
		WaterQuality = Quality_LAPTOP_ULTRA.LAPTOP,
		WaterReflections = Quality_OFF_HIGH.OFF,
		Caustics = Quality_Caustics.OFF
	};

	public static GraphicsSettings LOW => new GraphicsSettings
	{
		Preset = Quality_Preset.LOW,
		FixedFrameRate = Quality_FixedFrameRate.OFF,
		VerticalSync = Quality_VerticalSync.OFF,
		FieldOfView = 0f,
		DrawDistance = Quality_DrawDistance.NEAR,
		TextureQuality = Quality_Textures.LOW,
		ShadowQuality = Quality_Shadows.LOW,
		ShaderQuality = Quality_LOW_ULTRA.LOW,
		BillboardDensity = Quality_BillboardDensity.LOW,
		Grass = Quality_Grass.OFF,
		Clouds = Quality_VolumetricClouds.OFF,
		Antialiasing = Quality_Antialiasing.OFF,
		AmbientOcclusion = Quality_AmbientOcclusion.OFF,
		MotionBlur = Quality_MotionBlur.OFF,
		Bloom = Quality_Bloom.OFF,
		LensEffects = Quality_OFF_ON.OFF,
		WaterQuality = Quality_LAPTOP_ULTRA.LOW,
		WaterReflections = Quality_OFF_HIGH.OFF,
		Caustics = Quality_Caustics.OFF
	};

	public static GraphicsSettings MEDIUM => new GraphicsSettings
	{
		Preset = Quality_Preset.MEDIUM,
		FixedFrameRate = Quality_FixedFrameRate.OFF,
		VerticalSync = Quality_VerticalSync.OFF,
		FieldOfView = 0f,
		DrawDistance = Quality_DrawDistance.FAR,
		TextureQuality = Quality_Textures.MEDIUM,
		ShadowQuality = Quality_Shadows.MEDIUM,
		ShaderQuality = Quality_LOW_ULTRA.MEDIUM,
		BillboardDensity = Quality_BillboardDensity.MEDIUM,
		Grass = Quality_Grass.ON,
		Clouds = Quality_VolumetricClouds.LOW,
		Antialiasing = Quality_Antialiasing.FXAA,
		AmbientOcclusion = Quality_AmbientOcclusion.OFF,
		MotionBlur = Quality_MotionBlur.OFF,
		Bloom = Quality_Bloom.OFF,
		LensEffects = Quality_OFF_ON.ON,
		WaterQuality = Quality_LAPTOP_ULTRA.MEDIUM,
		WaterReflections = Quality_OFF_HIGH.OFF,
		Caustics = Quality_Caustics.ON
	};

	public static GraphicsSettings HIGH => new GraphicsSettings
	{
		Preset = Quality_Preset.HIGH,
		FixedFrameRate = Quality_FixedFrameRate.OFF,
		VerticalSync = Quality_VerticalSync.OFF,
		FieldOfView = 0f,
		DrawDistance = Quality_DrawDistance.VERY_FAR,
		TextureQuality = Quality_Textures.HIGH,
		ShadowQuality = Quality_Shadows.HIGH,
		ShaderQuality = Quality_LOW_ULTRA.HIGH,
		BillboardDensity = Quality_BillboardDensity.HIGH,
		Grass = Quality_Grass.ON,
		Clouds = Quality_VolumetricClouds.MEDIUM,
		Antialiasing = Quality_Antialiasing.SMAA,
		AmbientOcclusion = Quality_AmbientOcclusion.HIGH,
		MotionBlur = Quality_MotionBlur.OFF,
		Bloom = Quality_Bloom.ON,
		LensEffects = Quality_OFF_ON.ON,
		WaterQuality = Quality_LAPTOP_ULTRA.HIGH,
		WaterReflections = Quality_OFF_HIGH.MEDIUM,
		Caustics = Quality_Caustics.ON
	};

	public static GraphicsSettings ULTRA => new GraphicsSettings
	{
		Preset = Quality_Preset.ULTRA,
		FixedFrameRate = Quality_FixedFrameRate.OFF,
		VerticalSync = Quality_VerticalSync.OFF,
		FieldOfView = 0f,
		DrawDistance = Quality_DrawDistance.VERY_FAR,
		TextureQuality = Quality_Textures.ULTRA,
		ShadowQuality = Quality_Shadows.ULTRA,
		ShaderQuality = Quality_LOW_ULTRA.ULTRA,
		BillboardDensity = Quality_BillboardDensity.HIGH,
		Grass = Quality_Grass.ON,
		Clouds = Quality_VolumetricClouds.HIGH,
		Antialiasing = Quality_Antialiasing.TXAA,
		AmbientOcclusion = Quality_AmbientOcclusion.HIGH,
		MotionBlur = Quality_MotionBlur.ON,
		Bloom = Quality_Bloom.ON,
		LensEffects = Quality_OFF_ON.ON,
		WaterQuality = Quality_LAPTOP_ULTRA.ULTRA,
		WaterReflections = Quality_OFF_HIGH.HIGH,
		Caustics = Quality_Caustics.ON
	};
}
