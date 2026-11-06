using System;
using UnityEngine;

namespace uTools;

public class EaseManager
{
	public delegate float EaseDelegate(float start, float end, float t);

	private static float linear(float start, float end, float value)
	{
		return Mathf.Lerp(start, end, value);
	}

	private static float clerp(float start, float end, float value)
	{
		float num = 0f;
		float num2 = 360f;
		float num3 = Mathf.Abs((num2 - num) / 2f);
		float num4 = 0f;
		float num5 = 0f;
		if (end - start < 0f - num3)
		{
			num5 = (num2 - start + end) * value;
			return start + num5;
		}
		if (end - start > num3)
		{
			num5 = (0f - (num2 - end + start)) * value;
			return start + num5;
		}
		return start + (end - start) * value;
	}

	private static float spring(float start, float end, float value)
	{
		value = Mathf.Clamp01(value);
		value = (Mathf.Sin(value * (float)Math.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + 1.2f * (1f - value));
		return start + (end - start) * value;
	}

	private static float easeInQuad(float start, float end, float value)
	{
		end -= start;
		return end * value * value + start;
	}

	private static float easeOutQuad(float start, float end, float value)
	{
		end -= start;
		return (0f - end) * value * (value - 2f) + start;
	}

	private static float easeInOutQuad(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end / 2f * value * value + start;
		}
		value--;
		return (0f - end) / 2f * (value * (value - 2f) - 1f) + start;
	}

	private static float easeInCubic(float start, float end, float value)
	{
		end -= start;
		return end * value * value * value + start;
	}

	private static float easeOutCubic(float start, float end, float value)
	{
		value--;
		end -= start;
		return end * (value * value * value + 1f) + start;
	}

