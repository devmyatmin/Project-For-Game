namespace uTools;

public class uTweenValue : uTweener
{
	public float from;

	public float to;

	private float mValue;

	public float value
	{
		get
		{
			return mValue;
		}
		set
		{
			mValue = value;
		}
	}

	protected virtual void ValueUpdate(float value, bool isFinished)
	{
	}

	protected override void OnUpdate(float factor, bool isFinished)
	{
		value = from + factor * (to - from);
		ValueUpdate(value, isFinished);
	}
}
