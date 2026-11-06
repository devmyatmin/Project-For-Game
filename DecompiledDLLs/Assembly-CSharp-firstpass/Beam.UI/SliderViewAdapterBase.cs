using UnityEngine;

namespace Beam.UI;

public abstract class SliderViewAdapterBase<TSlider, TLabel> : ViewAdapterBase, ISliderViewAdapter, IViewAdapter where TSlider : MonoBehaviour
{
	private TSlider _slider;

	protected TSlider Slider
	{
		get
		{
			if ((Object)(object)_slider == (Object)null)
			{
				_slider = ((Component)this).GetComponent<TSlider>();
			}
			return _slider;
		}
	}

	protected abstract TLabel Label { get; }

	protected abstract TLabel ValueLabel { get; }

	public abstract string Text { get; set; }

	public abstract string ValueText { get; set; }

	public abstract float Value { get; set; }

	public abstract float NormalizedValue { get; set; }

	public event FloatEventHandler ValueChanged;

	protected virtual void OnValueChanged(float value)
	{
		ValueChanged?.Invoke(value);
	}

	protected virtual void OnDestroy()
	{
		ValueChanged = null;
	}
}
