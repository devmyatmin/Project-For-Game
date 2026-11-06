using UnityEngine;

namespace uTools;

[AddComponentMenu("uTools/Tween/Tween Position(uTools)")]
public class uTweenPosition : uTweener
{
	public Vector3 from;

	public Vector3 to;

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
			return ((Transform)cachedRectTransform).localPosition;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Transform)cachedRectTransform).localPosition = value;
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

	public static uTweenPosition Begin(GameObject go, Vector3 from, Vector3 to, float duration = 1f, float delay = 0f)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		uTweenPosition uTweenPosition2 = uTweener.Begin<uTweenPosition>(go, duration);
		uTweenPosition2.from = from;
		uTweenPosition2.to = to;
		uTweenPosition2.duration = duration;
		uTweenPosition2.delay = delay;
		if (duration <= 0f)
		{
			uTweenPosition2.Sample(1f, _isFinished: true);
			((Behaviour)uTweenPosition2).enabled = false;
		}
		return uTweenPosition2;
	}

	[ContextMenu("Set 'From' to current value")]
	public override void SetStartToCurrentValue()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		from = value;
	}

	[ContextMenu("Set 'To' to current value")]
	public override void SetEndToCurrentValue()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		to = value;
	}

	[ContextMenu("Assume value of 'From'")]
	public override void SetCurrentValueToStart()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		value = from;
	}

	[ContextMenu("Assume value of 'To'")]
	public override void SetCurrentValueToEnd()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		value = to;
	}
}
