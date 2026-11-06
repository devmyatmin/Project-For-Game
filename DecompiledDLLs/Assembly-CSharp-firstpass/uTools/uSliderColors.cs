using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace uTools;

[AddComponentMenu("uTools/Tween/Slider Colors(uTools)")]
public class uSliderColors : MonoBehaviour
{
	public Image target;

	public Color[] colors = new Color[3]
	{
		Color.red,
		Color.yellow,
		Color.green
	};

	private Slider mSlider;

	private void Start()
	{
		mSlider = ((Component)this).GetComponent<Slider>();
		if ((Object)(object)mSlider == (Object)null)
		{
			Debug.LogError((object)" 'uSliderColors' can't find 'Slider'.");
			return;
		}
		if ((Object)(object)target == (Object)null)
		{
			target = ((Component)mSlider).GetComponentInChildren<Image>();
		}
		UnityAction<float> val = OnValueChanged;
		((UnityEvent<float>)(object)mSlider.onValueChanged).AddListener(val);
		OnValueChanged(mSlider.value);
	}

	public void OnValueChanged(float value)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		float num = value * (float)(colors.Length - 1);
		int num2 = Mathf.FloorToInt(num);
		Color color = colors[0];
		if (num2 + 1 < colors.Length)
		{
			color = Color.Lerp(colors[num2], colors[num2 + 1], num - (float)num2);
		}
		else if (num2 < colors.Length)
		{
			color = colors[num2];
		}
		((Graphic)target).color = color;
	}

	public uSliderColors()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
	}
}
