using UnityEngine;

namespace uTools;

[AddComponentMenu("uTools/Tween/Tween Rotation(uTools)")]
public class uTweenRotation : uTweener
{
	public Vector3 from;

	public Vector3 to;

	private RectTransform mRectTransfrom;

	public RectTransform target;

	public RectTransform cacheRectTransfrom
	{
		get
		{
			if ((Object)(object)target == (Object)null)
			{
				mRectTransfrom = ((Component)this).GetComponent<RectTransform>();
			}
			else
			{
				mRectTransfrom = target;
			}
			return mRectTransfrom;
		}
	}

	public Quaternion value
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Transform)cacheRectTransfrom).localRotation;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Transform)cacheRectTransfrom).localRotation = value;
		}
	}

	protected override void OnUpdate(float _factor, bool _isFinished)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		value = Quaternion.Euler(Vector3.Lerp(from, to, _factor));
	}

	public static uTweenRotation Begin(GameObject go, Vector3 from, Vector3 to, float duration = 1f, float delay = 0f)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		uTweenRotation uTweenRotation2 = uTweener.Begin<uTweenRotation>(go, duration);
		uTweenRotation2.from = from;
		uTweenRotation2.to = to;
		uTweenRotation2.duration = duration;
		uTweenRotation2.delay = delay;
		if (duration <= 0f)
		{
			uTweenRotation2.Sample(1f, _isFinished: true);
			((Behaviour)uTweenRotation2).enabled = false;
		}
		return uTweenRotation2;
	}
}
