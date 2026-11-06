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
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)_light).enabled = on;
		if (Object.op_Implicit((Object)(object)_renderer))
		{
			if (on)
			{
				((Renderer)_renderer).materials[1].EnableKeyword("_EMISSION");
				((Renderer)_renderer).materials[1].SetColor("_EmissionColor", _emissive);
			}
			else
			{
				((Renderer)_renderer).materials[1].DisableKeyword("_EMISSION");
				((Renderer)_renderer).materials[1].SetColor("_EmissionColor", Color.black);
			}
		}
	}
}
