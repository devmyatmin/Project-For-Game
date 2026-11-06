using UnityEngine;

namespace Beam.UI;

public static class ExtensionMethods
{
	public static void SmoothFill(this IFilledImageViewAdapter image, float value, float smoothing)
	{
		image.Fill = Mathf.Lerp(image.Fill, value, Time.unscaledDeltaTime * smoothing);
	}
}
