using UnityEngine;

namespace Beam.Crafting;

public class Interactive_TORCH : InteractiveObject
{
	private bool _on;

	[SerializeField]
	private Light _light;

	[SerializeField]
	private MeshRenderer _renderer;

	[SerializeField]
	[ColorUsage(false, true)]
	private Color _emissive;

	public override void Hold(bool holding)
	{
		base.Hold(holding);
		if (!holding)
		{
			_on = false;
			UpdateFx(_on);
		}
	}

	public override void Use()
	{
		_on = !_on;
		UpdateFx(_on);
	}

	private void UpdateFx(bool on)
	{
		_light.enabled = on;
		if ((bool)_renderer)
		{
			if (on)
			{
				_renderer.materials[1].EnableKeyword("_EMISSION");
				_renderer.materials[1].SetColor("_EmissionColor", _emissive);
			}
			else
			{
				_renderer.materials[1].DisableKeyword("_EMISSION");
				_renderer.materials[1].SetColor("_EmissionColor", Color.black);
			}
		}
	}
}
