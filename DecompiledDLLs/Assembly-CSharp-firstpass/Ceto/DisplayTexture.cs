using UnityEngine;

namespace Ceto;

[RequireComponent(typeof(Camera))]
public class DisplayTexture : MonoBehaviour
{
	public enum DISPLAY
	{
		NONE,
		OVERLAY_HEIGHT,
		OVERLAY_NORMAL,
		OVERLAY_FOAM,
		OVERLAY_CLIP,
		REFLECTION0,
		REFLECTION1,
		OCEAN_MASK0,
		OCEAN_MASK1,
		OCEAN_DEPTH0,
		OCEAN_DEPTH1,
		WAVE_SLOPEMAP0,
		WAVE_SLOPEMAP1,
		WAVE_DISPLACEMENTMAP0,
		WAVE_DISPLACEMENTMAP1,
		WAVE_DISPLACEMENTMAP2,
		WAVE_DISPLACEMENTMAP3,
		WAVE_FOAM0,
		WAVE_FOAM1
	}

	public bool enlarge;

	public DISPLAY display;

	private void Start()
	{
	}

	private void OnGUI()
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Ocean.Instance == (Object)null)
		{
			return;
		}
		Camera component = ((Component)this).GetComponent<Camera>();
		CameraData cameraData = Ocean.Instance.FindCameraData(component);
		if (cameraData == null)
		{
			return;
		}
		Texture val = FindTexture(cameraData, component);
		if (!((Object)(object)val == (Object)null))
		{
			int num;
			int num2;
			if ((val.width == Screen.width && val.height == Screen.height) || (val.width == Screen.width / 2 && val.height == Screen.height / 2))
			{
				num = Screen.width / (enlarge ? 2 : 3);
				num2 = Screen.height / (enlarge ? 2 : 3);
			}
			else
			{
				num = 256 * ((!enlarge) ? 1 : 2);
				num2 = 256 * ((!enlarge) ? 1 : 2);
			}
			GUI.DrawTexture(new Rect((float)(Screen.width - num - 5), 5f, (float)num, (float)num2), val, (ScaleMode)0, false);
		}
	}

	private Texture FindTexture(CameraData data, Camera cam)
	{
		if ((Object)(object)Ocean.Instance == (Object)null)
		{
			return null;
		}
		WaveSpectrum component = ((Component)Ocean.Instance).GetComponent<WaveSpectrum>();
		switch (display)
		{
		case DISPLAY.OVERLAY_HEIGHT:
			if (data.overlay != null)
			{
				return (Texture)(object)data.overlay.height;
			}
			return null;
		case DISPLAY.OVERLAY_NORMAL:
			if (data.overlay != null)
			{
				return (Texture)(object)data.overlay.normal;
			}
			return null;
		case DISPLAY.OVERLAY_FOAM:
			if (data.overlay != null)
			{
				return (Texture)(object)data.overlay.foam;
			}
			return null;
		case DISPLAY.OVERLAY_CLIP:
			if (data.overlay != null)
			{
				return (Texture)(object)data.overlay.clip;
			}
			return null;
		case DISPLAY.REFLECTION0:
			if (data.reflection != null)
			{
				return (Texture)(object)data.reflection.target0;
			}
			return null;
		case DISPLAY.REFLECTION1:
			if (data.reflection != null)
			{
				return (Texture)(object)data.reflection.target1;
			}
			return null;
		case DISPLAY.OCEAN_MASK0:
			if (data.mask != null)
			{
				return (Texture)(object)data.mask.target0;
			}
			return null;
		case DISPLAY.OCEAN_MASK1:
			if (data.mask != null)
			{
				return (Texture)(object)data.mask.target1;
			}
			return null;
		case DISPLAY.OCEAN_DEPTH0:
			if (data.depth != null)
			{
				return (Texture)(object)data.depth.target0;
			}
			return null;
		case DISPLAY.OCEAN_DEPTH1:
			if (data.depth != null)
			{
				return (Texture)(object)data.depth.target1;
			}
			return null;
		case DISPLAY.WAVE_SLOPEMAP0:
			if (!((Object)(object)component == (Object)null))
			{
				return (Texture)(object)component.SlopeMaps[0];
			}
			return null;
		case DISPLAY.WAVE_SLOPEMAP1:
			if (!((Object)(object)component == (Object)null))
			{
				return (Texture)(object)component.SlopeMaps[1];
			}
			return null;
		case DISPLAY.WAVE_DISPLACEMENTMAP0:
			if (!((Object)(object)component == (Object)null))
			{
				return (Texture)(object)component.DisplacementMaps[0];
			}
			return null;
		case DISPLAY.WAVE_DISPLACEMENTMAP1:
			if (!((Object)(object)component == (Object)null))
			{
				return (Texture)(object)component.DisplacementMaps[1];
			}
			return null;
		case DISPLAY.WAVE_DISPLACEMENTMAP2:
			if (!((Object)(object)component == (Object)null))
			{
				return (Texture)(object)component.DisplacementMaps[2];
			}
			return null;
		case DISPLAY.WAVE_DISPLACEMENTMAP3:
			if (!((Object)(object)component == (Object)null))
			{
				return (Texture)(object)component.DisplacementMaps[3];
			}
			return null;
		case DISPLAY.WAVE_FOAM0:
			if (!((Object)(object)component == (Object)null))
			{
				return (Texture)(object)component.FoamMaps[0];
			}
			return null;
		case DISPLAY.WAVE_FOAM1:
			if (!((Object)(object)component == (Object)null))
			{
				return (Texture)(object)component.FoamMaps[1];
			}
			return null;
		default:
			return null;
		}
	}
}
