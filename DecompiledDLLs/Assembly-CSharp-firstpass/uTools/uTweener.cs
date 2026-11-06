using UnityEngine;
using UnityEngine.Events;

namespace uTools;

public abstract class uTweener : MonoBehaviour
{
	public AnimationCurve animationCurve = new AnimationCurve(new Keyframe[2]
	{
		new Keyframe(0f, 0f, 0f, 1f),
		new Keyframe(1f, 1f, 1f, 0f)
	});

	public EaseType easeType;

	public LoopStyle loopStyle;

	public float delay;

	public float duration = 1f;

	public bool ignoreTimeScale = true;

	public UnityEvent onFinished;

	private float mAmountPerDelta = 1000f;

	private float mDuration;

	private float mStartTime = -1f;

	private float mFactor;

	public float factor
	{
		get
		{
			return mFactor;
		}
		set
		{
			mFactor = Mathf.Clamp01(value);
		}
	}

	public float amountPerDelta
	{
		get
		{
			if (mDuration != duration)
			{
				mDuration = duration;
				mAmountPerDelta = ((duration > 0f) ? (1f / duration) : 1000f);
			}
			return mAmountPerDelta;
		}
	}

	private void Start()
	{
		Update();
	}

	private void Update()
	{
		float num = (ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
		float num2 = (ignoreTimeScale ? Time.unscaledTime : Time.time);
		if (mStartTime < 0f)
		{
			mStartTime = num2 + delay;
		}
		if (num2 < mStartTime)
		{
			return;
		}
		mFactor += amountPerDelta * num;
		if (loopStyle == LoopStyle.Loop)
		{
			if (mFactor > 1f)
			{
				mFactor -= Mathf.Floor(mFactor);
			}
		}
		else if (loopStyle == LoopStyle.PingPong)
		{
			if (mFactor > 1f)
			{
				mFactor = 1f - (mFactor - Mathf.Floor(mFactor));
				mAmountPerDelta = 0f - mAmountPerDelta;
			}
			else if (mFactor < 0f)
			{
				mFactor = 0f - mFactor;
				mFactor -= Mathf.Floor(mFactor);
				mAmountPerDelta = 0f - mAmountPerDelta;
			}
		}
		if (loopStyle == LoopStyle.Once && (duration == 0f || mFactor > 1f || mFactor < 0f))
		{
			mFactor = Mathf.Clamp01(mFactor);
			Sample(mFactor, _isFinished: true);
			((Behaviour)this).enabled = false;
			if (onFinished != null)
			{
				onFinished.Invoke();
			}
		}
		else
		{
			Sample(mFactor, _isFinished: false);
		}
	}

	public void Sample(float _factor, bool _isFinished)
	{
		float num = Mathf.Clamp01(_factor);
		num = ((easeType == EaseType.none) ? animationCurve.Evaluate(num) : EaseManager.EasingFromType(0f, 1f, num, easeType));
		OnUpdate(num, _isFinished);
	}

	protected virtual void OnUpdate(float _factor, bool _isFinished)
	{
	}

	public void Reset()
	{
		((Behaviour)this).enabled = true;
		easeType = EaseType.linear;
		loopStyle = LoopStyle.Once;
		delay = 0f;
		duration = 1f;
		ignoreTimeScale = true;
		onFinished = null;
		mAmountPerDelta = 1000f;
		mDuration = 0f;
		mStartTime = -1f;
		mFactor = 0f;
	}

	public static T Begin<T>(GameObject _go, float _duration) where T : uTweener
	{
		T val = _go.GetComponent<T>();
		if ((Object)(object)val == (Object)null)
		{
			val = _go.AddComponent<T>();
		}
		val.Reset();
		val.duration = _duration;
		((Behaviour)val).enabled = true;
		return val;
	}

	public void Play(PlayDirection dir = PlayDirection.Forward)
	{
		mAmountPerDelta = ((dir == PlayDirection.Reverse) ? (0f - Mathf.Abs(amountPerDelta)) : Mathf.Abs(amountPerDelta));
		((Behaviour)this).enabled = true;
		Update();
	}

	public void Toggle()
	{
		mAmountPerDelta *= -1f;
		((Behaviour)this).enabled = true;
	}

	[ContextMenu("Set 'From' to current value")]
	public virtual void SetStartToCurrentValue()
	{
	}

	[ContextMenu("Set 'To' to current value")]
	public virtual void SetEndToCurrentValue()
	{
	}

	[ContextMenu("Assume value of 'From'")]
	public virtual void SetCurrentValueToStart()
	{
	}

	[ContextMenu("Assume value of 'To'")]
	public virtual void SetCurrentValueToEnd()
	{
	}

	protected uTweener()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected Obj, but got Unknown
	}
}
