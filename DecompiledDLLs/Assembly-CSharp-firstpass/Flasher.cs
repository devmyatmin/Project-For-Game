using System;
using Beam.UI;
using UnityEngine;

public class Flasher : MonoBehaviour
{
	[SerializeField]
	private WatchViewAdapterBase _watchView;

	[SerializeField]
	private USliderViewAdapter _slider;

	[SerializeField]
	private UImageViewAdapter _image;

	[SerializeField]
	private float _threshold;

	private bool _flashing;

	private void Awake()
	{
		_watchView.ShowView += Watch_Show;
		_watchView.HideView += Watch_Hide;
		_slider.ValueChanged += Slider_ValueChanged;
	}

	private void OnDestroy()
	{
		if ((Object)(object)_watchView != (Object)null)
		{
			_watchView.ShowView -= Watch_Show;
			_watchView.HideView -= Watch_Hide;
		}
		if ((Object)(object)_slider != (Object)null)
		{
			_slider.ValueChanged -= Slider_ValueChanged;
		}
	}

	private void Watch_Show()
	{
		Slider_ValueChanged(_slider.NormalizedValue);
	}

	private void Watch_Hide()
	{
		Stop();
	}

	private void Slider_ValueChanged(float value)
	{
		if (value <= _threshold && !_flashing)
		{
			LeanTween.value(((Component)this).gameObject, (Action<float>)((float val) =>
			{
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				_image.Color = new Color(val, val, val, 1f);
			}), 1f, 0.5f, 0.7f).setLoopPingPong();
			_flashing = true;
		}
		else if (value > _threshold && _flashing)
		{
			Stop();
		}
	}

	public void Stop()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		LeanTween.cancel(((Component)this).gameObject);
		_image.Color = Color.white;
		_flashing = false;
	}
}
