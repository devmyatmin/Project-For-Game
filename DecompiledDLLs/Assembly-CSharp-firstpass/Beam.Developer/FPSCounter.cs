using UnityEngine;
using UnityEngine.UI;

namespace Beam.Developer;

public class FPSCounter : Singleton<FPSCounter>
{
	[SerializeField]
	private Text _text;

	[SerializeField]
	private float _updateInterval = 0.5f;

	private float _accum;

	private int _frames;

	private float _timeleft;

	private void Start()
	{
		_timeleft = _updateInterval;
		Enable(value: false);
	}

	public static void Enable(bool value)
	{
		if (Object.op_Implicit((Object)(object)Singleton<FPSCounter>.Instance))
		{
			Singleton<FPSCounter>.Instance.Enable_(value);
		}
	}

	protected void Enable_(bool value)
	{
		((Behaviour)_text).enabled = value;
		((Behaviour)this).enabled = value;
	}

	private void Update()
	{
		_timeleft -= Time.deltaTime;
		_accum += Time.timeScale / Time.deltaTime;
		_frames++;
		if ((double)_timeleft <= 0.0)
		{
			float num = _accum / (float)_frames;
			string empty = string.Empty;
			if (num > 30f)
			{
				empty = "<color=#42FF00>";
			}
			else
			{
				empty = ((!(num < 10f)) ? "<color=#CCAA00>" : "<color=#FF0000>");
				_timeleft = _updateInterval;
				_accum = 0f;
				_frames = 0;
			}
			string text = $"{empty}{num:F2} FPS</color>";
			_text.text = text;
		}
	}
}
