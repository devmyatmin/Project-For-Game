using UnityEngine;

public static class CursorUtils
{
	private static CursorLockMode _lockState = Cursor.lockState;

	private static bool _visible = Cursor.visible;

	public static CursorLockMode LockState
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return Cursor.lockState;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			Cursor.lockState = value;
		}
	}

	public static bool Visible
	{
		get
		{
			return Cursor.visible;
		}
		set
		{
			Cursor.visible = value;
		}
	}

	static CursorUtils()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
	}
}
