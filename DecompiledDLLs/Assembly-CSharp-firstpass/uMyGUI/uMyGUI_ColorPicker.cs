using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace uMyGUI;

public class uMyGUI_ColorPicker : MonoBehaviour
{
	public class ColorEventArgs : EventArgs
	{
		public readonly Color Value;

		public ColorEventArgs(Color p_value)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			Value = p_value;
		}
	}

	[SerializeField]
	private Slider m_redSlider;

	[SerializeField]
	private Slider m_greenSlider;

	[SerializeField]
	private Slider m_blueSlider;

	[SerializeField]
	private Color m_pickedColor = Color.gray;

	[SerializeField]
	private Graphic m_colorPreview;

	public EventHandler<ColorEventArgs> m_onChanged;

	public Slider RedSlider
	{
		get
		{
			return m_redSlider;
		}
		set
		{
			m_redSlider = value;
		}
	}

	public Slider GreenSlider
	{
		get
		{
			return m_greenSlider;
		}
		set
		{
			m_greenSlider = value;
		}
	}

	public Slider BlueSlider
	{
		get
		{
			return m_blueSlider;
		}
		set
		{
			m_blueSlider = value;
		}
	}

	public Color PickedColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_pickedColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			if (m_pickedColor != value)
			{
				m_pickedColor = value;
				UpdateColor();
				if (m_onChanged != null)
				{
					m_onChanged(this, new ColorEventArgs(m_pickedColor));
				}
			}
		}
	}

	public Graphic ColorPreview
	{
		get
		{
			return m_colorPreview;
		}
		set
		{
			m_colorPreview = value;
		}
	}

	private void Start()
	{
		if ((Object)(object)m_redSlider == (Object)null || (Object)(object)m_greenSlider == (Object)null || (Object)(object)m_blueSlider == (Object)null)
		{
			Debug.LogError((object)"uMyGUI_ColorPicker: all three sliders (RGB) must be set in inspector!");
			((Behaviour)this).enabled = false;
			return;
		}
		UpdateColor();
		((UnityEvent<float>)(object)m_redSlider.onValueChanged).AddListener((UnityAction<float>)SetRedValue);
		((UnityEvent<float>)(object)m_greenSlider.onValueChanged).AddListener((UnityAction<float>)SetGreenValue);
		((UnityEvent<float>)(object)m_blueSlider.onValueChanged).AddListener((UnityAction<float>)SetBlueValue);
	}

	private void OnDestroy()
	{
		m_onChanged = null;
		((UnityEvent<float>)(object)m_redSlider.onValueChanged).RemoveListener((UnityAction<float>)SetRedValue);
		((UnityEvent<float>)(object)m_greenSlider.onValueChanged).RemoveListener((UnityAction<float>)SetGreenValue);
		((UnityEvent<float>)(object)m_blueSlider.onValueChanged).RemoveListener((UnityAction<float>)SetBlueValue);
	}

	private void SetRedValue(float p_redValue)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (m_pickedColor.r != p_redValue)
		{
			m_pickedColor.r = p_redValue;
			UpdateColor();
			if (m_onChanged != null)
			{
				m_onChanged(this, new ColorEventArgs(m_pickedColor));
			}
		}
	}

	private void SetGreenValue(float p_greenValue)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (m_pickedColor.g != p_greenValue)
		{
			m_pickedColor.g = p_greenValue;
			UpdateColor();
			if (m_onChanged != null)
			{
				m_onChanged(this, new ColorEventArgs(m_pickedColor));
			}
		}
	}

	private void SetBlueValue(float p_blueValue)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (m_pickedColor.b != p_blueValue)
		{
			m_pickedColor.b = p_blueValue;
			UpdateColor();
			if (m_onChanged != null)
			{
				m_onChanged(this, new ColorEventArgs(m_pickedColor));
			}
		}
	}

	private void UpdateColor()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		m_redSlider.value = m_pickedColor.r;
		m_greenSlider.value = m_pickedColor.g;
		m_blueSlider.value = m_pickedColor.b;
		if ((Object)(object)m_colorPreview != (Object)null)
		{
			m_colorPreview.color = m_pickedColor;
		}
	}

	public uMyGUI_ColorPicker()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
