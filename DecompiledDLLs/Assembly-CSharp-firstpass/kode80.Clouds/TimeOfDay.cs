using System;
using UnityEngine;

namespace kode80.Clouds;

[ExecuteInEditMode]
public class TimeOfDay : MonoBehaviour
{
	public Material proceduralSkybox;

	public kode80Clouds clouds;

	public Light sun;

	public TimeOfDayKeyFrame[] keyFrames;

	private void Awake()
	{
		SortKeyFrames();
	}

	private void Start()
	{
		SortKeyFrames();
	}

	private void Update()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		if (keyFrames.Length >= 1)
		{
			float x = ((Component)sun).transform.localEulerAngles.x;
			TimeOfDayKeyFrame timeOfDayKeyFrame = KeyFrameBelowAngle(x);
			TimeOfDayKeyFrame timeOfDayKeyFrame2 = KeyFrameAboveAngle(x);
			float num = timeOfDayKeyFrame2.angle - timeOfDayKeyFrame.angle;
			float num2 = ((num == 0f) ? 0.5f : ((x - timeOfDayKeyFrame.angle) / num));
			RenderSettings.fogColor = Color.Lerp(timeOfDayKeyFrame.fogColor, timeOfDayKeyFrame2.fogColor, num2);
			if ((Object)(object)sun != (Object)null)
			{
				sun.color = Color.Lerp(timeOfDayKeyFrame.sunColor, timeOfDayKeyFrame2.sunColor, num2);
			}
			if ((Object)(object)clouds != (Object)null)
			{
				clouds.cloudBaseColor = Color.Lerp(timeOfDayKeyFrame.bottomColor, timeOfDayKeyFrame2.bottomColor, num2);
				clouds.cloudTopColor = Color.Lerp(timeOfDayKeyFrame.topColor, timeOfDayKeyFrame2.topColor, num2);
			}
			if (Object.op_Implicit((Object)(object)proceduralSkybox))
			{
				proceduralSkybox.SetFloat("_SunSize", Mathf.Lerp(timeOfDayKeyFrame.sunSize, timeOfDayKeyFrame2.sunSize, num2));
				proceduralSkybox.SetFloat("_AtmosphereThickness", Mathf.Lerp(timeOfDayKeyFrame.atmosphereThickness, timeOfDayKeyFrame2.atmosphereThickness, num2));
			}
		}
	}

	public void SortKeyFrames()
	{
		Array.Sort(keyFrames, (TimeOfDayKeyFrame a, TimeOfDayKeyFrame b) => a.angle.CompareTo(b.angle));
	}

	private TimeOfDayKeyFrame KeyFrameBelowAngle(float angle)
	{
		return KeyFrameForAngle(angle, findGreaterThan: false);
	}

	private TimeOfDayKeyFrame KeyFrameAboveAngle(float angle)
	{
		return KeyFrameForAngle(angle, findGreaterThan: true);
	}

	private TimeOfDayKeyFrame KeyFrameForAngle(float angle, bool findGreaterThan)
	{
		if (keyFrames.Length != 0)
		{
			TimeOfDayKeyFrame timeOfDayKeyFrame = keyFrames[0];
			TimeOfDayKeyFrame timeOfDayKeyFrame2 = keyFrames[keyFrames.Length - 1];
			if (angle <= timeOfDayKeyFrame.angle)
			{
				return timeOfDayKeyFrame;
			}
			if (angle >= timeOfDayKeyFrame2.angle)
			{
				return timeOfDayKeyFrame2;
			}
			for (int i = 0; i < keyFrames.Length; i++)
			{
				timeOfDayKeyFrame = keyFrames[i];
				timeOfDayKeyFrame2 = ((i < keyFrames.Length - 1) ? keyFrames[i + 1] : timeOfDayKeyFrame);
				if (angle >= timeOfDayKeyFrame.angle && angle <= timeOfDayKeyFrame2.angle)
				{
					if (!findGreaterThan)
					{
						return timeOfDayKeyFrame;
					}
					return timeOfDayKeyFrame2;
				}
			}
		}
		return null;
	}
}
