namespace Beam.UI;

public static class OpenStateExt
{
	public static bool IsOpen(this OpenState openState, bool orReserved = true)
	{
		if (orReserved)
		{
			return openState != OpenState.Close;
		}
		return openState == OpenState.Open;
	}
}
