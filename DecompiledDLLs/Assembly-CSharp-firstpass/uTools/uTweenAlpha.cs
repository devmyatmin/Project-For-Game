using UnityEngine;
using UnityEngine.UI;

namespace uTools;

[AddComponentMenu("uTools/Tween/Tween Alpha(uTools)")]
public class uTweenAlpha : uTweenValue
{
	public bool includeChilds;

	private Text mText;

	private Light mLight;

	private Material mMat;

	private Image mImage;

	private SpriteRenderer mSpriteRender;

	private float mAlpha;

	public float alpha
	{
		get
		{
			return mAlpha;
		}
		set
		{
			SetAlpha(((Component)this).transform, value);
			mAlpha = value;
		}
	}

	protected override void ValueUpdate(float value, bool isFinished)
	{
		alpha = value;
	}

	private void SetAlpha(Transform _transform, float _alpha)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		Color white = Color.white;
		mText = ((Component)_transform).GetComponent<Text>();
		if ((Object)(object)mText != (Object)null)
		{
			white = ((Graphic)mText).color;
			white.a = _alpha;
			((Graphic)mText).color = white;
		}
		mLight = ((Component)_transform).GetComponent<Light>();
		if ((Object)(object)mLight != (Object)null)
		{
			white = mLight.color;
			white.a = _alpha;
			mLight.color = white;
		}
		mImage = ((Component)_transform).GetComponent<Image>();
		if ((Object)(object)mImage != (Object)null)
		{
			white = ((Graphic)mImage).color;
			white.a = _alpha;
			((Graphic)mImage).color = white;
		}
		mSpriteRender = ((Component)_transform).GetComponent<SpriteRenderer>();
		if ((Object)(object)mSpriteRender != (Object)null)
		{
			white = mSpriteRender.color;
			white.a = _alpha;
			mSpriteRender.color = white;
		}
		if ((Object)(object)((Component)_transform).GetComponent<Renderer>() != (Object)null)
		{
			mMat = ((Component)_transform).GetComponent<Renderer>().material;
			if ((Object)(object)mMat != (Object)null)
			{
				white = mMat.color;
				white.a = _alpha;
				mMat.color = white;
			}
		}
		if (includeChilds)
		{
			for (int i = 0; i < _transform.childCount; i++)
			{
				Transform child = _transform.GetChild(i);
				SetAlpha(child, _alpha);
			}
		}
	}
}
