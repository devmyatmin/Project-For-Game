using UnityEngine;

namespace RootMotion;

public static class BakerUtilities
{
	public static void ReduceKeyframes(AnimationCurve curve, float maxError)
	{
		if (!(maxError <= 0f))
		{
			curve.keys = GetReducedKeyframes(curve, maxError);
		}
	}

	public static Keyframe[] GetReducedKeyframes(AnimationCurve curve, float maxError)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Keyframe[] array = curve.keys;
		int num = 1;
		while (num < array.Length - 1 && array.Length > 2)
		{
			Keyframe[] array2 = new Keyframe[array.Length - 1];
			int num2 = 0;
			for (int i = 0; i < array.Length; i++)
			{
				if (num != i)
				{
					array2[num2] = new Keyframe(array[i].time, array[i].value, array[i].inTangent, array[i].outTangent);
					num2++;
				}
			}
			AnimationCurve val = new AnimationCurve
			{
				keys = array2
			};
			float num3 = Mathf.Abs(val.Evaluate(array[num].time) - array[num].value);
			float num4 = array[num].time + (array[num - 1].time - array[num].time) * 0.5f;
			float num5 = array[num].time + (array[num + 1].time - array[num].time) * 0.5f;
			float num6 = Mathf.Abs(val.Evaluate(num4) - curve.Evaluate(num4));
			float num7 = Mathf.Abs(val.Evaluate(num5) - curve.Evaluate(num5));
			if (num3 < maxError && num6 < maxError && num7 < maxError)
			{
				array = array2;
			}
			else
			{
				num++;
			}
		}
		return array;
	}

	public static void SetLoopFrame(float time, AnimationCurve curve)
	{
		Keyframe[] keys = curve.keys;
		keys[^1].value = keys[0].value;
		float inTangent = Mathf.Lerp(keys[0].inTangent, keys[^1].inTangent, 0.5f);
		keys[0].inTangent = inTangent;
		keys[^1].inTangent = inTangent;
		float outTangent = Mathf.Lerp(keys[0].outTangent, keys[^1].outTangent, 0.5f);
		keys[0].outTangent = outTangent;
		keys[^1].outTangent = outTangent;
		keys[^1].time = time;
		curve.keys = keys;
	}

	public static void SetTangentMode(AnimationCurve curve)
	{
	}

	public static Quaternion EnsureQuaternionContinuity(Quaternion lastQ, Quaternion q)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = new Quaternion(0f - q.x, 0f - q.y, 0f - q.z, 0f - q.w);
		Quaternion val2 = new Quaternion(Mathf.Lerp(lastQ.x, q.x, 0.5f), Mathf.Lerp(lastQ.y, q.y, 0.5f), Mathf.Lerp(lastQ.z, q.z, 0.5f), Mathf.Lerp(lastQ.w, q.w, 0.5f));
		Quaternion val3 = new Quaternion(Mathf.Lerp(lastQ.x, val.x, 0.5f), Mathf.Lerp(lastQ.y, val.y, 0.5f), Mathf.Lerp(lastQ.z, val.z, 0.5f), Mathf.Lerp(lastQ.w, val.w, 0.5f));
		float num = Quaternion.Angle(lastQ, val2);
		if (!(Quaternion.Angle(lastQ, val3) < num))
		{
			return q;
		}
		return val;
	}
}