	private static float easeInOutCubic(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end / 2f * value * value * value + start;
		}
		value -= 2f;
		return end / 2f * (value * value * value + 2f) + start;
	}

	private static float easeInQuart(float start, float end, float value)
	{
		end -= start;
		return end * value * value * value * value + start;
	}

	private static float easeOutQuart(float start, float end, float value)
	{
		value--;
		end -= start;
		return (0f - end) * (value * value * value * value - 1f) + start;
	}

	private static float easeInOutQuart(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end / 2f * value * value * value * value + start;
		}
		value -= 2f;
		return (0f - end) / 2f * (value * value * value * value - 2f) + start;
	}

	private static float easeInQuint(float start, float end, float value)
	{
		end -= start;
		return end * value * value * value * value * value + start;
	}

	private static float easeOutQuint(float start, float end, float value)
	{
		value--;
		end -= start;
		return end * (value * value * value * value * value + 1f) + start;
	}

	private static float easeInOutQuint(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end / 2f * value * value * value * value * value + start;
		}
		value -= 2f;
		return end / 2f * (value * value * value * value * value + 2f) + start;
	}

	private static float easeInSine(float start, float end, float value)
	{
		end -= start;
		return (0f - end) * Mathf.Cos(value / 1f * ((float)Math.PI / 2f)) + end + start;
	}

	private static float easeOutSine(float start, float end, float value)
	{
		end -= start;
		return end * Mathf.Sin(value / 1f * ((float)Math.PI / 2f)) + start;
	}

	private static float easeInOutSine(float start, float end, float value)
	{
		end -= start;
		return (0f - end) / 2f * (Mathf.Cos((float)Math.PI * value / 1f) - 1f) + start;
	}

	private static float easeInExpo(float start, float end, float value)
	{
		end -= start;
		return end * Mathf.Pow(2f, 10f * (value / 1f - 1f)) + start;
	}

	private static float easeOutExpo(float start, float end, float value)
	{
		end -= start;
		return end * (0f - Mathf.Pow(2f, -10f * value / 1f) + 1f) + start;
	}

	private static float easeInOutExpo(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end / 2f * Mathf.Pow(2f, 10f * (value - 1f)) + start;
		}
		value--;
		return end / 2f * (0f - Mathf.Pow(2f, -10f * value) + 2f) + start;
	}

	private static float easeInCirc(float start, float end, float value)
	{
		end -= start;
		return (0f - end) * (Mathf.Sqrt(1f - value * value) - 1f) + start;
	}

	private static float easeOutCirc(float start, float end, float value)
	{
		value--;
		end -= start;
		return end * Mathf.Sqrt(1f - value * value) + start;
	}

	private static float easeInOutCirc(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return (0f - end) / 2f * (Mathf.Sqrt(1f - value * value) - 1f) + start;
		}
		value -= 2f;
		return end / 2f * (Mathf.Sqrt(1f - value * value) + 1f) + start;
	}

	private static float easeInBounce(float start, float end, float value)
	{
		end -= start;
		float num = 1f;
		return end - easeOutBounce(0f, end, num - value) + start;
	}

	private static float easeOutBounce(float start, float end, float value)
	{
		value /= 1f;
		end -= start;
		if (value < 372f / 1023f)
		{
			return end * (7.5625f * value * value) + start;
		}
		if (value < 744f / 1023f)
		{
			value -= 558f / 1023f;
			return end * (7.5625f * value * value + 0.75f) + start;
		}
		if ((double)value < 930.0 / 1023.0)
		{
			value -= 837f / 1023f;
			return end * (7.5625f * value * value + 0.9375f) + start;
		}
		value -= 21f / 22f;
		return end * (7.5625f * value * value + 63f / 64f) + start;
	}

	private static float easeInOutBounce(float start, float end, float value)
	{
		end -= start;
		float num = 1f;
		if (value < num / 2f)
		{
			return easeInBounce(0f, end, value * 2f) * 0.5f + start;
		}
		return easeOutBounce(0f, end, value * 2f - num) * 0.5f + end * 0.5f + start;
	}

	private static float easeInBack(float start, float end, float value)
	{
		end -= start;
		value /= 1f;
		float num = 1.70158f;
		return end * value * value * ((num + 1f) * value - num) + start;
	}

	private static float easeOutBack(float start, float end, float value)
	{
		float num = 1.70158f;
		end -= start;
		value = value / 1f - 1f;
		return end * (value * value * ((num + 1f) * value + num) + 1f) + start;
	}

	private static float easeInOutBack(float start, float end, float value)
	{
		float num = 1.70158f;
		end -= start;
		value /= 0.5f;
		if (value < 1f)
		{
			num *= 1.525f;
			return end / 2f * (value * value * ((num + 1f) * value - num)) + start;
		}
		value -= 2f;
		num *= 1.525f;
		return end / 2f * (value * value * ((num + 1f) * value + num) + 2f) + start;
	}

	private static float punch(float amplitude, float value)
	{
		float num = 9f;
		if (value == 0f)
		{
			return 0f;
		}
		if (value == 1f)
		{
			return 0f;
		}
		float num2 = 0.3f;
		num = num2 / ((float)Math.PI * 2f) * Mathf.Asin(0f);
		return amplitude * Mathf.Pow(2f, -10f * value) * Mathf.Sin((value * 1f - num) * ((float)Math.PI * 2f) / num2);
	}

	private static float easeInElastic(float start, float end, float value)
	{
		end -= start;
		float num = 1f;
		float num2 = num * 0.3f;
		float num3 = 0f;
		float num4 = 0f;
		if (value == 0f)
		{
			return start;
		}
		if ((value /= num) == 1f)
		{
			return start + end;
		}
		if (num4 == 0f || num4 < Mathf.Abs(end))
		{
			num4 = end;
			num3 = num2 / 4f;
		}
		else
		{
			num3 = num2 / ((float)Math.PI * 2f) * Mathf.Asin(end / num4);
		}
		return 0f - num4 * Mathf.Pow(2f, 10f * --value) * Mathf.Sin((value * num - num3) * ((float)Math.PI * 2f) / num2) + start;
	}

	private static float easeOutElastic(float start, float end, float value)
	{
		end -= start;
		float num = 1f;
		float num2 = num * 0.3f;
		float num3 = 0f;
		float num4 = 0f;
		if (value == 0f)
		{
			return start;
		}
		if ((value /= num) == 1f)
		{
			return start + end;
		}
		if (num4 == 0f || num4 < Mathf.Abs(end))
		{
			num4 = end;
			num3 = num2 / 4f;
		}
		else
		{
			num3 = num2 / ((float)Math.PI * 2f) * Mathf.Asin(end / num4);
		}
		return num4 * Mathf.Pow(2f, -10f * value) * Mathf.Sin((value * num - num3) * ((float)Math.PI * 2f) / num2) + end + start;
	}

	private static float easeInOutElastic(float start, float end, float value)
	{
		end -= start;
		float num = 1f;
		float num2 = num * 0.3f;
		float num3 = 0f;
		float num4 = 0f;
		if (value == 0f)
		{
			return start;
		}
		if ((value /= num / 2f) == 2f)
		{
			return start + end;
		}
		if (num4 == 0f || num4 < Mathf.Abs(end))
		{
			num4 = end;
			num3 = num2 / 4f;
		}
		else
		{
			num3 = num2 / ((float)Math.PI * 2f) * Mathf.Asin(end / num4);
		}
		if (value < 1f)
		{
			return -0.5f * (num4 * Mathf.Pow(2f, 10f * --value) * Mathf.Sin((value * num - num3) * ((float)Math.PI * 2f) / num2)) + start;
		}
		return num4 * Mathf.Pow(2f, -10f * --value) * Mathf.Sin((value * num - num3) * ((float)Math.PI * 2f) / num2) * 0.5f + end + start;
	}

	public static float EasingFromType(float start, float end, float t, EaseType type)
	{
		return type switch
		{
			EaseType.easeInQuad => easeInQuad(start, end, t), 
			EaseType.easeOutQuad => easeOutQuad(start, end, t), 
			EaseType.easeInOutQuad => easeInOutQuad(start, end, t), 
			EaseType.easeInCubic => easeInCubic(start, end, t), 
			EaseType.easeOutCubic => easeOutCubic(start, end, t), 
			EaseType.easeInOutCubic => easeInOutCubic(start, end, t), 
			EaseType.easeInQuart => easeInQuart(start, end, t), 
			EaseType.easeOutQuart => easeOutQuart(start, end, t), 
			EaseType.easeInOutQuart => easeInOutQuart(start, end, t), 
			EaseType.easeInQuint => easeInQuint(start, end, t), 
			EaseType.easeOutQuint => easeOutQuint(start, end, t), 
			EaseType.easeInOutQuint => easeInOutQuint(start, end, t), 
			EaseType.easeInSine => easeInSine(start, end, t), 
			EaseType.easeOutSine => easeOutSine(start, end, t), 
			EaseType.easeInOutSine => easeInOutSine(start, end, t), 
			EaseType.easeInExpo => easeInExpo(start, end, t), 
			EaseType.easeOutExpo => easeOutExpo(start, end, t), 
			EaseType.easeInOutExpo => easeInOutExpo(start, end, t), 
			EaseType.easeInCirc => easeInCirc(start, end, t), 
			EaseType.easeOutCirc => easeOutCirc(start, end, t), 
			EaseType.easeInOutCirc => easeInOutCirc(start, end, t), 
			EaseType.linear => linear(start, end, t), 
			EaseType.spring => spring(start, end, t), 
			EaseType.easeInBounce => easeInBounce(start, end, t), 
			EaseType.easeOutBounce => easeOutBounce(start, end, t), 
			EaseType.easeInOutBounce => easeInOutBounce(start, end, t), 
			EaseType.easeInBack => easeInBack(start, end, t), 
			EaseType.easeOutBack => easeOutBack(start, end, t), 
			EaseType.easeInOutBack => easeInOutBack(start, end, t), 
			EaseType.easeInElastic => easeInElastic(start, end, t), 
			EaseType.easeOutElastic => easeOutElastic(start, end, t), 
			EaseType.easeInOutElastic => easeInOutElastic(start, end, t), 
			_ => linear(start, end, t), 
		};
	}
}
