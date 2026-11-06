using System.Collections.Generic;
using UnityEngine;

namespace uTools;

public class uTweenPath : uTweenValue
{
	public RectTransform target;

	public List<Vector3> paths;

	private int mIndex = -1;

	private int mPathsCount;

	private bool mCache;

	private int pathIndex
	{
		get
		{
			return mIndex;
		}
		set
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			if (mIndex != value)
			{
				mIndex = value;
				Debug.Log((object)((Transform)target).localPosition);
				uTweenPosition.Begin(((Component)target).gameObject, ((Transform)target).localPosition, paths[mIndex], duration / (float)paths.Count).loopStyle = LoopStyle.Loop;
			}
		}
	}

	private void Cache()
	{
		mCache = true;
		if (paths.Count > 1)
		{
			mPathsCount = paths.Count - 1;
		}
		if ((Object)(object)target == (Object)null)
		{
			target = ((Component)this).GetComponent<RectTransform>();
		}
		from = 0f;
		to = mPathsCount;
	}

	private void Update()
	{
	}

	protected override void ValueUpdate(float _factor, bool _isFinished)
	{
		if (!mCache)
		{
			Cache();
		}
		pathIndex = Mathf.FloorToInt(_factor);
		Debug.Log((object)pathIndex);
	}
}
