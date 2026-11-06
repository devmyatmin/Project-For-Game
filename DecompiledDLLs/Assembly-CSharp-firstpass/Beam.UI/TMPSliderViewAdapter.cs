using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Beam.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Slider))]
public class TMPSliderViewAdapter : SliderViewAdapterBase<Slider, TMPLabelViewAdapter>
{
	[SerializeField]
	private TMPLabelViewAdapter _label;

	[SerializeField]
	private TMPLabelViewAdapter _valueLabel;

	protected override TMPLabelViewAdapter Label => _label;

	protected override TMPLabelViewAdapter ValueLabel => _valueLabel;

	public override string Text
	{
		get
		{
			return _label.Text;
		}
		set
		{
			_label.Text = value;
		}
	}

	public override string ValueText
	{
		get
		{
			return _valueLabel.Text;
		}
		set
		{
			_valueLabel.Text = value;
		}
	}

	public override float Value
	{
		get
		{
			return Slider.value;
		}
		set
		{
			Slider.value = value;
		}
	}

	public override float NormalizedValue
	{
		get
		{
			return Slider.normalizedValue;
		}
		set
		{
			Slider.normalizedValue = value;
		}
	}

	public override bool Enabled
	{
		get
		{
			return ((Component)Slider).gameObject.activeSelf;
		}
		set
		{
			((Component)Slider).gameObject.SetActive(value);
		}
	}

	private void Awake()
	{
		((UnityEventBase)Slider.onValueChanged).RemoveAllListeners();
		((UnityEvent<float>)(object)Slider.onValueChanged).AddListener((UnityAction<float>)OnValueChanged);
	}

	protected override void OnDestroy()
	{
		((UnityEventBase)Slider.onValueChanged).RemoveAllListeners();
		Slider.onValueChanged = null;
		base.OnDestroy();
	}
}
