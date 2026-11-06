namespace Beam;

public static class UVIndexUtility
{
	public static UVIndex GetUVIndex(int value)
	{
		if (value > 10)
		{
			return UVIndex.Extreme;
		}
		if (value > 7)
		{
			return UVIndex.VeryHigh;
		}
		if (value > 5)
		{
			return UVIndex.Moderate;
		}
		_ = 2;
		return UVIndex.Low;
	}

	public static string GetFormattedUVIndex(UVIndex index)
	{
		return index switch
		{
			UVIndex.Low => "PLAYER_WATCH_UV_INDEX_LABEL_LOW", 
			UVIndex.Moderate => "PLAYER_WATCH_UV_INDEX_LABEL_MEDIUM", 
			UVIndex.High => "PLAYER_WATCH_UV_INDEX_LABEL_HIGH", 
			UVIndex.VeryHigh => "PLAYER_WATCH_UV_INDEX_LABEL_VERY_HIGH", 
			UVIndex.Extreme => "PLAYER_WATCH_UV_INDEX_LABEL_EXTREME", 
			_ => string.Empty, 
		};
	}

	public static float GetBurnTime(UVIndex uv)
	{
		return uv switch
		{
			UVIndex.Moderate => 5f, 
			UVIndex.High => 4f, 
			UVIndex.VeryHigh => 2f, 
			UVIndex.Extreme => 1f, 
			_ => 24f, 
		};
	}
}
