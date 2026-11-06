using UnityEngine;

namespace uTools;

[AddComponentMenu("uTools/Tween/Tween Scale(uTools)")]
public class uTweenScale : uTweener
{
	public Vector3 from = Vector3.zero;

	public Vector3 to = Vector3.one;

	private RectTransform mRectTransform;

	public RectTransform cachedRectTransform
	{
		get
		{
			if ((Object)(object)mRectTransform == (Object)null)
			{
				mRectTransform = ((Component)this).GetComponent<RectTransform>();
			}
			return mRectTransform;
		}
	}

	public Vector3 value
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Transform)cachedRectTransform).localScale;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Transform)cachedRectTransform).localScale = value;
		}
	}

	protected override void OnUpdate(float factor, bool isFinished)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		value = from + factor * (to - from);
	}

	public static uTweenScale Begin(GameObject go, Vector3 from, Vector3 to, float duration = 1f, float delay = 0f)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		uTweenScale uTweenScale2 = uTweener.Begin<uTweenScale>(go, duration);
		uTweenScale2.from = from;
		uTweenScale2.to = to;
		uTweenScale2.duration = duration;
		uTweenScale2.delay = delay;
		if (duration <= 0f)
		{
			uTweenScale2.Sample(1f, _isFinished: true);
			((Behaviour)uTweenScale2).enabled = false;
		}
		return uTweenScale2;
	}

	public uTweenScale()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
	}
}
