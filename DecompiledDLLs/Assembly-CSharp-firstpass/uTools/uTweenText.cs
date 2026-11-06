using System;
using UnityEngine;
using UnityEngine.UI;

namespace uTools;

[AddComponentMenu("uTools/Tween/Tween Value(uTools)")]
public class uTweenText : uTweenValue
{
	private Text mText;

	public int digits;

	public Text cacheText
	{
		get
		{
			mText = ((Component)this).GetComponent<Text>();
			if ((Object)(object)mText == (Object)null)
			{
				Debug.LogError((object)"'uTweenText' can't find 'Value'");
			}
			return mText;
		}
	}

	protected override void ValueUpdate(float value, bool isFinished)
	{
		cacheText.text = Math.Round(value, digits).ToString();
	}

	public static uTweenText Begin(Text label, float duration, float delay, float from, float to)
	{
		uTweenText uTweenText2 = uTweener.Begin<uTweenText>(((Component)label).gameObject, duration);
		uTweenText2.from = from;
		uTweenText2.to = to;
		uTweenText2.delay = delay;
		if (duration <= 0f)
		{
			uTweenText2.Sample(1f, _isFinished: true);
			((Behaviour)uTweenText2).enabled = false;
		}
		return uTweenText2;
	}
}
