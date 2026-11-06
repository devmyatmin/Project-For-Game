using UnityEngine;
using UnityEngine.UI;

namespace uTools;

[AddComponentMenu("uTools/Tween/Tween Color(uTools)")]
public class uTweenColor : uTweener
{
	public Color from = Color.white;

	public Color to = Color.white;

	public bool includeChilds;

	private Text mText;

	private Light mLight;

	private Material mMat;

	private Image mImage;

	private SpriteRenderer mSpriteRender;

	private Color mColor = Color.white;

	public Color colorValue
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return mColor;
		}
		set
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			SetColor(((Component)this).transform, value);
			mColor = value;
		}
	}

	protected override void OnUpdate(float factor, bool isFinished)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		colorValue = Color.Lerp(from, to, factor);
	}

	public static uTweenColor Begin(GameObject go, float duration, float delay, Color from, Color to)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		uTweenColor uTweenColor2 = uTweener.Begin<uTweenColor>(go, duration);
		uTweenColor2.from = from;
		uTweenColor2.to = to;
		uTweenColor2.delay = delay;
		if (duration <= 0f)
		{
			uTweenColor2.Sample(1f, _isFinished: true);
			((Behaviour)uTweenColor2).enabled = false;
		}
		return uTweenColor2;
	}

	private void SetColor(Transform _transform, Color _color)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		mText = ((Component)_transform).GetComponent<Text>();
		if ((Object)(object)mText != (Object)null)
		{
			((Graphic)mText).color = _color;
		}
		mLight = ((Component)_transform).GetComponent<Light>();
		if ((Object)(object)mLight != (Object)null)
		{
			mLight.color = _color;
		}
		mImage = ((Component)_transform).GetComponent<Image>();
		if ((Object)(object)mImage != (Object)null)
		{
			((Graphic)mImage).color = _color;
		}
		mSpriteRender = ((Component)_transform).GetComponent<SpriteRenderer>();
		if ((Object)(object)mSpriteRender != (Object)null)
		{
			mSpriteRender.color = _color;
		}
		if ((Object)(object)((Component)_transform).GetComponent<Renderer>() != (Object)null)
		{
			mMat = ((Component)_transform).GetComponent<Renderer>().material;
			if ((Object)(object)mMat != (Object)null)
			{
				mMat.color = _color;
			}
		}
		if (includeChilds)
		{
			for (int i = 0; i < _transform.childCount; i++)
			{
				Transform child = _transform.GetChild(i);
				SetColor(child, _color);
			}
		}
	}

	public uTweenColor()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
	}
}
