using UnityEngine;
using UnityEngine.UI;

namespace uTools;

[AddComponentMenu("uTools/Tween/Tween Slider(uTools)")]
public class uTweenSlider : uTweenValue
{
	private Slider mSlider;

	public bool NeedCarry;

	public Slider cacheSlider
	{
		get
		{
			mSlider = ((Component)this).GetComponent<Slider>();
			if ((Object)(object)mSlider == (Object)null)
			{
				Debug.LogError((object)"'uTweenSlider' can't find 'Slider'");
			}
			return mSlider;
		}
	}

	public float sliderValue
	{
		set
		{
			if (NeedCarry)
			{
				cacheSlider.value = ((value >= 1f) ? (value - Mathf.Floor(value)) : value);
			}
			else
			{
				cacheSlider.value = ((value > 1f) ? (value - Mathf.Floor(value)) : value);
			}
		}
	}

	protected override void ValueUpdate(float value, bool isFinished)
	{
		sliderValue = value;
	}

	public static uTweenSlider Begin(Slider slider, float duration, float delay, float from, float to)
	{
		uTweenSlider uTweenSlider2 = uTweener.Begin<uTweenSlider>(((Component)slider).gameObject, duration);
		uTweenSlider2.from = from;
		uTweenSlider2.to = to;
		uTweenSlider2.delay = delay;
		if (duration <= 0f)
		{
			uTweenSlider2.Sample(1f, _isFinished: true);
			((Behaviour)uTweenSlider2).enabled = false;
		}
		return uTweenSlider2;
	}
}
