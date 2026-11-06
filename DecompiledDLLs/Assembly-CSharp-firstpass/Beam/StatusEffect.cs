namespace Beam;

public static class StatusEffect
{
	public const float HEALTH_REGEN_TO_FULL_HOURS = 7f;

	public const float BREATH_BOOST_DURATION_HOURS = 3f;

	public const float SUNBLOCK_DURATION_HOURS = 12f;

	public const float SHARK_REPELLENT_DURATION_HOURS = 12f;

	public const float SPLINT_DURATION_HOURS = 48f;

	public const float DROWNING_DAYS_TO_KILL = 0.007f;

	public const float STARVATION_DAYS_TO_KILL = 2f;

	public const float DEHYDRATION_DAYS_TO_KILL = 2f;

	public const float BLEEDING_DAYS_TO_KILL = 3f;

	public const float ILLNESS_DAYS_TO_KILL = 6f;

	public const float POISON_DAYS_TO_KILL = 3f;

	public const float MALNUTRITION_DAYS_TO_KILL = 3f;

	public const float HARD_HEALTH_REGEN_TO_FULL_HOURS = 7f;

	public const float HARD_BREATH_BOOST_DURATION_HOURS = 3f;

	public const float HARD_SUNBLOCK_DURATION_HOURS = 12f;

	public const float HARD_SHARK_REPELLENT_DURATION_HOURS = 12f;

	public const float HARD_SPLINT_DURATION_HOURS = 48f;

	public const float HARD_DROWNING_DAYS_TO_KILL = 0.007f;

	public const float HARD_STARVATION_DAYS_TO_KILL = 1f;

	public const float HARD_DEHYDRATION_DAYS_TO_KILL = 1f;

	public const float HARD_BLEEDING_DAYS_TO_KILL = 1.5f;

	public const float HARD_ILLNESS_DAYS_TO_KILL = 3f;

	public const float HARD_POISON_DAYS_TO_KILL = 1.5f;

	public const float HARD_MALNUTRITION_DAYS_TO_KILL = 1.5f;

	public static PlayerEffect HEALTH_REGENERATION;

	public static PlayerEffect SUNBLOCK;

	public static PlayerEffect SHARK_REPELLENT;

	public static PlayerEffect BOOST_BREATH;

	public static PlayerEffect SPLINT;

	public static PlayerEffect TIRED;

	public static PlayerEffect STARVATION;

	public static PlayerEffect DEHYDRATION;

	public static PlayerEffect BLEEDING;

	public static PlayerEffect DROWNING;

	public static PlayerEffect BROKEN_BONES;

	public static PlayerEffect ILLNESS;

	public static PlayerEffect POISON;

	public static PlayerEffect SUNSTROKE;

	public static PlayerEffect DIARRHOEA;

	public static PlayerEffect MALNUTRITION;

	public static float GetHealthEffectRatePerHour(float days)
	{
		return 700f / days / 24f;
	}

	public static float GetHungerEffectRatePerHour(float days)
	{
		return 700f / days / 24f;
	}

	public static float GetThirstEffectRatePerHour(float days)
	{
		return 700f / days / 24f;
	}

