namespace Beam;

public static class LoadState_Ext
{
	public static bool IsLoading(this LoadState state)
	{
		return state == LoadState.Loading;
	}

	public static bool IsLoaded(this LoadState state)
	{
		return state == LoadState.Loaded;
	}

	public static bool IsIdle(this LoadState state)
	{
		return state == LoadState.Idle;
	}
}
