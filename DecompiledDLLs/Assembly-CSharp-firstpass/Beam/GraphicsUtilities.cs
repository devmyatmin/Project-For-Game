using UnityEngine;

namespace Beam;

public static class GraphicsUtilities
{
	public static void SetVSync(Quality_VerticalSync quality)
	{
		switch (quality)
		{
		case Quality_VerticalSync.TWO_FRAMES:
			QualitySettings.vSyncCount = 2;
			break;
		case Quality_VerticalSync.ONE_FRAME:
			QualitySettings.vSyncCount = 1;
			break;
		case Quality_VerticalSync.OFF:
			QualitySettings.vSyncCount = 0;
			break;
		}
	}

	public static int ToInt(this Quality_FixedFrameRate rate)
	{
		return rate switch
		{
			Quality_FixedFrameRate.THIRTY => 30, 
			Quality_FixedFrameRate.FORTY => 40, 
			Quality_FixedFrameRate.FIFTY => 50, 
			Quality_FixedFrameRate.SIXTY => 60, 
			Quality_FixedFrameRate.SEVENTY => 70, 
			Quality_FixedFrameRate.EIGHTY => 80, 
			Quality_FixedFrameRate.NINETY => 90, 
			Quality_FixedFrameRate.ONE_HUNDRED => 100, 
			Quality_FixedFrameRate.ONE_HUNDRED_TEN => 110, 
			Quality_FixedFrameRate.ONE_HUNDRED_TWENTY => 120, 
			_ => -1, 
		};
	}

	public static void SetFixedFrameRate(Quality_FixedFrameRate fixedFrameRate)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Resolution currentResolution = Screen.currentResolution;
		float num = currentResolution.refreshRate;
		int num2;
		if (fixedFrameRate == Quality_FixedFrameRate.OFF)
		{
			num2 = -1;
			SetFrameRateAndTimeStep(num2);
		}
		else
		{
			int num3 = fixedFrameRate.ToInt();
			if (num3 < 30 || num3 > 120)
			{
				num3 = (int)num;
				Debug.LogError((object)"GraphicsUtilities::SetFixedFrameRate:: Something went wrong. Saved frame rate value was out of bounds.Frame rate is now set to 60 FPS.");
			}
			num2 = num3;
			SetFrameRateAndTimeStep(num2);
		}
		Debug.Log((object)$"Refresh Rate: {num}Hz");
		Debug.Log((object)$"Application Target Frame Rate: {num2}FPS");
	}

	private static void SetFrameRateAndTimeStep(float rate)
	{
		Application.targetFrameRate = (int)rate;
	}
}
