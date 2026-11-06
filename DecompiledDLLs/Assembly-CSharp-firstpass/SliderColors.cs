using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderColors : MonoBehaviour
{
	[SerializeField]
	private Color[] colors;

	[SerializeField]
	private Image _fillSprite;

	private Slider _slider;

	private void Start()
	{
		_slider = ((Component)this).GetComponent<Slider>();
		Update();
	}

	private void Update()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		float normalizedValue = _slider.normalizedValue;
		normalizedValue *= (float)(colors.Length - 1);
		int num = Mathf.FloorToInt(normalizedValue);
		Color color = colors[0];
		if (num >= 0)
		{
			if (num + 1 >= colors.Length)
			{
				color = ((num >= colors.Length) ? colors[colors.Length - 1] : colors[num]);
			}
			else
			{
				float num2 = normalizedValue - (float)num;
				color = Color.Lerp(colors[num], colors[num + 1], num2);
			}
		}
		((Graphic)_fillSprite).color = color;
	}
}
