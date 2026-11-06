using UnityEngine;

namespace Beam.UI;

public class WatchScreenViewAdapter : WatchScreenViewAdapterBase
{
	[SerializeField]
	private Renderer _watchScreenRenderer;

	private Renderer _watchRenderer;

	[SerializeField]
	private Renderer _watchRendererMale;

	[SerializeField]
	private Renderer _watchRendererFemale;

	[SerializeField]
	private Transform _watchMaleScreenPos;

	[SerializeField]
	private Transform _watchFemaleScreenPos;

	[SerializeField]
	private Light _light;

	public override void SetWatchMesh(int type)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (type == 0)
		{
			_watchRenderer = _watchRendererMale;
			_watchRendererMale.enabled = true;
			_watchRendererFemale.enabled = false;
			((Component)_watchScreenRenderer).transform.position = _watchMaleScreenPos.position;
		}
		else
		{
			_watchRenderer = _watchRendererFemale;
			_watchRendererFemale.enabled = true;
			_watchRendererMale.enabled = false;
			((Component)_watchScreenRenderer).transform.position = _watchFemaleScreenPos.position;
		}
	}

	public override void SetWatchTexture(Texture texture)
	{
		_watchScreenRenderer.material.mainTexture = texture;
	}

	public override void SetBacklight(bool enabled)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_watchRenderer == (Object)null))
		{
			_light.intensity = (enabled ? 1f : 0.3f);
			for (int i = 0; i < _watchRenderer.sharedMaterials.Length; i++)
			{
				_watchRenderer.sharedMaterials[i].EnableKeyword("_EMISSION");
				_watchRenderer.sharedMaterials[i].SetColor("_EmissionColor", enabled ? Color.Lerp(Color.black, Color.white, 0.5f) : Color.Lerp(Color.black, Color.white, 0.2f));
			}
		}
	}
}