	public static void CreateEffects()
	{
		bool flag = PlayerUtilities.GetServerGameDifficulty() == EGameDifficultyMode.Hard;
		HEALTH_REGENERATION = new PlayerEffect("HEALTH_REGENERATION", "PLAYER_STATUS_EFFECT_HEALTHY", positiveEffect: true, 0f, 0f, 0f, 700f / (flag ? 7f : 7f));
		SUNBLOCK = new PlayerEffect("SUNBLOCK", "PLAYER_STATUS_EFFECT_SUNBLOCK", positiveEffect: true, 0f, 0f, 0f, 0f, flag ? 12f : 12f);
		SHARK_REPELLENT = new PlayerEffect("SHARK_REPELLENT", "PLAYER_STATUS_EFFECT_SHARK_REPELLENT", positiveEffect: true, 0f, 0f, 0f, 0f, flag ? 12f : 12f);
		BOOST_BREATH = new PlayerEffect("BOOST_BREATH", "PLAYER_STATUS_EFFECT_BREATH_BOOST", positiveEffect: true, 0f, 0f, 0f, 0f, flag ? 3f : 3f);
		SPLINT = new PlayerEffect("SPLINT", "PLAYER_STATUS_EFFECT_SPLINT", positiveEffect: true, 0f, 0f, 0f, 0f, flag ? 48f : 48f);
		TIRED = new PlayerEffect("TIRED", "PLAYER_STATUS_EFFECT_TIRED", positiveEffect: false, 0f, 0f, 0f, 0f);
		STARVATION = new PlayerEffect("STARVING", "PLAYER_STATUS_EFFECT_STARVATION", positiveEffect: false, 0f, 0f, 0f, 0f - GetHealthEffectRatePerHour(flag ? 1f : 2f));
		DEHYDRATION = new PlayerEffect("DEHYDRATION", "PLAYER_STATUS_EFFECT_DEHYDRATION", positiveEffect: false, 0f, 0f, 0f, 0f - GetHealthEffectRatePerHour(flag ? 1f : 2f));
		BLEEDING = new PlayerEffect("BLEEDING", "PLAYER_STATUS_EFFECT_BLEEDING", positiveEffect: false, 0f, 0f, 0f, 0f - GetHealthEffectRatePerHour(flag ? 1.5f : 3f));
		DROWNING = new PlayerEffect("DROWNING", "PLAYER_STATUS_EFFECT_DROWNING", positiveEffect: false, 0f, 0f, 0f, 0f);
		BROKEN_BONES = new PlayerEffect("BROKEN_BONES", "PLAYER_STATUS_EFFECT_BROKEN_BONES", positiveEffect: false, 0f, 0f, 0f, 0f);
		ILLNESS = new PlayerEffect("ILLNESS", "PLAYER_STATUS_EFFECT_ILLNESS", positiveEffect: false, 0f, 0f, 0f, 0f - GetHealthEffectRatePerHour(flag ? 3f : 6f), 24f * (flag ? 3f : 6f));
		POISON = new PlayerEffect("POISON", "PLAYER_STATUS_EFFECT_POISONING", positiveEffect: false, 0f, 0f, 0f, 0f - GetHealthEffectRatePerHour(flag ? 1.5f : 3f), 24f * (flag ? 1.5f : 3f));
		SUNSTROKE = new PlayerEffect("SUNSTROKE", "PLAYER_STATUS_EFFECT_SUNSTROKE", positiveEffect: false, -200f, -200f, 0f, 0f, 7f);
		DIARRHOEA = new PlayerEffect("DIARRHOEA", "PLAYER_STATUS_EFFECT_DIARRHOEA", positiveEffect: false, -700f, 0f, 0f, 0f, 1f);
		MALNUTRITION = new PlayerEffect("MALNUTRITION", "PLAYER_STATUS_EFFECT_MALNUTRITION", positiveEffect: false, 0f, 0f, 0f, 0f - GetHealthEffectRatePerHour(flag ? 1.5f : 3f), 24f * (flag ? 1.5f : 3f));
	}

	public static void EnableAllNegativeStatusEffects()
	{
		PlayerRegistry.LocalPlayer.Statistics.ApplyStatusEffect(TIRED);
		PlayerRegistry.LocalPlayer.Statistics.ApplyStatusEffect(STARVATION);
		PlayerRegistry.LocalPlayer.Statistics.ApplyStatusEffect(DEHYDRATION);
		PlayerRegistry.LocalPlayer.Statistics.ApplyStatusEffect(BLEEDING);
		PlayerRegistry.LocalPlayer.Statistics.ApplyStatusEffect(DROWNING);
		PlayerRegistry.LocalPlayer.Statistics.ApplyStatusEffect(BROKEN_BONES);
		PlayerRegistry.LocalPlayer.Statistics.ApplyStatusEffect(ILLNESS);
		PlayerRegistry.LocalPlayer.Statistics.ApplyStatusEffect(POISON);
		PlayerRegistry.LocalPlayer.Statistics.ApplyStatusEffect(SUNSTROKE);
		PlayerRegistry.LocalPlayer.Statistics.ApplyStatusEffect(DIARRHOEA);
		PlayerRegistry.LocalPlayer.Statistics.ApplyStatusEffect(MALNUTRITION);
		PlayerRegistry.LocalPlayer.Statistics.SetHealth(10f);
	}
}
