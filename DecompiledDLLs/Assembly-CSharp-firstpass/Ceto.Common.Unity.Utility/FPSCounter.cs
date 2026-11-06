using UnityEngine;

namespace Ceto.Common.Unity.Utility;

public class FPSCounter : MonoBehaviour
{
	private float updateInterval = 0.5f;

	private float accum;

	private float frames;

	private float timeleft;

	public float FrameRate { get; set; }

	private void Start()
	{
		timeleft = updateInterval;
	}

	private void Update()
	{
		timeleft -= Time.deltaTime;
		accum += Time.timeScale / Time.deltaTime;
		frames++;
		if (timeleft <= 0f)
		{
			FrameRate = accum / frames;
			timeleft = updateInterval;
			accum = 0f;
			frames = 0f;
		}
	}
}
