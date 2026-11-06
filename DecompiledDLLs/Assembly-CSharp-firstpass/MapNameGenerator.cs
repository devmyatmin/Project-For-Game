using System;

public static class MapNameGenerator
{
	private static readonly string[] Prefixes = new string[8] { "MAP_NAME_PREFIX_1", "MAP_NAME_PREFIX_2", "MAP_NAME_PREFIX_3", "MAP_NAME_PREFIX_4", "MAP_NAME_PREFIX_5", "MAP_NAME_PREFIX_6", "MAP_NAME_PREFIX_7", "MAP_NAME_PREFIX_8" };

	private static readonly string[] Stems = new string[10] { "MAP_NAME_STEM_1", "MAP_NAME_STEM_2", "MAP_NAME_STEM_3", "MAP_NAME_STEM_4", "MAP_NAME_STEM_5", "MAP_NAME_STEM_6", "MAP_NAME_STEM_7", "MAP_NAME_STEM_8", "MAP_NAME_STEM_9", "MAP_NAME_STEM_10" };

	private static readonly string[] Suffixes = new string[6] { "MAP_NAME_SUFFIX_1", "MAP_NAME_SUFFIX_2", "MAP_NAME_SUFFIX_3", "MAP_NAME_SUFFIX_4", "MAP_NAME_SUFFIX_5", "MAP_NAME_SUFFIX_6" };

	public static string GenerateRandomMapName(int seedEffect)
	{
		Random random = new Random(StrandedWorld.WORLD_SEED + seedEffect);
		return string.Join(" ", new string[3]
		{
			Prefixes[random.Next(0, Prefixes.Length)],
			Stems[random.Next(0, Stems.Length)],
			Suffixes[random.Next(0, Suffixes.Length)]
		});
	}
}
